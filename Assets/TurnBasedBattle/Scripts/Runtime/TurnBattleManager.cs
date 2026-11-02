using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TurnBasedBattle
{
    [DefaultExecutionOrder(-80)]
    public sealed class TurnBattleManager : MonoBehaviour
    {
        public BattleTuning tuning;
        public BattleActor player;
        public BattleActor[] enemies;
        public Transform enemyFormation;
        public Camera battleCamera;
        public BattleHUD hud;
        public BattleEffects effects;
        public EarthPrisonController earthPrison;
        [Range(1,3)] public int initialEnemies = 1;
        public bool manualSimulation;
        public bool verboseLog;

        public BattlePhase Phase { get; private set; }
        public BattleActionDefinition Current { get; private set; }
        public BattleActor Source { get; private set; }
        public BattleActor SelectedTarget => selectedEnemy>=0 && selectedEnemy<enemies.Length ? enemies[selectedEnemy] : null;
        public BattleActionKind SelectedAction { get; private set; }
        public float ActionTime { get; private set; }
        public BattleActor ActionTarget => targets.Count>0?targets[0]:null;
        public float Ultimate { get; private set; }
        public int PlayerTurn { get; private set; }
        public int SkillReadyTurn { get; private set; }
        public int SkillWait => Mathf.Max(0,SkillReadyTurn-PlayerTurn);
        public bool Guarding { get; private set; }
        public bool Paused { get; private set; }
        public float PlaybackSpeed { get; private set; } = 1;
        public string Message { get; private set; } = "";
        public int BattleId { get; private set; }
        public int ActionId { get; private set; }
        public int EnemyActionsStarted { get; private set; }
        public int CompletedActions { get; private set; }
        public bool InAction => Phase==BattlePhase.PlayerAction || Phase==BattlePhase.EnemyAction;
        public bool CommandsAvailable => !Paused && (Phase==BattlePhase.PlayerCommand || Phase==BattlePhase.PlayerTargeting);
        public bool EasyDodge { get; private set; }
        public IReadOnlyList<BattleHitRecord> HitRecords => records;
        public IReadOnlyList<string> ActionHistory => history;

        readonly List<BattleActor> targets = new List<BattleActor>(4);
        readonly List<BattleHitRecord> records = new List<BattleHitRecord>(128);
        readonly List<string> history = new List<string>(128);
        readonly List<DodgeInput> queuedDodge = new List<DodgeInput>(8);
        struct DodgeInput { public double time; public int action; }
        DodgeAttempt[] attempts = Array.Empty<DodgeAttempt>();
        bool[] hitSent = Array.Empty<bool>();
        int selectedEnemy, enemyCursor;
        bool actionDidDamage, enemyDidDamage, enemyDidDodge, released;
        double epoch, lastRealTime;
        float messageRemaining;
        InputAction dodgeInput;
        bool started;

        void OnEnable()
        {
            if(dodgeInput==null)
            {
                dodgeInput=new InputAction("Timed Dodge",InputActionType.Button,"<Keyboard>/space");
                dodgeInput.performed+=OnDodgeInput;
            }
            dodgeInput.Enable();
        }
        void OnDisable() { dodgeInput?.Disable(); queuedDodge.Clear(); if(effects!=null)effects.StopAll(); if(earthPrison!=null)earthPrison.StopEffect(); }
        void OnDestroy() { if(dodgeInput!=null){dodgeInput.performed-=OnDodgeInput;dodgeInput.Dispose();} }
        void Start() { started=true; Restart(initialEnemies); }
        void OnDodgeInput(InputAction.CallbackContext context)
        {
            if(Paused || Phase!=BattlePhase.EnemyAction || manualSimulation) return;
            queuedDodge.Add(new DodgeInput{time=context.time,action=ActionId});
        }

        void Update()
        {
            if(!started || tuning==null) return;
            double now=Time.realtimeSinceStartupAsDouble;
            float wallDelta=(float)Math.Max(0,now-lastRealTime); lastRealTime=now;
            ReadControls();
            if(Paused || manualSimulation) { hud?.Refresh(); return; }
            if(InAction)
            {
                // Input timestamps and the action epoch share the realtimeSinceStartup time base.
                queuedDodge.Sort((a,b)=>a.time.CompareTo(b.time));
                for(int i=0;i<queuedDodge.Count;i++) if(queuedDodge[i].action==ActionId)
                    SubmitDodge((queuedDodge[i].time-epoch)*PlaybackSpeed);
                queuedDodge.Clear();
                AdvanceAction((float)Math.Max(ActionTime,(now-epoch)*PlaybackSpeed));
            }
            else TickPresentation(Mathf.Min(wallDelta,.2f)*PlaybackSpeed);
            hud?.Refresh();
        }

        void ReadControls()
        {
            var k=Keyboard.current; if(k==null || manualSimulation) return;
            if(k.pKey.wasPressedThisFrame) TogglePause();
            if(k.f1Key.wasPressedThisFrame) hud?.ToggleDebug();
            if(k.f2Key.wasPressedThisFrame) FillUltimate();
            if(k.f5Key.wasPressedThisFrame) Restart(1);
            if(k.f6Key.wasPressedThisFrame) Restart(3);
            if(Paused)return;
            if(CommandsAvailable)
            {
                if(k.digit1Key.wasPressedThisFrame)Select(BattleActionKind.Attack);
                if(k.digit2Key.wasPressedThisFrame)Select(BattleActionKind.Skill);
                if(k.digit3Key.wasPressedThisFrame)Select(BattleActionKind.Guard);
                if(k.rKey.wasPressedThisFrame)Select(BattleActionKind.EarthPrison);
                if(k.enterKey.wasPressedThisFrame)Confirm();
                if(k.escapeKey.wasPressedThisFrame)CancelSelection();
                if(k.tabKey.wasPressedThisFrame)CycleTarget();
            }
        }

        public void Restart(int count)
        {
            if(tuning==null || player==null)return;
            BattleId++; ActionId++; records.Clear();history.Clear();queuedDodge.Clear();targets.Clear();
            Current=null;Source=null;ActionTime=0;attempts=Array.Empty<DodgeAttempt>();hitSent=Array.Empty<bool>();
            Paused=false;Guarding=false;Ultimate=tuning.startingUltimate;PlayerTurn=1;SkillReadyTurn=1;
            EnemyActionsStarted=CompletedActions=0;selectedEnemy=0;enemyCursor=0;
            effects?.StopAll();effects?.SetPaused(false);earthPrison?.StopEffect();
            initialEnemies=Mathf.Clamp(count,1,enemies.Length);
            player.gameObject.SetActive(true);player.Initialize(tuning.playerHealth);
            for(int i=0;i<enemies.Length;i++) {enemies[i].gameObject.SetActive(i<initialEnemies);enemies[i].Initialize(tuning.enemyHealth);}
            Phase=BattlePhase.PlayerCommand; SelectedAction=BattleActionKind.Attack;
            lastRealTime=Time.realtimeSinceStartupAsDouble;epoch=lastRealTime;
            Say("Chọn một hành động. SPACE để né khi quái ra đòn.",5);
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            hud?.ClearFloating();hud?.Refresh(true);
        }

        public bool CanSelect(BattleActionKind kind)
        {
            if(!CommandsAvailable || player.Dead || LivingEnemies()==0)return false;
            if(kind==BattleActionKind.EarthPrison && Ultimate<100)return false;
            if(kind==BattleActionKind.Skill && PlayerTurn<SkillReadyTurn)return false;
            return kind==BattleActionKind.Attack || kind==BattleActionKind.Skill || kind==BattleActionKind.Guard || kind==BattleActionKind.EarthPrison;
        }
        public void Select(BattleActionKind kind)
        {
            if(!CanSelect(kind)) { if(CommandsAvailable)Say(kind==BattleActionKind.EarthPrison?"Ultimate chưa đầy.":"Kỹ năng đang hồi lượt.",2);return; }
            SelectedAction=kind;EnsureTarget();Phase=BattlePhase.PlayerTargeting;
            Say(kind==BattleActionKind.EarthPrison?"EARTH PRISON · Toàn bộ quái. Xác nhận để thi triển.":kind==BattleActionKind.Guard?"Giảm 50% sát thương trong pha quái kế tiếp.":"Chọn mục tiêu bên phải rồi XÁC NHẬN.",4);
            hud?.Refresh(true);
        }
        public void SelectTarget(int index)
        {
            if(!CommandsAvailable || index<0 || index>=enemies.Length || !IsAlive(enemies[index]))return;
            selectedEnemy=index;hud?.Refresh(true);
        }
        void CycleTarget()
        {
            for(int i=1;i<=enemies.Length;i++){int n=(selectedEnemy+i)%enemies.Length;if(IsAlive(enemies[n])){SelectTarget(n);return;}}
        }
        public void CancelSelection() {if(!CommandsAvailable)return;Phase=BattlePhase.PlayerCommand;Say("Chọn hành động.",2);hud?.Refresh(true);}

        public bool Confirm()
        {
            if(Phase!=BattlePhase.PlayerTargeting || !CanSelect(SelectedAction))return false;
            var definition=tuning.Get(SelectedAction);if(definition==null)return false;
            EnsureTarget();if(SelectedTarget==null || !IsAlive(SelectedTarget))return false;
            if(definition.ultimateCost>0)Ultimate=Mathf.Max(0,Ultimate-definition.ultimateCost);
            if(definition.cooldownTurns>0)SkillReadyTurn=PlayerTurn+definition.cooldownTurns+1;
            targets.Clear();
            if(definition.allEnemies){foreach(var e in enemies)if(IsAlive(e))targets.Add(e);}
            else if(definition.kind!=BattleActionKind.Guard)targets.Add(SelectedTarget);
            if(definition.kind==BattleActionKind.Guard)Guarding=true;
            BeginAction(player,definition,BattlePhase.PlayerAction);
            return true;
        }

        void BeginAction(BattleActor source,BattleActionDefinition definition,BattlePhase phase)
        {
            ActionId++;Current=definition;Source=source;Phase=phase;ActionTime=0;
            epoch=Time.realtimeSinceStartupAsDouble;queuedDodge.Clear();
            hitSent=new bool[definition.hits.Length];attempts=new DodgeAttempt[definition.hits.Length];
            for(int i=0;i<attempts.Length;i++)attempts[i]=new DodgeAttempt();
            actionDidDamage=enemyDidDamage=enemyDidDodge=released=false;
            source.Play(definition.clipName);
            source.BeginActionMotion(definition,targets.Count>0?targets[0]:null,definition.kind==BattleActionKind.EarthPrison?(Vector3?)enemyFormation.position:null);
            if(history.Count>200)history.RemoveAt(0);
            history.Add($"{BattleId}:{ActionId} {source.actorName} → {definition.kind}");
            if(verboseLog)Debug.Log(history[history.Count-1]);
            effects?.BeginAction(definition,source,targets.Count>0?targets[0]:source);
            Say(definition.displayName,phase==BattlePhase.EnemyAction?.35f:1.8f);
            hud?.Refresh(true);
        }

        public bool SubmitDodge(double timestamp)
        {
            if(Paused || Phase!=BattlePhase.EnemyAction || Current==null || player.Dead)return false;
            double extra=EasyDodge?.09:tuning.extraDodgeWindow;
            for(int i=0;i<attempts.Length;i++)
            {
                if(!attempts[i].Submit(timestamp,Current.hits[i],extra))continue;
                if(attempts[i].outcome==DodgeOutcome.Dodged)
                {
                    player.Dodge(Mathf.Max(.05f,Current.hits[i].time-ActionTime));
                    Say("ĐÚNG NHỊP!",.65f);
                }
                else Say(timestamp<Current.hits[i].open-extra?"QUÁ SỚM":"QUÁ MUỘN",.8f);
                if(verboseLog)Debug.Log($"Dodge A{ActionId}/H{i}: input={timestamp:F4}, window={Current.hits[i].open-extra:F3}..{Current.hits[i].close+extra:F3}, {attempts[i].outcome}");
                return true;
            }
            return false;
        }
        public void DodgeNow()
        {
            if(Current==null || Paused || manualSimulation)return;
            SubmitDodge((Time.realtimeSinceStartupAsDouble-epoch)*PlaybackSpeed);
        }

        public DodgeAttempt GetAttempt(int index) => index>=0 && index<attempts.Length?attempts[index]:null;
        public int CueIndex
        {
            get
            {
                if(Phase!=BattlePhase.EnemyAction || Current==null)return -1;
                for(int i=0;i<Current.hits.Length;i++)if(ActionTime>=Current.hits[i].cue && ActionTime<=Current.hits[i].time+.18f)return i;
                return -1;
            }
        }

        public void AdvanceForTest(float dt)
        {
            if(Paused)return;
            if(InAction)AdvanceAction(ActionTime+Mathf.Max(0,dt));else TickPresentation(Mathf.Max(0,dt));
            epoch=Time.realtimeSinceStartupAsDouble-ActionTime/PlaybackSpeed;
            hud?.Refresh();
        }

        void AdvanceAction(float targetTime)
        {
            if(!InAction || Current==null)return;
            if(player.Dead){FinishBattle(false);return;}
            if(Source==null || Source.Dead){CompleteAction();return;}
            float previous=ActionTime;ActionTime=Mathf.Min(targetTime,Current.duration);
            float dt=Mathf.Max(0,ActionTime-previous);
            Source.EvaluateActionMotion(ActionTime);
            TickPresentation(dt);
            if(!released && ActionTime>=Current.releaseTime) {released=true;effects?.Release(Current.kind);}
            for(int h=0;h<Current.hits.Length;h++)
            {
                if(hitSent[h] || ActionTime+1e-6f<Current.HitTime(h))continue;
                hitSent[h]=true;var hit=Current.hits[h];
                bool enemy=Phase==BattlePhase.EnemyAction;
                bool dodge=enemy && attempts[h].Resolve()==DodgeOutcome.Dodged;
                for(int i=0;i<targets.Count;i++)
                {
                    var target=targets[i];if(!IsAlive(target))continue;
                    float amount=dodge?0:hit.damage*(enemy && Guarding?tuning.guardMultiplier:1);
                    float dealt=target.Damage(amount);
                    records.Add(new BattleHitRecord{battleId=BattleId,actionId=ActionId,hitIndex=h,targetId=target.actorId,actionTime=Current.HitTime(h),damage=dealt,hpAfter=target.HP,dodged=dodge});
                    if(records.Count>512)records.RemoveAt(0);
                    if(enemy){enemyDidDodge|=dodge;enemyDidDamage|=dealt>0;}
                    else actionDidDamage|=dealt>0;
                    effects?.Hit(target,dodge,Current.kind==BattleActionKind.EarthPrison?1.3f:.75f);
                    hud?.ShowResult(target,dodge?"NÉ!":Mathf.RoundToInt(dealt).ToString(),dodge);
                }
                if(player.Dead){FinishBattle(false);return;}
            }
            effects?.Present(Current,Source,targets.Count>0?targets[0]:Source,ActionTime,attempts);
            if(Current.kind==BattleActionKind.EarthPrison && earthPrison!=null && ActionTime>=Current.vfxStart)
            {
                float fxTime=ActionTime-Current.vfxStart;
                if(fxTime<8)
                {
                    if(!earthPrison.Active && Source.CastPositionReady)earthPrison.PlayEffect(Source.focusSocket?Source.focusSocket:Source.castSocket,enemyFormation.position,ActionId);
                    earthPrison.Evaluate(fxTime,Mathf.Min(dt,fxTime));
                }
                else if(earthPrison.Active)earthPrison.StopEffect();
            }
            if(ActionTime>=Current.duration-1e-6f)CompleteAction();
        }

        void TickPresentation(float dt)
        {
            player.Tick(dt);foreach(var e in enemies)if(e.gameObject.activeSelf)e.Tick(dt);
            effects?.Tick(dt);
            messageRemaining-=dt;if(messageRemaining<=0)Message="";
        }

        void CompleteAction()
        {
            if(Current==null)return;
            bool wasPlayer=Phase==BattlePhase.PlayerAction;
            var definition=Current;CompletedActions++;
            if(Source!=null)Source.EndActionMotion();
            if(Source!=null && !Source.Dead)Source.Play(wasPlayer && Guarding?"Guard":"IdleBattle");
            if(wasPlayer && definition.kind!=BattleActionKind.EarthPrison && actionDidDamage)Ultimate=Mathf.Clamp(Ultimate+definition.ultimateGain,0,100);
            if(!wasPlayer)Ultimate=Mathf.Clamp(Ultimate+(enemyDidDodge?tuning.dodgeGain:enemyDidDamage?tuning.damagedGain:0),0,100);
            Current=null;Source=null;queuedDodge.Clear();effects?.EndAction();earthPrison?.StopEffect();
            if(player.Dead){FinishBattle(false);return;}
            if(LivingEnemies()==0){FinishBattle(true);return;}
            if(wasPlayer)enemyCursor=0;
            NextEnemy();
        }

        void NextEnemy()
        {
            while(enemyCursor<enemies.Length)
            {
                var enemy=enemies[enemyCursor++];if(!IsAlive(enemy))continue;
                targets.Clear();targets.Add(player);EnemyActionsStarted++;
                bool combo=tuning.enemyCombos && PlayerTurn>=2 && (PlayerTurn+enemy.actorId)%2==0;
                BeginAction(enemy,combo?tuning.enemyCombo:tuning.enemyStrike,BattlePhase.EnemyAction);return;
            }
            Guarding=false;PlayerTurn++;EnsureTarget();Phase=BattlePhase.PlayerCommand;Current=null;Source=null;
            if(!player.Dead)player.Play("IdleBattle");
            Say("LƯỢT CỦA BẠN",1.5f);hud?.Refresh(true);
        }

        void FinishBattle(bool victory)
        {
            if(Source!=null)Source.EndActionMotion();
            Current=null;Source=null;queuedDodge.Clear();Guarding=false;
            effects?.EndAction();earthPrison?.StopEffect();Phase=victory?BattlePhase.Victory:BattlePhase.Defeat;
            if(!player.Dead)player.Play("IdleBattle");
            foreach(var e in enemies)if(IsAlive(e))e.Play("IdleBattle");
            Say(victory?"CHIẾN THẮNG":"THẤT BẠI",1000);hud?.Refresh(true);
        }
        public void FillUltimate(){if(Phase==BattlePhase.Victory || Phase==BattlePhase.Defeat)return;Ultimate=100;Say("DEBUG · Ultimate đã đầy",2);hud?.Refresh(true);}
        public void ToggleEasy(){EasyDodge=!EasyDodge;hud?.Refresh(true);}
        public void SetPlaybackSpeed(float speed)
        {
            float held=ActionTime;PlaybackSpeed=Mathf.Clamp(speed,.5f,2);
            epoch=Time.realtimeSinceStartupAsDouble-held/PlaybackSpeed;queuedDodge.Clear();hud?.Refresh(true);
        }
        public void TogglePause()
        {
            if(!started)return;
            if(!Paused && InAction && !manualSimulation)AdvanceAction((float)((Time.realtimeSinceStartupAsDouble-epoch)*PlaybackSpeed));
            Paused=!Paused;queuedDodge.Clear();
            epoch=Time.realtimeSinceStartupAsDouble-ActionTime/PlaybackSpeed;
            lastRealTime=Time.realtimeSinceStartupAsDouble;effects?.SetPaused(Paused);hud?.Refresh(true);
        }
        void OnApplicationFocus(bool focus) {if(!Application.isEditor && started && !focus && !Paused)TogglePause();}
        void EnsureTarget(){if(IsAlive(SelectedTarget))return;for(int i=0;i<enemies.Length;i++)if(IsAlive(enemies[i])){selectedEnemy=i;return;}}
        static bool IsAlive(BattleActor actor)=>actor!=null && actor.gameObject.activeSelf && !actor.Dead;
        int LivingEnemies(){int n=0;foreach(var e in enemies)if(IsAlive(e))n++;return n;}
        void Say(string message,float seconds){Message=message;messageRemaining=seconds;}
    }
}

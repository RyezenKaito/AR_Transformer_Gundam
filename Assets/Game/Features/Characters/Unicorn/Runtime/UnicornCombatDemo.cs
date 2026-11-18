using System;
using System.Collections.Generic;
using System.Linq;
using GundamGame.CombatDemo;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GundamGame.Unicorn
{
    public sealed class UnicornCombatDemo : MonoBehaviour
    {
        public enum Phase { Deploying, Select, Skill, Windup, EnemyRecovery, Victory, Defeat }
        public readonly struct Skill
        {
            public readonly string name, clip;
            public readonly int cost, frames;
            public readonly float hp, armor;
            public Skill(string name, string clip, int cost, int frames, float hp, float armor)
            { this.name=name; this.clip=clip; this.cost=cost; this.frames=frames; this.hp=hp; this.armor=armor; }
        }
        public static readonly Skill[] Skills = {
            new Skill("Twin Vulcan","TwinVulcan",0,27,.65f,.45f),
            new Skill("Psycho-Frame Awakening","PsychoFrameAwakening",1,84,0,0),
            new Skill("Beam Magnum","BeamMagnum",2,92,1.85f,1.10f),
            new Skill("Psycho-Frame Full Burst","FullBurst",5,195,3.40f,1.65f)
        };

        public const string CombatCueEvent = "OnCombatCue";
        public const int BuffCue = 1, MagnumCue = 2, ComboFirstCue = 3, ComboSecondCue = 4, ComboKickCue = 5, UltraBeamCue = 6;
        const float EndFrameTolerance = .00001f;
        const float WheelZoomSensitivity = .22f;
        const float MinCameraDistance = 7f, MaxCameraDistance = 17f;

        public DemoFighter player;
        public DemoFighter[] enemies;
        public GameObject model;
        public AnimationClip[] clips;
        public Camera battleCamera;
        public UnicornCombatEffects effects;
        public Transform muzzle, eject, vulcanL, vulcanR, focusHead, focusBody, handRight, handLeft, kickFoot;
        public UnicornBuffSettings buffSettings;
        public CharacterCombatProfile profile;
        public MechaBattleHUD hudPrefab;
        public RougeHudArt hudArt;
        public int BuffTurns { get; private set; }
        public int EffectiveAttack => Mathf.RoundToInt(player.attack*(1+(BuffTurns>0?buffSettings.attackBonus:0)));
        public int EffectiveDefense => Mathf.RoundToInt(player.defense*(1+(BuffTurns>0?buffSettings.defenseBonus:0)));
        public Phase State { get; private set; }
        public int Burst { get; private set; }
        public int TargetIndex { get; private set; }
        public int SelectedSkill { get; private set; }
        public int DamageEvents { get; private set; }
        public int SkillCompletions { get; private set; }
        public int LastResolvedSkill { get; private set; }
        public bool IsPaused => manualPause || appPause || trackingPaused;
        public bool Cinematic => State==Phase.Skill && activeSkill==3 && !arPresentation;
        public float ActionSeconds => clock;
        public string Notice { get; private set; }
        public float Playback { get; set; } = 1;
        public bool deterministicDefense;
        public float defenseTestRoll=.25f;
        public bool arPresentation;
        public bool IsStoryBattle { get; set; }
        public event Action<BattleReactionResult> ReactionResolved;
        public Action PauseRequested { get; set; }
        public bool showHud=true;
        public bool CanChooseAction => State==Phase.Select && !IsPaused;
        public bool CanDefend => State==Phase.Windup && !IsPaused && defense==0 && (!enemyBrain || enemyBrain.Move.hits>0);
        EnemyBrain enemyBrain;
        int enemyHits;
        float enemyDefenseScale;
        float enemyRecoil;
        Vector3 enemyRecoilDirection;
        public float WindupDuration => enemyBrain ? enemyBrain.FirstHitTime : 1.45f;
        public string IncomingMove => enemyBrain ? enemyBrain.Move.label : "BEAM RIFLE";
        public bool IsIncomingAttack => State==Phase.Windup && (!enemyBrain || enemyBrain.Move.hits>0);
        public const float DodgeWindow=.40f, ParryWindow=.20f;
        public float DefenseProgress => Mathf.Clamp01(clock/WindupDuration);
        public float DefenseRemaining => WindupDuration-clock;
        public DemoFighter Target => enemies[TargetIndex];
        public DemoFighter Acting => acting;
        public bool HasStarted => started;
        public bool TrackingPaused => trackingPaused;
        public bool CanExecuteSelected => CanChooseAction && Burst>=GetSkill(SelectedSkill).cost &&
            (SelectedSkill==1 || Target.IsAlive);
        public void PreviewTurns(List<DemoFighter> output,int count) => turns.Preview(output,count,acting);
        public Skill GetSkill(int index)
        {
            if(!profile)return Skills[index];
            var skill=profile.skills[index];
            return new Skill(skill.displayName,skill.animation.name,
                skill.burstCost,Mathf.RoundToInt(skill.duration*30f),skill.damageMultiplier,skill.armorMultiplier);
        }

        readonly RougeTurnOrder turns = new RougeTurnOrder();
        readonly System.Collections.Generic.Dictionary<string, AnimationClip> actions = new();
        Renderer[][] enemyRenderers;
        Transform magnumMount;
        bool cinematicOccludersHidden;
        AnimationEvent[] combatCues=Array.Empty<AnimationEvent>();
        int nextCombatCue;
        DemoFighter acting, lockedTarget;
        int activeSkill, cueMask, defense;
        float clock, idleClock, reactionClock=-1, reactionLength;
        string reaction;
        bool manualPause, appPause, trackingPaused, started;
        Vector3 visualHome;
        Quaternion visualRotation;
        float yaw=99, pitch=27, distance=13.1f;
        Vector3 arenaCameraFocus = new(0, .75f, .12f);
        Vector2 arenaCameraPitchYaw = new(27, 99);
        float arenaCameraDistance = 13.1f;
        Vector3 cameraHome;
        Quaternion cameraRotation;
        GUIStyle heading, button;
        UnicornBattleHud hud;
        CombatTestScene testScene;
        static readonly Color Cyan=new(.23f,.86f,.99f), Amber=new(1,.65f,.23f), Rose=new(1,.25f,.43f);

        void Start()
        {
            foreach (var clip in clips) if (clip) actions[clip.name]=clip;
            if(profile)
                foreach(var skill in profile.skills)
                    if(skill.animation)actions[skill.animation.name]=skill.animation;
            foreach(var animator in model.GetComponentsInChildren<Animator>()) animator.enabled=false;
            enemyRenderers=enemies.Select(enemy=>enemy.GetComponentsInChildren<Renderer>()).ToArray();
            magnumMount=model.GetComponentsInChildren<Transform>().Single(item=>item.name=="RifleMount");
            visualHome=player.visual.localPosition;
            visualRotation=player.visual.localRotation;
            effects.Initialize(model);
            testScene=GetComponentInParent<CombatTestScene>();
            started=true;
            ResetBattle(true);
            if(!IsStoryBattle)hud=UnicornBattleHud.Create(this,hudPrefab,hudArt);
        }

        public void ResetBattle(bool deploy=false)
        {
            if (!started) return;
            effects.Clear();
            player.ResetFighter();
            foreach (var enemy in enemies){enemy.ResetFighter();if(enemy.TryGetComponent<EnemyBrain>(out var brain))brain.ResetBrain();}
            enemyBrain=null;
            enemyRecoil=0;
            turns.Reset(player,enemies);
            Burst=CharacterCombatProfile.StartingBurst;TargetIndex=0;SelectedSkill=0;activeSkill=-1;LastResolvedSkill=-1;
            clock=0;idleClock=0;reactionClock=-1;cueMask=0;defense=0;acting=null;lockedTarget=null;
            combatCues=Array.Empty<AnimationEvent>();nextCombatCue=0;BuffTurns=0;
            DamageEvents=0;SkillCompletions=0;manualPause=false;
            ResetVisual();
            Notice="RX-0 // HỆ THỐNG SẴN SÀNG";
            State=deploy?Phase.Deploying:Phase.Select;
            if (!deploy) Advance();
            UpdateCamera();
        }

        public void SetPaused(bool value) { manualPause=value; effects.SetPaused(IsPaused); }
        public void ConfigureArenaCamera(Vector3 focus, Vector2 pitchYaw, float orbitDistance)
        {
            arenaCameraFocus = focus;
            arenaCameraPitchYaw = pitchYaw;
            arenaCameraDistance = Mathf.Clamp(orbitDistance, MinCameraDistance, MaxCameraDistance);
            ResetCameraView();
        }
        public void ResetCameraView()
        {
            pitch=arenaCameraPitchYaw.x;
            yaw=arenaCameraPitchYaw.y;
            distance=arenaCameraDistance;
            UpdateCamera();
        }
        public void SetTrackingValid(bool value) { trackingPaused=!value; if(started)effects.SetPaused(IsPaused); }
        void OnApplicationPause(bool value) { appPause=value; if(started)effects.SetPaused(IsPaused); }
        public void ChargeForTest() { if(State==Phase.Select&&!IsPaused){Burst=5;Notice="QA // BURST 5/5";} }

        public bool SelectTarget(int index)
        {
            if (IsPaused || State!=Phase.Select || index<0 || index>=enemies.Length || !enemies[index].IsAlive) return false;
            TargetIndex=index; return true;
        }

        public bool SelectSkill(int index)
        {
            if(IsPaused || State!=Phase.Select || index<0 || index>=Skills.Length) return false;
            SelectedSkill=index;
            return true;
        }

        public bool ExecuteSkill()
        {
            if(IsPaused || State!=Phase.Select || acting!=player) return false;
            var skill=GetSkill(SelectedSkill);
            if(Burst<skill.cost || (SelectedSkill!=1 && !enemies[TargetIndex].IsAlive)) return false;
            var clip=FindClip(skill.clip);
            if(!clip)return false;
            combatCues=clip.events.Where(item=>item.functionName==CombatCueEvent).OrderBy(item=>item.time).ToArray();
            nextCombatCue=0;
            Burst-=skill.cost;
            activeSkill=SelectedSkill;lockedTarget=activeSkill==1?null:enemies[TargetIndex];clock=0;cueMask=0;
            reactionClock=-1;
            cameraHome=battleCamera.transform.position;cameraRotation=battleCamera.transform.rotation;
            State=Phase.Skill;Notice=skill.name.ToUpperInvariant();
            if(Cinematic)SetCinematicOccluders(true);
            ApplySkillPose();
            return true;
        }

        public bool Defend(bool parry)
        {
            if (!CanDefend) return false;
            var remaining=DefenseRemaining;
            if(remaining>(parry?ParryWindow:DodgeWindow) || remaining<0)
            { defense=1;Notice="SAI THỜI ĐIỂM";ReactionResolved?.Invoke(BattleReactionResult.Hit);return false; }
            if(parry){defense=2;Notice="PARRY // I-FIELD";BeginReaction("Parry",.57f);}
            else
            {
                bool evade=RougeBattleDemo.DodgeEvades(player.speed,acting.speed,deterministicDefense?defenseTestRoll:UnityEngine.Random.value);
                defense=evade?3:1;Notice=evade?"DODGE // EVADED":"DODGE // MISSED";
                BeginReaction("Dodge",.6f);
            }
            ReactionResolved?.Invoke(defense==3?BattleReactionResult.Dodge:defense==1?BattleReactionResult.Hit:
                RougeBattleDemo.ParryMultiplier(EffectiveDefense,acting.attack)==0?BattleReactionResult.FullBlock:BattleReactionResult.HalfBlock);
            return true;
        }

        public bool SkipCinematic()
        {
            if(State!=Phase.Skill || activeSkill!=3 || IsPaused) return false;
            clock=GetSkill(3).frames/30f;
            ResolveCombatCues(false);
            effects.Clear();
            FinishSkill();
            UpdateCamera();
            return true;
        }

        void Update()
        {
            if (!started) return;
            ReadInput();
            if (!IsPaused) Step(Time.deltaTime*Mathf.Clamp(Playback,.15f,3));
            UpdateCamera();
        }

        void ReadInput()
        {
            var key=Keyboard.current;
            if(Cinematic)
            {
                if(key!=null && key.escapeKey.wasPressedThisFrame)SkipCinematic();
                return;
            }
            if (key!=null && !IsStoryBattle)
            {
                if(key.digit1Key.wasPressedThisFrame)SelectSkill(0);
                if(key.digit2Key.wasPressedThisFrame)SelectSkill(1);
                if(key.digit3Key.wasPressedThisFrame)SelectSkill(2);
                if(key.digit4Key.wasPressedThisFrame)SelectSkill(3);
                if(key.enterKey.wasPressedThisFrame)ExecuteSkill();
                if(key.qKey.wasPressedThisFrame)Defend(true);
                if(key.eKey.wasPressedThisFrame)Defend(false);
                if(key.escapeKey.wasPressedThisFrame)SkipCinematic();
                if(key.pKey.wasPressedThisFrame)
                {
                    if (PauseRequested != null) PauseRequested();
                    else SetPaused(!manualPause);
                }
                if(key.rKey.wasPressedThisFrame)ResetBattle();
            }
            if (arPresentation || Cinematic || IsPaused || Mouse.current==null) return;
            var mouse=Mouse.current;
            if(mouse.rightButton.isPressed)
            {
                var delta=mouse.delta.ReadValue();
                yaw+=delta.x*.18f;pitch=Mathf.Clamp(pitch-delta.y*.13f,7,50);
            }
            float scroll=mouse.scroll.ReadValue().y;
            ApplyWheelZoom(scroll);
            if(!IsStoryBattle && mouse.leftButton.wasPressedThisFrame && State==Phase.Select)
            {
                var pos=mouse.position.ReadValue();
                if(testScene && testScene.BlocksWorldPointer(pos))return;
                if(hud!=null && hud.BlocksWorldPointer(pos))return;
                if(pos.y<Screen.height*.25f || pos.y>Screen.height*.84f)return;
                var ray=battleCamera.ScreenPointToRay(pos);
                int nearest=-1;float depth=float.PositiveInfinity;
                for(int i=0;i<enemies.Length;i++)
                {
                    if(!enemies[i].IsAlive)continue;
                    var bound=new Bounds(enemies[i].AimPoint,new Vector3(1.6f,2.5f,1.6f));
                    if(bound.IntersectRay(ray,out var hit)&&hit<depth){nearest=i;depth=hit;}
                }
                if(nearest>=0)SelectTarget(nearest);
            }
        }

        void ApplyWheelZoom(float scroll)
        {
            if(arPresentation || Cinematic || IsPaused || Mathf.Abs(scroll)<=.001f)return;
            float factor=RougeArenaCamera.ScrollZoomFactor(
                scroll,WheelZoomSensitivity,RougeArenaCamera.UsesLegacyWindowsScrollRange);
            distance=Mathf.Clamp(distance*factor,MinCameraDistance,MaxCameraDistance);
        }

        void Step(float dt)
        {
            clock+=dt;idleClock+=dt;
            foreach(var enemy in enemies)
                if(enemy.TryGetComponent<EnemyBrain>(out var brain))brain.Observe(dt);
            switch(State)
            {
                case Phase.Deploying:
                    Sample("Deploy",clock);
                    player.visual.localPosition=visualHome+Vector3.up*(3.1f*Mathf.Pow(1-Mathf.Clamp01(clock/.55f),2));
                    Cue(0,17,()=>effects.Impact(player.transform.position,.75f,Cyan,true));
                    if(clock>=1.2f){ResetVisual();Advance();}
                    break;
                case Phase.Skill:
                    ResolveCombatCues(true);
                    ApplySkillPose();
                    PlayCues();
                    if(clock+EndFrameTolerance>=GetSkill(activeSkill).frames/30f)FinishSkill();
                    break;
                case Phase.Windup:
                    SampleReaction(dt);
                    float p=DefenseProgress;
                    if(enemyBrain)
                    {
                        EvaluateEnemyPose();
                        if(clock>=WindupDuration)
                        {
                            State=Phase.EnemyRecovery;
                        }
                        break;
                    }
                    else acting.Pose(Vector3.zero,new Vector3(-p*7,0,0),-p*26);
                    if(clock>=WindupDuration)EnemyHit();
                    break;
                case Phase.EnemyRecovery:
                    SampleReaction(dt);
                    if(enemyRecoil>0)
                    {
                        float strength=enemyRecoil;
                        enemyRecoil=Mathf.Max(0,enemyRecoil-dt*2.7f);
                        player.visual.position+=enemyRecoilDirection*Mathf.Sin((1-strength)*Mathf.PI)*strength*.52f;
                        if(enemyRecoil==0)player.visual.localPosition=visualHome;
                    }
                    if(enemyBrain)EvaluateEnemyPose();
                    if(clock>=(enemyBrain?enemyBrain.Duration:.68f))
                    {
                        if(enemyBrain)enemyBrain.Finish();
                        effects.SetEnemyJavelin(false);
                        acting.RestorePose();if(player.IsAlive)Advance();else State=Phase.Defeat;
                    }
                    break;
                case Phase.Select:
                case Phase.Victory:
                    Sample("Idle",idleClock,true);
                    break;
                case Phase.Defeat:
                    player.visual.localRotation=visualRotation;
                    Sample("Defeat",clock);
                    break;
            }
            effects.Tick(dt);
        }

        void ApplySkillPose()
        {
            player.visual.localPosition=visualHome;
            player.visual.localRotation=visualRotation;
            Vector3 delta=lockedTarget?lockedTarget.transform.position-player.transform.position:Vector3.zero;
            delta.y=0;
            Vector3 forward=delta.sqrMagnitude>.001f?delta.normalized:player.transform.forward;
            if(lockedTarget)player.visual.rotation=Quaternion.LookRotation(forward)*visualRotation;
            Sample(GetSkill(activeSkill).clip,clock);
            float frame=clock*30f;
            if(activeSkill==1)
            {
                float energy=Ease(Mathf.InverseLerp(5,42,frame))*(1-Ease(Mathf.InverseLerp(54,81,frame)));
                effects.SetResonance(energy);
                effects.Aura(player.transform.position,energy);
            }
            if(activeSkill==2 && frame>=36 && frame<=67)
            {
                float alignment=Pulse(frame,36,48,67);
                Vector3 from=muzzle.position-magnumMount.position;
                Vector3 toward=lockedTarget.AimPoint-magnumMount.position;
                var aim=Quaternion.FromToRotation(from,toward);
                magnumMount.rotation=Quaternion.Slerp(Quaternion.identity,aim,alignment)*magnumMount.rotation;
            }
            if(activeSkill==3)
            {
                float approach=Ease(Mathf.InverseLerp(9,26,frame));
                float air=Ease(Mathf.InverseLerp(67,91,frame))-Ease(Mathf.InverseLerp(153,181,frame));
                float retreat=Ease(Mathf.InverseLerp(151,181,frame));
                float strike=Pulse(frame,25,30,36)*.19f+Pulse(frame,39,44,50)*.19f+Pulse(frame,53,61,67)*.30f;
                float range=Mathf.Max(0,delta.magnitude-1.30f);
                player.visual.position+=forward*(range*(approach-retreat)-1.02f*air+strike)+Vector3.up*(2.15f*air);
                if(frame>=70)
                {
                    var aim=lockedTarget.AimPoint-focusBody.position;
                    float tilt=-Vector3.SignedAngle(aim,forward,player.visual.right);
                    player.visual.rotation*=Quaternion.Euler(Mathf.Clamp(tilt,0,37)*Pulse(frame,115,139,165),0,0);
                }
                float power=Ease(Mathf.InverseLerp(75,137,frame))*(1-Ease(Mathf.InverseLerp(139,163,frame)));
                effects.SetResonance(power);
                if(frame>=91 && frame<139)
                    effects.Converge(focusBody.position,BeamOrigin,power);
            }
        }

        void Cue(int bit,int frame,Action fire)
        {
            int flag=1<<bit;
            if((cueMask&flag)!=0 || clock+1e-5f<frame/30f)return;
            cueMask|=flag;fire();
        }

        void PlayCues()
        {
            switch(activeSkill)
            {
                case 0:
                    var target=lockedTarget.AimPoint;
                    Cue(0,7,()=>effects.Tracer(vulcanL.position,target));
                    Cue(1,9,()=>effects.Tracer(vulcanR.position,target));
                    Cue(2,12,()=>{effects.Tracer(vulcanL.position,target);ApplyHit(1);});
                    Cue(3,14,()=>effects.Tracer(vulcanR.position,target));
                    break;
                case 1:
                    Cue(0,12,()=>effects.Play(4));
                    break;
                case 2:
                    Cue(0,14,()=>effects.Play(0));
                    Cue(1,40,()=>effects.Impact(muzzle.position,.19f,Amber,false));
                    Cue(2,54,()=>effects.Eject(eject.position,player.visual.right));
                    Cue(3,78,()=>effects.Play(0));
                    break;
                case 3:
                    Cue(0,20,()=>effects.Play(0));
                    Cue(1,74,()=>effects.Ring(player.transform.position+Vector3.up*.05f,.2f,1.6f,Rose,.42f));
                    Cue(2,93,()=>effects.Play(4));
                    break;
            }
        }

        void ResolveCombatCues(bool feedback)
        {
            // SampleAnimation does not dispatch Animation Events. Use the
            // imported event time and sample its pose before resolving the cue.
            float elapsed=clock;
            while(nextCombatCue<combatCues.Length && combatCues[nextCombatCue].time<=elapsed+EndFrameTolerance)
            {
                var cue=combatCues[nextCombatCue++];
                if(feedback)
                {
                    clock=cue.time;
                    ApplySkillPose();
                }
                OnCombatCue(cue.intParameter,feedback);
            }
            clock=elapsed;
        }

        Vector3 BeamOrigin => (handLeft.position+handRight.position)*.5f+player.visual.forward*.19f;

        void OnCombatCue(int cue,bool feedback)
        {
            switch(cue)
            {
                case BuffCue:
                    BuffTurns=buffSettings.followingTurns;
                    Notice="PSYCHO-FRAME AWAKENING";
                    if(feedback)effects.BuffPulse(player.transform.position);
                    break;
                case MagnumCue:
                    if(feedback)effects.Beam(muzzle.position,lockedTarget.AimPoint);
                    ApplyHit(1,feedback);
                    break;
                case ComboFirstCue:
                case ComboSecondCue:
                case ComboKickCue:
                    Vector3 limb=cue==ComboFirstCue?handRight.position:cue==ComboSecondCue?handLeft.position:kickFoot.position;
                    if(feedback)effects.MartialImpact(Vector3.Lerp(limb,lockedTarget.AimPoint,.65f),player.visual.forward,cue==ComboKickCue);
                    ApplyHit(cue==ComboKickCue?.22f:.12f,feedback);
                    break;
                case UltraBeamCue:
                    if(feedback)effects.UltraBeam(BeamOrigin,lockedTarget.AimPoint);
                    ApplyHit(.54f,feedback,true);
                    break;
            }
        }

        void ApplyHit(float fraction,bool feedback=true,bool finisher=false)
        {
            if(!lockedTarget.IsAlive || (activeSkill==3 && !finisher && lockedTarget.Health==1))return;
            var skill=GetSkill(activeSkill);
            int oldBreak=lockedTarget.ArmorBreakCount;
            // Keep the Ultra Beam as the finisher even against a low-HP target.
            int floor=activeSkill==3&&!finisher?1:0;
            int amount=Deal(player,lockedTarget,skill.hp*fraction,skill.armor*fraction,1,floor);
            DamageEvents++;
            bool broke=lockedTarget.ArmorBreakCount!=oldBreak;
            if(feedback)
            {
                effects.Impact(lockedTarget.AimPoint,finisher?.94f:.38f,broke?Cyan:Rose,broke||finisher);
                if(broke)effects.Play(5);
            }
            if(lockedTarget.IsAlive) lockedTarget.Pose(Vector3.zero,new Vector3(-7,0,activeSkill==1?9:3));
            Notice=(broke?"ARMOR BREAK  /  ":"")+"−"+amount+" HP";
        }

        int Deal(DemoFighter from,DemoFighter to,float hp,float armor,float reaction,int healthFloor=0)
        {
            int attack=from==player?EffectiveAttack:from.attack;
            to.TryGetComponent<EnemyBrain>(out var targetBrain);
            if(targetBrain)
            {
                targetBrain.RegisterPressure(attack*hp);
                if(targetBrain.Guarding && Vector3.Dot(to.transform.forward,(from.transform.position-to.transform.position).normalized)>.25f)
                {
                    reaction*=.42f;
                    if(targetBrain.shield)effects.Shield(targetBrain.shield.position,to.transform.forward);
                }
            }
            int defenseValue=to==player?EffectiveDefense:to.defense;
            float raw=attack*hp*100f/(100f+defenseValue);
            return to.ReceivePreciseCombatHit(raw,(.5f*attack+.5f*from.breakDamage)*armor,reaction,healthFloor);
        }

        void FinishSkill()
        {
            LastResolvedSkill=activeSkill;SkillCompletions++;
            Burst=Mathf.Min(CharacterCombatProfile.MaximumBurst,Burst+(activeSkill==0?1:0)+CharacterCombatProfile.TurnEndBurst);
            if(lockedTarget && lockedTarget.IsAlive)lockedTarget.RestorePose();
            if(lockedTarget && GetSkill(activeSkill).hp>0 && lockedTarget.TryGetComponent<EnemyBrain>(out var targetBrain))
                targetBrain.EndGuard();
            if(activeSkill==3){battleCamera.transform.SetPositionAndRotation(cameraHome,cameraRotation);}
            if(activeSkill!=1 && BuffTurns>0)BuffTurns--;
            activeSkill=-1;
            combatCues=Array.Empty<AnimationEvent>();
            effects.SetResonance(0);
            ResetVisual();Advance();
        }

        void Advance()
        {
            clock=0;cueMask=0;defense=0;
            if(!player.IsAlive){State=Phase.Defeat;Notice="RX-0 // HẠ GỤC";return;}
            if(!enemies.Any(e=>e.IsAlive)){State=Phase.Victory;Notice="NHIỆM VỤ HOÀN THÀNH";return;}
            if(!enemies[TargetIndex].IsAlive)TargetIndex=Array.FindIndex(enemies,e=>e.IsAlive);
            acting=turns.TakeNext();
            enemyBrain=acting==player?null:acting.GetComponent<EnemyBrain>();
            State=acting==player?Phase.Select:Phase.Windup;
            Notice=acting==player?"CHỌN MỤC TIÊU VÀ KỸ NĂNG":acting.displayName+" // CHUẨN BỊ TẤN CÔNG";
            if(State==Phase.Windup)
            {
                if(enemyBrain)
                {
                    enemyBrain.Begin(player);
                    enemyHits=0;
                    Notice=enemyBrain.Move.label+(enemyBrain.Move.hits>0?" / DODGE OR PARRY":" / DEFENSIVE STANCE");
                }
                effects.Play(0);
            }
        }

        void EnemyHit()
        {
            if(defense==0)ReactionResolved?.Invoke(BattleReactionResult.Hit);
            var hitFrom=acting.MuzzlePoint;
            float scale=defense==3?0:defense==2?RougeBattleDemo.ParryMultiplier(EffectiveDefense,acting.attack):1;
            int hp=0;
            Vector3 field=handLeft.position+player.visual.forward*.16f;
            var aim=defense==3?player.AimPoint+player.transform.right*.8f:defense==2?field:player.AimPoint;
            hp=Deal(acting,player,.37f,.55f,scale);
            effects.Beam(hitFrom,aim,true);
            if(defense==2)effects.Shield(field,(hitFrom-field).normalized);
            if(hp>0)
            {
                effects.Impact(player.AimPoint,.38f,Amber,false);
                if(defense==0 || reactionClock<0)BeginReaction("HitReact",.8f);
            }
            if(!player.IsAlive)
            {
                player.visual.localRotation=visualRotation;
                BeginReaction("Defeat",1);
            }
            Notice=defense==3?"NÉ THÀNH CÔNG":
                defense==2?(hp==0?"PARRY // CHẶN HOÀN TOÀN":"PARRY // −"+hp+" HP"):"RX-0  /  −"+hp+" HP";
            clock=0;State=Phase.EnemyRecovery;
        }

        void EvaluateEnemyPose()
        {
            enemyBrain.Evaluate(clock,OnEnemyCue);
            effects.SetEnemyJavelin(enemyBrain.IsMelee && enemyBrain.Slot==2 && enemyBrain.BladeLit,
                enemyBrain.BladeBase,enemyBrain.BladeTip);
        }

        void OnEnemyCue(EnemyProfile.Cue cue)
        {
            EnemyAttackPresentation.Play(cue, enemyBrain, player, effects, () => clock, enemyHits, defense==3);
            if (cue==EnemyProfile.Cue.Hit) ResolveEnemyHit();
            if (cue==EnemyProfile.Cue.Guard) Notice="TACTICAL SHIELD GUARD";
        }

        void ResolveEnemyHit()
        {
            if(enemyHits==0)
            {
                if(defense==0)ReactionResolved?.Invoke(BattleReactionResult.Hit);
                enemyDefenseScale=defense==3?0:defense==2?RougeBattleDemo.ParryMultiplier(EffectiveDefense,acting.attack):1;
            }
            enemyHits++;
            if(!player.IsAlive)return;
            var move=enemyBrain.Move;
            float scale = EnemyAttackPresentation.Contacts(enemyBrain,player) ? enemyDefenseScale : 0;
            int damage=Deal(acting,player,move.damage/(float)Mathf.Max(1,acting.attack*move.hits),move.armorMultiplier/move.hits,scale);
            if(defense==2 && enemyHits==1)effects.Shield(handLeft.position, -acting.transform.forward);
            if(damage>0)
            {
                effects.Impact(player.AimPoint,.44f,Amber,false);
                if(defense==0 || reactionClock<0)BeginReaction("HitReact",.45f);
                if(enemyBrain.IsBlast)
                {
                    enemyRecoil=1;
                    enemyRecoilDirection=Vector3.ProjectOnPlane(player.transform.position-acting.transform.position,Vector3.up).normalized;
                }
            }
            Notice=defense==3?"DODGE / EVADED":damage>0?move.label+" / −"+damage+" HP":move.label+" / NO DAMAGE";
            if(!player.IsAlive)BeginReaction("Defeat",1);
        }

        void BeginReaction(string name,float length)
        { reaction=name;reactionLength=length;reactionClock=0; }

        void SampleReaction(float dt)
        {
            if(reactionClock<0){Sample("Idle",idleClock,true);return;}
            reactionClock+=dt;
            Sample(reaction,reactionClock);
            player.visual.localPosition=visualHome;
            if(reaction=="Dodge")player.visual.localPosition+=Vector3.right*.48f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(reactionClock/.6f));
            if(reactionClock>=reactionLength && reaction!="Defeat")reactionClock=-1;
        }

        void Sample(string name,float t,bool loop=false)
        {
            var clip=FindClip(name);
            if(clip)
                clip.SampleAnimation(model,loop?Mathf.Repeat(t,clip.length):Mathf.Min(t,clip.length));
        }

        AnimationClip FindClip(string name) => actions.TryGetValue(name,out var clip) ||
            actions.TryGetValue("Unicorn_"+name,out clip) ? clip : null;

        void ResetVisual()
        {
            SetCinematicOccluders(false);
            player.visual.localPosition=visualHome;player.visual.localRotation=visualRotation;
            Sample("Idle",0);
        }

        void SetCinematicOccluders(bool hide)
        {
            if(hide==cinematicOccludersHidden)return;
            cinematicOccludersHidden=hide;
            for(int i=0;i<enemyRenderers.Length;i++)
                if(enemies[i]!=lockedTarget || !hide)
                    foreach(var renderer in enemyRenderers[i])renderer.forceRenderingOff=hide;
        }

        void UpdateCamera()
        {
            if(arPresentation || !battleCamera || IsPaused)return;
            Vector3 focus=arenaCameraFocus;
            if(!Cinematic)
            {
                var orbit=Quaternion.Euler(pitch,yaw,0);
                CinematicPose(focus+orbit*new Vector3(0,0,-distance),focus);
                return;
            }
            Vector3 front=player.transform.position==lockedTarget.transform.position
                ? player.transform.forward : Vector3.ProjectOnPlane(lockedTarget.transform.position-player.transform.position,Vector3.up).normalized;
            Vector3 right=Vector3.Cross(Vector3.up,front);
            Vector3 body=focusBody.position, target=lockedTarget.AimPoint;
            float frame=clock*30f;
            Vector3 fightFocus=Vector3.Lerp(body-Vector3.up*.32f,target,.43f);
            Vector3 fight=fightFocus-right*4.7f-front*.85f+Vector3.up*1.17f;
            Vector3 chargeFocus=Vector3.Lerp(body,BeamOrigin,.28f);
            Vector3 charge=chargeFocus-right*3.4f+front*2.85f+Vector3.up*.46f;
            Vector3 fireFocus=Vector3.Lerp(body,target,.51f);
            Vector3 fire=fireFocus-right*6.9f-front*.90f+Vector3.up*1.45f;
            if(frame<66)
            {
                float entry=Ease(frame/12);
                CinematicPose(Vector3.Lerp(cameraHome,fight,entry),Vector3.Lerp(focus,fightFocus,entry));
            }
            else if(frame<124)
            {
                float rise=Ease(Mathf.InverseLerp(66,94,frame));
                CinematicPose(Vector3.Lerp(fight,charge,rise),Vector3.Lerp(fightFocus,chargeFocus,rise));
            }
            else
            {
                float wide=Ease(Mathf.InverseLerp(124,138,frame));
                float end=Ease(Mathf.InverseLerp(163,195,frame));
                CinematicPose(Vector3.Lerp(Vector3.Lerp(charge,fire,wide),cameraHome,end),
                    Vector3.Lerp(Vector3.Lerp(chargeFocus,fireFocus,wide),focus,end));
            }
        }

        void CinematicPose(Vector3 p,Vector3 focus)
        { battleCamera.transform.position=p; battleCamera.transform.LookAt(focus); }
        static float Ease(float t)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(t));
        static float Pulse(float frame,float start,float peak,float end) =>
            Ease(Mathf.InverseLerp(start,peak,frame))*(1-Ease(Mathf.InverseLerp(peak,end,frame)));

        void OnGUI()
        {
            if(!started || !showHud || !Cinematic)return;
            heading??=new GUIStyle(GUI.skin.label){fontSize=21,fontStyle=FontStyle.Bold,
                normal={textColor=Cyan},alignment=TextAnchor.MiddleLeft};
            button??=new GUIStyle(GUI.skin.button){fontSize=15,fontStyle=FontStyle.Bold};
            var old=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0),
                Quaternion.identity,Vector3.one*scale);
            GUI.color=new Color(.02f,.03f,.05f,.97f);
            GUI.DrawTexture(new Rect(0,0,1280,66),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0,654,1280,66),Texture2D.whiteTexture);
            GUI.color=Color.white;
            GUI.Label(new Rect(36,665,800,42),"RX-0    /    PSYCHO-FRAME FULL BURST",heading);
            if(GUI.Button(new Rect(1120,668,125,34),"SKIP  ESC",button))SkipCinematic();
            GUI.matrix=old;
        }
    }
}

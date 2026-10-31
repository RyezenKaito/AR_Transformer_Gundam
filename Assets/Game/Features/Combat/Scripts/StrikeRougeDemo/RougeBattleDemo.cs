using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GundamGame.CombatDemo
{
    /// <summary>Self-contained one-versus-three prototype. Imported assets remain untouched.</summary>
    public sealed class RougeBattleDemo : MonoBehaviour
    {
        public enum BattlePhase { PlayerTurn, PlayerAction, EnemyWindup, EnemyAction, Victory, Defeat, Suspended }
        public enum DefenseResult { None, Failed, Parry, PerfectParry, Dodge }
        public Transform arenaRoot;
        public DemoFighter player;
        public DemoFighter[] enemies = Array.Empty<DemoFighter>();
        public bool gameplayEnabled = true;
        public Shader effectShader;
        public RougeSkillPresentation presentation;
        public bool IsOpening { get; private set; }
        public Canvas GameplayCanvas => canvas;
        public MechaBattleHUD hudPrefab;
        public MechaBattleHUD Hud { get; private set; }
        public const int MaximumBurst=5;
        public DemoFighter CurrentActor { get; private set; }
        public DemoFighter Target => CurrentTarget();
        public bool LastTimingSucceeded { get; private set; }
        public float IncomingProgress => Mathf.Clamp01((float)((CombatTime-windupStart)/Math.Max(.001,IncomingImpactTime-windupStart)));
        public RougeArenaPlacement Placement => presentation!=null?presentation.placement:null;
        readonly RougeTurnOrder turnOrder=new RougeTurnOrder();
        public void PreviewTurns(List<DemoFighter> output,int count=5){turnOrder.Preview(output,count,CurrentActor);}
        public static int SkillCost(int skill)=>skill==1?1:skill==2?2:skill==3?5:0;
        public int PreviewBurstAfterTurn(int skill)=>Mathf.Min(MaximumBurst,Burst-SkillCost(skill)+(skill==0?1:0)+1);
        public float DodgeChance => CurrentActor!=null && player.speed>CurrentActor.speed?1:.5f;
        public float ParryDamageMultiplier => CurrentActor!=null && player.defense*1.25f>CurrentActor.attack?0:.5f;
        public static bool DodgeEvades(int ownSpeed,int attackerSpeed,float roll)=>ownSpeed>attackerSpeed || roll<.5f;
        public static float ParryMultiplier(int ownDef,int attackerAttack)=>ownDef*1.25f>attackerAttack?0:.5f;
        [Header("Prototype tuning")]
        public float enemyWindupSeconds = 1.8f;
        public int enemyDamage = 24;
        public float parryWindow = 0.20f;
        public float perfectParryWindow = 0.09f;
        public float dodgeWindow = 0.40f;
        public int beamDamage = 35;
        public int saberDamage = 65;
        public int barrageDamage = 43;
        public int ultimatePrimaryDamage = 120;
        public int ultimateSplashDamage = 60;

        public BattlePhase Phase { get; private set; } = BattlePhase.Suspended;
        [Obsolete("Use the five-cell Burst resource.")] public int Energy => Burst;
        public int Burst { get; private set; } = 2;
        public int Round { get; private set; } = 1;
        public int SelectedTarget { get; private set; }
        public int BarrageCooldown { get; private set; }
        public double IncomingImpactTime { get; private set; }
        public double IncomingSecondsRemaining => IncomingImpactTime - CombatTime;
        public bool DefenseConsumed { get; private set; }
        public DefenseResult LastDefense { get; private set; }
        public string LastMessage { get; private set; } = "Choose a target and a skill.";
        public int ActionsResolved { get; private set; }

        Canvas canvas;
        GameObject ownedEventSystem;
        readonly List<GameObject> effects = new List<GameObject>();
        readonly List<Material> materials = new List<Material>();
        readonly Dictionary<LineRenderer, float> lineWidths = new Dictionary<LineRenderer, float>();
        readonly List<RaycastResult> pointerUIHits = new List<RaycastResult>();
        bool targetPointerDown, targetPointerDragged, targetPointerIsTouch;
        int targetTouchId;
        Vector2 targetPointerStart;
        Material pinkMaterial, blueMaterial, goldMaterial, whiteMaterial;
        GameObject marker;
        LineRenderer targetRing;
        double windupStart;
        string attackerName = "";
        bool started;
        double pausedAt, pausedSeconds;
        BattlePhase resumePhase = BattlePhase.PlayerTurn;
        double CombatTime => Time.timeAsDouble - pausedSeconds - (gameplayEnabled ? 0 : Time.timeAsDouble - pausedAt);

        void Start()
        {
            if (arenaRoot == null) arenaRoot = transform;
            if (presentation == null) presentation = GetComponent<RougeSkillPresentation>();
            if (presentation != null) { presentation.battle = this; presentation.Initialize(); }
            foreach (var line in arenaRoot.GetComponentsInChildren<LineRenderer>(true)) lineWidths[line] = line.startWidth;
            pinkMaterial = NewMaterial(new Color(1f, .15f, .52f));
            blueMaterial = NewMaterial(new Color(.12f, .85f, 1f));
            goldMaterial = NewMaterial(new Color(1f, .68f, .14f));
            whiteMaterial = NewMaterial(new Color(.9f, 1f, 1f));
            BuildUI();
            BuildTargetMarker();
            started = true;
            if (!gameplayEnabled) pausedAt = Time.timeAsDouble;
            ResetState(false);
            if (presentation != null) { IsOpening = true; Phase = BattlePhase.PlayerAction; Run(OpeningSequence()); }
            else Run(AdvanceTurns());
        }

        IEnumerator OpeningSequence()
        {
            LastMessage = "STRIKE ROUGE OOTORI / DEPLOYING";
            yield return presentation.Arrival();
            IsOpening = false; yield return AdvanceTurns();
        }

        void Update()
        {
            if (!started) return;

            HandleTargetPointer();

            if (Keyboard.current != null && gameplayEnabled)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) { if(Hud!=null)Hud.SelectSkill(0);else TryUseSkill(0); }
                if (Keyboard.current.digit2Key.wasPressedThisFrame) { if(Hud!=null)Hud.SelectSkill(1);else TryUseSkill(1); }
                if (Keyboard.current.digit3Key.wasPressedThisFrame) { if(Hud!=null)Hud.SelectSkill(2);else TryUseSkill(2); }
                if (Keyboard.current.digit4Key.wasPressedThisFrame) { if(Hud!=null)Hud.SelectSkill(3);else TryUseSkill(3); }
                if (Keyboard.current.enterKey.wasPressedThisFrame && Hud!=null) Hud.ExecuteSelected();
                if (Keyboard.current.qKey.wasPressedThisFrame) TryDefense(true);
                if (Keyboard.current.eKey.wasPressedThisFrame) TryDefense(false);
                if (Keyboard.current.tabKey.wasPressedThisFrame) CycleTarget();
            }
            RefreshUI();
            if (targetRing != null)
            {
                DemoFighter target = CurrentTarget();
                marker.SetActive(target != null && gameplayEnabled);
                if (target != null) marker.transform.position = target.transform.position + arenaRoot.up * UnitScale * .018f;
                targetRing.transform.localRotation = Quaternion.Euler(0, Time.time * 28f, 0);
            }
        }

        void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
            if (canvas != null) Destroy(canvas.gameObject);
            if (ownedEventSystem != null) Destroy(ownedEventSystem);
            if (marker != null) Destroy(marker);
            ClearEffects();
        }

        void LateUpdate()
        {
            // LineRenderer widths are world units; positions follow the board transform.
            // Apply the same board scale to floor lines, target rings and transient FX.
            foreach (var pair in lineWidths)
                if (pair.Key != null) pair.Key.startWidth = pair.Key.endWidth = pair.Value * UnitScale;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (gameplayEnabled == enabled) return;
            gameplayEnabled = enabled;
            targetPointerDown = false;
            if (presentation != null) presentation.SetPaused(!enabled);
            if (!started) return;
            if (!enabled)
            {
                pausedAt = Time.timeAsDouble;
                resumePhase = Phase;
                Phase = BattlePhase.Suspended;
            }
            else
            {
                pausedSeconds += Time.timeAsDouble - pausedAt;
                Phase = resumePhase;
            }
        }

        public void ResetBattle(){ResetState(true);}
        void ResetState(bool startFlow)
        {
            if (!started) return;
            StopAllCoroutines();
            targetPointerDown = false;
            IsOpening = false;
            if (presentation != null) presentation.ResetPresentation();
            ClearEffects();
            Burst = 2; Round = 1; BarrageCooldown = 0; SelectedTarget = 0; CurrentActor=null;
            IncomingImpactTime = 0; DefenseConsumed = false; LastDefense = DefenseResult.None; ActionsResolved = 0;
            if (player == null || enemies == null || enemies.Length == 0)
            {
                Phase = BattlePhase.Suspended;
                LastMessage = "Assign the player and enemy fighters in the Inspector.";
                return;
            }
            player.ResetFighter();
            foreach (var enemy in enemies) if (enemy != null) enemy.ResetFighter();
            turnOrder.Reset(player,enemies);
            EnsureValidTarget();
            resumePhase = BattlePhase.PlayerTurn;
            Phase = gameplayEnabled ? BattlePhase.PlayerTurn : BattlePhase.Suspended;
            LastMessage = gameplayEnabled ? "Your turn. Choose a target, then a skill." : "Place the battlefield to begin.";
            if(Hud!=null)Hud.ClearSelection();
            if(startFlow)Run(AdvanceTurns());
            RefreshUI();
        }

        public void ChargeUltimateForTest()
        {
            if (Phase != BattlePhase.PlayerTurn || !gameplayEnabled) return;
            Burst = MaximumBurst;
            LastMessage = "TEST: Burst 5/5. Select Full Burst, then EXECUTE.";
        }

        public void SelectTarget(int index)
        {
            if (!started || !gameplayEnabled || Phase != BattlePhase.PlayerTurn || enemies == null || index < 0 || index >= enemies.Length || enemies[index] == null || !enemies[index].IsAlive) return;
            SelectedTarget = index;
            LastMessage = "TARGET / " + enemies[index].displayName + " / choose skill, then EXECUTE";
            if (Hud != null) Hud.Refresh(true);
        }

        public Camera TargetingCamera => Placement != null && Placement.IsARMode ? Placement.arCamera :
            presentation != null && presentation.virtualCamera != null ? presentation.virtualCamera : Camera.main;

        public bool TrySelectTargetAtScreenPoint(Vector2 position)
        {
            if (!started || !gameplayEnabled || Phase != BattlePhase.PlayerTurn || TargetingCamera == null ||
                (Hud != null && Hud.WorldSelectionBlocked) || IsPointerOverUI(position)) return false;
            var camera = TargetingCamera;
            if (!camera.pixelRect.Contains(position)) return false;
            Ray ray = camera.ScreenPointToRay(position);
            int selected = -1; float nearest = float.PositiveInfinity;
            for (int i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy) continue;
                // Body-sized pick volumes live in actor space, not in imported meshes.
                // This remains correct when the AR board is moved, rotated or scaled.
                float height = Mathf.Max(.1f, enemy.height);
                var body = new Bounds(Vector3.up * height * .55f, new Vector3(height * .8f, height * 1.15f, height * .8f));
                var localRay = new Ray(enemy.transform.InverseTransformPoint(ray.origin), enemy.transform.InverseTransformVector(ray.direction));
                if (!body.IntersectRay(localRay, out float distance)) continue;
                float worldDistance = Vector3.Distance(ray.origin, enemy.transform.TransformPoint(localRay.GetPoint(distance)));
                if (worldDistance >= nearest) continue;
                nearest = worldDistance; selected = i;
            }
            if (selected < 0) return false;
            SelectTarget(selected); return true;
        }

        bool IsPointerOverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            pointerUIHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, pointerUIHits);
            foreach (var hit in pointerUIHits) if (hit.module is GraphicRaycaster) return true;
            return false;
        }

        void HandleTargetPointer()
        {
            if (!gameplayEnabled || Phase != BattlePhase.PlayerTurn || (Hud != null && Hud.WorldSelectionBlocked))
            { targetPointerDown = false; return; }
            var screen = Touchscreen.current;
            if (screen == null && targetPointerIsTouch) targetPointerDown = false;
            int activeTouches = 0;
            if (screen != null) foreach (var touch in screen.touches) if (touch.press.isPressed) activeTouches++;
            if (activeTouches > 0 || targetPointerDown && targetPointerIsTouch)
            {
                var touch = screen.primaryTouch;
                if (activeTouches > 1) { targetPointerDown = false; return; }
                if (touch.press.wasPressedThisFrame && activeTouches == 1)
                    BeginTargetPointer(touch.position.ReadValue(), true, touch.touchId.ReadValue());
                if (!targetPointerDown || !targetPointerIsTouch) return;
                if (touch.touchId.ReadValue() != targetTouchId) { targetPointerDown = false; return; }
                if (Vector2.Distance(targetPointerStart, touch.position.ReadValue()) > 14) targetPointerDragged = true;
                if (touch.press.wasReleasedThisFrame) EndTargetPointer(touch.position.ReadValue());
                return;
            }
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame && !mouse.rightButton.isPressed)
                BeginTargetPointer(mouse.position.ReadValue(), false, 0);
            if (!targetPointerDown) return;
            if (mouse.rightButton.isPressed || Vector2.Distance(targetPointerStart, mouse.position.ReadValue()) > 14) targetPointerDragged = true;
            if (mouse.leftButton.wasReleasedThisFrame) EndTargetPointer(mouse.position.ReadValue());
        }
        void BeginTargetPointer(Vector2 position, bool touch, int id)
        {
            targetPointerDown = !IsPointerOverUI(position);
            targetPointerDragged = false; targetPointerStart = position;
            targetPointerIsTouch = touch; targetTouchId = id;
        }
        void EndTargetPointer(Vector2 position)
        {
            bool select = targetPointerDown && !targetPointerDragged;
            targetPointerDown = false;
            if (select) TrySelectTargetAtScreenPoint(position);
        }

        public void CycleTarget()
        {
            for (int step = 1; step <= enemies.Length; step++)
            {
                int index = (SelectedTarget + step) % enemies.Length;
                if (enemies[index] != null && enemies[index].IsAlive) { SelectTarget(index); return; }
            }
        }

        public bool CanUseSkill(int skill)
        {
            if (!started || !gameplayEnabled || Phase != BattlePhase.PlayerTurn || player == null || !player.IsAlive || CurrentTarget() == null) return false;
            return skill>=0 && skill<4 && Burst>=SkillCost(skill);
        }

        public bool TryUseSkill(int skill)
        {
            if (!CanUseSkill(skill)) return false;
            Phase = BattlePhase.PlayerAction;
            Burst-=SkillCost(skill);
            Run(PlayerSequence(skill, CurrentTarget()));
            return true;
        }

        public static DefenseResult EvaluateDefense(double pressTime, double impactTime, bool parry, float parrySeconds = .20f, float perfectSeconds = .09f, float dodgeSeconds = .40f)
        {
            double remaining = impactTime - pressTime;
            if (remaining < 0) return DefenseResult.Failed;
            if (!parry) return remaining <= dodgeSeconds ? DefenseResult.Dodge : DefenseResult.Failed;
            return remaining <= parrySeconds ? DefenseResult.Parry : DefenseResult.Failed;
        }

        public bool TryDefense(bool parry)
        {
            if (!gameplayEnabled || Phase != BattlePhase.EnemyWindup || DefenseConsumed) return false;
            // Sample once, at input time. A held key or repeated taps cannot retry this attack.
            DefenseConsumed = true;
            LastDefense = EvaluateDefense(CombatTime, IncomingImpactTime, parry, parryWindow, perfectParryWindow, dodgeWindow);
            LastTimingSucceeded=LastDefense!=DefenseResult.Failed;
            if(LastTimingSucceeded && !parry && !DodgeEvades(player.speed,CurrentActor.speed,UnityEngine.Random.value))LastDefense=DefenseResult.Failed;
            LastMessage = !LastTimingSucceeded?"TIMING MISSED / reaction spent":parry?"PARRY TIMED / brace for impact":LastDefense==DefenseResult.Dodge?"DODGE TIMED / evasion successful":"DODGE TIMED / evasion roll failed";
            return true;
        }

        public string DebugSnapshot()
        {
            string result = "phase=" + Phase + "; round=" + Round + "; hp=" + (player == null ? -1 : player.Health) + "; armor="+(player==null?-1:player.Armor)+"; burst=" + Burst + "; target=" + SelectedTarget + "; reaction=" + LastDefense + "; reactionUsed=" + DefenseConsumed + "; actions=" + ActionsResolved;
            for (int i = 0; i < enemies.Length; i++) result += "; enemy" + i + "=" + (enemies[i] == null ? -1 : enemies[i].Health);
            return result + "; message=" + LastMessage;
        }

        IEnumerator PlayerSequence(int skill, DemoFighter target)
        {
            LastMessage=new[]{"BEAM RIFLE BURST","ANTI-SHIP BLADE","OOTORI MISSILES","FULL BURST / 5 CELLS"}[skill];
            if(presentation!=null)
                yield return presentation.Perform(skill,target,enemies,(victim,amount)=>ResolvePlayerHit(victim,amount,skill,target));
            else
            {
                yield return Aim(player,.3f);
                yield return Beam(player.MuzzlePoint,target.AimPoint,pinkMaterial,.06f,.2f);
                ResolvePlayerHit(target,new[]{beamDamage,saberDamage,barrageDamage,ultimatePrimaryDamage}[skill],skill,target);
            }
            Burst=Mathf.Min(MaximumBurst,Burst+(skill==0?1:0)+1);
            ActionsResolved++;Round++;
            yield return WaitActive(.45f);
            if(AllEnemiesDead()){Phase=BattlePhase.Victory;LastMessage="VICTORY / all targets neutralized";yield break;}
            EnsureValidTarget();yield return AdvanceTurns();
        }

        void ResolvePlayerHit(DemoFighter victim,int amount,int skill,DemoFighter primary)
        {
            if(victim==null || !victim.IsAlive)return;
            int total=skill==0?beamDamage:skill==1?saberDamage:skill==2?barrageDamage:victim==primary?ultimatePrimaryDamage:ultimateSplashDamage;
            float armorDamage=(player.attack*.5f+player.breakDamage*.5f)*amount/Mathf.Max(1,total);
            victim.ReceiveCombatHit(amount,armorDamage);
        }
        IEnumerator AdvanceTurns()
        {
            while(player.IsAlive && !AllEnemiesDead())
            {
                CurrentActor=turnOrder.TakeNext();
                if(CurrentActor==null)yield break;
                if(CurrentActor==player)
                {Phase=BattlePhase.PlayerTurn;LastMessage="YOUR TURN / tap any enemy, choose a skill, then EXECUTE";yield break;}
                yield return EnemyAttack(CurrentActor);
            }
            Phase=player.IsAlive?BattlePhase.Victory:BattlePhase.Defeat;
            LastMessage=player.IsAlive?"VICTORY / all targets neutralized":"DEFEAT / reset to retry";
        }
        IEnumerator EnemyAttack(DemoFighter enemy)
        {
            attackerName=enemy.displayName;DefenseConsumed=false;LastDefense=DefenseResult.None;LastTimingSucceeded=false;
            Phase=BattlePhase.EnemyWindup;windupStart=CombatTime;
            IncomingImpactTime=windupStart+Mathf.Max(.7f,enemyWindupSeconds);
            LastMessage=attackerName+" / prepare DODGE or PARRY";
            enemy.Pose(Vector3.zero,new Vector3(-5,0,0),-35,-8);
            while(CombatTime<IncomingImpactTime)yield return null;
            Phase=BattlePhase.EnemyAction;
            if(presentation!=null)presentation.React(LastDefense);
            yield return Beam(enemy.MuzzlePoint,player.AimPoint,goldMaterial,.06f,.13f);
            float multiplier=LastDefense==DefenseResult.Dodge?0:LastDefense==DefenseResult.Parry?ParryDamageMultiplier:1;
            int dealt=player.ReceiveCombatHit(enemyDamage,enemy.attack*.5f+enemy.breakDamage*.5f,multiplier);
            if(LastDefense==DefenseResult.Dodge)LastMessage="DODGE / attack avoided";
            else if(LastDefense==DefenseResult.Parry)LastMessage="PARRY / "+dealt+" HP lost";
            else LastMessage=(LastTimingSucceeded?"TIMED / DODGE FAILED":"HIT")+" / "+dealt+" HP lost";
            Run(Impact(player.AimPoint,multiplier==0?blueMaterial:goldMaterial,.4f,.3f));
            if(enemy.IsAlive)enemy.RestorePose();
            yield return WaitActive(.65f);
            if(presentation!=null)presentation.EndDefense();else player.RestorePose();
        }

        IEnumerator Aim(DemoFighter fighter, float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                fighter.Pose(Vector3.zero, new Vector3(-3 * t, 0, 0), -55 * t, -12 * t);
                yield return null;
            }
        }

        IEnumerator SaberAttack(DemoFighter target, int damage)
        {
            // Store the path relative to the arena, so relocating the AR board mid-action
            // cannot send the visual back toward its previous position in the room.
            Vector3 home = arenaRoot.InverseTransformPoint(player.RestVisualWorldPosition);
            Vector3 direction = (arenaRoot.InverseTransformPoint(target.transform.position) - arenaRoot.InverseTransformPoint(player.transform.position)).normalized;
            Vector3 destination = arenaRoot.InverseTransformPoint(target.transform.position) - direction * .75f;
            destination.y = home.y;
            float elapsed = 0;
            while (elapsed < .42f)
            {
                elapsed += Time.deltaTime;
                player.Pose(Vector3.zero, new Vector3(-12, 0, 0), -80, -20);
                player.MoveVisualToWorld(arenaRoot.TransformPoint(Vector3.Lerp(home, destination, Mathf.SmoothStep(0, 1, elapsed / .42f))));
                yield return null;
            }
            for (int hit = 0; hit < 2; hit++)
            {
                Vector3 center = target.AimPoint;
                yield return Slash(center, hit == 0 ? 1 : -1);
                if (target.IsAlive) Damage(target, hit == 0 ? damage / 2 : damage - damage / 2, pinkMaterial);
                player.Pose(Vector3.zero, new Vector3(0, hit == 0 ? 27 : -27, 0), hit == 0 ? 20 : -90, hit == 0 ? 30 : -30);
                player.MoveVisualToWorld(arenaRoot.TransformPoint(destination));
                yield return WaitActive(.12f);
            }
            elapsed = 0;
            while (elapsed < .35f)
            {
                elapsed += Time.deltaTime;
                player.MoveVisualToWorld(arenaRoot.TransformPoint(Vector3.Lerp(destination, home, Mathf.SmoothStep(0, 1, elapsed / .35f))));
                yield return null;
            }
            player.RestorePose();
        }

        void Damage(DemoFighter fighter, int amount, Material material)
        {
            if (fighter == null || !fighter.IsAlive) return;
            fighter.TakeDamage(amount);
            Run(Impact(fighter.AimPoint, material, .40f, .32f));
        }

        DemoFighter CurrentTarget() => enemies != null && SelectedTarget >= 0 && SelectedTarget < enemies.Length && enemies[SelectedTarget] != null && enemies[SelectedTarget].IsAlive ? enemies[SelectedTarget] : null;
        void EnsureValidTarget()
        {
            if (CurrentTarget() != null) return;
            for (int i = 0; i < enemies.Length; i++) if (enemies[i] != null && enemies[i].IsAlive) { SelectedTarget = i; return; }
        }
        bool AllEnemiesDead() { foreach (var enemy in enemies) if (enemy != null && enemy.IsAlive) return false; return true; }
        float UnitScale => Mathf.Abs(arenaRoot.lossyScale.x);

        Material NewMaterial(Color color)
        {
            Shader shader = effectShader != null ? effectShader : Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = "RougeDemo Runtime FX" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            materials.Add(material);
            return material;
        }

        GameObject Effect(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(arenaRoot, false);
            effects.Add(go);
            return go;
        }
        void Release(GameObject go) { effects.Remove(go); if (go != null) { lineWidths.Remove(go.GetComponent<LineRenderer>()); Destroy(go); } }
        void ClearEffects() { foreach (var go in effects) if (go != null) { lineWidths.Remove(go.GetComponent<LineRenderer>()); Destroy(go); } effects.Clear(); }

        LineRenderer Line(GameObject go, Material material, float width, int count)
        {
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material; line.useWorldSpace = false; line.positionCount = count;
            line.startWidth = line.endWidth = width; line.numCapVertices = 4; line.numCornerVertices = 3;
            lineWidths[line] = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }
        IEnumerator Beam(Vector3 from, Vector3 to, Material material, float width, float seconds)
        {
            var go = Effect("Beam FX");
            var line = Line(go, material, width, 2);
            line.SetPosition(0, arenaRoot.InverseTransformPoint(from)); line.SetPosition(1, arenaRoot.InverseTransformPoint(to));
            float elapsed = 0;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                lineWidths[line] = width * Mathf.Lerp(1.4f, .05f, elapsed / seconds);
                yield return null;
            }
            Release(go);
        }
        IEnumerator Slash(Vector3 center, int side)
        {
            var go = Effect("Saber arc FX");
            var line = Line(go, pinkMaterial, .075f, 20);
            Vector3 c = arenaRoot.InverseTransformPoint(center);
            for (int i = 0; i < 20; i++)
            {
                float a = Mathf.Lerp(-1.15f, 1.15f, i / 19f);
                line.SetPosition(i, c + new Vector3(Mathf.Sin(a) * .85f, Mathf.Sin(a) * .55f * side, -.25f + Mathf.Cos(a) * .20f));
            }
            yield return WaitActive(.16f);
            Release(go);
        }
        IEnumerator Projectile(Vector3 from, Vector3 to, Material material, float seconds, int seed)
        {
            var go = Effect("Barrage projectile FX");
            var line = Line(go, material, .055f, 2);
            Vector3 a = arenaRoot.InverseTransformPoint(from), b = arenaRoot.InverseTransformPoint(to);
            float elapsed = 0;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                Vector3 p = Vector3.Lerp(a, b, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * (.65f + seed * .15f);
                line.SetPosition(0, p); line.SetPosition(1, p - (b - a).normalized * .22f);
                yield return null;
            }
            Run(Impact(to, material, .23f, .22f));
            Release(go);
        }
        IEnumerator Impact(Vector3 center, Material material, float size, float seconds)
        {
            var go = Effect("Impact ring FX");
            go.transform.position = center;
            var line = Line(go, material, .04f, 25); line.loop = true;
            for (int i = 0; i < 25; i++) { float a = i * Mathf.PI * 2 / 25; line.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0)); }
            float elapsed = 0;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                go.transform.localScale = Vector3.one * Mathf.Lerp(.02f, size, t);
                lineWidths[line] = Mathf.Lerp(.09f, .005f, t);
                yield return null;
            }
            Release(go);
        }
        void BuildTargetMarker()
        {
            marker = new GameObject("Selected target ring"); marker.transform.SetParent(arenaRoot, false);
            targetRing = Line(marker, pinkMaterial, .025f, 48); targetRing.loop = true;
            for (int i = 0; i < 48; i++) { float a = i * Mathf.PI * 2 / 48; targetRing.SetPosition(i, new Vector3(Mathf.Cos(a) * .60f, 0, Mathf.Sin(a) * .60f)); }
        }

        void BuildUI()
        {
            if(FindAnyObjectByType<EventSystem>()==null)
            {
                ownedEventSystem=new GameObject("Rouge EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            Hud=hudPrefab!=null?Instantiate(hudPrefab):MechaBattleHUD.CreateFallback();
            canvas=Hud.GetComponent<Canvas>();Hud.Bind(this);
        }
        void RefreshUI(){if(Hud!=null)Hud.Refresh();}
        void Run(IEnumerator sequence) { StartCoroutine(Pausable(sequence)); }
        IEnumerator Pausable(IEnumerator sequence)
        {
            // Flatten nested routines so tracking loss pauses every animation and delay,
            // without affecting other systems or discarding the battle's current action.
            var stack = new Stack<IEnumerator>(); stack.Push(sequence);
            while (stack.Count > 0)
            {
                if (!gameplayEnabled) { yield return null; continue; }
                var current = stack.Peek();
                if (!current.MoveNext()) { stack.Pop(); continue; }
                if (current.Current is IEnumerator nested) stack.Push(nested);
                else yield return current.Current;
            }
        }
        IEnumerator WaitActive(float seconds)
        {
            double deadline = CombatTime + seconds;
            while (CombatTime < deadline) yield return null;
        }
    }
}


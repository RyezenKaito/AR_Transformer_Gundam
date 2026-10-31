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
        public int Energy { get; private set; } = 100;
        public int Burst { get; private set; }
        public int Round { get; private set; } = 1;
        public int SelectedTarget { get; private set; }
        public int BarrageCooldown { get; private set; }
        public double IncomingImpactTime { get; private set; }
        public double IncomingSecondsRemaining => IncomingImpactTime - CombatTime;
        public bool DefenseConsumed { get; private set; }
        public DefenseResult LastDefense { get; private set; }
        public string LastMessage { get; private set; } = "Choose a target and a skill.";
        public int ActionsResolved { get; private set; }

        RectTransform safeRoot;
        Canvas canvas;
        GameObject ownedEventSystem;
        Text headerText, resourcesText, statusText, reactionText;
        Image healthFill, energyFill, burstFill, telegraphFill;
        Button[] skillButtons, targetButtons;
        Text[] skillLabels, targetLabels;
        Button parryButton, dodgeButton, chargeButton;
        GameObject defensePanel;
        Font font;
        readonly List<GameObject> effects = new List<GameObject>();
        readonly List<Material> materials = new List<Material>();
        readonly Dictionary<LineRenderer, float> lineWidths = new Dictionary<LineRenderer, float>();
        Material pinkMaterial, blueMaterial, goldMaterial, whiteMaterial;
        GameObject marker;
        LineRenderer targetRing;
        double windupStart;
        string attackerName = "";
        Rect lastSafeArea;
        Vector2Int lastScreen;
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
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pinkMaterial = NewMaterial(new Color(1f, .15f, .52f));
            blueMaterial = NewMaterial(new Color(.12f, .85f, 1f));
            goldMaterial = NewMaterial(new Color(1f, .68f, .14f));
            whiteMaterial = NewMaterial(new Color(.9f, 1f, 1f));
            BuildUI();
            BuildTargetMarker();
            started = true;
            if (!gameplayEnabled) pausedAt = Time.timeAsDouble;
            ResetBattle();
            if (presentation != null) { IsOpening = true; Phase = BattlePhase.PlayerAction; Run(OpeningSequence()); }
        }

        IEnumerator OpeningSequence()
        {
            LastMessage = "STRIKE ROUGE OOTORI / DEPLOYING";
            yield return presentation.Arrival();
            IsOpening = false; Phase = BattlePhase.PlayerTurn;
            LastMessage = "Your turn. Choose a target, then a skill.";
        }

        void Update()
        {
            if (!started) return;
            UpdateSafeArea();
            if (Keyboard.current != null && gameplayEnabled)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) TryUseSkill(0);
                if (Keyboard.current.digit2Key.wasPressedThisFrame) TryUseSkill(1);
                if (Keyboard.current.digit3Key.wasPressedThisFrame) TryUseSkill(2);
                if (Keyboard.current.digit4Key.wasPressedThisFrame) TryUseSkill(3);
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

        public void ResetBattle()
        {
            if (!started) return;
            StopAllCoroutines();
            IsOpening = false;
            if (presentation != null) presentation.ResetPresentation();
            ClearEffects();
            Energy = 100; Burst = 0; Round = 1; BarrageCooldown = 0; SelectedTarget = 0;
            IncomingImpactTime = 0; DefenseConsumed = false; LastDefense = DefenseResult.None; ActionsResolved = 0;
            if (player == null || enemies == null || enemies.Length == 0)
            {
                Phase = BattlePhase.Suspended;
                LastMessage = "Assign the player and enemy fighters in the Inspector.";
                return;
            }
            player.ResetFighter();
            foreach (var enemy in enemies) if (enemy != null) enemy.ResetFighter();
            EnsureValidTarget();
            resumePhase = BattlePhase.PlayerTurn;
            Phase = gameplayEnabled ? BattlePhase.PlayerTurn : BattlePhase.Suspended;
            LastMessage = gameplayEnabled ? "Your turn. Choose a target, then a skill." : "Place the battlefield to begin.";
            RefreshUI();
        }

        public void ChargeUltimateForTest()
        {
            if (Phase != BattlePhase.PlayerTurn || !gameplayEnabled) return;
            Burst = 100;
            LastMessage = "TEST: Burst filled. Use Rouge Full Assault.";
        }

        public void SelectTarget(int index)
        {
            if (Phase != BattlePhase.PlayerTurn || index < 0 || index >= enemies.Length || enemies[index] == null || !enemies[index].IsAlive) return;
            SelectedTarget = index;
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
            return skill == 0 || (skill == 1 && Energy >= 25) || (skill == 2 && Energy >= 40 && BarrageCooldown == 0) || (skill == 3 && Burst >= 100);
        }

        public bool TryUseSkill(int skill)
        {
            if (!CanUseSkill(skill)) return false;
            Phase = BattlePhase.PlayerAction;
            if (skill == 1) Energy -= 25;
            if (skill == 2) { Energy -= 40; BarrageCooldown = 3; }
            if (skill == 3) Burst = 0;
            Run(PlayerSequence(skill, CurrentTarget()));
            return true;
        }

        public static DefenseResult EvaluateDefense(double pressTime, double impactTime, bool parry, float parrySeconds = .20f, float perfectSeconds = .09f, float dodgeSeconds = .40f)
        {
            double remaining = impactTime - pressTime;
            if (remaining < 0) return DefenseResult.Failed;
            if (!parry) return remaining <= dodgeSeconds ? DefenseResult.Dodge : DefenseResult.Failed;
            if (remaining <= perfectSeconds) return DefenseResult.PerfectParry;
            return remaining <= parrySeconds ? DefenseResult.Parry : DefenseResult.Failed;
        }

        public bool TryDefense(bool parry)
        {
            if (!gameplayEnabled || Phase != BattlePhase.EnemyWindup || DefenseConsumed) return false;
            // Sample once, at input time. A held key or repeated taps cannot retry this attack.
            DefenseConsumed = true;
            LastDefense = EvaluateDefense(CombatTime, IncomingImpactTime, parry, parryWindow, perfectParryWindow, dodgeWindow);
            LastMessage = LastDefense == DefenseResult.Failed ? "Too early or too late. Reaction spent." : parry ? "Guard timed. Brace for impact." : "Dodge timed. Incoming attack avoided.";
            return true;
        }

        public string DebugSnapshot()
        {
            string result = "phase=" + Phase + "; round=" + Round + "; hp=" + (player == null ? -1 : player.Health) + "; energy=" + Energy + "; burst=" + Burst + "; barrageCooldown=" + BarrageCooldown + "; target=" + SelectedTarget + "; reaction=" + LastDefense + "; reactionUsed=" + DefenseConsumed + "; actions=" + ActionsResolved;
            for (int i = 0; i < enemies.Length; i++) result += "; enemy" + i + "=" + (enemies[i] == null ? -1 : enemies[i].Health);
            return result + "; message=" + LastMessage;
        }

        IEnumerator PlayerSequence(int skill, DemoFighter target)
        {
            if (presentation != null)
            {
                LastMessage = new[] { "BEAM RIFLE BURST", "ANTI-SHIP BLADE ASSAULT", "OOTORI MISSILE BARRAGE", "CORE 100% / FULL ARMAMENT BURST" }[skill];
                yield return presentation.Perform(skill, target, enemies, (victim, amount) => victim.TakeDamage(amount));
                if (skill == 0) { Energy = Mathf.Min(100, Energy + 22); Burst = Mathf.Min(100, Burst + 18); }
                else if (skill == 1) Burst = Mathf.Min(100, Burst + 24);
                else if (skill == 2) Burst = Mathf.Min(100, Burst + 30);
                ActionsResolved++;
                yield return WaitActive(.45f);
                if (AllEnemiesDead()) { Phase = BattlePhase.Victory; LastMessage = "VICTORY! All targets defeated. Reset to try again."; yield break; }
                EnsureValidTarget(); yield return EnemySequence(); yield break;
            }
            switch (skill)
            {
                case 0:
                    LastMessage = "BEAM SHOT";
                    yield return Aim(player, .34f);
                    yield return Beam(player.MuzzlePoint, target.AimPoint, pinkMaterial, .07f, .2f);
                    Damage(target, beamDamage, pinkMaterial);
                    Energy = Mathf.Min(100, Energy + 22);
                    Burst = Mathf.Min(100, Burst + 18);
                    break;
                case 1:
                    LastMessage = "SABER ASSAULT";
                    yield return SaberAttack(target, saberDamage);
                    Burst = Mathf.Min(100, Burst + 24);
                    break;
                case 2:
                    LastMessage = "OOTORI BARRAGE";
                    yield return Aim(player, .4f);
                    for (int volley = 0; volley < 3; volley++)
                    {
                        foreach (var enemy in enemies)
                        {
                            if (enemy == null || !enemy.IsAlive) continue;
                            Vector3 origin = player.transform.TransformPoint(new Vector3(volley % 2 == 0 ? -.5f : .5f, player.height * .8f, -.15f));
                            Run(Projectile(origin, enemy.AimPoint, goldMaterial, .40f, volley));
                        }
                        yield return WaitActive(.19f);
                    }
                    yield return WaitActive(.43f);
                    foreach (var enemy in enemies) if (enemy != null && enemy.IsAlive) Damage(enemy, barrageDamage, goldMaterial);
                    Burst = Mathf.Min(100, Burst + 30);
                    break;
                case 3:
                    LastMessage = "ROUGE FULL ASSAULT   /   CORE 100%";
                    for (int i = 0; i < 3; i++) { Run(Impact(player.AimPoint, pinkMaterial, .65f + i * .18f, .45f)); yield return WaitActive(.16f); }
                    yield return SaberAttack(target, ultimatePrimaryDamage / 2);
                    yield return Aim(player, .28f);
                    yield return Beam(player.MuzzlePoint, target.AimPoint, pinkMaterial, .20f, .55f);
                    if (target.IsAlive) Damage(target, ultimatePrimaryDamage - ultimatePrimaryDamage / 2, pinkMaterial);
                    foreach (var enemy in enemies) if (enemy != null && enemy != target && enemy.IsAlive)
                    {
                        Run(Beam(player.MuzzlePoint, enemy.AimPoint, blueMaterial, .10f, .3f));
                        Damage(enemy, ultimateSplashDamage, blueMaterial);
                    }
                    break;
            }
            player.RestorePose();
            ActionsResolved++;
            yield return WaitActive(.6f);
            if (AllEnemiesDead()) { Phase = BattlePhase.Victory; LastMessage = "VICTORY! All targets defeated. Reset to try again."; yield break; }
            EnsureValidTarget();
            yield return EnemySequence();
        }

        IEnumerator EnemySequence()
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                DemoFighter enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                attackerName = enemy.displayName;
                DefenseConsumed = false; LastDefense = DefenseResult.None;
                Phase = BattlePhase.EnemyWindup;
                windupStart = CombatTime;
                IncomingImpactTime = windupStart + Mathf.Max(.7f, enemyWindupSeconds);
                LastMessage = attackerName + " is aiming. React near the end of the bar!";
                enemy.Pose(Vector3.zero, new Vector3(-5, 0, 0), -35, -8);
                while (CombatTime < IncomingImpactTime) yield return null;
                Phase = BattlePhase.EnemyAction;
                Vector3 targetPoint = player.AimPoint;
                if (presentation != null) presentation.React(LastDefense);
                if (LastDefense == DefenseResult.Dodge)
                {
                    if (presentation == null) player.Pose(new Vector3(.65f, 0, 0), new Vector3(0, 0, -18), -20);
                    LastMessage = "DODGE! No damage.";
                }
                else if (presentation == null && (LastDefense == DefenseResult.Parry || LastDefense == DefenseResult.PerfectParry))
                    player.Pose(Vector3.zero, new Vector3(0, -12, 0), -65);
                yield return Beam(enemy.MuzzlePoint, targetPoint, goldMaterial, .06f, .13f);
                switch (LastDefense)
                {
                    case DefenseResult.PerfectParry:
                        Burst = Mathf.Min(100, Burst + 22);
                        Run(Impact(player.AimPoint, whiteMaterial, .65f, .4f));
                        Damage(enemy, 18, blueMaterial);
                        LastMessage = "PERFECT PARRY! Counter 18 / Burst +22";
                        break;
                    case DefenseResult.Parry:
                        player.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(enemyDamage * .15f)));
                        Burst = Mathf.Min(100, Burst + 14);
                        Run(Impact(player.AimPoint, blueMaterial, .45f, .3f));
                        LastMessage = "PARRY! 85% damage blocked / Burst +14";
                        break;
                    case DefenseResult.Dodge: break;
                    default:
                        Damage(player, enemyDamage, goldMaterial);
                        Burst = Mathf.Min(100, Burst + 8);
                        LastMessage = "HIT! -" + enemyDamage + " HP / Burst +8";
                        break;
                }
                if (enemy.IsAlive) enemy.RestorePose();
                yield return WaitActive(.65f);
                if (!player.IsAlive) { Phase = BattlePhase.Defeat; LastMessage = "DEFEAT. Reset and try timed defense."; yield break; }
                if (presentation != null) presentation.EndDefense();
                else player.RestorePose();
                if (AllEnemiesDead()) { Phase = BattlePhase.Victory; LastMessage = "VICTORY! Counterattack defeated the last target."; yield break; }
            }
            Energy = Mathf.Min(100, Energy + 10);
            if (BarrageCooldown > 0) BarrageCooldown--;
            Round++;
            EnsureValidTarget();
            Phase = BattlePhase.PlayerTurn;
            LastMessage = "Your turn. Energy +10. Select the next skill.";
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
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                ownedEventSystem = new GameObject("Rouge Demo EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var root = new GameObject("Strike Rouge Battle HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            safeRoot = Rect("Safe area", root.transform, Vector2.zero, Vector2.one);
            RectTransform header = Panel("Header", safeRoot, new Vector2(.02f, .84f), new Vector2(.98f, .98f), new Color(.025f, .045f, .09f, .94f));
            headerText = Label(header, "STRIKE ROUGE   /   COMBAT LAB", new Vector2(.02f, .60f), new Vector2(.70f, .97f), 24, TextAnchor.MiddleLeft, Color.white);
            resourcesText = Label(header, "", new Vector2(.02f, .31f), new Vector2(.70f, .63f), 18, TextAnchor.MiddleLeft, new Color(.80f, .87f, .94f));
            healthFill = Bar(header, "Health", new Vector2(.02f, .12f), new Vector2(.22f, .23f), new Color(.30f, .95f, .65f));
            energyFill = Bar(header, "Energy", new Vector2(.25f, .12f), new Vector2(.45f, .23f), new Color(.15f, .75f, 1f));
            burstFill = Bar(header, "Burst", new Vector2(.48f, .12f), new Vector2(.68f, .23f), new Color(1f, .25f, .60f));
            Button reset = ButtonUI(header, "RESET", new Vector2(.86f, .52f), new Vector2(.98f, .92f), new Color(.19f, .25f, .34f), out _); reset.onClick.AddListener(ResetBattle);
            chargeButton = ButtonUI(header, "TEST Charge Ult", new Vector2(.72f, .08f), new Vector2(.98f, .44f), new Color(.35f, .15f, .31f), out _); chargeButton.onClick.AddListener(ChargeUltimateForTest);
            RectTransform targets = Rect("Target cards", safeRoot, new Vector2(.47f, .70f), new Vector2(.98f, .82f));
            targetButtons = new Button[enemies.Length]; targetLabels = new Text[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                int index = i; float left = i / (float)enemies.Length, right = (i + 1f) / enemies.Length - .012f;
                targetButtons[i] = ButtonUI(targets, "", new Vector2(left, 0), new Vector2(right, 1), new Color(.10f, .15f, .23f, .94f), out targetLabels[i]);
                targetLabels[i].fontSize = 18;
                targetButtons[i].onClick.AddListener(() => SelectTarget(index));
            }
            RectTransform footer = Panel("Action panel", safeRoot, new Vector2(.02f, .025f), new Vector2(.98f, .215f), new Color(.025f, .045f, .09f, .94f));
            statusText = Label(footer, "", new Vector2(.02f, .73f), new Vector2(.98f, .98f), 19, TextAnchor.MiddleLeft, Color.white);
            skillButtons = new Button[4]; skillLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                int skill = i;
                skillButtons[i] = ButtonUI(footer, "", new Vector2(.015f + i * .247f, .08f), new Vector2(.248f + i * .247f, .68f), i == 3 ? new Color(.63f, .12f, .36f) : new Color(.12f, .23f, .34f), out skillLabels[i]);
                skillLabels[i].fontSize = 21; skillButtons[i].onClick.AddListener(() => TryUseSkill(skill));
            }
            RectTransform defense = Panel("Timed defense", safeRoot, new Vector2(.28f, .25f), new Vector2(.72f, .43f), new Color(.035f, .055f, .10f, .96f));
            defensePanel = defense.gameObject;
            reactionText = Label(defense, "", new Vector2(.03f, .72f), new Vector2(.97f, .97f), 19, TextAnchor.MiddleCenter, Color.white);
            telegraphFill = Bar(defense, "Incoming", new Vector2(.04f, .57f), new Vector2(.96f, .67f), new Color(1f, .65f, .14f));
            parryButton = ButtonUI(defense, "Q   PARRY", new Vector2(.04f, .06f), new Vector2(.48f, .47f), new Color(.12f, .40f, .56f), out _);
            dodgeButton = ButtonUI(defense, "E   DODGE", new Vector2(.52f, .06f), new Vector2(.96f, .47f), new Color(.17f, .41f, .31f), out _);
            parryButton.onClick.AddListener(() => TryDefense(true)); dodgeButton.onClick.AddListener(() => TryDefense(false));
            Label(safeRoot, Application.isMobilePlatform ? "VIEW: drag to orbit / pinch to zoom" : "VIEW: RMB drag / wheel zoom / F reset", new Vector2(.025f,.655f), new Vector2(.46f,.69f), 14, TextAnchor.MiddleLeft, new Color(.65f,.75f,.85f));
            UpdateSafeArea();
        }

        void RefreshUI()
        {
            if (headerText == null) return;
            headerText.text = "STRIKE ROUGE   /   ROUND " + Round + "   /   " + PhaseLabel();
            resourcesText.text = "HP  " + (player == null ? "--" : player.Health + " / " + player.maxHealth) + "      ENERGY  " + Energy + " / 100      BURST  " + Burst + "%";
            SetFill(healthFill, player == null || player.maxHealth <= 0 ? 0 : player.Health / (float)player.maxHealth);
            SetFill(energyFill, Energy / 100f); SetFill(burstFill, Burst / 100f);
            statusText.text = LastMessage;
            string[] labels = { "1   BEAM RIFLE BURST\nFree  /  +22 Energy", "2   ANTI-SHIP BLADE\n25 Energy  /  Single target", "3   OOTORI MISSILES\n" + (BarrageCooldown > 0 ? "Cooldown: " + BarrageCooldown + " turns" : "40 Energy  /  All enemies"), "4   FULL ARMAMENT BURST\n" + (Burst >= 100 ? "ULTIMATE READY" : "Requires 100% Burst") };
            for (int i = 0; i < 4; i++) { skillLabels[i].text = labels[i]; skillButtons[i].interactable = CanUseSkill(i); }
            chargeButton.interactable = gameplayEnabled && Phase == BattlePhase.PlayerTurn;
            for (int i = 0; i < targetButtons.Length; i++)
            {
                var enemy = enemies[i]; bool alive = enemy != null && enemy.IsAlive;
                targetLabels[i].text = enemy == null ? "Missing target" : (i == SelectedTarget && alive ? "> " : "") + enemy.displayName + "\n" + (alive ? "HP " + enemy.Health + " / " + enemy.maxHealth : "DEFEATED");
                targetButtons[i].interactable = gameplayEnabled && Phase == BattlePhase.PlayerTurn && alive;
                targetButtons[i].image.color = alive && i == SelectedTarget ? new Color(.48f, .13f, .28f, .95f) : new Color(.10f, .15f, .23f, .94f);
            }
            bool showDefense = Phase == BattlePhase.EnemyWindup || Phase == BattlePhase.EnemyAction;
            defensePanel.SetActive(showDefense);
            if (showDefense)
            {
                float remaining = (float)(IncomingImpactTime - CombatTime);
                SetFill(telegraphFill, (float)((CombatTime - windupStart) / Math.Max(.001, IncomingImpactTime - windupStart)));
                telegraphFill.color = remaining <= parryWindow ? new Color(.3f, 1f, .75f) : remaining <= dodgeWindow ? new Color(.25f, .8f, 1f) : new Color(1f, .65f, .14f);
                reactionText.text = DefenseConsumed ? "REACTION LOCKED   /   " + LastDefense : remaining <= parryWindow ? "PARRY NOW!" : remaining <= dodgeWindow ? "DODGE NOW  /  PARRY SOON" : attackerName + "   /   WAIT FOR THE CUE";
                parryButton.interactable = dodgeButton.interactable = Phase == BattlePhase.EnemyWindup && !DefenseConsumed;
            }
        }
        string PhaseLabel()
        {
            switch (Phase) { case BattlePhase.PlayerTurn: return "YOUR TURN"; case BattlePhase.PlayerAction: return "ATTACK"; case BattlePhase.EnemyWindup: case BattlePhase.EnemyAction: return "ENEMY TURN"; case BattlePhase.Victory: return "VICTORY"; case BattlePhase.Defeat: return "DEFEAT"; default: return "PLACE ARENA"; }
        }
        void UpdateSafeArea()
        {
            if (safeRoot == null || Screen.width == 0 || Screen.height == 0) return;
            Rect area = Screen.safeArea; var screen = new Vector2Int(Screen.width, Screen.height);
            if (area == lastSafeArea && screen == lastScreen) return;
            lastSafeArea = area; lastScreen = screen;
            safeRoot.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
        }
        RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var rect = Rect(name, parent, min, max); rect.gameObject.AddComponent<Image>().color = color; return rect;
        }
        Text Label(Transform parent, string value, Vector2 min, Vector2 max, int size, TextAnchor alignment, Color color)
        {
            var rect = Rect("Label", parent, min, max); var text = rect.gameObject.AddComponent<Text>(); text.font = font; text.fontSize = size;
            text.text = value; text.color = color; text.alignment = alignment; text.raycastTarget = false; text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        Button ButtonUI(Transform parent, string value, Vector2 min, Vector2 max, Color color, out Text label)
        {
            var rect = Panel(value.Length > 0 ? value : "Button", parent, min, max, color); var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>(); var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f); colors.pressedColor = new Color(.7f, .8f, .9f); colors.disabledColor = new Color(.50f, .50f, .55f, .55f); button.colors = colors;
            label = Label(rect, value, new Vector2(.03f, .04f), new Vector2(.97f, .96f), 18, TextAnchor.MiddleCenter, Color.white); return button;
        }
        Image Bar(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var rect = Panel(name + " track", parent, min, max, new Color(.15f, .19f, .25f));
            var fill = Panel(name + " fill", rect, Vector2.zero, Vector2.one, color).GetComponent<Image>();
            fill.raycastTarget = false; return fill;
        }
        static void SetFill(Image fill, float amount)
        {
            // Rect sizing works without a sprite and stays correct in all canvas render modes.
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(amount), 1);
        }
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


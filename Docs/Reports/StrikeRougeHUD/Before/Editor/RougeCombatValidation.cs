using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GundamGame.CombatDemo.Editor
{
    // Editor-only smoke test: drives the same public actions as the HUD.
    [InitializeOnLoad]
    public static class RougeCombatValidation
    {
        const string Request = "Temp/StrikeRougeValidation.request";
        const string Running = "RougeCombatValidation.Running";
        const string Report = "Docs/Reports/StrikeRougeCombatValidation.txt";
        static readonly List<string> lines = new List<string>();
        static RougeBattleDemo battle;
        static int step;
        static double deadline, pauseUntil, remaining;
        static int pausedHealth;
        static bool initialized;
        static bool relocated;
        static readonly HashSet<string> captured = new HashSet<string>();
        static float frozenClipTime;
        static Vector3 cameraPosition;
        static Quaternion cameraRotation;
        static float cameraFov;
        static Vector3 normalCameraPosition;
        static Quaternion normalCameraRotation;
        static Mouse testMouse,physicalMouse;
        static Touchscreen testTouch,physicalTouch;
        static float inputYaw,inputDistance;
        static Vector2 inputPoint;
        static readonly Dictionary<Renderer,bool> originalVisibility=new Dictionary<Renderer,bool>();
        static RougeCombatValidation()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) SessionState.SetBool(Running, false);
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredEditMode) return;
                SessionState.SetBool(Running, false); initialized = false; step = 0;
                Application.logMessageReceived -= OnLog;
                CleanupInput();
            };
        }

        [MenuItem("Gundam/Strike Rouge/Validate Combat Demo")]
        public static void RequestValidation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Directory.CreateDirectory("Temp"); File.WriteAllText(Request, "validate");
        }

        static void Update()
        {
            try
            {
                if (!SessionState.GetBool(Running, false))
                {
                    if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
                    // A shell/editor may still be closing the short-lived request file.
                    // Retry next update instead of turning this harmless race into a failure.
                    try { File.Delete(Request); }
                    catch (IOException) { return; }
                    var scene = EditorSceneManager.GetActiveScene();
                    if (scene.isDirty)
                    {
                        string folder = "Assets/Sandbox/StrikeRougeCombat/PreservedScenes";
                        Directory.CreateDirectory(folder);
                        if (!EditorSceneManager.SaveScene(scene, folder + "/BeforeValidation_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true))
                            throw new Exception("Cannot preserve the current scene.");
                    }
                    if (!File.Exists(RougeCombatSceneBuilder.ScenePath)) RougeCombatSceneBuilder.Build();
                    else EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
                    SessionState.SetBool(Running, true);
                    EditorApplication.isPlaying = true;
                    return;
                }
                if (!EditorApplication.isPlaying) return;
                if (!initialized)
                {
                    battle = UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
                    if (battle == null || battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) return;
                    initialized = true; deadline = EditorApplication.timeSinceStartup + 160;
                    lines.Clear();
                    captured.Clear();
                    battle.ResetBattle();
                    Application.logMessageReceived += OnLog;
                    lines.Add("Strike Rouge combat validation " + DateTime.Now.ToString("s"));
                    lines.Add("Unity " + Application.unityVersion + " / Editor virtual arena");
                }
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timed out at step " + step + ": " + battle.DebugSnapshot());
                Tick();
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        static void OnLog(string condition, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                lines.Add("UNITY ERROR: " + condition + "\n" + stack);
        }

        static void Tick()
        {
            CaptureSkillFrame();
            switch (step)
            {
                case 0:
                    Check(battle.enemies.Length == 3, "Three enemy actors assigned");
                    Check(battle.player.rightArm != null && battle.player.torso != null, "Strike Rouge rig assigned");
                    Check(battle.presentation != null && battle.presentation.AnimationReady, "Imported and authored animation clips assigned");
                    Check(battle.presentation.SourceAnimationsPlayed > 0, "Imported appearance clip played before the first turn");
                    Check(battle.presentation.rifleMuzzle != null && battle.presentation.beamMuzzle != null && battle.presentation.railMuzzle != null && battle.presentation.swordTip != null, "Weapon muzzle and blade sockets assigned");
                    Check(!battle.CanUseSkill(3), "Ultimate locked below 100% Burst");
                    Check(RougeBattleDemo.EvaluateDefense(0, 1, true) == RougeBattleDemo.DefenseResult.Failed, "Early parry fails");
                    Check(RougeBattleDemo.EvaluateDefense(.85, 1, true) == RougeBattleDemo.DefenseResult.Parry, "Parry window");
                    Check(RougeBattleDemo.EvaluateDefense(.95, 1, true) == RougeBattleDemo.DefenseResult.PerfectParry, "Perfect parry window");
                    Check(RougeBattleDemo.EvaluateDefense(.7, 1, false) == RougeBattleDemo.DefenseResult.Dodge, "Dodge window");
                    Check(RougeBattleDemo.EvaluateDefense(1.01, 1, false) == RougeBattleDemo.DefenseResult.Failed, "Late reaction fails");
                    Check(battle.TryUseSkill(0), "Beam Shot accepted"); step++; break;
                case 1:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.enemies[0].Health == 145 && battle.player.Health == 328, "Beam damage and three enemy turns resolve");
                    Check(battle.presentation.DamageEvents == 3, "Rifle resolves exactly three damage events");
                    battle.ResetBattle();
                    battle.arenaRoot.SetPositionAndRotation(new Vector3(2, .5f, 1), Quaternion.Euler(0, 35, 0));
                    battle.arenaRoot.localScale = Vector3.one * .1f;
                    var previewCamera=battle.presentation.virtualCamera;
                    normalCameraPosition=previewCamera.transform.position;normalCameraRotation=previewCamera.transform.rotation;
                    previewCamera.transform.position=battle.arenaRoot.TransformPoint(new Vector3(7,6,-11));previewCamera.transform.LookAt(battle.arenaRoot.TransformPoint(new Vector3(0,1,0)));
                    battle.TryUseSkill(1); relocated = false; pauseUntil = EditorApplication.timeSinceStartup + .25; step++; break;
                case 2:
                    if (!relocated && EditorApplication.timeSinceStartup >= pauseUntil)
                    {
                        battle.arenaRoot.position += new Vector3(2, 0, -1);
                        battle.presentation.virtualCamera.transform.position += new Vector3(2,0,-1);
                        relocated = true;
                    }
                    if (battle.Phase != RougeBattleDemo.BattlePhase.EnemyWindup) break;
                    Check(battle.enemies[0].Health == 115, "Saber damage totals 65");
                    Check(battle.presentation.DamageEvents == 2, "Blade damage synchronized to two impacts");
                    Check(relocated && battle.player.visual.localPosition.sqrMagnitude < .000001f, "Saber returns to its slot on a moved, rotated and scaled board");
                    var grid = battle.arenaRoot.Find("Grid").GetComponent<LineRenderer>();
                    Check(Math.Abs(grid.startWidth - .0015f) < .0001, "Authored line width follows 0.1 board scale");
                    Check(battle.TryDefense(true) && battle.LastDefense == RougeBattleDemo.DefenseResult.Failed, "Early live parry consumes reaction");
                    Check(!battle.TryDefense(false), "Cannot retry defense on the same attack"); step++; break;
                case 3:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.EnemyWindup || battle.DefenseConsumed || battle.IncomingSecondsRemaining > .08) break;
                    Check(battle.TryDefense(true) && battle.LastDefense == RougeBattleDemo.DefenseResult.PerfectParry, "Live perfect parry"); step++; break;
                case 4:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.EnemyWindup || battle.DefenseConsumed || battle.IncomingSecondsRemaining > .3) break;
                    Check(battle.TryDefense(false) && battle.LastDefense == RougeBattleDemo.DefenseResult.Dodge, "Live dodge"); step++; break;
                case 5:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.player.Health == 376 && battle.enemies[1].Health == 182, "Defense blocks damage and perfect parry counters");
                    battle.arenaRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity); battle.arenaRoot.localScale = Vector3.one;
                    battle.presentation.virtualCamera.transform.SetPositionAndRotation(normalCameraPosition,normalCameraRotation);
                    battle.ResetBattle(); battle.TryUseSkill(2); step++; break;
                case 6:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.EnemyWindup) break;
                    Check(battle.enemies[0].Health == 137 && battle.enemies[1].Health == 157 && battle.enemies[2].Health == 167, "Barrage hits all three enemies for 43");
                    Check(battle.presentation.MissilesLaunched == 9 && battle.presentation.DamageEvents == 9, "Nine homing missiles resolve once each");
                    remaining = battle.IncomingSecondsRemaining; pausedHealth = battle.player.Health;
                    battle.SetGameplayEnabled(false); pauseUntil = EditorApplication.timeSinceStartup + 2; step++; break;
                case 7:
                    if (EditorApplication.timeSinceStartup < pauseUntil) break;
                    Check(battle.Phase == RougeBattleDemo.BattlePhase.Suspended && battle.player.Health == pausedHealth && Math.Abs(battle.IncomingSecondsRemaining - remaining) < .04, "Tracking suspension preserves HP and reaction time");
                    battle.SetGameplayEnabled(true); step++; break;
                case 8:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.BarrageCooldown == 2 && !battle.CanUseSkill(2), "Barrage cooldown blocks immediate reuse");
                    battle.TryUseSkill(0); step++; break;
                case 9:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.BarrageCooldown == 1, "Cooldown decrements per player turn"); battle.TryUseSkill(0); step++; break;
                case 10:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.BarrageCooldown == 0 && battle.CanUseSkill(2), "Barrage ready after two blocked turns");
                    battle.ResetBattle(); battle.SelectTarget(1); battle.ChargeUltimateForTest();
                    Check(battle.CanUseSkill(3) && battle.TryUseSkill(3), "Test charge unlocks ultimate"); step++; break;
                case 11:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.PlayerTurn) break;
                    Check(battle.enemies[0].Health == 120 && battle.enemies[1].Health == 80 && battle.enemies[2].Health == 150, "Ultimate deals 120 primary / 60 splash");
                    Check(!battle.presentation.CinematicActive, "Ultimate cinematic releases camera control");
                    Check(battle.presentation.SourceAnimationsPlayed >= 2, "Imported launcher clip used by ultimate");
                    battle.ResetBattle(); foreach (var enemy in battle.enemies) enemy.TakeDamage(enemy.maxHealth - 1);
                    battle.TryUseSkill(2); step++; break;
                case 12:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.Victory) break;
                    Check(true, "Victory after defeating all enemies");
                    battle.ResetBattle(); battle.player.TakeDamage(battle.player.maxHealth - 1); battle.TryUseSkill(0); step++; break;
                case 13:
                    if (battle.Phase != RougeBattleDemo.BattlePhase.Defeat) break;
                    Check(true, "Defeat when player HP reaches zero");
                    battle.ResetBattle(); battle.TryUseSkill(2); pauseUntil = EditorApplication.timeSinceStartup + .6; step++; break;
                case 14:
                    if (EditorApplication.timeSinceStartup < pauseUntil) break;
                    battle.ResetBattle(); pauseUntil = EditorApplication.timeSinceStartup + .3; step++; break;
                case 15:
                    if (EditorApplication.timeSinceStartup < pauseUntil) break;
                    Check(battle.player.Health == 400 && battle.Energy == 100 && battle.Burst == 0 && battle.ActionsResolved == 0 && battle.Phase == RougeBattleDemo.BattlePhase.PlayerTurn, "Reset interrupts running action and restores battle");
                    bool clean = true; foreach (Transform child in battle.arenaRoot) if (child.name.EndsWith(" FX")) clean = false;
                    Check(clean, "Reset removes all transient effects");
                    var placement = UnityEngine.Object.FindAnyObjectByType<RougeArenaPlacement>();
                    placement.RequestARMode(); pauseUntil = EditorApplication.timeSinceStartup + .5; step++; break;
                case 16:
                    if (EditorApplication.timeSinceStartup < pauseUntil) break;
                    var ar = UnityEngine.Object.FindAnyObjectByType<RougeArenaPlacement>();
                    Check(!ar.IsARMode && ar.CanBattle && !ar.IsChecking, "Editor AR request returns to virtual arena");
                    Directory.CreateDirectory("Docs/Reports");
                    ScreenCapture.CaptureScreenshot("Docs/Reports/StrikeRougeCombatDemo.png");
                    pauseUntil = EditorApplication.timeSinceStartup + 1; step++; break;
                case 17:
                    if (EditorApplication.timeSinceStartup < pauseUntil) break;
                    var orbit=battle.presentation.arenaCamera;
                    Check(orbit!=null && orbit.CanControl && orbit.Orbit(33,-6,.85f), "Virtual camera accepts orbit and dolly zoom");
                    var camera = battle.presentation.virtualCamera;
                    cameraPosition=camera.transform.position;cameraRotation=camera.transform.rotation;cameraFov=camera.fieldOfView;
                    originalVisibility.Clear();foreach(var renderer in battle.arenaRoot.GetComponentsInChildren<Renderer>(true))originalVisibility[renderer]=renderer.forceRenderingOff;
                    battle.ChargeUltimateForTest();battle.TryUseSkill(3);step++;break;
                case 18:
                    if(!battle.presentation.CinematicActive || battle.presentation.animationRig.ActionTime<.4f)break;
                    var cinema=battle.presentation.cinematicDirector;
                    Check(cinema.Active && cinema.CurrentShot==RougeCinematicDirector.Shot.CasterFocus && cinema.AllEnemiesHidden, "Ultimate opens on the caster with enemies suppressed");
                    Check(cinema.GameplayHudHidden, "Normal gameplay HUD is hidden during the character cut-in");
                    Check(CasterFits(cinema), "Entire caster bounds fit inside cinematic viewport");
                    Check(!battle.presentation.arenaCamera.Orbit(30,10,.5f), "Orbit input is locked during the cinematic");
                    Check(Vector3.Distance(battle.presentation.virtualCamera.transform.position,cameraPosition)<.001f && Quaternion.Angle(battle.presentation.virtualCamera.transform.rotation,cameraRotation)<.01f, "Cinematic renders separately without moving the gameplay camera");
                    battle.SetGameplayEnabled(false);frozenClipTime=battle.presentation.animationRig.ActionTime;pauseUntil=EditorApplication.timeSinceStartup+1.3;step++;break;
                case 19:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Check(Math.Abs(battle.presentation.animationRig.ActionTime-frozenClipTime)<.04, "Tracking loss freezes animation and cinematic clock");
                    battle.SetGameplayEnabled(true);battle.ResetBattle();pauseUntil=EditorApplication.timeSinceStartup+.3;step++;break;
                case 20:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    var restored=battle.presentation.virtualCamera;
                    Check(!battle.presentation.CinematicActive && battle.presentation.ActiveEffectCount==0, "Reset during ultimate removes VFX and cinematic overlay");
                    Check(Vector3.Distance(restored.transform.position,cameraPosition)<.001f && Quaternion.Angle(restored.transform.rotation,cameraRotation)<.01f && Math.Abs(restored.fieldOfView-cameraFov)<.001f, "Reset restores camera pose and FOV");
                    Check(battle.presentation.animationRig.CurrentClipName=="O-02-IDLE", "Reset returns to the imported idle animation");
                    Check(battle.GameplayCanvas.enabled && battle.presentation.arenaCamera.CanControl, "Reset restores gameplay HUD and camera controls");
                    bool visible=true;foreach(var state in originalVisibility)if(state.Key!=null && state.Key.forceRenderingOff!=state.Value)visible=false;
                    Check(visible, "Reset restores original renderer visibility");
                    Check(captured.Count==5, "Captured four skills and the character-focused ultimate intro");
                    physicalMouse=Mouse.current;testMouse=InputSystem.AddDevice<Mouse>("RougeCameraTestMouse");testMouse.MakeCurrent();
                    inputPoint=new Vector2(Screen.width*.55f,Screen.height*.48f);inputYaw=battle.presentation.arenaCamera.Yaw;inputDistance=battle.presentation.arenaCamera.Distance;
                    InputSystem.QueueStateEvent(testMouse,new MouseState{position=inputPoint}.WithButton(MouseButton.Right));
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 21:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    InputSystem.QueueStateEvent(testMouse,new MouseState{position=inputPoint+new Vector2(90,25),delta=new Vector2(90,25)}.WithButton(MouseButton.Right));
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 22:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Check(Math.Abs(Mathf.DeltaAngle(inputYaw,battle.presentation.arenaCamera.Yaw))>1, "Mouse right-drag rotates the virtual camera");
                    InputSystem.QueueStateEvent(testMouse,new MouseState{position=inputPoint,scroll=new Vector2(0,120)});
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 23:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Check(battle.presentation.arenaCamera.Distance<inputDistance, "Mouse wheel zooms the virtual camera");
                    InputSystem.RemoveDevice(testMouse);testMouse=null;if(physicalMouse!=null && physicalMouse.added)physicalMouse.MakeCurrent();
                    physicalTouch=Touchscreen.current;testTouch=InputSystem.AddDevice<Touchscreen>("RougeCameraTestTouch");testTouch.MakeCurrent();
                    inputYaw=battle.presentation.arenaCamera.Yaw;Touch(1,UnityEngine.InputSystem.TouchPhase.Began,inputPoint);
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 24:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Touch(1,UnityEngine.InputSystem.TouchPhase.Moved,inputPoint+new Vector2(65,0));pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 25:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Check(Math.Abs(Mathf.DeltaAngle(inputYaw,battle.presentation.arenaCamera.Yaw))>1, "One-finger drag rotates the virtual camera");
                    Touch(1,UnityEngine.InputSystem.TouchPhase.Ended,inputPoint+new Vector2(65,0));pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 26:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Touch(1,UnityEngine.InputSystem.TouchPhase.Began,inputPoint-Vector2.right*40);Touch(2,UnityEngine.InputSystem.TouchPhase.Began,inputPoint+Vector2.right*40);
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 27:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    inputDistance=battle.presentation.arenaCamera.Distance;
                    Touch(1,UnityEngine.InputSystem.TouchPhase.Moved,inputPoint-Vector2.right*100);Touch(2,UnityEngine.InputSystem.TouchPhase.Moved,inputPoint+Vector2.right*100);
                    pauseUntil=EditorApplication.timeSinceStartup+.15;step++;break;
                case 28:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    Check(battle.presentation.arenaCamera.Distance<inputDistance, "Two-finger pinch zooms the virtual camera");
                    Touch(1,UnityEngine.InputSystem.TouchPhase.Ended,inputPoint-Vector2.right*100);Touch(2,UnityEngine.InputSystem.TouchPhase.Ended,inputPoint+Vector2.right*100);
                    CleanupInput();
                    var view=battle.presentation.arenaCamera;view.Orbit(720,999,.00001f);
                    Check(Math.Abs(view.Pitch-view.maximumPitch)<.01f && Math.Abs(view.Distance-view.minimumDistance)<.01f, "Orbit pitch and zoom-in limits are enforced");
                    view.Orbit(0,-999,99999);
                    Check(Math.Abs(view.Pitch-view.minimumPitch)<.01f && Math.Abs(view.Distance-view.maximumDistance)<.01f, "Orbit pitch and zoom-out limits are enforced");
                    view.ResetView();battle.ResetBattle();
                    pauseUntil=EditorApplication.timeSinceStartup+.3;step++;break;
                case 29:
                    if(EditorApplication.timeSinceStartup<pauseUntil)break;
                    ScreenCapture.CaptureScreenshot("Docs/Reports/StrikeRougeCombatDemo.png");pauseUntil=EditorApplication.timeSinceStartup+.4;step++;break;
                case 30:
                    if(EditorApplication.timeSinceStartup>=pauseUntil)Finish(true,battle.DebugSnapshot());break;
            }
        }
        static void CaptureSkillFrame()
        {
            var p=battle.presentation;if(p==null || battle.Phase!=RougeBattleDemo.BattlePhase.PlayerAction || battle.IsOpening)return;
            string name=null;
            if(p.CinematicActive && p.cinematicDirector.CurrentShot==RougeCinematicDirector.Shot.CasterFocus && p.animationRig.ActionTime>.65f)name="05_UltimateCharacterFocus";
            else if(p.LastSkill.StartsWith("Beam Rifle") && p.DamageEvents>0 && p.ActiveEffectCount>0)name="01_BeamRifle";
            else if(p.LastSkill.StartsWith("Anti-Ship") && p.animationRig.ActionTime>.58f && p.animationRig.ActionTime<1.2f)name="02_AntiShipBlade";
            else if(p.LastSkill.StartsWith("Ootori Missile") && p.MissilesLaunched>=6 && p.ActiveEffectCount>0)name="03_MissileBarrage";
            else if(p.LastSkill=="Full Armament Burst" && p.DamageEvents>=2 && p.CinematicActive)name="04_FullArmamentBurst";
            if(name==null || !captured.Add(name))return;
            Directory.CreateDirectory(RougeSkillSetup.ReportFolder);ScreenCapture.CaptureScreenshot(RougeSkillSetup.ReportFolder+"/"+name+".png");
        }
        static bool CasterFits(RougeCinematicDirector director)
        {
            var bounds=director.CasterBounds;var camera=director.ShotCamera;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                var p=camera.WorldToViewportPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
                if(p.z<=camera.nearClipPlane || p.x<0 || p.x>1 || p.y<.075f || p.y>.9f)return false;
            }
            return true;
        }
        static void Touch(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 position)
        {InputSystem.QueueStateEvent(testTouch,new TouchState{touchId=id,phase=phase,position=position,pressure=phase==UnityEngine.InputSystem.TouchPhase.Ended?0:1});}
        static void CleanupInput()
        {
            if(testMouse!=null && testMouse.added)InputSystem.RemoveDevice(testMouse);testMouse=null;
            if(testTouch!=null && testTouch.added)InputSystem.RemoveDevice(testTouch);testTouch=null;
            if(physicalMouse!=null && physicalMouse.added)physicalMouse.MakeCurrent();
            if(physicalTouch!=null && physicalTouch.added)physicalTouch.MakeCurrent();
        }
        static void Check(bool valid, string description)
        {
            if (!valid) throw new Exception("FAIL: " + description + "\n" + battle.DebugSnapshot());
            lines.Add("PASS: " + description);
        }
        static void Finish(bool success, string detail)
        {
            CleanupInput();
            Application.logMessageReceived -= OnLog;
            if (lines.Exists(x => x.StartsWith("UNITY ERROR:"))) success = false;
            lines.Add((success ? "RESULT: PASS" : "RESULT: FAIL") + "\n" + detail);
            Directory.CreateDirectory("Docs/Reports"); File.WriteAllLines(Report, lines);
            SessionState.SetBool(Running, false); initialized = false; step = 0;
            EditorApplication.isPlaying = false;
            Debug.Log("Strike Rouge validation " + (success ? "PASS" : "FAIL") + " -> " + Report);
        }
    }
}

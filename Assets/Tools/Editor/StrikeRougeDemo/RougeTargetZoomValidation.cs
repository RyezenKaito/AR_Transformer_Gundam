using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace GundamGame.CombatDemo.Editor
{
    [InitializeOnLoad]
    public static class RougeTargetZoomValidation
    {
        const string Running="RougeTargetZoomValidation.Running", Folder="Docs/Reports/StrikeRougeHUD";
        static readonly List<string> lines=new List<string>();
        static RougeBattleDemo battle;
        static Mouse mouse;
        static int step,actions;
        static int[] health;
        static double waitUntil,deadline;
        static float initialDistance;
        static Vector2 point;
        static bool initialized;
        static RougeTargetZoomValidation()
        {
            EditorApplication.update+=Update;
            EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Running,false);Cleanup();}};
        }
        [MenuItem("Gundam/Strike Rouge/Validate Zoom and Target Selection")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before validation.");
            var scene=EditorSceneManager.GetActiveScene();
            if(scene.isDirty)
            {
                Directory.CreateDirectory("Assets/Sandbox/StrikeRougeCombat/PreservedScenes");
                if(!EditorSceneManager.SaveScene(scene,"Assets/Sandbox/StrikeRougeCombat/PreservedScenes/BeforeTargetZoom_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve scene.");
            }
            if(scene.path!=RougeCombatSceneBuilder.ScenePath)EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
            EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        static void Update()
        {
            if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(!initialized)
                {
                    battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
                    if(battle==null||battle.Hud==null||battle.IsOpening||battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)return;
                    initialized=true;step=0;deadline=EditorApplication.timeSinceStartup+100;
                    lines.Clear();lines.Add("Zoom and targeting / Unity "+Application.unityVersion+" / "+DateTime.Now.ToString("s"));
                    lines.Add("Virtual arena, real Play Mode pointer input and damage callbacks. No device AR test.");
                    Application.logMessageReceived+=OnLog;Directory.CreateDirectory(Folder);
                    battle.player.maxHealth=4000;battle.player.ResetFighter();
                    foreach(var enemy in battle.enemies){enemy.maxHealth=4000;enemy.ResetFighter();}
                    mouse=InputSystem.AddDevice<Mouse>("RougeTargetZoomQAMouse");
                }
                if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timeout: step "+step+" / "+battle.DebugSnapshot());
                if(EditorApplication.timeSinceStartup<waitUntil)return;
                Tick();
            }
            catch(Exception ex){Finish(false,ex.ToString());}
        }
        static void Tick()
        {
            var hud=battle.Hud;var orbit=battle.presentation.arenaCamera;
            switch(step)
            {
                case 0:
                    hud.Refresh(true);Canvas.ForceUpdateCanvases();
                    if(hud.targetButtons.Any(b=>b.GetComponent<Graphic>().depth<0))return;
                    foreach(int index in new[]{2,0,1,2}){Click(hud.targetButtons[index]);Check(battle.SelectedTarget==index,"HUD selects enemy "+index+" without left-to-right restriction");}
                    Check(battle.ActionsResolved==0&&battle.Burst==2,"Changing targets spends no turn or Burst");
                    foreach(int index in new[]{1,0,2}){Check(battle.TrySelectTargetAtScreenPoint(ScreenPoint(index)),"Model body is pickable for enemy "+index);Check(battle.SelectedTarget==index,"World pick selects enemy "+index);}
                    Check(hud.PreviewLabel.Contains(battle.enemies[2].displayName.ToUpperInvariant()),"Skill preview names the chosen primary target");
                    Check(!battle.TrySelectTargetAtScreenPoint(new Vector2(-50,-50))&&battle.SelectedTarget==2,"Offscreen click preserves target");
                    battle.SetGameplayEnabled(false);battle.SelectTarget(0);Check(battle.SelectedTarget==2&&!battle.TrySelectTargetAtScreenPoint(ScreenPoint(0)),"Tracking suspension blocks both target selection paths");battle.SetGameplayEnabled(true);
                    float one=RougeArenaCamera.ScrollZoomFactor(1,.22f);
                    Check(one>.79f&&one<.81f,"Normalized wheel notch zooms about twenty percent closer");
                    Check(Mathf.Abs(one-RougeArenaCamera.ScrollZoomFactor(120,.22f,true))<.0001f,"Legacy Windows 120-unit notch matches normalized input");
                    Check(Mathf.Abs(one*RougeArenaCamera.ScrollZoomFactor(-1,.22f)-1)<.0001f,"Opposite wheel directions are reversible before clamps");
                    orbit.ResetView();orbit.ZoomByScroll(10000);Check(Mathf.Abs(orbit.Distance-orbit.minimumDistance)<.001f,"Zoom-in clamps at the safe minimum");
                    orbit.ZoomByScroll(-10000);Check(Mathf.Abs(orbit.Distance-orbit.maximumDistance)<.001f,"Zoom-out clamps at the safe maximum");orbit.ResetView();
                    initialDistance=orbit.Distance;point=new Vector2(Screen.width*.53f,Screen.height*.55f);
                    Queue(point,false,1);Next(1);break;
                case 1:
                    Check(orbit.Distance<initialDistance*.85f&&orbit.Distance>initialDistance*.7f,"Actual wheel input changes the virtual camera by the new sensitivity");
                    orbit.ResetView();point=ScreenPoint(2);battle.SelectTarget(0);Queue(point,true);Next(2);break;
                case 2: Queue(point,false);Next(3);break;
                case 3:
                    Check(battle.SelectedTarget==2,"Mouse click on right-hand enemy selects it through the runtime input path");
                    Check(battle.ActionsResolved==0,"Clicking a model selects without firing");
                    ScreenCapture.CaptureScreenshot(Folder+"/TargetSelection_RightEnemy.png");Next(30);break;
                case 30:
                    health=battle.enemies.Select(e=>e.Health).ToArray();actions=battle.ActionsResolved;
                    Click(hud.skillButtons[0]);Click(hud.executeButton);
                    Check(battle.Phase==RougeBattleDemo.BattlePhase.PlayerAction,"Selected primary begins attacking");
                    battle.SelectTarget(0);Check(battle.SelectedTarget==2&&!battle.TrySelectTargetAtScreenPoint(ScreenPoint(0)),"An executing attack keeps its captured target");
                    Check(!orbit.Orbit(10,0)||!battle.presentation.CinematicActive,"Camera respects the cinematic input lock");step=4;break;
                case 4:
                    AutoDefense();if(!Ready(actions+1))return;
                    Check(battle.enemies[2].Health<health[2]&&battle.enemies[0].Health==health[0]&&battle.enemies[1].Health==health[1],"Rifle damages only the chosen right-hand enemy");
                    Check(battle.SelectedTarget==2,"Chosen living target is preserved into the next player turn");
                    point=ScreenPoint(1);Queue(point,true);Next(5);break;
                case 5: Queue(point,false);Next(6);break;
                case 6:
                    Check(battle.SelectedTarget==1,"Next turn can switch directly to the middle enemy");
                    health=battle.enemies.Select(e=>e.Health).ToArray();actions=battle.ActionsResolved;
                    Click(hud.skillButtons[1]);Click(hud.executeButton);step=7;break;
                case 7:
                    AutoDefense();if(!Ready(actions+1))return;
                    Check(battle.enemies[1].Health<health[1]&&battle.enemies[0].Health==health[0]&&battle.enemies[2].Health==health[2],"Blade damages only the newly chosen middle enemy");
                    Check(battle.SelectedTarget==1,"Switching skills does not reset the primary target");
                    point=ScreenPoint(2);Queue(point,true);Next(8);break;
                case 8: Queue(point+new Vector2(45,0),true);Next(9);break;
                case 9: Queue(point,false);Next(10);break;
                case 10:
                    Check(battle.SelectedTarget==1,"Dragging over a model does not accidentally change target");
                    var menu=hud.GetComponentsInChildren<Button>(true).Single(b=>b.name=="System Menu");Click(menu);
                    Check(hud.MenuVisible&&!battle.TrySelectTargetAtScreenPoint(ScreenPoint(0)),"System overlay blocks world target selection");Click(menu);
                    point=ButtonPoint(hud.skillButtons[0]);Check(!battle.TrySelectTargetAtScreenPoint(point)&&battle.SelectedTarget==1,"HUD controls block clicks through to the battlefield");
                    battle.enemies[0].TakeDamage(int.MaxValue);battle.SelectTarget(0);
                    Check(battle.SelectedTarget==1&&!battle.TrySelectTargetAtScreenPoint(ScreenPoint(0)),"Defeated targets cannot be selected from UI or world");battle.enemies[0].ResetFighter();
                    Check(battle.TrySelectTargetAtScreenPoint(ScreenPoint(2)),"A living target can still be selected after rejecting a dead one");
                    Finish(true,"PASS: freely choose any living enemy; real rifle/blade hits follow that choice. Zoom was tested through the Input System.");break;
            }
        }
        static bool Ready(int actions)=>battle.Phase==RougeBattleDemo.BattlePhase.PlayerTurn&&battle.ActionsResolved>=actions;
        static void AutoDefense(){if(battle.Phase==RougeBattleDemo.BattlePhase.EnemyWindup&&!battle.DefenseConsumed&&battle.IncomingSecondsRemaining<.15)battle.TryDefense(true);}
        static Vector2 ScreenPoint(int index)=>battle.TargetingCamera.WorldToScreenPoint(battle.enemies[index].AimPoint);
        static Vector2 ButtonPoint(Button b)=>RectTransformUtility.WorldToScreenPoint(null,b.transform.TransformPoint(((RectTransform)b.transform).rect.center));
        static void Queue(Vector2 position,bool down,float scroll=0)
        {InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=(ushort)(down?1:0),scroll=new Vector2(0,scroll)});}
        static void Next(int value){step=value;waitUntil=EditorApplication.timeSinceStartup+.25;}
        static void Click(Button b)
        {
            battle.Hud.Refresh(true);Canvas.ForceUpdateCanvases();Check(b.interactable,"Button enabled: "+b.name);
            var data=new PointerEventData(EventSystem.current){position=ButtonPoint(b),button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Check(hits.Count>0&&(hits[0].gameObject==b.gameObject||hits[0].gameObject.transform.IsChildOf(b.transform)),"Top UI hit: "+b.name);
            ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);
        }
        static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name+"\n"+battle.DebugSnapshot());lines.Add("PASS: "+name);}
        static void OnLog(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)lines.Add("UNITY ERROR: "+text+"\n"+stack);}
        static void Cleanup(){Application.logMessageReceived-=OnLog;if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);mouse=null;initialized=false;}
        static void Finish(bool success,string detail)
        {
            if(lines.Any(l=>l.StartsWith("UNITY ERROR:")))success=false;
            lines.Add("RESULT: "+(success?"PASS":"FAIL"));lines.Add(detail);Directory.CreateDirectory(Folder);File.WriteAllLines(Folder+"/TargetZoomValidation.txt",lines);
            SessionState.SetBool(Running,false);Cleanup();EditorApplication.isPlaying=false;Debug.Log("Target/zoom validation "+(success?"PASS":"FAIL"));
        }
    }
}

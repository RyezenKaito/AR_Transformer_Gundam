using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GundamLobby.Editor
{
    [InitializeOnLoad]
    public static class UIIntegrationValidation
    {
        const string Running="UIIntegrationValidation.Running";
        const string Request="Temp/LobbyIntegrationValidation.request";
        static readonly string[] Animated={"Unicorn","StrikeRouge"};
        static readonly List<string> log=new List<string>();
        static HomeLobbyController lobby;
        static InventoryPanel inventory;
        static StrikerSelectionController selection;
        static Transform[] bones;
        static Quaternion[] rotations;
        static Vector3[] positions;
        static int step,index;
        static double waitUntil,deadline;
        static bool ready;
        static UIIntegrationValidation()
        {
            EditorApplication.update+=Update;
            EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Running,false);ready=false;Application.logMessageReceived-=OnLog;RestorePreference();}};
        }
        [MenuItem("Gundam/Lobby/Validate UI Integration")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first.");
            if(EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the scene first.");
            EditorSceneManager.OpenScene(UIIntegrationAuthoring.ScenePath);
            SessionState.SetBool("UIIntegration.HadPreference",PlayerPrefs.HasKey(StrikerSelectionController.PreferenceKey));
            SessionState.SetString("UIIntegration.Preference",PlayerPrefs.GetString(StrikerSelectionController.PreferenceKey,""));
            SessionState.SetBool("UIIntegration.RestorePreference",true);
            PlayerPrefs.SetString(StrikerSelectionController.PreferenceKey,"Unicorn");
            typeof(GundamGame.CombatDemo.Editor.RougeHudValidation).GetMethod("SetSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1920,1080)});
            EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        static void RestorePreference()
        {
            if(!SessionState.GetBool("UIIntegration.RestorePreference",false))return;
            if(SessionState.GetBool("UIIntegration.HadPreference",false))PlayerPrefs.SetString(StrikerSelectionController.PreferenceKey,SessionState.GetString("UIIntegration.Preference",""));
            else PlayerPrefs.DeleteKey(StrikerSelectionController.PreferenceKey);
            PlayerPrefs.Save();SessionState.SetBool("UIIntegration.RestorePreference",false);
        }
        static void OnLog(string text,string stack,LogType type)
        {if(type==LogType.Exception||type==LogType.Assert||type==LogType.Error)log.Add("UNITY ERROR: "+text);}
        static void Check(bool pass,string text){log.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass)throw new Exception(text);}
        static void Update()
        {
            if(!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&!EditorApplication.isPlayingOrWillChangePlaymode&&File.Exists(Request))
            {File.Delete(Request);try{Run();}catch(Exception e){Directory.CreateDirectory(UIIntegrationAuthoring.Report);File.WriteAllText(UIIntegrationAuthoring.Report+"/Validation.txt","FAIL\n"+e);}return;}
            if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(!ready)
                {
                    lobby=UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>();
                    inventory=UnityEngine.Object.FindFirstObjectByType<InventoryPanel>();
                    selection=UnityEngine.Object.FindFirstObjectByType<StrikerSelectionController>();
                    if(!lobby||!inventory||!selection||!lobby.PreviewModel)return;
                    ready=true;index=0;step=0;log.Clear();deadline=EditorApplication.timeSinceStartup+120;
                    Directory.CreateDirectory(UIIntegrationAuthoring.Report);
                    log.Add("Lobby / inventory / roster integration / "+DateTime.Now.ToString("s")+" / Unity "+Application.unityVersion);
                    log.Add("Actual Editor Play Mode; no phone, networking or final stat balance validation.");
                    Application.logMessageReceived+=OnLog;
                }
                if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out at step "+step+" unit "+index);
                if(EditorApplication.timeSinceStartup<waitUntil)return;
                Tick();
            }
            catch(Exception e){Finish(false,e.ToString());}
        }
        static void Capture(string name){ScreenCapture.CaptureScreenshot(UIIntegrationAuthoring.Report+"/"+name+".png");}
        static void Remember(GameObject model)
        {
            bones=model.GetComponentsInChildren<Transform>(true);
            rotations=bones.Select(t=>t.localRotation).ToArray();positions=bones.Select(t=>t.localPosition).ToArray();
        }
        static bool Moved()=>Enumerable.Range(0,bones.Length).Any(i=>bones[i]&&(Quaternion.Angle(bones[i].localRotation,rotations[i])>.01f||Vector3.Distance(bones[i].localPosition,positions[i])>.00001f));
        static void Click(Button button)
        {
            Check(button!=null&&button.interactable,"Button available: "+(button?button.name:"missing"));
            ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        }
        static void Tick()
        {
            switch(step)
            {
                case 0:
                    Check(selection.catalog.units.Length==10&&selection.cards.Length==10,"All ten catalog units and roster cards are present");
                    Check(EditorBuildSettings.scenes[0].path==UIIntegrationAuthoring.ScenePath,"HomeLobby is the first build scene");
                    int missing=EditorSceneManager.GetActiveScene().GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
                    Check(missing==0,"Imported scene has no missing MonoBehaviour scripts");
                    foreach(var unit in selection.catalog.units)Check(unit.prefab!=null&&unit.portrait!=null,"Model and portrait references: "+unit.id);
                    Check(lobby.PreviewModel.GetComponentInChildren<UnicornDemo.UnicornMotion>()!=null,"Home lobby spawns the animated Unicorn");
                    Remember(lobby.PreviewModel);waitUntil=EditorApplication.timeSinceStartup+.8;step=1;break;
                case 1:
                    Check(Moved(),"Home lobby Unicorn joints visibly advance over time");Capture("01_HomeLobby");
                    waitUntil=EditorApplication.timeSinceStartup+.3;step=11;break;
                case 11:inventory.Open();waitUntil=EditorApplication.timeSinceStartup+.3;step=2;break;
                case 2:
                    Check(inventory.IsOpen&&inventory.PreviewModel!=null&&inventory.VisibleItemCount>0,"Inventory opens with items and the selected model");
                    Check(inventory.PreviewModel.GetComponentsInChildren<Renderer>().All(r=>r.gameObject.layer==30),"Inventory renderers stay on the isolated preview layer");
                    Remember(inventory.PreviewModel);waitUntil=EditorApplication.timeSinceStartup+.8;step=3;break;
                case 3:
                    Check(Moved(),"Inventory preview animates while HomeLobbyController is disabled");Capture("02_Inventory");
                    waitUntil=EditorApplication.timeSinceStartup+.3;step=12;break;
                case 12:
                    inventory.Close();selection.Open();
                    Check(selection.cards.Count(card=>card.gameObject.activeInHierarchy)==2&&selection.counterLabel.text=="02/02","Only two completed Gundam appear in the roster");
                    waitUntil=EditorApplication.timeSinceStartup+.3;step=4;break;
                case 4:
                    int selected=Array.FindIndex(selection.catalog.units,u=>u.id==Animated[index]);
                    Click(selection.cards[selected].GetComponent<Button>());
                    Check(selection.SelectedIndex==selected&&selection.PreviewModel!=null,"Roster button selects "+Animated[index]);
                    if(Animated[index]!="Unicorn")Check(selection.PreviewModel.GetComponent<LobbyPreviewAnimation>()?.IsPlaying==true,"PlayableGraph active for "+Animated[index]);
                    Remember(selection.PreviewModel);waitUntil=EditorApplication.timeSinceStartup+.85;step=5;break;
                case 5:
                    Check(Moved(),"Source rig joints advance for "+Animated[index]);
                    var renderers=selection.PreviewModel.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&!(r is LineRenderer)).ToArray();
                    Check(renderers.Length>0&&renderers.SelectMany(r=>r.sharedMaterials).All(m=>m!=null&&m.shader!=null&&m.shader.name!="Hidden/InternalErrorShader"),"Renderable materials for "+Animated[index]);
                    Capture("Roster_"+Animated[index]);
                    index++;if(index<Animated.Length){step=4;waitUntil=EditorApplication.timeSinceStartup+.15;break;}
                    selection.Select(Array.FindIndex(selection.catalog.units,u=>u.id=="StrikeRouge"));
                    Click(selection.confirmButton);waitUntil=EditorApplication.timeSinceStartup+.3;step=6;break;
                case 6:
                    Check(!selection.IsOpen&&lobby.enabled&&lobby.modelPivot.gameObject.activeSelf,"Confirm closes selection and restores the lobby");
                    Check(lobby.PreviewModel.GetComponent<LobbyPreviewAnimation>()?.unitId=="StrikeRouge","Confirmed animated Rouge is shown in the lobby");
                    inventory.Open();waitUntil=EditorApplication.timeSinceStartup+.3;step=7;break;
                case 7:
                    Check(inventory.PreviewModel.GetComponent<LobbyPreviewAnimation>()?.unitId=="StrikeRouge","Inventory follows the confirmed Gundam");
                    Remember(inventory.PreviewModel);waitUntil=EditorApplication.timeSinceStartup+.8;step=8;break;
                case 8:
                    Check(Moved(),"Confirmed Rouge animates inside inventory");Capture("03_InventoryRouge");
                    waitUntil=EditorApplication.timeSinceStartup+.3;step=13;break;
                case 13:
                    inventory.Close();selection.Open();selection.Select(Array.FindIndex(selection.catalog.units,u=>u.id=="Unicorn"));selection.Close();
                    Check(selection.EquippedIndex==Array.FindIndex(selection.catalog.units,u=>u.id=="StrikeRouge"),"Canceling a different preview preserves the equipped Gundam");
                    selection.Open();waitUntil=EditorApplication.timeSinceStartup+.4;step=9;break;
                case 9:
                    Check(selection.PreviewModel.GetComponent<LobbyPreviewAnimation>()?.IsPlaying==true,"Animation restarts correctly after closing and reopening selection");
                    Capture("04_StrikerSelection");waitUntil=EditorApplication.timeSinceStartup+.3;step=10;break;
                case 10:Finish(true,"Home, inventory, both playable Gundam, confirm/cancel and reopen passed.");break;
            }
        }
        static void Finish(bool passed,string detail)
        {
            Application.logMessageReceived-=OnLog;if(log.Any(l=>l.StartsWith("UNITY ERROR:")))passed=false;
            log.Add("RESULT: "+(passed?"PASS":"FAIL"));log.Add(detail);
            File.WriteAllLines(UIIntegrationAuthoring.Report+"/Validation.txt",log);
            SessionState.SetBool(Running,false);RestorePreference();ready=false;
            typeof(GundamGame.CombatDemo.Editor.RougeHudValidation).GetMethod("RestoreSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            EditorApplication.isPlaying=false;Debug.Log("Lobby integration validation "+(passed?"PASS":"FAIL"));
        }
    }
}

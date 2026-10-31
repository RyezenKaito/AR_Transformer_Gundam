using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GundamGame.CombatDemo.Editor
{
    [InitializeOnLoad]
    public static class RougeHudValidation
    {
        const string Running="RougeHudValidation.Running";
        const string Folder="Docs/Reports/StrikeRougeHUD";
        static readonly List<string> lines=new List<string>();
        static RougeBattleDemo battle;
        static int step,sizeIndex,originalSize=-1;
        static double deadline,waitUntil;
        static bool initialized;
        static float frozenClip;
        static int actions,healthBefore;
        static Vector3 cameraPosition;
        static Quaternion cameraRotation;
        static EditorWindow gameView;
        static PropertyInfo selectedSize;
        static readonly Vector2Int[] Sizes={new Vector2Int(1920,1080),new Vector2Int(2340,1080),new Vector2Int(1280,720),new Vector2Int(1024,768)};
        static RougeHudValidation()
        {
            EditorApplication.update+=Update;
            EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Running,false);initialized=false;Application.logMessageReceived-=OnLog;RestoreSize();}};
        }
        [MenuItem("Gundam/Strike Rouge/Validate Mecha HUD")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before validation.");
            var current=EditorSceneManager.GetActiveScene();
            if(current.isDirty)
            {Directory.CreateDirectory("Assets/Sandbox/StrikeRougeCombat/PreservedScenes");if(!EditorSceneManager.SaveScene(current,"Assets/Sandbox/StrikeRougeCombat/PreservedScenes/BeforeHudValidation_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve scene.");}
            if(current.path!=RougeCombatSceneBuilder.ScenePath)EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
            SessionState.SetBool("RougeHudValidation.LayoutOnly",false);SetSize(Sizes[0]);SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        [MenuItem("Gundam/Strike Rouge/Validate HUD Layouts")]
        public static void RunLayouts(){Run();SessionState.SetBool("RougeHudValidation.LayoutOnly",true);}
        static void Update()
        {
            if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(!initialized)
                {
                    battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();if(battle==null||battle.Hud==null||battle.IsOpening||battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)return;
                    initialized=true;step=0;sizeIndex=0;deadline=EditorApplication.timeSinceStartup+200;
                    lines.Clear();lines.Add("Strike Rouge Mecha HUD validation "+DateTime.Now.ToString("s"));lines.Add("Unity "+Application.unityVersion+" / virtual arena / simulated Game View resolutions");
                    Application.logMessageReceived+=OnLog;Directory.CreateDirectory(Folder);
                    if(SessionState.GetBool("RougeHudValidation.LayoutOnly",false))
                    {battle.ChargeUltimateForTest();battle.Hud.SelectSkill(3);waitUntil=EditorApplication.timeSinceStartup+.7;step=10;}
                    else{battle.player.maxHealth=4000;battle.player.ResetFighter();foreach(var e in battle.enemies){e.maxHealth=4000;e.ResetFighter();}PureRules();}
                }
                if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out step "+step+" "+battle.DebugSnapshot());
                Tick();
            }
            catch(Exception ex){Finish(false,ex.ToString());}
        }
        static void OnLog(string value,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)lines.Add("UNITY ERROR: "+value+"\n"+stack);}
        static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name+"\n"+battle.DebugSnapshot());lines.Add("PASS: "+name);}
        static void PureRules()
        {
            Check(RougeBattleDemo.MaximumBurst==5,"Burst capacity is five cells");
            Check(Enumerable.Range(0,4).Select(RougeBattleDemo.SkillCost).SequenceEqual(new[]{0,1,2,5}),"Skill costs are 0 1 2 5");
            Check(RougeBattleDemo.EvaluateDefense(9.5,10,false)==RougeBattleDemo.DefenseResult.Failed,"Dodge outside timing fails");
            Check(RougeBattleDemo.EvaluateDefense(9.7,10,false)==RougeBattleDemo.DefenseResult.Dodge,"Dodge inside timing is eligible");
            Check(RougeBattleDemo.EvaluateDefense(9.85,10,true)==RougeBattleDemo.DefenseResult.Parry,"Parry inside timing is eligible");
            Check(RougeBattleDemo.EvaluateDefense(10.01,10,true)==RougeBattleDemo.DefenseResult.Failed,"Late parry fails");
            Check(RougeBattleDemo.DodgeEvades(145,144,1),"Higher Speed always evades after valid timing");
            Check(RougeBattleDemo.DodgeEvades(144,144,.499f)&&!RougeBattleDemo.DodgeEvades(144,144,.5f),"Equal Speed uses the 50 percent threshold");
            Check(RougeBattleDemo.DodgeEvades(120,160,.499f)&&!RougeBattleDemo.DodgeEvades(120,160,.5f),"Lower Speed uses the 50 percent threshold");
            Check(RougeBattleDemo.ParryMultiplier(80,99)==0&&RougeBattleDemo.ParryMultiplier(80,100)==.5f,"Parry compares Def times 1.25 strictly above Attack");
            var go=new GameObject("Combat rule test");var f=go.AddComponent<DemoFighter>();f.maxHealth=1000;f.maxArmor=100;f.visual=go.transform;f.ResetFighter();
            Check(f.ReceiveCombatHit(100,80)==50&&Mathf.Abs(f.Armor-20)<.01f,"Armor hit depletes real armor and halves HP damage");
            Check(f.ReceiveCombatHit(100,80)==50&&f.ArmorBroken&&f.ArmorBreakCount==1,"Breaking hit retains initial armor protection and triggers break once");
            Check(f.ReceiveCombatHit(100,80)==100&&f.ArmorBreakCount==1,"Broken armor no longer halves HP damage");
            f.ResetFighter();Check(f.ReceiveCombatHit(100,80,0)==0&&f.Armor==100,"Full parry preserves HP and armor");
            Check(f.ReceiveCombatHit(100,80,.5f)==25&&f.Armor==60,"Half parry scales both damage channels once");
            UnityEngine.Object.Destroy(go);
            var order=new RougeTurnOrder();order.Reset(battle.player,battle.enemies);Check(order.TakeNext()==battle.enemies[2],"Fastest living Gundam acts first");
            Check(order.TakeNext()==battle.enemies[1]&&order.TakeNext()==battle.player,"Speed action order is reflected in simulation");
            var original=Enumerable.Repeat(0,4).ToArray();order.Reset(battle.player,battle.enemies);
            for(int i=0;i<1000;i++){var actor=order.TakeNext();int index=actor==battle.player?0:Array.IndexOf(battle.enemies,actor)+1;original[index]++;}
            Check(original[3]>original[0]&&original[0]>original[1],"Faster actors receive more turns over time");
        }
        static void Tick()
        {
            var hud=battle.Hud;
            switch(step)
            {
                case 0:
                    if(!ReadyForUI())break;
                    Check(battle.Burst==2,"Battle starts with two Burst cells");hud.Refresh(true);
                    Check(hud.LitBurstCells==2&&hud.burstCells.Length==5,"HUD shows exactly five cells with two lit");
                    Check(hud.art!=null&&hud.art.playerPortrait!=null&&hud.art.enemyPortraits.All(x=>x!=null)&&hud.art.weaponIcons.All(x=>x!=null),"Portrait and weapon artwork references are complete");
                    actions=battle.ActionsResolved;Click(hud.skillButtons[2]);Check(hud.SelectedSkill==2&&battle.ActionsResolved==actions&&battle.Phase==RougeBattleDemo.BattlePhase.PlayerTurn,"Skill button previews without attacking");
                    Click(hud.skillButtons[3]);Check(!hud.executeButton.interactable&&hud.PreviewLabel.Contains("INSUFFICIENT"),"Unavailable ultimate explains missing Burst and disables execute");
                    Check(!hud.ExecuteSelected()&&battle.Burst==2,"Cannot execute ultimate below five cells");
                    Click(hud.skillButtons[0]);Check(hud.PreviewLabel.Contains("2 > 4"),"Basic attack preview includes skill and end-turn recovery");
                    healthBefore=battle.enemies[0].Health;Click(hud.executeButton);Check(battle.Phase==RougeBattleDemo.BattlePhase.PlayerAction,"Execute button starts selected skill");
                    Check(!hud.ExecuteSelected(),"Repeated execute is rejected during animation");step++;break;
                case 1:
                    AutoDefense();if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn||battle.ActionsResolved<actions+1)break;
                    if(!ReadyForUI())break;
                    Check(battle.Burst==4,"Basic attack recovers two cells including end-turn recovery");
                    Check(battle.enemies[0].Health<healthBefore&&battle.enemies[0].Armor<battle.enemies[0].maxArmor,"Skill changes the target HP and Armor values used by HUD");
                    Click(hud.skillButtons[1]);Click(hud.executeButton);Check(battle.Burst==3,"Skill two spends one cell immediately");step++;break;
                case 2:
                    AutoDefense();if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn||battle.ActionsResolved<actions+2)break;
                    if(!ReadyForUI())break;
                    Check(battle.Burst==4,"Skill two receives only the one end-turn cell");
                    Click(hud.skillButtons[2]);Click(hud.executeButton);Check(battle.Burst==2,"Skill three spends two cells immediately");step++;break;
                case 3:
                    AutoDefense();if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn||battle.ActionsResolved<actions+3)break;
                    if(!ReadyForUI())break;
                    Check(battle.Burst==3,"Missile skill receives one end-turn cell");Check(battle.presentation.MissilesLaunched==9,"Missile presentation resolves nine missiles");
                    foreach(var enemy in battle.enemies)Check(enemy.Armor<enemy.maxArmor,"Missile barrage depletes armor of "+enemy.displayName);
                    battle.ChargeUltimateForTest();hud.SelectSkill(3);hud.Refresh(true);Check(hud.LitBurstCells==5&&hud.executeButton.interactable,"Five cells light up and enable ultimate");
                    ScreenCapture.CaptureScreenshot(Folder+"/01_FullBurstReady.png");waitUntil=EditorApplication.timeSinceStartup+.3;step++;break;
                case 4:
                    if(EditorApplication.timeSinceStartup<waitUntil)break;
                    cameraPosition=battle.presentation.virtualCamera.transform.position;cameraRotation=battle.presentation.virtualCamera.transform.rotation;
                    Click(hud.executeButton);Check(battle.Burst==0,"Ultimate consumes all five cells");step++;break;
                case 5:
                    if(!battle.presentation.CinematicActive||battle.presentation.animationRig.ActionTime<.5f)break;
                    Check(!battle.GameplayCanvas.enabled&&battle.presentation.cinematicDirector.AllEnemiesHidden,"Ultimate cut-in hides combat HUD and enemies");
                    Check(!battle.presentation.arenaCamera.Orbit(30,0),"Cinematic locks orbit input");
                    ScreenCapture.CaptureScreenshot(Folder+"/02_UltimateFocus.png");step++;break;
                case 6:
                    AutoDefense();if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn||battle.ActionsResolved<actions+4)break;
                    if(!ReadyForUI())break;
                    Check(battle.Burst==1,"Ultimate finishes with one end-turn cell");Check(battle.GameplayCanvas.enabled&&!battle.presentation.CinematicActive,"HUD returns after cinematic");
                    Check(Vector3.Distance(cameraPosition,battle.presentation.virtualCamera.transform.position)<.001f&&Quaternion.Angle(cameraRotation,battle.presentation.virtualCamera.transform.rotation)<.01f,"Ultimate preserves the viewing angle");
                    hud.SelectSkill(0);hud.ExecuteSelected();step++;break;
                case 7:
                    if(battle.Phase!=RougeBattleDemo.BattlePhase.EnemyWindup)break;
                    hud.Refresh(true);Canvas.ForceUpdateCanvases();if(hud.parryButton.GetComponent<Graphic>().depth<0)break;
                    hud.Refresh(true);Check(hud.DefenseVisible,"Enemy windup replaces weapon buttons with defense controls");
                    ScreenCapture.CaptureScreenshot(Folder+"/03_Defense.png");
                    Click(hud.parryButton);Check(battle.DefenseConsumed,"First defense button consumes one reaction");Check(!battle.TryDefense(false),"Cannot combine Dodge and Parry on the same attack");
                    battle.SetGameplayEnabled(false);frozenClip=battle.presentation.animationRig.ActionTime;waitUntil=EditorApplication.timeSinceStartup+.5;step++;break;
                case 8:
                    if(EditorApplication.timeSinceStartup<waitUntil)break;
                    Check(Mathf.Abs(frozenClip-battle.presentation.animationRig.ActionTime)<.04f,"Tracking suspension freezes the animation clock");hud.Refresh(true);Check(hud.trackingPanel.gameObject.activeSelf,"Tracking suspension displays the pause overlay");
                    battle.SetGameplayEnabled(true);battle.ResetBattle();step++;break;
                case 9:
                    AutoDefense();if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)break;
                    Check(battle.Burst==2&&!battle.presentation.CinematicActive&&battle.presentation.ActiveEffectCount==0,"Reset restores Burst and clears cinematic effects");
                    Check(battle.player.Armor<=battle.player.maxArmor&&battle.player.Armor>0,"Reset restores real armor before enemy actions");
                    Check(battle.presentation.arenaCamera.Orbit(20,-3,.95f),"Virtual orbit remains functional");
                    SetSize(Sizes[sizeIndex]);waitUntil=EditorApplication.timeSinceStartup+.7;step++;break;
                case 10:
                    AutoDefense();if(EditorApplication.timeSinceStartup<waitUntil)break;
                    hud.Refresh(true);Canvas.ForceUpdateCanvases();
                    Check(Screen.width==Sizes[sizeIndex].x&&Screen.height==Sizes[sizeIndex].y,"Game View renders "+Sizes[sizeIndex].x+"x"+Sizes[sizeIndex].y);
                    var screenCorners=new Vector3[4];hud.safeRoot.GetWorldCorners(screenCorners);
                    Check(Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null,screenCorners[0]),Vector2.zero)<2&&Vector2.Distance(RectTransformUtility.WorldToScreenPoint(null,screenCorners[2]),new Vector2(Screen.width,Screen.height))<2,"Canvas covers the actual render area at "+Screen.width+"x"+Screen.height);
                    foreach(var button in hud.skillButtons)Check(Inside(button.transform as RectTransform,hud.safeRoot),"Skill tile inside safe area at "+Screen.width+"x"+Screen.height);
                    foreach(var button in hud.skillButtons)
                    {var name=button.transform.Find("Skill Name") as RectTransform;var cost=button.transform.Find("Cost") as RectTransform;Check(name!=null&&cost!=null&&!Overlap(name,cost),"Skill name and cost do not overlap: "+button.name+" at "+Screen.width+"x"+Screen.height);}
                    Check(!Overlap(hud.statusPanel,hud.actionPanel),"Status and weapon panels do not overlap at "+Screen.width+"x"+Screen.height);
                    Check(hud.safeRoot.GetComponentsInChildren<Text>(true).All(t=>t.font!=null),"All HUD labels have a valid font at "+Screen.width+"x"+Screen.height);
                    foreach(var text in hud.safeRoot.GetComponentsInChildren<Text>())if(!string.IsNullOrWhiteSpace(text.text))Check(text.cachedTextGenerator.characterCountVisible>0,"Visible text is generated: "+text.name+" at "+Screen.width+"x"+Screen.height);
                    ScreenCapture.CaptureScreenshot(Folder+"/Layout_"+Screen.width+"x"+Screen.height+".png");
                    waitUntil=EditorApplication.timeSinceStartup+.35;step=12;break;
                case 12:
                    if(EditorApplication.timeSinceStartup<waitUntil)break;
                    sizeIndex++;if(sizeIndex<Sizes.Length){SetSize(Sizes[sizeIndex]);waitUntil=EditorApplication.timeSinceStartup+.7;step=10;break;}
                    hud.SetSafeAreaPreview(new Rect(80,24,Screen.width-120,Screen.height-48));Canvas.ForceUpdateCanvases();
                    Check(Inside(hud.statusPanel,hud.safeRoot)&&Inside(hud.actionPanel,hud.safeRoot),"Simulated notch and edge insets preserve all primary controls");hud.SetSafeAreaPreview(null);
                    if(SessionState.GetBool("RougeHudValidation.LayoutOnly",false)){Finish(true,"Final layout pass uses the scene's normal HP values. Combat flow is covered by Validation.txt.");break;}
                    battle.ChargeUltimateForTest();hud.SelectSkill(3);hud.ExecuteSelected();waitUntil=EditorApplication.timeSinceStartup+.6;step=11;break;
                case 11:
                    if(EditorApplication.timeSinceStartup<waitUntil)break;
                    battle.ResetBattle();Check(!battle.presentation.CinematicActive&&battle.GameplayCanvas.enabled,"Reset during ultimate restores HUD immediately");Check(battle.presentation.ActiveEffectCount==0,"Reset during ultimate removes active effects");
                    Finish(true,"All checks completed. Physical AR and on-device performance were not tested.");break;
            }
        }
        static void AutoDefense(){if(battle.Phase==RougeBattleDemo.BattlePhase.EnemyWindup&&!battle.DefenseConsumed&&battle.IncomingSecondsRemaining<.15)battle.TryDefense(true);}
        static bool ReadyForUI(){if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)return false;battle.Hud.Refresh(true);Canvas.ForceUpdateCanvases();return battle.Hud.skillButtons.All(b=>b.GetComponent<Graphic>().depth>=0);}
        static bool Inside(RectTransform child,RectTransform parent)
        {var c=new Vector3[4];child.GetWorldCorners(c);Rect r=parent.rect;return c.All(v=>{Vector3 p=parent.InverseTransformPoint(v);return p.x>=r.xMin-2&&p.x<=r.xMax+2&&p.y>=r.yMin-2&&p.y<=r.yMax+2;});}
        static bool Overlap(RectTransform a,RectTransform b)
        {var ac=new Vector3[4];var bc=new Vector3[4];a.GetWorldCorners(ac);b.GetWorldCorners(bc);return new Rect(ac[0],ac[2]-ac[0]).Overlaps(new Rect(bc[0],bc[2]-bc[0]));}
        static void Click(Button button)
        {
            battle.Hud.Refresh(true);Canvas.ForceUpdateCanvases();Check(button.interactable,"UI button is interactable: "+button.name);
            var data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(((RectTransform)button.transform).rect.center))};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Check(hits.Count>0&&(hits[0].gameObject==button.gameObject||hits[0].gameObject.transform.IsChildOf(button.transform)),"Button is the top UI raycast hit: "+button.name+" at "+data.position+"; hits="+string.Join(",",hits.Select(h=>h.gameObject.name))+"; depth="+button.GetComponent<Graphic>().depth);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }
        static void SetSize(Vector2Int size)
        {
            const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
            var assembly=typeof(UnityEditor.Editor).Assembly;var type=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var sizes=singleton.GetProperty("instance",flags).GetValue(null);
            var group=type.GetProperty("currentGroup",flags).GetValue(sizes);
            var enumType=assembly.GetType("UnityEditor.GameViewSizeType");var viewSizeType=assembly.GetType("UnityEditor.GameViewSize");
            var entry=Activator.CreateInstance(viewSizeType,flags,null,new object[]{Enum.Parse(enumType,"FixedResolution"),size.x,size.y,"HUD QA "+size.x+"x"+size.y},null);
            group.GetType().GetMethod("AddCustomSize",flags).Invoke(group,new[]{entry});
            int count=(int)group.GetType().GetMethod("GetTotalCount",flags).Invoke(group,null);
            var viewType=assembly.GetType("UnityEditor.GameView");gameView=EditorWindow.GetWindow(viewType);selectedSize=viewType.GetProperty("selectedSizeIndex",flags);
            if(originalSize<0)originalSize=(int)selectedSize.GetValue(gameView);
            selectedSize.SetValue(gameView,count-1);gameView.Repaint();
        }
        static void RestoreSize(){if(gameView!=null&&selectedSize!=null&&originalSize>=0)try{selectedSize.SetValue(gameView,originalSize);}catch{}originalSize=-1;}
        static void Finish(bool ok,string detail)
        {
            Application.logMessageReceived-=OnLog;if(lines.Any(l=>l.StartsWith("UNITY ERROR:")))ok=false;
            lines.Add("RESULT: "+(ok?"PASS":"FAIL"));lines.Add(detail);Directory.CreateDirectory(Folder);File.WriteAllLines(Folder+(SessionState.GetBool("RougeHudValidation.LayoutOnly",false)?"/LayoutValidation.txt":"/Validation.txt"),lines);
            SessionState.SetBool(Running,false);initialized=false;RestoreSize();EditorApplication.isPlaying=false;Debug.Log("Mecha HUD validation "+(ok?"PASS":"FAIL"));
        }
    }
}

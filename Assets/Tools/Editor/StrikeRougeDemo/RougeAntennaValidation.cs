using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GundamGame.CombatDemo.Editor
{
    [InitializeOnLoad]
    public static class RougeAntennaValidation
    {
        const string Running="RougeAntennaValidation.Running";
        static readonly List<string> lines=new List<string>();
        static RougeBattleDemo battle;
        static bool initialized,beamObserved;
        static int step,target;
        static int[] health;
        static float pausedTime;
        static Vector3 originalScale;
        static double deadline,waitUntil;
        static RougeAntennaValidation()
        {
            EditorApplication.update+=Update;
            EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Running,false);initialized=false;Application.logMessageReceived-=OnLog;}};
        }
        [MenuItem("Gundam/Strike Rouge/Validate V-Fin Pulse")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before validation.");
            if(EditorSceneManager.GetActiveScene().path!=RougeCombatSceneBuilder.ScenePath)throw new InvalidOperationException("Open the combat demo first.");
            if(EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the combat scene before validation.");
            var setSize=typeof(RougeHudValidation).GetMethod("SetSize",BindingFlags.NonPublic|BindingFlags.Static);
            setSize.Invoke(null,new object[]{new Vector2Int(1600,900)});
            SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        static void Check(bool pass,string detail){lines.Add((pass?"PASS: ":"FAIL: ")+detail);if(!pass)throw new Exception(detail);}
        static void OnLog(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)lines.Add("UNITY ERROR: "+text);}
        static void Update()
        {
            if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(!initialized)
                {
                    battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
                    if(battle==null||battle.Hud==null||battle.IsOpening)return;
                    initialized=true;step=0;target=2;beamObserved=false;deadline=EditorApplication.timeSinceStartup+100;
                    originalScale=battle.arenaRoot.localScale;lines.Clear();
                    lines.Add("Rouge V-Fin Pulse / Unity "+Application.unityVersion+" / "+DateTime.Now.ToString("s"));
                    lines.Add("Real Editor Play Mode, virtual arena. Device AR not tested.");
                    Directory.CreateDirectory(RougeAntennaAuthoring.ReportFolder);Application.logMessageReceived+=OnLog;
                }
                if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Validation timeout at "+step);
                if(EditorApplication.timeSinceStartup<waitUntil)return;
                Tick();
            }
            catch(Exception ex){Finish(false,ex.ToString());}
        }
        static void Prepare()
        {
            battle.SetGameplayEnabled(true);battle.player.speed=160;battle.player.maxHealth=4000;
            foreach(var enemy in battle.enemies){enemy.speed=120;enemy.maxHealth=4000;}
            battle.ResetBattle();beamObserved=false;
        }
        static void Tick()
        {
            var p=battle.presentation;var hud=battle.Hud;
            switch(step)
            {
                case 0:
                    Check(p.AnimationReady&&p.animationRig.antenna.name=="Rouge_AntennaPulse","Dedicated head-fire clip assigned and presentation ready");
                    Check(p.antennaMuzzle!=null&&p.antennaMuzzle.parent.name.StartsWith("head_"),"Emitter is parented to the head bone");
                    Check(p.antennaLeftTip.parent==p.antennaMuzzle.parent&&p.antennaRightTip.parent==p.antennaMuzzle.parent,"Both V-fin tips follow the same animated head");
                    Check(battle.player.muzzle==p.antennaMuzzle&&p.antennaMuzzle!=p.rifleMuzzle,"Basic attack and fallback use head, not hand/shield socket");
                    Check(AssetDatabase.GetAssetPath(hud.art.weaponIcons[0])==RougeAntennaAuthoring.IconPath,"HUD uses the new antenna icon");
                    Check(hud.skillButtons[0].GetComponentsInChildren<Text>().Any(t=>t.text=="V-FIN PULSE"),"Skill button label matches the antenna attack");
                    Prepare();step=1;break;
                case 1:
                    if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)return;
                    battle.SelectTarget(target);health=battle.enemies.Select(e=>e.Health).ToArray();
                    hud.SelectSkill(0);Check(hud.PreviewLabel.Contains("Antenna pulse"),"Skill preview describes antenna firing");
                    Check(hud.ExecuteSelected(),"Execute accepts skill one against enemy "+target);step=2;break;
                case 2:
                    var beam=battle.arenaRoot.GetComponentsInChildren<LineRenderer>().FirstOrDefault(l=>l.name=="Weapon beam halo FX");
                    if(beam!=null && (target!=2 || p.DamageEvents>0))
                    {
                        beamObserved=true;
                        Vector3 start=beam.transform.TransformPoint(beam.GetPosition(0));
                        Check(Vector3.Distance(start,p.antennaMuzzle.position)<.04f*Mathf.Abs(battle.arenaRoot.lossyScale.x),"Visible beam starts at animated V-fin muzzle for enemy "+target);
                        if(target==2)
                        {
                            battle.SetGameplayEnabled(false);pausedTime=p.animationRig.ActionTime;
                            ScreenCapture.CaptureScreenshot(RougeAntennaAuthoring.ReportFolder+"/01_AntennaBeam.png");
                            waitUntil=EditorApplication.timeSinceStartup+.35;step=3;break;
                        }
                        step=4;
                    }
                    if(battle.ActionsResolved>0&&!beamObserved)throw new Exception("No V-fin beam observed");
                    break;
                case 3:
                    Check(Mathf.Abs(p.animationRig.ActionTime-pausedTime)<.0001f&&battle.ActionsResolved==0,"Tracking pause freezes head animation and unfinished attack");
                    battle.SetGameplayEnabled(true);step=4;break;
                case 4:
                    if(battle.ActionsResolved<1)return;
                    Check(p.DamageEvents==3,"Exactly three damage callbacks for enemy "+target);
                    Check(battle.enemies[target].Health<health[target]&&Enumerable.Range(0,3).Where(i=>i!=target).All(i=>battle.enemies[i].Health==health[i]),"Only the selected enemy is damaged");
                    Check(battle.Burst==4,"Start 2/5, zero cost, skill +1 and turn +1 gives 4/5 Burst");
                    if(target>0)
                    {
                        target--;
                        if(target==1)battle.arenaRoot.localScale=originalScale*.35f;
                        else battle.arenaRoot.localScale=originalScale;
                        Prepare();step=1;
                    }
                    else {Prepare();step=5;}
                    break;
                case 5:
                    if(battle.Phase!=RougeBattleDemo.BattlePhase.PlayerTurn)return;
                    Check(battle.TryUseSkill(0),"Begin basic attack for reset cancellation");step=6;break;
                case 6:
                    if(p.ActiveEffectCount==0)return;
                    battle.ResetBattle();
                    Check(p.ActiveEffectCount==0&&p.DamageEvents==0,"Reset cancels antenna charge and clears all VFX/damage counters");
                    Check(!p.CinematicActive&&battle.GameplayCanvas.enabled,"Normal HUD and camera remain available after reset");
                    hud.Refresh(true);ScreenCapture.CaptureScreenshot(RougeAntennaAuthoring.ReportFolder+"/02_AntennaHUD.png");
                    waitUntil=EditorApplication.timeSinceStartup+.3;step=7;break;
                case 7:Finish(true,"All three targets passed, including scaled arena, pause/resume and reset. Damage/armor tuning and Burst costs unchanged.");break;
            }
        }
        static void Finish(bool ok,string detail)
        {
            Application.logMessageReceived-=OnLog;
            if(lines.Any(l=>l.StartsWith("UNITY ERROR:")))ok=false;
            lines.Add("RESULT: "+(ok?"PASS":"FAIL"));lines.Add(detail);
            File.WriteAllLines(RougeAntennaAuthoring.ReportFolder+"/Validation.txt",lines);
            SessionState.SetBool(Running,false);initialized=false;
            typeof(RougeHudValidation).GetMethod("RestoreSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
            EditorApplication.isPlaying=false;Debug.Log("Rouge V-Fin validation "+(ok?"PASS":"FAIL"));
        }
    }
}

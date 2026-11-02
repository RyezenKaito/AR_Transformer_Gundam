using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TurnBasedBattle.Editor
{
    public static class BattleEnvironmentVerification
    {
        static void Need(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("Turn Battle/Environment/Verify Switching and Combat")]
        public static void Run()
        {
            Need(EditorApplication.isPlaying,"Enter Play Mode first");
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var s=battle.GetComponent<BattleEnvironmentSwitcher>();var camera=s.desktopOrbit;
            bool manual=battle.manualSimulation,camManual=camera.manualSimulation;int chosen=s.SelectedIndex;
            var results=new List<string>();
            try
            {
                battle.manualSimulation=true;camera.manualSimulation=true;battle.Restart(1);camera.ResetView(true);
                Need(s.Count==3,"Expected three environments");
                for(int i=0;i<s.Count;i++)
                {
                    int expected=(s.SelectedIndex+1)%s.Count;int id=battle.BattleId;float hp=battle.player.HP;var phase=battle.Phase;var pose=camera.transform.position;float yaw=camera.Yaw;
                    s.nextButton.onClick.Invoke();Need(s.SelectedIndex==expected,"Serialized button listener missing or doubled");
                    Need(battle.BattleId==id && battle.player.HP==hp && battle.Phase==phase,"Changing environment reset battle");
                    Need(camera.transform.position==pose && camera.Yaw==yaw,"Changing environment moved camera");
                    for(int n=1;n<s.Count;n++)Need(s.environments[n].root.activeSelf==(n==expected),"Multiple environment roots active");
                    Need(s.trainingRenderers.All(r=>r.enabled==(expected==0)),"Training renderers state");
                    var root=s.environments[expected].root;
                    if(root)foreach(var r in root.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)Need(m && m.shader && m.shader.isSupported && m.shader.name.StartsWith("Universal Render Pipeline/"),"Invalid material: "+r.name);
                    results.Add("PASS — UI button selects "+s.CurrentName+" without resetting HP, turn or orbit");
                }
                battle.Select(BattleActionKind.Attack);Need(battle.Confirm(),"Attack rejected");battle.AdvanceForTest(2);
                float time=battle.ActionTime;var position=battle.player.transform.position;int action=battle.ActionId;int records=battle.HitRecords.Count;
                s.nextButton.onClick.Invoke();Need(battle.ActionTime==time&&battle.ActionId==action&&battle.player.transform.position==position&&battle.HitRecords.Count==records,"Mid-action switch changed combat");
                results.Add("PASS — switching during approach preserves motion clock and damage records");
                for(int n=0;n<s.Count;n++)
                {
                    s.Apply(n);battle.Restart(3);battle.FillUltimate();battle.Select(BattleActionKind.EarthPrison);Need(battle.Confirm(),"Ultimate rejected");int id=battle.ActionId;int limit=2000;
                    while(battle.InAction&&battle.ActionId==id&&limit-->0)battle.AdvanceForTest(1f/60);
                    Need(limit>0&&battle.enemies.All(e=>e.HP==240),"Ultimate damage in "+s.CurrentName);
                    Need(Vector3.Distance(battle.player.transform.position,battle.player.AnchorPosition)<.001f,"Did not return to anchor");
                    results.Add("PASS — 1v3 Earth Prison damage and return in "+s.CurrentName);
                }
                battle.Restart(1);var external=new GameObject("EnvironmentExternalCamera_TEST").AddComponent<Camera>();external.enabled=false;external.backgroundColor=Color.magenta;external.fieldOfView=63;
                try
                {
                    camera.UseExternalCamera(external);var ambient=RenderSettings.ambientLight;s.Next();
                    Need(external.backgroundColor==Color.magenta&&external.fieldOfView==63&&RenderSettings.ambientLight==ambient,"External camera/lighting ownership changed");
                    results.Add("PASS — external camera background, projection and lighting ownership preserved (simulation)");
                }
                finally{camera.RestoreDesktopCamera();UnityEngine.Object.DestroyImmediate(external.gameObject);}
            }
            finally{battle.Restart(1);s.Apply(chosen);camera.ResetView(true);battle.manualSimulation=manual;camera.manualSimulation=camManual;}
            File.WriteAllText("Assets/TurnBasedBattle/Reports/EnvironmentValidation.md","# Environment verification\n\n"+string.Join("\n\n",results)+"\n\nAndroid device / physical AR tracking not tested. HDRP procedural shading is approximated using baked URP maps.\n");
            Debug.Log("ENVIRONMENT_VERIFICATION_PASS: "+results.Count+" checks");
        }
        [MenuItem("Turn Battle/Environment/Capture Environment Previews")]
        public static void Capture(){UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>().StartCoroutine(Preview());}
        static IEnumerator Preview()
        {
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var s=battle.GetComponent<BattleEnvironmentSwitcher>();var orbit=s.desktopOrbit;var c=battle.battleCamera;
            bool manual=battle.manualSimulation,camManual=orbit.manualSimulation;int selected=s.SelectedIndex;
            try
            {
                battle.manualSimulation=true;orbit.manualSimulation=true;battle.Restart(1);orbit.ResetView(true);
                for(int i=0;i<s.Count;i++)
                {
                    s.Apply(i);for(int n=0;n<20;n++)yield return null;
                    Shot(c,battle.hud.GetComponent<Canvas>(),"Assets/TurnBasedBattle/Reports/Environment_"+i+".png");
                }
                s.Apply(s.Count-1);orbit.Orbit(65,12);for(int n=0;n<120;n++)orbit.Step(1f/60);
                yield return null;yield return new WaitForEndOfFrame();
                Shot(c,battle.hud.GetComponent<Canvas>(),"Assets/TurnBasedBattle/Reports/SphereCity_Orbit.png");
            }
            finally{s.Apply(selected);battle.Restart(1);orbit.ResetView(true);battle.manualSimulation=manual;orbit.manualSimulation=camManual;}
            Debug.Log("ENVIRONMENT_PREVIEWS_READY");
        }
        static void Shot(Camera camera,Canvas canvas,string path)
        {
            var old=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;var mode=canvas.renderMode;var cam=canvas.worldCamera;float plane=canvas.planeDistance;
            var rt=new RenderTexture(1440,810,24);var tex=new Texture2D(1440,810,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;camera.aspect=1440f/810;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,810),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            }
            finally{canvas.renderMode=mode;canvas.worldCamera=cam;canvas.planeDistance=plane;camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TurnBasedBattle;

namespace ProjectEnvironments.Editor
{
    public static class EnvironmentVerification
    {
        const string Report="Assets/EnvironmentSystem/Documentation/Verification.md";
        static readonly List<string> results=new List<string>();
        static void Check(bool condition,string message)
        {
            results.Add((condition?"PASS — ":"FAIL — ")+message);
            Directory.CreateDirectory(Path.GetDirectoryName(Report));
            File.WriteAllText(Report,"# Environment switching — Editor Play Mode\n\n"+string.Join("\n\n",results)+"\n\nStandalone player / Android not tested.\n");
            if(!condition) throw new Exception(message);
        }
        [MenuItem("Tools/Environment System/Verify Switching in Play Mode")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying || !EnvironmentNavigator.Instance) throw new Exception("Open EnvironmentHub and enter Play Mode first.");
            results.Clear(); EnvironmentNavigator.Instance.StartCoroutine(Routine());
        }
        static IEnumerator Wait(EnvironmentNavigator n)
        {
            float until=Time.realtimeSinceStartup+90;
            while(n.Busy && Time.realtimeSinceStartup<until) yield return null;
            Check(!n.Busy,"Scene transition completed within timeout");
            yield return null;
        }
        static IEnumerator Routine()
        {
            var n=EnvironmentNavigator.Instance;
            bool hadPreference=PlayerPrefs.HasKey(EnvironmentNavigator.PreferenceKey);
            string old=PlayerPrefs.GetString(EnvironmentNavigator.PreferenceKey,"");
            try
            {
                Check(EnvironmentTools.Validate().Count==0,"Catalog IDs, scene references and enabled build scenes valid");
                Check(!n.Select("missing-test-id") && !n.Busy,"Unknown ID rejected without beginning a load");
                var unavailable=n.Catalog.environments.FirstOrDefault(d=>d && !d.available);
                if(unavailable) Check(!n.Select(unavailable.id) && !n.Busy,"Unavailable environment rejected");
                foreach(string id in n.Catalog.environments.Where(d=>d && d.available).Select(d=>d.id).ToArray())
                {
                    var d=n.Catalog.Find(id);
                    Check(n.Select(id),"Load request accepted: "+id);
                    Check(!n.Select(id),"Repeated request blocked during load: "+id);
                    yield return Wait(n);
                    Check(SceneManager.GetActiveScene().path==d.ScenePath,"Correct scene active: "+id);
                    Check(SceneManager.sceneCount==1,"Single environment scene loaded: "+id);
                    Check(UnityEngine.Object.FindObjectsByType<EnvironmentNavigator>().Length==1,"One persistent navigator: "+id);
                    Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(x=>x.enabled)==1,"One active audio listener: "+id);
                    Check(UnityEngine.Object.FindObjectsByType<Camera>().Count(x=>x.enabled)==1,"One active camera: "+id);
                    if(d.battleVariant>=0) Check(UnityEngine.Object.FindAnyObjectByType<BattleEnvironmentSwitcher>().SelectedIndex==d.battleVariant,"Correct existing battle variant: "+id);
                    if(id=="biogear-lab") VerifyBiogearContent();
                    Check(n.LastSelectedId==id,"Last selection saved: "+id);
                    Check(n.OpenHub(),"Return to hub accepted: "+id);
                    yield return Wait(n);
                    Check(SceneManager.GetActiveScene().path==n.Catalog.hubScenePath,"Hub restored after "+id);
                }
                // Missing-scene fixture is in memory only, never written into the asset catalog.
                var saved=n.Catalog.hubScenePath;
                n.Catalog.hubScenePath="Assets/MissingScene_TEST.unity";
                bool rejected=!n.OpenHub() && !n.Busy;
                n.Catalog.hubScenePath=saved;
                Check(rejected,"Missing scene rejected with usable error state");
                Debug.Log("ENVIRONMENT_SWITCHING_PASS: "+results.Count+" checks");
            }
            finally
            {
                if(hadPreference) PlayerPrefs.SetString(EnvironmentNavigator.PreferenceKey,old); else PlayerPrefs.DeleteKey(EnvironmentNavigator.PreferenceKey);
                PlayerPrefs.Save();
            }
        }
        static void VerifyBiogearContent()
        {
            var explorer=UnityEngine.Object.FindAnyObjectByType<BiogearLab.BiogearExplorer>();
            Check(explorer && explorer.viewpoints.Length==8 && explorer.viewpoints.All(t=>t),"Biogear has eight saved review viewpoints");
            var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>();
            Check(renderers.All(r=>r.sharedMaterials.All(m=>m && m.shader && m.shader.isSupported)),"Biogear has no missing or unsupported materials");
            Check(UnityEngine.Object.FindObjectsByType<MeshFilter>().All(m=>m.sharedMesh && m.sharedMesh.vertexCount>0),"Biogear meshes remain valid after scene load");
            Check(UnityEngine.Object.FindObjectsByType<Transform>().Count(t=>t.name.StartsWith("Water platform "))==5,"Biogear has five water platforms");
            for(int i=0;i<8;i++)
            {
                explorer.View(i);
                Check(Vector3.Distance(explorer.transform.position,explorer.viewpoints[i].position)<.001f,"Review camera moves to viewpoint "+(i+1));
            }
            explorer.View(0);
            Physics.SyncTransforms();
            Check(Physics.Raycast(new Vector3(15,20,0),Vector3.down,out var ringHit,30) && ringHit.point.y>3.9f,"Main ring has a standing surface collider");
            Check(Physics.Raycast(new Vector3(0,22,0),Vector3.down,out var holeHit,30) && holeHit.point.y<6,"Upper disk center remains a real open hole");
            Check(!Physics.Linecast(new Vector3(-24,1,0),new Vector3(-32,1,0)),"Dome entrance collision is open");
            Check(!Physics.Linecast(new Vector3(0,2.1f,17.5f),new Vector3(0,5,31.5f)),"Corridor passage collision is open");
        }
    }
}

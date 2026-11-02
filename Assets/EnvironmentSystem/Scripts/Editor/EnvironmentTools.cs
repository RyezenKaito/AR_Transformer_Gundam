using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace ProjectEnvironments.Editor
{
    public static class EnvironmentTools
    {
        public const string Hub="Assets/EnvironmentSystem/Scenes/EnvironmentHub.unity";
        public const string CatalogPath="Assets/EnvironmentSystem/Resources/EnvironmentCatalog.asset";
        const string Battle="Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity";
        [MenuItem("Tools/Environment System/Open Environment Hub")]
        public static void OpenHub()
        {
            if(EditorApplication.isPlaying) { EnvironmentNavigator.Instance.OpenHub(); return; }
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(Hub);
        }
        [MenuItem("Tools/Environment System/Install Environment Hub")]
        public static void Install()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            Directory.CreateDirectory("Assets/EnvironmentSystem/Scenes");
            Directory.CreateDirectory("Assets/EnvironmentSystem/Resources");
            Directory.CreateDirectory("Assets/EnvironmentSystem/Data");
            Directory.CreateDirectory("Assets/EnvironmentSystem/Materials");
            AssetDatabase.Refresh();
            var previous=SceneManager.GetActiveScene();
            try
            {
                CreateScene(Hub,false,true);
            }
            finally { if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); }
            var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(CatalogPath);
            if(!catalog) { catalog=ScriptableObject.CreateInstance<EnvironmentCatalog>(); AssetDatabase.CreateAsset(catalog,CatalogPath); }
            var entries=new List<EnvironmentDefinition>(catalog.environments.Where(d=>d));
            Add(entries,"training","SÀN TẬP","Trận đấu hiện có · phông nền sàn tập.",Battle,0,new Color(.24f,.55f,.68f));
            Add(entries,"hangar","SCI-FI HANGAR","Trận đấu hiện có · nhà chứa robot.",Battle,1,new Color(.43f,.48f,.64f));
            Add(entries,"sphere-city","SPHERE CITY","Trận đấu hiện có · thành phố vòm.",Battle,2,new Color(.19f,.65f,.7f));
            Add(entries,"biogear-lab","BIOGEAR LAB","Đã có brief/reference · chưa dựng map.","",-1,new Color(.48f,.72f,.18f));
            catalog.environments=entries.ToArray(); EditorUtility.SetDirty(catalog);
            RegisterScenes(catalog); AssetDatabase.SaveAssets();
            Debug.Log("ENVIRONMENT_SYSTEM_INSTALLED: "+catalog.environments.Length+" definitions. Existing scenes preserved.");
        }
        static void Add(List<EnvironmentDefinition> list,string id,string title,string description,string path,int variant,Color accent)
        {
            // Re-running installation preserves user changes and additional maps.
            if(list.Any(d=>d.id==id)) return;
            string assetPath="Assets/EnvironmentSystem/Data/"+id+".asset";
            var d=AssetDatabase.LoadAssetAtPath<EnvironmentDefinition>(assetPath);
            if(!d)
            {
                d=ScriptableObject.CreateInstance<EnvironmentDefinition>(); d.id=id; d.displayName=title; d.description=description;
                d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path); d.SyncScene(); d.battleVariant=variant; d.accent=accent; d.available=d.scene;
                AssetDatabase.CreateAsset(d,assetPath);
            }
            list.Add(d);
        }
        [MenuItem("Tools/Environment System/Register Catalog Scenes in Build")]
        public static void RegisterScenes()
        { RegisterScenes(AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(CatalogPath)); AssetDatabase.SaveAssets(); }
        static void RegisterScenes(EnvironmentCatalog c)
        {
            if(!c) throw new Exception("Missing catalog.");
            var scenes=EditorBuildSettings.scenes.ToList();
            var paths=new HashSet<string>{c.hubScenePath};
            foreach(var d in c.environments) if(d) { d.SyncScene(); EditorUtility.SetDirty(d); if(d.available) paths.Add(d.ScenePath); }
            foreach(var path in paths)
            {
                if(string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                var item=scenes.Find(s=>s.path==path); if(item!=null) item.enabled=true; else scenes.Add(new EditorBuildSettingsScene(path,true));
            }
            EditorBuildSettings.scenes=scenes.ToArray();
        }
        [MenuItem("Tools/Environment System/Validate Catalog")]
        public static void ValidateMenu() { var issues=Validate(); Debug.Log(issues.Count==0?"ENVIRONMENT_CATALOG_VALID":string.Join("\n",issues)); }
        public static List<string> Validate()
        {
            var issues=new List<string>(); var c=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(CatalogPath);
            if(!c) { issues.Add("Missing EnvironmentCatalog."); return issues; }
            var enabled=new HashSet<string>(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path));
            if(!enabled.Contains(c.hubScenePath)) issues.Add("Hub missing from enabled build scenes.");
            var ids=new HashSet<string>();
            foreach(var d in c.environments)
            {
                if(!d) { issues.Add("Null definition."); continue; }
                if(string.IsNullOrWhiteSpace(d.id) || !ids.Add(d.id)) issues.Add("Empty or duplicate ID: "+d.name);
                d.SyncScene(); EditorUtility.SetDirty(d);
                if(d.available && (!d.scene || !enabled.Contains(d.ScenePath))) issues.Add(d.id+": scene missing or not enabled in build.");
                if(!d.thumbnail) Debug.Log(d.id+": using accent thumbnail fallback.");
            }
            AssetDatabase.SaveAssets(); return issues;
        }
        static Material Mat(string id,Color color)
        {
            string path="Assets/EnvironmentSystem/Materials/"+id+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path); if(m) return m;
            m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=color; m.SetFloat("_Smoothness",.25f); AssetDatabase.CreateAsset(m,path); return m;
        }
        static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.position=position; g.transform.localScale=scale; g.GetComponent<Renderer>().sharedMaterial=material; return g;
        }
        static void CreateScene(string path,bool desert,bool hub)
        {
            if(File.Exists(path)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path)); AssetDatabase.Refresh();
            var s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive); SceneManager.SetActiveScene(s);
            try
            {
                var camera=new GameObject("Preview Camera").AddComponent<Camera>(); camera.tag="MainCamera"; camera.gameObject.AddComponent<AudioListener>();
                camera.transform.position=new Vector3(25,22,-30); camera.transform.LookAt(new Vector3(0,0,0)); camera.fieldOfView=55;
                camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=desert?new Color(.73f,.66f,.51f):new Color(.16f,.39f,.54f);
                if(!hub)
                {
                    var light=new GameObject("Sun").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.7f; light.transform.rotation=Quaternion.Euler(48,-30,0); light.shadows=LightShadows.Soft;
                    RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(.48f,.55f,.62f); RenderSettings.skybox=null;
                    var ground=Mat(desert?"Sand":"SeaPreview",desert?new Color(.66f,.47f,.25f):new Color(.04f,.48f,.61f));
                    Shape(desert?"Sand Base":"Sea Base — placeholder",PrimitiveType.Cube,new Vector3(0,-1.4f,0),new Vector3(100,1,100),ground);
                    Shape("Build Platform",PrimitiveType.Cylinder,new Vector3(0,-.3f,0),new Vector3(28,.6f,28),Mat("BuildDeck",new Color(.55f,.65f,.67f)));
                    for(int i=0;i<8;i++)
                    {
                        float a=i*Mathf.PI/4;
                        Shape("Blockout Landmark "+i,PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*19,2,Mathf.Sin(a)*19),new Vector3(3,6+i%3,4),Mat(desert?"DesertRock":"CoastalRock",desert?new Color(.58f,.36f,.19f):new Color(.35f,.45f,.32f)));
                    }
                }
                EditorSceneManager.SaveScene(s,path);
            }
            finally { EditorSceneManager.CloseScene(s,true); }
        }
    }
    // Re-sync stored runtime paths if a scene is moved/renamed in the Editor.
    public sealed class EnvironmentScenePaths : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] from)
        {
            if(!moved.Any(p=>p.EndsWith(".unity")) && !deleted.Any(p=>p.EndsWith(".unity"))) return;
            EditorApplication.delayCall+=()=> { foreach(var guid in AssetDatabase.FindAssets("t:EnvironmentDefinition")) { var d=AssetDatabase.LoadAssetAtPath<EnvironmentDefinition>(AssetDatabase.GUIDToAssetPath(guid)); d.SyncScene(); EditorUtility.SetDirty(d); } AssetDatabase.SaveAssets(); };
        }
    }
}


using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace GundamLobby.Editor
{
    public static class UIIntegrationAuthoring
    {
        public const string ScenePath="Assets/Game/Scenes/HomeLobby.unity";
        public const string Folder="Assets/Game/Features/MainMenu/Presentation";
        public const string Report="Docs/Reports/UIIntegration";
        static readonly string[] Ids={"Unicorn","Justice","Extreme","StrikeRouge","RX78","RX78Saber"};
        static string Source(string id)
        {
            if(id=="Unicorn")return "Assets/Models/Unicorn Gundam/Demo/Prefabs/UnicornAnimated.prefab";
            string folder=id=="Justice"?"ZGMF-X09A-Justice-Gundam":id=="Extreme"?"EXtreme-Application-ONEXtreme-GUNDAM":id=="StrikeRouge"?"MBF-02-Strike-Rouge-Ootori":id=="RX78"?"RX-78-2-Gundam":"RX-78-2-Gundam-Part-2";
            return AssetDatabase.FindAssets("t:GameObject",new[]{"Assets/Models/"+folder+"/source"}).Select(AssetDatabase.GUIDToAssetPath).First(p=>p.EndsWith(".glb",StringComparison.OrdinalIgnoreCase));
        }
        static string ClipName(string id)=>id=="Justice"?"00-IDLE":id=="Extreme"?"Idle":id=="StrikeRouge"?"O-02-IDLE":id=="RX78"?"G01-RIFLE":"G02-SABER";

        [MenuItem("Gundam/Lobby/Integrate Animated Previews")]
        public static void Integrate()
        {
            try
            {
                Directory.CreateDirectory(Report);Directory.CreateDirectory(Folder+"/Prefabs");
                AssetDatabase.Refresh();
                var scene=EditorSceneManager.OpenScene(ScenePath);
                var catalog=AssetDatabase.LoadAssetAtPath<StrikerCatalog>("Assets/Game/Features/MainMenu/Lobby/Striker/StrikerCatalog.asset");
                if(catalog==null||catalog.units.Length!=10)throw new InvalidOperationException("UI catalog must contain ten Gundam.");
                var report=new List<string>{"UI integration / Unity "+Application.unityVersion,"UI source: Clone/UI / duy / e74c18a","Original Assets/Models, combat, AR packages and global render settings are retained."};
                foreach(var id in Ids)
                {
                    var entry=catalog.units.First(u=>u.id==id);
                    entry.prefab=Build(id,report);
                }
                EditorUtility.SetDirty(catalog);
                var lobby=UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>();
                var inventory=UnityEngine.Object.FindFirstObjectByType<InventoryPanel>();
                var selection=UnityEngine.Object.FindFirstObjectByType<StrikerSelectionController>();
                if(lobby==null||inventory==null||selection==null)throw new InvalidOperationException("The imported scene needs lobby, inventory and Striker controllers.");
                var unicorn=catalog.units.First(u=>u.id=="Unicorn").prefab;
                lobby.gundamPrefab=unicorn;inventory.modelPrefab=unicorn;
                selection.catalog=catalog;
                // Hide the member's editor-only frozen example so it cannot overlap runtime models.
                foreach(var go in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>t.name.Contains("Editor Preview")))go.gameObject.SetActive(false);
                EditorUtility.SetDirty(lobby);EditorUtility.SetDirty(inventory);EditorUtility.SetDirty(selection);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
                // Make the imported home screen the entry point, retaining existing demo scenes.
                var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToList();scenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
                File.WriteAllLines(Report+"/AnimationMapping.txt",report);
                File.WriteAllText(Report+"/AuthoringResult.txt","PASS\n"+DateTime.Now.ToString("s"));
                Debug.Log("Gundam lobby integration ready: six animated preview prefabs, ten catalog entries.");
            }
            catch(Exception e)
            {
                Directory.CreateDirectory(Report);File.WriteAllText(Report+"/AuthoringResult.txt","FAIL\n"+e);Debug.LogException(e);
                if(Application.isBatchMode)EditorApplication.Exit(1);
                else throw;
            }
        }
        static GameObject Build(string id,List<string> report)
        {
            string sourcePath=Source(id);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if(source==null)throw new InvalidOperationException("Missing source "+sourcePath);
            var root=new GameObject(id+" Animated Lobby Preview");
            var geometry=new GameObject("Source rig and geometry").transform;geometry.SetParent(root.transform,false);
            var model=UnityEngine.Object.Instantiate(source,geometry);model.name=source.name;
            try
            {
                foreach(var script in model.GetComponentsInChildren<MonoBehaviour>(true))
                    if(script!=null&&!(script is UnicornDemo.UnicornMotion))UnityEngine.Object.DestroyImmediate(script);
                foreach(var collider in model.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(collider);
                foreach(var a in model.GetComponentsInChildren<Animator>(true)){a.enabled=false;a.runtimeAnimatorController=null;}
                AnimationClip clip=null;
                if(id!="Unicorn")
                {
                    clip=AssetDatabase.LoadAllAssetsAtPath(sourcePath).OfType<AnimationClip>().FirstOrDefault(c=>c.name==ClipName(id));
                    if(clip==null)throw new InvalidOperationException("Missing source clip "+ClipName(id));
                    clip.SampleAnimation(model,Mathf.Min(2,clip.length));
                    var animator=model.GetComponent<Animator>();if(animator==null)animator=model.AddComponent<Animator>();
                    animator.runtimeAnimatorController=null;animator.applyRootMotion=false;
                    var motion=root.AddComponent<LobbyPreviewAnimation>();motion.unitId=id;motion.rig=animator;motion.clip=clip;
                    motion.speed=id.StartsWith("RX78")?.55f:1f;
                }
                // Imported rigs face +Z; the home UI's camera observes from -Z.
                geometry.localRotation=Quaternion.Euler(0,180,0);
                foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode=ShadowCastingMode.On;
                    if(renderer is SkinnedMeshRenderer skin){skin.updateWhenOffscreen=true;var localBounds=skin.localBounds;localBounds.Expand(.2f);skin.localBounds=localBounds;}
                }
                var visible=model.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&!(r is LineRenderer)).ToArray();
                if(visible.Length==0)throw new InvalidOperationException("No renderers in "+id);
                var bounds=visible[0].bounds;foreach(var renderer in visible.Skip(1))bounds.Encapsulate(renderer.bounds);
                float scale=Mathf.Min(8.66f/Mathf.Max(.01f,bounds.size.y),8.8f/Mathf.Max(.01f,bounds.size.x));
                geometry.localScale=Vector3.one*scale;
                geometry.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                string path=Folder+"/Prefabs/"+id+"_AnimatedPreview.prefab";
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);
                report.Add(id+" | "+sourcePath+" | "+(clip!=null?clip.name+" / "+clip.length.ToString("F2")+"s":"existing UnicornMotion idle")+" | "+path);
                return prefab;
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}

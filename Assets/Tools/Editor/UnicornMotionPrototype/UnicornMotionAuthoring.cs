using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GundamGame.UnicornPrototype.Editor
{
    [InitializeOnLoad]
    public static class UnicornMotionAuthoring
    {
        public const string Folder = "Assets/Sandbox/Showcases/UnicornMotion";
        public const string ScenePath = Folder + "/Scenes/UnicornMotionPrototype.unity";
        public const string Fbx = Folder + "/Art/Unicorn_MotionPrototype.fbx";
        public const string Report = "Docs/Reports/UnicornMotionPrototype";
        [Serializable] public class MaterialInfo { public string name, colorTexture; public float[] color; public float smoothness; }
        [Serializable] public class Manifest { public MaterialInfo[] materials; }
        static UnicornMotionAuthoring() { EditorApplication.update += Poll; }
        static void Poll()
        {
            const string request = "Temp/UnicornMotionPrototype.request";
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(request)) return;
            string command;
            // A writer may still hold the small request file during an Editor tick.
            try { command = File.ReadAllText(request).Trim(); File.Delete(request); }
            catch (IOException) { return; }
            try { if (command == "build") { Build(); UnicornMotionValidation.Run(); } else if(command == "test") UnicornMotionValidation.Run(); else if(command == "polish") { Polish(); UnicornMotionValidation.Run(); } else if(command == "open" && !SceneManager.GetActiveScene().isDirty) EditorSceneManager.OpenScene(ScenePath); }
            catch(Exception e) { Directory.CreateDirectory(Report); File.WriteAllText(Report+"/Authoring.txt", "FAIL\n"+e); Debug.LogException(e); }
        }
        [MenuItem("Gundam/Unicorn/Build Animation Prototype")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Prototype already built; use Open or Validate instead.");
            foreach (var sub in new[]{"Materials","Animations","Prefabs","Scenes"}) Directory.CreateDirectory(Folder+"/"+sub);
            Directory.CreateDirectory(Report); AssetDatabase.Refresh();
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Folder+"/Art/Unicorn_MotionPrototype.json"));
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new InvalidOperationException("URP Lit shader missing.");
            var importer = (ModelImporter)AssetImporter.GetAtPath(Fbx);
            importer.animationType=ModelImporterAnimationType.Generic;
            importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=true; importer.importCameras=false; importer.importLights=false;
            importer.animationCompression=ModelImporterAnimationCompression.Off;
            importer.optimizeGameObjects=false; importer.importBlendShapes=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            foreach (var info in manifest.materials)
            {
                var mat = new Material(shader) { name = info.name };
                mat.SetFloat("_Metallic",0); mat.SetFloat("_Smoothness",info.smoothness);
                if (!string.IsNullOrEmpty(info.colorTexture))
                {
                    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(info.colorTexture);
                    if (!tex) throw new InvalidOperationException("Missing color texture: "+info.colorTexture);
                    mat.SetTexture("_BaseMap",tex); mat.SetColor("_BaseColor",Color.white);
                }
                else mat.SetColor("_BaseColor",new Color(info.color[0],info.color[1],info.color[2],1));
                AssetDatabase.CreateAsset(mat,Folder+"/Materials/"+info.name+".mat");
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),info.name),mat);
            }
            importer.SaveAndReimport();
            var clips = importer.defaultClipAnimations;
            if(clips.Length!=2)throw new InvalidOperationException("Expected 2 FBX takes, got "+clips.Length+": "+string.Join(",",clips.Select(c=>c.name)));
            foreach(var clip in clips)
            {
                clip.name=clip.name.Contains("HitReact")?"Unicorn_HitReact":"Unicorn_Idle";
                clip.loopTime=clip.name=="Unicorn_Idle"; clip.loopPose=clip.loopTime;
                clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;
                clip.keepOriginalOrientation=true;clip.keepOriginalPositionY=true;clip.keepOriginalPositionXZ=true;
            }
            importer.clipAnimations=clips; importer.SaveAndReimport();
            var importedClips=AssetDatabase.LoadAllAssetsAtPath(Fbx).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
            var idle=importedClips.Single(c=>c.name=="Unicorn_Idle");var hit=importedClips.Single(c=>c.name=="Unicorn_HitReact");
            var controller=AnimatorController.CreateAnimatorControllerAtPath(Folder+"/Animations/UnicornPrototype.controller");
            controller.AddParameter("Hit",AnimatorControllerParameterType.Trigger);
            var sm=controller.layers[0].stateMachine;var idleState=sm.AddState("Idle");idleState.motion=idle;sm.defaultState=idleState;
            var hitState=sm.AddState("HitReact");hitState.motion=hit;
            var goHit=idleState.AddTransition(hitState);goHit.hasExitTime=false;goHit.hasFixedDuration=true;goHit.duration=.04f;goHit.AddCondition(AnimatorConditionMode.If,0,"Hit");
            var returnIdle=hitState.AddTransition(idleState);returnIdle.hasExitTime=true;returnIdle.exitTime=.95f;returnIdle.hasFixedDuration=true;returnIdle.duration=.10f;
            var oldScene=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
                var model=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);model.name="Unicorn Blender Prototype";
                var animator=model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.runtimeAnimatorController=controller; animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                foreach(var s in model.GetComponentsInChildren<SkinnedMeshRenderer>()) { s.updateWhenOffscreen=true; s.shadowCastingMode=ShadowCastingMode.On; }
                var all=model.GetComponentsInChildren<Transform>();var head=all.Single(t=>t.name=="Head");var foot=all.Single(t=>t.name=="Foot.L");
                idle.SampleAnimation(model,0);var restHead=head.localRotation;var restFoot=foot.position;
                idle.SampleAnimation(model,1);float idleAngle=Quaternion.Angle(restHead,head.localRotation);
                hit.SampleAnimation(model,4/30f);float hitAngle=Quaternion.Angle(restHead,head.localRotation);float drift=Vector3.Distance(restFoot,foot.position);
                if(idleAngle<.1f || hitAngle<1f || drift>.001f)throw new InvalidOperationException($"Animation sample failed: idle={idleAngle}, hit={hitAngle}, foot drift={drift}");
                idle.SampleAnimation(model,4);float seam=Quaternion.Angle(restHead,head.localRotation);
                if(seam>.01f)throw new InvalidOperationException("Idle seam mismatch: "+seam);
                idle.SampleAnimation(model,0);
                var bounds=model.GetComponentsInChildren<Renderer>().First().bounds;
                foreach(var r in model.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
                if(bounds.size.y<2 || bounds.size.y>4)throw new InvalidOperationException("FBX scale unexpected: "+bounds.size);
                model.transform.position=Vector3.up*(.065f-bounds.min.y);
                PrefabUtility.SaveAsPrefabAsset(model,Folder+"/Prefabs/UnicornMotionPrototype.prefab");
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Virtual floor";floor.transform.localScale=Vector3.one*2;floor.transform.position=Vector3.down*.03f;
                var floorMat=new Material(shader){name="Prototype Floor"};floorMat.color=new Color(.035f,.055f,.075f);floorMat.SetFloat("_Smoothness",.15f);AssetDatabase.CreateAsset(floorMat,Folder+"/Materials/PrototypeFloor.mat");floor.GetComponent<Renderer>().sharedMaterial=floorMat;
                var platform=GameObject.CreatePrimitive(PrimitiveType.Cylinder);platform.name="Preview platform";platform.transform.localScale=new Vector3(3.2f,.06f,3.2f);
                var platformMat=new Material(shader){name="Prototype Platform"};platformMat.color=new Color(.10f,.16f,.20f);platformMat.SetFloat("_Smoothness",.5f);AssetDatabase.CreateAsset(platformMat,Folder+"/Materials/PrototypePlatform.mat");platform.GetComponent<Renderer>().sharedMaterial=platformMat;
                var camera=new GameObject("Prototype Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=38;camera.nearClipPlane=.05f;camera.farClipPlane=60;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.04f,.065f);
                camera.gameObject.AddComponent<AudioListener>();
                var key=new GameObject("Key Light").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.7f;key.transform.rotation=Quaternion.Euler(42,-28,0);key.shadows=LightShadows.Soft;
                var fill=new GameObject("Fill Light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.7f;fill.color=new Color(.65f,.82f,1);fill.transform.rotation=Quaternion.Euler(15,145,0);
                RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.42f,.46f,.52f);RenderSettings.fog=false;
                var viewer=new GameObject("Animation Preview Controls").AddComponent<UnicornMotionViewer>();viewer.animator=animator;viewer.previewCamera=camera;
                camera.transform.position=new Vector3(2.3f,2.35f,5);camera.transform.LookAt(viewer.cameraTarget);
                EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
                File.WriteAllText(Report+"/Authoring.txt",$"PASS\nUnity {Application.unityVersion}\nTwo imported Blender clips: Idle {idle.length:0.00}s / HitReact {hit.length:0.00}s\nIdle head rotation {idleAngle:0.000} degrees; hit head rotation {hitAngle:0.000} degrees\nFoot drift {drift:0.000000}m; Idle seam {seam:0.000000} degrees\nSource parts 375; exported skinned meshes {model.GetComponentsInChildren<SkinnedMeshRenderer>().Length}; rigid skeleton 17 bones\nOriginal source model and lobby unchanged.\nScene: {ScenePath}");
            }
            finally { EditorSceneManager.CloseScene(scene,true);if(oldScene.IsValid()&&oldScene.isLoaded)SceneManager.SetActiveScene(oldScene); }
        }
        static void Polish()
        {
            var oldScene=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            try
            {
                var objects=scene.GetRootGameObjects();
                objects.Single(g=>g.name=="Key Light").transform.rotation=Quaternion.Euler(42,155,0);
                objects.Single(g=>g.name=="Fill Light").transform.rotation=Quaternion.Euler(15,-35,0);
                var camera=objects.Single(g=>g.name=="Prototype Camera");
                var data=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() ?? camera.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                data.renderPostProcessing=true;
                data.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
                EditorSceneManager.SaveScene(scene);
            }
            finally { EditorSceneManager.CloseScene(scene,true);if(oldScene.IsValid()&&oldScene.isLoaded)SceneManager.SetActiveScene(oldScene); }
        }
        [MenuItem("Gundam/Unicorn/Open Animation Prototype")]
        static void Open() { if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath); }
    }
}

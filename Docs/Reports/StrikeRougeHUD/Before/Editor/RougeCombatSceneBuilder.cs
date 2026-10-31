using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace GundamGame.CombatDemo.Editor
{
    public static class RougeCombatSceneBuilder
    {
        public const string ScenePath = "Assets/Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity";
        const string Art = "Assets/Sandbox/StrikeRougeCombat/Materials";
        [MenuItem("Gundam/Strike Rouge/Open Combat Demo")]
        public static void OpenDemo()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Gundam/Strike Rouge/Create Combat Demo")]
        public static void CreateFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorUtility.DisplayDialog("Existing demo", "Open the existing demo. Creation does not overwrite it.", "OK"); return; }
            Build();
        }
        public static void Build()
        {
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Demo already exists; refusing to overwrite.");
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)); Directory.CreateDirectory(Art); AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.48f,.53f,.65f);
            var floorMat = Material("Platform",new Color(.035f,.065f,.10f),false);
            var blue = Material("Cyan", new Color(.05f,.65f,.85f),true);
            var pink = Material("Rose",new Color(.95f,.15f,.4f),true);
            var gridMat = Material("Grid",new Color(.06f,.18f,.24f),true);
            var arena = new GameObject("Battlefield").transform;
            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name="Virtual tactical platform";ground.transform.SetParent(arena,false);ground.transform.localPosition=new Vector3(0,-.12f,0);ground.transform.localScale=new Vector3(11,.2f,8);ground.GetComponent<Renderer>().sharedMaterial=floorMat;
            for(int x=-5;x<=5;x++) Line(arena,"Grid",new[]{new Vector3(x,0,-4),new Vector3(x,0,4)},gridMat,.015f);
            for(int z=-4;z<=4;z++) Line(arena,"Grid",new[]{new Vector3(-5.5f,0,z),new Vector3(5.5f,0,z)},gridMat,.015f);
            Line(arena,"Arena boundary",new[]{new Vector3(-5.5f,.01f,-4),new Vector3(5.5f,.01f,-4),new Vector3(5.5f,.01f,4),new Vector3(-5.5f,.01f,4),new Vector3(-5.5f,.01f,-4)},blue,.035f);
            var player=Fighter(arena,"Strike Rouge Ootori","MBF-02-Strike-Rouge-Ootori",new Vector3(-1,0,-2.3f),0,400,2.1f,pink);
            var enemies=new[]{Fighter(arena,"RX-78-2","RX-78-2-Gundam",new Vector3(-3.2f,0,1.8f),180,180,1.9f,blue),Fighter(arena,"Justice","ZGMF-X09A-Justice-Gundam",new Vector3(0,0,2.6f),180,200,2f,blue),Fighter(arena,"EXtreme","EXtreme-Application-ONEXtreme-GUNDAM",new Vector3(3.2f,0,1.8f),180,210,2.1f,blue)};
            var cam=new GameObject("Virtual Battle Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";cam.transform.position=new Vector3(8.2f,8,-12.8f);cam.transform.LookAt(new Vector3(0,.5f,.4f));cam.fieldOfView=43;cam.nearClipPlane=.03f;cam.farClipPlane=150;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.015f,.025f,.045f);cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var key=new GameObject("Key Light").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.6f;key.transform.rotation=Quaternion.Euler(40,-40,0);key.shadows=LightShadows.Soft;
            var fill=new GameObject("Fill Light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.65f;fill.color=new Color(.55f,.7f,1);fill.transform.rotation=Quaternion.Euler(30,130,0);
            var controller=new GameObject("Battle Controller").AddComponent<RougeBattleDemo>(); controller.arenaRoot=arena;controller.player=player;controller.enemies=enemies;
            var session=new GameObject("AR Session").AddComponent<ARSession>();session.enabled=false;
            var xr=new GameObject("AR Origin");xr.SetActive(false);var origin=xr.AddComponent<XROrigin>();
            var offset=new GameObject("Camera Offset");offset.transform.SetParent(xr.transform,false);origin.CameraFloorOffsetObject=offset;
            var ar=new GameObject("AR Camera",typeof(Camera),typeof(AudioListener));ar.transform.SetParent(offset.transform,false);var arCamera=ar.GetComponent<Camera>();arCamera.nearClipPlane=.01f;arCamera.farClipPlane=50;origin.Camera=arCamera;ar.AddComponent<UniversalAdditionalCameraData>();ar.AddComponent<ARCameraManager>();ar.AddComponent<ARCameraBackground>();
            var pose=ar.AddComponent<TrackedPoseDriver>();pose.positionInput=new InputActionProperty(new InputAction("AR Position",InputActionType.Value,"<XRHMD>/centerEyePosition"));pose.rotationInput=new InputActionProperty(new InputAction("AR Rotation",InputActionType.Value,"<XRHMD>/centerEyeRotation"));pose.trackingStateInput=new InputActionProperty(new InputAction("AR Tracking",InputActionType.Value,"<XRHMD>/trackingState"));
            var planeManager=xr.AddComponent<ARPlaneManager>();planeManager.requestedDetectionMode=PlaneDetectionMode.Horizontal;var raycasts=xr.AddComponent<ARRaycastManager>();var anchors=xr.AddComponent<ARAnchorManager>();
            var placement=new GameObject("Arena Placement").AddComponent<RougeArenaPlacement>();placement.arenaRoot=arena;placement.virtualCamera=cam;placement.arCamera=arCamera;placement.xrRoot=xr;placement.session=session;placement.planes=planeManager;placement.raycasts=raycasts;placement.anchors=anchors;placement.battleController=controller;
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();Selection.activeGameObject=controller.gameObject;
            if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.LookAt(new Vector3(0,1,0),cam.transform.rotation,12);
            Debug.Log("Strike Rouge combat demo ready: "+ScenePath);
        }
        static DemoFighter Fighter(Transform arena,string label,string folder,Vector3 position,float yaw,int hp,float height,Material marker)
        {
            var root=new GameObject(label);root.transform.SetParent(arena,false);
            var visual=new GameObject("Animated Visual").transform;visual.SetParent(root.transform,false);
            var path=AssetDatabase.FindAssets("t:GameObject",new[]{"Assets/Models/"+folder}).Select(AssetDatabase.GUIDToAssetPath).First(p=>p.EndsWith(".glb",StringComparison.OrdinalIgnoreCase));
            var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),visual);model.name="Model Instance";
            foreach(var animator in model.GetComponentsInChildren<Animator>(true)) animator.enabled=false;
            var renderers=model.GetComponentsInChildren<Renderer>(true);Bounds bounds=renderers[0].bounds;foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            var bones=model.GetComponentsInChildren<Transform>(true);var hips=bones.FirstOrDefault(t=>t.name.StartsWith("hips",StringComparison.OrdinalIgnoreCase));
            float scale=height/Mathf.Max(bounds.size.y,.01f);var center=hips!=null?hips.position:bounds.center;
            model.transform.localScale*=scale;model.transform.localPosition-=new Vector3(center.x,bounds.min.y,center.z)*scale;
            var fighter=root.AddComponent<DemoFighter>();fighter.displayName=label;fighter.visual=visual;fighter.maxHealth=hp;fighter.height=height;fighter.accent=marker.color;
            fighter.rightArm=bones.FirstOrDefault(t=>t.name.StartsWith("upper_arm.R",StringComparison.OrdinalIgnoreCase));fighter.leftArm=bones.FirstOrDefault(t=>t.name.StartsWith("upper_arm.L",StringComparison.OrdinalIgnoreCase));fighter.torso=bones.FirstOrDefault(t=>t.name.StartsWith("chest",StringComparison.OrdinalIgnoreCase));
            var hand=bones.FirstOrDefault(t=>t.name.StartsWith("hand.R",StringComparison.OrdinalIgnoreCase));if(hand!=null)fighter.muzzle=hand;
            root.transform.localPosition=position;root.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var points=Enumerable.Range(0,49).Select(i=>new Vector3(Mathf.Cos(i*Mathf.PI/24)*.7f,.015f,Mathf.Sin(i*Mathf.PI/24)*.7f)).ToArray();Line(root.transform,"Home slot",points,marker,.025f);
            return fighter;
        }
        static Material Material(string name,Color color,bool unlit)
        {
            string path=Art+"/"+name+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null)return existing;
            var mat=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));mat.color=color;AssetDatabase.CreateAsset(mat,path);return mat;
        }
        static void Line(Transform parent,string name,Vector3[] points,Material mat,float width)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=line.endWidth=width;line.sharedMaterial=mat;line.numCapVertices=2;}
    }
}

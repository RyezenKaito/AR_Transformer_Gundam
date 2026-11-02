using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TurnBasedBattle;
using ProjectEnvironments;
using ProjectEnvironments.Editor;

namespace WarehouseArena.Editor
{
    public static class WarehouseBattleBuilder
    {
        public const string RootPath="Assets/Environments/Warehouse";
        public const string ScenePath=RootPath+"/Scenes/Warehouse_Battle.unity";
        const string Kit="Assets/SciFi Warehouse Kit";
        const string SourceBattle="Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity";
        static readonly Dictionary<string,Material> replacements=new Dictionary<string,Material>();
        static readonly Dictionary<string,string> prefabs=new Dictionary<string,string>();
        static Transform environment;
        static Material yellow,dark,cyan,signMaterial;
        static Transform Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        static Material Simple(string name,Color color,bool emission=false)
        {
            string path=RootPath+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.32f);m.SetFloat("_Metallic",.15f);
            if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*2.5f);}EditorUtility.SetDirty(m);return m;
        }
        public static void PrepareMaterials()
        {
            replacements.Clear();
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Kit+"/Art/Materials"}))
            {
                var original=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(original.shader.name.StartsWith("Skybox/"))continue;
                string path=RootPath+"/Materials/"+original.name+"_URP.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
                m.shader=Shader.Find("Universal Render Pipeline/Lit");m.shaderKeywords=Array.Empty<string>();m.enableInstancing=true;
                m.SetColor("_BaseColor",original.HasProperty("_Color")?original.GetColor("_Color"):Color.white);
                foreach(var pair in new[]{new[]{"_MainTex","_BaseMap"},new[]{"_BumpMap","_BumpMap"},new[]{"_MetallicGlossMap","_MetallicGlossMap"},new[]{"_OcclusionMap","_OcclusionMap"},new[]{"_EmissionMap","_EmissionMap"}})
                {
                    if(!original.HasProperty(pair[0]))continue;
                    m.SetTexture(pair[1],original.GetTexture(pair[0]));m.SetTextureScale(pair[1],original.GetTextureScale(pair[0]));m.SetTextureOffset(pair[1],original.GetTextureOffset(pair[0]));
                }
                m.SetFloat("_Metallic",original.HasProperty("_Metallic")?original.GetFloat("_Metallic"):.5f);
                m.SetFloat("_Smoothness",original.HasProperty("_Glossiness")?original.GetFloat("_Glossiness"):.4f);
                if(m.GetTexture("_BumpMap")){m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",original.GetFloat("_BumpScale"));}
                if(m.GetTexture("_MetallicGlossMap")){m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Smoothness",original.GetFloat("_GlossMapScale"));}
                if(m.GetTexture("_OcclusionMap")){m.EnableKeyword("_OCCLUSIONMAP");m.SetFloat("_OcclusionStrength",original.GetFloat("_OcclusionStrength"));}
                var emission=original.HasProperty("_EmissionColor")?original.GetColor("_EmissionColor"):Color.black;
                if(emission.maxColorComponent>0 || m.GetTexture("_EmissionMap")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission*2.5f);}
                float mode=original.HasProperty("_Mode")?original.GetFloat("_Mode"):0;
                m.SetFloat("_Surface",mode>=2?1:0);m.SetFloat("_AlphaClip",mode==1?1:0);m.SetFloat("_ZWrite",mode>=2?0:1);m.SetFloat("_Cull",mode==1?0:2);
                m.SetFloat("_SrcBlend",mode>=2?(int)BlendMode.SrcAlpha:(int)BlendMode.One);m.SetFloat("_DstBlend",mode>=2?(int)BlendMode.OneMinusSrcAlpha:(int)BlendMode.Zero);
                if(mode==1){m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",original.GetFloat("_Cutoff"));m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");}
                else if(mode>=2){m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;m.SetOverrideTag("RenderType","Transparent");}
                else {m.renderQueue=2000;m.SetOverrideTag("RenderType","Opaque");}
                EditorUtility.SetDirty(m);replacements[original.name]=m;
            }
            yellow=Simple("Hazard Yellow",new Color(.92f,.62f,.06f));dark=Simple("Painted Graphite",new Color(.035f,.055f,.07f));cyan=Simple("Cyan Inlay",new Color(.05f,.68f,.94f),true);
            signMaterial=AssetDatabase.LoadAssetAtPath<Material>(RootPath+"/Materials/Signage.mat");if(!signMaterial){signMaterial=new Material(Shader.Find("WarehouseArena/Depth Tested Sign"));AssetDatabase.CreateAsset(signMaterial,RootPath+"/Materials/Signage.mat");}
            AssetDatabase.SaveAssets();
        }
        static GameObject Module(string name,Transform parent,Vector3 center,float bottom=0,float yaw=0,bool cameraBlocker=true)
        {
            if(!prefabs.TryGetValue(name,out string path))throw new Exception("Missing Warehouse prefab: "+name);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=(GameObject)PrefabUtility.InstantiatePrefab(source,parent);go.name=name;
            go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one;
            // Several packaged variations have scene-authoring offsets in the prefab root.
            foreach(var script in go.GetComponentsInChildren<MonoBehaviour>(true))UnityEngine.Object.DestroyImmediate(script);
            foreach(var camera in go.GetComponentsInChildren<Camera>(true))UnityEngine.Object.DestroyImmediate(camera);
            foreach(var audio in go.GetComponentsInChildren<AudioSource>(true))UnityEngine.Object.DestroyImmediate(audio);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.sharedMaterials=r.sharedMaterials.Select(m=>m && replacements.TryGetValue(m.name,out var replacement)?replacement:m).ToArray();
            var filters=go.GetComponentsInChildren<MeshFilter>(true);var bounds=new Bounds();bool first=true;
            foreach(var mf in filters)
            {
                var b=mf.sharedMesh.bounds;
                for(int i=0;i<8;i++){var v=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));v=parent.InverseTransformPoint(mf.transform.TransformPoint(v));if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);}
            }
            go.transform.localPosition+=new Vector3(center.x-bounds.center.x,bottom-bounds.min.y,center.z-bounds.center.z);
            if(cameraBlocker)foreach(var t in go.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            return go;
        }
        static GameObject Box(string name,Transform parent,Vector3 center,Vector3 size,Material mat,bool collision=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=center;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static void Sign(string text,Transform parent,Vector3 position,float size,Quaternion rotation,Color color)
        {
            var go=new GameObject(text);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localRotation=rotation;
            var label=go.AddComponent<TextMesh>();label.text=text;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.color=color;go.GetComponent<MeshRenderer>().sharedMaterial=signMaterial;go.AddComponent<WarehouseFontAtlas>();
        }
        static void Architecture()
        {
            var floor=Group("01_Floor_48x42",environment);
            for(int x=0;x<8;x++)for(int z=0;z<7;z++)Module("Floor Tile "+((x+z)%4+1).ToString("D2"),floor,new Vector3(-21+x*6,0,-18+z*6),-.25f,0,false);
            var collider=new GameObject("Continuous battle floor");collider.transform.SetParent(floor,false);collider.transform.localPosition=new Vector3(0,-.3f,0);collider.AddComponent<BoxCollider>().size=new Vector3(48,.5f,42);
            var walls=Group("02_WallsAndDockDoors",environment);
            for(int z=0;z<7;z++)foreach(int side in new[]{-1,1})
            {
                float pz=-18+z*6;
                Module(z==3?"Wall BayDoor":z%3==0?"Wall Fan":"Wall Plain",walls,new Vector3(side*24,0,pz),-1,side==1?180:0);
                Module(z%2==0?"Wall Window":"Wall Plain",walls,new Vector3(side*24,0,pz),9,side==1?180:0);
            }
            for(int x=0;x<8;x++)foreach(int side in new[]{-1,1})
            {
                float px=-21+x*6;
                Module((x==3||x==4)?"Wall BayDoor":x==0?"Wall Door":"Wall Plain",walls,new Vector3(px,0,side*21),-1,side==1?90:-90);
                Module(x%2==0?"Wall Window":"Wall Plain",walls,new Vector3(px,0,side*21),9,side==1?90:-90);
            }
            var roof=Group("03_RoofAndBeams",environment);
            for(int x=0;x<8;x++)for(int z=0;z<7;z++)Module((x==2||x==5)&&z%2==1?"Ceiling Skylight":"Ceiling Closed",roof,new Vector3(-21+x*6,0,-18+z*6),18,0,true);
            foreach(int side in new[]{-1,1})for(int i=0;i<8;i++)
            {
                Module("Pillar",walls,new Vector3(-21+i*6,0,side*20.8f),0);
                Module("Pillar",walls,new Vector3(-21+i*6,0,side*20.8f),9);
            }
            for(int x=0;x<8;x++)for(int z=0;z<4;z++)Module("Ceiling Support",roof,new Vector3(-21+x*6,0,-18+z*12),17.5f,0);
            var services=Group("04_OverheadServices",environment);
            for(int z=0;z<7;z++)foreach(int side in new[]{-1,1})Module("Duct Straight",services,new Vector3(side*17,0,-18+z*6),14.5f,0);
            for(int x=0;x<6;x++)Module("Duct Straight",services,new Vector3(-15+x*6,0,16),14.5f,90);
            Sign("DOCK 09",walls,new Vector3(0,8,20.6f),.38f,Quaternion.identity,new Color(.95f,.75f,.27f));
            Sign("CARGO // TRANSFER",walls,new Vector3(0,5.8f,20.55f),.11f,Quaternion.identity,new Color(.7f,.8f,.83f));
        }
        static void CatwalksAndCargo()
        {
            var walk=Group("05_YellowCatwalks",environment);
            foreach(int side in new[]{-1,1})for(int z=0;z<6;z++)Module("Catwalk Long Rails",walk,new Vector3(side*21,0,-15+z*6),0,side==1?180:0);
            for(int x=0;x<6;x++)Module("Catwalk Long Rails",walk,new Vector3(-15+x*6,0,18),0,90);
            Module("Catwalk Stairs",walk,new Vector3(17,0,-14),0,0);Module("Catwalk Stairs",walk,new Vector3(-17,0,10),0,180);
            var cargo=Group("06_CargoPerimeter",environment);
            foreach(int side in new[]{-1,1})for(int i=0;i<4;i++)
            {
                float x=-15+i*10;
                Module("Shelf Variation "+(i%6+1).ToString("D2"),cargo,new Vector3(x,0,side*15.7f),0,side==1?0:180);
                Module("Pallet Variation "+(i%9+1).ToString("D2"),cargo,new Vector3(x+3,0,side*13.7f),0,(i%2)*90);
                Module("Barrel",cargo,new Vector3(x+4.2f,0,side*14.9f),0,0);
                if(i%2==0)Module("Crate Long",cargo,new Vector3(x+1.5f,0,side*12.8f),0,90);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Module("Cart",cargo,new Vector3(side*20,0,1),0,90);
                Module("Garbage Bin",cargo,new Vector3(side*22,0,-5),0,0);
                Module("Fire Extinguisher",cargo,new Vector3(side*23,0,-6),1,side==1?90:-90);
                Module("Security Cam",cargo,new Vector3(side*22.5f,0,-16),7,side==1?90:-90);
            }
            var markings=Group("07_ClearCombatZone",environment);
            foreach(int side in new[]{-1,1})
            {
                Box("Yellow zone boundary",markings,new Vector3(side*12.8f,.018f,0),new Vector3(.1f,.015f,20),yellow);
                Box("Yellow zone boundary",markings,new Vector3(0,.018f,side*10),new Vector3(25.6f,.015f,.1f),yellow);
                for(int i=0;i<6;i++)Box("Floor cyan inset",markings,new Vector3(-10+i*4,.02f,side*9.65f),new Vector3(.6f,.02f,.07f),cyan);
            }
            Sign("COMBAT SECTOR 09",markings,new Vector3(0,.04f,-8),.13f,Quaternion.Euler(90,0,0),new Color(.55f,.64f,.66f));
        }
        static void LightScene()
        {
            var group=Group("08_Lighting",environment);
            var sun=new GameObject("Skylight Sun").AddComponent<Light>();sun.transform.SetParent(group,false);sun.type=LightType.Directional;sun.intensity=1.3f;sun.color=new Color(.78f,.87f,1);sun.transform.rotation=Quaternion.Euler(58,-24,0);sun.shadows=LightShadows.Soft;RenderSettings.sun=sun;
            RenderSettings.skybox=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Art/Materials/Dune Skybox.mat");RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.22f,.28f,.34f);RenderSettings.ambientEquatorColor=new Color(.16f,.19f,.22f);RenderSettings.ambientGroundColor=new Color(.1f,.11f,.12f);RenderSettings.fog=false;
            for(int x=0;x<4;x++)for(int z=0;z<3;z++)
            {
                var pos=new Vector3(-18+x*12,16.6f,-14+z*14);Module("Hanging Light",group,pos,16.4f,0,false);
                var lamp=new GameObject("Overhead warehouse spotlight").AddComponent<Light>();lamp.transform.SetParent(group,false);lamp.transform.localPosition=pos;lamp.transform.rotation=Quaternion.Euler(90,0,0);lamp.type=LightType.Spot;lamp.spotAngle=105;lamp.innerSpotAngle=65;lamp.range=28;lamp.intensity=150;lamp.color=z%2==0?new Color(1,.9f,.72f):new Color(.75f,.87f,1);lamp.shadows=(x==1||x==2)&&z==1?LightShadows.Soft:LightShadows.None;
            }
            foreach(int side in new[]{-1,1})for(int i=0;i<7;i++)
            {
                Module("Wall Light",group,new Vector3(side*23.65f,0,-18+i*6),7.1f,side==1?90:-90,false);
                var lamp=new GameObject("Wall light bounce").AddComponent<Light>();lamp.transform.SetParent(group,false);lamp.transform.localPosition=new Vector3(side*22.8f,6.8f,-18+i*6);lamp.type=LightType.Point;lamp.range=9;lamp.intensity=5;lamp.color=new Color(.7f,.83f,1);lamp.shadows=LightShadows.None;
            }
            var probe=new GameObject("Warehouse reflection").AddComponent<ReflectionProbe>();probe.transform.SetParent(group,false);probe.transform.localPosition=new Vector3(0,5,0);probe.size=new Vector3(48,20,42);probe.boxProjection=true;probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=256;probe.intensity=.8f;
            string path=RootPath+"/Lighting/WarehouseVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.25f);bloom.threshold.Override(1.1f);AssetDatabase.AddObjectToAsset(bloom,profile);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(.65f);AssetDatabase.AddObjectToAsset(color,profile);}
            var volume=new GameObject("Warehouse post processing").AddComponent<Volume>();volume.transform.SetParent(group,false);volume.isGlobal=true;volume.priority=5;volume.sharedProfile=profile;
        }
        [MenuItem("Tools/Warehouse Arena/Build Battle Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");
            var current=SceneManager.GetActiveScene();if(current.isDirty)throw new Exception("Save the current scene before building a new battlefield.");
            foreach(string dir in new[]{"Scenes","Materials","Lighting","Prefabs","Documentation/Review"})Directory.CreateDirectory(RootPath+"/"+dir);AssetDatabase.Refresh();
            if(current.path==ScenePath)EditorSceneManager.OpenScene(EnvironmentTools.Hub);
            if(File.Exists(ScenePath)){Directory.CreateDirectory("Backups/Warehouse");File.Copy(ScenePath,"Backups/Warehouse/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);}
            File.Copy(SourceBattle,ScenePath,true);AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);
            var scene=EditorSceneManager.OpenScene(ScenePath);PrepareMaterials();prefabs.Clear();
            foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{Kit+"/Prefabs"})){string path=AssetDatabase.GUIDToAssetPath(guid);prefabs[Path.GetFileNameWithoutExtension(path)]=path;}
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;
            foreach(string name in new[]{"MegaKitHangar","SphereCityArena"}){var t=board.Find(name);if(t)t.gameObject.SetActive(false);}
            var stage=board.Find("BattleArena");if(stage){foreach(var r in stage.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in stage.GetComponentsInChildren<Collider>(true))c.enabled=false;}
            foreach(string name in new[]{"Key Light","Fill Light","Battle Post Processing"}){var go=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==name);if(go)go.SetActive(false);}
            environment=Group("Warehouse_Sector09",board);Architecture();CatwalksAndCargo();LightScene();
            var switcher=battle.GetComponent<BattleEnvironmentSwitcher>();switcher.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="WAREHOUSE · SECTOR 09",root=environment.gameObject,ambient=new Color(.22f,.28f,.34f),background=new Color(.025f,.035f,.05f)}};switcher.trainingRenderers=Array.Empty<MeshRenderer>();switcher.Apply(0);if(switcher.nextButton)switcher.nextButton.gameObject.SetActive(false);
            battle.initialEnemies=1;var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.homeYaw=72;orbit.homePitch=16;orbit.homeDistance=8;orbit.ResetView(true);battle.battleCamera.useOcclusionCulling=false;battle.battleCamera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var views=Group("09_ReviewCameras",environment);
            for(int i=0;i<3;i++){var camera=new GameObject(new[]{"Warehouse_Overview","Warehouse_Catwalk","Warehouse_Floor"}[i]).AddComponent<Camera>();camera.transform.SetParent(views,false);camera.transform.localPosition=new[]{new Vector3(20,12,-17),new Vector3(-19,6,-17),new Vector3(-15,2.2f,-12)}[i];camera.transform.LookAt(new[]{new Vector3(0,3,3),new Vector3(2,4,13),new Vector3(0,3,8)}[i]);camera.fieldOfView=i==0?65:62;camera.nearClipPlane=.1f;camera.farClipPlane=150;camera.useOcclusionCulling=false;camera.enabled=false;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
            PrefabUtility.SaveAsPrefabAsset(environment.gameObject,RootPath+"/Prefabs/Warehouse_Sector09.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Register();Capture();Debug.Log("WAREHOUSE_BATTLE_READY: "+ScenePath);
        }
        public static void Register()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var d=catalog.Find("warehouse-sector09");
            if(!d){d=ScriptableObject.CreateInstance<EnvironmentDefinition>();d.id="warehouse-sector09";AssetDatabase.CreateAsset(d,RootPath+"/WarehouseDefinition.asset");catalog.environments=catalog.environments.Concat(new[]{d}).ToArray();EditorUtility.SetDirty(catalog);}
            d.displayName="WAREHOUSE · SECTOR 09";d.description="Kho công nghiệp · cầu trên cao · trận robot 1v1 / 1v3.";d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);d.battleVariant=-1;d.available=true;d.accent=new Color(.75f,.53f,.16f);d.SyncScene();EditorUtility.SetDirty(d);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/Warehouse Arena/Open Battle Scene")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Tools/Warehouse Arena/Capture Review")]
        public static void Capture()
        {
            var cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include).Where(c=>c.name.StartsWith("Warehouse_")).ToArray();
            foreach(var c in cameras)
            {
                var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(RootPath+"/Documentation/Review/"+c.name+".png",tex.EncodeToPNG());}
                finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
            }
            AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("warehouse-sector09");d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(RootPath+"/Documentation/Review/Warehouse_Overview.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();
        }
    }
}

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

namespace StreetCityArena.Editor
{
    public static class StreetCityBuilder
    {
        public const string Root="Assets/Environments/StreetCity";
        public const string ScenePath=Root+"/Scenes/StreetCity_Battle.unity";
        const string Data=Root+"/ImportData";
        [Serializable] class MeshEntry {public string name,file;public int material,node,group,vertices,triangles;public float[] min,max;}
        [Serializable] class GroupEntry {public int id;public string name;public float[] min,max;}
        [Serializable] class MaterialEntry {public string name,baseMap,metalRough,alphaMode;public float[] color;public float metallic,roughness,alphaCutoff;public bool doubleSided;}
        [Serializable] class Manifest {public MeshEntry[] meshes;public GroupEntry[] groups;public MaterialEntry[] materials;}
        static Vector3 Vec(float[] x)=>new Vector3(x[0],x[1],x[2]);
        static Transform Group(string name,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
        static Mesh ReadMesh(string file)
        {
            using(var reader=new BinaryReader(File.OpenRead(Data+"/"+file)))
            {
                int count=reader.ReadInt32(),indices=reader.ReadInt32();var v=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];var t=new int[indices];
                for(int i=0;i<count;i++)v[i]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                for(int i=0;i<count;i++)n[i]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                for(int i=0;i<count;i++)uv[i]=new Vector2(reader.ReadSingle(),reader.ReadSingle());
                for(int i=0;i<indices;i++)t[i]=reader.ReadInt32();
                var mesh=new Mesh{indexFormat=IndexFormat.UInt32,name=Path.GetFileNameWithoutExtension(file)};mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.triangles=t;mesh.RecalculateBounds();return mesh;
            }
        }
        static Mesh Persist(Mesh mesh,string name)
        {
            string path=Root+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!m){mesh.name=name;AssetDatabase.CreateAsset(mesh,path);return mesh;}
            m.Clear();m.indexFormat=mesh.indexFormat;m.vertices=mesh.vertices;m.normals=mesh.normals;m.uv=mesh.uv;m.triangles=mesh.triangles;m.RecalculateBounds();m.UploadMeshData(false);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(m);return m;
        }
        static Material Basic(string name,Color color,float smoothness=.3f)
        {
            string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smoothness);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
        }
        static Material[] ImportMaterials(Manifest manifest)
        {
            var materials=new List<Material>();
            foreach(var d in manifest.materials)
            {
                var m=Basic(d.name,new Color(d.color[0],d.color[1],d.color[2],d.color[3]),1-d.roughness);m.SetFloat("_Metallic",d.metallic);m.SetFloat("_Cull",d.doubleSided?0:2);
                if(!string.IsNullOrEmpty(d.baseMap))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Data+"/"+d.baseMap));
                if(!string.IsNullOrEmpty(d.metalRough))
                {
                    string packedPath=Root+"/Textures/"+d.name+"_MetalSmooth.png";
                    if(!File.Exists(packedPath))
                    {
                        string input=Data+"/"+d.metalRough;var ti=(TextureImporter)AssetImporter.GetAtPath(input);ti.sRGBTexture=false;ti.isReadable=true;ti.SaveAndReimport();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(input);var colors=texture.GetPixels();
                        for(int i=0;i<colors.Length;i++)colors[i]=new Color(colors[i].b*d.metallic,0,0,1-colors[i].g*d.roughness);
                        var packed=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false,true);packed.SetPixels(colors);packed.Apply();File.WriteAllBytes(packedPath,packed.EncodeToPNG());UnityEngine.Object.DestroyImmediate(packed);AssetDatabase.ImportAsset(packedPath);var target=(TextureImporter)AssetImporter.GetAtPath(packedPath);target.sRGBTexture=false;target.SaveAndReimport();
                    }
                    m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath));m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Metallic",1);m.SetFloat("_Smoothness",1);
                }
                materials.Add(m);
            }
            return materials.ToArray();
        }
        static GameObject ImportModel(Manifest manifest,Material[] materials)
        {
            var model=Group("Street City Buildings 8 — Imported GLB");var groups=new Dictionary<int,Transform>();
            foreach(var g in manifest.groups)
            {
                var pivot=Group(g.name+" ["+g.id+"]",model);var center=(Vec(g.min)+Vec(g.max))*.5f;center.y=g.min[1];pivot.localPosition=center;groups[g.id]=pivot;
            }
            foreach(var e in manifest.meshes)
            {
                var mesh=ReadMesh(e.file);var p=groups[e.group];var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]-=p.localPosition;mesh.vertices=vertices;mesh.RecalculateBounds();mesh=Persist(mesh,Path.GetFileNameWithoutExtension(e.file));
                var go=new GameObject(e.name);go.transform.SetParent(p,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=materials[e.material];
            }
            return model.gameObject;
        }
        static GameObject Box(string name,Transform parent,Vector3 center,Vector3 size,Material mat,bool collide=true)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=center;g.transform.localScale=size;g.GetComponent<MeshRenderer>().sharedMaterial=mat;if(!collide)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        static Material Asphalt(float length)
        {
            string path=Root+"/Textures/Asphalt.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(!texture)
            {
                texture=new Texture2D(256,256,TextureFormat.RGBA32,true);var pixels=new Color[65536];var random=new System.Random(19);
                for(int y=0;y<256;y++)for(int x=0;x<256;x++){float n=Mathf.PerlinNoise(x*.055f,y*.055f)*.032f+(float)random.NextDouble()*.035f;pixels[y*256+x]=new Color(.10f+n,.115f+n,.13f+n);}
                texture.SetPixels(pixels);texture.wrapMode=TextureWrapMode.Repeat;texture.Apply();AssetDatabase.CreateAsset(texture,path);
            }
            texture.anisoLevel=8;texture.filterMode=FilterMode.Trilinear;EditorUtility.SetDirty(texture);
            var m=Basic("Asphalt",Color.white,.18f);m.SetTexture("_BaseMap",texture);m.SetTextureScale("_BaseMap",new Vector2(length/4,18/4f));return m;
        }
        static Bounds BoundsOf(Transform t)
        {var rs=t.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
        static float LayoutBuildings(GameObject imported,Transform environment,Manifest manifest)
        {
            imported.transform.SetParent(environment,false);imported.name="01_OriginalBuildingMeshes";
            float[] cursor={0,0};var placed=new List<(Transform t,float width)>();int count=0;
            foreach(var d in manifest.groups)
            {
                var group=imported.transform.Find(d.name+" ["+d.id+"]");var dimensions=Vec(d.max)-Vec(d.min);
                if(dimensions.y<.1f){group.gameObject.SetActive(false);continue;}
                int side=count%2;float yaw=dimensions.z>dimensions.x?90:0;yaw+=side==0?0:180;
                group.localPosition=Vector3.zero;group.localScale=Vector3.one*4;group.localRotation=Quaternion.Euler(0,yaw,0);
                var b=BoundsOf(group);float width=b.size.x;float z=(side==0?1:-1)*(12+b.size.z*.5f);
                group.position+=new Vector3(cursor[side]+width*.5f-b.center.x,-b.min.y,z-b.center.z);cursor[side]+=width+1.8f;placed.Add((group,width));
                foreach(var t in group.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
                var collision=group.gameObject.AddComponent<BoxCollider>();var localBounds=new Bounds();bool first=true;
                foreach(var mf in group.GetComponentsInChildren<MeshFilter>())
                {var mb=mf.sharedMesh.bounds;for(int i=0;i<8;i++){var p=group.InverseTransformPoint(mf.transform.TransformPoint(mb.center+Vector3.Scale(mb.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));if(first){localBounds=new Bounds(p,Vector3.zero);first=false;}else localBounds.Encapsulate(p);}}
                collision.center=localBounds.center;collision.size=localBounds.size;
                count++;
            }
            float length=Mathf.Max(cursor[0],cursor[1])+20;
            foreach(var e in placed)e.t.position-=Vector3.right*(length-20)*.5f;
            return length;
        }
        static void Street(Transform environment,float length)
        {
            var road=Group("02_Avenue",environment);var asphalt=Asphalt(length);var concrete=Basic("Sidewalk concrete",new Color(.44f,.46f,.46f));var white=Basic("White road paint",new Color(.82f,.83f,.77f));var yellow=Basic("Yellow road paint",new Color(.94f,.67f,.16f));var metal=Basic("Street metal",new Color(.055f,.075f,.09f),.5f);
            Box("Continuous street floor",road,new Vector3(0,-.2f,0),new Vector3(length,.4f,18),asphalt);
            foreach(int side in new[]{-1,1})
            {
                Box("Raised sidewalk",road,new Vector3(0,.04f,side*10.5f),new Vector3(length,.32f,3),concrete);
                Box("Curb",road,new Vector3(0,.15f,side*9.08f),new Vector3(length,.3f,.22f),concrete);
                Box("Road edge stripe",road,new Vector3(0,.012f,side*8.45f),new Vector3(length,.018f,.1f),white,false);
                for(float x=-length*.5f+5;x<length*.5f;x+=15)
                {
                    Box("Sidewalk joint",road,new Vector3(x,.207f,side*10.5f),new Vector3(.025f,.01f,2.75f),metal,false);
                    var pole=Group("Streetlight",road);pole.localPosition=new Vector3(x,.2f,side*11);
                    var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Lamp post";g.transform.SetParent(pole,false);g.transform.localPosition=new Vector3(0,3.9f,0);g.transform.localScale=new Vector3(.14f,3.9f,.14f);g.GetComponent<Renderer>().sharedMaterial=metal;g.layer=2;
                    Box("Lamp arm",pole,new Vector3(0,7.8f,-side*.8f),new Vector3(.15f,.13f,1.8f),metal);
                    Box("Lamp head",pole,new Vector3(0,7.75f,-side*1.7f),new Vector3(.55f,.12f,.9f),white);
                }
            }
            for(float x=-length*.5f+3;x<length*.5f;x+=7){Box("Center dashed marking",road,new Vector3(x,.015f,0),new Vector3(3.5f,.02f,.12f),yellow,false);}
            for(int crossing=-1;crossing<=1;crossing+=2)for(int i=-6;i<=6;i++)Box("Crosswalk",road,new Vector3(crossing*34,.018f,i*1.2f),new Vector3(3,.02f,.5f),white,false);
            Box("District ground",road,new Vector3(0,-.5f,0),new Vector3(length+70,.2f,110),concrete);
        }
        static void Lighting(Transform environment)
        {
            var g=Group("03_Daylight",environment);var sun=new GameObject("City sun").AddComponent<Light>();sun.transform.SetParent(g,false);sun.type=LightType.Directional;sun.intensity=1.65f;sun.color=new Color(1,.94f,.82f);sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.Soft;RenderSettings.sun=sun;
            RenderSettings.skybox=AssetDatabase.LoadAssetAtPath<Material>("Assets/BiogearLab/Materials/TropicalSky.mat");RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.49f,.62f,.75f);RenderSettings.ambientEquatorColor=new Color(.35f,.4f,.44f);RenderSettings.ambientGroundColor=new Color(.2f,.22f,.24f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0035f;RenderSettings.fogColor=new Color(.62f,.74f,.8f);
            string path=Root+"/Lighting/StreetVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);AssetDatabase.AddObjectToAsset(tone,profile);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.15f);AssetDatabase.AddObjectToAsset(bloom,profile);}
            var volume=new GameObject("Street post processing").AddComponent<Volume>();volume.transform.SetParent(g,false);volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
            var probe=new GameObject("Street reflection").AddComponent<ReflectionProbe>();probe.transform.SetParent(g,false);probe.transform.localPosition=new Vector3(0,4,0);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=128;probe.size=new Vector3(40,25,30);probe.intensity=.65f;
        }
        [MenuItem("Tools/Street City/Build Gundam Battlefield")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first.");var active=SceneManager.GetActiveScene();if(active.isDirty)throw new Exception("Save your current scene before building.");
            foreach(string dir in new[]{"Scenes","Meshes","Materials","Textures","Prefabs","Lighting","Documentation/Review"})Directory.CreateDirectory(Root+"/"+dir);AssetDatabase.Refresh();
            if(active.path==ScenePath)EditorSceneManager.OpenScene(EnvironmentTools.Hub);
            if(File.Exists(ScenePath)){Directory.CreateDirectory("Backups/StreetCity");File.Copy(ScenePath,"Backups/StreetCity/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);}
            File.Copy("Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity",ScenePath,true);AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);var scene=EditorSceneManager.OpenScene(ScenePath);
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Data+"/manifest.json"));var materials=ImportMaterials(manifest);var imported=ImportModel(manifest,materials);PrefabUtility.SaveAsPrefabAsset(imported,Root+"/Prefabs/StreetCity_Original.prefab");
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;
            foreach(string name in new[]{"MegaKitHangar","SphereCityArena"}){var t=board.Find(name);if(t)t.gameObject.SetActive(false);}
            var old=board.Find("BattleArena");if(old){foreach(var r in old.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in old.GetComponentsInChildren<Collider>(true))c.enabled=false;}
            foreach(string name in new[]{"Key Light","Fill Light","Battle Post Processing"}){var go=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==name);if(go)go.SetActive(false);}
            board.position=new Vector3(5.4f,0,0);var environment=Group("StreetCity_Battlefield");float length=LayoutBuildings(imported,environment,manifest);Street(environment,length);Lighting(environment);
            var switcher=battle.GetComponent<BattleEnvironmentSwitcher>();switcher.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="CITY STREET",root=environment.gameObject,ambient=new Color(.49f,.62f,.75f),background=new Color(.62f,.74f,.8f)}};switcher.trainingRenderers=Array.Empty<MeshRenderer>();switcher.Apply(0);if(switcher.nextButton)switcher.nextButton.gameObject.SetActive(false);
            battle.initialEnemies=1;var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.homeYaw=90;orbit.homePitch=14;orbit.homeDistance=8.2f;orbit.minPitch=-85;orbit.maxPitch=89;orbit.maxDistance=80;orbit.ResetView(true);battle.battleCamera.clearFlags=CameraClearFlags.Skybox;battle.battleCamera.useOcclusionCulling=false;battle.battleCamera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var reviews=Group("04_ReviewCameras",environment);
            for(int i=0;i<2;i++){var camera=new GameObject(i==0?"StreetCity_Overview":"StreetCity_Road").AddComponent<Camera>();camera.transform.SetParent(reviews,false);camera.transform.position=i==0?new Vector3(-30,22,-28):new Vector3(-14,3.5f,-5);camera.transform.LookAt(i==0?new Vector3(4,3,0):new Vector3(7,2,0));camera.fieldOfView=i==0?62:58;camera.farClipPlane=600;camera.nearClipPlane=.1f;camera.useOcclusionCulling=false;camera.enabled=false;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
            PrefabUtility.SaveAsPrefabAsset(environment.gameObject,Root+"/Prefabs/StreetCity_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Register();Capture();Debug.Log("STREET_CITY_READY: Gundam at "+battle.player.transform.position+", avenue "+length+"m");
        }
        static void Register()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var d=catalog.Find("street-city");if(!d){d=ScriptableObject.CreateInstance<EnvironmentDefinition>();d.id="street-city";AssetDatabase.CreateAsset(d,Root+"/StreetCityDefinition.asset");}
            d.displayName="CITY STREET · GUNDAM";d.description="Đại lộ giữa các khối nhà · Gundam 1v1 / 1v3.";d.available=true;d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);d.battleVariant=-1;d.accent=new Color(.37f,.48f,.62f);d.SyncScene();EditorUtility.SetDirty(d);catalog.environments=new[]{d}.Concat(catalog.environments.Where(x=>x!=d)).ToArray();EditorUtility.SetDirty(catalog);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();
        }
        [MenuItem("Tools/Street City/Open Battlefield")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Tools/Street City/Capture Review")]
        public static void Capture()
        {
            foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>().Where(c=>c.name.StartsWith("StreetCity_")))
            {
                var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var previous=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();File.WriteAllBytes(Root+"/Documentation/Review/"+c.name+".png",texture.EncodeToPNG());}
                finally{c.targetTexture=previous;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
            }
            AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("street-city");d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Documentation/Review/StreetCity_Overview.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();
        }
    }
}

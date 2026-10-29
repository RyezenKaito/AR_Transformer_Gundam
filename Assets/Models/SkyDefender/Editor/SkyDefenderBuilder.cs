using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SkyDefender.Editor
{
    public static class SkyDefenderBuilder
    {
        public const string Root = "Assets/Models/SkyDefender";
        public const string ScenePath = Root + "/Scenes/SkyDefender_Atelier.unity";
        static Material red, scarlet, white, gold, dark, steel, black, glow, floor, marking, decals;
        static readonly List<Mesh> extra = new List<Mesh>();
        static Vector3 V(float x,float y,float z=0) { return new Vector3(x,y,z); }
        static Transform G(string n,Transform t,Vector3 p=default(Vector3),Vector3 e=default(Vector3)) { return SDM.Group(n,t,p,e); }
        static GameObject P(string n,Transform t,Material m,float depth,params Vector3[] p) { return SDM.Plate(n,t,p,depth,m,Mathf.Min(depth*.3f,.035f)); }
        static GameObject MP(string n,Transform t,int s,Material m,float depth,params Vector3[] p) { return P(n,t,m,depth,p.Select(x=>V(x.x*s,x.y,x.z)).ToArray()); }
        static void Line(string n,Transform t,Vector3 a,Vector3 b,Material m,float r=.006f) { SDM.Beam(n,t,a,b,r,m,6); }
        static void Rivet(Transform t,Vector3 p,float r=.014f) { SDM.Beam("Recessed bolt socket",t,p+V(0,0,.003f),p-V(0,0,.009f),r,black,12);SDM.Beam("Titanium hex fastener",t,p-V(0,0,.009f),p-V(0,0,.013f),r*.57f,steel,6); }
        static void Disc(string n,Transform t,Vector3 p,float r,Material m) { SDM.Beam(n,t,p+V(0,0,.02f),p-V(0,0,.025f),r,m,24); }
        static void Joint(string n,Transform t,Vector3 p,float r)
        {
            SDM.Orb(n+" inner joint",t,p,V(r*2,r*2,r*2),dark);
            Disc(n+" black bearing",t,p-V(0,0,r*.76f),r*.80f,black);
            SDM.Ring(n+" machined collar",t,p-V(0,0,r*.84f),r*.67f,.015f,gold);
            Disc(n+" brushed cap",t,p-V(0,0,r*.96f),r*.40f,steel);
            Rivet(t,p-V(0,0,r*1.12f),r*.15f);
        }
        static void Vent(Transform t,Vector3 p,Vector3 size,int count)
        {
            SDM.Box("Deep intake recess",t,p,size,black);
            for(int i=0;i<count;i++) SDM.Box("Intake titanium louver",t,p+V(0,(i+.5f)*size.y/count-size.y/2,-size.z*.55f),V(size.x*.87f,size.y/count*.28f,.018f),steel,V(-12,0,0));
        }
        // Closed octagonal hull with changing sections: volume for torso, limbs and helmet.
        static GameObject Hull(string n,Transform parent,Material material,params Vector4[] sections)
        {
            var verts=new List<Vector3>();var uv=new List<Vector2>();var ids=new List<int>();
            Action<Vector3,Vector3,Vector3> tri=(a,b,c)=>{int k=verts.Count;verts.Add(a);verts.Add(b);verts.Add(c);uv.Add(new Vector2(a.x+a.z,a.y));uv.Add(new Vector2(b.x+b.z,b.y));uv.Add(new Vector2(c.x+c.z,c.y));ids.AddRange(new[]{k,k+1,k+2});};
            var rings=new List<Vector3[]>();
            foreach(var s in sections) {float x=s.y/2,z=s.z/2;float bx=x*.28f,bz=z*.28f;rings.Add(new[]{V(-x+bx,s.x,-z+s.w),V(x-bx,s.x,-z+s.w),V(x,s.x,-z+bz+s.w),V(x,s.x,z-bz+s.w),V(x-bx,s.x,z+s.w),V(-x+bx,s.x,z+s.w),V(-x,s.x,z-bz+s.w),V(-x,s.x,-z+bz+s.w)});}
            for(int j=0;j<rings.Count-1;j++)for(int i=0;i<8;i++){int k=(i+1)%8;tri(rings[j][i],rings[j+1][i],rings[j+1][k]);tri(rings[j][i],rings[j+1][k],rings[j][k]);}
            for(int i=0;i<8;i++){int k=(i+1)%8;tri(V(0,sections[0].x,sections[0].w),rings[0][i],rings[0][k]);int last=rings.Count-1;tri(V(0,sections[last].x,sections[last].w),rings[last][k],rings[last][i]);}
            var mesh=new Mesh{name=n};mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(ids,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();extra.Add(mesh);
            var go=G(n,parent).gameObject;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        static Vector4 S(float y,float width,float depth,float centerZ=0){return new Vector4(y,width,depth,centerZ);}
        [MenuItem("Sky Defender/Build model and atelier")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before rebuilding.");
            foreach(string d in new[]{"Prefabs","Meshes","Materials","Textures","Scenes","Settings"})Directory.CreateDirectory(Root+"/"+d);
            var previous=SceneManager.GetActiveScene();
            if(previous.IsValid()&&previous.isDirty)
            {
                Directory.CreateDirectory("Docs/SceneBackups");
                string backup="Assets/Models/SkyDefender/Scenes/Preserved_"+previous.name+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";
                if(!EditorSceneManager.SaveScene(previous,backup,true))throw new IOException("Could not preserve unsaved scene.");
            }
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SDM.Meshes.Clear();extra.Clear();Materials();
            Transform model=G("SKY DEFENDER | Crimson Sovereign",null);
            Body(model);Legs(model);Arms(model);Transform wings=Wings(model);
            float sole=model.GetComponentsInChildren<Renderer>().Min(x=>x.bounds.min.y);
            foreach(Transform child in model)child.localPosition-=Vector3.up*sole;
            Stage();Lighting();
            var all=SDM.Meshes.Concat(extra).ToArray();
            string meshPath=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/SkyDefender_AuthoredGeometry.asset");
            AssetDatabase.CreateAsset(all[0],meshPath);for(int i=1;i<all.Length;i++)AssetDatabase.AddObjectToAsset(all[i],meshPath);
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAsset(model.gameObject,Root+"/Prefabs/SkyDefender_Detailed.prefab");
            var solo=UnityEngine.Object.Instantiate(model.gameObject);solo.name="Sky Defender | Body and weapons";
            UnityEngine.Object.DestroyImmediate(solo.transform.Find(wings.name).gameObject);
            PrefabUtility.SaveAsPrefabAsset(solo,Root+"/Prefabs/SkyDefender_Body.prefab");UnityEngine.Object.DestroyImmediate(solo);
            var wingClone=UnityEngine.Object.Instantiate(wings.gameObject);wingClone.name="Sky Defender | Wing binders";wingClone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            PrefabUtility.SaveAsPrefabAsset(wingClone,Root+"/Prefabs/SkyDefender_Wings.prefab");UnityEngine.Object.DestroyImmediate(wingClone);
            var combined=Combine(model);PrefabUtility.SaveAsPrefabAsset(combined,Root+"/Prefabs/SkyDefender_Combined.prefab");
            float robotHeight=model.Find("04 Small scale helmet and crown").GetComponentsInChildren<Renderer>().Max(x=>x.bounds.max.y);
            combined.transform.localScale=Vector3.one*(.25f/robotHeight);
            PrefabUtility.SaveAsPrefabAsset(combined,Root+"/Prefabs/SkyDefender_AR_25cm.prefab");UnityEngine.Object.DestroyImmediate(combined);
            var viewer=G("Showcase orbit controls",null).gameObject.AddComponent<SkyDefender.SkyDefenderViewer>();viewer.viewCamera=Camera.main;viewer.wings=wings;
            viewer.SetView(16,5,13.6f,V(0,2.7f,0));
            PrefabUtility.SaveAsPrefabAssetAndConnect(model.gameObject,Root+"/Prefabs/SkyDefender_Detailed.prefab",InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Selection.activeGameObject=model.gameObject;
            if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(V(0,2.7f),Quaternion.Euler(5,12,0),7);
            Directory.CreateDirectory("Docs/SkyDefender");
            var filters=model.GetComponentsInChildren<MeshFilter>();
            File.WriteAllText("Docs/SkyDefender/BuildReport.txt","Sky Defender built from overview.png\nMesh parts="+filters.Length+"\nTriangles="+filters.Sum(f=>(long)f.sharedMesh.triangles.Length/3)+"\nMaterials="+model.GetComponentsInChildren<MeshRenderer>().Select(r=>r.sharedMaterial).Distinct().Count()+"\nRobot height including crown="+robotHeight+"\nAR scale="+(.25f/robotHeight)+"\nScene="+ScenePath+"\n");
            Debug.Log(File.ReadAllText("Docs/SkyDefender/BuildReport.txt"));
        }
        static GameObject Combine(Transform source)
        {
            var go=new GameObject("Sky Defender | Combined display");
            var renderers=source.GetComponentsInChildren<MeshRenderer>();
            string container=null;
            foreach(var group in renderers.GroupBy(x=>x.sharedMaterial))
            {
                var instances=group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=source.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray();
                var mesh=new Mesh{name="SD_Combined_"+group.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(instances,true,true);mesh.RecalculateBounds();
                if(container==null){container=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/SkyDefender_CombinedGeometry.asset");AssetDatabase.CreateAsset(mesh,container);}else AssetDatabase.AddObjectToAsset(mesh,container);
                var child=G(group.Key.name,go.transform).gameObject;child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
            var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var collider=go.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size;
            AssetDatabase.SaveAssets();return go;
        }
        static void Materials()
        {
            var grain=SurfaceTexture("MicroMetal_Normal",true);var rough=SurfaceTexture("Metal_RoughnessVariation",false);
            red=Mat("01_Oxblood_anodized_alloy",new Color(.36f,.012f,.026f),.82f,.61f,grain,rough);
            scarlet=Mat("02_Crimson_lacquer_metal",new Color(.57f,.025f,.038f),.72f,.56f,grain,rough);
            white=Mat("03_Ivory_ceramic_armor",new Color(.73f,.76f,.79f),.22f,.43f,grain,null);
            gold=Mat("04_Brushed_champagne_gold",new Color(.72f,.45f,.17f),1,.72f,grain,rough);
            dark=Mat("05_Gunmetal_internal_frame",new Color(.047f,.057f,.072f),.86f,.40f,grain,rough);
            steel=Mat("06_Machined_titanium",new Color(.37f,.43f,.49f),.95f,.66f,grain,null);
            black=Mat("07_Deep_panel_recess",new Color(.006f,.009f,.014f),.15f,.28f,null,null);
            glow=Mat("08_Emerald_optics",new Color(.008f,.40f,.23f),.32f,.75f,null,null);glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.04f,1,.48f)*1.8f);EditorUtility.SetDirty(glow);
            floor=Mat("09_Atelier_charcoal",new Color(.026f,.035f,.049f),.38f,.32f,grain,null);
            marking=Mat("10_Engraved_gold_markings",new Color(.61f,.46f,.23f),.65f,.40f,null,null);
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/TechnicalStencils_Atlas.png");
            decals=Mat("11_Technical_stencil_decals",Color.white,.15f,.35f,null,null);decals.SetTexture("_BaseMap",atlas);
            decals.SetFloat("_Surface",1);decals.SetFloat("_Blend",0);decals.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);decals.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);decals.SetInt("_ZWrite",0);decals.SetOverrideTag("RenderType","Transparent");decals.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");decals.renderQueue=3000;EditorUtility.SetDirty(decals);
        }
        static Material Mat(string n,Color c,float metal,float smooth,Texture2D normal,Texture2D grain)
        {
            string path=Root+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.name=n;
            m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.SetFloat("_BumpScale",.055f);m.enableInstancing=true;
            if(normal){m.SetTexture("_BumpMap",normal);m.SetTextureScale("_BumpMap",new Vector2(3,3));m.EnableKeyword("_NORMALMAP");}
            if(grain){m.SetTexture("_BaseMap",grain);m.SetTextureScale("_BaseMap",new Vector2(3,3));}
            if(metal>.2f)
            {
                int size=256;var map=new Texture2D(size,size,TextureFormat.RGBA32,false,true);var pixels=new Color[size*size];var random=new System.Random(n.GetHashCode());
                for(int y=0;y<size;y++)for(int x=0;x<size;x++){float variation=((float)random.NextDouble()-.5f)*.065f+Mathf.Sin(y*.41f)*.018f;pixels[y*size+x]=new Color(metal,0,0,Mathf.Clamp01(smooth+variation));}
                map.SetPixels(pixels);map.Apply();string mapPath=Root+"/Textures/"+n+"_MetallicSmoothness.png";File.WriteAllBytes(mapPath,map.EncodeToPNG());UnityEngine.Object.DestroyImmediate(map);AssetDatabase.ImportAsset(mapPath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(mapPath);importer.sRGBTexture=false;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();
                m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(mapPath));m.SetTextureScale("_MetallicGlossMap",new Vector2(3,3));m.SetFloat("_Smoothness",1);m.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            EditorUtility.SetDirty(m);return m;
        }
        static Texture2D SurfaceTexture(string n,bool normal)
        {
            const int size=512;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false,true);var pixels=new Color[size*size];var random=new System.Random(7241);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float g=(float)random.NextDouble(),brush=Mathf.Sin(y*1.7f)*.025f+Mathf.Sin(y*.13f)*.016f;
                pixels[y*size+x]=normal?new Color(.5f+(g-.5f)*.05f,.5f+brush,1,1):new Color(1-(g*.024f+Mathf.Abs(brush)),1-(g*.024f+Mathf.Abs(brush)),1-(g*.024f+Mathf.Abs(brush)),1);
            }
            tex.SetPixels(pixels);tex.Apply();string path=Root+"/Textures/"+n+".png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void Stage()
        {
            var stage=G("STAGE | octagonal presentation plinth",null);
            SDM.Beam("Wide octagonal basalt base",stage,V(0,-.24f),V(0,-.13f),1.87f,floor,8);
            SDM.Beam("Upper machined deck",stage,V(0,-.13f),V(0,-.008f),1.79f,dark,8);
            SDM.Ring("Gold inlaid perimeter",stage,V(0,-.001f),1.69f,.008f,gold,V(90,0,0));
            for(int i=0;i<24;i++)
            {
                float angle=i*Mathf.PI/12;Vector3 a=V(Mathf.Cos(angle)*1.54f,.0f,Mathf.Sin(angle)*1.54f),b=V(Mathf.Cos(angle)*1.64f,.0f,Mathf.Sin(angle)*1.64f);Line("Deck radial etching",stage,a,b,marking,.003f);
            }
            Label(stage,"SKY DEFENDER",V(0,.011f,-1.33f),.105f,new Color(.65f,.52f,.30f),V(90,0,0));
            Label(stage,"GN-0000 / SD     -     CRIMSON SOVEREIGN",V(0,.012f,-1.47f),.035f,new Color(.5f,.54f,.6f),V(90,0,0));
            SDM.Box("Infinite studio floor",stage,V(0,-.30f,0),V(160,.10f,160),floor);
            var verts=new List<Vector3>();var ids=new List<int>();
            for(int i=0;i<=24;i++){float a=i*Mathf.PI*.5f/24;float y=-.25f+7*(1-Mathf.Cos(a)),z=4+7*Mathf.Sin(a);verts.Add(V(-60,y,z));verts.Add(V(60,y,z));}
            verts.Add(V(-60,70,11));verts.Add(V(60,70,11));
            for(int i=0;i<25;i++){int k=i*2;ids.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
            var mesh=new Mesh{name="SkyDefender_CurvedBackdrop"};mesh.SetVertices(verts);mesh.SetTriangles(ids,0);mesh.RecalculateNormals();extra.Add(mesh);
            var backdrop=G("Seamless curved backdrop",stage).gameObject;backdrop.AddComponent<MeshFilter>().sharedMesh=mesh;backdrop.AddComponent<MeshRenderer>().sharedMaterial=floor;
            SDM.Box("Rear studio wall",stage,V(0,20,-21),V(120,42,.15f),floor);
            SDM.Box("Left studio wall",stage,V(-25,20,0),V(.15f,42,120),floor);
            SDM.Box("Right studio wall",stage,V(25,20,0),V(.15f,42,120),floor);
            foreach(var r in stage.GetComponentsInChildren<MeshRenderer>())if(r.name.Contains("studio")||r.name.Contains("backdrop"))r.shadowCastingMode=ShadowCastingMode.Off;
        }
        static void Lighting()
        {
            var original=QualitySettings.renderPipeline?QualitySettings.renderPipeline:GraphicsSettings.defaultRenderPipeline;
            if(original&& (original.name.StartsWith("SkyDefender")||original.name.StartsWith("HaoLing")))original=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Models/Unicorn Gundam/Demo/Generated/UnicornMobilePipeline.asset");
            var pipeline=UnityEngine.Object.Instantiate(original);pipeline.name="SkyDefender_Studio_Pipeline";
            var so=new SerializedObject(pipeline);
            foreach(string key in new[]{"m_MainLightShadowsSupported","m_SupportsHDR","m_SoftShadowsSupported","m_ReflectionProbeBlending"}){var p=so.FindProperty(key);if(p!=null)p.boolValue=true;}
            foreach(var pair in new Dictionary<string,int>{{"m_AdditionalLightsRenderingMode",1},{"m_AdditionalLightsPerObjectLimit",8},{"m_MSAA",4},{"m_MainLightShadowmapResolution",4096},{"m_ShadowCascadeCount",2}}){var p=so.FindProperty(pair.Key);if(p!=null)p.intValue=pair.Value;}
            var shadow=so.FindProperty("m_ShadowDistance");if(shadow!=null)shadow.floatValue=20;var scale=so.FindProperty("m_RenderScale");if(scale!=null)scale.floatValue=1.25f;so.ApplyModifiedPropertiesWithoutUndo();
            string pipelinePath=Root+"/Settings/SkyDefender_Studio_Pipeline.asset";Save(pipeline,pipelinePath);
            var savedPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
            var rendererPath=Root+"/Settings/SkyDefender_Studio_Renderer.asset";
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if(!renderer)
            {
                renderer=ScriptableObject.CreateInstance<UniversalRendererData>();renderer.name="SkyDefender_Studio_Renderer";AssetDatabase.CreateAsset(renderer,rendererPath);
                var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Fine armor contact shadows";AssetDatabase.AddObjectToAsset(ao,renderer);renderer.rendererFeatures.Add(ao);
                var aoData=new SerializedObject(ao);aoData.FindProperty("m_Settings.Intensity").floatValue=1.4f;aoData.FindProperty("m_Settings.Radius").floatValue=.12f;aoData.FindProperty("m_Settings.DirectLightingStrength").floatValue=.18f;aoData.FindProperty("m_Settings.Samples").enumValueIndex=0;aoData.FindProperty("m_Settings.Downsample").boolValue=false;aoData.ApplyModifiedPropertiesWithoutUndo();ao.Create();EditorUtility.SetDirty(renderer);
            }
            var pipelineData=new SerializedObject(savedPipeline);pipelineData.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue=renderer;pipelineData.ApplyModifiedPropertiesWithoutUndo();
            var controller=G("Studio render settings",null).gameObject.AddComponent<SkyDefender.SkyDefenderStudio>();controller.originalPipeline=original;controller.studioPipeline=savedPipeline;QualitySettings.renderPipeline=controller.studioPipeline;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.22f,.27f,.34f);RenderSettings.ambientEquatorColor=new Color(.12f,.15f,.20f);RenderSettings.ambientGroundColor=new Color(.035f,.043f,.055f);RenderSettings.skybox=null;RenderSettings.fog=false;
            Reflection();
            Light("Key | broad neutral softbox",V(32,-28,0),new Color(1,.96f,.92f),1.8f,true);
            Light("Fill | blue silver",V(18,47,0),new Color(.70f,.83f,1),.65f,false);
            Light("Rim | pale gold",V(25,161,0),new Color(1,.89f,.73f),1.6f,false);
            var camera=G("Sky Defender Camera",null).gameObject.AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=32;camera.nearClipPlane=.03f;camera.farClipPlane=200;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.011f,.017f,.027f);camera.allowHDR=true;camera.gameObject.AddComponent<AudioListener>();
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var volume=G("Studio tone and bloom",null).gameObject.AddComponent<Volume>();volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);var adjust=profile.Add<ColorAdjustments>();adjust.postExposure.Override(.0f);adjust.contrast.Override(8);adjust.saturation.Override(-5);
            var bloom=profile.Add<Bloom>();bloom.intensity.Override(.10f);bloom.threshold.Override(1.5f);
            Save(profile,Root+"/Settings/AtelierVolume.asset");volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"/Settings/AtelierVolume.asset");
            foreach(var component in volume.sharedProfile.components)if(!EditorUtility.IsPersistent(component))AssetDatabase.AddObjectToAsset(component,volume.sharedProfile);
            EditorUtility.SetDirty(volume.sharedProfile);
        }
        static void Save(UnityEngine.Object obj,string path)
        {
            obj.name=Path.GetFileNameWithoutExtension(path);
            var old=AssetDatabase.LoadMainAssetAtPath(path);if(old){EditorUtility.CopySerialized(obj,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(obj);}else AssetDatabase.CreateAsset(obj,path);
        }
        static void Light(string name,Vector3 rotation,Color color,float power,bool shadow)
        {
            var light=G(name,null,V(0,6),rotation).gameObject.AddComponent<Light>();light.type=LightType.Directional;light.color=color;light.intensity=power;light.shadows=shadow?LightShadows.Soft:LightShadows.None;light.shadowBias=.02f;light.shadowNormalBias=.08f;
        }
        static void Reflection()
        {
            int n=128;var cube=new Cubemap(n,TextureFormat.RGBAHalf,true);cube.name="SkyDefender_SoftboxReflection";
            for(int face=0;face<6;face++)
            {
                var pixels=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++)
                {
                    float u=(x+.5f)*2/n-1,v=(y+.5f)*2/n-1;Vector3 d;
                    switch(face){case 0:d=V(1,-v,-u);break;case 1:d=V(-1,-v,u);break;case 2:d=V(u,1,v);break;case 3:d=V(u,-1,-v);break;case 4:d=V(u,-v,1);break;default:d=V(-u,-v,-1);break;}d.Normalize();
                    float key=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(-.7f,.8f,-.8f).normalized)),32)*5;
                    float rim=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(.8f,.5f,.5f).normalized)),26)*3;
                    float fill=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,V(.6f,.1f,-1).normalized)),14)*1.4f;
                    float b=.08f+Mathf.Max(d.y,0)*.20f;pixels[y*n+x]=new Color(b+key+rim+fill*.7f,b+key*.94f+rim*.85f+fill*.85f,b+key*.85f+rim*.65f+fill,1);
                }
                cube.SetPixels(pixels,(CubemapFace)face);
            }
            cube.Apply(true,false);Save(cube,Root+"/Settings/StudioReflection.asset");RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=AssetDatabase.LoadAssetAtPath<Cubemap>(Root+"/Settings/StudioReflection.asset");RenderSettings.reflectionIntensity=1.1f;
        }
        public static void Capture(string name,int width=1800,int height=1400)
        {
            var camera=Camera.main;if(!camera)throw new InvalidOperationException("No camera");
            var previous=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            Directory.CreateDirectory("Docs/Art/Renders/SkyDefender");File.WriteAllBytes("Docs/Art/Renders/SkyDefender/"+name+".png",image.EncodeToPNG());camera.targetTexture=previous;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
        }
        static void Blade(string n,Transform t,Vector3 a,Vector3 b,float w,Material m,float depth=.065f){SDM.Blade(n,t,a,b,w,depth,m);}
        static void Stripe(Transform t,Vector3 p,int side)
        {
            for(int i=0;i<3;i++)SDM.Box("Gold warning chevron",t,p+V(side*i*.024f,0,0),V(.010f,.052f,.005f),marking,V(0,0,-25*side));
        }
        static void Decal(Transform t,Vector3 p,Vector2 size,int cell,float angle=0)
        {
            var go=G("Technical stencil "+cell,t,p,V(0,0,angle)).gameObject;
            var mesh=new Mesh{name="SD_Stencil_"+cell};mesh.vertices=new[]{V(-size.x/2,-size.y/2),V(-size.x/2,size.y/2),V(size.x/2,size.y/2),V(size.x/2,-size.y/2)};
            float u=(cell%4)*.25f,v=1-(cell/4+1)*.25f;mesh.uv=new[]{new Vector2(u,v),new Vector2(u,v+.25f),new Vector2(u+.25f,v+.25f),new Vector2(u+.25f,v)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();extra.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=decals;r.shadowCastingMode=ShadowCastingMode.Off;
        }
        static void Label(Transform t,string txt,Vector3 p,float size,Color color,Vector3 rotation=default(Vector3))
        {
            var go=G("Stencil | "+txt,t,p,rotation).gameObject;var text=go.AddComponent<TextMesh>();text.text=txt;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=90;text.characterSize=size/10;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=color;go.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
        }
        static void Body(Transform model)
        {
            var hips=G("01 Pelvis and floating tassets",model,V(0,2.44f,0));
            Hull("Pelvis load-bearing frame",hips,dark,S(-.16f,.45f,.32f),S(.12f,.58f,.35f));
            P("Red central codpiece",hips,red,.12f,V(-.15f,.11f,-.23f),V(.15f,.11f,-.23f),V(.11f,-.22f,-.26f),V(0,-.39f,-.30f),V(-.11f,-.22f,-.26f));
            Blade("Codpiece gold spine",hips,V(0,.06f,-.28f),V(0,-.32f,-.34f),.08f,gold,.025f);
            var abdomen=G("02 Articulated abdomen",model,V(0,2.90f,0));
            Hull("Narrow titanium waist",abdomen,black,S(-.36f,.31f,.29f),S(.19f,.49f,.37f));
            for(int i=0;i<5;i++)
            {
                float y=-.29f+i*.095f;
                P("Overlapping abdominal rib "+i,abdomen,i%2==0?dark:red,.085f,V(-.22f,y+.065f,-.19f),V(0,y+.022f,-.25f),V(.22f,y+.065f,-.19f),V(.17f,y-.01f,-.20f),V(0,y-.045f,-.28f),V(-.17f,y-.01f,-.20f));
                for(int s=-1;s<=1;s+=2)Line("Abdominal machined lip",abdomen,V(s*.15f,y,-.23f),V(0,y-.03f,-.283f),gold,.006f);
                SDM.Box("Rear lumbar vertebra",abdomen,V(0,y,.19f),V(.17f,.07f,.09f),steel);
            }
            var chest=G("03 Thorax armored core",model,V(0,3.42f,0));
            Hull("Thorax titanium hull",chest,dark,S(-.36f,.48f,.36f),S(-.04f,.92f,.49f),S(.22f,.80f,.39f));
            P("Deep crimson sternum",chest,red,.13f,V(-.21f,.23f,-.26f),V(.21f,.23f,-.26f),V(.27f,.01f,-.30f),V(.12f,-.27f,-.31f),V(0,-.39f,-.28f),V(-.12f,-.27f,-.31f),V(-.27f,.01f,-.30f));
            P("Sternum gold crest",chest,gold,.024f,V(-.11f,.18f,-.32f),V(.11f,.18f,-.32f),V(0,-.20f,-.37f));
            P("Sternum obsidian inset",chest,black,.018f,V(-.055f,.12f,-.35f),V(.055f,.12f,-.35f),V(0,-.08f,-.39f));
            Blade("Sternum emerald status lens",chest,V(0,.10f,-.38f),V(0,.02f,-.39f),.04f,glow,.012f);
            for(int s=-1;s<=1;s+=2)
            {
                MP("Ivory split pectoral armor",chest,s,white,.14f,V(.15f,.18f,-.26f),V(.40f,.25f,-.16f),V(.53f,.12f,-.20f),V(.43f,-.15f,-.28f),V(.15f,-.23f,-.34f),V(.24f,.0f,-.37f));
                MP("Pectoral red blade inset",chest,s,red,.025f,V(.26f,.12f,-.39f),V(.42f,.15f,-.31f),V(.36f,-.04f,-.37f),V(.22f,-.13f,-.40f));
                Line("Pectoral gold lower edge",chest,V(s*.17f,-.21f,-.37f),V(s*.43f,-.14f,-.31f),gold,.013f);
                Vent(chest,V(s*.36f,.07f,-.405f),V(.12f,.09f,.027f),3);
                Blade("Raised gorget",chest,V(s*.21f,.21f,-.03f),V(s*.29f,.46f,-.06f),.15f,white,.075f);
                SDM.Beam("Waist chrome ram",abdomen,V(s*.18f,-.24f,.06f),V(s*.31f,.35f,.05f),.025f,steel,16);
                SDM.Beam("Waist piston sleeve",abdomen,V(s*.18f,-.24f,.06f),V(s*.23f,.0f,.055f),.045f,dark,16);
                Blade("Flank gold guard",abdomen,V(s*.32f,.30f,-.02f),V(s*.23f,-.11f,-.14f),.13f,gold,.06f);
                var skirt=G((s<0?"L":"R")+" front tasset hinge",hips,V(s*.19f,-.02f,-.04f),V(-7,s*8,s*7));
                MP("Tasset crimson frame",skirt,s,red,.09f,V(.0f,.08f,-.16f),V(.23f,.09f,-.15f),V(.28f,-.26f,-.20f),V(.15f,-.58f,-.27f),V(.035f,-.34f,-.26f));
                MP("Tasset white central armor",skirt,s,white,.05f,V(.035f,.03f,-.215f),V(.16f,.045f,-.21f),V(.19f,-.25f,-.27f),V(.12f,-.49f,-.32f),V(.075f,-.25f,-.31f));
                Blade("Tasset gold slash",skirt,V(s*.19f,-.03f,-.27f),V(s*.17f,-.38f,-.33f),.04f,gold,.015f);
                Rivet(skirt,V(s*.11f,.01f,-.27f));
                Blade("Outer hip crimson lance",hips,V(s*.31f,.11f,.03f),V(s*.64f,-.53f,-.03f),.28f,red,.12f);
                Blade("Outer hip white layer",hips,V(s*.35f,.03f,-.05f),V(s*.58f,-.40f,-.13f),.17f,white,.055f);
                Blade("Outer hip gold trim",hips,V(s*.40f,.04f,-.10f),V(s*.62f,-.39f,-.16f),.045f,gold,.025f);
                Blade("Rear hip red armor",hips,V(s*.20f,.03f,.23f),V(s*.36f,-.50f,.38f),.25f,red,.10f);
                Blade("Rear hip ivory armor",hips,V(s*.19f,-.01f,.32f),V(s*.28f,-.39f,.46f),.13f,white,.05f);
                Stripe(chest,V(s*.43f,.18f,-.28f),s);
            }
            SDM.Beam("Exposed neck titanium",model,V(0,3.63f,0),V(0,3.99f,0),.08f,steel,20);
            SDM.Ring("Neck gimbal",model,V(0,3.78f,0),.115f,.025f,dark,V(90,0,0));
            Head(G("04 Small scale helmet and crown",model,V(0,3.91f,-.035f)));
            chest.localScale=V(1.10f,1,1.06f);
            for(int s=-1;s<=1;s+=2)
            {
                MP("Layered breastplate outer gilded frame",chest,s,gold,.025f,V(.39f,.20f,-.29f),V(.52f,.10f,-.25f),V(.45f,-.13f,-.32f),V(.29f,-.21f,-.38f),V(.39f,-.08f,-.38f));
                for(int i=0;i<3;i++)MP("Thorax outer overlapping scale "+i,chest,s,red,.045f,V(.38f,-.08f-i*.065f,-.23f),V(.53f,.0f-i*.065f,-.15f),V(.46f,-.15f-i*.065f,-.22f),V(.34f,-.22f-i*.065f,-.27f));
                Line("Thorax inner panel seam",chest,V(s*.20f,.09f,-.40f),V(s*.28f,.16f,-.34f),dark,.005f);
                Rivet(chest,V(s*.45f,.13f,-.29f),.011f);
                Blade("Collar front red guard",chest,V(s*.19f,.22f,-.09f),V(s*.15f,.37f,-.13f),.10f,red,.055f);
            }
        }
        static void Legs(Transform model)
        {
            for(int s=-1;s<=1;s+=2)
            {
                string side=s<0?"L":"R";
                var hip=G("05 "+side+" hip articulation",model,V(s*.26f,2.42f,.025f),V(0,s*-4,s*6));
                Joint("Hip",hip,Vector3.zero,.145f);
                var thigh=G(side+" femur",hip,V(0,-.18f,0));
                Hull("Femur metal skeleton",thigh,dark,S(-.65f,.20f,.22f),S(.02f,.25f,.27f));
                Hull("Ivory thigh shell",thigh,white,S(-.66f,.27f,.29f,-.03f),S(-.39f,.34f,.34f,-.035f),S(-.05f,.30f,.29f));
                P("Crimson thigh spear",thigh,red,.04f,V(-.09f,-.12f,-.19f),V(.09f,-.12f,-.19f),V(.065f,-.45f,-.22f),V(0,-.63f,-.22f),V(-.065f,-.45f,-.22f));
                for(int q=-1;q<=1;q+=2)
                {
                    Line("Thigh shell engraved seam",thigh,V(q*.12f,-.14f,-.17f),V(q*.13f,-.46f,-.20f),dark,.005f);
                    SDM.Beam("Femur hydraulic rod",thigh,V(q*.16f,-.08f,.03f),V(q*.17f,-.62f,.06f),.018f,steel,12);
                    SDM.Beam("Femur hydraulic sleeve",thigh,V(q*.16f,-.11f,.03f),V(q*.165f,-.38f,.045f),.027f,gold,12);
                }
                var knee=G(side+" knee hinge",hip,V(0,-.93f,-.005f),V(-3,0,0));
                Joint("Knee",knee,Vector3.zero,.16f);
                P("Kneecap crimson armor",knee,red,.11f,V(-.19f,.17f,-.13f),V(0,.29f,-.17f),V(.19f,.17f,-.13f),V(.14f,-.16f,-.19f),V(0,-.30f,-.25f),V(-.14f,-.16f,-.19f));
                Blade("Knee central gold spine",knee,V(0,.22f,-.24f),V(0,-.23f,-.31f),.08f,gold,.03f);
                for(int q=-1;q<=1;q+=2)Blade("Knee white outer fin",knee,V(q*.13f,.11f,-.18f),V(q*.22f,-.14f,-.24f),.11f,white,.05f);
                var shin=G(side+" long greave",knee,V(0,-.21f,.02f));
                Hull("Tibia interior frame",shin,dark,S(-.93f,.19f,.23f,.02f),S(-.02f,.23f,.28f));
                Hull("Tapered ivory shin shell",shin,white,S(-.94f,.26f,.29f),S(-.78f,.37f,.36f,.025f),S(-.34f,.33f,.40f,.035f),S(-.04f,.28f,.33f));
                P("Greave red armored spline",shin,red,.06f,V(-.10f,-.13f,-.18f),V(.10f,-.13f,-.18f),V(.12f,-.52f,-.22f),V(.085f,-.83f,-.21f),V(0,-1.02f,-.20f),V(-.085f,-.83f,-.21f),V(-.12f,-.52f,-.22f));
                Blade("Greave gold center inlay",shin,V(0,-.30f,-.25f),V(0,-.87f,-.26f),.043f,gold,.012f);
                for(int q=-1;q<=1;q+=2)
                {
                    Blade("Greave side ivory saber",shin,V(q*.16f,-.36f,-.06f),V(q*.25f,-.87f,-.07f),.14f,white,.085f);
                    Blade("Greave side crimson accent",shin,V(q*.20f,-.35f,.06f),V(q*.31f,-.70f,.04f),.12f,red,.065f);
                    Line("Greave engraved panel boundary",shin,V(q*.12f,-.35f,-.218f),V(q*.15f,-.68f,-.20f),dark,.005f);
                    Rivet(shin,V(q*.11f,-.24f,-.23f));Rivet(shin,V(q*.13f,-.77f,-.20f));
                }
                Vent(shin,V(0,-.34f,.255f),V(.15f,.24f,.035f),5);
                SDM.Beam("Rear calf silver piston",shin,V(s*.12f,-.20f,.22f),V(s*.12f,-.81f,.21f),.025f,steel,16);
                var foot=G(side+" ankle and articulated foot",knee,V(0,-1.26f,-.015f),V(0,s*-5,0));
                Joint("Ankle",foot,V(0,.07f,.02f),.115f);
                SDM.Box("Foot graphite sole",foot,V(0,-.095f,-.18f),V(.34f,.13f,.70f),dark,default(Vector3),.32f);
                Hull("Crimson armored toe",foot,red,S(-.10f,.32f,.67f,-.20f),S(.02f,.32f,.60f,-.20f),S(.14f,.21f,.31f,-.12f));
                P("Toe gold keel",foot,gold,.03f,V(-.035f,.04f,-.51f),V(.035f,.04f,-.51f),V(.03f,-.10f,-.55f),V(-.03f,-.10f,-.55f));
                for(int i=0;i<3;i++)Line("Toe armor break",foot,V(-.13f,-.04f,-.48f+i*.045f),V(.13f,-.04f,-.48f+i*.045f),black,.006f);
                for(int q=-1;q<=1;q+=2)Blade("White ankle guard",foot,V(q*.12f,.18f,.04f),V(q*.20f,-.02f,-.29f),.13f,white,.065f);
                Stripe(shin,V(-.07f,-.49f,-.267f),1);
                Decal(shin,V(.0f,-.67f,-.285f),new Vector2(.12f,.12f),s<0?4:5);
                Decal(thigh,V(.0f,-.27f,-.24f),new Vector2(.10f,.10f),6);
            }
        }
        static void Arms(Transform model)
        {
            for(int s=-1;s<=1;s+=2)
            {
                string side=s<0?"L":"R";
                var shoulder=G("06 "+side+" shoulder gimbal",model,V(s*.63f,3.48f,.0f),V(0,s*-5,s*13));
                Joint("Shoulder",shoulder,V(0,0,0),.17f);
                var shell=G(side+" layered pauldron",shoulder,V(s*.06f,.08f,0));
                MP("Crimson pauldron upper volume",shell,s,red,.35f,V(-.12f,.14f,-.17f),V(.18f,.33f,-.12f),V(.53f,.23f,-.06f),V(.38f,-.10f,-.18f),V(.15f,-.18f,-.25f),V(-.12f,-.02f,-.22f));
                MP("Pauldron ivory upper edge",shell,s,white,.09f,V(-.08f,.17f,-.24f),V(.19f,.31f,-.19f),V(.46f,.23f,-.15f),V(.23f,.10f,-.28f),V(.07f,.11f,-.30f));
                MP("Pauldron crimson raised blade",shell,s,scarlet,.055f,V(.13f,.21f,-.29f),V(.30f,.42f,-.17f),V(.50f,.23f,-.18f),V(.26f,.14f,-.31f));
                Blade("Pauldron gold shoulder edge",shell,V(s*.06f,.08f,-.32f),V(s*.44f,.20f,-.22f),.065f,gold,.02f);
                Blade("Pauldron outer white blade",shell,V(s*.27f,.09f,-.19f),V(s*.49f,-.24f,-.20f),.20f,white,.08f);
                Blade("Pauldron lower red blade",shell,V(s*.28f,-.01f,-.08f),V(s*.38f,-.36f,-.08f),.21f,red,.09f);
                Vent(shell,V(s*.18f,-.05f,-.29f),V(.19f,.10f,.035f),3);
                Rivet(shell,V(s*.27f,.17f,-.32f));Stripe(shell,V(s*.09f,.19f,-.31f),s);
                Decal(shell,V(s*.19f,.20f,-.345f),new Vector2(.15f,.11f),s<0?0:1,s*14);
                var upper=G(side+" humerus",shoulder,V(0,-.20f,0));
                Hull("Upper arm black skeleton",upper,dark,S(-.34f,.19f,.23f),S(.02f,.24f,.27f));
                Hull("Ivory upper arm shell",upper,white,S(-.36f,.25f,.26f),S(-.08f,.28f,.31f),S(.02f,.22f,.28f));
                Blade("Upper arm crimson panel",upper,V(0,-.05f,-.18f),V(0,-.33f,-.18f),.13f,red,.04f);
                var elbow=G(side+" elbow hinge",shoulder,V(0,-.66f,0),V(s<0?-7:-18,0,0));
                Joint("Elbow",elbow,Vector3.zero,.135f);
                var forearm=G(side+" forearm gauntlet",elbow,V(0,-.16f,0));
                Hull("Gauntlet dark frame",forearm,dark,S(-.45f,.21f,.26f),S(-.08f,.29f,.32f),S(.03f,.23f,.28f));
                P("Gauntlet crimson primary plate",forearm,red,.10f,V(-.13f,.04f,-.18f),V(.13f,.04f,-.18f),V(.18f,-.21f,-.19f),V(.10f,-.47f,-.19f),V(-.10f,-.47f,-.19f),V(-.18f,-.21f,-.19f));
                Blade("Gauntlet ivory pointed overlay",forearm,V(0,-.06f,-.24f),V(0,-.52f,-.24f),.17f,white,.045f);
                for(int q=-1;q<=1;q+=2)
                {
                    Blade("Gauntlet gold edging",forearm,V(q*.11f,.01f,-.22f),V(q*.15f,-.29f,-.24f),.035f,gold,.02f);
                    Rivet(forearm,V(q*.10f,-.15f,-.25f));
                }
                var hand=G(side+" wrist and segmented fingers",elbow,V(0,-.73f,-.01f));
                SDM.Beam("Wrist swivel",hand,V(0,.0f,0),V(0,.12f,0),.07f,steel,16);
                SDM.Box("Hand armored palm",hand,V(0,-.08f,.0f),V(.19f,.17f,.14f),dark);
                SDM.Box("Hand ivory dorsal plate",hand,V(0,-.055f,-.09f),V(.18f,.13f,.05f),white);
                for(int i=0;i<4;i++)for(int j=0;j<3;j++)SDM.Box("Finger "+(i+1)+" phalanx "+(j+1),hand,V(-.072f+i*.048f,-.15f-j*.037f,.005f-j*.028f),V(.037f,.044f,.054f),j==0?steel:dark,V(-j*25,0,0));
                SDM.Box("Opposing thumb base",hand,V(s*.12f,-.10f,-.005f),V(.055f,.08f,.065f),steel,V(0,0,s*30));
                SDM.Box("Thumb tip",hand,V(s*.13f,-.16f,-.05f),V(.052f,.06f,.06f),dark);
                if(s<0) Lance(G("07 Long beam glaive",hand,V(0,-.14f,-.045f),V(0,0,-31)));
                else Rifle(G("08 Variable beam rifle and shield",forearm,V(.14f,-.15f,-.04f),V(0,0,25)));
            }
        }
        static void Lance(Transform t)
        {
            SDM.Beam("Glaive titanium haft",t,V(0,.62f,0),V(0,-.58f,0),.035f,steel,16);
            for(int i=0;i<10;i++)SDM.Ring("Haft grip ridge",t,V(0,-.25f+i*.05f,0),.043f,.008f,dark,V(90,0,0));
            SDM.Box("Glaive guard",t,V(0,-.48f,0),V(.38f,.09f,.14f),gold);
            Blade("Glaive crimson chassis",t,V(0,-.48f,0),V(0,-2.25f,-.01f),.32f,red,.10f);
            Blade("Glaive white cutting edge",t,V(-.035f,-.68f,-.08f),V(0,-2.37f,-.075f),.16f,white,.035f);
            Blade("Glaive gold blade root",t,V(.07f,-.50f,-.07f),V(.04f,-1.43f,-.10f),.09f,gold,.025f);
            Line("Glaive inset line",t,V(-.03f,-.87f,-.13f),V(0,-2.12f,-.11f),dark,.005f);
            Blade("Glaive counterweight",t,V(0,.42f,0),V(0,.83f,0),.12f,red,.07f);
        }
        static void Rifle(Transform t)
        {
            Hull("Rifle reactor and magazine",t,dark,S(-.64f,.20f,.24f),S(.22f,.29f,.29f),S(.42f,.13f,.15f));
            P("Rifle gold outer chassis",t,gold,.18f,V(-.16f,.40f,-.13f),V(.24f,.24f,-.10f),V(.35f,-.31f,-.09f),V(.15f,-1.12f,-.12f),V(0,-1.50f,-.14f),V(-.16f,-.56f,-.17f));
            P("Rifle deep crimson shield panel",t,red,.065f,V(-.11f,.25f,-.21f),V(.17f,.16f,-.20f),V(.26f,-.27f,-.19f),V(.08f,-.97f,-.22f),V(0,-1.31f,-.22f),V(-.10f,-.49f,-.25f));
            Blade("Rifle ivory outer cutting fin",t,V(.20f,-.21f,-.22f),V(.02f,-1.53f,-.23f),.17f,white,.035f);
            Blade("Rifle ivory inner fin",t,V(-.10f,.06f,-.26f),V(-.17f,-.72f,-.28f),.12f,white,.04f);
            Disc("Rifle reactor dark bed",t,V(.025f,.08f,-.29f),.13f,dark);
            SDM.Ring("Rifle reactor golden ring",t,V(.025f,.08f,-.32f),.10f,.022f,gold);
            Disc("Rifle green power gauge",t,V(.025f,.08f,-.35f),.044f,glow);
            for(int i=0;i<5;i++)SDM.Box("Rifle heat dispersal fin",t,V(.01f,-.30f-i*.07f,-.28f),V(.13f,.02f,.025f),steel,V(0,0,15));
            Line("Rifle barrel rail left",t,V(-.045f,-.50f,-.27f),V(0,-1.25f,-.28f),gold,.009f);
            for(int i=0;i<3;i++)Rivet(t,V(.14f-i*.024f,-.24f-i*.18f,-.28f));
        }
        static Transform Wings(Transform model)
        {
            var wings=G("09 Twelve-blade wing binder assembly",model,V(0,3.20f,.35f));
            Hull("Backpack central red spine",wings,red,S(-.58f,.32f,.34f,.12f),S(.0f,.49f,.44f,.12f),S(.58f,.37f,.36f,.12f));
            for(int i=0;i<7;i++){SDM.Box("Dorsal gold vertebra",wings,V(0,-.43f+i*.145f,.36f),V(.13f,.06f,.045f),gold);}
            for(int s=-1;s<=1;s+=2)
            {
                var carrier=G((s<0?"L":"R")+" wing carrier pivot",wings,V(s*.25f,.05f,.07f),V(0,s*-10,0));
                SDM.Beam("Binder titanium load arm",carrier,V(0,0,0),V(s*.50f,.20f,.03f),.11f,dark,16);
                SDM.Beam("Binder chrome hydraulic",carrier,V(s*.02f,-.15f,-.02f),V(s*.50f,.09f,-.02f),.027f,steel,16);
                Joint("Binder bearing",carrier,V(s*.42f,.12f,-.01f),.16f);
                Vector3[] tips={V(1.40f,2.35f,.05f),V(2.35f,1.78f,.12f),V(3.18f,.88f,.19f),V(3.02f,.08f,.25f),V(2.51f,-.69f,.30f),V(1.65f,-1.25f,.35f)};
                float[] widths={.46f,.51f,.53f,.48f,.41f,.35f};
                for(int i=0;i<tips.Length;i++)
                {
                    var blade=G((s<0?"L":"R")+" detachable blade binder "+(i+1).ToString("D2"),carrier);
                    Vector3 a=V(s*(.24f+i*.025f),.27f-i*.10f,-i*.027f), b=V(s*tips[i].x,tips[i].y,tips[i].z);
                    WingBlade(blade,a,b,widths[i],s,i);
                }
                var thruster=G((s<0?"L":"R")+" backpack booster",wings,V(s*.30f,-.40f,.22f),V(85,0,0));
                Hull("Booster gold casing",thruster,gold,S(-.20f,.24f,.24f),S(.16f,.17f,.17f));
                SDM.Beam("Booster dark exhaust",thruster,V(0,-.22f,0),V(0,-.16f,0),.095f,black,20);
                SDM.Ring("Booster nozzle lip",thruster,V(0,-.22f,0),.10f,.017f,steel,V(90,0,0));
                SDM.Beam("Recessed booster emitter",thruster,V(0,-.217f,0),V(0,-.21f,0),.055f,glow,20);
            }
            return wings;
        }
        static void WingBlade(Transform t,Vector3 a,Vector3 b,float width,int sign,int index)
        {
            Vector3 d=b-a,side=Vector3.Cross(d.normalized,Vector3.forward).normalized;
            Blade("Binder graphite structural spar",t,a,b,width*.81f,dark,.13f);
            Blade("Crimson primary blade armor",t,a-V(0,0,.045f),a+d*.96f-V(0,0,.045f),width,red,.10f);
            Blade("Scarlet raised outer armor",t,a+d*.12f+side*width*.17f-V(0,0,.105f),a+d*.76f+side*width*.12f-V(0,0,.105f),width*.38f,scarlet,.04f);
            Blade("Ivory long cutting tip",t,a+d*.55f-V(0,0,.12f),b-V(0,0,.105f),width*.64f,white,.048f);
            Blade("Gold vein trailing edge",t,a+d*.12f-side*width*.19f-V(0,0,.12f),a+d*.79f-side*width*.14f-V(0,0,.13f),width*.14f,gold,.025f);
            Blade("Ivory split basal vane",t,a+d*.10f+side*width*.21f-V(0,0,.13f),a+d*.49f+side*width*.18f-V(0,0,.14f),width*.30f,white,.045f);
            Blade("Offset crimson trailing spur",t,a+d*.37f-side*width*.23f+V(0,0,.04f),a+d*.81f-side*width*.57f+V(0,0,.04f),width*.36f,red,.065f);
            Blade("Gold spur spine",t,a+d*.43f-side*width*.24f-V(0,0,.035f),a+d*.74f-side*width*.47f-V(0,0,.04f),width*.10f,gold,.016f);
            // Stepped armor over the root breaks the straight feather silhouette.
            for(int k=0;k<3;k++)
            {
                float u=.15f+k*.145f;Vector3 c=a+d*u-V(0,0,.18f);
                Vector3 e=d.normalized;
                P("Stepped crimson wing root segment "+k,t,k%2==0?red:scarlet,.035f,c-side*width*.20f,c+side*width*.20f,c+e*.22f+side*width*.24f,c+e*.34f-side*width*.06f,c+e*.18f-side*width*.28f);
                Line("Stepped armor gold inner lip",t,c-side*width*.20f-V(0,0,.012f),c+e*.18f-side*width*.28f-V(0,0,.012f),gold,.007f);
            }
            // Matching dorsal shells keep the rear inspection detailed as well.
            var rear=G("Dorsal blade detailing",t,V(0,0,.16f));
            Blade("Dorsal crimson armor volume",rear,a+d*.05f,a+d*.92f,width*.88f,red,.055f);
            Blade("Dorsal gold machined spar",rear,a+d*.10f+V(0,0,.075f),a+d*.79f+V(0,0,.075f),width*.16f,gold,.025f);
            Blade("Dorsal ivory blade tip",rear,a+d*.58f+V(0,0,.08f),b+V(0,0,.08f),width*.47f,white,.035f);
            for(int k=0;k<4;k++)
            {
                Vector3 p=a+d*(.20f+k*.11f)+V(0,0,.09f);
                SDM.Beam("Dorsal exposed silver connector",rear,p-side*width*.17f,p+side*width*.17f,.009f,steel,8);
                Disc("Dorsal binder screw",rear,p+side*width*.18f+V(0,0,.03f),.013f,gold);
            }
            for(int j=0;j<3;j++)
            {
                float u=.24f+j*.13f;Vector3 p=a+d*u-V(0,0,.155f);
                Line("Binder inset panel seam",t,p-side*width*.19f,p+side*width*.17f,black,.005f);
                Rivet(t,p-side*width*.15f,.011f);
                // Angular gold filigree inlaid into red armor, not a flat printed texture.
                Vector3 c=a+d*(u+.045f)-V(0,0,.165f);
                Line("Gold ornament chevron A",t,c-side*width*.14f,c+d.normalized*.08f,gold,.006f);
                Line("Gold ornament chevron B",t,c+d.normalized*.08f,c+side*width*.08f,gold,.006f);
            }
            Joint("Blade pivot",t,a+d*.08f-V(0,0,.14f),width*.15f);
            Decal(t,a+d*.75f-V(0,0,.19f),new Vector2(width*.25f,width*.25f),index%2==0?10:11,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90);
            SDM.Beam("Blade actuator sleeve",t,a+d*.09f+side*width*.21f,a+d*.26f+side*width*.21f,.029f,dark,12);
            SDM.Beam("Blade actuator chrome ram",t,a+d*.24f+side*width*.21f,a+d*.40f+side*width*.18f,.017f,steel,12);
        }
        static void Head(Transform t)
        {
            Hull("Helmet dark understructure",t,dark,S(-.16f,.22f,.22f),S(.03f,.35f,.29f),S(.22f,.27f,.24f),S(.27f,.15f,.15f));
            var rear=G("Helmet rear armored cap",t,V(0,.02f,.12f),V(0,180,0));
            P("Ivory occipital armor",rear,white,.07f,V(-.12f,.16f,-.055f),V(.12f,.16f,-.055f),V(.16f,.03f,-.08f),V(.10f,-.16f,-.04f),V(-.10f,-.16f,-.04f),V(-.16f,.03f,-.08f));
            Blade("Helmet red rear ridge",rear,V(0,.22f,-.10f),V(0,-.15f,-.10f),.07f,red,.03f);
            Vent(rear,V(0,-.06f,-.14f),V(.13f,.07f,.015f),3);
            P("Narrow ivory face mask",t,white,.085f,V(-.10f,.015f,-.20f),V(.10f,.015f,-.20f),V(.085f,-.12f,-.22f),V(0,-.21f,-.24f),V(-.085f,-.12f,-.22f));
            P("Brow shadow visor",t,black,.025f,V(-.14f,.065f,-.215f),V(.14f,.065f,-.215f),V(.10f,-.012f,-.245f),V(0,.015f,-.26f),V(-.10f,-.012f,-.245f));
            for(int s=-1;s<=1;s+=2)
            {
                MP("Slit emerald eye",t,s,glow,.008f,V(.014f,.038f,-.262f),V(.112f,.049f,-.245f),V(.085f,.005f,-.263f),V(.02f,.012f,-.272f));
                MP("Ivory angular helmet cheek",t,s,white,.08f,V(.105f,.21f,-.13f),V(.18f,.13f,-.08f),V(.20f,-.07f,-.10f),V(.11f,-.23f,-.18f),V(.09f,-.04f,-.23f),V(.14f,.04f,-.19f));
                Blade("Cheek gold edge",t,V(s*.18f,.08f,-.17f),V(s*.13f,-.18f,-.25f),.038f,gold,.017f);
                Blade("Gold V fin foundation",t,V(s*.02f,.11f,-.22f),V(s*.38f,.33f,-.15f),.095f,gold,.026f);
                Blade("Ivory swept antenna",t,V(s*.08f,.16f,-.15f),V(s*.28f,.64f,-.015f),.105f,white,.04f);
                Blade("Crimson inner antenna",t,V(s*.065f,.18f,-.19f),V(s*.18f,.51f,-.10f),.065f,red,.035f);
                SDM.Box("Helmet side sensor",t,V(s*.19f,.025f,0),V(.045f,.14f,.10f),red);
                for(int i=0;i<3;i++)Line("Face mask vent",t,V(s*.025f,-.065f-i*.025f,-.245f),V(s*.07f,-.055f-i*.025f,-.24f),dark,.004f);
            }
            Blade("Crimson central crown",t,V(0,.12f,-.19f),V(0,.47f,-.06f),.10f,red,.07f);
            SDM.Box("Forehead gold sensor mount",t,V(0,.235f,-.18f),V(.082f,.13f,.035f),gold);
            SDM.Box("Forehead emerald sensor",t,V(0,.24f,-.204f),V(.049f,.085f,.018f),glow);
            Blade("Crimson chin spear",t,V(0,-.085f,-.25f),V(0,-.235f,-.26f),.07f,red,.045f);
        }
    }
}

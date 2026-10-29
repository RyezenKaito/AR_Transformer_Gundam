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
using static BlueFlame.Editor.BFG;

namespace BlueFlame.Editor
{
    public static partial class BlueFlameBuilder
    {
        public const string Root="Assets/Models/BlueFlame";
        public const string ScenePath=Root+"/Scenes/BlueFlame_Atelier.unity";
        [MenuItem("BlueFlame/Build full model and atelier")]
        public static void BuildFull()=>Build(false);
        [MenuItem("BlueFlame/Preview head and thorax study")]
        public static void BuildStudy()=>Build(true);
        static void Build(bool study)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before building.");
            foreach(string d in new[]{"Meshes","Materials","Textures","Settings","Prefabs","Scenes"})Directory.CreateDirectory(Root+"/"+d);
            Directory.CreateDirectory("Docs/BlueFlame");Directory.CreateDirectory("Docs/Art/Renders/BlueFlame");
            var previous=SceneManager.GetActiveScene();
            if(previous.isDirty && !previous.name.StartsWith("BlueFlame"))
            {
                var backup=Root+"/Scenes/Preserved_"+previous.name+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";
                if(!EditorSceneManager.SaveScene(previous,backup,true))throw new IOException("Unable to preserve previous unsaved scene.");
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);scene.name=study?"BlueFlame_Study":"BlueFlame_Atelier";
            Meshes.Clear();AssetDatabase.Refresh();Materials();
            var model=G("BLUEFLAME CH-01 | Mechanical display model",null);
            Body(model);Transform wings=null;
            if(!study){Pelvis(model);Legs(model);Arms(model);wings=Flight(model);}
            Refine(model,study);
            Transform energy=study?null:Energy(model);
            Stage();Lighting();
            var viewer=G("BlueFlame / Inspection controls",null).gameObject.AddComponent<BlueFlame.BlueFlameViewer>();viewer.viewCamera=Camera.main;viewer.wings=wings;viewer.energy=energy;
            if(study)viewer.SetView(12,3,5.7f,V(0,5.94f,0));else viewer.SetView(23,6,19.5f,V(0,3.60f,0));
            if(!study)
            {
                SaveMeshes();
                PrefabUtility.SaveAsPrefabAssetAndConnect(model.gameObject,Root+"/Prefabs/BlueFlame_Detailed.prefab",InteractionMode.AutomatedAction);
                var body=UnityEngine.Object.Instantiate(model.gameObject);body.name="BlueFlame / Body and weapons";UnityEngine.Object.DestroyImmediate(body.transform.Cast<Transform>().First(t=>t.name==wings.name).gameObject);UnityEngine.Object.DestroyImmediate(body.transform.Cast<Transform>().First(t=>t.name==energy.name).gameObject);
                PrefabUtility.SaveAsPrefabAsset(body,Root+"/Prefabs/BlueFlame_Body.prefab");UnityEngine.Object.DestroyImmediate(body);
                var pack=UnityEngine.Object.Instantiate(wings.gameObject);pack.name="BlueFlame / Flight unit";PrefabUtility.SaveAsPrefabAsset(pack,Root+"/Prefabs/BlueFlame_FlightUnit.prefab");UnityEngine.Object.DestroyImmediate(pack);
                var fx=UnityEngine.Object.Instantiate(energy.gameObject);fx.name="BlueFlame / Optional energy";fx.SetActive(true);PrefabUtility.SaveAsPrefabAsset(fx,Root+"/Prefabs/BlueFlame_EnergyFX.prefab");UnityEngine.Object.DestroyImmediate(fx);
                var combined=Combine(model);PrefabUtility.SaveAsPrefabAsset(combined,Root+"/Prefabs/BlueFlame_Combined.prefab");
                var rs=model.GetComponentsInChildren<MeshRenderer>();float low=rs.Min(r=>r.bounds.min.y),high=rs.Max(r=>r.bounds.max.y);float factor=.25f/(high-low);
                var ar=new GameObject("BlueFlame / 25cm display");combined.transform.SetParent(ar.transform,false);combined.transform.localPosition=V(0,-low,0);ar.transform.localScale=Vector3.one*factor;
                PrefabUtility.SaveAsPrefabAsset(ar,Root+"/Prefabs/BlueFlame_AR_25cm.prefab");UnityEngine.Object.DestroyImmediate(ar);
                EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
                var filters=model.GetComponentsInChildren<MeshFilter>();
                File.WriteAllText("Docs/BlueFlame/BuildReport.txt","BlueFlame CH-01 reference-based authored interpretation\nParts="+filters.Length+"\nTriangles="+filters.Sum(f=>(long)f.sharedMesh.triangles.Length/3)+"\nMaterials="+rs.Select(r=>r.sharedMaterial).Distinct().Count()+"\nAR model height=0.25 m\nScene="+ScenePath+"\n");
                Debug.Log(File.ReadAllText("Docs/BlueFlame/BuildReport.txt"));
            }
            Selection.activeGameObject=model.gameObject;
            if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(study?V(0,5.95f):V(0,3.6f),Quaternion.Euler(5,13,0),study?3:11);
        }
        static void SaveMeshes()
        {
            var path=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/BlueFlame_AuthoredGeometry.asset");
            AssetDatabase.CreateAsset(Meshes[0],path);for(int i=1;i<Meshes.Count;i++)AssetDatabase.AddObjectToAsset(Meshes[i],path);AssetDatabase.SaveAssets();
        }
        static GameObject Combine(Transform source)
        {
            var root=new GameObject("BlueFlame / Combined display");string container=null;
            foreach(var g in source.GetComponentsInChildren<MeshRenderer>().GroupBy(r=>r.sharedMaterial))
            {
                var mesh=new Mesh{name="BF_Batched_"+g.Key.name,indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(g.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=source.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray(),true,true);
                if(container==null){container=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/BlueFlame_CombinedGeometry.asset");AssetDatabase.CreateAsset(mesh,container);}else AssetDatabase.AddObjectToAsset(mesh,container);
                BFG.Object(g.Key.name,root.transform,mesh,g.Key);
            }
            var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var collider=root.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size;return root;
        }
        static Material Mat(string n,Color c,float metal,float smooth)
        {
            string path=Root+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.name=n;m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;
            if(metal>.1f)
            {
                string tp=Root+"/Textures/"+n+"_MetallicSmoothness.png";
                if(!File.Exists(tp))
                {
                    var t=new Texture2D(512,512,TextureFormat.RGBA32,false,true);var colors=new Color[512*512];var random=new System.Random(1701+n.Length);
                    for(int y=0;y<512;y++)for(int x=0;x<512;x++)
                    {float noise=(float)random.NextDouble()-.5f;float broad=Mathf.PerlinNoise(x*.026f,y*.026f)-.5f;float brush=Mathf.Sin(y*2.9f+x*.006f)*.012f;colors[y*512+x]=new Color(metal,0,0,Mathf.Clamp01(smooth+noise*.025f+broad*.045f+brush));}
                    t.SetPixels(colors);t.Apply();File.WriteAllBytes(tp,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(tp);
                    var ti=(TextureImporter)AssetImporter.GetAtPath(tp);ti.sRGBTexture=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.SaveAndReimport();
                }
                m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tp));m.SetFloat("_Smoothness",1);m.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            EditorUtility.SetDirty(m);return m;
        }
        static void Materials()
        {
            white=Mat("01_IceWhite_CeramicLacquer",new Color(.79f,.84f,.89f),.18f,.43f);
            porcelain=Mat("02_CoolWhite_ArmorInset",new Color(.61f,.69f,.77f),.25f,.38f);
            blue=Mat("03_CobaltBlue_AnodizedAlloy",new Color(.012f,.071f,.52f),.66f,.57f);
            navy=Mat("04_DeepBlue_StructuralCarrier",new Color(.014f,.026f,.075f),.61f,.42f);
            frame=Mat("05_DarkGunmetal_CastFrame",new Color(.055f,.073f,.093f),.82f,.36f);
            steel=Mat("06_MachinedTitanium",new Color(.36f,.43f,.50f),.94f,.65f);
            gold=Mat("07_ChampagneGold",new Color(.78f,.49f,.16f),.92f,.60f);
            red=Mat("08_Crimson_Enamel",new Color(.40f,.018f,.035f),.48f,.48f);
            black=Mat("09_Recesses_AndSeals",new Color(.008f,.011f,.016f),.05f,.24f);
            cyan=Mat("10_Cyan_OpticalEmission",new Color(.015f,.53f,.83f),.18f,.62f);cyan.EnableKeyword("_EMISSION");cyan.SetColor("_EmissionColor",new Color(.01f,.70f,1)*2.4f);
            glass=Mat("11_Reactor_SapphireLens",new Color(.012f,.28f,.48f),.60f,.90f);glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.01f,.33f,.65f)*.6f);
            floor=Mat("12_Studio_Graphite",new Color(.036f,.043f,.055f),.12f,.26f);
            decal=Mat("13_ServiceStencils",Color.white,0,.30f);
            var atlasPath=Root+"/Textures/BlueFlame_ServiceMarkings.png";var ti=AssetImporter.GetAtPath(atlasPath) as TextureImporter;if(ti!=null){ti.alphaIsTransparency=true;ti.mipmapEnabled=true;ti.maxTextureSize=2048;ti.SaveAndReimport();}
            decal.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath));decal.SetFloat("_Surface",1);decal.SetFloat("_Blend",0);decal.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);decal.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);decal.SetInt("_ZWrite",0);decal.SetOverrideTag("RenderType","Transparent");decal.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");decal.renderQueue=3000;
            foreach(var m in new[]{cyan,glass}){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;MaterialEditor.FixupEmissiveFlag(m);m.EnableKeyword("_EMISSION");}
            glass.SetColor("_EmissionColor",new Color(.08f,.85f,1.15f)*2.2f);
            foreach(var m in new[]{cyan,glass,decal})EditorUtility.SetDirty(m);AssetDatabase.SaveAssets();
        }
        static void Stage()
        {
            var p=G("DISPLAY / Precision-machined exhibition plinth",null);
            Rod("Octagonal foundation",p,V(0,-.23f,0),V(0,-.06f,0),2.05f,frame,8);
            Rod("Upper display deck",p,V(0,-.06f,0),V(0,.07f,0),1.98f,floor,8);
            Tube("Titanium perimeter inlay",p,V(0,.075f,0),1.82f,1.804f,.009f,steel,V(90,0,0));
            for(int i=0;i<32;i++){float a=i*Mathf.PI/16;Rod("Deck engraved index",p,V(Mathf.Cos(a)*1.70f,.077f,Mathf.Sin(a)*1.70f),V(Mathf.Cos(a)*1.76f,.077f,Mathf.Sin(a)*1.76f),.004f,steel,6);}
            var name=G("BlueFlame identification",p,V(0,.087f,-1.40f),V(90,0,0));
            var tm=name.gameObject.AddComponent<TextMesh>();tm.text="BLUEFLAME  /  CH-01";tm.fontSize=72;tm.characterSize=.035f;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(.66f,.74f,.83f);
            var sub=G("Display series",p,V(0,.088f,-1.58f),V(90,0,0)).gameObject.AddComponent<TextMesh>();sub.text="MECHANICAL STUDY     /     01";sub.fontSize=48;sub.characterSize=.020f;sub.anchor=TextAnchor.MiddleCenter;sub.color=new Color(.35f,.48f,.6f);
            B("Studio floor",p,V(0,-.29f,0),V(160,.1f,160),floor);p.Find("Studio floor").GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            var geo=new Geo();
            for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI*.5f/40,b=(i+1)*Mathf.PI*.5f/40;
                var v0=V(-50,-.24f+8*(1-Mathf.Cos(a)),3+8*Mathf.Sin(a));var v1=V(50,v0.y,v0.z);
                var v2=V(50,-.24f+8*(1-Mathf.Cos(b)),3+8*Mathf.Sin(b));var v3=V(-50,v2.y,v2.z);
                var n0=V(0,Mathf.Cos(a),-Mathf.Sin(a));var n1=V(0,Mathf.Cos(b),-Mathf.Sin(b));geo.SmoothFace(new[]{v0,v3,v2,v1},new[]{n0,n1,n1,n0});
            }
            geo.Face(V(-50,7.76f,11),V(-50,50,11),V(50,50,11),V(50,7.76f,11));
            BFG.Object("Seamless studio cyclorama",p,geo.Finish("Cyclorama"),floor).GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
        public static void Capture(string name,int width=2000,int height=1600)
        {
            var c=Camera.main;var previous=c.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);Texture2D image=null;
            try{var request=new UniversalRenderPipeline.SingleCameraRequest{destination=rt};RenderPipeline.SubmitRenderRequest(c,request);RenderTexture.active=rt;image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory("Docs/Art/Renders/BlueFlame");File.WriteAllBytes("Docs/Art/Renders/BlueFlame/"+name+".png",image.EncodeToPNG());}
            finally{c.targetTexture=previous;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);if(image)UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}

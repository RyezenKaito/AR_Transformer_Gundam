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

namespace HaoLing.Editor
{
    public static class HaoLingBuilder
    {
        public const string Root="Assets/Models/HaoLing";
        public const string ScenePath=Root+"/Scenes/HaoLing_Phoenix_Atelier.unity";
        static Material stage,stageEdge,lettering;
        [MenuItem("Hao Ling/Build detailed trio and showcase")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop play mode before building.");
            foreach(string d in new[]{"Materials","Textures","Meshes","Prefabs","Scenes","Backups","Renders","Settings"})Directory.CreateDirectory(Root+"/"+d);
            var old=SceneManager.GetActiveScene();
            if(old.IsValid()&&old.isDirty)
            {
                string backup=Root+"/Backups/"+old.name+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity";
                if(!EditorSceneManager.SaveScene(old,backup,true))throw new IOException("Could not preserve open scene.");
                Debug.Log("HAOLING previous unsaved scene preserved: "+backup);
            }
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            HLM.Meshes.Clear();Materials();
            var trio=HLM.Group("HAO LING | White Emperor & twin phoenix",null);
            var robot=HaoLingRobot.Build(trio);
            float sole=robot.GetComponentsInChildren<Renderer>().Min(r=>r.bounds.min.y);
            foreach(Transform child in robot)child.localPosition-=Vector3.up*sole;
            robot.localPosition=Vector3.up*.067f;
            var feng=HaoLingPhoenix.Build(trio,"MC-02F · FENG",false);
            var huang=HaoLingPhoenix.Build(trio,"MC-02H · HUANG",true);
            feng.localPosition=new Vector3(-2.25f,3.45f,.60f);feng.localEulerAngles=new Vector3(-6,-24,-27);
            huang.localPosition=new Vector3(2.40f,2.55f,.45f);huang.localEulerAngles=new Vector3(-5,27,24);
            Stage(); Lighting();
            SaveMeshes();
            var detailed=PrefabUtility.SaveAsPrefabAsset(trio.gameObject,Root+"/Prefabs/HaoLing_Trio_Detailed.prefab");
            var robotSolo=CloneForPrefab(robot,"EMP02_HaoLing_Detailed");
            var fengSolo=CloneForPrefab(feng,"MC02F_Feng_Detailed");
            var huangSolo=CloneForPrefab(huang,"MC02H_Huang_Detailed");
            var optimized=Combine(trio);
            PrefabUtility.SaveAsPrefabAsset(optimized.gameObject,Root+"/Prefabs/HaoLing_Trio_Combined.prefab");
            optimized.localScale=Vector3.one*.075f;
            PrefabUtility.SaveAsPrefabAsset(optimized.gameObject,Root+"/Prefabs/HaoLing_Trio_AR_25cm.prefab");
            UnityEngine.Object.DestroyImmediate(optimized.gameObject);
            var camera=Camera.main;
            var viewer=new GameObject("Atelier | orbit controls").AddComponent<HaoLing.HaoLingShowcase>();
            viewer.viewCamera=camera;viewer.display=trio;viewer.feng=feng;viewer.huang=huang;viewer.distance=11.8f;viewer.yaw=9;viewer.pitch=8;viewer.target=new Vector3(0,2.25f,0);
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Selection.activeGameObject=trio.gameObject;
            if(SceneView.lastActiveSceneView){SceneView.lastActiveSceneView.LookAt(new Vector3(0,2.0f,0),Quaternion.Euler(9,9,0),7.5f);SceneView.lastActiveSceneView.sceneLighting=true;}
            var rends=trio.GetComponentsInChildren<MeshRenderer>();
            long triangles=trio.GetComponentsInChildren<MeshFilter>().Sum(x=>(long)x.sharedMesh.triangles.Length/3);
            string report="HAO LING BUILD OK | mesh parts="+rends.Length+" | triangles="+triangles+" | materials="+rends.Select(r=>r.sharedMaterial).Distinct().Count()+" | scene="+ScenePath;
            File.WriteAllText(Root+"/BuildReport.txt",report+"\nDetailed authored reconstruction from photographs; obscured rear surfaces inferred.\n");
            Debug.Log(report);
        }
        static GameObject CloneForPrefab(Transform source,string name)
        {
            var go=UnityEngine.Object.Instantiate(source.gameObject);go.name=name;go.transform.SetParent(null);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return null;
        }
        static void SaveMeshes()
        {
            string path=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/HaoLing_AuthoredGeometry.asset");
            if(HLM.Meshes.Count==0)return;
            AssetDatabase.CreateAsset(HLM.Meshes[0],path);
            for(int i=1;i<HLM.Meshes.Count;i++)AssetDatabase.AddObjectToAsset(HLM.Meshes[i],path);
            AssetDatabase.SaveAssets();
        }
        static Transform Combine(Transform source)
        {
            var output=HLM.Group("HAO LING | Combined geometry",null);
            foreach(Transform unit in source)
            {
                var target=HLM.Group(unit.name,output,unit.localPosition,unit.localEulerAngles);target.localScale=unit.localScale;
                var groups=new Dictionary<Material,List<CombineInstance>>();
                foreach(var r in unit.GetComponentsInChildren<MeshRenderer>())
                {
                    var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;
                    var mat=r.sharedMaterial;if(!groups.ContainsKey(mat))groups[mat]=new List<CombineInstance>();
                    groups[mat].Add(new CombineInstance{mesh=mf.sharedMesh,transform=unit.worldToLocalMatrix*r.transform.localToWorldMatrix});
                }
                foreach(var kv in groups)
                {
                    var mesh=new Mesh{name=unit.name+"_"+kv.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(kv.Value.ToArray(),true,true);mesh.RecalculateBounds();
                    string asset=AssetDatabase.GenerateUniqueAssetPath(Root+"/Meshes/"+Safe(mesh.name)+".asset");AssetDatabase.CreateAsset(mesh,asset);
                    var go=HLM.Group(kv.Key.name,target).gameObject;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=kv.Key;
                }
                var box=target.gameObject.AddComponent<BoxCollider>();var b=new Bounds();bool init=false;
                foreach(var mf in target.GetComponentsInChildren<MeshFilter>()){if(!init){b=mf.sharedMesh.bounds;init=true;}else b.Encapsulate(mf.sharedMesh.bounds);}
                box.center=b.center;box.size=b.size;
            }
            return output;
        }
        static string Safe(string s){foreach(char c in Path.GetInvalidFileNameChars())s=s.Replace(c,'_');return s.Replace('·','_');}
        static void Materials()
        {
            Texture2D ceramic=Texture("Ceramic_Albedo",0),metal=Texture("Brushed_Alloy_Albedo",1),crystal=Texture("Ruby_Facets_Albedo",2),normal=Texture("MicroSurface_Normal",3),mask=Texture("MetallicSmoothness",4);
            HLM.White=Mat("01_Porcelain_white",new Color(.73f,.79f,.86f),.16f,.57f,ceramic,normal);
            HLM.Pearl=Mat("02_Pearl_highlight",new Color(.86f,.88f,.88f),.22f,.67f,ceramic,normal);
            HLM.Navy=Mat("03_Midnight_indigo",new Color(.035f,.044f,.10f),.65f,.47f,metal,normal);
            HLM.Frame=Mat("04_Titanium_skeleton",new Color(.12f,.15f,.19f),.83f,.54f,metal,normal);
            HLM.Gold=Mat("05_Champagne_gold",new Color(.67f,.48f,.22f),.82f,.65f,metal,normal);
            HLM.Black=Mat("06_Panel_recess",new Color(.009f,.014f,.024f),.32f,.3f,metal,null);
            HLM.Red=Mat("07_Carmine_edge",new Color(.42f,.018f,.065f),.62f,.66f,crystal,null);
            HLM.Ruby=Mat("08_Ruby_energy_crystal",new Color(.82f,.028f,.20f),.42f,.88f,crystal,null,new Color(.40f,.004f,.035f));
            HLM.Teal=Mat("09_Jade_optics",new Color(.005f,.58f,.56f),.50f,.85f,null,null,new Color(.01f,.70f,.62f)*1.5f);
            stage=Mat("10_Basalt_display",new Color(.023f,.031f,.040f),.35f,.31f,metal,normal);
            stageEdge=Mat("11_Machined_display_edge",new Color(.065f,.078f,.09f),.65f,.6f,metal,normal);
            lettering=Mat("12_Gold_engraving",new Color(.67f,.53f,.30f),.45f,.55f,null,null,new Color(.13f,.085f,.03f));
            // Retain distinct ceramic/alloy responses; a shared metallic map would override these scalars.
            foreach(var m in new[]{HLM.White,HLM.Pearl,HLM.Navy,HLM.Frame,HLM.Gold,HLM.Red}){m.SetTexture("_MetallicGlossMap",null);m.DisableKeyword("_METALLICSPECGLOSSMAP");}
            HLM.Ruby.color=new Color(.82f,.028f,.20f,.88f);
            HLM.Ruby.SetFloat("_Surface",1);HLM.Ruby.SetFloat("_Blend",0);HLM.Ruby.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);HLM.Ruby.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);HLM.Ruby.SetFloat("_ZWrite",0);
            HLM.Ruby.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");HLM.Ruby.SetOverrideTag("RenderType","Transparent");HLM.Ruby.renderQueue=3000;
        }
        static Material Mat(string name,Color color,float metallic,float smooth,Texture2D tex,Texture2D normal,Color? emission=null)
        {
            string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;
            m.SetTexture("_BaseMap",tex);m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",.12f);
            if(normal)m.EnableKeyword("_NORMALMAP");
            if(emission.HasValue){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission.Value);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
            EditorUtility.SetDirty(m);return m;
        }
        static Texture2D Texture(string name,int kind)
        {
            const int n=512;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color32[n*n];var rng=new System.Random(10727+kind);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float noise=(float)rng.NextDouble();float v=1;Color c=Color.white;
                if(kind==0){v=.966f+noise*.025f;float etch=(x%128==3&&y%128>96)?-.035f:0;c=new Color(v+etch,v+etch,v+etch);}
                if(kind==1){v=.86f+noise*.04f+.05f*Mathf.Sin(y*2.56f);c=new Color(v,v,v);}
                if(kind==2){v=.75f+.18f*Mathf.Abs(Mathf.Sin(x*.013f+y*.019f))+.05f*noise;c=new Color(v,v*.96f,v*.98f);}
                if(kind==3){c=new Color(.5f+(noise-.5f)*.025f,.5f+((float)rng.NextDouble()-.5f)*.025f,1);}
                if(kind==4){c=new Color(.74f,0,0,.72f+noise*.06f);}
                pixels[y*n+x]=c;
            }
            t.SetPixels32(pixels);t.Apply();string path=Root+"/Textures/"+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode=TextureWrapMode.Repeat;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;
            if(kind==3){importer.textureType=TextureImporterType.NormalMap;importer.sRGBTexture=false;}else if(kind==4)importer.sRGBTexture=false;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void Stage()
        {
            var stageRoot=HLM.Group("Atelier | octagonal display plinth",null);
            HLM.Beam("Lower octagonal foundation",stageRoot,new Vector3(0,-.18f,0),new Vector3(0,-.07f,0),2.13f,stageEdge,8);
            HLM.Beam("Basalt top slab",stageRoot,new Vector3(0,-.07f,0),new Vector3(0,.06f,0),2.05f,stage,8);
            HLM.Ring("Inlaid outer orbit",stageRoot,new Vector3(0,.067f,0),1.85f,.009f,HLM.Gold,new Vector3(90,0,0));
            HLM.Ring("Inlaid inner orbit",stageRoot,new Vector3(0,.068f,0),1.67f,.004f,HLM.Gold,new Vector3(90,0,0));
            for(int i=0;i<32;i++)
            {
                float a=i*Mathf.PI*2/32;Vector3 p=new Vector3(Mathf.Cos(a)*1.76f,.071f,Mathf.Sin(a)*1.76f);
                HLM.Box("Index engraving "+i,stageRoot,p,new Vector3(.025f,.005f,i%4==0?.11f:.045f),lettering,new Vector3(0,-a*Mathf.Rad2Deg,0));
            }
            for(int i=0;i<8;i++)
            {
                float a=(i+.5f)*Mathf.PI/4;var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                HLM.Beam("Radial inset "+i,stageRoot,d*.98f+Vector3.up*.069f,d*1.57f+Vector3.up*.069f,.006f,stageEdge,4);
                HLM.Box("Floating armor tile "+i,stageRoot,d*1.5f+Vector3.up*.08f,new Vector3(.25f,.055f,.13f),stageEdge,new Vector3(0,90-a*Mathf.Rad2Deg,0));
            }
            // Museum identification plaques are separate from the model.
            Label("HAO LING",new Vector3(0,.11f,-1.63f),.115f,lettering,stageRoot,new Vector3(72,0,0));
            Label("EMP-02   /   WHITE EMPEROR",new Vector3(0,.1f,-1.83f),.040f,lettering,stageRoot,new Vector3(72,0,0));
            var floor=HLM.Box("Infinite charcoal ground",stageRoot,new Vector3(0,-.22f,0),new Vector3(200,.05f,200),stage);
            // Curved studio sweep removes the hard horizon behind the floating birds.
            var verts=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=16;i++)
            {
                float angle=i*Mathf.PI*.5f/16;float yy=-.195f+8*(1-Mathf.Cos(angle)),zz=5+8*Mathf.Sin(angle);
                verts.Add(new Vector3(-60,yy,zz));verts.Add(new Vector3(60,yy,zz));
            }
            verts.Add(new Vector3(-60,50,13));verts.Add(new Vector3(60,50,13));
            for(int i=0;i<17;i++){int k=i*2;triangles.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
            var cyc=new Mesh{name="HL_StudioCyclorama"};cyc.SetVertices(verts);cyc.SetTriangles(triangles,0);cyc.RecalculateNormals();cyc.RecalculateBounds();HLM.Meshes.Add(cyc);
            var backdrop=HLM.Group("Curved studio backdrop",stageRoot).gameObject;backdrop.AddComponent<MeshFilter>().sharedMesh=cyc;backdrop.AddComponent<MeshRenderer>().sharedMaterial=stage;
        }
        static void Label(string text,Vector3 pos,float size,Material material,Transform parent,Vector3 rotation)
        {
            var go=HLM.Group(text,parent,pos,rotation).gameObject;var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=100;tm.characterSize=size*.1f;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=new Color(.75f,.63f,.40f);tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");go.GetComponent<MeshRenderer>().sharedMaterial=tm.font.material;
        }
        static void Lighting()
        {
            SetupStudioPipeline();
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.29f,.34f,.43f);RenderSettings.ambientEquatorColor=new Color(.19f,.22f,.28f);RenderSettings.ambientGroundColor=new Color(.07f,.08f,.11f);RenderSettings.ambientIntensity=1;
            RenderSettings.fog=false;RenderSettings.skybox=null;
            StudioReflection();
            Light("Key | large warm softbox",new Vector3(-3,7,-6),new Vector3(30,-30,0),new Color(1,.94f,.86f),1.65f,LightType.Directional);
            Light("Fill | cool softbox",new Vector3(5,4,-2),new Vector3(22,45,0),new Color(.68f,.81f,1),.7f,LightType.Directional);
            Light("Rim | silver separation",new Vector3(0,5,5),new Vector3(30,180,0),new Color(.71f,.83f,1),1.8f,LightType.Directional);
            var camera=HLM.Group("Atelier Camera",null).gameObject.AddComponent<Camera>();camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.012f,.017f,.025f);camera.fieldOfView=32;camera.nearClipPlane=.03f;camera.farClipPlane=180;camera.allowHDR=true;camera.allowMSAA=true;
            Vector3 target=new Vector3(0,2.25f,0);camera.transform.position=target+Quaternion.Euler(8,9,0)*new Vector3(0,0,-11.8f);camera.transform.LookAt(target);
            camera.gameObject.AddComponent<AudioListener>();var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var volume=HLM.Group("Atelier | color grading",null).gameObject.AddComponent<Volume>();volume.isGlobal=true;var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var adjust=profile.Add<ColorAdjustments>();adjust.postExposure.Override(.15f);adjust.contrast.Override(7);adjust.saturation.Override(-4);
            var bloom=profile.Add<Bloom>();bloom.intensity.Override(.12f);bloom.threshold.Override(1.3f);bloom.scatter.Override(.35f);
            string path=Root+"/Materials/AtelierVolume.asset";if(AssetDatabase.LoadAssetAtPath<VolumeProfile>(path))path=AssetDatabase.GenerateUniqueAssetPath(path);AssetDatabase.CreateAsset(profile,path);volume.sharedProfile=profile;
        }
        static void SetupStudioPipeline()
        {
            var current=QualitySettings.renderPipeline?QualitySettings.renderPipeline:GraphicsSettings.defaultRenderPipeline;
            if(current && current.name.StartsWith("HaoLing_Studio"))current=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Models/Unicorn Gundam/Demo/Generated/UnicornMobilePipeline.asset");
            var studio=UnityEngine.Object.Instantiate(current);studio.name="HaoLing_Studio_Pipeline";
            string path=AssetDatabase.GenerateUniqueAssetPath(Root+"/Settings/HaoLing_Studio_Pipeline.asset");AssetDatabase.CreateAsset(studio,path);
            var so=new SerializedObject(studio);
            foreach(string key in new[]{"m_MainLightShadowsSupported","m_SupportsHDR","m_SoftShadowsSupported","m_ReflectionProbeBlending","m_ReflectionProbeBoxProjection"}){var p=so.FindProperty(key);if(p!=null)p.boolValue=true;}
            foreach(var kv in new Dictionary<string,int>{{"m_AdditionalLightsRenderingMode",1},{"m_AdditionalLightsPerObjectLimit",8},{"m_MSAA",4},{"m_MainLightShadowmapResolution",2048},{"m_ShadowCascadeCount",2}}){var p=so.FindProperty(kv.Key);if(p!=null)p.intValue=kv.Value;}
            var distance=so.FindProperty("m_ShadowDistance");if(distance!=null)distance.floatValue=30;
            so.ApplyModifiedPropertiesWithoutUndo();
            var controller=HLM.Group("Atelier | studio rendering",null).gameObject.AddComponent<HaoLing.HaoLingStudioLighting>();controller.originalPipeline=current;controller.studioPipeline=studio;
            QualitySettings.renderPipeline=studio;
        }
        static void StudioReflection()
        {
            const int n=128;var cube=new Cubemap(n,TextureFormat.RGBAHalf,true);cube.name="HaoLing_Studio_Softboxes";
            for(int face=0;face<6;face++)
            {
                var pix=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++)
                {
                    float u=(x+.5f)*2/n-1,v=(y+.5f)*2/n-1;Vector3 d;
                    switch(face){case 0:d=new Vector3(1,-v,-u);break;case 1:d=new Vector3(-1,-v,u);break;case 2:d=new Vector3(u,1,v);break;case 3:d=new Vector3(u,-1,-v);break;case 4:d=new Vector3(u,-v,1);break;default:d=new Vector3(-u,-v,-1);break;}
                    d.Normalize();float softA=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(-.55f,.7f,-.45f).normalized)),30)*2.8f;
                    float softB=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(.70f,.35f,.5f).normalized)),18)*1.8f;
                    float baseValue=.08f+Mathf.Max(0,d.y)*.22f;pix[y*n+x]=new Color(baseValue+softA+softB*.8f,baseValue*1.06f+softA*.97f+softB*.9f,baseValue*1.2f+softA*.92f+softB,1);
                }
                cube.SetPixels(pix,(CubemapFace)face);
            }
            cube.Apply(true,false);AssetDatabase.CreateAsset(cube,AssetDatabase.GenerateUniqueAssetPath(Root+"/Settings/Studio_Reflection.asset"));RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=cube;RenderSettings.reflectionIntensity=1;
        }
        static void Light(string name,Vector3 pos,Vector3 angles,Color color,float intensity,LightType type)
        {
            var l=HLM.Group(name,null,pos,angles).gameObject.AddComponent<Light>();l.type=type;l.color=color;l.intensity=intensity;l.shadows=name.StartsWith("Key")?LightShadows.Soft:LightShadows.None;l.shadowBias=.15f;l.shadowNormalBias=.4f;
        }
        [MenuItem("Hao Ling/Capture current camera")]
        public static void Capture()
        {
            var camera=Camera.main;if(!camera)throw new InvalidOperationException("No showcase camera.");
            var rt=new RenderTexture(1920,1200,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();Directory.CreateDirectory(Root+"/Renders");File.WriteAllBytes(Root+"/Renders/HaoLing_Beauty.png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=prev;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);AssetDatabase.Refresh();
        }
    }
}

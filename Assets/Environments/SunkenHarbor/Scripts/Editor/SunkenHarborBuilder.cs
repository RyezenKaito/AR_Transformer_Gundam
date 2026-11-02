using System;
using System.IO;
using System.Linq;
using System.Text;
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
using Random=UnityEngine.Random;

namespace SunkenHarbor.Editor
{
 public static class SunkenHarborBuilder
 {
  public const string Root="Assets/Environments/SunkenHarbor",ScenePath=Root+"/Scenes/SunkenHarbor_Battle.unity";
  static Dictionary<Material,Material> converted=new Dictionary<Material,Material>();
  static StringBuilder manifest=new StringBuilder();static HarborConfig config;static int rendererIndex;
  static Material sand,concrete,rock,metal,kelp,grass,coral,shaft,particle,bubble,ocean,optics;
  static Transform Group(string name,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
  static Bounds BoundsOf(GameObject o){var rr=o.GetComponentsInChildren<Renderer>(true);var b=new Bounds(o.transform.position,Vector3.zero);if(rr.Length>0){b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);}return b;}
  static Material Mat(string name,string shader,Color tint)
  {string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find(shader);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);if(m.HasProperty("_WaterLevel"))m.SetFloat("_WaterLevel",42);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
  static Material Surface(string name,Color tint,float film=.3f)
  {var m=Mat(name,"SunkenHarbor/Weathered Surface",tint);m.SetFloat("_Biofilm",film);m.SetFloat("_Smoothness",.17f);return m;}
  static Material Convert(Material original)
  {
   if(!original)return concrete;if(converted.TryGetValue(original,out var cached))return cached;
   string path=AssetDatabase.GetAssetPath(original),id=AssetDatabase.AssetPathToGUID(path);string name=original.name+"_"+id.Substring(0,Math.Min(8,id.Length));var m=Surface(name,Color.white,.5f);Color c=Color.white;
   if(original.HasProperty("_BaseColor"))c=original.GetColor("_BaseColor");else if(original.HasProperty("_Color"))c=original.GetColor("_Color");m.SetColor("_BaseColor",c);
   foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorMap"})if(original.HasProperty(p)&&original.GetTexture(p)){m.SetTexture("_BaseMap",original.GetTexture(p));m.SetTextureScale("_BaseMap",original.GetTextureScale(p));m.SetTextureOffset("_BaseMap",original.GetTextureOffset(p));break;}
   if(original.HasProperty("_BumpMap")&&original.GetTexture("_BumpMap")){m.SetTexture("_BumpMap",original.GetTexture("_BumpMap"));m.EnableKeyword("_NORMALMAP");}
   m.SetFloat("_Metallic",original.HasProperty("_Metallic")?Mathf.Min(.55f,original.GetFloat("_Metallic")):0);
   if(path.Contains("Flora-Form")){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cull",0);m.SetFloat("_Cutoff",.4f);m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;}
   converted[original]=m;manifest.AppendLine("- Material source: "+path+" → "+AssetDatabase.GetAssetPath(m));return m;
  }
  static GameObject Place(string path,string name,Transform parent,Vector3 pos,float height,float width,float yaw=0,float lean=0,bool collision=false)
  {
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing source asset "+path);
   var pivot=Group(name,parent);var o=(GameObject)PrefabUtility.InstantiatePrefab(asset,pivot);o.transform.localPosition=Vector3.zero;var b=BoundsOf(o);float scale=Mathf.Min(height/Mathf.Max(.001f,b.size.y),width/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)));o.transform.localScale*=scale;b=BoundsOf(o);o.transform.position+=pivot.position-new Vector3(b.center.x,b.min.y,b.center.z);pivot.localPosition=pos;pivot.localRotation=Quaternion.Euler(0,yaw,lean);
   foreach(var a in o.GetComponentsInChildren<Animator>())UnityEngine.Object.DestroyImmediate(a);
   foreach(var l in o.GetComponentsInChildren<Light>())UnityEngine.Object.DestroyImmediate(l);
   foreach(var r in o.GetComponentsInChildren<Renderer>(true)){r.sharedMaterials=r.sharedMaterials.Select(Convert).ToArray();r.gameObject.isStatic=true;r.gameObject.layer=2;}
   if(collision){if(path.Contains("Industrial Tanks")){var bounds=BoundsOf(o);var box=pivot.gameObject.AddComponent<BoxCollider>();box.center=pivot.InverseTransformPoint(bounds.center);box.size=new Vector3(bounds.size.x,bounds.size.y,bounds.size.z);}else foreach(var mf in o.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh&&!mf.GetComponent<Collider>())mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;}
   else foreach(var c in o.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
   manifest.AppendLine("- "+name+": "+path+" at "+pos);return pivot.gameObject;
  }
  static Mesh Persist(Mesh mesh,string name)
  {string path=Root+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);mesh.name=name;if(!m){AssetDatabase.CreateAsset(mesh,path);return mesh;}m.Clear();m.name=name;m.indexFormat=mesh.indexFormat;m.vertices=mesh.vertices;m.triangles=mesh.triangles;m.normals=mesh.normals;m.uv=mesh.uv;m.tangents=mesh.tangents;m.RecalculateBounds();EditorUtility.SetDirty(m);UnityEngine.Object.DestroyImmediate(mesh);return m;}
  static Mesh Make(List<Vector3> v,List<int> t,List<Vector2> uv,string name)
  {var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();m.RecalculateTangents();return Persist(m,name);}
  static GameObject MeshObject(string name,Transform parent,Mesh mesh,Material material,bool collision=false)
  {var o=Group(name,parent).gameObject;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=material;o.isStatic=true;o.layer=2;if(collision)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;}
  static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material material,bool collide=false)
  {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;o.isStatic=true;o.layer=2;if(!collide)UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;}
  static void Beam(string name,Transform p,Vector3 a,Vector3 b,float radius,Material m)
  {var o=Box(name,p,(a+b)*.5f,new Vector3(radius,(b-a).magnitude,radius),m);o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
  static Texture2D NoiseTexture(string name,bool soft=false,bool ring=false)
  {
   string path=Root+"/Textures/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(existing)return existing;int n=soft?64:256;var tex=new Texture2D(n,n,TextureFormat.RGBA32,true);var pixels=new Color[n*n];
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){float noise=Mathf.PerlinNoise(x*.13f,y*.13f)*.11f+Mathf.PerlinNoise(x*.73f,y*.73f)*.035f;float d=Vector2.Distance(new Vector2(x,y),Vector2.one*(n-1)*.5f)/(n*.5f);float alpha=soft?Mathf.Pow(Mathf.Clamp01(1-d),2):1;if(ring)alpha=Mathf.Exp(-Mathf.Pow((d-.68f)*10,2))*.8f+Mathf.Clamp01(1-d)*.08f;pixels[y*n+x]=soft?new Color(1,1,1,alpha):new Color(.67f+noise,.64f+noise,.54f+noise,1);}
   tex.SetPixels(pixels);tex.wrapMode=soft?TextureWrapMode.Clamp:TextureWrapMode.Repeat;tex.Apply();AssetDatabase.CreateAsset(tex,path);return tex;
  }
  static void Materials()
  {
   sand=Surface("Seabed sand",new Color(.82f,.83f,.73f),.04f);sand.SetTexture("_BaseMap",NoiseTexture("SeabedGrain"));
   concrete=Surface("Aged harbour concrete",new Color(.59f,.61f,.56f),.85f);concrete.SetTexture("_BaseMap",NoiseTexture("HarbourConcrete"));concrete.SetTextureScale("_BaseMap",new Vector2(3,3));metal=Surface("Marine corroded metal",new Color(.27f,.30f,.28f),.55f);
   rock=Surface("Immersed rock",new Color(.51f,.55f,.48f),.4f);kelp=Surface("Olive kelp",new Color(.21f,.33f,.12f),.1f);kelp.SetFloat("_Cull",0);kelp.SetFloat("_Sway",.65f);grass=Surface("Short seagrass",new Color(.25f,.37f,.19f),.05f);grass.SetFloat("_Cull",0);grass.SetFloat("_Sway",.17f);coral=Surface("Muted sea fans",new Color(.38f,.24f,.30f),.3f);coral.SetFloat("_Cull",0);coral.SetFloat("_Sway",.12f);
   ocean=Mat("Open sea surface","SunkenHarbor/Ocean Surface",Color.white);ocean.SetColor("_DeepColor",new Color(.02f,.14f,.17f));
   optics=Mat("Camera water optics","SunkenHarbor/Underwater Optics",Color.white);optics.SetVector("_Absorption",config.absorption);optics.SetColor("_Scatter",config.scatter);optics.SetFloat("_Visibility",config.visibility);
   shaft=Mat("Soft shafts","SunkenHarbor/Soft Light Shaft",new Color(.24f,.62f,.65f,.035f));
   particle=Mat("Water suspended particles","Universal Render Pipeline/Particles/Unlit",new Color(.44f,.65f,.64f,.20f));bubble=Mat("Air bubbles","Universal Render Pipeline/Particles/Unlit",new Color(.65f,.87f,.9f,.45f));
   foreach(var m in new[]{particle,bubble}){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3010;m.SetTexture("_BaseMap",NoiseTexture(m==bubble?"BubbleRing":"SoftParticle",true,m==bubble));}
  }
  static int ConfigureRenderer()
  {
   var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;string rendererPath=Root+"/Lighting/HarborRenderer.asset";var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);var so=new SerializedObject(pipeline);var list=so.FindProperty("m_RendererDataList");
   if(!data){string source=AssetDatabase.GetAssetPath(list.GetArrayElementAtIndex(0).objectReferenceValue);AssetDatabase.CopyAsset(source,rendererPath);data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);}
   var feature=data.rendererFeatures.OfType<FullScreenPassRendererFeature>().FirstOrDefault(f=>f.name=="Harbor underwater optics");if(!feature){feature=ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();feature.name="Harbor underwater optics";AssetDatabase.AddObjectToAsset(feature,data);data.rendererFeatures.Add(feature);}
   feature.injectionPoint=FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;feature.fetchColorBuffer=true;feature.requirements=ScriptableRenderPassInput.Depth;feature.passMaterial=optics;feature.passIndex=0;feature.Create();feature.SetActive(true);EditorUtility.SetDirty(feature);data.SetDirty();EditorUtility.SetDirty(data);
   int index=-1;for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).objectReferenceValue==data)index=i;
   if(index<0){Directory.CreateDirectory("Backups/SunkenHarbor");string backup="Backups/SunkenHarbor/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_Pipeline.asset";File.Copy(AssetDatabase.GetAssetPath(pipeline),backup,true);index=list.arraySize;list.InsertArrayElementAtIndex(index);list.GetArrayElementAtIndex(index).objectReferenceValue=data;so.ApplyModifiedProperties();EditorUtility.SetDirty(pipeline);}
   AssetDatabase.SaveAssets();manifest.AppendLine("- Renderer: "+rendererPath+" at pipeline index "+index+". Default renderer left at its existing index; no Render Graph toggle.");return index;
  }
  static void CameraSettings(Camera camera)
  {var data=camera.GetUniversalAdditionalCameraData();data.SetRenderer(rendererIndex);data.requiresColorTexture=true;data.requiresDepthTexture=true;data.renderPostProcessing=true;camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=1500;camera.nearClipPlane=.1f;camera.useOcclusionCulling=false;}
  [Serializable] class MeshEntry{public string name,file;public int material;}
  [Serializable] class MaterialEntry{public string name,baseMap;public float[] color;public float metallic,roughness;public bool doubleSided;}
  [Serializable] class GLBManifest{public MeshEntry[] meshes;public MaterialEntry[] materials;public float[] min,max;}
  static void ImportCrane()
  {
   string dir=Root+"/ImportData/Crane";var d=JsonUtility.FromJson<GLBManifest>(File.ReadAllText(dir+"/manifest.json"));var mats=new List<Material>();for(int i=0;i<d.materials.Length;i++){var e=d.materials[i];var m=Surface("Crane_"+i,new Color(e.color[0],e.color[1],e.color[2],e.color[3]),.48f);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/"+e.baseMap));m.SetFloat("_Smoothness",Mathf.Min(.35f,1-e.roughness));m.SetFloat("_Metallic",e.metallic*.45f);m.SetFloat("_Cull",e.doubleSided?0:2);mats.Add(m);}
   var root=Group("Harbor crane — source GLB");Vector3 center=new Vector3((d.min[0]+d.max[0])*.5f,d.min[1],(d.min[2]+d.max[2])*.5f);float scale=1/(d.max[1]-d.min[1]);
   for(int k=0;k<d.meshes.Length;k++){var e=d.meshes[k];using(var reader=new BinaryReader(File.OpenRead(dir+"/"+e.file))){int n=reader.ReadInt32(),ic=reader.ReadInt32();var v=new Vector3[n];var norms=new Vector3[n];var uv=new Vector2[n];var t=new int[ic];for(int i=0;i<n;i++)v[i]=(new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle())-center)*scale;for(int i=0;i<n;i++)norms[i]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());for(int i=0;i<n;i++)uv[i]=new Vector2(reader.ReadSingle(),reader.ReadSingle());for(int i=0;i<ic;i++)t[i]=reader.ReadInt32();var m=new Mesh{indexFormat=IndexFormat.UInt32,vertices=v,normals=norms,uv=uv,triangles=t};m.RecalculateBounds();m.RecalculateTangents();MeshObject(e.name,root,Persist(m,"Crane_"+k),mats[e.material]);}}
   PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/HarborCrane_Imported.prefab");UnityEngine.Object.DestroyImmediate(root.gameObject);manifest.AppendLine("- Crane GLB: 9 parts / 255362 triangles; one source imported; duplicate download not used. Original pages/creator attribution not inferred from local filename.");
  }
  static Mesh Grid(string name,int n,float size,Func<float,float,float> height)
  {var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){float xx=(x/(float)n-.5f)*size,zz=(z/(float)n-.5f)*size;v.Add(new Vector3(xx,height(xx,zz),zz));uv.Add(new Vector2(xx*.22f,zz*.22f));}for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;t.AddRange(new[]{a,a+n+1,a+1,a+1,a+n+1,a+n+2});}return Make(v,t,uv,name);}
  static void Seabed(Transform p)
  {
   var floor=MeshObject("S01 Continuous seabed",p,Grid("HarborSeabed",144,700,(x,z)=>{float edge=Mathf.Clamp01((Mathf.Max(Mathf.Abs(x)/58,Mathf.Abs(z)/48)-1)*.5f);return -.015f+edge*(Mathf.PerlinNoise(x*.027f+33,z*.027f+19)*5-3);}),sand,true);floor.layer=0;
   string slab="Assets/Shipping Container Environment/Prefabs/Structures/concrete_slab.prefab";for(int i=0;i<8;i++)Place(slab,"S01 Broken buried quay slab "+i,p,new Vector3(-22+i*6,-.2f,8+(i%2)*3),.20f,5,i*13,0,true);
   for(int i=0;i<5;i++){var g=Box("S01 Submerged foundation strip",p,new Vector3(-22+i*10,-.22f,-11),new Vector3(7,.4f,2.4f),concrete,true);g.layer=0;g.transform.Rotate(0,i*5,0);}
   var rocks=Group("S03_S06 Photogrammetry rock boundaries",p);string path="Assets/TheTalesFactory/Photoscanned MoutainsRocks PBR/Prefabs/";string[] names={"MountainRocks01","MountainRocks02","MountainsRocks03","MountainRocks01_B","MountainRocks02_A"};
   for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;Vector3 pos=new Vector3(Mathf.Cos(a)*(63+i%3*6),-1,Mathf.Sin(a)*(58+i%4*4));Place(path+names[i%names.Length]+".prefab","R"+(i+1).ToString("00")+" Reef rock",rocks,pos,9+i%4*4,22+i%3*7,i*47,0,true);}
  }
  static string Containers(string s)=>"Assets/Shipping Container Environment/Prefabs/"+s+".prefab";
  static string Factory(string s)=>"Assets/TirgamesAssets/Factory/Prefabs/"+s+".prefab";
  static string Modular(string s)=>"Assets/Modular_SciFi_Pack (146 Objects, LowPoly)/Prefabs/Metal/"+s+".prefab";
  static void Structures(Transform p)
  {
   var office=Group("U01 Tilted harbour control block",p);office.localPosition=new Vector3(-8,-.3f,33);office.localRotation=Quaternion.Euler(0,8,-7);
   for(int level=0;level<3;level++){Box("Weathered floor plate",office,new Vector3(0,level*3.6f,0),new Vector3(14,.3f,10),concrete,true);for(int side=-1;side<=1;side+=2){for(int j=0;j<4;j++){float x=-6+j*4;Beam("Window pier",office,new Vector3(x,level*3.6f,side*5),new Vector3(x,level*3.6f+3.4f,side*5),.45f,concrete);Box("Broken window parapet",office,new Vector3(x+1,level*3.6f+.7f,side*5),new Vector3(2,.8f,.35f),concrete);}}}
   Box("Roof deck",office,new Vector3(0,10.8f,0),new Vector3(14,.3f,10),concrete,true);Beam("Bent roof antenna",office,new Vector3(2,11,1),new Vector3(2.8f,15.5f,1),.13f,metal);
   Place(Containers("Structures/ladder_1"),"U01 Ladder",office,new Vector3(-6,0,-5.3f),9,2); // corrected below if package stores it under Props
  }
  static void HarbourBuildings(Transform p)
  {
   // U01 has open window bays built from structural members, not opaque window textures.
   var office=Group("U01 Harbour control ruins",p);office.localPosition=new Vector3(-8,-.2f,33);office.localRotation=Quaternion.Euler(0,7,-7);
   for(int l=0;l<3;l++){float y=l*3.5f;Box("Concrete floor",office,new Vector3(0,y,0),new Vector3(14,.25f,10),concrete,true);for(int side=-1;side<=1;side+=2)for(int j=0;j<5;j++){float x=-6+j*3;Box("Window column",office,new Vector3(x,y+1.7f,side*5),new Vector3(.4f,3.4f,.4f),concrete,true);if(j<4)Box("Low parapet",office,new Vector3(x+1.5f,y+.6f,side*5),new Vector3(2.6f,1,.35f),concrete,true);}}
   Box("Damaged roof",office,new Vector3(0,10.5f,0),new Vector3(14,.35f,10),concrete,true);Beam("Antenna",office,new Vector3(2,10.6f,1),new Vector3(2.8f,15,1),.12f,metal);Place(Containers("Props/ladder_1"),"Roof access ladder",office,new Vector3(-7.3f,0,1),9,1.5f);
   Place(Containers("Structures/warehouse_1"),"U02 Submerged warehouse",p,new Vector3(-32,-.5f,29),11,20,90,-8,true);
   var lab=Group("U03 Abandoned pressure station",p);lab.localPosition=new Vector3(16,0,31);Box("Foundation",lab,new Vector3(0,.15f,0),new Vector3(12,.3f,9),concrete,true);
   for(int i=0;i<3;i++){Place(Modular("Wall1/Wall2_3"),"Pressure station wall",lab,new Vector3(-4+i*4,0,4),5,4,180,0,true);Place(Modular("Wall1/Wall2_3"),"Side wall",lab,new Vector3(-6,0,-3+i*3),5,3,90,0,true);}
   Place(Modular("Door/Door4"),"Industrial pressure door",lab,new Vector3(3,0,-4),4.6f,4,0,0,true);Box("Corroded roof",lab,new Vector3(0,5.2f,0),new Vector3(12,.35f,9),metal,true);
   var cyan=Surface("Weak surviving indicator",new Color(.1f,.35f,.34f),.1f);cyan.SetColor("_EmissionColor",new Color(.05f,.45f,.4f));Box("Remaining indicator",lab,new Vector3(2,3,-4.25f),new Vector3(.5f,.12f,.1f),cyan);
   var ruin=Group("U04 Collapsed southern block",p);ruin.localPosition=new Vector3(3,-1.3f,-35);ruin.localRotation=Quaternion.Euler(17,-12,22);
   for(int i=0;i<3;i++){Box("Tilted floor slab",ruin,new Vector3(0,i*3.3f,0),new Vector3(18,.3f,11),concrete,true);for(int j=0;j<4;j++)Box("Exposed broken support",ruin,new Vector3(-7+j*4.7f,i*3.3f+1.5f,4.8f),new Vector3(.6f,3,.6f),concrete,true);}Box("Surviving wall",ruin,new Vector3(-8,4,0),new Vector3(.5f,8,10),concrete,true);
   for(int i=0;i<12;i++){int side=i<7?-1:1;Vector3 pos=side<0?new Vector3(-42+(i%3)*5,0,-1+(i/3)*8):new Vector3(38+(i%2)*7,0,-13+(i-7)*7);int variant=i%4+1;Place(Containers("Containers/container_"+variant+(i==3||i==9?"_open":"")),(side<0?"U05":"U06")+" Container "+i,p,pos,3.7f,12,(i%3)*28, i%4==0?12:0,true);}
   var pier=Group("U11 Broken northern quay",p);for(int i=0;i<5;i++){float x=-44+i*18;Box("Old pier segment",pier,new Vector3(x,5,47),new Vector3(14,.7f,7),concrete,true);for(int j=0;j<2;j++)Box("Pier pillar",pier,new Vector3(x+(j==0?-5:5),1.7f,47),new Vector3(1,6,1),concrete,true);Place(Containers("Structures/rail_1"),"Corroded railing",pier,new Vector3(x,5.4f,44),1.3f,10,90);}
  }
  static void Industry(Transform p)
  {
   Place("Assets/Industrial Tanks/Round Tanks/Round_tanks.fbx","U08 Storage tanks",p,new Vector3(48,0,35),12,24,20,4,true);
   Place("Assets/Industrial Tanks/Long Tanks/Long_Tanks.fbx","U09 Fallen horizontal tank",p,new Vector3(35,-.3f,-37),5.5f,16,-25,15,true);
   Place(Factory("FanBig01Motor01"),"U07 Pump motor",p,new Vector3(34,0,25),3.1f,5,35,0,true);Place(Factory("PowerBox01_1"),"U07 Corroded controls",p,new Vector3(30,0,28),2.5f,3,25);
   for(int i=0;i<5;i++){Vector3 a=new Vector3(33+i*2,.4f,27),b=new Vector3(47+i*.3f,.4f,33);Beam("U07 Seabed transfer pipe",p,a,b,.22f,metal);}
   Place(Root+"/Prefabs/HarborCrane_Imported.prefab","U10 Leaning harbour crane",p,new Vector3(-42,-.4f,47),28,36,-12,-5,true);
   for(int i=0;i<14;i++){Vector3 pos=new Vector3(-44+Random.Range(-5,6),0,-30+Random.Range(-5,5));Place(Factory(i%3==0?"Barrel01a":"Debris01_"+(i%6+1)),"U12 Seabed machinery debris "+i,p,pos,i%3==0?1.15f:.45f,4,i*39);}
  }
  static void Islands(Transform parent)
  {
   Vector3[] pos={new Vector3(0,0,154),new Vector3(195,0,25),new Vector3(-205,0,-20),new Vector3(350,0,270),new Vector3(-360,0,260)};float[] widths={260,220,200,240,260};
   for(int island=0;island<pos.Length;island++)
   {
    int source=island%5+1;var d=AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/Free Island Collection/Environment/Terrain/Terrains/Terrain "+source+".asset");const int n=96;var heights=new float[(n+1)*(n+1)];float lo=float.MaxValue,hi=0;for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){float h=d.GetInterpolatedHeight(x/(float)n,z/(float)n);heights[z*(n+1)+x]=h;lo=Mathf.Min(lo,h);hi=Mathf.Max(hi,h);}
    var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();float width=widths[island];for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){float xx=x/(float)n-.5f,zz=z/(float)n-.5f;float radial=Mathf.Clamp01(1-(xx*xx+zz*zz)*2.7f);float norm=(heights[z*(n+1)+x]-lo)/Mathf.Max(1,hi-lo);float y=-18+Mathf.Pow(norm,.8f)*Mathf.Pow(radial,.3f)*(island<3?100:120);v.Add(new Vector3(xx*width,y,zz*width));uv.Add(new Vector2(xx*12,zz*12));}
    for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;t.AddRange(new[]{a,a+n+1,a+1,a+1,a+n+1,a+n+2});}
    var mat=Surface("Island rock and vegetation "+island,new Color(.83f,.84f,.76f),.5f);var layer=d.terrainLayers.FirstOrDefault(l=>l&&l.diffuseTexture&&l.diffuseTexture.name.IndexOf("rock",StringComparison.OrdinalIgnoreCase)>=0)??d.terrainLayers.FirstOrDefault(l=>l&&l.diffuseTexture);if(layer)mat.SetTexture("_BaseMap",layer.diffuseTexture);mat.SetFloat("_Island",1);
    var g=MeshObject("A0"+(island+1)+" Island from terrain "+source,parent,Make(v,t,uv,"Island_"+island),mat,true);g.transform.localPosition=pos[island];
    for(int j=0;j<(island<3?45:12);j++){int ix=Random.Range(12,n-12),iz=Random.Range(12,n-12);Vector3 point=v[iz*(n+1)+ix];if(point.y<48)continue;Place("Assets/Flora-Form/Vegetation/Juniper_Bush_01.prefab","Island coastal shrub",g.transform,point,Random.Range(2,4.5f),5,j*53);}
    manifest.AppendLine("- Island "+island+": sampled independent mesh from Terrain "+source+"; original TerrainData unchanged.");
   }
  }
  static Mesh Ribbon(string name,int blades,float height)
  {var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();for(int b=0;b<blades;b++){float angle=b*2.4f,h=height*Random.Range(.65f,1.15f);Vector3 offset=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*.3f;for(int j=0;j<=12;j++){float f=j/12f;Vector3 c=offset+new Vector3(Mathf.Sin(f*4+b)*h*.07f,f*h,Mathf.Cos(f*3+b)*h*.05f);float width=Mathf.Sin(Mathf.PI*f)*height*.035f+.012f;Vector3 side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*width;v.Add(c-side);v.Add(c+side);uv.Add(new Vector2(0,f));uv.Add(new Vector2(1,f));}int start=b*26;for(int j=0;j<12;j++){int a=start+j*2;t.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}}return Make(v,t,uv,name);}
  static void SeaLife(Transform p)
  {
   var tall=Ribbon("Tall kelp ribbon mesh",7,4.5f);var mid=Ribbon("Medium kelp ribbon mesh",5,1.7f);var low=Ribbon("Seagrass patch",8,.55f);
   for(int i=0;i<28;i++){float a=i*Mathf.PI*2/28;Vector3 pos=new Vector3(Mathf.Cos(a)*(31+i%3*5),0,Mathf.Sin(a)*(25+i%4*3));var o=MeshObject("V"+(i+1).ToString("00")+" Swaying marine plant",p,i%5==0?tall:mid,i%5==0?kelp:grass);o.transform.localPosition=pos;o.transform.Rotate(0,i*71,0);o.transform.localScale=Vector3.one*Random.Range(.7f,1.2f);o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
   for(int i=0;i<260;i++){float x=Random.Range(-58,58),z=Random.Range(-46,46);if(Mathf.Abs(x)<25&&Mathf.Abs(z)<12)continue;var o=MeshObject("V low seagrass "+i,p,low,grass);o.transform.localPosition=new Vector3(x,0,z);o.transform.Rotate(0,i*49,0);o.transform.localScale=Vector3.one*Random.Range(.6f,1.3f);o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
   var fishMesh=FishMesh();var fishMat=Surface("Distant silver fish",new Color(.38f,.55f,.54f),0);for(int school=0;school<3;school++){var g=Group("Fish school "+school,p);var life=g.gameObject.AddComponent<HarborLife>();life.center=new Vector3(school==0?-30:school==1?25:2,9+school*4,school==1?-32:36);life.radius=10+school*2;var fs=new List<Transform>();for(int i=0;i<10;i++){var f=MeshObject("Small background fish",g,fishMesh,fishMat);f.isStatic=false;f.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;f.transform.localPosition=life.center+new Vector3(i*.4f,0,0);f.transform.localScale=Vector3.one*Random.Range(.55f,1);fs.Add(f.transform);}life.fish=fs.ToArray();}
  }
  static Mesh FishMesh()
  {var v=new List<Vector3>{new Vector3(0,0,.55f),new Vector3(.16f,0,0),new Vector3(0,.2f,0),new Vector3(-.16f,0,0),new Vector3(0,-.17f,0),new Vector3(0,0,-.4f),new Vector3(-.2f,.18f,-.65f),new Vector3(.2f,-.18f,-.65f)};var t=new List<int>{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1,5,6,7,5,7,6};return Make(v,t,v.Select(_=>Vector2.zero).ToList(),"SmallFish");}
  static void WaterAndEffects(Transform p,HarborAtmosphere atmosphere)
  {
   var water=MeshObject("Ocean surface — top and underside",p,Grid("OceanGrid",180,1800,(x,z)=>0),ocean);water.transform.localPosition=Vector3.up*42;water.isStatic=false;water.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;water.GetComponent<Renderer>().receiveShadows=false;atmosphere.ocean=water.transform;
   var particles=new List<ParticleSystem>();for(int i=0;i<6;i++){var ps=Group(i<3?"Suspended marine particles "+i:"B0"+(i-2)+" Bubble leak",p).gameObject.AddComponent<ParticleSystem>();bool bubbles=i>=3;ps.transform.localPosition=bubbles?new Vector3(i==3?34:i==4?-42:35,2,i==3?25:i==4?47:-37):new Vector3(-30+i*30,12,5);var main=ps.main;main.startLifetime=bubbles?18:35;main.startSpeed=bubbles?2.1f:.025f;main.startSize=bubbles?new ParticleSystem.MinMaxCurve(.07f,.18f):new ParticleSystem.MinMaxCurve(.012f,.04f);main.startColor=bubbles?new Color(.65f,.86f,.9f,.4f):new Color(.55f,.67f,.62f,.22f);main.maxParticles=bubbles?65:180;main.simulationSpace=ParticleSystemSimulationSpace.World;var em=ps.emission;em.rateOverTime=bubbles?1.8f:4;var shape=ps.shape;shape.shapeType=bubbles?ParticleSystemShapeType.Cone:ParticleSystemShapeType.Box;shape.angle=5;shape.radius=.2f;if(!bubbles)shape.scale=new Vector3(50,23,95);if(bubbles)ps.transform.localRotation=Quaternion.Euler(-90,0,0);var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.08f;velocity.y=bubbles?0:.015f;var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=bubbles?bubble:particle;renderer.shadowCastingMode=ShadowCastingMode.Off;particles.Add(ps);}
   atmosphere.particles=particles.ToArray();
   for(int i=0;i<4;i++){var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Soft light shaft approximation "+i;o.transform.SetParent(p,false);o.transform.localPosition=new Vector3(-27+i*19,25,15+i%2*10);o.transform.localScale=new Vector3(3+i*.7f,16,3+i*.7f);o.transform.localRotation=Quaternion.Euler(0,0,-14);o.GetComponent<Renderer>().sharedMaterial=shaft;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
  }
  static void Lighting(Transform p)
  {
   var sun=Group("Marine sunlight",p).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.98f,.91f);sun.intensity=2.5f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(47,-35,0);RenderSettings.sun=sun;RenderSettings.fog=false;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.50f,.68f,.72f);RenderSettings.ambientEquatorColor=new Color(.32f,.46f,.46f);RenderSettings.ambientGroundColor=new Color(.23f,.31f,.28f);
   var sky=Mat("Marine sky","SunkenHarbor/Marine Sky",Color.white);RenderSettings.skybox=sky;
   string path=Root+"/Lighting/HarborVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.12f);bloom.threshold.Override(1.2f);AssetDatabase.AddObjectToAsset(bloom,profile);}
   var volume=Group("Marine global volume",p).gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
  }
  static void Reviews(Transform p)
  {
   string[] names={"SunkenHarbor_Gameplay","SunkenHarbor_OverviewUnderwater","SunkenHarbor_SurfaceLookUp","SunkenHarbor_Crane","SunkenHarbor_Ruins","SunkenHarbor_SeabedDetail","SunkenHarbor_SurfaceIslands"};Vector3[] pos={new Vector3(-9,3.8f,-11),new Vector3(-48,21,-52),new Vector3(1,3,0),new Vector3(-15,8,13),new Vector3(-7,5,-15),new Vector3(-15,1.7f,9),new Vector3(-240,130,-270)};Vector3[] targets={new Vector3(5,2,5),new Vector3(2,5,15),new Vector3(0,43,10),new Vector3(-42,16,47),new Vector3(3,4,-35),new Vector3(-10,.5f,17),new Vector3(0,40,105)};
   for(int i=0;i<names.Length;i++){var c=Group(names[i],p).gameObject.AddComponent<Camera>();c.transform.localPosition=pos[i];c.transform.LookAt(p.TransformPoint(targets[i]));c.fieldOfView=i==1||i==6?66:60;c.enabled=false;CameraSettings(c);}
  }
  [MenuItem("Tools/Sunken Harbor/Build Battlefield")]
  public static void Build()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first");var active=SceneManager.GetActiveScene();if(active.isDirty&&!string.IsNullOrEmpty(active.path)){Directory.CreateDirectory("Backups/SunkenHarbor");EditorSceneManager.SaveScene(active,"Backups/SunkenHarbor/PreviousScene_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);}
   foreach(string dir in new[]{"Scenes","Meshes","Materials","Textures","Lighting","Settings","Prefabs","Audio","Documentation/Review"})Directory.CreateDirectory(Root+"/"+dir);AssetDatabase.Refresh();
   if(File.Exists(ScenePath)){string dest="Backups/SunkenHarbor/"+DateTime.Now.ToString("yyyyMMdd_HHmmss");foreach(var file in Directory.GetFiles(Root,"*",SearchOption.AllDirectories)){string target=dest+file.Substring(Root.Length);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target,true);}}
   string cp=Root+"/Settings/HarborConfig.asset";config=AssetDatabase.LoadAssetAtPath<HarborConfig>(cp);if(!config){config=ScriptableObject.CreateInstance<HarborConfig>();AssetDatabase.CreateAsset(config,cp);}var randomState=Random.state;
   try
   {
    Random.InitState(config.seed);converted.Clear();manifest.Clear();manifest.AppendLine("# Sunken Harbor asset manifest\n\nSource package assets left unchanged.\n");Materials();rendererIndex=ConfigureRenderer();ImportCrane();
    File.Copy("Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity",ScenePath,true);AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);var scene=EditorSceneManager.OpenScene(ScenePath);var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;board.position=new Vector3(5.4f,0,0);
    foreach(string name in new[]{"MegaKitHangar","SphereCityArena"}){var o=board.Find(name);if(o)o.gameObject.SetActive(false);}var stage=board.Find("BattleArena");if(stage){foreach(var r in stage.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in stage.GetComponentsInChildren<Collider>(true))c.enabled=false;}foreach(string name in new[]{"Key Light","Fill Light","Battle Post Processing"}){var o=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==name);if(o)o.SetActive(false);}
    var root=Group("SunkenHarbor_Battlefield");var origin=Group("SunkenHarbor_LayoutOrigin",root);origin.localPosition=new Vector3(4.85f,0,0);var atmosphere=root.gameObject.AddComponent<HarborAtmosphere>();atmosphere.config=config;atmosphere.optics=optics;
    Seabed(Group("01_SeabedAndRockCliffs",origin));HarbourBuildings(Group("02_SunkenStructures_U01_U06_U11",origin));Industry(Group("03_IndustrialTanksPumpsAndCrane",origin));Islands(Group("11_SurfaceIslands",origin));SeaLife(Group("08_MarineVegetationAndFish",origin));WaterAndEffects(Group("10_OceanAndWaterVFX",origin),atmosphere);Lighting(Group("14_LightingAndAudio",origin));Reviews(Group("15_ReviewCameras",origin));MarineDetails(root);
    var switcher=battle.GetComponent<BattleEnvironmentSwitcher>();switcher.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="SUNKEN HARBOR · THALASSA",root=root.gameObject,ambient=new Color(.30f,.48f,.52f),background=new Color(.035f,.22f,.27f)}};switcher.trainingRenderers=Array.Empty<MeshRenderer>();switcher.Apply(0);if(switcher.nextButton)switcher.nextButton.gameObject.SetActive(false);
    var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.homeYaw=85;orbit.homePitch=8;orbit.homeDistance=9;orbit.maxDistance=100;orbit.minPitch=-85;orbit.maxPitch=89;orbit.ResetView(true);CameraSettings(battle.battleCamera);battle.initialEnemies=1;
    atmosphere.materials=root.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m&&AssetDatabase.GetAssetPath(m).StartsWith(Root)).Distinct().ToArray();atmosphere.Apply();
    PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/SunkenHarbor_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Register();File.WriteAllText(Root+"/Documentation/AssetManifest.md",manifest.ToString());Debug.Log("SUNKEN_HARBOR_READY: scene="+ScenePath+" water="+config.waterLevel+" renderer="+rendererIndex);
   }finally{Random.state=randomState;}
  }
  static void MarineDetails(Transform root)
  {
   var origin=root.Find("SunkenHarbor_LayoutOrigin");var previous=origin.Find("16_MarineDetails");if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);var p=Group("16_MarineDetails",origin);
   var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();Action<Vector3,Vector3,float> twig=(a,b,width)=>{int k=verts.Count;Vector3 side=Vector3.Cross((b-a).normalized,Vector3.forward)*width;verts.Add(a-side);verts.Add(a+side);verts.Add(b-side*.3f);verts.Add(b+side*.3f);uv.Add(new Vector2(0,a.y/2));uv.Add(new Vector2(1,a.y/2));uv.Add(new Vector2(0,b.y/2));uv.Add(new Vector2(1,b.y/2));tris.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});};
   twig(Vector3.zero,new Vector3(0,1.8f,0),.035f);for(int i=0;i<8;i++){float y=.25f+i*.18f;for(int s=-1;s<=1;s+=2){Vector3 a=new Vector3(0,y,0),b=new Vector3(s*(.65f-i*.04f),y+.3f,.04f);twig(a,b,.025f);for(int j=1;j<4;j++){Vector3 q=Vector3.Lerp(a,b,j*.25f);twig(q,q+new Vector3(s*.12f,.23f,0),.012f);}}}
   var fan=Make(verts,tris,uv,"ProceduralSeaFan");Vector2[] points={new Vector2(-24,17),new Vector2(27,18),new Vector2(-30,-23),new Vector2(24,-22),new Vector2(-46,15),new Vector2(45,10),new Vector2(-20,36),new Vector2(26,40)};
   for(int i=0;i<points.Length;i++){var g=Group("V Sea fan cluster "+i,p);g.localPosition=new Vector3(points[i].x,0,points[i].y);Place("Assets/TheTalesFactory/Photoscanned MoutainsRocks PBR/Prefabs/MountainRocks01_B.prefab","Small marine rock",g,Vector3.zero,.65f,2.8f,i*37);for(int j=0;j<3;j++){var o=MeshObject("Sea fan",g,fan,coral);o.transform.localPosition=new Vector3(j*.45f-.45f,.2f,j*.15f);o.transform.localScale=Vector3.one*Random.Range(.5f,.9f);o.transform.Rotate(0,i*43+j*25,0);o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}}
   var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var ps=Group("Landing sediment response",p).gameObject.AddComponent<ParticleSystem>();var main=ps.main;main.startLifetime=4;main.startSpeed=.35f;main.startSize=new ParticleSystem.MinMaxCurve(.2f,.65f);main.startColor=new Color(.45f,.58f,.48f,.15f);main.maxParticles=65;main.simulationSpace=ParticleSystemSimulationSpace.World;var em=ps.emission;em.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=.4f;var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particle;var response=p.gameObject.AddComponent<HarborSediment>();response.battle=battle;response.sediment=ps;
   var audio=Group("Submarine ambience",p).gameObject.AddComponent<AudioSource>();audio.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/UnderwaterAmbience.wav");audio.loop=true;audio.playOnAwake=true;audio.spatialBlend=0;audio.volume=.12f;var at=root.GetComponent<HarborAtmosphere>();at.ambience=audio;
  }
  [MenuItem("Tools/Sunken Harbor/Polish Lighting Water And Marine Life")]
  public static void PolishCurrent()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first");config=AssetDatabase.LoadAssetAtPath<HarborConfig>(Root+"/Settings/HarborConfig.asset");config.absorption=new Vector3(.028f,.015f,.010f);config.scatter=new Color(.045f,.20f,.23f);EditorUtility.SetDirty(config);Materials();rendererIndex=ConfigureRenderer();var root=GameObject.Find("SunkenHarbor_Battlefield").transform;var origin=root.Find("SunkenHarbor_LayoutOrigin");
   var old=origin.Find("11_SurfaceIslands");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);Islands(Group("11_SurfaceIslands",origin));var light=origin.Find("14_LightingAndAudio");UnityEngine.Object.DestroyImmediate(light.gameObject);Lighting(Group("14_LightingAndAudio",origin));MarineDetails(root);
   foreach(string name in new[]{"U08 Storage tanks","U09 Fallen horizontal tank"}){var t=origin.Find("03_IndustrialTanksPumpsAndCrane/"+name);if(!t)continue;foreach(var c in t.GetComponentsInChildren<MeshCollider>())UnityEngine.Object.DestroyImmediate(c);if(!t.GetComponent<BoxCollider>()){var b=BoundsOf(t.gameObject);var box=t.gameObject.AddComponent<BoxCollider>();box.center=t.InverseTransformPoint(b.center);box.size=b.size;}}
   var at=root.GetComponent<HarborAtmosphere>();at.materials=root.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m&&AssetDatabase.GetAssetPath(m).StartsWith(Root)).Distinct().ToArray();at.Apply();
   PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/SunkenHarbor_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Capture();Debug.Log("SUNKEN_HARBOR_POLISHED");
  }
  static void Register()
  {var cat=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var d=cat.Find("sunken-harbor");if(!d){d=ScriptableObject.CreateInstance<EnvironmentDefinition>();d.id="sunken-harbor";AssetDatabase.CreateAsset(d,Root+"/SunkenHarborDefinition.asset");}d.displayName="SUNKEN HARBOR · THALASSA";d.description="Gundam dưới đáy biển · cảng chìm, cần cẩu, rong và quần đảo.";d.battleVariant=-1;d.available=true;d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);d.SyncScene();d.accent=new Color(.1f,.65f,.69f);EditorUtility.SetDirty(d);cat.environments=new[]{d}.Concat(cat.environments.Where(x=>x&&x!=d)).ToArray();EditorUtility.SetDirty(cat);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();}
  [MenuItem("Tools/Sunken Harbor/Capture Review")]
  public static void Capture()
  {Directory.CreateDirectory(Root+"/Documentation/Review");foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("SunkenHarbor_"))){var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Root+"/Documentation/Review/"+c.name+".png",tex.EncodeToPNG());}finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}}AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("sunken-harbor");if(d){d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Documentation/Review/SunkenHarbor_Gameplay.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();}Debug.Log("SUNKEN_HARBOR_REVIEW_SAVED");}
 }
}

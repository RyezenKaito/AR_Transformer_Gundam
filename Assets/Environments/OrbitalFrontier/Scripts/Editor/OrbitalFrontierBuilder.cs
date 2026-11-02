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

namespace OrbitalFrontier.Editor
{
 public static partial class OrbitalFrontierBuilder
 {
  public const string Root="Assets/Environments/OrbitalFrontier",ScenePath=Root+"/Scenes/OrbitalFrontier_Battle.unity";
  static Material hull,dark,orange,solar,cyan,red,sky;
  static Dictionary<Material,Material> converted=new Dictionary<Material,Material>();
  static StringBuilder manifest=new StringBuilder();
  static Transform Group(string n,Transform p=null){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
  static Material Mat(string n,Color c,string shader="Universal Render Pipeline/Lit")
  {string path=Root+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find(shader);m.SetColor("_BaseColor",c);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.34f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
  static Material Glow(string n,Color c,float strength){var m=Mat(n,c);m.SetColor("_EmissionColor",c*strength);m.EnableKeyword("_EMISSION");return m;}
  static Mesh Persist(Mesh m,string n)
  {m.name=n;string p=Root+"/Meshes/"+n+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(!old){AssetDatabase.CreateAsset(m,p);return m;}old.Clear();old.indexFormat=m.indexFormat;old.vertices=m.vertices;old.normals=m.normals;old.uv=m.uv;old.triangles=m.triangles;old.tangents=m.tangents;old.RecalculateBounds();EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;}
  static GameObject MeshObject(string n,Transform p,Mesh mesh,Material mat)
  {var o=Group(n,p).gameObject;o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.receiveShadows=true;o.layer=2;return o;}
  static GameObject Primitive(PrimitiveType type,string n,Transform p,Vector3 pos,Vector3 size,Material mat)
  {var o=GameObject.CreatePrimitive(type);o.name=n;o.transform.SetParent(p,false);o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());o.layer=2;return o;}
  static GameObject Box(string n,Transform p,Vector3 pos,Vector3 size,Material m)=>Primitive(PrimitiveType.Cube,n,p,pos,size,m);
  static GameObject Beam(string n,Transform p,Vector3 a,Vector3 b,float width,Material m)
  {var o=Box(n,p,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),m);o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return o;}
  static OrbitalMotion Motion(GameObject o,Vector3 angular,float drift=0,Vector3 axis=default)
  {var m=o.AddComponent<OrbitalMotion>();m.angularVelocity=angular;m.driftAmplitude=drift;m.driftAxis=axis;m.phase=Random.Range(0,6.28f);return m;}
  static Bounds BoundsOf(GameObject o){var rr=o.GetComponentsInChildren<Renderer>(true);var b=new Bounds(o.transform.position,Vector3.zero);if(rr.Length>0){b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);}return b;}
  static Material Convert(Material source)
  {
   if(!source)return hull;if(converted.TryGetValue(source,out var m))return m;
   string path=AssetDatabase.GetAssetPath(source);string id=AssetDatabase.AssetPathToGUID(path);string n="Source_"+source.name+"_"+id.Substring(0,Math.Min(8,id.Length));Color c=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.GetColor("_Color"):Color.white;m=Mat(n,c);
   foreach(string k in new[]{"_BaseMap","_MainTex","_BaseColorMap"})if(source.HasProperty(k)&&source.GetTexture(k)){m.SetTexture("_BaseMap",source.GetTexture(k));m.SetTextureScale("_BaseMap",source.GetTextureScale(k));m.SetTextureOffset("_BaseMap",source.GetTextureOffset(k));break;}
   if(source.HasProperty("_BumpMap")&&source.GetTexture("_BumpMap")){m.SetTexture("_BumpMap",source.GetTexture("_BumpMap"));m.EnableKeyword("_NORMALMAP");}
   foreach(string k in new[]{"_MetallicGlossMap","_OcclusionMap","_EmissionMap"})if(source.HasProperty(k)&&source.GetTexture(k))m.SetTexture(k,source.GetTexture(k));
   if(m.GetTexture("_MetallicGlossMap"))m.EnableKeyword("_METALLICSPECGLOSSMAP");if(m.GetTexture("_OcclusionMap"))m.EnableKeyword("_OCCLUSIONMAP");
   m.SetFloat("_Metallic",source.HasProperty("_Metallic")?Mathf.Min(.65f,source.GetFloat("_Metallic")):.2f);m.SetFloat("_Smoothness",source.HasProperty("_Smoothness")?Mathf.Min(.55f,source.GetFloat("_Smoothness")):.3f);
   if(source.HasProperty("_EmissionColor")){var ec=source.GetColor("_EmissionColor");m.SetColor("_EmissionColor",ec);if(ec.maxColorComponent>.001f)m.EnableKeyword("_EMISSION");}
   if(source.HasProperty("_Cull"))m.SetFloat("_Cull",source.GetFloat("_Cull"));
   if((source.HasProperty("_Surface")&&source.GetFloat("_Surface")>.5f)||source.name.ToLowerInvariant().Contains("glass")){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");m.SetShaderPassEnabled("DepthOnly",false);m.SetShaderPassEnabled("DepthNormals",false);m.SetShaderPassEnabled("ShadowCaster",false);m.renderQueue=3000;}
   converted[source]=m;return m;
  }
  static GameObject Place(string path,string n,Transform p,Vector3 pos,float extent,Vector3 angles)
  {
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing source "+path);var pivot=Group(n,p);var o=(GameObject)PrefabUtility.InstantiatePrefab(asset,pivot);o.transform.localPosition=Vector3.zero;var b=BoundsOf(o);o.transform.localScale*=extent/Mathf.Max(.001f,Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z)));b=BoundsOf(o);o.transform.position+=pivot.position-b.center;
   foreach(var script in o.GetComponentsInChildren<MonoBehaviour>(true))UnityEngine.Object.DestroyImmediate(script);foreach(var a in o.GetComponentsInChildren<Animator>(true))UnityEngine.Object.DestroyImmediate(a);foreach(var c in o.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(c);foreach(var c in o.GetComponentsInChildren<Camera>(true))UnityEngine.Object.DestroyImmediate(c);foreach(var a in o.GetComponentsInChildren<AudioListener>(true))UnityEngine.Object.DestroyImmediate(a);foreach(var l in o.GetComponentsInChildren<Light>(true))UnityEngine.Object.DestroyImmediate(l);
   foreach(var r in o.GetComponentsInChildren<Renderer>(true)){r.sharedMaterials=r.sharedMaterials.Select(Convert).ToArray();r.gameObject.layer=2;}
   pivot.localPosition=pos;pivot.localRotation=Quaternion.Euler(angles);manifest.AppendLine("- "+n+": "+path+"; "+pos+" extent="+extent);return pivot.gameObject;
  }
  [Serializable] class Entry{public string name,file;public int material,group;}
  [Serializable] class GroupEntry{public string name;public int id;public float[] min,max;}
  [Serializable] class MaterialEntry{public string name,baseMap,normalMap,alphaMode;public float[] color;public float metallic,roughness,alphaCutoff;public bool doubleSided;}
  [Serializable] class Manifest{public Entry[] meshes;public GroupEntry[] groups;public MaterialEntry[] materials;}
  static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
  static List<string> ImportGLB(string folder)
  {
   string dir=Root+"/ImportData/"+folder;var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(dir+"/manifest.json"));var materials=new List<Material>();int total=0;
   for(int i=0;i<data.materials.Length;i++)
   {var e=data.materials[i];var m=Mat(folder+"_"+i,new Color(e.color[0],e.color[1],e.color[2],e.color[3]));if(!string.IsNullOrEmpty(e.baseMap))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/"+e.baseMap));m.SetFloat("_Metallic",Mathf.Min(folder=="Asteroids"?.35f:.7f,e.metallic));m.SetFloat("_Smoothness",Mathf.Min(folder=="Asteroids"?.27f:.6f,1-e.roughness));m.SetFloat("_Cull",e.doubleSided?0:2);
   if(!string.IsNullOrEmpty(e.normalMap)){string np=dir+"/"+e.normalMap;var ti=(TextureImporter)AssetImporter.GetAtPath(np);if(ti.textureType!=TextureImporterType.NormalMap){ti.textureType=TextureImporterType.NormalMap;ti.SaveAndReimport();}m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(np));m.EnableKeyword("_NORMALMAP");}
   if(e.alphaMode=="BLEND"){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}materials.Add(m);}
   var paths=new List<string>();foreach(var group in data.groups)
   {var root=Group(folder+"_"+group.id);Vector3 center=(V(group.min)+V(group.max))*.5f;Vector3 size=V(group.max)-V(group.min);float scale=1/Mathf.Max(size.x,Mathf.Max(size.y,size.z));int index=0;
    foreach(var e in data.meshes.Where(x=>x.group==group.id))using(var r=new BinaryReader(File.OpenRead(dir+"/"+e.file)))
    {int n=r.ReadInt32(),count=r.ReadInt32();var vs=new Vector3[n];var ns=new Vector3[n];var uv=new Vector2[n];var ts=new int[count];for(int j=0;j<n;j++)vs[j]=(new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle())-center)*scale;for(int j=0;j<n;j++)ns[j]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());for(int j=0;j<n;j++)uv[j]=new Vector2(r.ReadSingle(),r.ReadSingle());for(int j=0;j<count;j++)ts[j]=r.ReadInt32();var mesh=new Mesh{indexFormat=IndexFormat.UInt32,vertices=vs,normals=ns,uv=uv,triangles=ts};mesh.RecalculateBounds();mesh.RecalculateTangents();MeshObject(e.name,root,Persist(mesh,folder+"_"+group.id+"_"+index++),materials[e.material]);total+=count/3;}
    string path=Root+"/Prefabs/"+folder+"_"+group.id+".prefab";PrefabUtility.SaveAsPrefabAsset(root.gameObject,path);paths.Add(path);UnityEngine.Object.DestroyImmediate(root.gameObject);
   }manifest.AppendLine("- GLB "+folder+": "+data.meshes.Length+" primitives, "+data.groups.Length+" groups, "+total+" triangles; preserved mesh normals and tangent UVs.");return paths;
  }
  static Mesh Sphere()
  {
   const int w=192,h=96;var v=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();for(int y=0;y<=h;y++)for(int x=0;x<=w;x++){float a=x/(float)w*Mathf.PI*2,b=y/(float)h*Mathf.PI;var n=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));v.Add(n);ns.Add(n);uv.Add(new Vector2(x/(float)w,1-y/(float)h));}for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*(w+1)+x;t.AddRange(new[]{i,i+1,i+w+1,i+1,i+w+2,i+w+1});}var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetNormals(ns);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateTangents();m.RecalculateBounds();return Persist(m,"CelestialSphere");
  }
  static void Materials()
  {
   hull=Mat("Station ceramic alloy",new Color(.65f,.70f,.74f));hull.SetFloat("_Metallic",.4f);dark=Mat("Graphite trusses",new Color(.12f,.15f,.19f));dark.SetFloat("_Metallic",.6f);orange=Mat("Rescue amber panels",new Color(.72f,.24f,.055f));solar=Mat("Solar cells navy",new Color(.018f,.035f,.11f));solar.SetFloat("_Metallic",.65f);solar.SetFloat("_Smoothness",.5f);cyan=Glow("Cold navigation lamps",new Color(.08f,.6f,1),2.5f);red=Glow("Port warning lamps",new Color(.9f,.06f,.02f),1.7f);
   string path=Root+"/Materials/Deep space sky.mat";sky=AssetDatabase.LoadAssetAtPath<Material>(path);var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/SpaceSkies Free/Skybox_3/Purple_4K_Resolution.mat");if(!sky){sky=new Material(source);AssetDatabase.CreateAsset(sky,path);}else EditorUtility.CopySerialized(source,sky);sky.name="Deep space sky";sky.SetFloat("_Exposure",.85f);sky.SetFloat("_Rotation",220);EditorUtility.SetDirty(sky);
  }
  static void Planets(Transform p)
  {
   var mesh=Sphere();string path=Root+"/Textures/Earth_NASA_BlueMarble_200401.jpg";var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.maxTextureSize=4096;ti.mipmapEnabled=true;ti.wrapMode=TextureWrapMode.Repeat;ti.SaveAndReimport();var earth=Mat("Earth NASA Blue Marble",Color.white,"OrbitalFrontier/Planet");earth.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));earth.SetFloat("_Moon",0);
   var g=Group("E01 Earth system",p);g.localPosition=new Vector3(2200,-400,2100);g.localRotation=Quaternion.Euler(18,0,-13);var globe=MeshObject("E01 Earth surface NASA",g,mesh,earth);globe.transform.localScale=Vector3.one*650;globe.transform.localRotation=Quaternion.Euler(0,5,0);Motion(globe,new Vector3(0,.8f,0));
   var clouds=Mat("Earth moving cloud shell",new Color(.95f,.98f,1,.48f),"OrbitalFrontier/Planet Layers");clouds.SetFloat("_Atmosphere",0);var c=MeshObject("E01-C Differential cloud layer",g,mesh,clouds);c.transform.localScale=Vector3.one*652.5f;Motion(c,new Vector3(0,1.03f,0));
   var atmosphere=Mat("Earth thin atmosphere",new Color(.12f,.44f,1,.8f),"OrbitalFrontier/Planet Layers");atmosphere.SetFloat("_Atmosphere",1);var a=MeshObject("E01-A Blue atmospheric limb",g,mesh,atmosphere);a.transform.localScale=Vector3.one*660;a.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;c.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
   var moonMat=Mat("Moon procedural craters",new Color(.65f,.64f,.61f),"OrbitalFrontier/Planet");moonMat.SetFloat("_Moon",1);var moon=MeshObject("E02 Moon - simulated lunar surface",p,mesh,moonMat);moon.transform.localPosition=new Vector3(2300,520,-280);moon.transform.localScale=Vector3.one*125;Motion(moon,new Vector3(0,.06f,0));
   var distant=Mat("Tinniuq fictional planet",new Color(.65f,.57f,.46f),"OrbitalFrontier/Planet");distant.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FreeSciFi/Textures/PlanetAsteroids/Tinniuq_Albedo.png"));distant.SetFloat("_Moon",0);var d=MeshObject("E03 Tinniuq distant fictional world",p,mesh,distant);d.transform.localPosition=new Vector3(-2100,1150,-2700);d.transform.localScale=Vector3.one*160;Motion(d,new Vector3(0,.12f,0));
   GasGiants(p,mesh);manifest.AppendLine("- Earth: NASA Blue Marble January 2004, source URL in master prompt; procedural cloud and atmosphere shells. Moon is procedural artistic surface, not a NASA lunar map. Tinniuq texture from FreeSciFi. Celestial placement is compositional, not an astronomical scale model.");
  }
  static void SolarWing(Transform p,Vector3 pos,float roll=0)
  {
   var g=Group("Photovoltaic wing",p);g.localPosition=pos;g.localRotation=Quaternion.Euler(0,0,roll);Box("Aluminium support backplate",g,Vector3.zero,new Vector3(20,.18f,8.8f),hull);Box("Deep blue cell surface",g,new Vector3(0,.12f,0),new Vector3(19.7f,.06f,8.5f),solar);
   for(int x=0;x<=16;x++)Box("Cell bus bar",g,new Vector3(-9.8f+x*1.225f,.16f,0),new Vector3(.035f,.02f,8.5f),dark);for(int z=0;z<=8;z++)Box("Cell row separation",g,new Vector3(0,.16f,-4.25f+z*1.0625f),new Vector3(19.6f,.02f,.035f),dark);
  }
  static void Station(Transform p){SpaceWrecks(p);}
  static void Ships(Transform p,string alien)
  {
   var hero=Place(alien,"S01 Alien capital ship",p,new Vector3(132,10,0),56,new Vector3(-7,25,8));Motion(hero,new Vector3(0,.016f,0),1.5f,new Vector3(1,.2f,.3f));
   for(int i=0;i<3;i++){var g=Place("Assets/FreeSciFi/Prefabs/PolygonSpaceship/StarSparrow_1_LP_Red.prefab","S0"+(i+2)+" StarSparrow patrol",p,new Vector3(58+i*12,6+i*3,50+i*13),9,new Vector3(-9,-80,5-i*4));Motion(g,Vector3.zero,2.5f,new Vector3(.8f,.1f,-1));}
   for(int i=0;i<2;i++){var g=Place("Assets/SF_Fighter/SF_Free-Fighter.prefab","S0"+(i+5)+" SF escort",p,new Vector3(65+i*13,-14+i*4,-5-i*10),12,new Vector3(12,55,-8));Motion(g,Vector3.zero,2,new Vector3(1,.2f,1));}
   var r35=Place("Assets/R35/Dropship_R35.prefab","S07 R35 docking transport",p,new Vector3(48,-9,60),27,new Vector3(0,110,3));Motion(r35,Vector3.zero,.4f,Vector3.up);
  }
  static void Asteroids(Transform p,List<string> paths)
  {
   for(int i=0;i<88;i++)
   {float angle=i*2.399963f;float r=80+(i%7)*18;Vector3 pos=new Vector3(Mathf.Cos(angle)*r,Mathf.Sin(i*1.71f)*(35+i%5*12),Mathf.Sin(angle)*r);if(pos.x>0&&pos.z>0)pos.y-=50;float extent=i<10?Random.Range(9,17):i<48?Random.Range(2.5f,7):Random.Range(.65f,2.1f);var g=Place(paths[i%paths.Count],"A"+i.ToString("00")+" Asteroid variant "+i%paths.Count,p,pos,extent,new Vector3(i*13,i*47,i*29));Motion(g,Random.onUnitSphere*(i<10?.13f:Random.Range(.3f,1.2f)));}
  }
  static void Salvage(Transform p){AssetFragments(p);}
  static void Lighting(Transform p)
  {
   var sun=Group("Sun - single directional source",p).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.94f,.86f);sun.intensity=1.65f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(32,28,0);RenderSettings.sun=sun;RenderSettings.skybox=sky;RenderSettings.fog=false;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.3f,.36f,.47f);RenderSettings.ambientIntensity=1;RenderSettings.reflectionIntensity=.35f;
   var volume=Group("Space exposure and restrained bloom",p).gameObject.AddComponent<Volume>();volume.isGlobal=true;string path=Root+"/Settings/SpaceVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}foreach(var c in profile.components.ToArray())UnityEngine.Object.DestroyImmediate(c,true);profile.components.Clear();var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.12f);bloom.threshold.Override(1.15f);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);foreach(var c in profile.components)if(!AssetDatabase.Contains(c))AssetDatabase.AddObjectToAsset(c,profile);EditorUtility.SetDirty(profile);volume.sharedProfile=profile;
  }
  static void CameraSettings(Camera c){var d=c.GetUniversalAdditionalCameraData();d.SetRenderer(0);d.renderPostProcessing=true;d.requiresColorTexture=true;d.requiresDepthTexture=true;c.clearFlags=CameraClearFlags.Skybox;c.farClipPlane=10000;c.nearClipPlane=.05f;c.useOcclusionCulling=false;}
  static void Review(Transform p,string name,Vector3 position,Vector3 target)
  {var c=Group("Orbital_"+name,p).gameObject.AddComponent<Camera>();c.transform.position=position;c.transform.LookAt(target);c.fieldOfView=48;CameraSettings(c);c.enabled=false;}
  static void Reviews(Transform p,Camera main)
  {Review(p,"Gameplay",main.transform.position,main.transform.position+main.transform.forward*10);Review(p,"Wreckage",new Vector3(-5,30,25),new Vector3(57,5,78));Review(p,"CapitalShip",new Vector3(20,22,-40),new Vector3(136.85f,10,0));Review(p,"Overview",new Vector3(-165,100,-220),new Vector3(35,0,15));Review(p,"UnderFormation",new Vector3(-9,-18,-6),new Vector3(5,2,0));Review(p,"AsteroidField",new Vector3(45,35,20),new Vector3(-100,-25,-140));Review(p,"Saturn",new Vector3(-1000,400,900),new Vector3(-2395,650,1800));Review(p,"Jupiter",new Vector3(2200,0,600),new Vector3(2905,350,250));}
  [MenuItem("Tools/Orbital Frontier/Build Battlefield")]
  public static void Build()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before building");var active=SceneManager.GetActiveScene();if(active.isDirty){Directory.CreateDirectory("Backups/OrbitalFrontier");EditorSceneManager.SaveScene(active,"Backups/OrbitalFrontier/Previous_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);}foreach(string dir in new[]{"Scenes","Meshes","Materials","Settings","Prefabs","Documentation/Review"})Directory.CreateDirectory(Root+"/"+dir);if(File.Exists(ScenePath)){Directory.CreateDirectory("Backups/OrbitalFrontier");File.Copy(ScenePath,"Backups/OrbitalFrontier/Space_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);}AssetDatabase.Refresh();var state=Random.state;
   try
   {Random.InitState(91042);manifest.Clear();converted.Clear();Materials();var alien=ImportGLB("AlienShip");var asteroid=ImportGLB("Asteroids");File.Copy("Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity",ScenePath,true);AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);var scene=EditorSceneManager.OpenScene(ScenePath);var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;board.position=new Vector3(5.4f,0,0);
    foreach(string n in new[]{"MegaKitHangar","SphereCityArena","BattleArena"}){var o=board.Find(n);if(o)o.gameObject.SetActive(false);}foreach(string n in new[]{"Key Light","Fill Light","Battle Post Processing"}){var o=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==n);if(o)o.SetActive(false);}
    var root=Group("OrbitalFrontier_Battlefield");var origin=Group("LayoutOrigin",root);origin.localPosition=new Vector3(4.85f,0,0);Planets(Group("01_CelestialBodies",origin));Station(Group("02_Station",origin));Ships(Group("03_Ships",origin),alien[0]);Asteroids(Group("04_AsteroidField",origin),asteroid);Salvage(Group("05_Salvage",origin));ScenicFlybys(Group("08_Flybys",origin));Lighting(Group("06_Lighting",root));
    var sw=battle.GetComponent<BattleEnvironmentSwitcher>();sw.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="ORBITAL FRONTIER · EARTHWARD",root=root.gameObject,ambient=new Color(.3f,.36f,.47f),background=Color.black}};sw.trainingRenderers=Array.Empty<MeshRenderer>();sw.Apply(0);if(sw.nextButton)sw.nextButton.gameObject.SetActive(false);
    foreach(var a in board.GetComponentsInChildren<BattleActor>(true))a.zeroGravity=true;var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.freeOrbit=true;orbit.environmentFarClip=10000;orbit.homeYaw=65;orbit.homePitch=18;orbit.homeDistance=10;orbit.minDistance=2.5f;orbit.maxDistance=140;orbit.minPitch=-89;orbit.maxPitch=89;orbit.ResetView(true);CameraSettings(battle.battleCamera);battle.initialEnemies=1;Reviews(Group("07_ReviewCameras",root),battle.battleCamera);
    PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/OrbitalFrontier_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Register();File.WriteAllText(Root+"/Documentation/AssetManifest.md","# Orbital Frontier\n\n"+manifest);Debug.Log("ORBITAL_FRONTIER_READY "+ScenePath);
   }finally{Random.state=state;}
  }
  static void Register()
  {var cat=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var d=cat.Find("orbital-frontier");if(!d){d=ScriptableObject.CreateInstance<EnvironmentDefinition>();d.id="orbital-frontier";AssetDatabase.CreateAsset(d,Root+"/OrbitalFrontierDefinition.asset");}d.displayName="ORBITAL FRONTIER · EARTHWARD";d.description="Gundam ngoài không gian · Earth, tàu chiến, trạm quỹ đạo và thiên thạch · xoay 360°.";d.available=true;d.battleVariant=-1;d.accent=new Color(.25f,.6f,1);d.scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);d.SyncScene();EditorUtility.SetDirty(d);cat.environments=new[]{d}.Concat(cat.environments.Where(x=>x&&x!=d)).ToArray();EditorUtility.SetDirty(cat);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();}
  [MenuItem("Tools/Orbital Frontier/Capture Review")]
  public static void Capture()
  {Directory.CreateDirectory(Root+"/Documentation/Review");foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>().Where(c=>c.name.StartsWith("Orbital_"))){var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Root+"/Documentation/Review/"+c.name+".png",tex.EncodeToPNG());}finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}}AssetDatabase.Refresh();var def=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("orbital-frontier");if(def){def.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Documentation/Review/Orbital_Gameplay.png");EditorUtility.SetDirty(def);AssetDatabase.SaveAssets();}Debug.Log("ORBITAL_REVIEW_SAVED");}
 }
}





namespace OrbitalFrontier.Editor
{
 public static partial class OrbitalFrontierBuilder
 {
  const string Sparrow="Assets/FreeSciFi/Prefabs/PolygonSpaceship/";
  static GameObject WreckPart(string file,string name,Transform parent,Vector3 at,float extent,Vector3 angles)
  {
   var o=Place(file,name,parent,at,extent,angles);
   foreach(var ps in o.GetComponentsInChildren<ParticleSystem>(true)){ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.gameObject.SetActive(false);}
   return o;
  }
  static void SpaceWrecks(Transform p)
  {
   var omega=Group("W01 Ruptured Omega interceptor",p);omega.localPosition=new Vector3(52,5,78);omega.localRotation=Quaternion.Euler(17,-34,21);
   WreckPart("Assets/Omega_fighterG/Prefabs/OutBody.prefab","Omega damaged outer hull",omega,new Vector3(0,0,0),22,new Vector3(0,0,0));
   WreckPart("Assets/Omega_fighterG/Prefabs/Frame_body.prefab","Exposed frame drifting away",omega,new Vector3(-3,-2,5),17,new Vector3(9,-10,12));
   WreckPart("Assets/Omega_fighterG/Prefabs/Cockipt.prefab","Separated cockpit block",omega,new Vector3(1,1,-13),5,new Vector3(-15,21,6));
   WreckPart(Sparrow+"StarSparrow_Engine_LP_Red.prefab","Detached drive assembly",omega,new Vector3(12,0,7),6,new Vector3(31,50,17));
   WreckPart(Sparrow+"StarSparrow_Wing_2_LP_Red.prefab","Torn-off stabilizer",omega,new Vector3(-12,4,3),9,new Vector3(8,71,54));
   Motion(omega.gameObject,new Vector3(.045f,.08f,.035f),.5f,Vector3.up);
   var sparrow=Group("W02 Split StarSparrow wreck",p);sparrow.localPosition=new Vector3(-24,15,102);sparrow.localRotation=Quaternion.Euler(-15,65,-22);
   WreckPart(Sparrow+"StarSparrow_Core_LP_Red.prefab","Cracked red fuselage",sparrow,Vector3.zero,12,Vector3.zero);
   WreckPart(Sparrow+"StarSparrow_Wing_1_LP_Red.prefab","Bent port wing",sparrow,new Vector3(-5,0,1),11,new Vector3(8,-12,24));
   WreckPart(Sparrow+"StarSparrow_Wing_1_Mirrored_LP_Red.prefab","Detached starboard wing",sparrow,new Vector3(9,3,4),10,new Vector3(-12,20,43));
   WreckPart(Sparrow+"StarSparrow_Engine_LP_Red.prefab","Broken power section",sparrow,new Vector3(0,-2,9),7,new Vector3(24,36,11));
   Motion(sparrow.gameObject,new Vector3(-.05f,.12f,.08f),.4f,new Vector3(1,.2f,0));
   var engine=Group("W03 Derelict drive section",p);engine.localPosition=new Vector3(15,-24,-82);engine.localRotation=Quaternion.Euler(35,29,-14);
   WreckPart("Assets/Omega_fighterG/Prefabs/Frame_body.prefab","Skeletal drive spine",engine,Vector3.zero,16,new Vector3(0,0,0));
   for(int i=0;i<3;i++)WreckPart(Sparrow+"StarSparrow_Engine_LP_Red.prefab","Drifting drive nacelle "+i,engine,new Vector3((i-1)*5,0,5+i),5,new Vector3(i*8,12,i*17));
   Motion(engine.gameObject,new Vector3(.1f,-.13f,.06f));
   manifest.AppendLine("- V2: primitive cylinder station removed. W01–W03 are wrecks made from supplied Omega hull/frame/cockpit and StarSparrow core/wing/engine prefabs.");
  }
  static void AssetFragments(Transform p)
  {
   string[] source={"StarSparrow_Wing_3_LP_Red","StarSparrow_Engine_LP_Red","StarSparrow_Weapon_LP_Red","StarSparrow_Wing_2_LP_Red","StarSparrow_Thruster_LP_Red","StarSparrow_Core_LP_Red"};
   for(int i=0;i<36;i++)
   {
    float a=i*2.399963f;Vector3 pos=new Vector3(Mathf.Cos(a)*(69+i%4*13),-18-i%6*7,Mathf.Sin(a)*(75+i%5*10));
    if(i<6)pos=new Vector3(33+i*8,2+i%3*3,67+i%2*13);
    var o=WreckPart(Sparrow+source[i%source.Length]+".prefab","D"+i.ToString("00")+" Salvaged "+source[i%source.Length],p,pos,i<6?4+i%3:1.2f+i%4*.65f,new Vector3(i*37,i*73,i*29));
    Motion(o,Random.onUnitSphere*Random.Range(.25f,1.2f),.3f,Random.onUnitSphere);
   }
   manifest.AppendLine("- V2: toy satellite and generic boxes replaced with 36 source-asset ship parts, including wings, nacelles, weapons, thrusters and torn cores.");
  }
  static void GasGiants(Transform p,Mesh sphere)
  {
   var jMat=Mat("Jupiter turbulent cloud belts",Color.white,"OrbitalFrontier/Planet");jMat.SetFloat("_GasGiant",1);jMat.SetFloat("_Moon",0);
   var j=MeshObject("E04 Jupiter - procedural cloud bands",p,sphere,jMat);j.transform.localPosition=new Vector3(2900,350,250);j.transform.localScale=Vector3.one*200;j.transform.localRotation=Quaternion.Euler(3,4,0);Motion(j,new Vector3(0,1.1f,0));
   var system=Group("E05 Saturn system",p);system.localPosition=new Vector3(-2400,650,1800);system.localRotation=Quaternion.Euler(55,12,12);
   var sMat=Mat("Saturn pale atmospheric belts",Color.white,"OrbitalFrontier/Planet");sMat.SetFloat("_GasGiant",2);sMat.SetFloat("_Moon",0);var s=MeshObject("E05 Saturn body",system,sphere,sMat);s.transform.localScale=Vector3.one*180;Motion(s,new Vector3(0,.7f,0));
   var verts=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();const int n=256;for(int i=0;i<=n;i++){float a=i/(float)n*Mathf.PI*2;for(int k=0;k<2;k++){float r=k==0?1.3f:2.35f;verts.Add(new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r));uv.Add(new Vector2(k,i/(float)n));}}for(int i=0;i<n;i++){int a=i*2;tri.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}var m=new Mesh();m.SetVertices(verts);m.SetUVs(0,uv);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateBounds();
   var rings=MeshObject("E05 Saturn annular dust rings",system,Persist(m,"SaturnRings"),Mat("Saturn fine rings",new Color(.73f,.63f,.44f,.85f),"OrbitalFrontier/Planet Rings"));rings.transform.localScale=Vector3.one*180;rings.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
   manifest.AppendLine("- V2: Jupiter cloud belts/vortex and Saturn body/rings use procedural shaders. Celestial bodies are composed for the game view, not astronomical distances.");
  }
  static LineRenderer Wake(Transform p,string name,Material m,float width,Color head)
  {
   var l=Group(name,p).gameObject.AddComponent<LineRenderer>();l.sharedMaterial=m;l.useWorldSpace=false;l.positionCount=16;l.alignment=LineAlignment.View;l.textureMode=LineTextureMode.Stretch;l.widthMultiplier=width;l.widthCurve=new AnimationCurve(new Keyframe(0,1),new Keyframe(.12f,.85f),new Keyframe(1,0));var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(head,0),new GradientColorKey(new Color(1,.24f,.012f),.5f),new GradientColorKey(new Color(.65f,.015f,.001f),1)},new[]{new GradientAlphaKey(.9f,0),new GradientAlphaKey(.55f,.3f),new GradientAlphaKey(0,1)});l.colorGradient=gradient;l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;return l;
  }
  static void ScenicFlybys(Transform p)
  {
   var material=Mat("Cool luminous asteroid wake",new Color(1.5f,1.5f,1.5f,1),"OrbitalFrontier/Scenic Wake");string[] asteroid=AssetDatabase.FindAssets("t:Prefab",new[]{Root+"/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).Where(x=>Path.GetFileName(x).StartsWith("Asteroids_")).OrderBy(x=>x).ToArray();
   Vector3 forward=new Vector3(.906f,0,.423f),right=new Vector3(.423f,0,-.906f);
   for(int i=0;i<6;i++)
   {
    var g=Group("F"+i+" Continuous luminous asteroid flyby",p);var f=g.gameObject.AddComponent<OrbitalFlyby>();float depth=95+i*18;float h=10+i%3*12;var center=forward*depth+Vector3.up*h;Vector3 diagonal=right*90+Vector3.up*(i%2==0?-13:18);f.start=center-diagonal;f.end=center+diagonal;f.duration=6.5f+i*.85f;f.phase=i/6f;f.tailLength=14+i*2;
    var body=Place(asteroid[i%asteroid.Length],"Small tumbling meteor head",g,Vector3.zero,.65f+i%3*.22f,new Vector3(i*17,i*43,i*37));Motion(body,Random.onUnitSphere*26);
    f.core=Wake(g,"Golden meteor core",material,.16f,new Color(1,.85f,.24f));f.wake=Wake(g,"Crimson orange outer wake",material,.65f,new Color(1,.38f,.025f));
    float u=f.phase;g.localPosition=Vector3.Lerp(f.start,f.end,u);Vector3 direction=-(f.end-f.start).normalized;float tail=Mathf.Min(f.tailLength,u*(f.end-f.start).magnitude);foreach(var line in new[]{f.core,f.wake})for(int k=0;k<16;k++)line.SetPosition(k,direction*tail*k/15f);
   }
   manifest.AppendLine("- V2: six continuously recycled flybys, staggered phases, dual tapered luminous wakes; fixed paths remain outside combat. Analytic tails do not join the endpoints at wrap.");
  }
  [MenuItem("Tools/Orbital Frontier/Apply V2 Wrecks Planets And Flybys")]
  public static void UpgradeV2()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first");if(SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);
   var root=GameObject.Find("OrbitalFrontier_Battlefield");if(!root)throw new Exception("Space root missing");var layout=root.transform.Find("LayoutOrigin");var state=Random.state;
   try
   {
    Random.InitState(91043);converted.Clear();manifest.Clear();Materials();
    foreach(string path in new[]{"02_Station","05_Salvage","08_Flybys"}){var old=layout.Find(path);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);}
    SpaceWrecks(Group("02_Station",layout));AssetFragments(Group("05_Salvage",layout));ScenicFlybys(Group("08_Flybys",layout));
    var celestial=layout.Find("01_CelestialBodies");foreach(string n in new[]{"E04 Jupiter - procedural cloud bands","E05 Saturn system"}){var old=celestial.Find(n);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);}GasGiants(celestial,AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/CelestialSphere.asset"));
    celestial.Find("E01 Earth system/E01 Earth surface NASA").GetComponent<OrbitalMotion>().angularVelocity=new Vector3(0,.8f,0);celestial.Find("E01 Earth system/E01-C Differential cloud layer").GetComponent<OrbitalMotion>().angularVelocity=new Vector3(0,1.03f,0);
    var ships=layout.Find("03_Ships");var hero=ships.Find("S01 Alien capital ship");hero.localPosition=new Vector3(132,10,0);var heroRotation=hero.localRotation;hero.localRotation=Quaternion.identity;hero.localScale=Vector3.one;var heroBounds=BoundsOf(hero.gameObject);hero.localScale=Vector3.one*(56/Mathf.Max(heroBounds.size.x,Mathf.Max(heroBounds.size.y,heroBounds.size.z)));hero.localRotation=heroRotation;ships.Find("S07 R35 docking transport").localPosition=new Vector3(48,-9,60);
    for(int i=0;i<3;i++)ships.Find("S0"+(i+2)+" StarSparrow patrol").localPosition=new Vector3(58+i*12,6+i*3,50+i*13);
    for(int i=0;i<2;i++)ships.Find("S0"+(i+5)+" SF escort").localPosition=new Vector3(65+i*13,-14+i*4,-5-i*10);
    var reviews=root.transform.Find("07_ReviewCameras");var cam=reviews.Find("Orbital_Wreckage")??reviews.Find("Orbital_Station");cam.name="Orbital_Wreckage";cam.position=new Vector3(-5,30,25);cam.LookAt(new Vector3(57,5,78));var cap=reviews.Find("Orbital_CapitalShip");cap.position=new Vector3(20,22,-40);cap.LookAt(new Vector3(136.85f,10,0));
    PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/OrbitalFrontier_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();File.WriteAllText(Root+"/Documentation/V2_Changes.md","# V2 space scenery revision\n\n"+manifest);Debug.Log("ORBITAL_FRONTIER_V2_SAVED");
   }finally{Random.state=state;}
  }
 }
}








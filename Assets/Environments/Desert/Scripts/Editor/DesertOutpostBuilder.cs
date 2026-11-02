using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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

namespace DesertOutpost.Editor
{
 public static class DesertOutpostBuilder
 {
  public const string Root="Assets/Environments/Desert",ScenePath=Root+"/Scenes/Desert_Battle.unity";
  static readonly Dictionary<Material,Material> converted=new Dictionary<Material,Material>();
  static readonly List<Bounds> buildings=new List<Bounds>();
  static readonly StringBuilder manifest=new StringBuilder();
  static Material sandstone,sand,wood,rust,dark,olive,cactus;
  static int meshIndex;
  static Transform Group(string name,Transform p=null){var g=new GameObject(name);g.transform.SetParent(p,false);return g.transform;}
  static Bounds BoundsOf(GameObject o){var rs=o.GetComponentsInChildren<Renderer>(true);var b=new Bounds(o.transform.position,Vector3.zero);if(rs.Length>0){b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);}return b;}
  static Material Lit(string name,Color c,float smooth=.15f){string p=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
  static Material Convert(Material original)
  {
   if(!original)return sand;if(converted.TryGetValue(original,out var cached))return cached;
   string id=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(original));string name=original.name+"_"+id.Substring(0,Math.Min(8,id.Length));
   var m=Lit(name,Color.white);Texture tex=null;foreach(string prop in new[]{"_BaseMap","_MainTex","_BaseColorMap"})if(original.HasProperty(prop)&&original.GetTexture(prop)){tex=original.GetTexture(prop);break;}
   Color tint=Color.white;if(original.HasProperty("_Color"))tint=original.GetColor("_Color");else if(original.HasProperty("_BaseColor"))tint=original.GetColor("_BaseColor");
   m.SetColor("_BaseColor",tint);if(tex){m.SetTexture("_BaseMap",tex);if(original.HasProperty("_MainTex")){m.SetTextureScale("_BaseMap",original.GetTextureScale("_MainTex"));m.SetTextureOffset("_BaseMap",original.GetTextureOffset("_MainTex"));}}
   if(original.HasProperty("_BumpMap")&&original.GetTexture("_BumpMap")){m.SetTexture("_BumpMap",original.GetTexture("_BumpMap"));m.SetFloat("_BumpScale",.65f);m.EnableKeyword("_NORMALMAP");}
   string path=AssetDatabase.GetAssetPath(original);bool leaf=path.Contains("Flora-Form");bool clip=leaf||(original.HasProperty("_Mode")&&original.GetFloat("_Mode")==1);
   if(clip){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.4f);m.EnableKeyword("_ALPHATEST_ON");m.SetOverrideTag("RenderType","TransparentCutout");m.renderQueue=2450;m.SetFloat("_Cull",0);}else{m.SetFloat("_AlphaClip",0);m.DisableKeyword("_ALPHATEST_ON");m.SetFloat("_Cull",2);m.renderQueue=-1;}
   converted[original]=m;manifest.AppendLine("- "+path+" → "+AssetDatabase.GetAssetPath(m));return m;
  }
  static void Repair(GameObject o,bool collision)
  {
   foreach(var r in o.GetComponentsInChildren<Renderer>(true)){r.sharedMaterials=r.sharedMaterials.Select(Convert).ToArray();r.gameObject.isStatic=true;r.gameObject.layer=2;}
   foreach(var a in o.GetComponentsInChildren<Animator>())UnityEngine.Object.DestroyImmediate(a);
   foreach(var l in o.GetComponentsInChildren<Light>())UnityEngine.Object.DestroyImmediate(l);
   if(!collision)foreach(var c in o.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
  }
  static GameObject Place(string path,string name,Transform parent,Vector3 pos,float height,float yaw=0,float maxWidth=100,bool collision=false)
  {
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset){manifest.AppendLine("MISSING: "+path);return Group(name+" MISSING",parent).gameObject;}
   var pivot=Group(name,parent);var o=(GameObject)PrefabUtility.InstantiatePrefab(asset,pivot);o.transform.localPosition=Vector3.zero;o.transform.localRotation=Quaternion.Euler(0,yaw,0);
   var b=BoundsOf(o);float scale=height/Mathf.Max(.001f,b.size.y);scale=Mathf.Min(scale,maxWidth/Mathf.Max(b.size.x,b.size.z));o.transform.localScale*=scale;b=BoundsOf(o);o.transform.position+=new Vector3(-b.center.x,-b.min.y,-b.center.z)+pivot.position;pivot.localPosition=pos;Repair(o,collision);
   if(collision){foreach(var mf in o.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh&&!mf.GetComponent<Collider>())mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;buildings.Add(BoundsOf(o));}
   manifest.AppendLine("- "+name+": "+path+" @ "+pos);return pivot.gameObject;
  }
  static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material m,bool collision=false)
  {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=m;o.isStatic=true;o.layer=2;if(!collision)UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;}
  static void Beam(string name,Transform p,Vector3 a,Vector3 b,float width,Material m){var g=Box(name,p,(a+b)*.5f,new Vector3(width,(a-b).magnitude,width),m);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
  static Mesh Persist(Mesh m,string name){string p=Root+"/Meshes/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(existing){EditorUtility.CopySerialized(m,existing);UnityEngine.Object.DestroyImmediate(m);return existing;}m.name=name;AssetDatabase.CreateAsset(m,p);return m;}
  static GameObject MeshObject(string name,Transform parent,Mesh mesh,Material m,bool collide=false){var o=Group(name,parent).gameObject;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=m;o.isStatic=true;o.layer=2;if(collide)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;}
  static Mesh MakeMesh(List<Vector3> v,List<int> t,List<Vector2> uv,string name){var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();m.RecalculateTangents();return Persist(m,name);}
  static float Height(float x,float z)
  {
   if(Mathf.Abs(x)<60&&Mathf.Abs(z)<46)return 0;
   float n=Mathf.PerlinNoise(x*.019f+31,z*.019f+62);float margin=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Max(Mathf.Abs(x)/60,Mathf.Abs(z)/46)-1)*.55f));return margin*(1+n*6);
  }
  static void Ground(Transform p)
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();int n=150;float width=750;
   for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){float xx=(x/(float)n-.5f)*width,zz=(z/(float)n-.5f)*width;v.Add(new Vector3(xx,Height(xx,zz)-.015f,zz));uv.Add(new Vector2(xx*.2f,zz*.2f));}
   for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;t.AddRange(new[]{a,a+n+1,a+1,a+1,a+n+1,a+n+2});}
   var o=MeshObject("Continuous sand and compacted combat road",p,MakeMesh(v,t,uv,"DesertGround"),sand,true);o.layer=0;
  }
  static Mesh Mesa(string name,float radius,float height,int seed)
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();int sides=44,rings=25;
   for(int j=0;j<=rings;j++)for(int k=0;k<=sides;k++){float y=j/(float)rings,a=k/(float)sides*Mathf.PI*2;float profile=Mathf.Lerp(1,.56f,y)+Mathf.Sin(y*36+seed)*.025f;float noise=Mathf.PerlinNoise(Mathf.Cos(a)*1.7f+seed,Mathf.Sin(a)*1.7f+j*.07f);float r=radius*profile*(.8f+noise*.32f);float top=1+.07f*Mathf.Sin(a*3+seed);v.Add(new Vector3(Mathf.Cos(a)*r,y*height*top,Mathf.Sin(a)*r));uv.Add(new Vector2(k/(float)sides,y));}
   for(int j=0;j<rings;j++)for(int k=0;k<sides;k++){int a=j*(sides+1)+k;t.AddRange(new[]{a,a+1,a+sides+1,a+1,a+sides+2,a+sides+1});}
   int center=v.Count;v.Add(new Vector3(0,height,0));uv.Add(Vector2.one*.5f);for(int k=0;k<sides;k++)t.AddRange(new[]{center,rings*(sides+1)+k,rings*(sides+1)+k+1});
   for(int i=0;i<t.Count;i+=3){int a=t[i];t[i]=t[i+2];t[i+2]=a;}return MakeMesh(v,t,uv,name);
  }
  static void Arch(Transform parent)
  {
   var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();int segments=48,tube=14;
   for(int i=0;i<=segments;i++){float a=i/(float)segments*Mathf.PI;for(int j=0;j<=tube;j++){float q=j/(float)tube*Mathf.PI*2;float r=4.5f+Mathf.Sin(a*5)*.6f+Mathf.Sin(q*3+a*11)*.25f;v.Add(new Vector3(Mathf.Cos(a)*(17+Mathf.Cos(q)*r),Mathf.Sin(a)*(25+Mathf.Cos(q)*r),Mathf.Sin(q)*r*1.15f));uv.Add(new Vector2(i/(float)segments,j/(float)tube));}}
   for(int i=0;i<segments;i++)for(int j=0;j<tube;j++){int a=i*(tube+1)+j;t.AddRange(new[]{a,a+tube+1,a+1,a+1,a+tube+1,a+tube+2});}
   var o=MeshObject("M02 Natural erosion arch — open geometry",parent,MakeMesh(v,t,uv,"NaturalArch"),sandstone,true);o.transform.localPosition=new Vector3(15,5,89);o.transform.localRotation=Quaternion.Euler(0,-12,0);
   for(int i=0;i<2;i++){var foot=MeshObject("M02 eroded supporting buttress",parent,Mesa("ArchButtress"+i,8,13,61+i),sandstone,true);foot.transform.localPosition=new Vector3(i==0?-2:32,-1,89);}
  }
  static void Landscape(Transform p)
  {
   for(int i=0;i<9;i++){var o=MeshObject("M01 North sandstone wall "+i,p,Mesa("NorthMesa"+i,17+i%3*3,32+i%4*6,13+i),sandstone,true);o.transform.localPosition=new Vector3(-98+i*25,-2,78+(i%2)*12);o.transform.localScale=new Vector3(1,1,1.1f);}
   // Leave the central arch opening clear instead of plugging it with a wall.
   foreach(Transform x in p)if(x.name.StartsWith("M01")&&x.localPosition.x>-25&&x.localPosition.x<60)x.gameObject.SetActive(false);
   for(int i=0;i<8;i++){var o=MeshObject("M03 South mesa "+i,p,Mesa("SouthMesa"+i,20,23+i%3*6,30+i),sandstone,true);o.transform.localPosition=new Vector3(-92+i*27,-2,-86-i%2*7);}
   for(int i=0;i<6;i++){var o=MeshObject(i<3?"M04 Western spires":"M05 Eastern canyon",p,Mesa("SideMesa"+i,i<3?8:15,24+i%3*9,47+i),sandstone,true);o.transform.localPosition=new Vector3(i<3?-88:88,0,-32+i%3*33);o.transform.localScale=new Vector3(.85f,1,1.25f);}
   Arch(p);
   for(int i=0;i<8;i++){int index=i%5+1;var o=Place("Assets/InfinitaStudio - Mountains/Prefabs/Fbx/mountain_canyon_0"+index+".prefab","M07 Distant mountain "+i,p,new Vector3(Mathf.Cos(i*Mathf.PI/4)*225,-8,Mathf.Sin(i*Mathf.PI/4)*225),75+i%3*12,i*41,160);}
   for(int i=0;i<2;i++){var o=MeshObject("M06 Sand dune "+i,p,Mesa("Dune"+i,14,4.3f,88+i),sand);o.transform.localPosition=i==0?new Vector3(-40,-1,54):new Vector3(34,-1,-56);o.transform.localScale=new Vector3(1.6f,1,1);}
   Vector2[] rocks={new Vector2(-40,17),new Vector2(-21,-19),new Vector2(13,19),new Vector2(35,-19),new Vector2(-51,-34),new Vector2(48,12),new Vector2(-8,41),new Vector2(23,-43)};
   for(int i=0;i<rocks.Length;i++){var g=Group("R0"+(i+1)+" Rock cluster",p);g.localPosition=new Vector3(rocks[i].x,0,rocks[i].y);for(int j=0;j<4;j++){var o=MeshObject("Weathered sandstone",g,Mesa("Boulder"+i+"_"+j,j==0?1.6f:.5f,j==0?1.9f:.5f,100+i*4+j),sandstone);o.transform.localPosition=new Vector3(j==0?0:Random.Range(-2,2),-.15f,j==0?0:Random.Range(-2,2));o.transform.localRotation=Quaternion.Euler(Random.Range(-20,20),Random.Range(0,360),Random.Range(-10,10));}}
  }
  [Serializable] class ImportedMesh {public string name,file;public int material;}
  [Serializable] class ImportedMaterial {public string name,baseMap;public float[] color;public float metallic,roughness;public bool doubleSided;}
  [Serializable] class ImportedManifest {public ImportedMesh[] meshes;public ImportedMaterial[] materials;public float[] min,max;}
  static GameObject ImportGLB(string name)
  {
   string data=Root+"/ImportData/"+name;var d=JsonUtility.FromJson<ImportedManifest>(File.ReadAllText(data+"/manifest.json"));var mats=new List<Material>();
   for(int i=0;i<d.materials.Length;i++){var e=d.materials[i];var m=Lit(name+"_"+i,new Color(e.color[0],e.color[1],e.color[2],e.color[3]),Mathf.Min(.5f,1-e.roughness));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(data+"/"+e.baseMap));m.SetFloat("_Cull",e.doubleSided?0:2);mats.Add(m);}
   var g=Group(name+" imported GLB");var min=new Vector3(d.min[0],d.min[1],d.min[2]);var max=new Vector3(d.max[0],d.max[1],d.max[2]);var center=(min+max)*.5f;center.y=min.y;float scale=1/Mathf.Max(.00001f,max.y-min.y);
   for(int k=0;k<d.meshes.Length;k++){var e=d.meshes[k];using(var r=new BinaryReader(File.OpenRead(data+"/"+e.file))){int n=r.ReadInt32(),tn=r.ReadInt32();var v=new Vector3[n];var normal=new Vector3[n];var uv=new Vector2[n];var t=new int[tn];for(int i=0;i<n;i++)v[i]=(new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle())-center)*scale;for(int i=0;i<n;i++)normal[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());for(int i=0;i<n;i++)uv[i]=new Vector2(r.ReadSingle(),r.ReadSingle());for(int i=0;i<tn;i++)t[i]=r.ReadInt32();var mesh=new Mesh{indexFormat=IndexFormat.UInt32,vertices=v,normals=normal,uv=uv,triangles=t};mesh.RecalculateBounds();mesh.RecalculateTangents();MeshObject(e.name,g,Persist(mesh,name+"_"+k),mats[e.material]);}}
   var asset=PrefabUtility.SaveAsPrefabAsset(g.gameObject,Root+"/Prefabs/"+name+"_Imported.prefab");UnityEngine.Object.DestroyImmediate(g.gameObject);manifest.AppendLine("- GLB source: D:/Downloads/"+(name=="Shack"?"old_shack.glb":"old_wooden_watchtower_house_3.glb")+"; normalized by geometry bounds.");return asset;
  }
  static string Abandoned(string s)=>"Assets/Abandoned buildings/Prefab/abandoned_buildings/"+s+".prefab";
  static string Factory(string s)=>"Assets/TirgamesAssets/Factory/Prefabs/"+s+".prefab";
  static GameObject House(string id,string source,Transform p,Vector3 pos,float height,float width,float yaw=0)=>Place(source,id,p,pos,height,yaw,width,true);
  static void Stairs(Transform parent,Vector3 start,float height,int steps=12)
  {for(int i=0;i<steps;i++)Box("Weathered stair tread",parent,start+new Vector3(0,height*(i+.5f)/steps,-i*.32f),new Vector3(1.4f,.12f,.38f),wood);Beam("Stair stringer",parent,start+new Vector3(-.6f,0,.1f),start+new Vector3(-.6f,height,-steps*.32f),.15f,wood);Beam("Stair stringer",parent,start+new Vector3(.6f,0,.1f),start+new Vector3(.6f,height,-steps*.32f),.15f,wood);}
  static void Buildings(Transform p)
  {
   string desert="Assets/Desert Buildings/Prefabs/Desert_Building_V";
   var h=House("H01 Balcony timber house",Abandoned("house_aband"),p,new Vector3(-31,0,29),6.2f,13,180).transform;
   Place(Abandoned("house_roof"),"Patched roof",h,new Vector3(0,6.1f,0),2.1f,180,13);
   Box("Balcony weathered deck",h,new Vector3(0,3,-5),new Vector3(10,.2f,2.2f),wood,true);
   for(int i=0;i<5;i++){float x=-4.8f+i*2.4f;Beam("Deck support post",h,new Vector3(x,0,-5.7f),new Vector3(x,4,-5.7f),.17f,wood);if(i<4)Beam("Diagonal brace",h,new Vector3(x,.8f,-5.7f),new Vector3(x+2.4f,2.9f,-5.7f),.16f,wood);}
   Beam("Balcony handrail",h,new Vector3(-5,4,-6),new Vector3(5,4,-6),.13f,wood);Stairs(h,new Vector3(6,0,-3),3,12);
   House("H02 Sand plaster home",desert+"3.prefab",p,new Vector3(-13,0,30),8,12,180);
   House("H03 Partially collapsed dwelling",Abandoned("house_ruined"),p,new Vector3(4,0,30),5,12,180);
   var h4=House("H04 Machine repair shed",Root+"/Prefabs/Shack_Imported.prefab",p,new Vector3(23,0,29),5.2f,12,180);
   House("H05 Eastern desert dwelling",desert+"1.prefab",p,new Vector3(41,0,27),5,11,180);
   House("H06 Old shack",Root+"/Prefabs/Shack_Imported.prefab",p,new Vector3(-35,0,-28),5.5f,11,0);
   var h7=House("H07 Abandoned supply trader",desert+"4.prefab",p,new Vector3(-18,0,-28),5.5f,12,0).transform;
   for(int i=0;i<4;i++)Beam("Awning post",h7,new Vector3(-4+i*2.7f,0,5),new Vector3(-4+i*2.7f,3,5),.15f,wood);
   Box("Faded sheet awning",h7,new Vector3(0,3.15f,4),new Vector3(9,.08f,3),rust);
   var h8=House("H08 Boarded timber dwelling",Abandoned("house_aband"),p,new Vector3(0,0,-29),4.7f,11).transform;Place(Abandoned("house_roof"),"Gabled roof",h8,new Vector3(0,4.5f,0),2,0,11);
   House("H09 Supply station office",desert+"5.prefab",p,new Vector3(19,0,-29),7.5f,13);
   House("H10 Damaged supply warehouse",desert+"2.prefab",p,new Vector3(39,0,-29),7,13,0);
   House("H11 Wooden watchtower",Root+"/Prefabs/Watchtower_Imported.prefab",p,new Vector3(-48,0,29),18,8,0);
   House("H12 Pump station",Root+"/Prefabs/Shack_Imported.prefab",p,new Vector3(50,0,-8),4,8,90);
   // Collapsed rafters and a battered door define the ruined house without plugging its opening.
   for(int i=0;i<6;i++){var o=Box("H03 fallen roof timber",p,new Vector3(7+Random.Range(-2,2),.2f,36+Random.Range(-2,2)),new Vector3(.2f,.2f,Random.Range(2,4)),wood);o.transform.Rotate(0,Random.Range(0,180),Random.Range(-5,5));}
  }
  static void Cylinder(string name,Transform p,Vector3 pos,float radius,float height,Material material)
  {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos+Vector3.up*height*.5f;g.transform.localScale=new Vector3(radius*2,height*.5f,radius*2);g.GetComponent<Renderer>().sharedMaterial=material;g.layer=2;g.isStatic=true;}
  static void Tube(string name,Transform parent,List<Vector3> path,float radius,Material mat,bool ribs=false)
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();int sides=ribs?48:10;
   for(int i=0;i<path.Count;i++){var direction=i==path.Count-1?path[i]-path[i-1]:path[i+1]-path[i];var rotation=Quaternion.FromToRotation(Vector3.up,direction);float taper=i==path.Count-1?.1f:1;
    for(int j=0;j<=sides;j++){float a=j/(float)sides*Mathf.PI*2;float r=radius*taper*(ribs?1+.09f*Mathf.Cos(a*12):1);v.Add(path[i]+rotation*new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r));uv.Add(new Vector2(j/(float)sides,i/(float)path.Count));}}
   for(int i=0;i<path.Count-1;i++)for(int j=0;j<sides;j++){int a=i*(sides+1)+j;t.AddRange(new[]{a,a+1,a+sides+1,a+1,a+sides+2,a+sides+1});}
   for(int i=0;i<t.Count;i+=3){int a=t[i];t[i]=t[i+2];t[i+2]=a;}MeshObject(name,parent,MakeMesh(v,t,uv,"Tube_"+meshIndex++),mat);
  }
  static void Crate(Transform p,Vector3 pos,float size)
  {var g=Group("Aged supply crate",p);g.localPosition=pos;Box("Dark inner box",g,new Vector3(0,size*.5f,0),Vector3.one*size,dark);for(int i=0;i<5;i++){float y=size*(i+.5f)/5;Box("Front plank",g,new Vector3(0,y,-size*.51f),new Vector3(size,.17f*size,.05f),wood);Box("Back plank",g,new Vector3(0,y,size*.51f),new Vector3(size,.17f*size,.05f),wood);}for(int side=-1;side<=1;side+=2){Beam("Crate brace",g,new Vector3(-size*.46f,0,side*size*.54f),new Vector3(size*.46f,size,side*size*.54f),.09f,wood);}Box("Crate lid",g,new Vector3(0,size,0),new Vector3(size,.08f,size),wood);}
  static void Industry(Transform p)
  {
   for(int i=0;i<3;i++){var g=Group("I01 Storage vessel "+i,p);g.localPosition=new Vector3(40+i*3.4f,0,-43);Cylinder("Rusted vessel",g,Vector3.zero,1.25f,6+i%2,rust);Cylinder("Top rim",g,new Vector3(0,6+i%2,0),1.3f,.15f,dark);Tube("Feed pipe",g,new List<Vector3>{new Vector3(0,5,0),new Vector3(0,5,2),new Vector3(0,1,2),new Vector3(0,1,4)},.12f,rust);}
   var tank=Group("I02 Water tank and ladder",p);tank.localPosition=new Vector3(49,0,41);Cylinder("Large tank",tank,Vector3.zero,2.7f,10,rust);for(int i=0;i<20;i++)Beam("Ladder rung",tank,new Vector3(-.5f,i*.45f,2.85f),new Vector3(.5f,i*.45f,2.85f),.06f,dark);Beam("Ladder rail",tank,new Vector3(-.55f,0,2.85f),new Vector3(-.55f,9.5f,2.85f),.08f,dark);Beam("Ladder rail",tank,new Vector3(.55f,0,2.85f),new Vector3(.55f,9.5f,2.85f),.08f,dark);
   Place(Factory("FanBig01Motor01"),"I03 Workshop motor",p,new Vector3(26,0,21),2,0,3);
   Place(Factory("MetalCabinet01_1"),"I03 Equipment cabinet",p,new Vector3(20,0,22),2,180,2);
   Vector2[] centers={new Vector2(-36,21),new Vector2(-14,22),new Vector2(25,21),new Vector2(42,19),new Vector2(-37,-20),new Vector2(-21,-20),new Vector2(19,-21),new Vector2(35,-40),new Vector2(49,-15)};
   for(int i=0;i<centers.Length;i++){var g=Group("I04 Supply cluster "+i,p);g.localPosition=new Vector3(centers[i].x,0,centers[i].y);for(int j=0;j<3;j++){Place(Factory("Barrel01"+(char)('a'+(i+j)%4)),"Weathered barrel",g,new Vector3(j*.9f,0,.3f*(j%2)),1.1f,j*59,1.4f);Crate(g,new Vector3(-1-j*.85f,0,j*.3f),.75f+j*.16f);}Place(Factory("Debris01_"+(i%6+1)),"Abandoned machine fragments",g,new Vector3(0,0,2),.25f,i*43,3);}
   for(int i=0;i<6;i++){float x=-40+i*16;var pole=Group("I06 Utility pole "+i,p);pole.localPosition=new Vector3(x,0,20);Beam("Weathered pole",pole,Vector3.zero,Vector3.up*8,.18f,wood);Beam("Cross arm",pole,new Vector3(0,7.4f,-1),new Vector3(0,7.4f,1),.16f,wood);if(i<5){var path=new List<Vector3>();for(int k=0;k<=16;k++){float f=k/16f;path.Add(new Vector3(x+16*f,7.5f-Mathf.Sin(f*Mathf.PI)*.8f,20));}Tube("Sagging power cable",p,path,.025f,dark);}}
   var canopy=Group("I05 Broken machine canopy",p);canopy.localPosition=new Vector3(-42,0,-40);for(int i=0;i<4;i++)Beam("Canopy post",canopy,new Vector3(i%2*8-4,0,i/2*5-2.5f),new Vector3(i%2*8-4,4,i/2*5-2.5f),.2f,wood);for(int i=0;i<5;i++)if(i!=2)Box("Old roof sheet",canopy,new Vector3(-3.2f+i*1.6f,4,0),new Vector3(1.55f,.08f,6),rust);
  }
  static bool ClearPlant(Vector3 local,Transform origin){if(Mathf.Abs(local.z)<15&&Mathf.Abs(local.x)<60)return false;Vector3 world=origin.TransformPoint(local);foreach(var b in buildings){var expanded=b;expanded.Expand(.8f);if(expanded.Contains(world+Vector3.up))return false;}return true;}
  static void Vegetation(Transform p)
  {
   Vector2[] locations={new Vector2(-44,19),new Vector2(-39,37),new Vector2(-24,40),new Vector2(-9,21),new Vector2(9,40),new Vector2(20,20),new Vector2(-19,37),new Vector2(46,35),new Vector2(-45,-23),new Vector2(-26,-39),new Vector2(3,-40),new Vector2(13,-20),new Vector2(30,-40),new Vector2(45,-20),new Vector2(-52,4),new Vector2(51,18)};
   float[] heights={5.5f,3.2f,4.5f,2.4f,5,3.6f,1.3f,6,4,2.2f,4.8f,2.8f,3.8f,5.3f,3,2};
   for(int i=0;i<locations.Length;i++){var g=Group("C"+(i+1).ToString("00")+" Ribbed desert cactus",p);g.localPosition=new Vector3(locations[i].x,0,locations[i].y);float h=heights[i],r=h*.055f;Tube("Ribbed main stem",g,new List<Vector3>{Vector3.zero,new Vector3(.05f,h*.4f,0),new Vector3(.08f,h*.85f,0),new Vector3(.08f,h*.98f,0),new Vector3(.08f,h,0)},r,cactus,true);if(i%3!=1){for(int side=-1;side<=1;side+=2){float y=h*(side<0?.4f:.6f);Tube("Curved cactus arm",g,new List<Vector3>{new Vector3(0,y,0),new Vector3(side*r*1.5f,y,0),new Vector3(side*r*2.8f,y+h*.08f,0),new Vector3(side*r*3,y+h*.23f,0),new Vector3(side*r*3,y+h*.26f,0)},r*.65f,cactus,true);}}}
   Vector2[] trees={new Vector2(-18,21),new Vector2(16,40),new Vector2(36,18),new Vector2(-29,-41),new Vector2(5,-40),new Vector2(48,8),new Vector2(-50,-10),new Vector2(52,-38)};
   for(int i=0;i<trees.Length;i++){var g=Group("T"+(i+1).ToString("00")+(i==4||i==6?" Dead desert tree":" Sparse olive desert tree"),p);g.localPosition=new Vector3(trees[i].x,0,trees[i].y);float h=i==1?8:5.5f;Tube("Gnarled trunk",g,new List<Vector3>{Vector3.zero,new Vector3(.2f,h*.35f,0),new Vector3(-.2f,h*.65f,.15f),new Vector3(.3f,h*.9f,0)},.19f,wood);for(int j=0;j<4;j++){float a=j*1.6f;var end=new Vector3(Mathf.Cos(a)*2,h*(.65f+j*.07f),Mathf.Sin(a)*1.6f);Tube("Irregular branching limb",g,new List<Vector3>{new Vector3(0,h*.4f,0),end*.75f,end},.1f,wood);if(i!=4&&i!=6){var bush=Place("Assets/Flora-Form/Vegetation/GreenBush.prefab","Sparse foliage",g,end-new Vector3(0,.35f,0),1.5f,j*67,2.8f);foreach(var r in bush.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;}}}}
   string[] types={"GrassDry_A","GrassDry_B","BushDry_A","BushDry_B","Bush_Twig"};
   for(int group=0;group<12;group++){var g=Group("G"+(group+1).ToString("00")+" Dry flora patch",p);float cx=-43+group%6*17,cz=group<6?20:-20;for(int j=0;j<25;j++){Vector3 pos=new Vector3(cx+Random.Range(-5,5),0,cz+Random.Range(-3,3));if(!ClearPlant(pos,p))continue;Place("Assets/Flora-Form/Vegetation/"+types[j%types.Length]+".prefab","Golden dry grass and scrub",g,pos,j%5<2?Random.Range(.25f,.65f):Random.Range(.45f,.9f),Random.Range(0,360),1.2f);}}
  }
  static void Lighting(Transform p)
  {
   var sun=Group("Warm late-afternoon sun",p).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.87f,.68f);sun.intensity=2.1f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(34,-45,0);RenderSettings.sun=sun;
   var skyPath=Root+"/Lighting/DesertSky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);if(!sky){sky=new Material(Shader.Find("Skybox/Procedural"));AssetDatabase.CreateAsset(sky,skyPath);}sky.SetColor("_SkyTint",new Color(.43f,.55f,.65f));sky.SetColor("_GroundColor",new Color(.45f,.32f,.2f));sky.SetFloat("_AtmosphereThickness",.8f);sky.SetFloat("_SunSize",.025f);sky.SetFloat("_Exposure",1.1f);RenderSettings.skybox=sky;
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.54f,.61f,.68f);RenderSettings.ambientEquatorColor=new Color(.46f,.4f,.31f);RenderSettings.ambientGroundColor=new Color(.3f,.22f,.14f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.67f,.57f,.44f);RenderSettings.fogDensity=.0023f;
   string path=Root+"/Lighting/DesertVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);AssetDatabase.AddObjectToAsset(tone,profile);var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.12f);bloom.threshold.Override(1.3f);AssetDatabase.AddObjectToAsset(bloom,profile);}
   var vol=Group("Desert colour and atmosphere",p).gameObject.AddComponent<Volume>();vol.isGlobal=true;vol.priority=10;vol.sharedProfile=profile;
   var dustMat=Lit("Airborne sand",new Color(.74f,.58f,.36f,.08f));dustMat.shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");dustMat.SetFloat("_Surface",1);dustMat.SetFloat("_SrcBlend",5);dustMat.SetFloat("_DstBlend",10);dustMat.SetFloat("_ZWrite",0);dustMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");dustMat.renderQueue=3000;
   var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;float a=Mathf.Pow(Mathf.Clamp01(1-d),2)*(.6f+.4f*Mathf.PerlinNoise(x*.15f,y*.15f));pixels[y*64+x]=new Color(1,1,1,a);}texture.SetPixels(pixels);texture.Apply();string tp=Root+"/Textures/DustSoft.asset";var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(tp);if(existing){EditorUtility.CopySerialized(texture,existing);UnityEngine.Object.DestroyImmediate(texture);texture=existing;}else AssetDatabase.CreateAsset(texture,tp);dustMat.SetTexture("_BaseMap",texture);dustMat.SetColor("_BaseColor",new Color(.8f,.65f,.45f,.10f));
   for(int i=0;i<6;i++){var ps=Group("D0"+(i+1)+" Low drifting sand",p).gameObject.AddComponent<ParticleSystem>();ps.transform.localPosition=new Vector3(-50+i*20,.5f,i%2==0?17:-18);var main=ps.main;main.startLifetime=13;main.startSize=new ParticleSystem.MinMaxCurve(3,6);main.startSpeed=.25f;main.maxParticles=45;main.startColor=new Color(.8f,.68f,.49f,.12f);main.simulationSpace=ParticleSystemSimulationSpace.World;var em=ps.emission;em.rateOverTime=1.4f;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(12,1,5);var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.45f;velocity.y=.025f;velocity.z=.08f;var color=ps.colorOverLifetime;color.enabled=true;var grad=new Gradient();grad.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.7f,.2f),new GradientAlphaKey(.7f,.7f),new GradientAlphaKey(0,1)});color.color=grad;ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=dustMat;ps.GetComponent<ParticleSystemRenderer>().shadowCastingMode=ShadowCastingMode.Off;}
  }
  static void Reviews(Transform p)
  {
   string[] names={"Desert_Overview","Desert_Gameplay","Desert_StreetReverse","Desert_H01_H02","Desert_H04_H10","Desert_Flora","Desert_Arch"};
   Vector3[] positions={new Vector3(-62,38,-62),new Vector3(-12,3.1f,-5),new Vector3(23,3,-4),new Vector3(-36,3,11),new Vector3(18,3,12),new Vector3(-46,1.8f,12),new Vector3(-5,8,23)};
   Vector3[] targets={new Vector3(4,5,8),new Vector3(8,2,4),new Vector3(-20,3,6),new Vector3(-22,4,29),new Vector3(27,3,29),new Vector3(-42,2.7f,21),new Vector3(15,23,89)};
   for(int i=0;i<names.Length;i++){var c=Group(names[i],p).gameObject.AddComponent<Camera>();c.transform.localPosition=positions[i];c.transform.LookAt(p.TransformPoint(targets[i]));c.enabled=false;c.clearFlags=CameraClearFlags.Skybox;c.nearClipPlane=.1f;c.farClipPlane=650;c.fieldOfView=i==0?64:60;c.useOcclusionCulling=false;c.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
  }
  static void Materials()
  {
   wood=Lit("Weathered wood",new Color(.48f,.38f,.26f));rust=Lit("Oxidised metal",new Color(.36f,.22f,.13f));dark=Lit("Dark aged fittings",new Color(.10f,.085f,.065f));olive=Lit("Olive foliage",new Color(.35f,.38f,.19f));cactus=Lit("Ribbed cactus",new Color(.22f,.30f,.115f));
   // Procedural grain and oxidisation textures are persisted rather than generated each Play Mode.
   foreach(var m in new[]{wood,rust,cactus}){string path=Root+"/Textures/"+m.name+".asset";var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(!tex){tex=new Texture2D(256,256,TextureFormat.RGB24,true);var colors=new Color[65536];for(int y=0;y<256;y++)for(int x=0;x<256;x++){float n=Mathf.PerlinNoise(x*(m==wood?.22f:.08f),y*(m==wood?.012f:.08f));float a=.55f+n*.65f;if(m==cactus)a*=.84f+.16f*Mathf.Cos(x*.3f);colors[y*256+x]=new Color(a,a,a);}tex.SetPixels(colors);tex.wrapMode=TextureWrapMode.Repeat;tex.Apply();AssetDatabase.CreateAsset(tex,path);}m.SetTexture("_BaseMap",tex);}
   var sh=Shader.Find("Desert/Layered Sandstone");if(!sh)throw new Exception("Desert surface shader not imported");
   sandstone=Lit("Layered red sandstone",new Color(.62f,.32f,.17f));sandstone.shader=sh;sandstone.SetFloat("_Mode",1);sandstone.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/InfinitaStudio - Mountains/Textures/mountain_03_diffuse.png"));
   sand=Lit("Desert sand and road",new Color(.68f,.50f,.30f));sand.shader=sh;sand.SetFloat("_Mode",0);
  }
  [MenuItem("Tools/Desert Outpost/Build Detailed Battlefield")]
  public static void Build()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before authoring");var active=SceneManager.GetActiveScene();string backup="Backups/Desert/"+DateTime.Now.ToString("yyyyMMdd_HHmmss");Directory.CreateDirectory(backup);if(active.path==ScenePath)EditorSceneManager.SaveScene(active,backup+"/BeforeDetailedBuild.unity",true);else if(File.Exists(ScenePath))File.Copy(ScenePath,backup+"/BeforeDetailedBuild.unity");
   foreach(string d in new[]{"Meshes","Materials","Textures","Lighting","Prefabs","Documentation/Review"})Directory.CreateDirectory(Root+"/"+d);AssetDatabase.Refresh();
   var scene=active.path==ScenePath?active:EditorSceneManager.OpenScene(ScenePath);var state=Random.state;
   try{Random.InitState(48271);converted.Clear();buildings.Clear();meshIndex=0;manifest.Clear();manifest.AppendLine("# Desert Outpost — actual asset manifest\n\nGenerated "+DateTime.Now.ToString("s")+". Original package assets preserved.\n");Materials();ImportGLB("Shack");ImportGLB("Watchtower");
    var old=scene.GetRootGameObjects().FirstOrDefault(x=>x.name=="Desert_Battlefield");if(old)UnityEngine.Object.DestroyImmediate(old);
    var root=Group("Desert_Battlefield");var origin=Group("Desert_LayoutOrigin",root);origin.localPosition=new Vector3(4.85f,0,0);
    Ground(Group("01_TerrainAndRoad",origin));Landscape(Group("07_Mountains_M01_M07",origin));Buildings(Group("02_Buildings_H01_H12",origin));Industry(Group("03_Industry_I01_I07",origin));Vegetation(Group("04_DesertTreesCactiAndDryFlora",origin));Lighting(Group("11_LightingAndDust",origin));Reviews(Group("12_ReviewCameras",origin));
    var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var board=battle.player.transform.parent;foreach(string name in new[]{"MegaKitHangar","SphereCityArena"}){var x=board.Find(name);if(x)x.gameObject.SetActive(false);}var stage=board.Find("BattleArena");if(stage){foreach(var r in stage.GetComponentsInChildren<Renderer>(true))r.enabled=false;foreach(var c in stage.GetComponentsInChildren<Collider>(true))c.enabled=false;}
    var sw=battle.GetComponent<BattleEnvironmentSwitcher>();sw.environments=new[]{new BattleEnvironmentSwitcher.EnvironmentOption{title="DESERT OUTPOST · RED CANYON",root=root.gameObject,ambient=new Color(.48f,.43f,.36f),background=new Color(.67f,.57f,.44f)}};sw.trainingRenderers=Array.Empty<MeshRenderer>();sw.Apply(0);if(sw.nextButton)sw.nextButton.gameObject.SetActive(false);
    var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();orbit.homeYaw=85;orbit.homePitch=8;orbit.homeDistance=9;orbit.minPitch=-85;orbit.maxPitch=89;orbit.maxDistance=80;orbit.ResetView(true);battle.battleCamera.farClipPlane=650;battle.battleCamera.clearFlags=CameraClearFlags.Skybox;battle.battleCamera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
    Polish(root);Finish(root);DesertCanyonUpgrade.Apply(root);DesertDensityExpansion.Apply(root);PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/Desert_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    var catalog=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);var definition=catalog.Find("desert-outpost");definition.displayName="DESERT OUTPOST · RED CANYON";definition.description="Đường cát giữa nhà gỗ–tôn bỏ hoang, xương rồng và vòm đá sa thạch.";EditorUtility.SetDirty(definition);EnvironmentTools.RegisterScenes();AssetDatabase.SaveAssets();
    manifest.AppendLine("\nRaft on the Desert not located in project: ribbed cactus and sparse desert trees generated as persistent meshes, using Flora-Form foliage. No downloaded assets. GLB source attribution requires the original downloaded model pages/licenses; local filenames alone do not identify creators. No performance target claimed.");File.WriteAllText(Root+"/Documentation/AssetManifest.md",manifest.ToString());Debug.Log("DESERT_DETAILED_READY | 12 named buildings, 16 ribbed cacti, 8 trees, 12 flora patches, geometric arch and canyon layers. Backup="+backup);
   }finally{Random.state=state;}
  }
  // A separate pass permits visual corrections without rebuilding all imported meshes.
  [MenuItem("Tools/Desert Outpost/Polish Current Scene")]
  public static void PolishCurrent()
  {var root=GameObject.Find("Desert_Battlefield").transform;Polish(root);Finish(root);PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/Desert_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
  static void Polish(Transform root)
  {
   if(root.Find("VisualPolishApplied"))return;converted.Clear();meshIndex=1000;Materials();Random.InitState(7192);
   var origin=root.Find("Desert_LayoutOrigin");var hg=origin.Find("02_Buildings_H01_H12");var h1=hg.Find("H01 Balcony timber house");
   var roof=h1.Find("Patched roof");if(roof){var b=BoundsOf(h1.GetChild(0).gameObject);roof.position=new Vector3(roof.position.x,b.max.y-.12f,roof.position.z);}
   // Move each complete named layout group, preserving its internal transforms.
   foreach(Transform house in hg){var v=house.localPosition;v.z*=.76f;house.localPosition=v;}
   // Populate the house fronts and support the upper wall rather than leaving an isolated roof.
   var h01extra=Group("H01 Weathered timber upper storey",h1);h01extra.localPosition=Vector3.zero;
   for(int j=0;j<21;j++){float x=-4.8f+j*.48f;Box("Vertical silvered facade plank",h01extra,new Vector3(x,2.15f,-3.9f),new Vector3(.45f,3.6f,.10f),wood);}
   for(int i=0;i<4;i++){var window=Box("Recessed boarded window",h01extra,new Vector3(-3.5f+i*2.3f,2.5f,-3.97f),new Vector3(1.25f,1.3f,.10f),dark);Beam("Cross boarding",h01extra,new Vector3(-4.1f+i*2.3f,2,-4.05f),new Vector3(-2.9f+i*2.3f,3,-4.05f),.12f,wood);}
   var mountain=origin.Find("07_Mountains_M01_M07");var replace=mountain.Cast<Transform>().Where(x=>x.name.StartsWith("M01")||x.name.StartsWith("M03")||x.name.StartsWith("M04")||x.name.StartsWith("M05")).ToArray();
   for(int i=0;i<replace.Length;i++){var old=replace[i];if(!old.gameObject.activeSelf)continue;Vector3 pos=old.localPosition;string name=old.name;UnityEngine.Object.DestroyImmediate(old.gameObject);Place("Assets/InfinitaStudio - Mountains/Prefabs/Fbx/mountain_canyon_0"+(i%5+1)+".prefab",name+" — canyon asset",mountain,pos,28+i%4*6,i*57,65,true);}
   // Open sky and softer warm stone separate the canyon from the foreground.
   sandstone.SetColor("_BaseColor",new Color(.56f,.40f,.28f));sand.SetColor("_BaseColor",new Color(.78f,.66f,.47f));
   var sky=RenderSettings.skybox;if(sky&&sky.HasProperty("_SkyTint")){sky.SetColor("_SkyTint",new Color(.68f,.74f,.77f));sky.SetFloat("_AtmosphereThickness",1.05f);EditorUtility.SetDirty(sky);}
   RenderSettings.fogDensity=.0035f;RenderSettings.fogColor=new Color(.72f,.66f,.55f);
   var industry=origin.Find("03_Industry_I01_I07");foreach(Transform g in industry){Vector3 v=g.localPosition;if(g.name.Contains("Supply cluster")||g.name.Contains("Utility pole"))v.z*=.92f;g.localPosition=v;}
   var flora=origin.Find("04_DesertTreesCactiAndDryFlora");
   foreach(Transform g in flora){if(g.name.StartsWith("G")){foreach(var renderer in g.GetComponentsInChildren<Renderer>()){renderer.shadowCastingMode=ShadowCastingMode.Off;foreach(var mat in renderer.sharedMaterials)if(mat){mat.SetColor("_BaseColor",new Color(1,.93f,.72f));EditorUtility.SetDirty(mat);}}}else if(g.name.StartsWith("T")||g.name.StartsWith("C")){var v=g.localPosition;v.z*=.83f;g.localPosition=v;}}
   var extra=Group("G13_G18 Golden roadside grass",flora);
   for(int i=0;i<140;i++){float x=Random.Range(-54,54),z=Random.Range(11,17)*(i%2==0?1:-1);Place("Assets/Flora-Form/Vegetation/GrassDry_"+(i%2==0?"A":"B")+".prefab","Golden grass tuft",extra,new Vector3(x,0,z),Random.Range(.45f,.85f),Random.Range(0,360),1.3f);}
   for(int i=0;i<35;i++){float x=Random.Range(-53,53),z=Random.Range(10,15)*(i%2==0?1:-1);var gravel=MeshObject("Roadside weathered stone",extra,AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Boulder0_1.asset"),sandstone);gravel.transform.localPosition=new Vector3(x,-.1f,z);gravel.transform.localScale=Vector3.one*Random.Range(.2f,.55f);}
   // Small roof hardware makes silhouettes differ without creating more runtime lights.
   foreach(string id in new[]{"H02 Sand plaster home","H09 Supply station office"}){var h=hg.Find(id);var b=BoundsOf(h.GetChild(0).gameObject);var tank=Group("Rooftop equipment",h);tank.position=new Vector3(b.center.x,b.max.y,b.center.z);Cylinder("Rooftop water drum",tank,Vector3.zero,.65f,1.25f,rust);Box("Aged solar module",tank,new Vector3(1.5f,.25f,0),new Vector3(1.5f,.08f,1.2f),dark).transform.localRotation=Quaternion.Euler(20,0,0);}
   // Keep the original low noise sand wisps; exposure remains driven by the shared volume.
   Group("VisualPolishApplied",root);EditorUtility.SetDirty(sand);EditorUtility.SetDirty(sandstone);
  }
  static void Finish(Transform root)
  {
   if(root.Find("FinalDetailApplied"))return;var origin=root.Find("Desert_LayoutOrigin");var hg=origin.Find("02_Buildings_H01_H12");var h1=hg.Find("H01 Balcony timber house");var facade=h1.Find("H01 Weathered timber upper storey");if(facade)facade.localPosition=new Vector3(0,0,-.5f);var roof=h1.Find("Patched roof");if(roof)roof.localScale*=1.3f;
   var sky=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Lighting/DesertSky.mat");sky.shader=Shader.Find("Desert/High Thin Clouds");RenderSettings.skybox=sky;EditorUtility.SetDirty(sky);
   var c=origin.Find("12_ReviewCameras/Desert_Gameplay");c.localPosition=new Vector3(-8,3.2f,-12);c.LookAt(origin.TransformPoint(new Vector3(6,2,6)));Group("FinalDetailApplied",root);
  }
  [MenuItem("Tools/Desert Outpost/Capture Review")]
  public static void Capture()
  {
   foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Desert_")))
   {var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Root+"/Documentation/Review/"+c.name+".png",tex.EncodeToPNG());}finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}}
   AssetDatabase.Refresh();var d=AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath).Find("desert-outpost");d.thumbnail=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Documentation/Review/Desert_Overview.png");EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();Debug.Log("DESERT_REVIEW_SAVED");
  }
 }
}

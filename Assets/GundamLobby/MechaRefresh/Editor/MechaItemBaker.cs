using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
namespace GundamLobby.Editor
{
 public static class MechaItemBaker
 {
  const string Root="Assets/GundamLobby/MechaRefresh";
  static Material gun,steel,silver,rubber,gold,cyan,red,purple,green;static Mesh beveled;
  static Material Mat(string id,Color c,float metal=.75f,bool glow=false)
  {
   string p=Root+"/Materials/"+id+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",glow?.65f:.46f);
   if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.18f);}EditorUtility.SetDirty(m);return m;
  }
  static Transform Piece(Transform p,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material,Vector3 euler=default)
  {
   var go=GameObject.CreatePrimitive(type);go.name=name;go.layer=29;go.transform.SetParent(p,false);go.transform.localPosition=pos;go.transform.localEulerAngles=euler;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;
  }
  static Transform Box(Transform p,string n,Vector3 pos,Vector3 size,Material m,Vector3 euler=default){var t=Piece(p,n,PrimitiveType.Cube,pos,size,m,euler);if(beveled)t.GetComponent<MeshFilter>().sharedMesh=beveled;return t;}
  static Mesh BevelMesh()
  {
   var verts=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();float edge=.43f;
   Action<Vector3,Vector3[]> face=(normal,points)=>{int start=verts.Count;if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)Array.Reverse(points);foreach(var point in points){verts.Add(point);normals.Add(normal.normalized);}for(int i=1;i<points.Length-1;i++){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}};
   for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2)
   {
    var n=Vector3.zero;n[axis]=sign;int u=(axis+1)%3,v=(axis+2)%3;var points=new Vector3[4];int[,] xy={{-1,-1},{1,-1},{1,1},{-1,1}};for(int i=0;i<4;i++){points[i][axis]=sign*.5f;points[i][u]=xy[i,0]*edge;points[i][v]=xy[i,1]*edge;}face(n,points);
   }
   for(int parallel=0;parallel<3;parallel++)for(int su=-1;su<=1;su+=2)for(int sv=-1;sv<=1;sv+=2)
   {
    int u=(parallel+1)%3,v=(parallel+2)%3;var n=Vector3.zero;n[u]=su;n[v]=sv;var points=new Vector3[4];for(int i=0;i<4;i++){points[i][parallel]=(i<2?-1:1)*edge;points[i][u]=su*(i==0||i==3?.5f:edge);points[i][v]=sv*(i==0||i==3?edge:.5f);}face(n,points);
   }
   for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)face(new Vector3(x,y,z),new[]{new Vector3(x*.5f,y*edge,z*edge),new Vector3(x*edge,y*.5f,z*edge),new Vector3(x*edge,y*edge,z*.5f)});
   var mesh=new Mesh{name="Machined beveled casing"};mesh.SetVertices(verts);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
  }
  static Transform Cylinder(Transform p,string n,Vector3 pos,float radius,float length,Material m,Vector3 euler=default)=>Piece(p,n,PrimitiveType.Cylinder,pos,new Vector3(radius*2,length*.5f,radius*2),m,euler);
  static void Bolts(Transform p,float w,float h,float z)
  {
   for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)Cylinder(p,"Titanium fastener",new Vector3(x*w,y*h,z),.035f,.025f,silver,new Vector3(90,0,0));
  }
  static void Vents(Transform p,float x,float y,float z,int count,Material m)
  {for(int i=0;i<count;i++)Box(p,"Machined vent",new Vector3(x,y+i*.09f,z),new Vector3(.35f,.024f,.024f),m);}
  static Transform Model(string id,Transform parent)
  {
   var root=new GameObject(id).transform;root.SetParent(parent,false);
   if(id=="fcore")
   {
    Cylinder(root,"Hexagonal housing",Vector3.zero,.64f,.32f,gun,new Vector3(90,0,0));Cylinder(root,"Silver collar",new(0,0,-.19f),.56f,.09f,silver,new(90,0,0));Cylinder(root,"Reactor face",new(0,0,-.26f),.46f,.07f,rubber,new(90,0,0));Cylinder(root,"Luminous crystal",new(0,0,-.32f),.22f,.13f,cyan,new(90,0,0));
    Box(root,"F stencil stem",new(-.055f,-.005f,-.405f),new(.027f,.19f,.018f),silver);Box(root,"F stencil upper",new(.003f,.075f,-.405f),new(.14f,.027f,.018f),silver);Box(root,"F stencil center",new(-.005f,.003f,-.405f),new(.11f,.027f,.018f),silver);
    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Cylinder(root,"Precision screw",new(Mathf.Cos(a)*.51f,Mathf.Sin(a)*.51f,-.25f),.016f,.025f,gold,new(90,0,0));}
    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Box(root,"Radial locking jaw",new(Mathf.Cos(a)*.49f,Mathf.Sin(a)*.49f,-.28f),new(.19f,.20f,.19f),steel,new(0,0,i*60));Box(root,"Calibration emitter",new(Mathf.Cos(a)*.37f,Mathf.Sin(a)*.37f,-.35f),new(.065f,.065f,.035f),cyan);}
    for(int i=0;i<3;i++)Box(root,"Frame fin",new(-.4f+i*.4f,.6f,.025f),new(.16f,.22f,.24f),gun);
   }
   else if(id=="xp_chip")
   {
    Box(root,"Carbon board",Vector3.zero,new(1.35f,.91f,.10f),rubber);Box(root,"Brushed casing",new(0,0,-.09f),new(1.23f,.78f,.10f),steel);Box(root,"Processor recess",new(0,0,-.16f),new(.84f,.53f,.08f),gun);Box(root,"Processor",new(0,0,-.23f),new(.45f,.39f,.11f),silver);Box(root,"Optical memory",new(0,0,-.30f),new(.27f,.22f,.035f),cyan);
    for(int i=0;i<10;i++){float x=-.55f+i*.12f;Box(root,"Gold connector",new(x,-.49f,0),new(.07f,.18f,.065f),gold);Box(root,"Data trace",new(x,.34f,-.165f),new(.025f,.12f,.025f),gold);}
    Vents(root,-.48f,-.15f,-.18f,4,silver);Bolts(root,.55f,.32f,-.18f);
   }
   else if(id=="credit_cache"||id=="supply_crate")
   {
    bool cache=id=="credit_cache";var accent=cache?gold:purple;Box(root,"Reinforced hard case",Vector3.zero,new(1.55f,.95f,.74f),gun);Box(root,"Raised lid",new(0,.43f,0),new(1.65f,.16f,.81f),steel);Box(root,"Front armor",new(0,0,-.39f),new(1.33f,.67f,.08f),steel);Box(root,"Inset face",new(0,0,-.45f),new(1.07f,.47f,.05f),rubber);
    for(int x=-1;x<=1;x+=2){Box(root,"Case rib",new(x*.56f,0,-.47f),new(.10f,.70f,.09f),silver);Box(root,"Lock",new(x*.55f,.31f,-.51f),new(.20f,.18f,.11f),accent);Box(root,"Corner bumper",new(x*.76f,-.39f,0),new(.18f,.2f,.84f),rubber);}
    Box(root,"Handle",new(0,.62f,0),new(.64f,.12f,.18f),rubber);Box(root,"Status display",new(0,.04f,-.50f),new(.34f,.15f,.025f),accent);Vents(root,0,-.25f,-.49f,2,silver);Bolts(root,.65f,.25f,-.51f);
    if(cache)for(int i=0;i<4;i++)Box(root,"Treasury marker",new(-.24f+i*.16f,-.14f,-.51f),new(.07f,.08f,.025f),gold);
   }
   else if(id=="assault_module")
   {
    Box(root,"Assault housing",Vector3.zero,new(1.15f,.86f,.51f),gun);Box(root,"Ceramic front",new(0,0,-.28f),new(.96f,.70f,.12f),steel);Cylinder(root,"Accelerator socket",new(0,0,-.40f),.30f,.18f,rubber,new(90,0,0));Cylinder(root,"Red accelerator",new(0,0,-.51f),.18f,.06f,red,new(90,0,0));
    for(int x=-1;x<=1;x+=2){for(int i=0;i<5;i++)Box(root,"Cooling fins",new(x*.61f,-.31f+i*.15f,.02f),new(.22f,.06f,.64f),steel);Cylinder(root,"Power connector",new(x*.43f,.47f,0),.08f,.20f,gold);}
    Bolts(root,.39f,.26f,-.37f);Box(root,"Signal strip",new(0,-.29f,-.36f),new(.30f,.045f,.025f),red);
   }
   else if(id=="bulwark_module")
   {
    Box(root,"Composite armor core",Vector3.zero,new(.92f,1.34f,.29f),gun);Box(root,"Central armor ridge",new(0,.04f,-.24f),new(.34f,1.19f,.19f),silver);
    for(int x=-1;x<=1;x+=2){Box(root,"Angled titanium armor",new(x*.34f,.07f,-.12f),new(.37f,1.17f,.19f),steel,new(0,x*18,0));Box(root,"Armor shoulder",new(x*.43f,.55f,-.12f),new(.27f,.23f,.35f),gun);Box(root,"Blue inset",new(x*.26f,.07f,-.28f),new(.035f,.59f,.035f),cyan);}
    Bolts(root,.34f,.47f,-.27f);Box(root,"Lower bumper",new(0,-.65f,-.02f),new(.66f,.17f,.38f),rubber);
   }
   else if(id=="thruster_module")
   {
    Box(root,"Thruster linkage",Vector3.zero,new(1.35f,.26f,.40f),gun);
    for(int x=-1;x<=1;x+=2)
    {
     float xx=x*.43f;Cylinder(root,"Engine shroud",new(xx,0,0),.31f,.95f,steel,new(90,0,0));Cylinder(root,"Engine collar",new(xx,0,-.46f),.35f,.15f,gun,new(90,0,0));Cylinder(root,"Nozzle",new(xx,0,-.56f),.26f,.12f,rubber,new(90,0,0));Cylinder(root,"Core",new(xx,0,-.63f),.17f,.03f,cyan,new(90,0,0));
     for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Box(root,"Nozzle vane",new(xx+Mathf.Cos(a)*.22f,Mathf.Sin(a)*.22f,-.65f),new(.065f,.08f,.07f),silver,new(0,0,i*45));}Box(root,"Vector actuator",new(xx,.38f,.1f),new(.24f,.17f,.66f),gun);Box(root,"Vector warning",new(xx,.47f,-.12f),new(.13f,.025f,.23f),gold);
    }
   }
   else if(id=="core_case")
   {
    Box(root,"Calibration cassette",Vector3.zero,new(1.65f,.78f,.72f),gun);Box(root,"Cassette lid",new(0,.39f,.02f),new(1.75f,.14f,.79f),steel);
    for(int i=0;i<5;i++){float x=-.62f+i*.31f;Cylinder(root,"Core cartridge",new(x,0,-.40f),.135f,.16f,silver,new(90,0,0));Cylinder(root,"Core seal",new(x,0,-.50f),.095f,.045f,cyan,new(90,0,0));Box(root,"Top latch",new(x,.33f,-.33f),new(.10f,.18f,.15f),gold);}
    Box(root,"Grip",new(0,.57f,0),new(.70f,.13f,.15f),rubber);Vents(root,0,-.25f,-.39f,2,steel);Bolts(root,.71f,.26f,-.41f);
   }
   else if(id=="data_archive")
   {
    Box(root,"Archive enclosure",Vector3.zero,new(1.20f,1.31f,.67f),gun);for(int x=-1;x<=1;x+=2)Box(root,"Steel rack rail",new(x*.55f,0,-.36f),new(.14f,1.22f,.13f),silver);
    for(int i=0;i<3;i++){float y=-.43f+i*.43f;Box(root,"Data cartridge",new(0,y,-.37f),new(.88f,.31f,.16f),steel);Box(root,"Status aperture",new(-.12f,y,-.47f),new(.43f,.10f,.025f),cyan);Box(root,"Pull tab",new(.31f,y,-.49f),new(.13f,.19f,.07f),rubber);for(int p=0;p<4;p++)Box(root,"Bus contact",new(-.36f+p*.13f,y-.11f,-.47f),new(.07f,.045f,.025f),gold);}
    Box(root,"Rack top",new(0,.68f,0),new(1.29f,.14f,.73f),steel);Bolts(root,.49f,.56f,-.44f);
   }
   else if(id=="credit_vault"||id=="frontline_crate")
   {
    bool vault=id=="credit_vault";var accent=vault?gold:red;Box(root,"Armored vault",Vector3.zero,new(1.72f,1.14f,.94f),gun);Box(root,"Lid",new(0,.58f,.01f),new(1.85f,.17f,1.02f),steel);Box(root,"Front plate",new(0,0,-.49f),new(1.47f,.91f,.13f),steel);Box(root,"Recess",new(0,0,-.57f),new(1.18f,.66f,.06f),rubber);
    for(int x=-1;x<=1;x+=2){Box(root,"Corner pillar",new(x*.75f,0,-.52f),new(.17f,1.05f,.17f),gold);Box(root,"Latch",new(x*.61f,.46f,-.56f),new(.22f,.28f,.19f),accent);Box(root,"Foot",new(x*.74f,-.57f,.02f),new(.32f,.19f,1.0f),rubber);}
    if(vault){Cylinder(root,"Combination lock",new(0,0,-.65f),.27f,.15f,gold,new(90,0,0));Cylinder(root,"Vault hub",new(0,0,-.75f),.15f,.07f,gun,new(90,0,0));for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Box(root,"Combination tick",new(Mathf.Cos(a)*.19f,Mathf.Sin(a)*.19f,-.75f),new(.035f,.06f,.02f),silver,new(0,0,i*45));}}
    else{Box(root,"Crossbrace left",new(0,0,-.64f),new(1.12f,.11f,.08f),silver,new(0,0,29));Box(root,"Crossbrace right",new(0,0,-.64f),new(1.12f,.11f,.08f),silver,new(0,0,-29));Box(root,"Supply seal",new(0,0,-.70f),new(.25f,.25f,.045f),red);}
    Box(root,"Carry handle",new(0,.78f,0),new(.69f,.14f,.21f),rubber);Bolts(root,.61f,.35f,-.64f);
   }
   else if(id=="breaker_module")
   {
    Cylinder(root,"Breach motor",Vector3.zero,.46f,.67f,gun,new(90,0,0));Cylinder(root,"Torque collar",new(0,0,-.35f),.49f,.16f,silver,new(90,0,0));Cylinder(root,"Breach emitter",new(0,0,-.48f),.24f,.16f,red,new(90,0,0));
    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Box(root,"Tungsten breach tooth",new(Mathf.Cos(a)*.35f,Mathf.Sin(a)*.35f,-.58f),new(.17f,.23f,.48f),steel,new(0,0,i*60));Box(root,"Copper drive coil",new(Mathf.Cos(a)*.41f,Mathf.Sin(a)*.41f,.03f),new(.14f,.14f,.49f),gold,new(0,0,i*60));}
    Box(root,"Mount",new(0,-.56f,.05f),new(.72f,.22f,.61f),gun);Bolts(root,.28f,.27f,-.46f);
   }
   else if(id=="defense_module")
   {
    Cylinder(root,"Field matrix",Vector3.zero,.58f,.33f,gun,new(90,0,0));Cylinder(root,"Silver dish",new(0,0,-.21f),.49f,.12f,silver,new(90,0,0));Cylinder(root,"Blue field face",new(0,0,-.30f),.38f,.09f,cyan,new(90,0,0));
    Box(root,"Shield spine",new(0,0,-.39f),new(.12f,.91f,.14f),steel);Box(root,"Shield crossbar",new(0,.10f,-.39f),new(.77f,.13f,.14f),steel);
    for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f+.785f;Box(root,"Emitter block",new(Mathf.Cos(a)*.47f,Mathf.Sin(a)*.47f,-.18f),new(.21f,.21f,.25f),gun,new(0,0,45+i*90));Cylinder(root,"Status diode",new(Mathf.Cos(a)*.47f,Mathf.Sin(a)*.47f,-.34f),.035f,.025f,green,new(90,0,0));}
   }
   else if(id=="precision_module")
   {
    Cylinder(root,"Sensor barrel",Vector3.zero,.34f,1.12f,gun,new(90,0,0));Cylinder(root,"Objective collar",new(0,0,-.60f),.42f,.18f,steel,new(90,0,0));Cylinder(root,"Lens seat",new(0,0,-.71f),.34f,.08f,rubber,new(90,0,0));Cylinder(root,"Optical lens",new(0,0,-.77f),.25f,.035f,gold,new(90,0,0));
    Box(root,"Stabilizer rail",new(0,-.41f,.03f),new(1.02f,.18f,1.23f),steel);for(int i=0;i<5;i++)Box(root,"Heat sink",new(0,.35f,-.31f+i*.17f),new(.60f,.13f,.055f),silver);Box(root,"Rangefinder",new(.45f,.09f,.02f),new(.21f,.33f,.68f),gun);Box(root,"Laser aperture",new(.45f,.09f,-.34f),new(.09f,.09f,.025f),red);
   }
   else if(id=="balanced_module")
   {
    Box(root,"Central processor",Vector3.zero,new(.67f,.67f,.57f),gun,new(0,0,45));Box(root,"Neural matrix",new(0,0,-.36f),new(.34f,.34f,.12f),cyan,new(0,0,45));
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Box(root,"Gyro frame",new(Mathf.Cos(a)*.58f,Mathf.Sin(a)*.58f,0),new(.38f,.13f,.25f),silver,new(0,0,90+i*45));Box(root,"Control node",new(Mathf.Cos(a)*.54f,Mathf.Sin(a)*.54f,-.18f),new(.10f,.10f,.10f),i%2==0?gold:cyan);}
    for(int x=-1;x<=1;x+=2)Box(root,"Gyro pivot",new(x*.70f,0,0),new(.23f,.32f,.39f),gun);Box(root,"Base connector",new(0,-.74f,.03f),new(.61f,.20f,.45f),steel);
   }
   else if(id=="repair_gel"||id=="overdrive_amp")
   {
    bool heal=id=="repair_gel";var liquid=heal?green:red;int count=heal?1:2;
    if(!heal)Box(root,"Ampoule carrier",new(0,0,.07f),new(1.10f,.88f,.34f),gun);
    for(int i=0;i<count;i++)
    {
     float x=count==1?0:-.30f+i*.60f;Cylinder(root,"Sealed medical vial",new(x,0,-.10f),heal?.29f:.22f,1.12f,liquid);Cylinder(root,"Top cap",new(x,.59f,-.10f),heal?.33f:.26f,.17f,silver);Cylinder(root,"Bottom cap",new(x,-.59f,-.10f),heal?.33f:.26f,.17f,gun);
     for(int side=-1;side<=1;side+=2)Box(root,"Vial guard",new(x+side*(heal?.27f:.20f),0,-.18f),new(.075f,.93f,.15f),steel);
     Box(root,"Label",new(x,0,heal?-.40f:-.33f),new(heal?.35f:.27f,.31f,.03f),rubber);
     if(heal){Box(root,"Medical cross vertical",new(x,0,-.43f),new(.07f,.22f,.025f),silver);Box(root,"Medical cross horizontal",new(x,0,-.43f),new(.23f,.07f,.025f),silver);}
     else{Box(root,"Overdrive slash",new(x,0,-.36f),new(.065f,.23f,.025f),gold,new(0,0,-25));Box(root,"Seal",new(x,.43f,-.33f),new(.17f,.07f,.025f),gold);}
    }
   }
   return root;
  }
  public static string Build(bool onlyMissing=true)
  {
   foreach(string dir in new[]{"Materials","Models","Icons","Resources","Meshes"})Directory.CreateDirectory(Root+"/"+dir);
   beveled=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/BeveledBox.asset");if(!beveled){beveled=BevelMesh();AssetDatabase.CreateAsset(beveled,Root+"/Meshes/BeveledBox.asset");}
   gun=Mat("Gunmetal",new(.07f,.08f,.095f));steel=Mat("Titanium",new(.28f,.31f,.35f));silver=Mat("Brushed Aluminum",new(.57f,.62f,.68f));rubber=Mat("Carbon",new(.018f,.027f,.035f),.15f);gold=Mat("Gold",new(.87f,.58f,.13f));cyan=Mat("Cyan",new(.035f,.44f,.57f),.35f,true);red=Mat("Red",new(.63f,.075f,.045f),.35f,true);purple=Mat("Purple",new(.32f,.12f,.52f),.35f,true);green=Mat("Green",new(.13f,.7f,.4f),.35f,true);
   var rig=new GameObject("Temporary Item Render Rig");rig.transform.position=new Vector3(3000,3000,3000);
   var result=new List<MechaItemVisuals.Entry>();
   try
   {
    var camGo=new GameObject("Item Camera");camGo.transform.SetParent(rig.transform,false);var cam=camGo.AddComponent<Camera>();cam.enabled=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.cullingMask=1<<29;cam.fieldOfView=30;cam.nearClipPlane=.03f;cam.farClipPlane=30;cam.allowHDR=false;cam.allowMSAA=false;
    var cameraData=cam.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=false;cameraData.antialiasing=AntialiasingMode.None;cameraData.requiresColorTexture=false;cameraData.requiresDepthTexture=false;
    for(int i=0;i<3;i++){var go=new GameObject("Studio Light "+i);go.transform.SetParent(rig.transform,false);var l=go.AddComponent<Light>();l.type=LightType.Directional;l.cullingMask=1<<29;l.color=i==0?new Color(.95f,.97f,1):i==1?new Color(1,.92f,.82f):new Color(.45f,.70f,1);l.intensity=i==0?1.8f:i==1?.75f:1.2f;go.transform.localRotation=Quaternion.Euler(i==0?38:12,i==0?-28:i==1?145:65,0);}
    foreach(var item in GameItems.All)
    {
     string existingPath=Root+"/Icons/"+item.id+".png",existingPrefab=Root+"/Models/"+item.id+".prefab";
     var existingIcon=AssetDatabase.LoadAssetAtPath<Sprite>(existingPath);var existingModel=AssetDatabase.LoadAssetAtPath<GameObject>(existingPrefab);
     if(onlyMissing&&existingIcon&&existingModel){result.Add(new MechaItemVisuals.Entry{id=item.id,icon=existingIcon,prefab=existingModel});continue;}
     var model=Model(item.id,rig.transform);var renderers=model.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new Exception("No model: "+item.id);
     var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
     float distance=Mathf.Max(bounds.size.y,bounds.size.x/1.3333f)*.5f/Mathf.Tan(15*Mathf.Deg2Rad)*1.26f+bounds.extents.z;
     cam.transform.position=bounds.center+new Vector3(1.15f,.72f,-2.8f).normalized*distance;cam.transform.LookAt(bounds.center);cam.aspect=4f/3;
     var rt=new RenderTexture(2048,1536,24,RenderTextureFormat.ARGB32){antiAliasing=1};rt.Create();var previous=RenderTexture.active;cam.targetTexture=rt;var image=new Texture2D(2048,1536,TextureFormat.RGBA32,false);
     try{cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,2048,1536),0,0);image.Apply();File.WriteAllBytes(Root+"/Icons/"+item.id+".png",image.EncodeToPNG());}finally{RenderTexture.active=previous;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
     string prefabPath=Root+"/Models/"+item.id+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(model.gameObject,prefabPath);UnityEngine.Object.DestroyImmediate(model.gameObject);
     string path=Root+"/Icons/"+item.id+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();
     result.Add(new MechaItemVisuals.Entry{id=item.id,prefab=prefab,icon=AssetDatabase.LoadAssetAtPath<Sprite>(path)});GC.Collect();
    }
    string catalogPath=Root+"/Resources/MechaItemVisuals.asset";var catalog=AssetDatabase.LoadAssetAtPath<MechaItemVisuals>(catalogPath);if(!catalog){catalog=ScriptableObject.CreateInstance<MechaItemVisuals>();AssetDatabase.CreateAsset(catalog,catalogPath);}catalog.items=result.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "Rendered "+result.Count+" mechanical item models and high resolution icons.";
   }
   finally{UnityEngine.Object.DestroyImmediate(rig);}
  }
 }
}
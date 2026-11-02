using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DesertOutpost.Editor
{
 public static class DesertCanyonUpgrade
 {
  const string Root=DesertOutpostBuilder.Root;
  static float N(float x,float y)=>Mathf.PerlinNoise(x+73.41f,y+19.28f);
  static float Top(float x)
  {
   // A broad asymmetric rock mass, high on the left, weathered down on the right.
   float h=52+12*Mathf.Exp(-Mathf.Pow((x+32)/19,2))-9*Mathf.SmoothStep(0,1,(x-15)/45);
   float edge=Mathf.SmoothStep(0,1,Mathf.Min((x+59)/15,(60-x)/16));
   return (h+N(x*.09f,1)*2+Mathf.Sin(x*.37f)*.4f)*Mathf.Pow(Mathf.Max(.001f,edge),.42f);
  }
  static float Floor(float x)
  {
   float u=(x-7)/26;if(Mathf.Abs(u)>=1)return -1;
   float profile=Mathf.Pow(Mathf.Max(0,1-Mathf.Pow(Mathf.Abs(u),3)),.60f);
   return profile*(29*(1+.10f*u)+.65f*Mathf.Sin(x*.27f)+.30f*Mathf.Sin(x*.63f));
  }
  static Vector3 Face(float x,float y,float t,bool back)
  {
   float broad=N(x*.035f,y*.035f)*7;
   float layers=(N(x*.10f,y*.23f)-.5f)*1.15f+(N(x*.36f,y*.42f)-.5f)*.32f;
   // Erosion is horizontal and undercuts the ledges; it does not inflate a circular tube.
   float toe=7*Mathf.Exp(-y/10),incision=Mathf.Pow(N(x*.13f,y*.07f),3)*.9f;
   float buttress=(19*Mathf.Exp(-Mathf.Pow((x+32)/12,2))+12*Mathf.Exp(-Mathf.Pow((x-44)/11,2)))*(.85f+.15f*N(x*.05f,y*.04f));
   float bevel=(Floor(x)>0?2.5f*Mathf.Exp(-Mathf.Max(0,y-Floor(x))/1.8f):0)+1.2f*Mathf.Exp(-Mathf.Max(0,Top(x)-y)/1.4f);
   float depth=12+broad+layers+toe-incision+buttress-bevel;
   float z=back?depth*.85f:-depth;
   return new Vector3(x+(N(x*.06f,y*.08f)-.5f)*.45f,y,z);
  }
  static Mesh Mountain()
  {
   const int cols=192,rows=100;int stride=rows+1,faceCount=(cols+1)*stride;
   var v=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
   for(int side=0;side<2;side++)for(int i=0;i<=cols;i++)
   {float x=Mathf.Lerp(-59,60,i/(float)cols),lower=Floor(x),upper=Top(x);for(int j=0;j<=rows;j++){float t=j/(float)rows;float y=Mathf.Lerp(lower,upper,t);v.Add(Face(x,y,t,side==1));uv.Add(new Vector2(x*.03f,y*.03f));}}
   for(int side=0;side<2;side++)for(int i=0;i<cols;i++)for(int j=0;j<rows;j++)
   {int a=side*faceCount+i*stride+j,b=a+stride; // front should face toward -Z
    if(side==0)triangles.AddRange(new[]{a,a+1,b,b,a+1,b+1});else triangles.AddRange(new[]{a,b,a+1,b,b+1,a+1});}
   for(int i=0;i<cols;i++)
   {int a=i*stride,b=(i+1)*stride;
    // Lower surface is the underside of the natural opening; upper surface caps the plateau.
    triangles.AddRange(new[]{a,b,a+faceCount,b,b+faceCount,a+faceCount});
    a+=rows;b+=rows;triangles.AddRange(new[]{a,a+faceCount,b,b,a+faceCount,b+faceCount});}
   for(int j=0;j<rows;j++)
   {int a=j,b=cols*stride+j;triangles.AddRange(new[]{a,a+faceCount,a+1,a+1,a+faceCount,a+faceCount+1,b,b+1,b+faceCount,b+1,b+faceCount+1,b+faceCount});}
   var mesh=new Mesh{name="IntegratedSandstoneMassif",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.RecalculateTangents();
   string path=Root+"/Meshes/IntegratedSandstoneMassif.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){existing.Clear();existing.name="IntegratedSandstoneMassif";existing.indexFormat=IndexFormat.UInt32;existing.vertices=mesh.vertices;existing.uv=mesh.uv;existing.triangles=mesh.triangles;existing.normals=mesh.normals;existing.tangents=mesh.tangents;existing.RecalculateBounds();EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(mesh);return existing;}AssetDatabase.CreateAsset(mesh,path);return mesh;
  }
  static Transform Group(string n,Transform p){var o=new GameObject(n);o.transform.SetParent(p,false);return o.transform;}
  public static void Apply(Transform environment)
  {
   var origin=environment.Find("Desert_LayoutOrigin");var mountains=origin.Find("07_Mountains_M01_M07");
   foreach(var t in mountains.Cast<Transform>().Where(t=>t.name.StartsWith("M02")||t.name.StartsWith("M02R")).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
   var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Eroded canyon massif.mat");
   if(!material){material=new Material(Shader.Find("Desert/Eroded Sandstone"));AssetDatabase.CreateAsset(material,Root+"/Materials/Eroded canyon massif.mat");}
   material.shader=Shader.Find("Desert/Eroded Sandstone");material.SetColor("_BaseColor",Color.white);material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Sandstone_Weathered_Albedo.png"));material.SetFloat("_WorldScale",.22f);material.SetFloat("_Relief",.025f);EditorUtility.SetDirty(material);
   var massif=Group("M02R Integrated eroded sandstone canyon",mountains);massif.localPosition=new Vector3(12,0,81);massif.localRotation=Quaternion.Euler(0,-8,0);
   var obj=Group("Broad plateau with asymmetric natural opening",massif).gameObject;obj.AddComponent<MeshFilter>().sharedMesh=Mountain();obj.AddComponent<MeshRenderer>().sharedMaterial=material;obj.AddComponent<MeshCollider>().sharedMesh=obj.GetComponent<MeshFilter>().sharedMesh;obj.isStatic=true;obj.layer=2;
   // Remove any close canyon asset that would plug the new opening from behind.
   foreach(Transform t in mountains)if(t!=massif&&t.name.StartsWith("M01")&&t.localPosition.x>-48&&t.localPosition.x<75)t.gameObject.SetActive(false);
   // Low talus uses package cliff fragments instead of decorative torus-shaped feet.
   for(int i=0;i<4;i++)
   {string path="Assets/InfinitaStudio - Mountains/Prefabs/Fbx/mountain_canyon_0"+(i%3+1)+".prefab";var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)continue;
    var pivot=Group("Eroded shoulder "+i,massif);var o=(GameObject)PrefabUtility.InstantiatePrefab(asset,pivot);var rs=o.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);o.transform.localScale*=new[]{14f,20f,12f,17f}[i]/b.size.y;b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);o.transform.position+=new Vector3(pivot.position.x-b.center.x,-b.min.y,pivot.position.z-b.center.z);pivot.localPosition=new Vector3(i<2?-49:52,-1,i%2==0?-11:8);foreach(var r in rs){r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();r.gameObject.isStatic=true;r.gameObject.layer=2;}}
   HarmonizeRockMaterials(environment);
   var arch=origin.Find("12_ReviewCameras/Desert_Arch");if(arch){arch.localPosition=new Vector3(4,10,3);arch.LookAt(origin.TransformPoint(new Vector3(12,30,81)));}
  }
  public static void HarmonizeRockMaterials(Transform environment)
  {
   string texturePath=Root+"/Textures/Sandstone_Weathered_Albedo.png";
   var importer=AssetImporter.GetAtPath(texturePath) as TextureImporter;
   if(importer&&(importer.wrapMode!=TextureWrapMode.Repeat||importer.anisoLevel!=8||!importer.mipmapEnabled)){importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=8;importer.mipmapEnabled=true;importer.sRGBTexture=true;importer.maxTextureSize=2048;importer.SaveAndReimport();}
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);if(!texture)throw new Exception("Missing weathered sandstone albedo");
   var materials=new Material[3];
   for(int i=0;i<3;i++){string path=Root+"/Materials/Sandstone_Unified_"+i+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Desert/Eroded Sandstone"));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("Desert/Eroded Sandstone");m.SetTexture("_BaseMap",texture);m.SetColor("_BaseColor",i==0?Color.white:i==1?new Color(.97f,.97f,.96f):new Color(.95f,.97f,.98f));m.SetFloat("_WorldScale",.22f);m.SetFloat("_Relief",i==0?.025f:i==1?.018f:.008f);m.enableInstancing=true;EditorUtility.SetDirty(m);materials[i]=m;}
   var mountains=environment.Find("Desert_LayoutOrigin/07_Mountains_M01_M07");
   foreach(var renderer in mountains.GetComponentsInChildren<Renderer>(true))
   {var owner=renderer.transform;while(owner.parent&&owner.parent!=mountains)owner=owner.parent;if(owner.name.StartsWith("M06"))continue;int index=owner.name.StartsWith("M07")?2:owner.name.StartsWith("M02R")?0:1;renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>materials[index]).ToArray();}
   foreach(var renderer in environment.GetComponentsInChildren<Renderer>(true))
   {var mats=renderer.sharedMaterials;bool changed=false;for(int i=0;i<mats.Length;i++)if(mats[i]&&mats[i].shader&&mats[i].shader.name=="Desert/Layered Sandstone"&&mats[i].HasProperty("_Mode")&&mats[i].GetFloat("_Mode")>.5f){mats[i]=materials[0];changed=true;}if(changed)renderer.sharedMaterials=mats;}
   var origin=environment.Find("Desert_LayoutOrigin");var cameras=origin.Find("12_ReviewCameras");var detail=cameras.Find("Desert_RockDetail");if(!detail){detail=Group("Desert_RockDetail",cameras);detail.gameObject.AddComponent<Camera>();detail.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing=true;}
   var camera=detail.GetComponent<Camera>();camera.enabled=false;camera.clearFlags=CameraClearFlags.Skybox;camera.nearClipPlane=.1f;camera.farClipPlane=650;camera.fieldOfView=55;detail.localPosition=new Vector3(-12,12,22);detail.LookAt(origin.TransformPoint(new Vector3(-15,18,43)));
  }
  [MenuItem("Tools/Desert Outpost/Replace Artificial Arch With Canyon")]
  public static void UpdateScene()
  {
   if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before updating canyon");var scene=SceneManager.GetActiveScene();if(scene.path!=DesertOutpostBuilder.ScenePath)throw new Exception("Open Desert Battle scene first");
   string backup="Backups/Desert/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_BeforeCanyon";Directory.CreateDirectory(backup);EditorSceneManager.SaveScene(scene,backup+"/Desert_Battle.unity",true);
   var root=GameObject.Find("Desert_Battlefield").transform;Apply(root);PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/Desert_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();DesertOutpostBuilder.Capture();Debug.Log("DESERT_CANYON_REPLACED: broad 119m sandstone massif; asymmetric eroded opening; artificial ring removed");
  }
 }
}

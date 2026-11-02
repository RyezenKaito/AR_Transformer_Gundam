using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Random=UnityEngine.Random;

namespace DesertOutpost.Editor
{
 public static class DesertDensityExpansion
 {
  const string Root=DesertOutpostBuilder.Root;
  static Transform Group(string n,Transform p){var o=new GameObject(n);o.transform.SetParent(p,false);return o.transform;}
  static Bounds BoundsOf(Transform t){var rs=t.GetComponentsInChildren<Renderer>();var b=rs.Length>0?rs[0].bounds:new Bounds(t.position,Vector3.zero);foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  static Transform Clone(Transform source,Transform parent,string name,Vector3 pos,float scale,float yaw)
  {var copy=UnityEngine.Object.Instantiate(source.gameObject,parent).transform;copy.name=name;copy.localPosition=pos;copy.localScale=source.localScale*scale;copy.localRotation=Quaternion.Euler(0,yaw,0)*source.localRotation;return copy;}
  class Patch {public Material material;public List<CombineInstance> instances=new List<CombineInstance>();}
  static Mesh SaveMesh(Mesh m,string name)
  {string path=Root+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);m.name=name;if(old){old.Clear();old.name=name;old.indexFormat=IndexFormat.UInt32;old.vertices=m.vertices;old.normals=m.normals;old.uv=m.uv;old.triangles=m.triangles;old.tangents=m.tangents;old.RecalculateBounds();EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;}AssetDatabase.CreateAsset(m,path);return m;}
  public static void Apply(Transform environment)
  {
   var state=Random.state;
   try
   {
    Random.InitState(58391);var origin=environment.Find("Desert_LayoutOrigin");var old=origin.Find("13_DensityExpansion");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
    var root=Group("13_DensityExpansion",origin);var houses=Group("Additional dwellings H13-H22",root);var trees=Group("Additional desert trees T09-T32",root);var cacti=Group("Additional cacti C17-C28",root);var grass=Group("Golden grass banks — combined patches",root);
    var originalHouses=origin.Find("02_Buildings_H01_H12");
    string[] templates={"H06 Old shack","H05 Eastern desert dwelling","H07 Abandoned supply trader","H04 Machine repair shed","H09 Supply station office","H06 Old shack","H05 Eastern desert dwelling","H08 Boarded timber dwelling","H04 Machine repair shed","H10 Damaged supply warehouse"};
    // H07's exact scene name is resolved by ID, not by a translated display title.
    Vector3[] positions={new Vector3(-44,0,40),new Vector3(-24,0,37),new Vector3(-4,0,39),new Vector3(16,0,40),new Vector3(36,0,42),new Vector3(-55,0,-28),new Vector3(-25,0,-43),new Vector3(-5,0,-43),new Vector3(15,0,-43),new Vector3(55,0,-35)};
    for(int i=0;i<positions.Length;i++)
    {string id=templates[i].Substring(0,3);var source=originalHouses.Cast<Transform>().First(t=>t.name.StartsWith(id+" "));Clone(source,houses,"H"+(13+i)+" Additional desert dwelling",positions[i],i%3==0?.86f:.94f,(i<5?180:0)+(i%3-1)*7);}
    var vegetation=origin.Find("04_DesertTreesCactiAndDryFlora");var treeSources=vegetation.Cast<Transform>().Where(t=>t.name.StartsWith("T")).ToArray();var cactusSources=vegetation.Cast<Transform>().Where(t=>t.name.StartsWith("C")).ToArray();
    var obstacles=new List<Bounds>();foreach(var h in originalHouses.Cast<Transform>().Concat(houses.Cast<Transform>()))if(h.name.Length>2&&h.name[0]=='H'&&char.IsDigit(h.name[1])){var b=BoundsOf(h);b.Expand(.7f);obstacles.Add(b);}
    foreach(var b in origin.Find("03_Industry_I01_I07").Cast<Transform>()){var bounds=BoundsOf(b);if(bounds.size.x<16&&bounds.size.z<16){bounds.Expand(.4f);obstacles.Add(bounds);}}
    Func<Vector3,float,bool> clear=(pos,padding)=>
    {if(Mathf.Abs(pos.x)<28&&Mathf.Abs(pos.z)<11+padding)return false;var world=origin.TransformPoint(pos);foreach(var box in obstacles){var b=box;b.Expand(padding*2);if(world.x>b.min.x&&world.x<b.max.x&&world.z>b.min.z&&world.z<b.max.z)return false;}return true;};
    int made=0;
    for(int attempt=0;attempt<500&&made<24;attempt++)
    {float x=Random.Range(-61f,62f),z=attempt<90?Random.Range(15f,19f)*(attempt%2==0?1:-1):Random.Range(28f,49f)*(attempt%2==0?1:-1);var p=new Vector3(x,0,z);if(!clear(p,1.4f))continue;
     bool dead=made%3==0;var source=dead?treeSources[4]:treeSources[new[]{0,1,2,3,5,7}[made%6]];var tree=Clone(source,trees,"T"+(9+made).ToString("00")+(dead?" Dry branching tree":" Sparse desert tree"),p,Random.Range(.8f,1.15f),Random.Range(0,360));
     foreach(var r in tree.GetComponentsInChildren<Renderer>())r.shadowCastingMode=r.name.Contains("foliage")?ShadowCastingMode.Off:r.shadowCastingMode;
     // Avoid identical branching silhouettes facing the same direction.
     var b=BoundsOf(tree);obstacles.Add(new Bounds(b.center,new Vector3(1.5f,b.size.y,1.5f)));made++;}
    int madeCacti=0;for(int a=0;a<200&&madeCacti<12;a++){Vector3 p=new Vector3(Random.Range(-60f,60f),0,Random.Range(16f,46f)*(a%2==0?1:-1));if(!clear(p,.6f))continue;Clone(cactusSources[madeCacti%cactusSources.Length],cacti,"C"+(17+madeCacti)+" Additional cactus",p,Random.Range(.7f,1.05f),Random.Range(0,360));madeCacti++;}
    var samples=vegetation.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshRenderer>()&&m.sharedMesh.isReadable&&m.GetComponent<MeshRenderer>().sharedMaterial&&m.GetComponent<MeshRenderer>().sharedMaterial.name.IndexOf("Gras",StringComparison.OrdinalIgnoreCase)>=0).GroupBy(m=>m.sharedMesh).Select(g=>g.First()).OrderByDescending(m=>m.sharedMesh.vertexCount).Take(2).ToArray();
    if(samples.Length==0)throw new Exception("No readable dry-grass template in scene");
    var patches=new Dictionary<string,Patch>();int tufts=0;
    for(int attempt=0;attempt<9000&&tufts<1500;attempt++)
    {
     bool center=tufts>=1100;float x,z;if(center){x=Random.Range(-26f,30f);z=Random.Range(-10f,10f);}else if(attempt%3==0){x=Random.Range(-62f,62f);z=Random.Range(28f,49f)*(attempt%2==0?1:-1);}else{x=Random.Range(-62f,62f);z=Random.Range(11.5f,20f)*(attempt%2==0?1:-1);}
     Vector3 pos=new Vector3(x,-.025f,z);if(!center&&!clear(pos,.12f))continue;if(center&&Mathf.Abs(z)<2.8f&&(Mathf.Abs(x+4.85f)<3||Mathf.Abs(x-4.85f)<3))continue;
     var template=samples[tufts%samples.Length];var mesh=template.sharedMesh;var renderer=template.GetComponent<MeshRenderer>();float target=center?Random.Range(.18f,.38f):Random.Range(.45f,.9f);float scale=target/Mathf.Max(.001f,mesh.bounds.size.y);scale=Mathf.Min(scale,1.7f/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.z));
     var rotation=Quaternion.Euler(0,Random.Range(0,360),0);var offset=new Vector3(mesh.bounds.center.x,mesh.bounds.min.y,mesh.bounds.center.z);Matrix4x4 matrix=Matrix4x4.TRS(pos,rotation,Vector3.one*scale)*Matrix4x4.Translate(-offset);
     for(int sub=0;sub<mesh.subMeshCount;sub++){var material=renderer.sharedMaterials[Mathf.Min(sub,renderer.sharedMaterials.Length-1)];if(!material)continue;int tileX=Mathf.FloorToInt((x+70)/14),tileZ=Mathf.FloorToInt((z+60)/14);string key=tileX+"_"+tileZ+"_"+material.name;if(!patches.TryGetValue(key,out var patch)){patch=new Patch{material=material};patches.Add(key,patch);}patch.instances.Add(new CombineInstance{mesh=mesh,subMeshIndex=sub,transform=matrix});}
     tufts++;
    }
    int patchId=0;foreach(var patch in patches.Values)
    {var m=new Mesh{indexFormat=IndexFormat.UInt32};m.CombineMeshes(patch.instances.ToArray(),true,true);m.RecalculateBounds();var go=Group("Golden grass bank "+patchId,grass).gameObject;go.AddComponent<MeshFilter>().sharedMesh=SaveMesh(m,"DenseGrassBank_"+patchId++);var r=go.AddComponent<MeshRenderer>();patch.material.SetColor("_BaseColor",new Color(1.5f,1.35f,.9f));patch.material.SetFloat("_Smoothness",0);EditorUtility.SetDirty(patch.material);r.sharedMaterial=patch.material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;go.isStatic=true;go.layer=2;}
    File.WriteAllText(Root+"/Documentation/DensityExpansion.md","# Desert density expansion\n\nAdded 10 houses (22 total), "+made+" trees ("+(8+made)+" total), "+madeCacti+" cacti ("+(16+madeCacti)+" total) and "+tufts+" extra dry-grass tufts in "+patchId+" combined patches.\n\nSeed 58391. Houses/trees exclude the central combat strip. Grass shares existing mesh/material sources and has no colliders; 400 short tufts add ground detail within the arena, with spawn clearances. Original scenery and canyon retained.\n");
    Debug.Log("DESERT_DENSITY_ADDED: 10 houses, "+made+" trees, "+madeCacti+" cacti, "+tufts+" grass tufts / "+patchId+" batches");
   }finally{Random.state=state;}
  }
  [MenuItem("Tools/Desert Outpost/Increase Houses Trees And Grass")]
  public static void UpdateScene()
  {if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first");var scene=SceneManager.GetActiveScene();if(scene.path!=DesertOutpostBuilder.ScenePath)throw new Exception("Open Desert scene first");string backup="Backups/Desert/"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_BeforeDensity";Directory.CreateDirectory(backup);EditorSceneManager.SaveScene(scene,backup+"/Desert_Battle.unity",true);var root=GameObject.Find("Desert_Battlefield").transform;Apply(root);PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/Desert_Battlefield.prefab");EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();DesertOutpostBuilder.Capture();}
 }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TurnBasedBattle;
using ProjectEnvironments;

namespace SunkenHarbor.Editor
{
 public static class HarborVerification
 {
  static List<string> checks=new List<string>();
  static void Need(bool condition,string message){checks.Add((condition?"PASS: ":"FAIL: ")+message);File.WriteAllText(SunkenHarborBuilder.Root+"/Documentation/Verification.md","# Sunken Harbor — Unity Editor Play Mode\n\n"+string.Join("\n\n",checks)+"\n\nStandalone/Android/performance benchmark not run.\n");if(!condition)throw new Exception(message);}
  static Color[] Pixels(Camera c)
  {var rt=new RenderTexture(256,144,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var texture=new Texture2D(256,144,TextureFormat.RGB24,false);try{c.targetTexture=rt;c.aspect=16f/9;c.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,256,144),0,0);texture.Apply();return texture.GetPixels();}finally{c.targetTexture=old;c.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}}
  static float Difference(Color[] a,Color[] b){float sum=0;for(int i=0;i<a.Length;i++)sum+=Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b);return sum/(a.Length*3);}
  [MenuItem("Tools/Sunken Harbor/Verify Battle Optics And Transitions")]
  public static void Run(){if(!EditorApplication.isPlaying||SceneManager.GetActiveScene().path!=SunkenHarborBuilder.ScenePath)throw new Exception("Enter Play Mode in Sunken Harbor");checks.Clear();EnvironmentNavigator.Instance.StartCoroutine(Check());}
  static IEnumerator Check()
  {
   var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();bool manual=battle.manualSimulation,cm=orbit.manualSimulation,input=orbit.inputEnabled;
   try
   {
    battle.manualSimulation=true;orbit.manualSimulation=true;orbit.inputEnabled=false;battle.Restart(1);var root=GameObject.Find("SunkenHarbor_Battlefield").transform;var at=root.GetComponent<HarborAtmosphere>();
    Need(at&&at.config&&at.ocean&&at.optics,"Atmosphere, water and config connected");Need(Mathf.Abs(at.ocean.position.y-42)<.1f,"Ocean surface 42m above battle floor");Need(!at.ocean.GetComponent<Collider>(),"Ocean does not block camera or skills");
    Need(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported)),"All active scene materials supported");
    Need(root.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.sharedMesh.isReadable).All(m=>m.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z))),"All readable environment meshes finite");
    var shaders=AssetDatabase.FindAssets("t:Shader",new[]{SunkenHarborBuilder.Root}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Shader>);Need(shaders.All(s=>!ShaderUtil.GetShaderMessages(s).Any(m=>m.severity==ShaderCompilerMessageSeverity.Error)),"Harbor shaders have no compile errors");
    var crane=AssetDatabase.LoadAssetAtPath<GameObject>(SunkenHarborBuilder.Root+"/Prefabs/HarborCrane_Imported.prefab");Need(crane.GetComponentsInChildren<MeshFilter>().Length==9,"All nine crane GLB parts retained");Need(crane.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3)==255362,"Crane source triangle count retained");
    Need(root.GetComponentsInChildren<HarborLife>().Length==3,"Three fish schools present");Need(root.GetComponentsInChildren<HarborSediment>().Length==1,"One sediment response connected");Need(at.ambience&&at.ambience.clip,"Procedural underwater ambience connected");
    Physics.SyncTransforms();foreach(var pos in new[]{battle.player.AnchorPosition,battle.enemies[0].AnchorPosition,new Vector3(-14,0,7),new Vector3(15,0,-7)})Need(Physics.Raycast(pos+Vector3.up*8,Vector3.down,out var hit,15,1<<0)&&Mathf.Abs(hit.point.y)<.1f,"Continuous seabed at "+pos);
    var blockers=Physics.OverlapBox(new Vector3(4.85f,4,0),new Vector3(22,3.8f,11),Quaternion.identity,1<<2,QueryTriggerInteraction.Ignore).Where(c=>c.transform.IsChildOf(root)).ToArray();Need(blockers.Length==0,"Central combat volume clear: "+string.Join(",",blockers.Select(c=>c.name)));
    Need(battle.CommandsAvailable&&battle.hud,"Player commands and HUD ready");
    foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
    {int enemies=kind==BattleActionKind.EarthPrison?3:1;battle.Restart(enemies);if(enemies==3)battle.FillUltimate();battle.Select(kind);Need(battle.Confirm(),kind+" accepted");int action=battle.ActionId,limit=5000;float damage=battle.Current.hits.Sum(h=>h.damage),peak=0;while(battle.InAction&&battle.ActionId==action&&limit-->0){battle.AdvanceForTest(1f/60);orbit.Step(1f/60);foreach(var r in battle.player.bodyRenderers)if(r&&r.enabled)peak=Mathf.Max(peak,r.bounds.max.y);if(limit%75==0)yield return null;}Need(limit>0,kind+" completes");for(int i=0;i<enemies;i++)Need(Mathf.Abs(battle.enemies[i].HP-(battle.enemies[i].maxHP-damage))<.01f,kind+" expected damage enemy "+i);Need(Vector3.Distance(battle.player.transform.position,battle.player.AnchorPosition)<.01f,kind+" returns to anchor");Need(peak<at.config.waterLevel-5,kind+" stays below surface (peak "+peak.ToString("F2")+"m)");}
    battle.Restart(1);battle.hud.attackButton.onClick.Invoke();battle.hud.confirmButton.onClick.Invoke();int turns=5000;while(battle.InAction&&turns-->0)battle.AdvanceForTest(1f/60);Need(turns>0&&battle.PlayerTurn==2&&battle.player.HP<battle.player.maxHP,"Enemy response completes and returns player control");battle.Restart(1);
    orbit.ResetView(true);for(int i=0;i<8;i++){orbit.Orbit(45,0);for(int j=0;j<60;j++)orbit.Step(1f/60);Need(!Physics.CheckSphere(orbit.transform.position,.04f,1<<2,QueryTriggerInteraction.Ignore),"Orbit free of scenery at step "+i);}
    orbit.Orbit(0,85);orbit.Zoom(65);for(int i=0;i<120;i++)orbit.Step(1f/60);Need(orbit.Pitch>70&&orbit.Distance>40,"Camera supports high pitch and distant zoom");orbit.ResetView(true);
    var camera=root.GetComponentsInChildren<Camera>().First(c=>c.name=="SunkenHarbor_Gameplay");Vector3 oldPos=camera.transform.position;Quaternion oldRot=camera.transform.rotation;Vector4 absorb=at.optics.GetVector("_Absorption");
    try{var wet=Pixels(camera);at.optics.SetVector("_Absorption",Vector4.zero);var dry=Pixels(camera);at.optics.SetVector("_Absorption",absorb);float diff=Difference(wet,dry);Need(diff>.01f,"GPU underwater attenuation changes image (MAD "+diff.ToString("F4")+")");camera.transform.position=new Vector3(0,85,0);camera.transform.rotation=Quaternion.Euler(-85,0,0);var above=Pixels(camera);at.optics.SetVector("_Absorption",Vector4.zero);var aboveDry=Pixels(camera);Need(Difference(above,aboveDry)<.006f,"Above-water sky bypasses underwater attenuation");}
    finally{at.optics.SetVector("_Absorption",absorb);camera.transform.position=oldPos;camera.transform.rotation=oldRot;}
    yield return new WaitForSecondsRealtime(.8f);Need(at.particles.Sum(p=>p.particleCount)>0,"Bubbles and suspended particles alive");Need(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"One enabled gameplay camera");Need(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"One enabled listener");
   }
   finally{if(battle){battle.Restart(1);battle.manualSimulation=manual;}if(orbit){orbit.manualSimulation=cm;orbit.inputEnabled=input;orbit.ResetView(true);}}
   var nav=EnvironmentNavigator.Instance;foreach(string id in new[]{"hub","desert-outpost","sunken-harbor","hub","sunken-harbor"})
   {Need(id=="hub"?nav.OpenHub():nav.Select(id),"Load request accepted: "+id);float start=Time.realtimeSinceStartup;while(nav.Busy&&Time.realtimeSinceStartup-start<45)yield return null;Need(!nav.Busy&&string.IsNullOrEmpty(nav.Error),"Load finished: "+id);Need(SceneManager.GetActiveScene().path==(id=="hub"?nav.Catalog.hubScenePath:nav.Catalog.Find(id).ScenePath),"Correct scene: "+id);Need(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"One camera after "+id);Need(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(c=>c.enabled)==1,"One listener after "+id);Need(UnityEngine.Object.FindObjectsByType<EventSystem>().Length<=1,"No duplicate EventSystem after "+id);if(id=="desert-outpost"){var b=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var so=new SerializedObject(b.battleCamera.GetUniversalAdditionalCameraData());Need(so.FindProperty("m_RendererIndex").intValue<=0,"Desert retains its original renderer, no harbor optics feature");}}
   Debug.Log("SUNKEN_HARBOR_VERIFICATION_PASS: "+checks.Count+" checks");
  }
 }
}

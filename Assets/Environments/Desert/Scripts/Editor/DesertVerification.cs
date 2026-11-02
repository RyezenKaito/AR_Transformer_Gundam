using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TurnBasedBattle;
using ProjectEnvironments;

namespace DesertOutpost.Editor
{
 public static class DesertVerification
 {
  static readonly List<string> checks=new List<string>();
  static void Need(bool ok,string text){checks.Add((ok?"PASS: ":"FAIL: ")+text);File.WriteAllText(DesertOutpostBuilder.Root+"/Documentation/Verification.md","# Desert Outpost — Unity Editor Play Mode\n\n"+string.Join("\n\n",checks)+"\n\nStandalone/Android and performance benchmarks not run.\n");if(!ok)throw new Exception(text);}
  [MenuItem("Tools/Desert Outpost/Verify Battle and Scene Transitions")]
  public static void Run(){if(!EditorApplication.isPlaying)throw new Exception("Enter Play Mode first");checks.Clear();EnvironmentNavigator.Instance.StartCoroutine(Check());}
  static IEnumerator Check()
  {
   var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();bool manual=battle.manualSimulation,cm=orbit.manualSimulation,input=orbit.inputEnabled;
   try
   {
    battle.manualSimulation=true;orbit.manualSimulation=true;orbit.inputEnabled=false;battle.Restart(1);
    var root=GameObject.Find("Desert_Battlefield");Need(root,"Desert battlefield loaded");var origin=root.transform.Find("Desert_LayoutOrigin");
    Need(origin.Find("02_Buildings_H01_H12").Cast<Transform>().Where(t=>t.name.StartsWith("H")&&t.name.Length>2&&char.IsDigit(t.name[1])).Select(t=>t.name.Substring(0,3)).Distinct().Count()==12,"12 individually named building IDs");
    var plants=origin.Find("04_DesertTreesCactiAndDryFlora");Need(plants.Cast<Transform>().Count(t=>t.name.StartsWith("C"))==16,"16 ribbed cactus roots");Need(plants.Cast<Transform>().Count(t=>t.name.StartsWith("T"))==8,"8 desert tree roots");
    Need(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported&&m.shader.name!="Standard")),"All active environment renderers use supported shaders");
    Need(root.GetComponentsInChildren<MeshFilter>().Where(mf=>mf.sharedMesh&&mf.sharedMesh.isReadable).All(mf=>mf.sharedMesh.vertices.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z))),"Environment meshes have finite vertex positions");
    Need(battle.hud&&battle.CommandsAvailable&&battle.player.HP>0,"HUD and player command phase connected");Physics.SyncTransforms();
    foreach(var p in new[]{battle.player.AnchorPosition,battle.enemies[0].AnchorPosition,new Vector3(-15,0,8),new Vector3(15,0,-8)})Need(Physics.Raycast(p+Vector3.up*8,Vector3.down,out var hit,12,1<<0)&&Mathf.Abs(hit.point.y)<.1f,"Continuous floor at "+p);
    var obstacles=Physics.OverlapBox(new Vector3(4.85f,4,0),new Vector3(23,3.8f,9.5f),Quaternion.identity,1<<2,QueryTriggerInteraction.Ignore).Where(c=>c.transform.IsChildOf(root.transform)).ToArray();Need(obstacles.Length==0,"Central combat flight volume clear: "+string.Join(",",obstacles.Select(x=>x.name)));
    foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
    {int enemies=kind==BattleActionKind.EarthPrison?3:1;battle.Restart(enemies);if(enemies==3)battle.FillUltimate();battle.Select(kind);Need(battle.Confirm(),kind+" accepted");int id=battle.ActionId,limit=4000;float damage=battle.Current.hits.Sum(h=>h.damage);while(battle.InAction&&battle.ActionId==id&&limit-->0){battle.AdvanceForTest(1f/60);orbit.Step(1f/60);if(limit%90==0)yield return null;}Need(limit>0,kind+" completes");for(int i=0;i<enemies;i++)Need(Mathf.Abs(battle.enemies[i].HP-(battle.enemies[i].maxHP-damage))<.01f,kind+" expected damage enemy "+i);Need(Vector3.Distance(battle.player.transform.position,battle.player.AnchorPosition)<.01f,kind+" returns to anchor");}
    battle.Restart(1);orbit.ResetView(true);for(int i=0;i<8;i++){orbit.Orbit(45,0);for(int j=0;j<50;j++)orbit.Step(1f/60);Need(!Physics.CheckSphere(orbit.transform.position,.04f,1<<2,QueryTriggerInteraction.Ignore),"Orbit clear step "+i);}
    orbit.Orbit(0,85);orbit.Zoom(40);for(int j=0;j<120;j++)orbit.Step(1f/60);Need(orbit.Pitch>60,"Camera can look up at sky");Need(orbit.Distance>20,"Camera can zoom out beyond close battle view");
   }
   finally{if(battle){battle.Restart(1);battle.manualSimulation=manual;}if(orbit){orbit.manualSimulation=cm;orbit.inputEnabled=input;orbit.ResetView(true);}}
   var nav=EnvironmentNavigator.Instance;
   foreach(string target in new[]{"hub","street-city","desert-outpost","hub","desert-outpost"})
   {Need(target=="hub"?nav.OpenHub():nav.Select(target),"Load request accepted: "+target);float start=Time.realtimeSinceStartup;while(nav.Busy&&Time.realtimeSinceStartup-start<45)yield return null;Need(!nav.Busy&&string.IsNullOrEmpty(nav.Error),"Load completes: "+target);Need(SceneManager.GetActiveScene().path==(target=="hub"?nav.Catalog.hubScenePath:nav.Catalog.Find(target).ScenePath),"Correct scene: "+target);Need(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.enabled)==1,"One enabled camera after "+target);Need(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(c=>c.enabled)==1,"One listener after "+target);Need(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length<=1,"No duplicate EventSystem after "+target);}
   Debug.Log("DESERT_VERIFICATION_PASS: "+checks.Count+" checks");
  }
 }
}

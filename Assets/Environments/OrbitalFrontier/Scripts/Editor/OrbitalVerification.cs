using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.Rendering;
using TurnBasedBattle;
using ProjectEnvironments;

namespace OrbitalFrontier.Editor
{
 public static class OrbitalVerification
 {
  static readonly List<string> checks=new List<string>();
  static void Need(bool ok,string label)
  {checks.Add((ok?"PASS: ":"FAIL: ")+label);File.WriteAllText(OrbitalFrontierBuilder.Root+"/Documentation/Verification.md","# Orbital Frontier — Editor Play Mode verification\n\n"+string.Join("\n\n",checks)+"\n\nStandalone/Android and performance benchmark not run.\n");if(!ok)throw new Exception(label);}
  [MenuItem("Tools/Orbital Frontier/Verify Battle And Navigation")]
  public static void Run()
  {if(!EditorApplication.isPlaying||SceneManager.GetActiveScene().path!=OrbitalFrontierBuilder.ScenePath)throw new Exception("Enter Play Mode in OrbitalFrontier");checks.Clear();EnvironmentNavigator.Instance.StartCoroutine(Check());}
  static IEnumerator Check()
  {
   var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var orbit=battle.battleCamera.GetComponent<BattleOrbitCamera>();bool manual=battle.manualSimulation,cm=orbit.manualSimulation,input=orbit.inputEnabled;
   try
   {
    battle.manualSimulation=true;orbit.manualSimulation=true;orbit.inputEnabled=false;battle.Restart(1);var root=GameObject.Find("OrbitalFrontier_Battlefield");var layout=root.transform.Find("LayoutOrigin");
    Need(root&&layout,"Space battlefield exists");Need(!RenderSettings.fog,"Vacuum has no global fog");Need(root.GetComponentsInChildren<Collider>().Length==0,"Space scenery has no floor or camera-blocking celestial colliders");
    Need(!battle.player.transform.parent.Find("BattleArena").gameObject.activeSelf,"Ground stage disabled");Need(battle.player.zeroGravity&&battle.enemies.All(a=>a.zeroGravity),"Zero-gravity pose enabled for every combat actor");
    Need(orbit.freeOrbit&&orbit.environmentFarClip>=10000,"Space-only orbit and far clip configured");
    Need(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported)),"All active scenery materials supported");
    foreach(var shader in AssetDatabase.FindAssets("t:Shader",new[]{OrbitalFrontierBuilder.Root}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Shader>))Need(!ShaderUtil.GetShaderMessages(shader).Any(m=>m.severity==ShaderCompilerMessageSeverity.Error),"Shader compile: "+shader.name);
    var prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{OrbitalFrontierBuilder.Root+"/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();var ast=prefabs.Where(p=>Path.GetFileName(p).StartsWith("Asteroids_")).ToArray();Need(ast.Length==10,"Ten distinct normalized asteroid prefabs");Need(ast.Sum(p=>AssetDatabase.LoadAssetAtPath<GameObject>(p).GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3))==23076,"Asteroid source triangles retained");
    var alien=prefabs.First(p=>Path.GetFileName(p).StartsWith("AlienShip_"));var ship=AssetDatabase.LoadAssetAtPath<GameObject>(alien);Need(ship.GetComponentsInChildren<MeshFilter>().Length==114,"Alien ship retains all 114 source primitives");Need(ship.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3)==49230,"Alien ship retains all 49230 triangles");
    Need(layout.Find("04_AsteroidField").childCount==88,"88 asteroids arranged outside combat");Need(layout.Find("03_Ships").childCount==7,"One capital ship, five fighters and one dropship");
    Need(!layout.Find("02_Station/ST01 Helios orbital research station")&&!layout.Find("05_Salvage/D01 Broken survey satellite"),"Crude cylinder station and toy satellite removed");
    Need(layout.Find("02_Station").childCount==3&&layout.Find("05_Salvage").childCount==36,"Three asset-built wreck clusters and 36 ship fragments");
    Need(layout.Find("01_CelestialBodies/E04 Jupiter - procedural cloud bands")&&layout.Find("01_CelestialBodies/E05 Saturn system/E05 Saturn annular dust rings"),"Jupiter cloud belts and Saturn with rings present");
    orbit.ResetView(true);var capital=layout.Find("03_Ships/S01 Alien capital ship");Vector3 onScreen=battle.battleCamera.WorldToViewportPoint(capital.position);Need(Vector3.Distance(capital.position,battle.player.transform.position)>120&&Vector3.Distance(capital.position,battle.player.transform.position)<160&&onScreen.z>0&&onScreen.x>.65f&&onScreen.x<.9f&&onScreen.y>.15f&&onScreen.y<.85f,"Capital ship is farther away and on the right of the default gameplay composition");
    var flybys=layout.GetComponentsInChildren<OrbitalFlyby>();Need(flybys.Length==6,"Six continuously recycled asteroid flybys");
    foreach(var f in flybys)
    {f.manualSimulation=true;int loops=f.CompletedLoops;bool visible=false,tailOk=true;float nearest=float.MaxValue;for(int frame=0;frame<210;frame++){f.Step(.1f);visible|=f.VisibleTail>2;tailOk&=f.VisibleTail<=f.tailLength+.01f;nearest=Mathf.Min(nearest,Vector3.Distance(f.transform.position,new Vector3(4.85f,2,0)));for(int j=1;j<f.core.positionCount;j++)tailOk&=Vector3.Distance(f.core.GetPosition(j-1),f.core.GetPosition(j))<=f.tailLength/15+.01f;}Need(f.CompletedLoops>loops&&visible&&tailOk,f.name+" repeats with visible bounded tail and no teleport streak");Need(nearest>55,f.name+" remains outside combat ("+nearest.ToString("F1")+"m)");f.manualSimulation=false;}
    var center=new Vector3(4.85f,2,0);float clearance=layout.GetComponentsInChildren<Renderer>().Min(r=>Vector3.Distance(center,r.bounds.ClosestPoint(center)));Need(clearance>40,"Safe spherical combat volume clear: "+clearance.ToString("F1")+"m nearest scenery bounds");
    var earth=layout.Find("01_CelestialBodies/E01 Earth system/E01 Earth surface NASA");var clouds=layout.Find("01_CelestialBodies/E01 Earth system/E01-C Differential cloud layer");Need(earth&&clouds&&earth.GetComponent<Renderer>().sharedMaterial.GetTexture("_BaseMap"),"NASA Earth texture, cloud shell and atmosphere present");Quaternion e0=earth.localRotation,c0=clouds.localRotation;yield return new WaitForSecondsRealtime(1.2f);Need(Quaternion.Angle(e0,earth.localRotation)>.1f&&Quaternion.Angle(c0,clouds.localRotation)>.15f,"Earth and clouds rotate during Play Mode");Need(layout.GetComponentsInChildren<OrbitalMotion>().Length>=140,"Planet, ship, asteroid and salvage motion connected");
    orbit.ResetView(true);for(int i=0;i<8;i++){orbit.Orbit(45,0);for(int j=0;j<60;j++)orbit.Step(1f/60);Need(Mathf.Abs(Mathf.DeltaAngle(orbit.Yaw,orbit.homeYaw+(i+1)*45))<.2f,"Yaw orbit step "+(i+1));}
    orbit.ResetView(true);orbit.Orbit(0,-85);orbit.Zoom(95);for(int i=0;i<180;i++)orbit.Step(1f/60);Need(orbit.Pitch<-70&&orbit.Distance>95,"Look upward and zoom past 95 m");Need(orbit.transform.position.y<orbit.Pivot.y-70,"Camera actually travels below formation, no floor clamp");Need(Vector3.Dot(battle.battleCamera.transform.forward,(orbit.Pivot-battle.battleCamera.transform.position).normalized)>.99f,"Under-formation camera keeps looking at the formation");
    orbit.Orbit(0,170);for(int i=0;i<180;i++)orbit.Step(1f/60);Need(orbit.transform.position.y>orbit.Pivot.y+70,"Camera can orbit above formation");Need(battle.battleCamera.farClipPlane>=10000,"Late orbit update preserves distant planet clipping range");orbit.ResetView(true);
    Need(battle.CommandsAvailable&&battle.hud,"Battle commands and HUD ready");foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
    {int enemies=kind==BattleActionKind.EarthPrison?3:1;battle.Restart(enemies);if(enemies==3)battle.FillUltimate();battle.Select(kind);Need(battle.Confirm(),kind+" accepted");int action=battle.ActionId,limit=5000;float damage=battle.Current.hits.Sum(h=>h.damage);while(battle.InAction&&battle.ActionId==action&&limit-->0){battle.AdvanceForTest(1f/60);orbit.Step(1f/60);if(limit%90==0)yield return null;}Need(limit>0,kind+" completes");for(int i=0;i<enemies;i++)Need(Mathf.Abs(battle.enemies[i].HP-(battle.enemies[i].maxHP-damage))<.01f,kind+" expected damage enemy "+i);Need(Vector3.Distance(battle.player.transform.position,battle.player.AnchorPosition)<.01f,kind+" returns to its zero-gravity anchor");}
    battle.Restart(1);battle.hud.attackButton.onClick.Invoke();battle.hud.confirmButton.onClick.Invoke();int turns=5000;while(battle.InAction&&turns-->0)battle.AdvanceForTest(1f/60);Need(turns>0&&battle.PlayerTurn==2&&battle.player.HP<battle.player.maxHP,"Enemy response and next player turn complete");battle.Restart(1);
    Need(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"One enabled gameplay camera");Need(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(c=>c.enabled)==1,"One enabled audio listener");
   }
   finally{if(battle){battle.Restart(1);battle.manualSimulation=manual;}if(orbit){orbit.manualSimulation=cm;orbit.inputEnabled=input;orbit.ResetView(true);}}
   var nav=EnvironmentNavigator.Instance;foreach(string id in new[]{"hub","sunken-harbor","orbital-frontier","desert-outpost","orbital-frontier"})
   {Need(id=="hub"?nav.OpenHub():nav.Select(id),"Load accepted: "+id);float start=Time.realtimeSinceStartup;while(nav.Busy&&Time.realtimeSinceStartup-start<45)yield return null;Need(!nav.Busy&&string.IsNullOrEmpty(nav.Error),"Load finished: "+id);Need(SceneManager.GetActiveScene().path==(id=="hub"?nav.Catalog.hubScenePath:nav.Catalog.Find(id).ScenePath),"Correct scene: "+id);Need(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"One camera after "+id);Need(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"One listener after "+id);Need(UnityEngine.Object.FindObjectsByType<EventSystem>().Length<=1,"No duplicate EventSystem after "+id);if(id!="hub"){var b=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var o=b.battleCamera.GetComponent<BattleOrbitCamera>();Need(o.freeOrbit==(id=="orbital-frontier")&&b.player.zeroGravity==(id=="orbital-frontier"),"Space opt-in remains scene-specific: "+id);var data=new SerializedObject(b.battleCamera.GetUniversalAdditionalCameraData());int index=data.FindProperty("m_RendererIndex").intValue;Need(id=="sunken-harbor"?index==1:index<=0,"Correct isolated renderer: "+id);}}
   Debug.Log("ORBITAL_FRONTIER_VERIFICATION_PASS: "+checks.Count+" checks");
  }
 }
}




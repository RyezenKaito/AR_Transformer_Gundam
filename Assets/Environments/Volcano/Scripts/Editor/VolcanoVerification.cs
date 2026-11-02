using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using TurnBasedBattle;
using ProjectEnvironments;

namespace VolcanoArena.Editor
{
    public static class VolcanoVerification
    {
        static readonly List<string> checks=new List<string>();
        static void Need(bool condition,string text)
        {
            checks.Add((condition?"PASS — ":"FAIL — ")+text);
            File.WriteAllText(VolcanoBuilder.Root+"/Documentation/Verification.md","# Volcanic Caldera — Editor Play Mode\n\n"+string.Join("\n\n",checks)+"\n\nStandalone build / Android / performance benchmark not tested.\n");
            if(!condition)throw new Exception(text);
        }
        [MenuItem("Tools/Volcano Arena/Verify Battle in Play Mode")]
        public static void Run()
        {
            if(!EditorApplication.isPlaying || SceneManager.GetActiveScene().path!=VolcanoBuilder.ScenePath)throw new Exception("Open Volcano_Battle and enter Play Mode first.");
            checks.Clear();EnvironmentNavigator.Instance.StartCoroutine(Verify());
        }
        static IEnumerator Verify()
        {
            var battle=UnityEngine.Object.FindAnyObjectByType<TurnBattleManager>();var camera=battle.battleCamera.GetComponent<BattleOrbitCamera>();
            bool manual=battle.manualSimulation,camManual=camera.manualSimulation,input=camera.inputEnabled;
            try
            {
                battle.manualSimulation=true;camera.manualSimulation=true;camera.inputEnabled=false;battle.Restart(1);camera.ResetView(true);
                var root=GameObject.Find("Volcano_Battlefield").transform;
                Need(battle.player && battle.enemies.Length==3 && battle.hud,"Battle rig, three enemy slots and HUD connected");
                Need(battle.CommandsAvailable && battle.player.HP>0,"Battle starts in player-command phase");
                Need(Vector3.Distance(battle.player.transform.position,Vector3.zero)<.01f,"Gundam stands at the basalt arena anchor");
                var atmosphere=root.GetComponent<VolcanoAtmosphere>();
                Need(atmosphere && atmosphere.lavaSurfaces.Length>=8,"Flowing lava, crater and falls connected");
                Need(atmosphere.smoke && atmosphere.eruption && atmosphere.ash,"Smoke, eruption and drifting ash connected");
                Need(atmosphere.ambience && atmosphere.ambience.clip,"Authored volcanic ambience connected");
                Need(atmosphere.config.lavaHeight<-.5f,"Lava surface below the combat deck");
                var lake=root.GetComponentsInChildren<Renderer>().First(r=>r.name=="Molten lake surrounding basalt");
                Need(lake.bounds.center.y> -2.5f && lake.bounds.center.y<-.5f,"Lava visibly close below the basalt rim");
                var surroundingRocks=root.GetComponentsInChildren<MeshCollider>().Count(c=>c.name.StartsWith("Lava shore outcrop")||c.name.StartsWith("Caldera ridge"));
                var surroundingFlows=atmosphere.lavaSurfaces.Count(r=>r.name.StartsWith("Shore lava cascade")||r.name.StartsWith("Ridge lava cascade"));
                Need(surroundingFlows>surroundingRocks*.5f,"Most surrounding peaks carry lava cascades");
                Need(atmosphere.lavaSurfaces.Count(r=>r.name.StartsWith("Volcano lava descent"))>=8,"Main volcano has lava streams around multiple slopes");
                Need(RenderSettings.skybox && battle.battleCamera.clearFlags==CameraClearFlags.Skybox,"Complete volcanic sky enabled");
                Need(root.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh.vertices.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z))),"Environment meshes contain finite vertices");
                Need(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m && m.shader && m.shader.isSupported && m.shader.name!="Standard")),"Volcano uses supported URP materials without Standard/pink shaders");
                Need(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"One enabled battle camera");
                Need(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"One enabled audio listener");
                Physics.SyncTransforms();
                foreach(var point in new[]{new Vector3(-5.4f,10,0),new Vector3(4.3f,10,0),new Vector3(0,10,-8),new Vector3(0,10,8)})
                    Need(Physics.Raycast(point,Vector3.down,out var hit,20) && hit.point.y>-.2f && hit.point.y<.15f,"Continuous combat floor under "+point.x+","+point.z);
                var blockers=Physics.OverlapBox(new Vector3(4.85f,5,0),new Vector3(13,4.8f,7.8f),Quaternion.identity,~0,QueryTriggerInteraction.Ignore).Where(c=>c.transform.IsChildOf(root)).ToArray();
                Need(blockers.Length==0,"Central 26 x 15.6 x 9.6 m volume clear of environment obstacles"+(blockers.Length>0?": "+string.Join(", ",blockers.Select(c=>c.name)):""));
                foreach(var kind in new[]{BattleActionKind.Attack,BattleActionKind.Skill,BattleActionKind.EarthPrison})
                {
                    int count=kind==BattleActionKind.EarthPrison?3:1;battle.Restart(count);if(kind==BattleActionKind.EarthPrison)battle.FillUltimate();
                    if(kind==BattleActionKind.Attack)battle.hud.attackButton.onClick.Invoke();else battle.Select(kind);
                    Need(battle.Confirm(),kind+" command accepted");int action=battle.ActionId;float damage=battle.Current.hits.Sum(h=>h.damage);int limit=4000;float maxHeight=0;bool floorCoverage=true;var deck=root.GetComponentsInChildren<MeshCollider>().First(c=>c.name=="Continuous solid basalt arena");
                    while(battle.InAction && battle.ActionId==action && limit-->0)
                    {
                        battle.AdvanceForTest(1f/60);camera.Step(1f/60);
                        var position=battle.player.transform.position;floorCoverage&=deck.Raycast(new Ray(new Vector3(position.x,50,position.z),Vector3.down),out var floorHit,100);
                        foreach(var r in battle.player.bodyRenderers)if(r && r.enabled)maxHeight=Mathf.Max(maxHeight,r.bounds.max.y);
                        if(limit%60==0)yield return null;
                    }
                    Need(limit>0,kind+" completes without action lock");
                    Need(floorCoverage,kind+" path stays above the smaller solid arena");
                    for(int i=0;i<count;i++)Need(Mathf.Abs(battle.enemies[i].HP-(battle.enemies[i].maxHP-damage))<.01f,kind+" applies expected damage to enemy "+(i+1));
                    Need(Vector3.Distance(battle.player.transform.position,battle.player.AnchorPosition)<.01f,kind+" returns robot to anchor");
                    Need(battle.player.weapons.Equipped==BattleWeaponKind.None,kind+" holsters weapon after return");
                    Need(maxHeight<30f,kind+" has clear flight space (peak "+maxHeight.ToString("F2")+" m)");
                }
                battle.Restart(1);battle.hud.attackButton.onClick.Invoke();battle.hud.confirmButton.onClick.Invoke();
                Need(battle.InAction,"HUD buttons commit a real attack");int guard=5000;
                while(battle.InAction && guard-->0)battle.AdvanceForTest(1f/60);
                Need(guard>0 && battle.PlayerTurn==2 && battle.CommandsAvailable,"Enemy responds and returns control to player");
                Need(battle.player.HP<battle.player.maxHP,"Enemy attack damages player");
                float unguardedDamage=battle.player.maxHP-battle.player.HP;
                battle.Restart(1);battle.Select(BattleActionKind.Guard);Need(battle.Confirm() && battle.Guarding,"Guard command enables damage reduction");guard=5000;
                while(battle.InAction && guard-->0)battle.AdvanceForTest(1f/60);
                Need(guard>0 && battle.CommandsAvailable && battle.PlayerTurn==2,"Guard and enemy turn complete");
                Need(Mathf.Abs((battle.player.maxHP-battle.player.HP)-unguardedDamage*battle.tuning.guardMultiplier)<.01f,"Guard reduces incoming damage by the configured multiplier");
                battle.Restart(1);camera.ResetView(true);
                for(int i=0;i<8;i++)
                {
                    camera.Orbit(45,0);for(int j=0;j<60;j++)camera.Step(1f/60);
                    Need(!Physics.CheckSphere(camera.transform.position,.03f,1<<2,QueryTriggerInteraction.Ignore),"Camera orbit stays outside scenery colliders at step "+(i+1));
                }
                camera.ResetView(true);camera.Orbit(0,1000);camera.Zoom(1000);for(int i=0;i<240;i++)camera.Step(1f/60);
                Need(Mathf.Abs(camera.Distance-80)<.01f && Mathf.Abs(camera.Pitch-89)<.01f,"Camera reaches 80 m overhead view");
                Need(!Physics.CheckSphere(camera.transform.position,.17f,1<<2,QueryTriggerInteraction.Ignore),"Overhead camera outside environment rocks");
                camera.ResetView(true);camera.Orbit(0,-1000);for(int i=0;i<240;i++)camera.Step(1f/60);
                Need(Vector3.Dot(camera.transform.forward,Vector3.up)>.99f,"Camera looks almost straight up into ash sky");
                Need(camera.arenaRoot.InverseTransformPoint(camera.transform.position).y>=.249f,"Sky camera remains above the ground");
                Need(EnvironmentNavigator.Instance.Catalog.Find("volcanic-caldera").available,"Volcano available in shared selector");
                Debug.Log("VOLCANO_BATTLE_VERIFICATION_PASS: "+checks.Count+" checks");
            }
            finally{battle.Restart(1);battle.manualSimulation=manual;camera.manualSimulation=camManual;camera.inputEnabled=input;camera.ResetView(true);}
        }
    }
}

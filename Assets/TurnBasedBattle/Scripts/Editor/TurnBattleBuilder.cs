using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MechaDemo;
using static TurnBasedBattle.Editor.BattleAssetUtility;

namespace TurnBasedBattle.Editor
{
    public static class TurnBattleBuilder
    {
        [MenuItem("Turn Battle/Build Turn Battle Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before building the scene.");
            foreach(string folder in new[]{"Scenes","Prefabs","Materials","Meshes","Textures","Audio","Config","Animations/Player","Animations/Enemy","Reports","Snapshots"})Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();
            var previous=SceneManager.GetActiveScene();
            if(previous.isDirty && !string.IsNullOrEmpty(previous.path))EditorSceneManager.SaveScene(previous,Root+"/Snapshots/BeforeBuild_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var manager=Node("TurnBattleManager").AddComponent<TurnBattleManager>();
            manager.tuning=BuildTuning();manager.initialEnemies=1;
            BuildArena();
            var cameraObject=Node("Battle Orbit Camera");cameraObject.tag="MainCamera";
            cameraObject.transform.position=new Vector3(.8f,8.2f,-19f);cameraObject.transform.LookAt(new Vector3(.8f,1.2f,0));
            var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5.9f;camera.nearClipPlane=.1f;camera.farClipPlane=100;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.048f,.075f);camera.allowHDR=true;
            cameraObject.AddComponent<AudioListener>();var orbit=cameraObject.AddComponent<BattleOrbitCamera>();orbit.battle=manager;
            var cameraData=camera.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=true;
            manager.battleCamera=camera;
            manager.player=BuildPlayer(manager.tuning);manager.player.transform.SetPositionAndRotation(new Vector3(-5.4f,0,0),Quaternion.Euler(0,90,0));
            manager.enemies=new BattleActor[3];
            var enemy=BuildEnemy(manager.tuning);manager.enemies[0]=enemy;enemy.actorId=10;enemy.actorName="SENTINEL • α";
            enemy.transform.SetPositionAndRotation(new Vector3(4.3f,0,0),Quaternion.Euler(0,-90,0));
            for(int i=1;i<3;i++)
            {
                var clone=UnityEngine.Object.Instantiate(enemy.gameObject);clone.name="Sentinel_"+i;var actor=clone.GetComponent<BattleActor>();actor.actorId=10+i;actor.actorName=i==1?"SENTINEL • β":"SENTINEL • γ";
                clone.transform.SetPositionAndRotation(new Vector3(5.05f,0,i==1?-2.15f:2.15f),Quaternion.Euler(0,-90,0));manager.enemies[i]=actor;clone.SetActive(false);
            }
            manager.enemyFormation=Node("EnemyFormationAnchor",null,new Vector3(4.3f,0,0)).transform;
            BattleVfxBuilder.BuildMaterials();manager.earthPrison=BattleVfxBuilder.BuildEarth(camera);manager.effects=BattleVfxBuilder.BuildEffects();
            var content=Node("BattleContent");
            GameObject.Find("BattleArena").transform.SetParent(content.transform,true);
            manager.player.transform.SetParent(content.transform,true);
            foreach(var actor in manager.enemies)actor.transform.SetParent(content.transform,true);
            manager.enemyFormation.SetParent(content.transform,true);manager.earthPrison.transform.SetParent(content.transform,true);manager.effects.transform.SetParent(content.transform,true);
            orbit.arenaRoot=content.transform;orbit.ResetView(true);
            manager.hud=BattleHudBuilder.Build(manager);
            Selection.activeGameObject=manager.gameObject;
            string path=Root+"/Scenes/TurnBattleDemo.unity";EditorSceneManager.SaveScene(scene,path);
            if(MegaKitEnvironmentBuilder.Available)MegaKitEnvironmentBuilder.Apply();
            if(SphereCityEnvironmentBuilder.Available)SphereCityEnvironmentBuilder.Apply();
            var builds=EditorBuildSettings.scenes.ToList();builds.RemoveAll(s=>s.path==path);builds.Insert(0,new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=builds.ToArray();
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            File.WriteAllText(Root+"/Reports/BuildReport.md","# Turn battle build\n\nScene: "+path+"\n\nReused Unicorn rigid rig. New action/animation/controller/HUD/camera/Earth Prison assets saved. Original realtime scenes and source model retained. Runtime verification is recorded separately.\n");
            Debug.Log("TURN_BATTLE_BUILD_OK: "+path);
        }

        static BattleTuning BuildTuning()
        {
            BattleActionDefinition Make(string name,BattleActionKind kind,string label,string clip,float duration,float release,BattleHit[] hits,int gain=0,int cost=0,int cooldown=0,bool all=false)
            {
                var a=ScriptableObject.CreateInstance<BattleActionDefinition>();a.name=name;a.kind=kind;a.displayName=label;a.clipName=clip;a.duration=duration;a.releaseTime=release;a.hits=hits;a.ultimateGain=gain;a.ultimateCost=cost;a.cooldownTurns=cooldown;a.allEnemies=all;
                return Save(a,Root+"/Config/"+name+".asset");
            }
            var t=ScriptableObject.CreateInstance<BattleTuning>();t.name="BattleTuning";
            void Flight(BattleActionDefinition a,BattleWeaponKind weapon,float stop,bool melee=false)
            {
                a.approachTarget=weapon!=BattleWeaponKind.Cannon;a.weapon=weapon;a.melee=melee;a.stopDistance=stop;
                a.travelStyle=weapon==BattleWeaponKind.Cannon?BattleTravelStyle.Stationary:melee?BattleTravelStyle.Run:BattleTravelStyle.Fly;
                a.travelStart=.65f;a.arriveTime=1.25f;a.returnStart=2.15f;a.returnEnd=3.02f;a.travelLift=.65f;
                a.gripTime=.42f;a.drawEnd=.66f;a.bladeOn=.72f;a.bladeOff=1.96f;a.sheatheStart=3.08f;a.holsterTime=3.52f;
                if(melee)
                {
                    a.travelStart=.75f;a.arriveTime=5.45f;a.returnStart=6.6f;a.returnEnd=11.5f;a.travelLift=0;a.runStride=2.1f;
                    a.bladeOn=5.28f;a.bladeOff=6.2f;a.sheatheStart=11.68f;a.holsterTime=12.1f;
                }
                EditorUtility.SetDirty(a);
            }
            t.attack=Make("BasicAttack",BattleActionKind.Attack,"TẤN CÔNG · Chạy áp sát / kiếm ánh sáng","BasicAttack",12.45f,5.75f,new[]{new BattleHit{time=5.9f,damage=100}},25);
            Flight(t.attack,BattleWeaponKind.Saber,1.55f,true);
            t.skill=Make("EnergyCannon",BattleActionKind.Skill,"KỸ NĂNG · Bắn pháo tại chỗ","SkillCast",4.0f,1.5f,new[]{new BattleHit{time=1.8f,damage=180}},20,0,2);
            Flight(t.skill,BattleWeaponKind.Cannon,3.1f);
            t.guard=Make("Guard",BattleActionKind.Guard,"PHÒNG THỦ · Giảm 50% sát thương","Guard",.65f,0,Array.Empty<BattleHit>());
            t.ultimate=Make("EarthPrison",BattleActionKind.EarthPrison,"EARTH PRISON · Bay cao / thi triển trên không","EarthPrisonCast",13.1f,5.2f,new[]{new BattleHit{time=3.35f,damage=120},new BattleHit{time=5.1f,damage=60},new BattleHit{time=5.5f,damage=60},new BattleHit{time=5.9f,damage=60},new BattleHit{time=6.3f,damage=60}},0,100,0,true);
            Flight(t.ultimate,BattleWeaponKind.None,4.85f);t.ultimate.airborneCast=true;t.ultimate.castHeightRatio=1.15f;t.ultimate.vfxStart=2.55f;t.ultimate.travelStart=.45f;t.ultimate.arriveTime=2.2f;t.ultimate.returnStart=10.55f;t.ultimate.returnEnd=12.5f;
            t.enemyStrike=Make("EnemyStrike",BattleActionKind.EnemyStrike,"SENTINEL · Chạy áp sát / phá giáp","EnemyStrike",12.45f,5.75f,new[]{new BattleHit{time=5.9f,damage=80,cue=5.0f,open=5.65f,close=5.85f}});
            Flight(t.enemyStrike,BattleWeaponKind.Saber,1.55f,true);
            t.enemyCombo=Make("EnemyCombo",BattleActionKind.EnemyCombo,"SENTINEL · Liên xạ tại chỗ","EnemyCombo",4.9f,1.3f,new[]{new BattleHit{time=1.6f,damage=55,cue=.7f,open=1.35f,close=1.55f},new BattleHit{time=2.6f,damage=55,cue=1.85f,open=2.35f,close=2.55f}});
            Flight(t.enemyCombo,BattleWeaponKind.Cannon,3.1f);t.enemyCombo.returnStart=3.05f;t.enemyCombo.returnEnd=3.92f;t.enemyCombo.sheatheStart=4.0f;t.enemyCombo.holsterTime=4.5f;
            t.enemyCombos=true;return Save(t,Root+"/Config/BattleTuning.asset");
        }

        static BattleActor BuildPlayer(BattleTuning tuning)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MechaCombatDemo/Unicorn_CombatRig.prefab");if(prefab==null)throw new InvalidOperationException("Existing Unicorn rig prefab missing.");
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(g,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);g.name="Unicorn_BattlePlayer";
            var rig=g.GetComponent<MechaRigData>();rig.ResetPose();rig.drawJointGizmos=false;
            foreach(var a in g.GetComponentsInChildren<Animator>(true)){a.runtimeAnimatorController=null;a.applyRootMotion=false;a.enabled=false;}
            var actor=g.AddComponent<BattleActor>();actor.actorId=1;actor.player=true;actor.actorName="UNICORN / RX-0";actor.rig=rig;actor.visual=rig.visualRoot;
            actor.clips=BattleClipBuilder.Build(rig,Root+"/Animations/Player",tuning);
            BattleMeshOptimizer.Combine(g.transform,"UnicornRigid",true);
            BattleWeaponBuilder.Build(actor);
            actor.bodyRenderers=g.GetComponentsInChildren<Renderer>(true);
            actor.PreviewPose("IdleBattle",0);
            PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/Unicorn_BattlePlayer.prefab");return actor;
        }

        static BattleActor BuildEnemy(BattleTuning tuning)
        {
            var g=Node("Sentinel_0");var visual=Node("VisualRoot",g.transform);var skeleton=Node("Skeleton",visual.transform);
            var joints=new List<Transform>();var map=new Dictionary<string,Transform>();
            Transform Joint(string name,Transform parent,Vector3 pos){var t=Node(name,parent,pos).transform;joints.Add(t);map.Add(name,t);return t;}
            var pelvis=Joint("Pelvis",skeleton.transform,new Vector3(0,1.05f,0));var spine=Joint("Spine",pelvis,new Vector3(0,.35f,0));var chest=Joint("Chest",spine,new Vector3(0,.35f,0));
            var neck=Joint("Neck",chest,new Vector3(0,.48f,0));var head=Joint("Head",neck,new Vector3(0,.15f,0));Joint("Backpack",chest,new Vector3(0,0,-.25f));
            var armor=Lit("SentinelArmor",new Color(.20f,.14f,.12f),default,.65f);var black=Lit("SentinelFrame",new Color(.035f,.048f,.065f),default,.6f);
            var red=Lit("SentinelRed",new Color(.48f,.12f,.055f),default,.5f);var glow=Lit("SentinelCore",new Color(1,.3f,.035f),new Color(2,.35f,.04f));
            Shape("HipArmor",PrimitiveType.Cube,pelvis,Vector3.zero,new Vector3(.72f,.30f,.50f),armor);
            Shape("SpineJoint",PrimitiveType.Cylinder,spine,Vector3.zero,new Vector3(.35f,.20f,.35f),black);
            Shape("ChestArmor",PrimitiveType.Cube,chest,new Vector3(0,.10f,0),new Vector3(.98f,.58f,.50f),armor);
            Shape("FrontPlastron",PrimitiveType.Cube,chest,new Vector3(0,.10f,.29f),new Vector3(.62f,.40f,.12f),red,Quaternion.Euler(0,0,45));
            Shape("CoreRing",PrimitiveType.Cylinder,chest,new Vector3(0,.12f,.39f),new Vector3(.28f,.035f,.28f),black,Quaternion.Euler(90,0,0));
            Shape("Core",PrimitiveType.Sphere,chest,new Vector3(0,.12f,.43f),new Vector3(.18f,.18f,.08f),glow);
            Shape("Helmet",PrimitiveType.Cube,head,new Vector3(0,.06f,0),new Vector3(.46f,.42f,.42f),armor,Quaternion.Euler(0,0,0));
            Shape("Visor",PrimitiveType.Cube,head,new Vector3(0,.10f,.22f),new Vector3(.34f,.07f,.035f),glow);
            Shape("Crest",PrimitiveType.Cube,head,new Vector3(0,.34f,0),new Vector3(.10f,.23f,.28f),red,Quaternion.Euler(-12,0,0));
            Transform leftHand=null;
            foreach(string side in new[]{"L","R"})
            {
                float sign=side=="L"?-1:1;
                var shoulder=Joint("Shoulder_"+side,chest,new Vector3(sign*.65f,.20f,0));
                var arm=Joint("UpperArm_"+side,chest,new Vector3(sign*.65f,.12f,0));var elbow=Joint("LowerArm_"+side,arm,new Vector3(0,-.48f,0));var hand=Joint("Hand_"+side,elbow,new Vector3(0,-.40f,0));
                Shape("ShoulderPlate",PrimitiveType.Cube,shoulder,new Vector3(sign*.04f,.03f,0),new Vector3(.50f,.32f,.68f),red,Quaternion.Euler(0,0,sign*-15));
                Shape("UpperArm",PrimitiveType.Cylinder,arm,new Vector3(0,-.21f,0),new Vector3(.30f,.23f,.30f),black);
                Shape("ForearmArmor",PrimitiveType.Cube,elbow,new Vector3(0,-.18f,.02f),new Vector3(.34f,.42f,.38f),armor);
                Shape("Fist",PrimitiveType.Cube,hand,Vector3.zero,new Vector3(.30f,.24f,.30f),black);
                var thigh=Joint("UpperLeg_"+side,pelvis,new Vector3(sign*.25f,-.08f,0));var knee=Joint("LowerLeg_"+side,thigh,new Vector3(0,-.40f,0));var foot=Joint("Foot_"+side,knee,new Vector3(0,-.42f,.01f));
                Shape("ThighArmor",PrimitiveType.Cube,thigh,new Vector3(0,-.17f,0),new Vector3(.34f,.36f,.38f),armor);
                Shape("Knee",PrimitiveType.Sphere,knee,new Vector3(0,0,.10f),new Vector3(.36f,.27f,.30f),red);
                Shape("ShinArmor",PrimitiveType.Cube,knee,new Vector3(0,-.23f,0),new Vector3(.31f,.36f,.33f),armor);
                Shape("Foot",PrimitiveType.Cube,foot,new Vector3(0,-.065f,.13f),new Vector3(.40f,.14f,.63f),black);
                foreach(string skirt in new[]{"FrontSkirt","SideSkirt","RearSkirt"})Joint(skirt+"_"+side,pelvis,new Vector3(sign*.30f,0,skirt=="FrontSkirt"?.28f:skirt=="RearSkirt"?-.24f:0));
                if(side=="L")leftHand=hand;
            }
            var rig=g.AddComponent<MechaRigData>();rig.visualRoot=visual.transform;rig.skeletonRoot=skeleton.transform;rig.joints=joints.ToArray();rig.restLocalPositions=joints.Select(t=>t.localPosition).ToArray();rig.restLocalRotations=joints.Select(t=>t.localRotation).ToArray();rig.referenceHeight=2.8f;
            var actor=g.AddComponent<BattleActor>();actor.rig=rig;actor.visual=visual.transform;
            actor.clips=BattleClipBuilder.Build(rig,Root+"/Animations/Enemy",tuning);BattleWeaponBuilder.Build(actor);
            actor.bodyRenderers=g.GetComponentsInChildren<Renderer>(true);actor.PreviewPose("IdleBattle",0);
            PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/Sentinel.prefab");return actor;
        }

        static void BuildArena()
        {
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.30f,.36f,.45f);RenderSettings.fog=false;
            var ground=Lit("ArenaGround",new Color(.035f,.065f,.085f),default,.5f);var panel=Lit("ArenaPanel",new Color(.065f,.105f,.135f),default,.55f);
            var border=Lit("ArenaTrim",new Color(.10f,.15f,.19f),default,.7f);var cyan=Lit("ArenaCyan",new Color(.06f,.45f,.55f),new Color(.04f,.75f,.9f));var orange=Lit("ArenaAmber",new Color(.55f,.27f,.08f),new Color(.9f,.32f,.035f));
            var stage=Node("BattleArena");Shape("StageBase",PrimitiveType.Cube,stage.transform,new Vector3(0,-.25f,0),new Vector3(25,.4f,14),ground);
            for(int x=-6;x<=6;x++)for(int z=-3;z<=3;z++)Shape("FloorPanel",PrimitiveType.Cube,stage.transform,new Vector3(x*1.8f,-.035f,z*1.8f),new Vector3(1.77f,.06f,1.77f),(x+z)%2==0?panel:ground);
            Shape("PlayerPlatform",PrimitiveType.Cylinder,stage.transform,new Vector3(-5.4f,-.025f,0),new Vector3(4.5f,.04f,4.5f),border);
            Shape("EnemyPlatform",PrimitiveType.Cylinder,stage.transform,new Vector3(4.3f,-.025f,0),new Vector3(8.8f,.04f,8.8f),border);
            Ring("PlayerRing",stage.transform,new Vector3(-5.4f,.035f,0),2.2f,cyan,.025f);
            Ring("EnemyRing",stage.transform,new Vector3(4.3f,.035f,0),4.25f,orange,.018f);
            for(int side=-1;side<=1;side+=2)
            {
                Shape("RearRailing",PrimitiveType.Cube,stage.transform,new Vector3(0,.15f,side*6.7f),new Vector3(25,.3f,.25f),border);
                Shape("RearLight",PrimitiveType.Cube,stage.transform,new Vector3(0,.32f,side*6.7f),new Vector3(24,.025f,.10f),cyan);
                for(int i=-5;i<=5;i+=2)Shape("ArenaBeacon",PrimitiveType.Cube,stage.transform,new Vector3(i*2.1f,.55f,side*6.7f),new Vector3(.16f,1.1f,.16f),border);
            }
            BattleMeshOptimizer.Combine(stage.transform,"ArenaStatic");
            var floor=Node("CameraFloor",stage.transform,new Vector3(0,-.3f,0));floor.layer=2;var floorCollider=floor.AddComponent<BoxCollider>();floorCollider.size=new Vector3(40,.5f,32);

            var key=Node("Key Light").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.75f;key.color=new Color(1,.91f,.80f);key.transform.rotation=Quaternion.Euler(42,-35,0);key.shadows=LightShadows.Soft;
            var fill=Node("Fill Light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.6f;fill.color=new Color(.36f,.64f,1);fill.transform.rotation=Quaternion.Euler(28,135,0);fill.shadows=LightShadows.None;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="BattleVolume";var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.28f);bloom.threshold.Override(1.15f);bloom.scatter.Override(.55f);
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);profile=Save(profile,Root+"/Config/BattleVolume.asset");
            foreach(var component in profile.components)if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);
            var volume=Node("Battle Post Processing").AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
        }
        static void Ring(string name,Transform parent,Vector3 center,float radius,Material mat,float width)
        {
            var line=Line(name,parent,mat,97);line.widthMultiplier=width;var points=new Vector3[97];for(int i=0;i<97;i++){float a=i*Mathf.PI*2/96;points[i]=center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);}line.SetPositions(points);
        }
    }
}

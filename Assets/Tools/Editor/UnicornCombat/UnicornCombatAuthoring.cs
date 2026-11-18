using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GundamGame.CombatDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace GundamGame.Unicorn.Editor
{
    public static class UnicornCombatAuthoring
    {
        public const string Character = "Assets/Game/Features/Characters/Unicorn";
        public const string Demo = "Assets/Sandbox/UnicornCombat";
        public const string ScenePath = Demo + "/Scenes/Unicorn Demo.unity";
        public const string PrefabPath = Character + "/Prefabs/Unicorn_Base.prefab";
        const string ModelPath = Character + "/Art/Unicorn_Base.fbx";
        const string MotionMaterials = "Assets/Sandbox/Showcases/UnicornMotion/Materials/";
        const string EquipmentTexture = "Assets/Models/rx-0-full-armor-unicorn-gundam/source/RX-0 Full Armor Unicorn Gundam/tex_7.png";
        public const string Reports = "Docs/Reports/UnicornCombat";

        [MenuItem("Gundam/Unicorn/Build Combat Demo")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(SceneManager.GetSceneByPath(ScenePath).isLoaded)throw new InvalidOperationException("Close the open Unicorn Demo scene before rebuilding it.");
            foreach(var path in new[]{Character+"/Prefabs",Character+"/Animations",Character+"/Materials",Demo+"/Scenes","Assets/Sandbox/CombatTest/Materials",Reports})
                Directory.CreateDirectory(path);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer=(ModelImporter)AssetImporter.GetAtPath(ModelPath);
            if(importer==null)throw new InvalidOperationException("Unicorn FBX not imported.");
            importer.animationType=ModelImporterAnimationType.Generic;
            importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects=false;
            importer.importLights=false;
            importer.importCameras=false;
            importer.importAnimation=true;
            importer.animationCompression=ModelImporterAnimationCompression.Off;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var clips=importer.defaultClipAnimations;
            var names=new[]{"Idle","HitReact","TwinVulcan","PsychoFrameAwakening","BeamMagnum","FullBurst","Deploy","Dodge","Parry","Defeat"};
            var cues=new Dictionary<string,(int frame,int cue)[]>
            {
                {"PsychoFrameAwakening",new[]{(48,UnicornCombatDemo.BuffCue)}},
                {"BeamMagnum",new[]{(48,UnicornCombatDemo.MagnumCue)}},
                {"FullBurst",new[]{(30,UnicornCombatDemo.ComboFirstCue),(44,UnicornCombatDemo.ComboSecondCue),
                    (61,UnicornCombatDemo.ComboKickCue),(139,UnicornCombatDemo.UltraBeamCue)}}
            };
            foreach(var name in names)
            {
                var clip=clips.SingleOrDefault(c=>c.takeName.EndsWith("Unicorn_"+name,StringComparison.Ordinal));
                if(clip==null)throw new InvalidOperationException("Missing take "+name+" in "+string.Join(", ",clips.Select(c=>c.takeName)));
                clip.name="Unicorn_"+name;
                clip.loopTime=name=="Idle";
                clip.loopPose=clip.loopTime;
                clip.lockRootRotation=clip.lockRootHeightY=clip.lockRootPositionXZ=true;
                clip.keepOriginalOrientation=clip.keepOriginalPositionY=clip.keepOriginalPositionXZ=true;
                if(cues.TryGetValue(name,out var markers))
                    clip.events=markers.Select(marker=>new AnimationEvent
                    {
                        functionName=UnicornCombatDemo.CombatCueEvent,
                        intParameter=marker.cue,
                        time=marker.frame/(clip.lastFrame-clip.firstFrame)
                    }).ToArray();
            }
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var sourceMaterials=source.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct().ToArray();
            foreach(var item in sourceMaterials)
            {
                string key=item.name.EndsWith("_RED",StringComparison.Ordinal)?item.name.Substring(0,item.name.Length-4):item.name;
                var old=AssetDatabase.LoadAssetAtPath<Material>(MotionMaterials+key+".mat");
                var materialPath=Character+"/Materials/"+key+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(!mat)
                {
                    mat=old?new Material(old):new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.name=key;
                    AssetDatabase.CreateAsset(mat,materialPath);
                }
                if(key=="UnicornEquipmentAtlas")
                {
                    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(EquipmentTexture);
                    if(!tex)throw new InvalidOperationException("Equipment atlas missing.");
                    mat.SetTexture("_BaseMap",tex);mat.color=Color.white;mat.SetFloat("_Smoothness",.32f);
                }
                else if(key.StartsWith("UnicornSaber",StringComparison.Ordinal))
                {
                    bool core=key=="UnicornSaberHotEdge",ribbon=key=="UnicornSaberRibbon";
                    mat.shader=Shader.Find("Gundam/Unicorn Saber Plasma");
                    mat.renderQueue=-1;
                    mat.SetColor("_Tint",core?new Color(4.2f,.64f,.79f):
                        ribbon?new Color(1.6f,.018f,.078f):new Color(2.9f,.032f,.096f));
                    mat.SetFloat("_Opacity",core?.87f:ribbon?.47f:.66f);
                    mat.SetFloat("_EdgeWeight",core?.12f:ribbon?.27f:.77f);
                    mat.SetShaderPassEnabled("ShadowCaster",false);
                }
                else if(!old)throw new InvalidOperationException("Original armor material not found: "+key);
                var map=mat.GetTexture("_BaseMap");
                if(map && map.name.IndexOf("_RED",StringComparison.OrdinalIgnoreCase)>=0)
                {
                    mat.name=key;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetTexture("_EmissionMap",map);mat.SetColor("_EmissionColor",Color.black);
                    mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
                }
                EditorUtility.SetDirty(mat);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),key),mat);
            }
            importer.clipAnimations=clips;
            importer.SaveAndReimport();
            var actions=AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().Where(c=>names.Any(n=>c.name=="Unicorn_"+n)).ToArray();
            if(actions.Length!=10)throw new InvalidOperationException("Expected ten unique animation clips.");
            foreach(var entry in cues)
            {
                var markers=actions.Single(action=>action.name=="Unicorn_"+entry.Key).events;
                if(markers.Length!=entry.Value.Length || entry.Value.Any(marker=>!markers.Any(e=>
                    e.functionName==UnicornCombatDemo.CombatCueEvent && e.intParameter==marker.cue &&
                    Mathf.Abs(e.time-marker.frame/30f)<.001f)))
                    throw new InvalidOperationException(entry.Key+" has incorrect combat event timing.");
            }
            source=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var controllerPath=Character+"/Animations/Unicorn_Base.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(!controller)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;
            foreach(var entry in machine.states)
                if(entry.state.name=="Unicorn_BeamTonfa"||entry.state.name=="Unicorn_BackpackBeamSaber")machine.RemoveState(entry.state);
            foreach(var clip in actions)
            {
                var state=machine.states.FirstOrDefault(s=>s.state.name==clip.name).state;
                if(!state)state=machine.AddState(clip.name);
                state.motion=clip;
                if(clip.name=="Unicorn_Idle")machine.defaultState=state;
            }

            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var model=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);
                model.name="Unicorn_Base";
                var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();
                animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())skin.updateWhenOffscreen=true;
                foreach(var fx in model.GetComponentsInChildren<Renderer>().Where(IsSaberEffect))
                {
                    fx.shadowCastingMode=ShadowCastingMode.Off;
                    fx.receiveShadows=false;
                }
                var gun=Find(model,"Unicorn_Donor_Rifle");
                var shield=Find(model,"Unicorn_Donor_Shield");
                if(gun.parent.name!="RifleMount"||shield.parent.name!="Forearm.L")
                    throw new InvalidOperationException("Weapon and shield bindings differ from the source rig.");
                if(FaceCount(gun)!=458||FaceCount(shield)!=455)throw new InvalidOperationException("Equipment geometry differs from the approved model.");
                var idle=actions.Single(c=>c.name=="Unicorn_Idle");
                idle.SampleAnimation(model,0);
                shield.gameObject.SetActive(false);
                PrefabUtility.RecordPrefabInstancePropertyModifications(shield.gameObject);
                var start=Find(model,"Muzzle").position;
                actions.Single(c=>c.name=="Unicorn_BeamMagnum").SampleAnimation(model,48f/30f);
                if(Vector3.Distance(start,Find(model,"Muzzle").position)<.1f)throw new InvalidOperationException("Magnum clip does not move the equipped muzzle.");
                var mount=Find(model,"RifleMount");
                var handSocket=Find(model,"Magnum_RightHandSocket");
                if(Vector3.Distance(mount.position,handSocket.position)>.03f)
                    throw new InvalidOperationException("Magnum does not match its hand socket at the fire frame.");
                actions.Single(c=>c.name=="Unicorn_BeamMagnum").SampleAnimation(model,84f/30f);
                if(Vector3.Distance(mount.position,Find(model,"Magnum_BackpackSocket").position)>.03f)
                    throw new InvalidOperationException("Magnum does not return to the backpack.");
                idle.SampleAnimation(model,0);
                var prefab=PrefabUtility.SaveAsPrefabAsset(model,PrefabPath);
                UnityEngine.Object.DestroyImmediate(model);

                var platform=new GameObject("Unicorn Test Arena").transform;
                Stage(platform);
                var player=Fighter(platform,prefab,"Unicorn",new Vector3(0,0,-2.5f),0,2.55f,680,185,100,86,153,60);
                player.lowerImportedArmsAtRest=false;
                var enemies=new[]{
                    Fighter(platform,Source("RX-78-2-Gundam"),"RX-78-2",new Vector3(-3.0f,0,2.3f),180,2.0f,250,36,90,70,134,36),
                    Fighter(platform,Source("ZGMF-X09A-Justice-Gundam"),"JUSTICE",new Vector3(0,0,2.75f),180,2.05f,270,105,100,78,146,46,"00-IDLE"),
                    Fighter(platform,Source("EXtreme-Application-ONEXtreme-GUNDAM"),"EXTREME",new Vector3(3.1f,0,2.3f),180,2.22f,315,185,117,90,156,55,"Idle")
                };
                var camera=new GameObject("Unicorn Combat Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
                camera.tag="MainCamera";camera.fieldOfView=39;camera.nearClipPlane=.03f;camera.farClipPlane=80;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.021f,.031f,.055f);
                var urp=camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                urp.renderPostProcessing=true;urp.antialiasing=AntialiasingMode.FastApproximateAntialiasing;
                camera.transform.position=new Vector3(-10,5,-3);camera.transform.LookAt(Vector3.up);
                var volume=new GameObject("Combat Bloom").AddComponent<Volume>();
                volume.isGlobal=true;
                var profilePath="Assets/Sandbox/CombatTest/Materials/Combat Volume.asset";
                var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
                if(!profile)
                {
                    profile=ScriptableObject.CreateInstance<VolumeProfile>();
                    AssetDatabase.CreateAsset(profile,profilePath);
                    var bloom=profile.Add<Bloom>();
                    bloom.threshold.Override(1.1f);
                    bloom.intensity.Override(.42f);
                    bloom.scatter.Override(.65f);
                    AssetDatabase.AddObjectToAsset(bloom,profile);
                    EditorUtility.SetDirty(profile);
                }
                volume.sharedProfile=profile;
                Sun("Key Light",new Color(1,.93f,.85f),1.9f,new Vector3(46,135,0));
                Sun("Blue Rim",new Color(.39f,.65f,1),1.25f,new Vector3(26,-35,0));
                RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.51f,.55f,.64f);
                var go=new GameObject("Unicorn Combat");
                var demo=go.AddComponent<UnicornCombatDemo>();demo.effects=go.AddComponent<UnicornCombatEffects>();
                demo.hudPrefab=AssetDatabase.LoadAssetAtPath<MechaBattleHUD>(
                    "Assets/Game/Features/Combat/UI/Shared/Prefabs/StrikeRougeMechaHUD.prefab");
                demo.hudArt=AssetDatabase.LoadAssetAtPath<RougeHudArt>(
                    "Assets/Game/Features/Characters/Unicorn/UI/Unicorn HUD Art.asset");
                demo.player=player;demo.enemies=enemies;demo.battleCamera=camera;demo.clips=actions;
                demo.model=player.visual.GetChild(0).GetChild(0).gameObject;
                demo.muzzle=Find(demo.model,"Muzzle");demo.eject=Find(demo.model,"Eject");
                demo.vulcanL=Find(demo.model,"Vulcan_L");demo.vulcanR=Find(demo.model,"Vulcan_R");
                demo.focusHead=Find(demo.model,"CameraFocus_Head");demo.focusBody=Find(demo.model,"CameraFocus_Body");
                demo.handLeft=Find(demo.model,"Hand.L");demo.handRight=Find(demo.model,"Hand.R");demo.kickFoot=Find(demo.model,"Foot.R");
                const string buffPath="Assets/Game/Features/Characters/Unicorn/Data/Unicorn Psycho-Frame Buff.asset";
                demo.buffSettings=AssetDatabase.LoadAssetAtPath<UnicornBuffSettings>(buffPath);
                if(!demo.buffSettings)
                {
                    demo.buffSettings=ScriptableObject.CreateInstance<UnicornBuffSettings>();
                    AssetDatabase.CreateAsset(demo.buffSettings,buffPath);
                }
                EditorSceneManager.SaveScene(scene,ScenePath);
                AssetDatabase.SaveAssets();
                File.WriteAllLines(Reports+"/Authoring.txt",new[]{
                    "PASS: Unicorn_Base uses equipped v02 source with complete shield and rifle.",
                    "PASS: Rifle and shield retain 458/455 triangles. Shield is disabled on the canonical Unity prefab.",
                    "PASS: Magnum attaches to the hand socket at frame 48 and returns to the backpack before Idle.",
                    "PASS: Ten Generic clips and muzzle motion verified.",
                    "PASS: Original armor and donor atlas remapped to URP.",
                    "Scene: "+ScenePath,"Prefab: "+PrefabPath
                });
            }
            finally
            {
                EditorSceneManager.CloseScene(scene,true);
                if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
            }
        }

        static int FaceCount(Transform item)
        {
            var mesh=item.GetComponent<MeshFilter>();
            if(!mesh)throw new InvalidOperationException("Expected rigid donor mesh: "+item.name);
            return (int)mesh.sharedMesh.GetIndexCount(0)/3;
        }
        static Transform Find(GameObject root,string name)=>root.GetComponentsInChildren<Transform>(true).Single(t=>t.name==name);
        static bool IsSaberEffect(Renderer renderer)=>renderer.sharedMaterials.Any(mat=>mat&&mat.name.StartsWith("UnicornSaber",StringComparison.Ordinal));
        static GameObject Source(string folder)=>AssetDatabase.FindAssets("t:GameObject",new[]{"Assets/Models/"+folder})
            .Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.EndsWith(".glb",StringComparison.OrdinalIgnoreCase))
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>).First(o=>o);
        static DemoFighter Fighter(Transform arena,GameObject prefab,string title,Vector3 home,float yaw,float height,int hp,int armor,int attack,int def,int speed,int breakDamage,string idleName=null)
        {
            var root=new GameObject(title).transform;root.SetParent(arena,false);
            var visual=new GameObject("Visual").transform;visual.SetParent(root,false);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab,arena.gameObject.scene);
            var placement=new GameObject("Model Placement").transform;
            placement.SetParent(visual,false);
            model.transform.SetParent(placement,false);
            foreach(var a in model.GetComponentsInChildren<Animator>())a.enabled=false;
            if(idleName!=null)
            {
                var idle=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(prefab)).OfType<AnimationClip>().Single(c=>c.name==idleName);
                idle.SampleAnimation(model,.65f);
                foreach(var child in model.GetComponentsInChildren<Transform>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
            var bounds=model.GetComponentsInChildren<Renderer>().Where(r=>!IsSaberEffect(r)).Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
            float factor=height/Mathf.Max(.01f,bounds.size.y);
            placement.localScale=Vector3.one*factor;
            placement.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*factor;
            root.localPosition=home;root.localRotation=Quaternion.Euler(0,yaw,0);
            var actor=root.gameObject.AddComponent<DemoFighter>();
            actor.visual=visual;actor.displayName=title;actor.height=height;
            actor.maxHealth=hp;actor.maxArmor=armor;actor.attack=attack;actor.defense=def;actor.speed=speed;actor.breakDamage=breakDamage;
            var ring=Enumerable.Range(0,49).Select(i=>new Vector3(Mathf.Cos(i*Mathf.PI/24)*.84f,.013f,Mathf.Sin(i*Mathf.PI/24)*.84f)).ToArray();
            Line(root,"Position slot",ring,Material(title=="Unicorn"?"Player Slot":"Enemy Slot",title=="Unicorn"?new Color(.16f,.64f,.74f):new Color(.65f,.21f,.32f),true),.027f);
            return actor;
        }
        static void Stage(Transform arena)
        {
            var floor=Material("Graphite Floor",new Color(.047f,.073f,.104f));
            var grid=Material("Grid",new Color(.081f,.157f,.21f),true);
            var trim=Material("Border",new Color(.13f,.45f,.56f),true);
            var edge=Material("Floor Edge",new Color(.076f,.114f,.156f));
            Cube(arena,"Platform",new Vector3(0,-.14f,0),new Vector3(11.4f,.26f,8.6f),floor);
            Cube(arena,"Platform Base",new Vector3(0,-.36f,0),new Vector3(11.65f,.22f,8.85f),edge);
            for(int i=-5;i<=5;i++)Line(arena,"Grid X",new[]{new Vector3(i,0,-4.1f),new Vector3(i,0,4.1f)},grid,.011f);
            for(int i=-4;i<=4;i++)Line(arena,"Grid Z",new[]{new Vector3(-5.5f,0,i),new Vector3(5.5f,0,i)},grid,.011f);
            Line(arena,"Border",new[]{new Vector3(-5.54f,.01f,-4.16f),new Vector3(5.54f,.01f,-4.16f),new Vector3(5.54f,.01f,4.16f),new Vector3(-5.54f,.01f,4.16f),new Vector3(-5.54f,.01f,-4.16f)},trim,.035f);
            for(int i=0;i<8;i++)
            {
                Cube(arena,"Edge segment",new Vector3(-4.9f+i*1.4f,.004f,-4.25f),new Vector3(.46f,.015f,.06f),trim);
                Cube(arena,"Edge segment",new Vector3(-4.9f+i*1.4f,.004f,4.25f),new Vector3(.46f,.015f,.06f),trim);
            }
        }
        static void Cube(Transform parent,string label,Vector3 pos,Vector3 size,Material mat)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=label;o.transform.SetParent(parent,false);
            o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;
        }
        static Material Material(string name,Color color,bool unlit=false)
        {
            var path="Assets/Sandbox/CombatTest/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m)return m;
            m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit")){name=name,color=color};
            AssetDatabase.CreateAsset(m,path);return m;
        }
        static void Line(Transform parent,string label,Vector3[] p,Material mat,float width)
        {
            var o=new GameObject(label);o.transform.SetParent(parent,false);var line=o.AddComponent<LineRenderer>();
            line.useWorldSpace=false;line.positionCount=p.Length;line.SetPositions(p);line.sharedMaterial=mat;
            line.startWidth=line.endWidth=width;line.shadowCastingMode=ShadowCastingMode.Off;
        }
        static void Sun(string name,Color color,float strength,Vector3 rotation)
        {
            var light=new GameObject(name).AddComponent<Light>();light.type=LightType.Directional;light.color=color;
            light.intensity=strength;light.transform.rotation=Quaternion.Euler(rotation);light.shadows=LightShadows.Soft;
        }
    }
}

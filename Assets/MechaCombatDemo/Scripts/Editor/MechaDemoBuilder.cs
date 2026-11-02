using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MechaDemo.Editor
{
    public static class MechaDemoBuilder
    {
        public const string Root="Assets/MechaCombatDemo";
        static Material floor,metal,cyan,orange,white,soft,sparks;
        [MenuItem("Mecha Demo/Build Combat Test Scene")]
        public static void Build()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode before building scene.");
            EnsureFolders();MakeMaterials();SetLayers();
            var current=SceneManager.GetActiveScene();
            if(current.isDirty)EditorSceneManager.SaveScene(current,Root+"/Scenes/WorkingSceneSnapshot.unity",true);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Unicorn_CombatRig.prefab");
            GameObject actor;
            if(prefab!=null)actor=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            else actor=MechaRigBuilder.Build(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab"),Root);
            if(PrefabUtility.IsPartOfPrefabInstance(actor))PrefabUtility.UnpackPrefabInstance(actor,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            actor.name="Unicorn_Player";actor.transform.position=Vector3.zero;
            SetLayer(actor,9);
            var rig=actor.GetComponent<MechaRigData>();rig.drawJointGizmos=false;rig.ResetPose();
            rig.swordSocket.localRotation=Quaternion.Euler(45,0,0);rig.gunSocket.localRotation=Quaternion.Euler(80,0,0);
            var animator=rig.visualRoot.GetComponent<Animator>();animator.runtimeAnimatorController=MechaAnimationBuilder.Build(rig.visualRoot.gameObject,Root);animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var health=actor.GetComponent<MechaHealth>();if(health==null)health=actor.AddComponent<MechaHealth>();health.playerFaction=true;health.maxHealth=1000;health.currentHealth=1000;health.displayName="UNICORN";health.showWorldHealth=false;
            var controller=actor.GetComponent<CharacterController>();if(controller==null)controller=actor.AddComponent<CharacterController>();controller.height=2.45f;controller.radius=.38f;controller.center=Vector3.up*1.225f;controller.stepOffset=.25f;controller.slopeLimit=45;controller.skinWidth=.045f;
            Transform sword=new GameObject("BeamSaber_Prototype").transform;sword.SetParent(rig.swordSocket,false);
            Primitive("SaberGrip",PrimitiveType.Cylinder,sword,Vector3.zero,new Vector3(.052f,.14f,.052f),metal,Quaternion.Euler(90,0,0));
            Primitive("Emitter",PrimitiveType.Cylinder,sword,new Vector3(0,0,.14f),new Vector3(.075f,.045f,.075f),metal,Quaternion.Euler(90,0,0));
            Primitive("EnergyBlade",PrimitiveType.Cylinder,sword,new Vector3(0,0,.88f),new Vector3(.028f,.725f,.028f),cyan,Quaternion.Euler(90,0,0));
            var bladeBase=Node("BladeBase",sword,new Vector3(0,0,.15f));var bladeTip=Node("BladeTip",sword,new Vector3(0,0,1.60f));
            var trail=bladeTip.gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=soft;trail.time=.10f;trail.startWidth=.11f;trail.endWidth=0;trail.minVertexDistance=.025f;trail.emitting=false;trail.numCornerVertices=3;trail.numCapVertices=3;trail.shadowCastingMode=ShadowCastingMode.Off;trail.startColor=new Color(.4f,.9f,1,.8f);trail.endColor=new Color(.1f,.5f,1,0);
            Transform gun=new GameObject("PulseEmitter_Prototype").transform;gun.SetParent(rig.gunSocket,false);
            Primitive("GunBody",PrimitiveType.Cube,gun,new Vector3(0,0,.27f),new Vector3(.15f,.19f,.70f),metal);
            Primitive("GunBarrel",PrimitiveType.Cylinder,gun,new Vector3(0,0,.64f),new Vector3(.045f,.17f,.045f),metal,Quaternion.Euler(90,0,0));
            Primitive("GunCore",PrimitiveType.Cube,gun,new Vector3(0,.105f,.35f),new Vector3(.055f,.025f,.32f),cyan);
            var muzzle=Node("Muzzle",gun,new Vector3(0,0,.83f));
            var effects=actor.GetComponent<MechaEffects>();if(effects==null)effects=actor.AddComponent<MechaEffects>();effects.energyMaterial=white;effects.coreMaterial=cyan;effects.softMaterial=soft;effects.sparkMaterial=sparks;effects.nozzles=rig.thrusterSockets;effects.bladeTip=bladeTip;effects.bladeBase=bladeBase;effects.bladeVisual=sword.Find("EnergyBlade");effects.muzzle=muzzle;effects.chargeOrigin=muzzle;effects.swordTrail=trail;
            var tuning=AssetDatabase.LoadAssetAtPath<MechaTuning>(Root+"/CombatTuning.asset");if(tuning==null){tuning=ScriptableObject.CreateInstance<MechaTuning>();AssetDatabase.CreateAsset(tuning,Root+"/CombatTuning.asset");}
            var player=actor.GetComponent<MechaPlayer>();if(player==null)player=actor.AddComponent<MechaPlayer>();player.animator=animator;player.bladeBase=bladeBase;player.bladeTip=bladeTip;player.muzzle=muzzle;player.effects=effects;player.thrusters=rig.thrusterSockets;player.tuning=tuning;player.environmentMask=1<<8;player.hurtboxMask=1<<10;
            MakeArena();
            GameObject dummy=MakeTarget(new Vector3(0,0,4.2f),"DUMMY // MULTI-HURTBOX",false,player);
            MakeTarget(new Vector3(7,0,9),"SPARRING UNIT",true,player);
            var cameraGo=new GameObject("Combat Camera");cameraGo.tag="MainCamera";var camera=cameraGo.AddComponent<Camera>();camera.nearClipPlane=.08f;camera.farClipPlane=160;camera.backgroundColor=new Color(.075f,.12f,.19f);camera.clearFlags=CameraClearFlags.SolidColor;cameraGo.AddComponent<AudioListener>();
            var cameraData=camera.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=true;cameraData.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var director=cameraGo.AddComponent<MechaCamera>();director.follow=actor.transform;director.player=player;director.environmentMask=1<<8;director.Snap();player.viewCamera=camera;
            MakeLighting();
            AnimatorControllerPreview(animator,"Idle",0);
            PrefabUtility.SaveAsPrefabAsset(actor,Root+"/Unicorn_CombatPlayer.prefab");
            EditorSceneManager.SaveScene(scene,Root+"/Scenes/MechaCombatTest.unity");
            var buildScenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);if(!buildScenes.Any(s=>s.path==Root+"/Scenes/MechaCombatTest.unity"))buildScenes.Add(new EditorBuildSettingsScene(Root+"/Scenes/MechaCombatTest.unity",true));EditorBuildSettings.scenes=buildScenes.ToArray();
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Selection.activeGameObject=actor;
            File.WriteAllText(Root+"/README.md",Readme());
        }
        public static void AnimatorControllerPreview(Animator animator,string state,float time)
        {
            foreach(var clip in animator.runtimeAnimatorController.animationClips)
                if(clip.name==state){clip.SampleAnimation(animator.gameObject,time);return;}
        }
        static void EnsureFolders(){foreach(string f in new[]{"Materials","Textures","Scenes","Reports","Animation"})Directory.CreateDirectory(Root+"/"+f);AssetDatabase.Refresh();}
        static void SetLayers()
        {
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=settings.FindProperty("layers");
            string[] names={"MechaGround","MechaPlayer","MechaTarget","MechaVFX"};
            for(int i=0;i<4;i++){var p=layers.GetArrayElementAtIndex(8+i);if(string.IsNullOrEmpty(p.stringValue)||p.stringValue==names[i])p.stringValue=names[i];}
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetLayer(GameObject g,int layer){g.layer=layer;foreach(Transform c in g.transform)SetLayer(c.gameObject,layer);}
        static Material Mat(string name,Color color,bool emission=false)
        {
            string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",.5f);m.SetFloat("_Smoothness",.48f);
            if(emission){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;m.SetColor("_EmissionColor",new Color(color.r*8,color.g*8,color.b*8,1));m.EnableKeyword("_EMISSION");}
            EditorUtility.SetDirty(m);return m;
        }
        static Material ParticleMat(string name,Color color)
        {
            string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Surface",1);m.SetFloat("_Blend",2);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.One);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;
            string texturePath=Root+"/Textures/SoftParticle.asset";var t=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(t==null){t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.name="SoftRadial";t.wrapMode=TextureWrapMode.Clamp;for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;float a=Mathf.Pow(Mathf.Clamp01(1-d),2);t.SetPixel(x,y,new Color(1,1,1,a));}t.Apply();AssetDatabase.CreateAsset(t,texturePath);}
            m.SetTexture("_BaseMap",t);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",t);EditorUtility.SetDirty(m);return m;
        }
        static void MakeMaterials()
        {
            floor=Mat("ArenaGraphite",new Color(.045f,.065f,.09f));metal=Mat("Gunmetal",new Color(.07f,.11f,.16f));cyan=Mat("EnergyCyan",new Color(.08f,.65f,1),true);orange=Mat("TargetOrange",new Color(.7f,.15f,.04f),true);white=ParticleMat("EnergyParticle",new Color(1.4f,1.8f,2,1));soft=ParticleMat("SoftEnergy",new Color(2,2,2,1));sparks=ParticleMat("ImpactSpark",Color.white);
        }
        static Transform Node(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        static GameObject Primitive(string n,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Material mat,Quaternion rotation=default(Quaternion),bool collider=false)
        {
            var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.transform.localRotation=rotation==default(Quaternion)?Quaternion.identity:rotation;g.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        static void MakeArena()
        {
            var arena=new GameObject("COMBAT LAB // test fixtures");
            var ground=Primitive("Ground 40m",PrimitiveType.Cube,arena.transform,new Vector3(0,-.16f,0),new Vector3(40,.3f,40),floor,default(Quaternion),true);ground.layer=8;
            for(int i=-20;i<=20;i++)
            {
                if(i%5!=0)continue;float w=.025f;
                Primitive("GridX_"+i,PrimitiveType.Cube,arena.transform,new Vector3(i,.002f,0),new Vector3(w,.004f,40),i%5==0?cyan:metal).GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
                Primitive("GridZ_"+i,PrimitiveType.Cube,arena.transform,new Vector3(0,.002f,i),new Vector3(40,.004f,w),i%5==0?cyan:metal).GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
            Wall(arena.transform,"Wall // dash stop",new Vector3(-6,1.5f,6),new Vector3(.3f,3,8));
            Wall(arena.transform,"Corner wall",new Vector3(-9,1.5f,10),new Vector3(6,3,.3f));
            Wall(arena.transform,"Thin obstacle",new Vector3(5,1.3f,-5),new Vector3(5,2.6f,.12f));
            Wall(arena.transform,"Low ceiling",new Vector3(-7,3.3f,-6),new Vector3(5,.3f,4));
            var ramp=Wall(arena.transform,"Ramp 25deg",new Vector3(10,.65f,-4),new Vector3(4,.3f,5));ramp.transform.rotation=Quaternion.Euler(-25,0,0);
            var steep=Wall(arena.transform,"Slope exceeds limit",new Vector3(14,1.2f,5),new Vector3(3,.3f,4));steep.transform.rotation=Quaternion.Euler(-60,0,0);
            for(int s=0;s<3;s++)Wall(arena.transform,"Step_"+s,new Vector3(10,.10f+s*.1f,2+s*.8f),new Vector3(3,.2f+s*.2f,.8f));
            foreach(float x in new[]{-20f,20f})Wall(arena.transform,"Arena boundary",new Vector3(x,1,0),new Vector3(.3f,2,40));
            foreach(float z in new[]{-20f,20f})Wall(arena.transform,"Arena boundary",new Vector3(0,1,z),new Vector3(40,2,.3f));
        }
        static GameObject Wall(Transform p,string n,Vector3 pos,Vector3 scale){var g=Primitive(n,PrimitiveType.Cube,p,pos,scale,metal,default(Quaternion),true);g.layer=8;return g;}
        static GameObject MakeTarget(Vector3 position,string name,bool ai,MechaPlayer player)
        {
            var g=new GameObject(name);g.transform.position=position;g.layer=10;
            var health=g.AddComponent<MechaHealth>();health.displayName=name;health.trainingDummy=!ai;health.maxHealth=ai?1800:5000;health.currentHealth=health.maxHealth;
            var controller=g.AddComponent<CharacterController>();controller.height=2.35f;controller.center=Vector3.up*1.175f;controller.radius=.43f;controller.stepOffset=.22f;controller.slopeLimit=45;
            Primitive("Torso",PrimitiveType.Cube,g.transform,new Vector3(0,1.45f,0),new Vector3(.72f,.85f,.42f),metal);
            Primitive("Core",PrimitiveType.Cube,g.transform,new Vector3(0,1.5f,-.23f),new Vector3(.3f,.3f,.07f),orange);
            Primitive("Head",PrimitiveType.Cube,g.transform,new Vector3(0,2.1f,0),new Vector3(.38f,.35f,.34f),metal);
            Primitive("Visor",PrimitiveType.Cube,g.transform,new Vector3(0,2.12f,-.18f),new Vector3(.3f,.05f,.03f),orange);
            foreach(float x in new[]{-.27f,.27f}){Primitive("Leg",PrimitiveType.Cube,g.transform,new Vector3(x,.5f,0),new Vector3(.23f,.95f,.28f),metal);var arm=Primitive("HurtboxArm",PrimitiveType.Cube,g.transform,new Vector3(x*2,1.4f,0),new Vector3(.23f,.75f,.28f),metal,default(Quaternion),true);arm.layer=10;}
            if(ai){var enemy=g.AddComponent<MechaEnemy>();enemy.player=player;enemy.environmentMask=1<<8;enemy.detectionRange=8;enemy.attackRange=2.7f;enemy.moveSpeed=2.4f;}
            return g;
        }
        static void MakeLighting()
        {
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.32f,.4f,.5f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.025f,.045f,.075f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.012f;
            var sun=new GameObject("Key Light");sun.transform.rotation=Quaternion.Euler(45,-35,0);var l=sun.AddComponent<Light>();l.type=LightType.Directional;l.color=new Color(.82f,.92f,1);l.intensity=1.45f;l.shadows=LightShadows.Soft;l.shadowBias=.08f;l.shadowNormalBias=.6f;
            var rim=new GameObject("Warm Rim");rim.transform.rotation=Quaternion.Euler(20,140,0);var r=rim.AddComponent<Light>();r.type=LightType.Directional;r.color=new Color(.4f,.65f,1);r.intensity=.6f;
            var volumeGo=new GameObject("Combat Post FX");var volume=volumeGo.AddComponent<Volume>();volume.isGlobal=true;string path=Root+"/Materials/CombatVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            if(!profile.TryGet<Bloom>(out var bloom))bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.35f);bloom.threshold.Override(1.25f);bloom.scatter.Override(.55f);
            if(!profile.TryGet<Tonemapping>(out var tone))tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);volume.sharedProfile=profile;if(!AssetDatabase.Contains(bloom))AssetDatabase.AddObjectToAsset(bloom,profile);if(!AssetDatabase.Contains(tone))AssetDatabase.AddObjectToAsset(tone,profile);EditorUtility.SetDirty(profile);
        }
        public static void Capture(Camera camera,string file,int width=1280,int height=800)
        {
            var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
        static string Readme(){return "# Unicorn Combat Demo\n\nOpen Scenes/MechaCombatTest.unity then Play.\n\nWASD run; Alt walk; Shift boost; Ctrl dash; Space jump/hold hover; C descend; LMB combo; RMB gun; MMB lock; Q slash; E charge beam; R ultimate; F1 help; F5 reset; Esc pause.\n\n375 source meshes use a mechanical rigid-part skeleton with named joints, no soft skin deformation. Original OBJ/prefab/scene preserved. RigMapping and clip sheets document authored paths and timing.\n\nWeapons, target/enemy and synthesized sound are prototype assets. Gun is hitscan with visible beam bolt. Native particle/line/trail effects target URP. Tune CombatTuning.asset. Camera is native custom controller, no Cinemachine dependency. AI is a simple collision-aware sparring proxy, not full pathfinding. Final visual animation polish still requires reviewing clips and extreme armor clearance.\n";}
    }
}






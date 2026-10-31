using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GundamGame.CombatDemo.Editor
{
    public static class RougeCombatAuthoring
    {
        const string AnimationFolder="Assets/Game/Features/Combat/Animations/StrikeRouge";
        static Transform Find(Transform root,string token) => root.GetComponentsInChildren<Transform>(true).First(t=>t.name.StartsWith(token,StringComparison.Ordinal) || t.name.EndsWith(token,StringComparison.Ordinal));
        [MenuItem("Gundam/Strike Rouge/Upgrade Skills and Source Animations")]
        public static void Upgrade()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before upgrading the scene.");
            Directory.CreateDirectory(AnimationFolder);
            string backups="Assets/Sandbox/StrikeRougeCombat/PreservedScenes";Directory.CreateDirectory(backups);
            var current=EditorSceneManager.GetActiveScene();
            if(current.isDirty && !EditorSceneManager.SaveScene(current,backups+"/BeforeSkillUpgrade_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve the current scene.");
            var scene=EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
            if(!EditorSceneManager.SaveScene(scene,backups+"/CombatBeforeUpgrade_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve the combat scene.");
            var battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
            if(battle==null)throw new InvalidOperationException("Battle controller missing.");
            var sourceClips=AssetDatabase.LoadAllAssetsAtPath(RougeSkillSetup.ModelPath).OfType<AnimationClip>().ToArray();
            AnimationClip Clip(string name)=>sourceClips.First(c=>c.name==name);
            var generated=Generate(Clip("O-02-IDLE"));AssetDatabase.SaveAssets();
            var model=battle.player.visual.GetChild(0);
            foreach(var a in model.GetComponentsInChildren<Animator>(true))a.enabled=false;
            var animator=model.GetComponent<Animator>();if(animator==null)animator=model.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController=null;animator.enabled=true;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var driver=battle.player.GetComponent<RougeAnimationDriver>();if(driver==null)driver=battle.player.gameObject.AddComponent<RougeAnimationDriver>();
            driver.rig=animator;driver.idle=Clip("O-02-IDLE");driver.arrival=Clip("O-01-APPER");driver.launcher=Clip("O-03-LAUNCHER");driver.separation=Clip("O-04-SEPARATION");
            driver.rifle=generated[0];driver.blade=generated[1];driver.barrage=generated[2];driver.guard=generated[3];driver.dodge=generated[4];
            battle.player.lowerImportedArmsAtRest=false;
            var presentation=battle.GetComponent<RougeSkillPresentation>();if(presentation==null)presentation=battle.gameObject.AddComponent<RougeSkillPresentation>();
            presentation.battle=battle;presentation.animationRig=driver;
            presentation.rifleMuzzle=Socket(Find(model,"_048"),"Rifle Muzzle",new Vector3(-.725f,.075f,0));
            presentation.beamMuzzle=Socket(Find(model,"_037"),"Beam Launcher Muzzle",new Vector3(-.02f,.095f,-1.19f));
            presentation.railMuzzle=Socket(Find(model,"_039"),"Railgun Muzzle",new Vector3(.02f,.095f,-1.19f));
            presentation.swordBase=Socket(Find(model,"_046"),"Blade Base",new Vector3(0,.15f,0));
            presentation.swordTip=Socket(Find(model,"_046"),"Blade Tip",new Vector3(0,1.89f,0));
            presentation.core=Socket(Find(model,"chest_"),"Core Glow",new Vector3(0,.04f,-.12f));
            presentation.shield=Socket(Find(model,"lower_arm.L"),"Guard Impact",new Vector3(0,.12f,-.12f));
            presentation.missilePorts=new[]{Socket(Find(model,"_035"),"Missile Port A",new Vector3(-.02f,.12f,.62f)),Socket(Find(model,"_044"),"Missile Port B",new Vector3(0,.73f,.02f))};
            presentation.virtualCamera=GameObject.Find("Virtual Battle Camera").GetComponent<Camera>();
            presentation.placement=UnityEngine.Object.FindAnyObjectByType<RougeArenaPlacement>();
            presentation.glowShader=Shader.Find("Gundam/AR Skill Glow");presentation.solidShader=Shader.Find("Universal Render Pipeline/Unlit");
            if(presentation.glowShader==null)throw new InvalidOperationException("AR Skill Glow shader was not imported.");
            battle.presentation=presentation;battle.player.muzzle=presentation.rifleMuzzle;
            ConfigureCameras(presentation);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Selection.activeGameObject=battle.gameObject;
            Directory.CreateDirectory(RougeSkillSetup.ReportFolder);
            File.WriteAllLines(RougeSkillSetup.ReportFolder+"/SkillAnimationMapping.txt",new[]{
                "Arrival: imported O-01-APPER, 3x playback + board-local descent",
                "Idle: imported O-02-IDLE, looping manual PlayableGraph",
                "Ultimate: imported O-03-LAUNCHER, 2x playback; railgun + sustained beam",
                "Available, preserved: imported O-04-SEPARATION (not forced into a repeating combat action)",
                "Rifle / blade / barrage / guard / dodge: authored Generic AnimationClips for the exact source rig",
                "Source .glb/.fbx and original Animator Controller are preserved.",
                "Generated combat assets: "+AnimationFolder,
                "Scene: "+RougeCombatSceneBuilder.ScenePath});
            Debug.Log("Strike Rouge skill upgrade complete.");
        }
        [MenuItem("Gundam/Strike Rouge/Upgrade Camera and Ultimate Focus")]
        public static void UpgradeCameras()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before upgrading cameras.");
            string backups="Assets/Sandbox/StrikeRougeCombat/PreservedScenes";Directory.CreateDirectory(backups);
            var current=EditorSceneManager.GetActiveScene();
            if(current.isDirty && !EditorSceneManager.SaveScene(current,backups+"/BeforeCameraChange_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve the current scene.");
            var scene=current.path==RougeCombatSceneBuilder.ScenePath?current:EditorSceneManager.OpenScene(RougeCombatSceneBuilder.ScenePath);
            if(!EditorSceneManager.SaveScene(scene,backups+"/CombatBeforeCamera_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true))throw new IOException("Cannot preserve the combat scene.");
            var battle=UnityEngine.Object.FindAnyObjectByType<RougeBattleDemo>();
            ConfigureCameras(battle.presentation);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("Rouge orbit camera and character-focused ultimate ready.");
        }
        static void ConfigureCameras(RougeSkillPresentation presentation)
        {
            var orbit=presentation.virtualCamera.GetComponent<RougeArenaCamera>();if(orbit==null)orbit=presentation.virtualCamera.gameObject.AddComponent<RougeArenaCamera>();
            orbit.arenaRoot=presentation.battle.arenaRoot;orbit.presentation=presentation;orbit.placement=presentation.placement;
            presentation.arenaCamera=orbit;
            var director=presentation.GetComponent<RougeCinematicDirector>();if(director==null)director=presentation.gameObject.AddComponent<RougeCinematicDirector>();
            presentation.cinematicDirector=director;
        }
        static Transform Socket(Transform parent,string name,Vector3 position)
        {
            var found=parent.Find(name);if(found==null){found=new GameObject(name).transform;found.SetParent(parent,false);}found.localPosition=position;found.localRotation=Quaternion.identity;return found;
        }

        struct Rest { public Transform t;public Vector3 p;public Quaternion q;public Vector3 s; }
        static AnimationClip[] Generate(AnimationClip idle)
        {
            var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(RougeSkillSetup.ModelPath));root.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                foreach(var a in root.GetComponentsInChildren<Animator>(true))a.enabled=false;
                idle.SampleAnimation(root,2);
                var nodes=root.GetComponentsInChildren<Transform>(true).Where(t=>t!=root.transform).ToArray();
                var rest=nodes.Select(t=>new Rest{t=t,p=t.localPosition,q=t.localRotation,s=t.localScale}).ToArray();
                var left=Find(root.transform,"hand.L");var right=Find(root.transform,"hand.R");var rifle=Find(root.transform,"_048");var sword=Find(root.transform,"_046");
                Vector3 rifleOffset=left.InverseTransformPoint(rifle.position),swordOffset=right.InverseTransformPoint(sword.position);
                Quaternion rifleRotation=Quaternion.Inverse(left.rotation)*rifle.rotation,swordRotation=Quaternion.Inverse(right.rotation)*sword.rotation;
                var upperL=Find(root.transform,"upper_arm.L");var lowerL=Find(root.transform,"lower_arm.L");var upperR=Find(root.transform,"upper_arm.R");var lowerR=Find(root.transform,"lower_arm.R");
                var chest=Find(root.transform,"chest_");var hips=Find(root.transform,"hips_");
                var result=new List<AnimationClip>();
                string[] names={"Rouge_RifleBurst","Rouge_AntiShipBlade","Rouge_MissileBackflip","Rouge_Guard","Rouge_Dodge"};
                float[] lengths={1.5f,1.85f,2f,.65f,.65f};
                for(int action=0;action<names.Length;action++)
                {
                    var clip=new AnimationClip{name=names[action],frameRate=30};
                    int frames=Mathf.CeilToInt(lengths[action]*30);
                    var values=new Dictionary<Transform,List<Keyframe>[]>();foreach(var t in nodes)values[t]=Enumerable.Range(0,7).Select(_=>new List<Keyframe>()).ToArray();
                    for(int frame=0;frame<=frames;frame++)
                    {
                        float time=lengths[action]*frame/frames;
                        foreach(var pose in rest){pose.t.localPosition=pose.p;pose.t.localRotation=pose.q;pose.t.localScale=pose.s;}
                        float envelope=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/.22f))*Mathf.SmoothStep(0,1,Mathf.Clamp01((lengths[action]-time)/.2f));
                        if(action==0)
                        {
                            chest.rotation=Quaternion.AngleAxis(-12*envelope,Vector3.up)*chest.rotation;
                            float recoil=0;foreach(float shot in new[]{.31f,.62f,.93f})recoil+=Mathf.Max(0,1-Mathf.Abs(time-shot)/.1f)*.055f;
                            SolveArm(upperL,lowerL,left,Vector3.Lerp(left.position,new Vector3(-.2f,1.35f,.46f-recoil),envelope),new Vector3(-1,1.2f,-.1f));
                            Follow(rifle,left,rifleOffset,rifleRotation);
                            left.rotation=Quaternion.Slerp(left.rotation,Quaternion.FromToRotation(-rifle.right,Vector3.forward)*left.rotation,envelope);
                        }
                        else if(action==1)
                        {
                            float[] times={0,.25f,.47f,.67f,.84f,1.02f,1.16f,1.48f,1.85f};
                            Vector3[] hands={right.position,new Vector3(.15f,1.92f,-.16f),new Vector3(.05f,1.88f,.05f),new Vector3(-.24f,1.12f,.58f),new Vector3(.2f,1.5f,.2f),new Vector3(-.08f,1.8f,.2f),new Vector3(.4f,1.08f,.55f),right.position,right.position};
                            Vector3[] aims={sword.up,new Vector3(.1f,.8f,-.6f),new Vector3(.1f,.8f,-.5f),new Vector3(-.65f,-.25f,1),new Vector3(.5f,.7f,.1f),new Vector3(-.7f,.6f,.1f),new Vector3(.8f,-.3f,.8f),sword.up,sword.up};
                            float yaw=CurveAt(times,new[]{0f,-35,-32,45,15,35,-40,0,0},time);
                            chest.rotation=Quaternion.AngleAxis(yaw,Vector3.up)*chest.rotation;
                            SolveArm(upperR,lowerR,right,VectorAt(times,hands,time),new Vector3(.8f,1.5f,-.3f));
                            Follow(sword,right,swordOffset,swordRotation);
                            right.rotation=Quaternion.FromToRotation(sword.up,VectorAt(times,aims,time).normalized)*right.rotation;
                            Follow(sword,right,swordOffset,swordRotation);
                            Vector3 support=sword.position-sword.up*.12f;
                            SolveArm(upperL,lowerL,left,Vector3.Lerp(left.position,support,envelope),new Vector3(-.7f,1.3f,0));
                        }
                        else if(action==2 || action==4)
                        {
                            hips.localPosition-=Vector3.up*(action==2?.1f:.09f)*envelope;
                            chest.localRotation*=Quaternion.Euler(14*envelope,0,action==4?-15*envelope:0);
                            Find(root.transform,"upper_leg.L").localRotation*=Quaternion.Euler(26*envelope,0,0);
                            Find(root.transform,"upper_leg.R").localRotation*=Quaternion.Euler(26*envelope,0,0);
                            Find(root.transform,"lower_leg.L").localRotation*=Quaternion.Euler(-35*envelope,0,0);
                            Find(root.transform,"lower_leg.R").localRotation*=Quaternion.Euler(-35*envelope,0,0);
                        }
                        else if(action==3)
                        {
                            SolveArm(upperL,lowerL,left,Vector3.Lerp(left.position,new Vector3(-.08f,1.55f,.39f),envelope),new Vector3(-.7f,1.3f,0));
                            chest.rotation=Quaternion.AngleAxis(-15*envelope,Vector3.up)*chest.rotation;
                        }
                        Follow(sword,right,swordOffset,swordRotation);
                        if(action==1 && envelope>.01f)
                        {
                            Follow(rifle,left,rifleOffset,rifleRotation);
                            rifle.position=Vector3.Lerp(rifle.position,new Vector3(-.33f,1.3f,-.28f),envelope);
                            rifle.rotation=Quaternion.Slerp(rifle.rotation,Quaternion.LookRotation(Vector3.down,Vector3.back),envelope);
                        }
                        else Follow(rifle,left,rifleOffset,rifleRotation);
                        foreach(var t in nodes)
                        {
                            var p=t.localPosition;var q=t.localRotation;float[] sample={p.x,p.y,p.z,q.x,q.y,q.z,q.w};
                            for(int k=0;k<7;k++)values[t][k].Add(new Keyframe(time,sample[k]));
                        }
                    }
                    foreach(var pair in values)
                    {
                        string path=AnimationUtility.CalculateTransformPath(pair.Key,root.transform);
                        string[] properties={"m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z","m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};
                        for(int k=0;k<7;k++)
                        {
                            var keys=pair.Value[k];
                            bool constant=keys.All(key=>Mathf.Abs(key.value-keys[0].value)<.00001f);
                            var curve=constant?new AnimationCurve(new Keyframe(0,keys[0].value),new Keyframe(lengths[action],keys[0].value)):new AnimationCurve(keys.ToArray());
                            for(int n=0;n<curve.length;n++){AnimationUtility.SetKeyLeftTangentMode(curve,n,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,n,AnimationUtility.TangentMode.Linear);}
                            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),properties[k]),curve);
                        }
                    }
                    clip.EnsureQuaternionContinuity();
                    string clipPath=AnimationFolder+"/"+names[action]+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if(existing==null){AssetDatabase.CreateAsset(clip,clipPath);result.Add(clip);}else{EditorUtility.CopySerialized(clip,existing);EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(clip);result.Add(existing);}
                }
                return result.ToArray();
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static void Follow(Transform weapon,Transform hand,Vector3 p,Quaternion q){weapon.SetPositionAndRotation(hand.TransformPoint(p),hand.rotation*q);}
        static float CurveAt(float[] times,float[] values,float time)
        {
            for(int i=1;i<times.Length;i++)if(time<=times[i])return Mathf.Lerp(values[i-1],values[i],Mathf.SmoothStep(0,1,Mathf.InverseLerp(times[i-1],times[i],time)));return values[values.Length-1];
        }
        static Vector3 VectorAt(float[] times,Vector3[] values,float time)
        {
            for(int i=1;i<times.Length;i++)if(time<=times[i])return Vector3.Lerp(values[i-1],values[i],Mathf.SmoothStep(0,1,Mathf.InverseLerp(times[i-1],times[i],time)));return values[values.Length-1];
        }
        static void SolveArm(Transform upper,Transform lower,Transform hand,Vector3 target,Vector3 pole)
        {
            float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);
            Vector3 direction=target-upper.position;float distance=Mathf.Clamp(direction.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);direction.Normalize();
            Vector3 bend=Vector3.ProjectOnPlane(pole-upper.position,direction).normalized;if(bend.sqrMagnitude<.01f)bend=Vector3.right;
            float along=(a*a-b*b+distance*distance)/(2*distance),height=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            Vector3 elbow=upper.position+direction*along+bend*height;
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            Vector3 end=upper.position+direction*distance;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,end-lower.position)*lower.rotation;
        }
    }
}

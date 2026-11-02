using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MechaDemo.Editor
{
    /// <summary>Authored mechanical key poses and baked foot-contact trajectories. Generated assets are tool-owned.</summary>
    public static class MechaAnimationBuilder
    {
        static readonly string[] BoneNames = { "Pelvis", "Spine", "Chest", "Neck", "Head", "Shoulder_L", "Shoulder_R", "UpperArm_L", "UpperArm_R", "LowerArm_L", "LowerArm_R", "Hand_L", "Hand_R", "UpperLeg_L", "UpperLeg_R", "LowerLeg_L", "LowerLeg_R", "Foot_L", "Foot_R", "FrontSkirt_L", "FrontSkirt_R", "SideSkirt_L", "SideSkirt_R", "RearSkirt_L", "RearSkirt_R" };
        static Dictionary<string, Transform> bones;
        static Dictionary<string, Vector3> restWorld;
        static Dictionary<string, Quaternion> restRotation;
        static GameObject visual;
        static StringBuilder sheets;
        static string clipsFolder;

        sealed class Pose
        {
            public Dictionary<string, Quaternion> rotations = new Dictionary<string, Quaternion>();
            public Vector3 pelvisOffset;
            public Pose R(string name, float x, float y = 0, float z = 0) { rotations[name] = Quaternion.Euler(x, y, z); return this; }
            public Pose Q(string name, Quaternion value) { rotations[name] = value; return this; }
            public Pose Copy() { var p = new Pose { pelvisOffset = pelvisOffset }; foreach (var kv in rotations) p.rotations[kv.Key] = kv.Value; return p; }
        }
        struct Mark
        {
            public float time; public Pose pose;
            public Mark(float t, Pose p) { time = t; pose = p; }
        }
        static Mark K(float t, Pose p) { return new Mark(t, p); }
        static Transform Find(string n) { return visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == n); }

        public static float NominalSpeed(string state)
        {
            switch (state)
            {
                case "WalkF": return 1.8f / .9f;
                case "WalkB": return 1.4f / .9f;
                case "WalkL": case "WalkR": return 1f / .9f;
                case "RunF": return 2.6f / .66f;
                case "RunB": return 2f / .66f;
                case "RunL": case "RunR": return 1.2f / .66f;
                default: return 0;
            }
        }

        public static RuntimeAnimatorController Build(GameObject visualRoot, string assetRoot)
        {
            visual = visualRoot;
            bones = new Dictionary<string, Transform>();
            restWorld = new Dictionary<string, Vector3>();
            restRotation = new Dictionary<string, Quaternion>();
            foreach (string n in BoneNames)
            {
                var t = Find(n); if (t == null) continue;
                bones.Add(n, t); restWorld.Add(n, visual.transform.InverseTransformPoint(t.position)); restRotation.Add(n, t.localRotation);
            }
            foreach (string required in new[] { "Pelvis", "Chest", "UpperArm_L", "UpperArm_R", "LowerArm_L", "LowerArm_R", "UpperLeg_L", "UpperLeg_R", "LowerLeg_L", "LowerLeg_R", "Foot_L", "Foot_R" })
                if (!bones.ContainsKey(required)) throw new InvalidOperationException("Missing rig joint for animation: " + required);

            clipsFolder = assetRoot.TrimEnd('/') + "/Animations";
            Directory.CreateDirectory(clipsFolder);
            AssetDatabase.Refresh();
            sheets = new StringBuilder("state,duration_seconds,loop,nominal_root_speed,notes\n");
            var clips = new Dictionary<string, AnimationClip>();
            Action<string, float, bool, string, Mark[]> add = (name, duration, loop, notes, keys) => clips.Add(name, Clip(name, duration, loop, notes, keys));

            var ready = Ready();
            add("Idle", 2.4f, true, "Combat ready; tiny chest settling; both feet planted", new[] {
                K(0,ready), K(.6f, ready.Copy().R("Chest",1,2,0).R("Head",-1,-2,0)),
                K(1.2f,ready.Copy().R("Chest",-1,1,0).R("LowerArm_R",-63)), K(1.8f,ready.Copy().R("Chest",0,2,0)), K(2.4f,ready) });

            foreach (string direction in new[] { "F", "B", "L", "R" })
            {
                clips.Add("Walk" + direction, Gait("Walk" + direction, false, direction));
                clips.Add("Run" + direction, Gait("Run" + direction, true, direction));
            }

            var boost = FreeLegPose(ready.Copy(), 16, 40, 12).R("Spine",13).R("Chest",-3,1).R("Head",-9,-1).R("UpperArm_R",-36,0,14).R("LowerArm_R",-70);
            add("Boost",1,true,"Jet glide; no ground walking cycle",new[]{K(0,boost),K(.5f,boost.Copy().R("Chest",-2,1).R("LowerLeg_L",43).R("LowerLeg_R",39)),K(1,boost)});
            add("BoostStart",.16f,false,"Ready to low jet-glide",new[]{K(0,ready),K(.06f,Brace(0,0,.12f).R("Spine",10)),K(.16f,boost)});
            add("BoostStop",.25f,false,"Low brace then ready",new[]{K(0,boost),K(.1f,Brace(0,0,.15f).R("Spine",-6)),K(.25f,ready)});
            foreach (string d in new[] { "F","B","L","R" })
            {
                float roll = d == "L" ? 14 : d == "R" ? -14 : 0;
                float pitch = d == "B" ? -10 : 10;
                var dash = FreeLegPose(ready.Copy(), 10,32,8).R("Spine",pitch,0,roll).R("Head",-pitch * .6f,0,-roll * .6f);
                add("Dash"+d,.22f,false,"0-.04 load; .04-.17 jet drive; .17-.22 brake; motor owns displacement",new[]{K(0,ready),K(.035f,Brace(0,0,.09f).R("Spine",pitch*.5f,0,roll*.5f)),K(.07f,dash),K(.15f,dash),K(.19f,Brace(0,0,.10f)),K(.22f,ready)});
            }
            add("Start",.18f,false,"Short anticipatory load then forward stride",new[]{K(0,ready),K(.055f,Brace(0,0,.08f).R("Spine",7)),K(.14f,GaitPose(.08f,false,"F",1.8f)),K(.18f,GaitPose(.12f,false,"F",1.8f))});
            add("Stop",.25f,false,"Foot brace; lower pelvis; chest catches up",new[]{K(0,GaitPose(.05f,false,"F",1.8f)),K(.10f,Brace(0,0,.16f).R("Spine",-5)),K(.19f,Brace(0,0,.06f)),K(.25f,ready)});
            foreach(int sign in new[]{-1,1})
                add(sign<0?"Turn180L":"Turn180R",.35f,false,"Turn load and settle; root yaw remains motor-owned",new[]{K(0,ready),K(.09f,Brace(sign*12,sign*10,.10f)),K(.20f,Brace(-sign*10,-sign*8,.12f)),K(.28f,Brace(sign*3,0,.05f)),K(.35f,ready)});

            var crouch=Brace(0,0,.17f).R("Spine",7).R("Chest",-2).R("Head",-4);
            var ascend=FreeLegPose(ready.Copy(),-18,48,-14).R("Spine",6).R("Head",-4);
            var hover=FreeLegPose(ready.Copy(),-12,40,-9).R("Spine",3).R("Chest",-1).R("Head",-2);
            var fall=FreeLegPose(ready.Copy(),-8,23,-6).R("UpperArm_L",-30,0,-17).R("UpperArm_R",-25,0,17).R("Spine",4);
            add("JumpStart",.18f,false,"Takeoff marker .14; feet load before lift",new[]{K(0,ready),K(.085f,crouch),K(.12f,crouch),K(.15f,Brace(0,0,.025f).R("Spine",4)),K(.18f,ascend)});
            add("Ascend",.45f,true,"Thruster-assisted ascent; knees tucked",new[]{K(0,ascend),K(.22f,ascend.Copy().R("LowerLeg_L",51).R("LowerLeg_R",47)),K(.45f,ascend)});
            add("Hover",1.2f,true,"Stable airborne pose with tiny settling, no large root bob",new[]{K(0,hover),K(.4f,hover.Copy().R("Chest",0,1).R("LowerLeg_R",42)),K(.8f,hover.Copy().R("Chest",-1,-1)),K(1.2f,hover)});
            add("Fall",.7f,true,"Feet prepare contact; stable upper-body weapon grip",new[]{K(0,fall),K(.35f,fall.Copy().R("LowerLeg_L",26).R("LowerLeg_R",24)),K(.7f,fall)});
            add("LandSoft",.28f,false,"Contact to shallow compression and recover",new[]{K(0,ready),K(.065f,Brace(0,0,.12f).R("Spine",5)),K(.16f,Brace(0,0,.06f).R("Spine",1)),K(.28f,ready)});
            add("LandHard",.55f,false,"Hard contact; controlled deep brace; chest lag",new[]{K(0,ready),K(.06f,Brace(0,0,.21f).R("Spine",11).R("Head",-7)),K(.15f,Brace(0,0,.20f).R("Spine",14).R("Head",-9)),K(.32f,Brace(0,0,.09f).R("Spine",4)),K(.55f,ready)});

            var a1wind=Brace(13,18,.10f,-.025f).R("UpperArm_R",-70,30,29).R("LowerArm_R",-72).R("Hand_R",0,12,-4).R("UpperArm_L",-32,0,-20);
            var a1pre=Brace(10,13,.09f,-.015f).R("UpperArm_R",-64,18,20).R("LowerArm_R",-52).R("Hand_R",2,6,-4);
            var a1hit=Brace(-6,-12,.13f,.025f).R("UpperArm_R",-32,10,-10).R("LowerArm_R",-25).R("Hand_R",12,-8,-9);
            var a1end=Brace(-11,-18,.10f,.03f).R("UpperArm_R",-5,-55,-57).R("LowerArm_R",-20).R("Hand_R",10,-8,-10);
            add("Attack1",.55f,false,"Active .12-.23; link pose .32-.40; dashcancel .25",new[]{K(0,ready),K(.06f,a1wind),K(.12f,a1pre),K(.18f,a1hit),K(.23f,a1end),K(.32f,a1end),K(.40f,a1end),K(.49f,Brace(0,0,.045f).R("UpperArm_R",-24,-12,-6).R("LowerArm_R",-61)),K(.55f,ready)});

            var a2wind=a1end.Copy().R("Pelvis",0,-14).R("Chest",0,-22).R("UpperArm_R",-17,-56,-58).R("LowerArm_R",-32).R("Hand_R",5,-8,-10);
            var a2pre=Brace(-10,-17,.10f,.02f).R("UpperArm_R",-25,-45,-35).R("LowerArm_R",-25).R("Hand_R",5);
            var a2hit=Brace(5,9,.13f,-.015f).R("UpperArm_R",-35,0,0).R("LowerArm_R",-15).R("Hand_R",5,0,6);
            var a2end=Brace(12,20,.10f,-.03f).R("UpperArm_R",-25,50,38).R("LowerArm_R",-25).R("Hand_R",5,8,4);
            var a2link=Brace(9,13,.095f,-.02f).R("UpperArm_R",-65,20,28).R("LowerArm_R",-76).R("Hand_R",-3,2,3);
            add("Attack2",.65f,false,"Active .16-.30; link .39-.48; dashcancel .32",new[]{K(0,a1end),K(.08f,a2wind),K(.16f,a2pre),K(.23f,a2hit),K(.30f,a2end),K(.39f,a2link),K(.48f,a2link),K(.57f,ready.Copy().R("UpperArm_R",-43,10,18).R("LowerArm_R",-66)),K(.65f,ready)});

            var overhead=Brace(2,4,.14f).R("UpperArm_R",-135,4,15).R("LowerArm_R",-32).R("Hand_R",-3,0,0).R("UpperArm_L",-40,-5,-15).R("Head",-6,-3);
            var overpre=overhead.Copy().R("UpperArm_R",-125,3,12).R("LowerArm_R",-34);
            var overhit=Brace(-2,-5,.20f,.02f).R("UpperArm_R",-38,-2,-2).R("LowerArm_R",-18).R("Hand_R",12,0,-3).R("Spine",10).R("Head",-8,3);
            var overend=Brace(-4,-7,.17f,.025f).R("UpperArm_R",-10,-8,-8).R("LowerArm_R",-10).R("Hand_R",16,0,-3).R("Spine",12).R("Head",-8,4);
            add("Attack3",.90f,false,"Active .28-.43; overhead silhouette; dashcancel .58",new[]{K(0,a2link),K(.14f,overhead),K(.25f,overhead),K(.28f,overpre),K(.35f,overhit),K(.43f,overend),K(.58f,overend.Copy().R("Spine",6)),K(.73f,Brace(0,0,.07f).R("UpperArm_R",-20,0,9).R("LowerArm_R",-45)),K(.90f,ready)});

            var aim=ready.Copy().R("UpperArm_L",-53,0,-10).R("LowerArm_L",-27).R("Hand_L",0,0,0).R("Chest",0,3).R("Head",0,-3);
            var recoil=aim.Copy().R("UpperArm_L",-58,0,-10).R("LowerArm_L",-30).R("Chest",-3,3).R("Head",2,-3);
            add("Aim",1,true,"Left gun socket local X=80 degrees; aim forward",new[]{K(0,aim),K(1,aim)});
            add("Fire",.20f,false,"Recoil .00-.04 then settle .16; upper-body mask supplied",new[]{K(0,aim),K(.025f,recoil),K(.06f,recoil.Copy().R("UpperArm_L",-56,0,-10)),K(.16f,aim),K(.20f,aim)});

            var qwind=Brace(10,22,.13f,-.02f).R("UpperArm_R",-72,48,30).R("LowerArm_R",-80).R("Hand_R",0,12,0).R("Spine",9);
            var qjet=FreeLegPose(qwind.Copy(),16,42,10).R("Spine",17).R("Head",-12,-12);
            var qpre=Brace(7,12,.10f).R("UpperArm_R",-66,22,15).R("LowerArm_R",-50).R("Spine",10);
            var qhit=Brace(-8,-16,.17f,.025f).R("UpperArm_R",-25,-5,-15).R("LowerArm_R",-20).R("Hand_R",10,-6,-10).R("Spine",6);
            var qend=Brace(-12,-22,.16f,.035f).R("UpperArm_R",-18,-60,-55).R("LowerArm_R",-38).R("Hand_R",8,-8,-10).R("Spine",-2);
            add("Q",.95f,false,"Load .00-.18; lunge .18-.38; active .38-.52; cancel .70",new[]{K(0,ready),K(.08f,qwind),K(.18f,qwind),K(.24f,qjet),K(.34f,qjet),K(.38f,qpre),K(.445f,qhit),K(.52f,qend),K(.70f,qend.Copy().R("Spine",-3)),K(.82f,Brace(0,0,.07f)),K(.95f,ready)});

            var ebrace=Brace(0,0,.13f).R("UpperArm_L",-53,0,-12).R("LowerArm_L",-27).R("Hand_L",0).R("UpperArm_R",-32,-8,15).R("LowerArm_R",-65).R("Spine",4).R("Head",-2);
            var echarge=ebrace.Copy().R("Chest",-1,-2).R("Head",0,2).R("UpperArm_R",-37,-5,16);
            var erecoil=ebrace.Copy().R("Spine",-5).R("Chest",-6,-2).R("UpperArm_L",-60,0,-12).R("LowerArm_L",-28).R("Head",3,2);
            add("E",1.25f,false,"Charge .00-.65; commit .45; fire .65; beam .65-.85; cancel after .95",new[]{K(0,ready),K(.20f,ebrace),K(.45f,echarge),K(.62f,echarge),K(.65f,echarge),K(.685f,erecoil),K(.76f,erecoil.Copy().R("Chest",-4,-2)),K(.85f,ebrace),K(1.02f,Brace(0,0,.08f).R("UpperArm_L",-48,0,-12).R("LowerArm_L",-32)),K(1.25f,ready)});

            var rload=Brace(10,18,.15f,-.025f).R("UpperArm_R",-30,25,22).R("LowerArm_R",-55).R("UpperArm_L",-36,-4,-15).R("Spine",5).R("Head",-3,-18);
            var rcharge=Brace(14,24,.17f,-.04f).R("UpperArm_R",-88,32,30).R("LowerArm_R",-72).R("Hand_R",-4,12,0).R("UpperArm_L",-40,-5,-18).R("Spine",3).R("Head",-5,-22);
            var rchargeHold=rcharge.Copy().R("UpperArm_R",-93,28,26).R("Chest",-1,24);
            var rjet=FreeLegPose(rchargeHold.Copy(),15,45,12).R("Spine",18).R("Head",-12,-22);
            var rover=Brace(5,8,.15f).R("UpperArm_R",-139,3,16).R("LowerArm_R",-30).R("Hand_R",-2).R("UpperArm_L",-46,0,-18).R("Spine",-4).R("Head",-6,-7);
            var rhit=Brace(-4,-8,.22f,.025f).R("UpperArm_R",-40,-3,-4).R("LowerArm_R",-15).R("Hand_R",12,-2,-4).R("Spine",12).R("Head",-10,5);
            var rfinish=Brace(-6,-10,.20f,.035f).R("UpperArm_R",-28,-10,-10).R("LowerArm_R",-25).R("Hand_R",5,-2,-4).R("Spine",14).R("Head",-10,6);
            add("R",4,false,"Activation .00-.45; charge .45-1.15; approach1.15-1.65; single active1.96-2.10; return3.20-4.00",new[]{K(0,ready),K(.15f,rload),K(.45f,rload),K(.80f,rcharge),K(1.10f,rchargeHold),K(1.15f,rchargeHold),K(1.25f,rjet),K(1.53f,rjet),K(1.65f,Brace(8,12,.18f)),K(1.80f,rover),K(1.92f,rover),K(1.96f,rover.Copy().R("UpperArm_R",-124,3,12)),K(2.025f,rhit),K(2.10f,rfinish),K(2.35f,rfinish),K(2.85f,rfinish.Copy().R("Spine",8).R("Head",-6,5)),K(3.20f,Brace(-2,-4,.09f).R("UpperArm_R",-12,-5,-4).R("LowerArm_R",-25)),K(3.6f,ready.Copy().R("UpperArm_R",-27,0,10).R("LowerArm_R",-54)),K(4,ready)});

            var hit=Brace(0,-4,.10f).R("Spine",-10).R("Chest",-8,-6).R("Head",5,5).R("UpperArm_R",-16,3,20).R("LowerArm_R",-67);
            add("Hit",.34f,false,"Short directional front-impact brace",new[]{K(0,ready),K(.055f,hit),K(.13f,hit.Copy().R("Spine",-5)),K(.34f,ready)});
            add("Stagger",.7f,false,"Larger impact then recover",new[]{K(0,ready),K(.10f,hit),K(.35f,hit.Copy().R("Spine",8).R("Head",-6)),K(.7f,ready)});
            var dead=FreeLegPose(ready.Copy(),-35,88,-35).R("Pelvis",18,0,6).R("Spine",34,0,5).R("Chest",13).R("Head",24).R("UpperArm_R",10,0,12).R("LowerArm_R",-15).R("UpperArm_L",12,0,-12).R("LowerArm_L",-18);
            dead.pelvisOffset=new Vector3(0,-.62f,0);
            add("Death",1.2f,false,"Collapse to disabled kneeling pose; no simulated ragdoll",new[]{K(0,ready),K(.16f,hit),K(.5f,dead.Copy().R("Spine",24)),K(.85f,dead),K(1.2f,dead)});

            string ctrlPath=assetRoot.TrimEnd('/')+"/MechaDemo.controller";
            var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if(ctrl==null) ctrl=AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var baseSM=ctrl.layers[0].stateMachine;
            foreach(var kv in clips.Where(x=>x.Key!="Aim"))
            {
                var state=baseSM.states.Select(x=>x.state).FirstOrDefault(x=>x.name==kv.Key);
                if(state==null) state=baseSM.AddState(kv.Key);
                state.motion=kv.Value; state.writeDefaultValues=true; state.speed=1;
                if(kv.Key=="Idle") baseSM.defaultState=state;
            }
            foreach(var state in baseSM.states)
                foreach(var tr in state.state.transitions) UnityEngine.Object.DestroyImmediate(tr,true);
            var mask=BuildUpperMask(assetRoot);
            var oldLayers=ctrl.layers;
            AnimatorControllerLayer upper=oldLayers.FirstOrDefault(x=>x.name=="UpperBody");
            if(upper==null)
            {
                upper=new AnimatorControllerLayer { name="UpperBody",defaultWeight=0,blendingMode=AnimatorLayerBlendingMode.Override,avatarMask=mask,stateMachine=new AnimatorStateMachine{name="UpperBody"} };
                AssetDatabase.AddObjectToAsset(upper.stateMachine,ctrl);
                ctrl.AddLayer(upper);
            }
            else upper.avatarMask=mask;
            foreach(string n in new[]{"Aim","Fire"})
            {
                var state=upper.stateMachine.states.Select(x=>x.state).FirstOrDefault(x=>x.name==n);
                if(state==null) state=upper.stateMachine.AddState(n);
                state.motion=clips[n];state.writeDefaultValues=false;state.speed=1;
                if(n=="Aim") upper.stateMachine.defaultState=state;
            }
            var layers=ctrl.layers;
            for(int i=0;i<layers.Length;i++) if(layers[i].name=="UpperBody") {layers[i]=upper;layers[i].defaultWeight=0;}
            ctrl.layers=layers;
            EditorUtility.SetDirty(ctrl);
            var animator=visual.GetComponent<Animator>();if(animator==null) animator=visual.AddComponent<Animator>();
            animator.runtimeAnimatorController=ctrl; animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            File.WriteAllText(assetRoot.TrimEnd('/')+"/AnimationClipSheets.csv",sheets.ToString());
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            return ctrl;
        }

        static Pose Ready()
        {
            var p=new Pose();
            foreach(string n in BoneNames)p.Q(n,Quaternion.identity);
            p.pelvisOffset=new Vector3(0,-.035f,0);
            p.R("Spine",2).R("Chest",0,2).R("Head",-1,-2)
             .R("UpperArm_R",-25,0,12).R("LowerArm_R",-65).R("Hand_R",0,0,0)
             .R("UpperArm_L",-45,0,-11).R("LowerArm_L",-35).R("Hand_L",0);
            SolveLeg(p,"L",restWorld["Foot_L"],0);SolveLeg(p,"R",restWorld["Foot_R"],0);
            return p;
        }

        static Pose Brace(float pelvisYaw,float chestYaw,float drop,float side=0)
        {
            var p=Ready(); p.pelvisOffset=new Vector3(side,-drop,0);
            p.R("Pelvis",0,pelvisYaw).R("Chest",0,chestYaw).R("Head",-1,-pelvisYaw*.55f-chestYaw*.5f);
            SolveLeg(p,"L",restWorld["Foot_L"]+new Vector3(0,0,.065f),0);
            SolveLeg(p,"R",restWorld["Foot_R"]+new Vector3(0,0,-.065f),0);
            p.R("FrontSkirt_L",-Mathf.Min(20,drop*70),0,-2).R("FrontSkirt_R",-Mathf.Min(20,drop*70),0,2);
            return p;
        }

        static Pose FreeLegPose(Pose p,float hip,float knee,float ankle)
        {
            p.R("UpperLeg_L",hip,0,-2).R("UpperLeg_R",hip+3,0,2)
             .R("LowerLeg_L",knee).R("LowerLeg_R",knee-3).R("Foot_L",ankle).R("Foot_R",ankle+2);
            p.R("FrontSkirt_L",Mathf.Min(0,hip)*.55f).R("FrontSkirt_R",Mathf.Min(0,hip)*.55f);
            return p;
        }

        static AnimationClip Gait(string name,bool run,string direction)
        {
            float period=run ? .66f : .9f;
            float stride=direction=="F"?(run?2.6f:1.8f):direction=="B"?(run?2f:1.4f):(run?1.2f:1f);
            var marks=new List<Mark>();
            for(int i=0;i<=32;i++) {float phase=i/32f;marks.Add(K(phase*period,GaitPose(phase,run,direction,stride)));}
            return Clip(name,period,true,"Baked two-bone foot trajectories; stance0-.5; swing.5-1; velocity scale by nominal_root_speed",marks.ToArray());
        }

        static Pose GaitPose(float phase,bool run,string direction,float stride)
        {
            var p=Ready();
            float wave=Mathf.Cos(phase*Mathf.PI*2);
            p.pelvisOffset=new Vector3(wave*(run ? .015f : .022f),-(run ? .16f : .105f)+Mathf.Cos(phase*Mathf.PI*4)*(run ? .015f : .01f),0);
            p.R("Pelvis",0,wave*(run?4:3),0).R("Spine",run?7:3,0,0).R("Chest",-2,-wave*(run?7:4),0).R("Head",-3,wave*(run?4:2));
            Vector3 dir=direction=="F"?Vector3.forward:direction=="B"?Vector3.back:direction=="L"?Vector3.left:Vector3.right;
            foreach(string side in new[]{"L","R"})
            {
                float t=Mathf.Repeat(phase+(side == "R" ? .5f : 0),1);
                float offset,height,pitch;
                if(t<=.5f)
                {
                    offset=stride*.25f-t*stride; height=0;
                    pitch=t<.08f?Mathf.Lerp(-6,0,t/.08f):t>.42f?Mathf.Lerp(0,10,(t-.42f)/.08f):0;
                }
                else
                {
                    float s=(t-.5f)*2;
                    offset=Mathf.Lerp(-stride*.25f,stride*.25f,s*s*(3-2*s));
                    height=Mathf.Sin(s*Mathf.PI)*(run ? .25f : .16f);
                    pitch=Mathf.Lerp(10,-6,s);
                }
                var target=restWorld["Foot_"+side]+dir*offset+Vector3.up*height;
                SolveLeg(p,side,target,direction=="F"?pitch:direction=="B"?-pitch*.6f:0);
            }
            p.R("UpperArm_R",-25+wave*3,0,12).R("LowerArm_R",-65-wave*3)
             .R("UpperArm_L",-45-wave*3,0,-11).R("LowerArm_L",-35+wave*2);
            p.R("FrontSkirt_L",-10-Mathf.Max(0,wave)*12).R("FrontSkirt_R",-10-Mathf.Max(0,-wave)*12);
            return p;
        }

        static void SolveLeg(Pose p,string side,Vector3 ankleTarget,float footPitch)
        {
            string hipName="UpperLeg_"+side,kneeName="LowerLeg_"+side,footName="Foot_"+side;
            if(!restWorld.ContainsKey(hipName)||!restWorld.ContainsKey(kneeName)||!restWorld.ContainsKey(footName))return;
            Quaternion pelvisQ=p.rotations.ContainsKey("Pelvis")?p.rotations["Pelvis"]:Quaternion.identity;
            Vector3 pelvis=restWorld["Pelvis"]+p.pelvisOffset;
            Vector3 hip=pelvis+pelvisQ*(restWorld[hipName]-restWorld["Pelvis"]);
            Vector3 upper=restWorld[kneeName]-restWorld[hipName],lower=restWorld[footName]-restWorld[kneeName];
            float a=upper.magnitude,b=lower.magnitude;
            if(a<.001f||b<.001f)return;
            Vector3 delta=ankleTarget-hip;
            float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);
            Vector3 axis=delta.sqrMagnitude>.00001f?delta.normalized:Vector3.down;
            float along=(a*a-b*b+d*d)/(2*d);
            float bend=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            Vector3 hint=Vector3.ProjectOnPlane(Vector3.forward,axis).normalized;
            if(hint.sqrMagnitude<.01f)hint=Vector3.ProjectOnPlane(Vector3.up,axis).normalized;
            Vector3 knee=hip+axis*along+hint*bend;
            Vector3 target=hip+axis*d;
            Quaternion hipWorld=Quaternion.FromToRotation(upper,knee-hip);
            Quaternion kneeWorld=Quaternion.FromToRotation(lower,target-knee);
            p.Q(hipName,Quaternion.Inverse(pelvisQ)*hipWorld);
            p.Q(kneeName,Quaternion.Inverse(hipWorld)*kneeWorld);
            p.Q(footName,Quaternion.Inverse(kneeWorld)*Quaternion.Euler(footPitch,0,0));
        }

        static AnimationClip Clip(string name,float duration,bool loop,string notes,Mark[] marks)
        {
            var clip=new AnimationClip {name=name,frameRate=60,legacy=false};
            foreach(var kv in bones)
            {
                string bone=kv.Key;
                string path=AnimationUtility.CalculateTransformPath(kv.Value,visual.transform);
                var q=new Quaternion[marks.Length];
                Quaternion previous=Quaternion.identity;
                for(int i=0;i<marks.Length;i++)
                {
                    Quaternion offset;
                    if(!marks[i].pose.rotations.TryGetValue(bone,out offset))offset=Quaternion.identity;
                    if(bone=="Shoulder_L"||bone=="Shoulder_R")
                    {
                        string side=bone.EndsWith("_L")?"L":"R";
                        Quaternion arm;
                        if(marks[i].pose.rotations.TryGetValue("UpperArm_"+side,out arm))
                        {
                            Vector3 e=arm.eulerAngles;
                            float pitch=Mathf.DeltaAngle(0,e.x),abduction=Mathf.DeltaAngle(0,e.z);
                            float sign=side=="L"?-1:1;
                            float opening=Mathf.Clamp(Mathf.Abs(pitch)*.07f+Mathf.Max(0,abduction*sign)*.25f,0,16);
                            offset=Quaternion.Euler(pitch*.08f,0,sign*opening);
                        }
                    }
                    q[i]=restRotation[bone]*offset;
                    if(i>0&&Quaternion.Dot(previous,q[i])<0)q[i]=new Quaternion(-q[i].x,-q[i].y,-q[i].z,-q[i].w);
                    previous=q[i];
                }
                WriteCurve(clip,path,"m_LocalRotation.x",marks,q.Select(v=>v.x).ToArray());
                WriteCurve(clip,path,"m_LocalRotation.y",marks,q.Select(v=>v.y).ToArray());
                WriteCurve(clip,path,"m_LocalRotation.z",marks,q.Select(v=>v.z).ToArray());
                WriteCurve(clip,path,"m_LocalRotation.w",marks,q.Select(v=>v.w).ToArray());
            }
            string pelvisPath=AnimationUtility.CalculateTransformPath(bones["Pelvis"],visual.transform);
            Vector3 basePos=bones["Pelvis"].localPosition;
            WriteCurve(clip,pelvisPath,"m_LocalPosition.x",marks,marks.Select(m=>basePos.x+m.pose.pelvisOffset.x).ToArray());
            WriteCurve(clip,pelvisPath,"m_LocalPosition.y",marks,marks.Select(m=>basePos.y+m.pose.pelvisOffset.y).ToArray());
            WriteCurve(clip,pelvisPath,"m_LocalPosition.z",marks,marks.Select(m=>basePos.z+m.pose.pelvisOffset.z).ToArray());
            clip.EnsureQuaternionContinuity();
            var settings=AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime=loop;settings.loopBlend=loop;settings.startTime=0;settings.stopTime=duration;
            AnimationUtility.SetAnimationClipSettings(clip,settings);
            string pathAsset=clipsFolder+"/"+name+".anim";
            var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(pathAsset);
            if(existing==null)AssetDatabase.CreateAsset(clip,pathAsset);
            else {EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;EditorUtility.SetDirty(clip);}
            sheets.AppendLine(name+","+duration.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+loop+","+NominalSpeed(name).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\""+notes.Replace("\"","'")+"\"");
            return clip;
        }
        static void WriteCurve(AnimationClip clip,string path,string property,Mark[] marks,float[] values)
        {
            var keys=new Keyframe[marks.Length];for(int i=0;i<keys.Length;i++)keys[i]=new Keyframe(marks[i].time,values[i]);
            var curve=new AnimationCurve(keys);
            for(int i=0;i<keys.Length;i++) {AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);}
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),property),curve);
        }
        /// <summary>Samples generated assets outside Play Mode and restores the complete hierarchy in finally.</summary>
        public static string ValidateAndWriteReport(GameObject visualRoot, RuntimeAnimatorController controller, string assetRoot)
        {
            var transforms = visualRoot.GetComponentsInChildren<Transform>(true);
            var positions = transforms.Select(t => t.localPosition).ToArray();
            var rotations = transforms.Select(t => t.localRotation).ToArray();
            var scales = transforms.Select(t => t.localScale).ToArray();
            var report = new StringBuilder("# Animation sampling validation\n\n");
            int finiteSamples = 0;
            try
            {
                var clips = controller.animationClips.Distinct().OrderBy(c => c.name).ToArray();
                report.AppendLine("Generated clips: " + clips.Length + ". Sampling checks are not a replacement for visual armor-clearance review.\n");
                foreach (var clip in clips)
                {
                    bool finite = true;
                    for (int i = 0; i <= 16; i++)
                    {
                        clip.SampleAnimation(visualRoot, clip.length * i / 16f);
                        foreach (var t in transforms)
                        {
                            var p = t.localPosition; var q = t.localRotation;
                            if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) || float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w)
                                || float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z)) finite = false;
                        }
                    }
                    if (finite) finiteSamples++;
                    report.AppendLine("- " + clip.name + ": duration " + clip.length.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " s; finite transforms: " + finite);
                }
                report.AppendLine("\n## Forward stance contact proxy\n");
                report.AppendLine("Measure ankle-joint movement after compensating nominal root translation over the left-foot stance phase .02-.48. Sole geometry and foot-pitch contact still require visual review.\n");
                foreach (string name in new[] { "WalkF", "RunF" })
                {
                    var clip = clips.First(c => c.name == name);
                    Transform foot = transforms.First(t => t.name == "Foot_L");
                    float stride = NominalSpeed(name) * clip.length;
                    clip.SampleAnimation(visualRoot, clip.length * .02f);
                    Vector3 first = visualRoot.transform.InverseTransformPoint(foot.position) + Vector3.forward * stride * .02f;
                    float maxDrift = 0, maxY = 0;
                    for (int i = 1; i <= 46; i++)
                    {
                        float phase = .02f + .46f * i / 46f;
                        clip.SampleAnimation(visualRoot, clip.length * phase);
                        Vector3 at = visualRoot.transform.InverseTransformPoint(foot.position) + Vector3.forward * stride * phase;
                        maxDrift = Mathf.Max(maxDrift, Vector3.Distance(first, at));
                        maxY = Mathf.Max(maxY, Mathf.Abs(first.y - at.y));
                    }
                    report.AppendLine("- " + name + ": max compensated stance ankle drift = " + maxDrift.ToString("F5", System.Globalization.CultureInfo.InvariantCulture) + " m; max vertical drift = " + maxY.ToString("F5", System.Globalization.CultureInfo.InvariantCulture) + " m.");
                }
                report.AppendLine("\nFinite-pose clip checks passed: " + finiteSamples + "/" + clips.Length + ". Root displacement belongs to motor. Combat foot-contact/root-lunge matching remains an integration check.\n");
            }
            finally
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].localPosition = positions[i]; transforms[i].localRotation = rotations[i]; transforms[i].localScale = scales[i];
                }
            }
            string folder = assetRoot.TrimEnd('/') + "/Reports";
            Directory.CreateDirectory(folder);
            File.WriteAllText(folder + "/AnimationValidation.md", report.ToString());
            AssetDatabase.Refresh();
            return report.ToString();
        }
        static AvatarMask BuildUpperMask(string root)
        {
            string path=root.TrimEnd('/')+"/UpperBody.mask";
            var mask=AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            bool fresh=mask==null;if(fresh)mask=new AvatarMask{name="UpperBody"};
            var transforms=visual.GetComponentsInChildren<Transform>(true).Where(t=>t==visual.transform||BoneNames.Contains(t.name)).ToArray();
            mask.transformCount=transforms.Length;
            for(int i=0;i<transforms.Length;i++)
            {
                string n=transforms[i].name;
                mask.SetTransformPath(i,AnimationUtility.CalculateTransformPath(transforms[i],visual.transform));
                bool active=n=="Spine"||n=="Chest"||n=="Neck"||n=="Head"||n.StartsWith("Shoulder")||n.StartsWith("UpperArm")||n.StartsWith("LowerArm")||n.StartsWith("Hand");
                mask.SetTransformActive(i,active);
            }
            for(int i=0;i<(int)AvatarMaskBodyPart.LastBodyPart;i++)mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i,false);
            if(fresh)AssetDatabase.CreateAsset(mask,path);else EditorUtility.SetDirty(mask);
            return mask;
        }
    }
}






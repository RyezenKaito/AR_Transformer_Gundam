using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MechaDemo;

namespace TurnBasedBattle.Editor
{
    public static class BattleClipBuilder
    {
        sealed class Pose
        {
            public Dictionary<string,Quaternion> q=new Dictionary<string,Quaternion>();
            public Vector3 offset;
            public Pose R(string name,float x,float y=0,float z=0){q[name]=Quaternion.Euler(x,y,z);return this;}
            public Pose Copy(){var p=new Pose{offset=offset};foreach(var k in q)p.q.Add(k.Key,k.Value);return p;}
        }
        struct Mark{public float t;public Pose p;public Mark(float time,Pose pose){t=time;p=pose;}}
        static Mark K(float t,Pose p)=>new Mark(t,p);
        static MechaRigData rig;
        static Dictionary<string,Vector3> rest;
        static Dictionary<string,Quaternion> rotations;

        public static AnimationClip[] Build(MechaRigData data,string directory,BattleTuning tuning=null)
        {
            rig=data;rig.ResetPose();rest=new Dictionary<string,Vector3>();rotations=new Dictionary<string,Quaternion>();
            foreach(var j in rig.joints){rest[j.name]=rig.visualRoot.InverseTransformPoint(j.position);rotations[j.name]=j.localRotation;}
            System.IO.Directory.CreateDirectory(directory);AssetDatabase.Refresh();
            var clips=new List<AnimationClip>();
            var ready=Ready();
            var breathIn=Brace(.012f).R("Spine",1.3f).R("Chest",-.7f,.7f).R("Head",-.5f,-.7f).R("UpperArm_R",-6,0,8);
            var breathOut=Brace(.035f).R("Spine",2.7f).R("Chest",.7f,-.7f).R("Head",-1.4f,.7f).R("LowerArm_L",-13);
            clips.Add(Clip(directory,"IdleBattle",3.6f,true,new[]{K(0,ready),K(.9f,breathIn),K(1.8f,ready),K(2.7f,breathOut),K(3.6f,ready)}));
            var aim=Brace(.035f).R("UpperArm_L",-53,0,-12).R("LowerArm_L",-27).R("Chest",0,4).R("Head",0,-4);
            var recoil=aim.Copy().R("Spine",-5).R("Chest",-5,4).R("UpperArm_L",-60,0,-12).R("LowerArm_L",-30);
            clips.Add(Clip(directory,"BasicAttack",tuning.attack.duration,false,ActionMarks(tuning.attack,ready)));
            var charged=Brace(.08f).R("UpperArm_L",-56,0,-15).R("LowerArm_L",-24).R("UpperArm_R",-30,-10,18).R("LowerArm_R",-80).R("Spine",4);
            clips.Add(Clip(directory,"SkillCast",tuning.skill.duration,false,ActionMarks(tuning.skill,ready)));
            var gather=Brace(.07f).R("UpperArm_L",-75,-10,-18).R("LowerArm_L",-65).R("UpperArm_R",-36,10,24).R("LowerArm_R",-78).R("Chest",-2,-5).R("Head",-8,4);
            var orb=gather.Copy().R("UpperArm_L",-82,0,-14).R("LowerArm_L",-65).R("Head",-10,0).R("Chest",-2,0);
            var release=Brace(.055f).R("UpperArm_L",-53,0,-10).R("LowerArm_L",-27).R("Spine",6).R("UpperArm_R",-25,0,16).R("LowerArm_R",-65);
            clips.Add(Clip(directory,"EarthPrisonCast",tuning.ultimate.duration,false,ActionMarks(tuning.ultimate,ready)));
            var evade=Brace(.12f).R("Spine",-12,0,19).R("Chest",-8,0,12).R("Head",2,0,-4).R("UpperArm_L",-38,0,-18).R("LowerArm_L",-72).R("UpperArm_R",-36,0,27).R("LowerArm_R",-76);
            clips.Add(Clip(directory,"DodgeInPlace",1,false,new[]{K(0,ready),K(.18f,evade),K(.55f,evade),K(1,ready)}));
            var guard=Brace(.08f).R("UpperArm_L",-65,20,-24).R("LowerArm_L",-88).R("UpperArm_R",-65,-20,24).R("LowerArm_R",-88).R("Spine",6).R("Head",5);
            clips.Add(Clip(directory,"Guard",1,false,new[]{K(0,ready),K(.25f,guard),K(1,guard)}));
            var hit=Brace(.075f).R("Spine",-12).R("Chest",-9,-5).R("Head",10,3).R("UpperArm_L",-30,0,-22).R("LowerArm_L",-55);
            clips.Add(Clip(directory,"HitReaction",.4f,false,new[]{K(0,ready),K(.065f,hit),K(.18f,hit.Copy().R("Spine",-6)),K(.4f,ready)}));
            var dead=Brace(.28f).R("Spine",32,0,5).R("Chest",16).R("Head",28).R("UpperArm_L",12,0,-15).R("LowerArm_L",-15).R("UpperArm_R",12,0,15).R("LowerArm_R",-15);
            clips.Add(Clip(directory,"Death",1.2f,false,new[]{K(0,ready),K(.12f,hit),K(.5f,dead.Copy().R("Spine",22)),K(.95f,dead),K(1.2f,dead)}));
            clips.Add(Clip(directory,"EnemyStrike",tuning.enemyStrike.duration,false,ActionMarks(tuning.enemyStrike,ready)));
            clips.Add(Clip(directory,"EnemyCombo",tuning.enemyCombo.duration,false,ActionMarks(tuning.enemyCombo,ready)));
            rig.ResetPose();return clips.ToArray();
        }

        static Mark[] ActionMarks(BattleActionDefinition a,Pose ready)
        {
            var prepare=Brace(.10f).R("Spine",7).R("Chest",3,-5).R("Head",-4,5);
            var draw=Brace(.06f).R("Chest",0,-8).R("UpperArm_R",-35,0,25).R("LowerArm_R",-85);
            if(a.weapon==BattleWeaponKind.Cannon)draw=Brace(.06f).R("Chest",0,8).R("UpperArm_L",-38,0,-22).R("LowerArm_L",-75);
            var boost=Brace(.04f).R("Spine",16).R("Head",-12).R("UpperArm_L",-25,0,-15).R("LowerArm_L",-65).R("UpperArm_R",-30,0,18).R("LowerArm_R",-85).R("UpperLeg_L",-18).R("LowerLeg_L",45).R("Foot_L",-27).R("UpperLeg_R",14).R("LowerLeg_R",32).R("Foot_R",-30);
            var windup=Brace(.07f).R("Spine",3).R("Chest",2,-18).R("Head",-2,14).R("UpperArm_R",-65,-22,28).R("LowerArm_R",-95);
            var aim=Brace(.06f).R("Spine",4).R("UpperArm_L",-53,0,-10).R("LowerArm_L",-27).R("UpperArm_R",-40,-10,8).R("LowerArm_R",-75);
            var returnPose=boost.Copy().R("Spine",12).R("Head",-8);
            if(a.travelStyle==BattleTravelStyle.Run)
            {
                boost=Brace(.045f).R("Spine",3).R("Head",-3).R("UpperArm_L",-18,0,-10).R("LowerArm_L",-80).R("UpperArm_R",-20,0,16).R("LowerArm_R",-75);
                returnPose=boost.Copy();
            }
            var marks=new List<Mark>{K(0,ready),K(.18f,prepare)};
            if(a.weapon!=BattleWeaponKind.None){marks.Add(K(a.gripTime,prepare));marks.Add(K(a.drawEnd,draw));}
            if(a.approachTarget){marks.Add(K(a.travelStart+.17f,boost));marks.Add(K(a.arriveTime-.16f,boost));}
            else marks.Add(K(a.drawEnd+.3f,aim));
            if(a.kind==BattleActionKind.EarthPrison)
            {
                var gather=Brace(.055f).R("Chest",-2,-4).R("Head",-8,4).R("UpperArm_L",-74,-8,-18).R("LowerArm_L",-65).R("UpperArm_R",-35,8,22).R("LowerArm_R",-70);
                var orb=gather.Copy().R("UpperArm_L",-82,0,-14).R("Chest",-3,0).R("Head",-10);
                marks.Add(K(a.arriveTime,prepare));marks.Add(K(a.vfxStart,prepare));marks.Add(K(a.vfxStart+.6f,gather));marks.Add(K(a.vfxStart+1.2f,orb));
                marks.Add(K(a.vfxStart+2.45f,orb.Copy().R("Chest",-2,1)));marks.Add(K(a.vfxStart+2.65f,orb));
                marks.Add(K(a.vfxStart+2.88f,aim));marks.Add(K(a.vfxStart+3.2f,aim));
                marks.Add(K(a.vfxStart+4,aim.Copy().R("Chest",1,2)));marks.Add(K(a.vfxStart+5,aim));marks.Add(K(a.vfxStart+6,aim.Copy().R("Chest",-1,-2)));marks.Add(K(a.vfxStart+7,aim));marks.Add(K(a.returnStart-.23f,prepare));
            }
            else
            {
                marks.Add(K(a.arriveTime,a.melee?windup:aim));
                foreach(var h in a.hits)
                {
                    marks.Add(K(h.time-.12f,a.melee?windup:aim));
                    marks.Add(K(h.time,a.melee?Brace(.09f).R("Spine",9).R("Chest",4,18).R("Head",-3,-10).R("UpperArm_R",-78,8,-8).R("LowerArm_R",-12):aim.Copy().R("Spine",-7).R("Chest",-5)));
                    marks.Add(K(h.time+.12f,a.melee?Brace(.07f).R("Chest",3,30).R("UpperArm_R",-68,35,-18).R("LowerArm_R",-30):aim));
                }
                marks.Add(K(a.returnStart-.22f,a.melee?windup:aim));
            }
            if(a.approachTarget){marks.Add(K(a.returnStart+.18f,returnPose));marks.Add(K(a.returnEnd-.22f,returnPose));}
            else marks.Add(K(a.returnStart+.18f,aim));
            marks.Add(K(a.returnEnd,Brace(.065f)));marks.Add(K(a.weapon==BattleWeaponKind.None?a.returnEnd+.16f:Mathf.Min(a.sheatheStart,a.returnEnd+.16f),prepare));
            if(a.weapon!=BattleWeaponKind.None){marks.Add(K(a.sheatheStart,draw));marks.Add(K(a.holsterTime,prepare));}
            marks.Add(K(a.duration,ready));
            return marks.OrderBy(m=>m.t).GroupBy(m=>m.t).Select(g=>g.Last()).ToArray();
        }

        static Pose Ready()
        {
            var p=new Pose();foreach(var j in rig.joints)p.q[j.name]=Quaternion.identity;
            p.offset=new Vector3(0,-.025f,0);
            p.R("Spine",2).R("Head",-1).R("UpperArm_L",-5,0,-8).R("LowerArm_L",-12).R("UpperArm_R",-5,0,8).R("LowerArm_R",-12);
            Legs(p);return p;
        }
        static Pose Brace(float drop){var p=Ready();p.offset=new Vector3(0,-drop,0);Legs(p);p.R("FrontSkirt_L",-drop*60).R("FrontSkirt_R",-drop*60);return p;}
        static void Legs(Pose p)
        {
            foreach(string side in new[]{"L","R"})
            {
                string h="UpperLeg_"+side,k="LowerLeg_"+side,f="Foot_"+side;
                if(!rest.ContainsKey(h)||!rest.ContainsKey(k)||!rest.ContainsKey(f))continue;
                Vector3 hip=rest[h]+p.offset,upper=rest[k]-rest[h],lower=rest[f]-rest[k];
                Vector3 d=rest[f]-hip;float a=upper.magnitude,b=lower.magnitude,len=Mathf.Clamp(d.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);
                Vector3 axis=d.normalized;float along=(a*a-b*b+len*len)/(2*len);float bend=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
                Vector3 hint=Vector3.ProjectOnPlane(Vector3.forward,axis).normalized;Vector3 knee=hip+axis*along+hint*bend;
                Quaternion hq=Quaternion.FromToRotation(upper,knee-hip),kq=Quaternion.FromToRotation(lower,hip+axis*len-knee);
                p.q[h]=hq;p.q[k]=Quaternion.Inverse(hq)*kq;p.q[f]=Quaternion.Inverse(kq);
            }
        }
        static AnimationClip Clip(string dir,string name,float duration,bool loop,Mark[] marks)
        {
            var clip=new AnimationClip{name=name,frameRate=60};
            foreach(var j in rig.joints)
            {
                string path=AnimationUtility.CalculateTransformPath(j,rig.visualRoot);var qs=new Quaternion[marks.Length];
                for(int i=0;i<marks.Length;i++)
                {
                    Quaternion offset=marks[i].p.q.ContainsKey(j.name)?marks[i].p.q[j.name]:Quaternion.identity;
                    if(j.name=="Shoulder_L"||j.name=="Shoulder_R")
                    {
                        string suffix=j.name.EndsWith("L")?"L":"R";float sign=suffix=="L"?-1:1;
                        Vector3 arm=marks[i].p.q["UpperArm_"+suffix].eulerAngles;
                        float pitch=Mathf.DeltaAngle(0,arm.x),roll=Mathf.DeltaAngle(0,arm.z);
                        offset=Quaternion.Euler(pitch*.07f,0,sign*Mathf.Clamp(Mathf.Abs(pitch)*.06f+Mathf.Max(0,roll*sign)*.25f,0,15));
                    }
                    qs[i]=rotations[j.name]*offset;
                    if(i>0&&Quaternion.Dot(qs[i-1],qs[i])<0)qs[i]=new Quaternion(-qs[i].x,-qs[i].y,-qs[i].z,-qs[i].w);
                }
                Curve(clip,path,"m_LocalRotation.x",marks,qs.Select(q=>q.x).ToArray());Curve(clip,path,"m_LocalRotation.y",marks,qs.Select(q=>q.y).ToArray());
                Curve(clip,path,"m_LocalRotation.z",marks,qs.Select(q=>q.z).ToArray());Curve(clip,path,"m_LocalRotation.w",marks,qs.Select(q=>q.w).ToArray());
                if(j.name=="Pelvis")
                {
                    Curve(clip,path,"m_LocalPosition.x",marks,marks.Select(m=>j.localPosition.x+m.p.offset.x).ToArray());
                    Curve(clip,path,"m_LocalPosition.y",marks,marks.Select(m=>j.localPosition.y+m.p.offset.y).ToArray());
                    Curve(clip,path,"m_LocalPosition.z",marks,marks.Select(m=>j.localPosition.z+m.p.offset.z).ToArray());
                }
            }
            clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;settings.startTime=0;settings.stopTime=duration;AnimationUtility.SetAnimationClipSettings(clip,settings);
            string assetPath=dir+"/"+name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if(existing!=null){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);clip=existing;}else AssetDatabase.CreateAsset(clip,assetPath);
            EditorUtility.SetDirty(clip);return clip;
        }
        static void Curve(AnimationClip c,string path,string property,Mark[] marks,float[] values)
        {
            var keys=new Keyframe[marks.Length];for(int i=0;i<keys.Length;i++)keys[i]=new Keyframe(marks[i].t,values[i]);
            var curve=new AnimationCurve(keys);for(int i=0;i<keys.Length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);}
            AnimationUtility.SetEditorCurve(c,EditorCurveBinding.FloatCurve(path,typeof(Transform),property),curve);
        }
    }
}

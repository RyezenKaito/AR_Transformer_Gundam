using System;
using UnityEngine;
using MechaDemo;

namespace TurnBasedBattle
{
    [DisallowMultipleComponent]
    public sealed class BattleActor : MonoBehaviour
    {
        public int actorId;
        public string actorName = "Sentinel";
        public bool player;
        [Tooltip("Scene-specific vacuum pose; battle anchors, timing and damage stay unchanged.")]
        public bool zeroGravity;
        public Transform visual, castSocket;
        public MechaRigData rig;
        public AnimationClip[] clips = Array.Empty<AnimationClip>();
        public Renderer[] bodyRenderers = Array.Empty<Renderer>();
        public float maxHP = 600f;
        public GameObject meleeWeapon;
        public Transform meleeSocket;
        public BattleWeaponRig weapons;
        public Transform focusSocket;
        public float bodyRadius=.5f;
        [Range(.05f,.3f)] public float poseBlendTime=.16f;
        public float HP { get; private set; }
        public bool Dead => HP <= 0;
        public Vector3 AnchorPosition => transform.parent?transform.parent.TransformPoint(anchorLocalPosition):anchorLocalPosition;
        public Quaternion AnchorRotation => transform.parent?transform.parent.rotation*anchorLocalRotation:anchorLocalRotation;
        public Vector3 AimPoint => transform.TransformPoint(Vector3.up * 1.45f);
        public Vector3 HealthBarPoint => rig && rig.Joint("Head") ? rig.Joint("Head").position + transform.up * .55f * Mathf.Abs(transform.lossyScale.y) : transform.TransformPoint(Vector3.up * 3.1f);
        public Vector3 CastPoint => weapons && weapons.Equipped==BattleWeaponKind.Cannon && weapons.muzzle ? weapons.muzzle.position : focusSocket ? focusSocket.position : castSocket != null ? castSocket.position : AimPoint;
        public string PoseName => poseName;
        public float PoseTime => poseTime;
        string poseName = "IdleBattle";
        float poseTime, reactionTime, dodgeHold, flashRemaining, transition;
        string reaction = "";
        Quaternion[] blendRotations;
        Vector3[] blendPositions;
        Transform[] joints;
        Quaternion[] baseRotations;
        Vector3[] basePositions;
        Vector3 anchorLocalPosition,destinationLocal;
        Quaternion anchorLocalRotation,attackLocalRotation;
        BattleActionDefinition motion;
        float motionTime,boostAmount;
        Vector3 flightSide;
        float lane;
        public bool IsApproaching => motion!=null;
        public BattleActor MotionTarget {get;private set;}
        MaterialPropertyBlock properties;
        bool initialized;
        BattleLocomotionPose locomotion;
        public float TravelWeight {get;private set;}
        float runDistance;
        float airborneWeight;
        public float AirborneWeight=>airborneWeight;
        public bool CastPositionReady=>motion!=null && (!motion.airborneCast || (motionTime>=motion.arriveTime && motionTime<motion.returnStart && Vector2.Distance(new Vector2(transform.localPosition.x,transform.localPosition.z),new Vector2(destinationLocal.x,destinationLocal.z))<.03f && airborneWeight>.999f && locomotion!=null && transform.localPosition.y-anchorLocalPosition.y+locomotion.LowestFootOffset>=rig.referenceHeight*motion.castHeightRatio-.03f));

        public void Initialize(float health)
        {
            if(!initialized)
            {
                anchorLocalPosition=transform.localPosition;anchorLocalRotation=transform.localRotation;
                if(rig == null) rig = GetComponent<MechaRigData>();
                if(visual == null) visual = rig != null ? rig.visualRoot : transform;
                var animator = visual.GetComponent<Animator>();
                if(animator != null) { animator.applyRootMotion = false; animator.enabled = false; }
                joints = rig != null ? rig.joints : Array.Empty<Transform>();
                blendRotations = new Quaternion[joints.Length]; blendPositions = new Vector3[joints.Length];
                baseRotations=new Quaternion[joints.Length];basePositions=new Vector3[joints.Length];
                properties = new MaterialPropertyBlock(); initialized = true;
            }
            maxHP = health; HP = health; reaction = ""; flashRemaining = 0;
            EndActionMotion();
            transform.SetPositionAndRotation(AnchorPosition, AnchorRotation);
            if(rig != null) rig.ResetPose();
            if(locomotion==null && rig)locomotion=new BattleLocomotionPose(this);
            poseName = "IdleBattle"; poseTime = 0; transition = 1;
            Sample("IdleBattle", 0); ApplyFlash(0);
        }

        AnimationClip FindClip(string name)
        {
            for(int i=0;i<clips.Length;i++) if(clips[i] != null && clips[i].name == name) return clips[i];
            return null;
        }

        void SaveBlend()
        {
            for(int i=0;i<joints.Length;i++) if(joints[i] != null) { blendRotations[i] = joints[i].localRotation; blendPositions[i] = joints[i].localPosition; }
            transition = 0;
        }

        public void Play(string name)
        {
            if(Dead && name != "Death") return;
            SaveBlend(); poseName = name; poseTime = 0;
        }

        public void Dodge(float timeUntilImpact)
        {
            if(Dead) return;
            reaction = "DodgeInPlace"; reactionTime = 0;
            dodgeHold = Mathf.Max(.05f, timeUntilImpact) + .06f;
        }

        public float Damage(float amount)
        {
            if(Dead || amount <= 0) return 0;
            float dealt = Mathf.Min(HP, amount); HP = Mathf.Max(0, HP - amount);
            reactionTime = 0; flashRemaining = .16f;
            if(Dead) { SaveBlend();reaction = ""; poseName = "Death"; poseTime = 0; }
            else reaction = "HitReaction";
            return dealt;
        }

        public void Tick(float dt)
        {
            if(!initialized) return;
            poseTime += dt; reactionTime += dt; transition = Mathf.Min(1, transition + dt / poseBlendTime);
            string clip = poseName; float t = poseTime;
            float reactionWeight=0;
            if(reaction == "DodgeInPlace")
            {
                clip = reaction;
                if(reactionTime < .12f) t = reactionTime / .12f * .18f;
                else if(reactionTime <= dodgeHold) t = .35f;
                else t = .55f + (reactionTime-dodgeHold)/.32f * .45f;
                reactionWeight=Mathf.SmoothStep(0,1,reactionTime/.06f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.75f,1,t)));
                if(t >= 1f) { reaction = ""; clip = poseName; t = poseTime;reactionWeight=0; }
            }
            else if(reaction == "HitReaction")
            {
                clip = reaction; t = reactionTime;
                reactionWeight=Mathf.SmoothStep(0,1,reactionTime/.04f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.24f,.4f,t)));
                if(t >= .40f) { reaction = ""; clip = poseName; t = poseTime;reactionWeight=0; }
            }
            Sample(poseName,poseTime);
            if(transition < 1)
            {
                float blend = transition*transition*(3-2*transition);
                for(int i=0;i<joints.Length;i++) if(joints[i] != null)
                {
                    joints[i].localRotation = Quaternion.Slerp(blendRotations[i], joints[i].localRotation, blend);
                    joints[i].localPosition = Vector3.Lerp(blendPositions[i], joints[i].localPosition, blend);
                }
            }
            if(motion!=null && locomotion!=null)
            {
                locomotion.Apply(zeroGravity?BattleTravelStyle.Fly:motion.travelStyle,TravelWeight,runDistance,motion.runStride,motionTime);
                if(motion.airborneCast)
                {
                    locomotion.Hover(airborneWeight,motionTime);
                    // Use the current lowest ankle relative to its rest height, so folding legs does not change clearance.
                    float footOffset=locomotion.LowestFootOffset;
                    var p=transform.localPosition;
                    p.y=anchorLocalPosition.y+airborneWeight*(rig.referenceHeight*motion.castHeightRatio-footOffset);
                    transform.localPosition=p;
                }
                if(motion.travelStyle==BattleTravelStyle.Run&&!zeroGravity)
                {
                    locomotion.Turn(Mathf.InverseLerp(motion.returnStart-.5f,motion.returnStart-.02f,motionTime),1-TravelWeight);
                    locomotion.Turn(Mathf.InverseLerp(motion.returnEnd-.18f,motion.returnEnd+.15f,motionTime),1-TravelWeight);
                }
            }
            if(reactionWeight>0)
            {
                for(int i=0;i<joints.Length;i++){baseRotations[i]=joints[i].localRotation;basePositions[i]=joints[i].localPosition;}
                Sample(clip,t);
                for(int i=0;i<joints.Length;i++)
                {
                    joints[i].localRotation=Quaternion.Slerp(baseRotations[i],joints[i].localRotation,reactionWeight);
                    joints[i].localPosition=Vector3.Lerp(basePositions[i],joints[i].localPosition,reactionWeight);
                }
            }
            if(zeroGravity&&locomotion!=null)locomotion.Hover(1-airborneWeight,poseTime);
            if(flashRemaining > 0) { flashRemaining = Mathf.Max(0, flashRemaining - dt); ApplyFlash(flashRemaining/.16f); }
            if(weapons)weapons.Evaluate(motion,motionTime,zeroGravity?Mathf.Max(.1f,TravelWeight*.8f,boostAmount):boostAmount);
        }

        static float Ease(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(6*t-15)+10);}
        // Integrated smooth velocity: accelerate, cruise, brake without the high mid-curve spike of a position ease.
        public static float TravelProgress(float u)
        {
            u=Mathf.Clamp01(u);const float ramp=.24f;
            if(u>1-ramp)return 1-TravelProgress(1-u);
            if(u<ramp)return (u*u*u/(ramp*ramp)-.5f*u*u*u*u/(ramp*ramp*ramp))/(1-ramp);
            return (u-ramp*.5f)/(1-ramp);
        }
        public void BeginActionMotion(BattleActionDefinition action,BattleActor target,Vector3? areaCenter=null)
        {
            EndActionMotion();if(!target)return;
            motion=action;
            MotionTarget=target;
            Vector3 up=transform.parent?transform.parent.up:Vector3.up;
            Vector3 goal=areaCenter??target.AnchorPosition;
            Vector3 delta=Vector3.ProjectOnPlane(goal-AnchorPosition,up);
            float scale=Mathf.Abs(transform.lossyScale.y);
            float standOff=action.melee?Mathf.Max(action.stopDistance,bodyRadius+target.bodyRadius+.45f):action.stopDistance;
            Vector3 point=AnchorPosition+delta.normalized*Mathf.Max(0,delta.magnitude-standOff*scale);
            destinationLocal=transform.parent?transform.parent.InverseTransformPoint(point):point;
            Quaternion facing=delta.sqrMagnitude>.001f?Quaternion.LookRotation(delta,up):AnchorRotation;
            attackLocalRotation=transform.parent?Quaternion.Inverse(transform.parent.rotation)*facing:facing;
            flightSide=Vector3.Cross(Vector3.up,(destinationLocal-anchorLocalPosition).normalized);lane=0;
            var battle=FindAnyObjectByType<TurnBattleManager>();
            if(battle!=null)foreach(var other in battle.enemies)
            {
                if(!other || other==target || other==this || !other.gameObject.activeSelf)continue;
                Vector3 p=transform.parent?transform.parent.InverseTransformPoint(other.AnchorPosition):other.AnchorPosition;
                Vector3 path=destinationLocal-anchorLocalPosition;float u=Mathf.Clamp01(Vector3.Dot(p-anchorLocalPosition,path)/Mathf.Max(.001f,path.sqrMagnitude));
                if(Vector3.Distance(p,anchorLocalPosition+path*u)<bodyRadius+other.bodyRadius+.25f)lane=(actorId%2==0?1:-1)*1.3f;
            }
            motionTime=boostAmount=0;
        }
        public void EvaluateActionMotion(float time)
        {
            if(motion==null)return;
            motionTime=time;
            TravelWeight=0;
            airborneWeight=0;
            if(!motion.approachTarget)
            {
                transform.localPosition=anchorLocalPosition;boostAmount=0;
                float aim=Ease(Mathf.InverseLerp(0,motion.drawEnd,time))*(1-Ease(Mathf.InverseLerp(motion.sheatheStart,motion.duration,time)));
                transform.localRotation=Quaternion.Slerp(anchorLocalRotation,attackLocalRotation,aim);
                return;
            }
            float outTime=Mathf.InverseLerp(motion.travelStart,motion.arriveTime,time);
            float backTime=Mathf.InverseLerp(motion.returnStart,motion.returnEnd,time);
            float outward=TravelProgress(outTime);
            float homeward=TravelProgress(backTime);
            float travel=outward*(1-homeward);
            float Window(float u)=>Ease(u/.24f)*Ease((1-u)/.24f);
            TravelWeight=Window(outTime)*(1-homeward)+Window(backTime);
            if(motion.airborneCast)airborneWeight=Ease(outTime)*(1-Ease(backTime));
            float lift=motion.travelStyle==BattleTravelStyle.Fly?Ease(outTime/.32f)*Ease((1-outTime)/.32f)*(1-homeward)+Ease(backTime/.32f)*Ease((1-backTime)/.32f):0;
            runDistance=Vector3.Distance(anchorLocalPosition,destinationLocal)*(time<motion.returnStart?outward:homeward);
            float laneWeight=Mathf.Sin(Mathf.PI*outward)*(1-homeward)+Mathf.Sin(Mathf.PI*homeward);
            transform.localPosition=Vector3.Lerp(anchorLocalPosition,destinationLocal,travel)+Vector3.up*lift*motion.travelLift+flightSide*lane*laneWeight*laneWeight;
            float facing=Ease(Mathf.InverseLerp(0,motion.travelStart,time))*(1-Ease(Mathf.InverseLerp(motion.returnEnd-.18f,motion.returnEnd+.15f,time)));
            transform.localRotation=Quaternion.Slerp(anchorLocalRotation,attackLocalRotation,facing);
            // Turn into the return flight, then settle back to the formation heading before landing.
            float turn=Ease(Mathf.InverseLerp(motion.returnStart-.5f,motion.returnStart-.02f,time))*(1-Ease(Mathf.InverseLerp(motion.returnEnd-.18f,motion.returnEnd+.15f,time)));
            transform.localRotation=transform.localRotation*Quaternion.Euler(0,180*turn,0);
            boostAmount=motion.travelStyle==BattleTravelStyle.Fly?Mathf.Max(Mathf.Clamp01(lift),airborneWeight*.4f):0;
        }
        public void EndActionMotion()
        {
            motion=null;MotionTarget=null;motionTime=boostAmount=TravelWeight=runDistance=airborneWeight=0;if(weapons)weapons.ResetWeapons();else if(meleeWeapon)meleeWeapon.SetActive(false);
            if(initialized){transform.localPosition=anchorLocalPosition;transform.localRotation=anchorLocalRotation;}
        }

        void Sample(string name,float time)
        {
            var clip = FindClip(name); if(clip == null || visual == null) return;
            float t = name == "IdleBattle" ? Mathf.Repeat(time, clip.length) : Mathf.Clamp(time,0,clip.length);
            clip.SampleAnimation(visual.gameObject, t);
        }

        public void PreviewPose(string name,float time) { if(!initialized) Initialize(maxHP); Sample(name,time); }

        void ApplyFlash(float amount)
        {
            for(int i=0;i<bodyRenderers.Length;i++) if(bodyRenderers[i] != null)
            {
                bodyRenderers[i].GetPropertyBlock(properties);
                properties.SetFloat("_BattleFlash", amount);
                bodyRenderers[i].SetPropertyBlock(properties);
            }
        }
    }
}

using UnityEngine;

namespace TurnBasedBattle
{
    /// <summary>Distance-driven gait layered over the action pose; stance feet counter root travel.</summary>
    public sealed class BattleLocomotionPose
    {
        readonly BattleActor actor;
        readonly Transform pelvis,spine,chest,head,leftArm,rightArm,leftElbow,rightElbow;
        readonly Quaternion leftRest,rightRest,leftElbowRest,rightElbowRest;
        readonly Leg[] legs;
        sealed class Leg
        {
            public Transform hip,knee,foot,skirt;
            public Vector3 restFoot;
            public Quaternion restFootRotation;
        }
        public BattleLocomotionPose(BattleActor owner)
        {
            actor=owner;var rig=actor.rig;
            pelvis=rig.Joint("Pelvis");spine=rig.Joint("Spine");chest=rig.Joint("Chest");head=rig.Joint("Head");leftArm=rig.Joint("UpperArm_L");rightArm=rig.Joint("UpperArm_R");
            leftElbow=rig.Joint("LowerArm_L");rightElbow=rig.Joint("LowerArm_R");
            leftRest=leftArm.localRotation;rightRest=rightArm.localRotation;leftElbowRest=leftElbow.localRotation;rightElbowRest=rightElbow.localRotation;
            legs=new Leg[2];
            for(int i=0;i<2;i++)
            {
                string side=i==0?"L":"R";
                var leg=new Leg{hip=rig.Joint("UpperLeg_"+side),knee=rig.Joint("LowerLeg_"+side),foot=rig.Joint("Foot_"+side),skirt=rig.Joint("FrontSkirt_"+side)};
                leg.restFoot=actor.transform.InverseTransformPoint(leg.foot.position);
                leg.restFootRotation=Quaternion.Inverse(actor.transform.rotation)*leg.foot.rotation;legs[i]=leg;
            }
        }
        static void Add(Transform joint,Vector3 angles,float weight){if(joint)joint.localRotation*=Quaternion.Euler(angles*weight);}
        public void Apply(BattleTravelStyle style,float weight,float distance,float stride,float time)
        {
            if(weight<=0 || style==BattleTravelStyle.Stationary)return;
            if(style==BattleTravelStyle.Fly)
            {
                Add(spine,new Vector3(3,0,Mathf.Sin(time*2)*1.8f),weight);
                Add(head,new Vector3(-3,0,0),weight);
                for(int i=0;i<legs.Length;i++){Add(legs[i].hip,new Vector3(Mathf.Sin(time*2+i)*3,0,0),weight);Add(legs[i].knee,new Vector3(Mathf.Sin(time*2+i)*-2,0,0),weight);}
                return;
            }
            float cycle=distance/Mathf.Max(1,stride)+.25f;
            float swing=Mathf.Sin(cycle*Mathf.PI*2);
            pelvis.localPosition+=new Vector3(.032f*swing,-.15f+.025f*Mathf.Cos(cycle*Mathf.PI*4),0)*weight;
            Add(spine,new Vector3(8,swing*4,swing*1.5f),weight);Add(chest,new Vector3(0,-swing*6,0),weight);Add(head,new Vector3(-6,swing*2,0),weight);
            // Match the support leg's fore/aft phase. A positive shoulder pitch swings back as the same leg steps forward.
            FootTrajectory(Mathf.Repeat(cycle,1),stride,out float leftForward,out _);
            float armPhase=Mathf.Clamp(leftForward/(stride*.3f),-1,1);
            leftArm.localRotation=Quaternion.Slerp(leftArm.localRotation,leftRest*Quaternion.Euler(-8+armPhase*23,0,-10),weight);
            rightArm.localRotation=Quaternion.Slerp(rightArm.localRotation,rightRest*Quaternion.Euler(-12-armPhase*17,0,12),weight);
            leftElbow.localRotation=Quaternion.Slerp(leftElbow.localRotation,leftElbowRest*Quaternion.Euler(-76+armPhase*7,0,0),weight);
            rightElbow.localRotation=Quaternion.Slerp(rightElbow.localRotation,rightElbowRest*Quaternion.Euler(-74-armPhase*5,0,0),weight);
            for(int i=0;i<legs.Length;i++)
            {
                float phase=Mathf.Repeat(cycle+i*.5f,1);FootTrajectory(phase,stride,out float forward,out float height);
                var leg=legs[i];Vector3 goal=actor.transform.TransformPoint(leg.restFoot+new Vector3(0,height,forward));
                Solve(leg,goal,weight);Add(leg.skirt,new Vector3(-Mathf.Max(0,forward)*24,0,0),weight);
            }
        }
        static void FootTrajectory(float phase,float stride,out float forward,out float height)
        {
            const float stance=.6f;
            if(phase<stance){forward=stride*(stance*.5f-phase);height=0;return;}
            float u=(phase-stance)/(1-stance),u2=u*u,u3=u2*u;
            float back=-stride*stance*.5f,front=-back,tangent=-stride*(1-stance);
            forward=(2*u3-3*u2+1)*back+(u3-2*u2+u)*tangent+(-2*u3+3*u2)*front+(u3-u2)*tangent;
            height=.24f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2);
        }
        public float LowestFootOffset=>Mathf.Min(actor.transform.InverseTransformPoint(legs[0].foot.position).y-legs[0].restFoot.y,actor.transform.InverseTransformPoint(legs[1].foot.position).y-legs[1].restFoot.y);
        public void Hover(float weight,float time)
        {
            if(weight<=0)return;
            // Preserve cast arms, animate only the lower body and subtle torso balance.
            Add(pelvis,new Vector3(0,0,Mathf.Sin(time*2.1f)*1.2f),weight);
            for(int i=0;i<2;i++)
            {
                Add(legs[i].hip,new Vector3(-12+i*4,0,i==0?-3:3),weight);
                Add(legs[i].knee,new Vector3(26+i*4,0,0),weight);
                Add(legs[i].foot,new Vector3(-10,0,0),weight);
            }
        }
        public void Turn(float phase,float weight)
        {
            if(phase<=0 || phase>=1 || weight<=0)return;
            float envelope=Mathf.Pow(Mathf.Sin(phase*Mathf.PI),2)*weight;
            pelvis.localPosition+=Vector3.down*.035f*envelope;
            Add(chest,new Vector3(0,Mathf.Sin(phase*Mathf.PI*2)*-7,0),envelope);
            for(int i=0;i<legs.Length;i++)
            {
                float step=Mathf.Clamp01(phase*2-i),lift=.13f*Mathf.Pow(Mathf.Sin(step*Mathf.PI),2);
                Solve(legs[i],actor.transform.TransformPoint(legs[i].restFoot+Vector3.up*lift),envelope);
            }
        }
        void Solve(Leg leg,Vector3 goal,float weight)
        {
            Quaternion h=leg.hip.rotation,k=leg.knee.rotation,f=leg.foot.rotation;
            float upper=Vector3.Distance(leg.hip.position,leg.knee.position),lower=Vector3.Distance(leg.knee.position,leg.foot.position);
            Vector3 delta=goal-leg.hip.position;float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(upper-lower)+.001f,upper+lower-.001f);
            Vector3 axis=delta.normalized,hint=Vector3.ProjectOnPlane(actor.transform.forward,axis).normalized;
            float along=(upper*upper-lower*lower+d*d)/(2*d);
            Vector3 knee=leg.hip.position+axis*along+hint*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            leg.hip.rotation=Quaternion.FromToRotation(leg.knee.position-leg.hip.position,knee-leg.hip.position)*leg.hip.rotation;
            leg.knee.rotation=Quaternion.FromToRotation(leg.foot.position-leg.knee.position,goal-leg.knee.position)*leg.knee.rotation;
            Quaternion solvedHip=leg.hip.rotation,solvedKnee=leg.knee.rotation;
            leg.hip.rotation=Quaternion.Slerp(h,solvedHip,weight);leg.knee.rotation=Quaternion.Slerp(k,solvedKnee,weight);
            leg.foot.rotation=Quaternion.Slerp(f,actor.transform.rotation*leg.restFootRotation,weight);
        }
    }
}

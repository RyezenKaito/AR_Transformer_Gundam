using UnityEngine;

namespace TurnBasedBattle
{
    public enum WeaponPoseState { Holstered,Reaching,Drawing,Equipped,Active,Recovering,Sheathing }
    /// <summary>One persistent instance per weapon. Analytic attachment keeps marker skipping deterministic.</summary>
    public sealed class BattleWeaponRig : MonoBehaviour
    {
        public BattleActor actor;
        public Transform saber,saberHolster,saberHand,blade,bladeTip,cannon,cannonHolster,cannonHand,muzzle,offHandGrip;
        public Transform[] boosters;
        public Light bladeLight;
        public WeaponPoseState State {get;private set;}
        public BattleWeaponKind Equipped {get;private set;}
        public float BladeAmount {get;private set;}
        public float LastGripError {get;private set;}
        public readonly Vector3[] TrailPoints=new Vector3[14];
        public bool TrailActive {get;private set;}
        // The saved idle blade is retracted (Z=0), so never use its current scale as its full length.
        Vector3 bladeScale=Vector3.one,bladePosition;
        bool ready;
        void Ensure(){if(ready)return;ready=true;if(blade)bladePosition=blade.localPosition;}
        public void ResetWeapons()
        {
            Ensure();State=WeaponPoseState.Holstered;Equipped=BattleWeaponKind.None;BladeAmount=0;
            Place(saber,saberHolster);Place(cannon,cannonHolster);SetBlade(0);
            TrailActive=false;
            if(boosters!=null)foreach(var b in boosters)if(b)b.gameObject.SetActive(false);
        }
        static void Place(Transform item,Transform socket){if(item && socket){item.SetPositionAndRotation(socket.position,socket.rotation);item.localScale=Vector3.one;}}
        public void Evaluate(BattleActionDefinition action,float t,float boost)
        {
            Ensure();
            if(boosters!=null)foreach(var b in boosters)if(b){b.gameObject.SetActive(boost>.01f);b.localScale=Vector3.one;var jet=b.GetComponent<BattleThrusterVfx>();if(jet)jet.Evaluate(boost,t);}
            if(action==null || action.weapon==BattleWeaponKind.None){Place(saber,saberHolster);Place(cannon,cannonHolster);SetBlade(0);Equipped=BattleWeaponKind.None;State=WeaponPoseState.Holstered;return;}
            bool sword=action.weapon==BattleWeaponKind.Saber;
            Transform item=sword?saber:cannon,holster=sword?saberHolster:cannonHolster,hand=sword?saberHand:cannonHand;
            if(!item || !holster || !hand)return;
            Place(sword?cannon:saber,sword?cannonHolster:saberHolster);
            float reach=0;
            if(t<action.gripTime)reach=Smooth(0,action.gripTime,t);
            else if(t<action.drawEnd)reach=1-Smooth(action.gripTime,action.drawEnd,t);
            else if(t>=action.sheatheStart && t<action.holsterTime)reach=Smooth(action.sheatheStart,action.holsterTime,t);
            else if(t>=action.holsterTime)reach=1-Smooth(action.holsterTime,action.duration-.03f,t);
            if(reach>0)Reach(sword?"R":"L",hand,holster,reach);
            bool held=t>=action.gripTime && t<action.holsterTime;
            if(sword && held && actor.MotionTarget!=null && action.hits.Length>0)
            {
                float hit=action.hits[0].time;
                float align=Smooth(hit-.23f,hit-.035f,t)*(1-Smooth(hit+.035f,hit+.23f,t));
                var wrist=actor.rig.Joint("Hand_R");
                Vector3 target=actor.MotionTarget.AimPoint;
                Quaternion aim=Quaternion.LookRotation(target-hand.position,actor.transform.up)*Quaternion.Inverse(hand.localRotation);
                wrist.rotation=Quaternion.Slerp(wrist.rotation,aim,align);
                bool trail=t>=hit-.2f && t<=hit+.15f;
                Vector3 tip=hand.TransformPoint(new Vector3(0,0,1.37f));
                if(!TrailActive || !trail)for(int i=0;i<TrailPoints.Length;i++)TrailPoints[i]=tip;
                else{for(int i=TrailPoints.Length-1;i>0;i--)TrailPoints[i]=TrailPoints[i-1];TrailPoints[0]=tip;}
                TrailActive=trail;
            }
            Place(item,held?hand:holster);
            if(Mathf.Abs(t-action.gripTime)<.025f || Mathf.Abs(t-action.holsterTime)<.025f)LastGripError=Vector3.Distance(hand.position,holster.position);
            Equipped=held?action.weapon:BattleWeaponKind.None;
            State=t<action.gripTime?WeaponPoseState.Reaching:t<action.drawEnd?WeaponPoseState.Drawing:t<action.bladeOn?WeaponPoseState.Equipped:t<action.bladeOff?WeaponPoseState.Active:t<action.sheatheStart?WeaponPoseState.Recovering:t<action.duration-.03f?WeaponPoseState.Sheathing:WeaponPoseState.Holstered;
            SetBlade(sword && held?Smooth(action.bladeOn,action.bladeOn+.13f,t)*(1-Smooth(action.bladeOff,action.bladeOff+.15f,t)):0);
            if(!sword && held && t>action.drawEnd && t<action.sheatheStart && offHandGrip)
            {
                float aimWeight=Smooth(action.drawEnd,action.drawEnd+.2f,t)*(1-Smooth(action.sheatheStart-.2f,action.sheatheStart,t));
                if(actor.MotionTarget)
                {
                    var wrist=actor.rig.Joint("Hand_L");
                    Quaternion aim=Quaternion.LookRotation(actor.MotionTarget.AimPoint-hand.position,actor.transform.up)*Quaternion.Inverse(hand.localRotation);
                    wrist.rotation=Quaternion.Slerp(wrist.rotation,aim,aimWeight);Place(item,hand);
                }
                Reach("R",saberHand,offHandGrip,aimWeight);
            }
        }
        static float Smooth(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
        void SetBlade(float amount)
        {
            BladeAmount=amount;if(!blade)return;blade.gameObject.SetActive(amount>.001f);
            blade.localScale=new Vector3(bladeScale.x,bladeScale.y,bladeScale.z*amount);
            blade.localPosition=bladePosition;
            if(bladeLight){bladeLight.enabled=amount>.01f;bladeLight.intensity=1.6f*amount;bladeLight.range=2*Mathf.Abs(actor.transform.lossyScale.y);}
        }
        void Reach(string side,Transform grip,Transform goal,float weight)
        {
            if(!grip || !goal || weight<=0)return;
            var upper=actor.rig.Joint("UpperArm_"+side);var lower=actor.rig.Joint("LowerArm_"+side);var hand=actor.rig.Joint("Hand_"+side);
            if(!upper||!lower||!hand)return;
            Quaternion uq=upper.rotation,lq=lower.rotation,hq=hand.rotation;
            Quaternion targetHand=goal.rotation*Quaternion.Inverse(grip.localRotation);
            Vector3 offset=Vector3.Scale(grip.localPosition,hand.lossyScale);
            Vector3 target=goal.position-targetHand*offset;
            float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);
            Vector3 delta=target-upper.position;float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);Vector3 axis=delta.normalized;
            Vector3 hint=actor.transform.TransformDirection(new Vector3(side=="R"?1:-1,-.2f,-.25f));
            Vector3 bend=Vector3.ProjectOnPlane(hint,axis).normalized;if(bend.sqrMagnitude<.01f)bend=actor.transform.forward;
            float along=(a*a-b*b+d*d)/(2*d);Vector3 elbow=upper.position+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
            Quaternion solvedUpper=upper.rotation,solvedLower=lower.rotation;
            upper.rotation=Quaternion.Slerp(uq,solvedUpper,weight);lower.rotation=Quaternion.Slerp(lq,solvedLower,weight);hand.rotation=Quaternion.Slerp(hq,targetHand,weight);
        }
    }
}

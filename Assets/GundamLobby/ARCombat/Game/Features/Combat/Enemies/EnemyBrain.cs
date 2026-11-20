using System;
using System.Linq;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    public sealed class EnemyBrain : MonoBehaviour
    {
        public enum State { Idle, Position, Telegraph, Attack, Recovery }
        public EnemyProfile profile;
        public DemoFighter fighter;
        public GameObject animatedModel, blade;
        public Transform rightHand, leftHand, bazookaDock, shieldDock, bazooka, shield, bazookaMuzzle;
        public State CurrentState { get; private set; }
        public EnemyProfile.Move Move => profile.moves[slot];
        public int Slot => slot;
        public bool Guarding { get; private set; }
        public bool IsRanged => profile.role==EnemyProfile.Role.Ranged;
        public bool IsBlast => IsRanged && slot==1;
        public bool IsMelee => !IsRanged && slot!=1;
        public Vector3 Aim { get; private set; }
        public Vector3 GroundAim => new(Aim.x,target.transform.position.y+.04f,Aim.z);
        public Vector3 Origin => IsBlast && bazooka ? (bazookaMuzzle?bazookaMuzzle.position:bazooka.TransformPoint(bazookaTip)) :
            IsRanged && rifle ? rifle.TransformPoint(rifleTip) : VulcanOrigin(0);
        public Vector3 BladeBase { get; private set; }
        public Vector3 BladeTip { get; private set; }
        public Vector3 BladeWindupDirection { get; private set; }
        public bool BladeLit => lit;
        public const float BlastRadius = 1.15f;

        readonly int[] cooldown=new int[3];
        Transform[] nodes;
        Transform rifle, head, swordBone, shoulder, elbow, shieldShoulder, shieldElbow, shieldSkinRoot, shieldBody;
        SkinnedMeshRenderer bladeSkin;
        Mesh bladeMesh;
        readonly System.Collections.Generic.List<Vector3> bladeVertices=new(32);
        int bladeBaseIndex=-1, bladeTipIndex=-1;
        Vector3[] restPositions, restScales;
        Quaternion[] restRotations;
        Vector3 home, startingPosition, facing, bazookaTip, rifleTip;
        Quaternion homeRotation;
        DemoFighter target;
        AnimationEvent[] cues;
        float[] hitTimes;
        int slot, cursor;
        float recentDamage, firedAt=-10, equip, guardRelease;
        bool lit;

        void Awake()
        {
            home=transform.localPosition;homeRotation=transform.localRotation;
            nodes=animatedModel.GetComponentsInChildren<Transform>(true);
            rifle=Find("ライフル");
            head=Find("頭") ?? Suffix("_18");
            if(!rightHand)rightHand=Suffix(".R_27");
            swordBone=Suffix("_42");
            shoulder=Suffix(".R_31");
            elbow=Suffix(".R_29");
            shieldShoulder=Find("腕.L");
            shieldElbow=Find("ひじ.L");
            shieldSkinRoot=Find("盾");
            shieldBody=Find("盾本体");
            if(shield && shieldBody)
                foreach(var renderer in shield.GetComponentsInChildren<Renderer>())renderer.enabled=false;
            if(blade)
            {
                bladeSkin=blade.GetComponentInChildren<SkinnedMeshRenderer>(true);
                if(bladeSkin)bladeMesh=new Mesh{name="RX-78-2 blade contact mesh"};
            }
            // Center of the front-most ring of vertices weighted to ライフル.
            rifleTip=new Vector3(0,.1557f,.7743f);
            if(bazooka)bazookaTip=bazooka.InverseTransformVector(bazooka.up*.48f);
            if(profile.clip)profile.clip.SampleAnimation(animatedModel,0);
            restPositions=nodes.Select(node=>node.localPosition).ToArray();
            restRotations=nodes.Select(node=>node.localRotation).ToArray();
            restScales=nodes.Select(node=>node.localScale).ToArray();
            ResetEquipment();
        }

        Transform Find(string name)=>Array.Find(nodes,node=>node.name==name);
        Transform Suffix(string suffix)=>Array.Find(nodes,node=>node.name.EndsWith(suffix,StringComparison.Ordinal));

        public void ResetBrain()
        {
            Array.Clear(cooldown,0,cooldown.Length);
            Guarding=false;
            recentDamage=guardRelease=0;
            Finish();
        }

        public void RegisterPressure(float damage)=>recentDamage+=damage;

        public void Observe(float dt)
        {
            if(guardRelease<=0)return;
            guardRelease=Mathf.Max(0,guardRelease-dt);
            profile.clip.SampleAnimation(animatedModel,0);
            PoseShield(guardRelease/.30f,guardRelease/.30f);
        }

        public void EndGuard()
        {
            if(!Guarding)return;
            Guarding=false;
            guardRelease=.30f;
        }

        public void Begin(DemoFighter opponent)
        {
            Guarding=false;
            guardRelease=0;
            target=opponent;
            for(int i=0;i<cooldown.Length;i++)cooldown[i]=Mathf.Max(0,cooldown[i]-1);
            float distance=Vector3.Distance(transform.position,target.transform.position);
            if(IsRanged)
                slot=cooldown[2]==0 && recentDamage>=22?2:cooldown[1]==0?1:0;
            else
                slot=cooldown[1]==0 && distance>2.8f?1:cooldown[2]==0?2:0;
            cooldown[slot]=Move.cooldownTurns+1;
            recentDamage=0;
            cursor=0;
            cues=Move.timeline ? Move.timeline.events.OrderBy(evt=>evt.time).ThenBy(evt=>evt.intParameter).ToArray() :
                Array.Empty<AnimationEvent>();
            hitTimes=cues.Where(e=>e.intParameter==(int)EnemyProfile.Cue.Hit).Select(e=>e.time).ToArray();
            CacheActionTiming();
            firedAt=-10;
            equip=0;
            lit=false;
            startingPosition=transform.position;
            facing=Vector3.ProjectOnPlane(target.transform.position-startingPosition,Vector3.up).normalized;
            if(facing.sqrMagnitude<.01f)facing=transform.forward;
            Aim=target.AimPoint;
            CurrentState=State.Position;
        }

        float firstHitTime, recoveryStartTime;
        public float FirstHitTime => firstHitTime;
        void CacheActionTiming()
        {
            var hit=Array.Find(cues,e=>e.intParameter==(int)EnemyProfile.Cue.Hit);
            firstHitTime=hit!=null?hit.time:Move.telegraph;
            var recover=Array.Find(cues,e=>e.intParameter==(int)EnemyProfile.Cue.Recover);
            recoveryStartTime=recover!=null?recover.time:Mathf.Max(0,Duration-Move.recovery);
        }
        public float Duration => Move.timeline ? Move.timeline.length : Move.telegraph+Move.recovery;

        public void Evaluate(float time,Action<EnemyProfile.Cue> dispatch)
        {
            // SampleAnimation does not dispatch Unity events. Evaluate the
            // exact authored event pose before emitting VFX or testing contact.
            while(cursor<cues.Length && cues[cursor].time<=time+.00001f)
            {
                var evt=cues[cursor++];
                var cue=(EnemyProfile.Cue)evt.intParameter;
                if(cue==EnemyProfile.Cue.Hit && IsMelee)
                {
                    Pose(evt.time-4f/30);
                    BladeWindupDirection=(BladeTip-BladeBase).normalized;
                }
                switch(cue)
                {
                    case EnemyProfile.Cue.Warn: CurrentState=State.Telegraph;break;
                    case EnemyProfile.Cue.Equip: equip=1;break;
                    case EnemyProfile.Cue.BladeOn: lit=true;break;
                    case EnemyProfile.Cue.Fire: CurrentState=State.Attack;firedAt=evt.time;break;
                    case EnemyProfile.Cue.Hit: CurrentState=State.Attack;break;
                    case EnemyProfile.Cue.Guard: Guarding=true;CurrentState=State.Attack;break;
                    case EnemyProfile.Cue.Recover: CurrentState=State.Recovery;break;
                    case EnemyProfile.Cue.BladeOff: lit=false;break;
                }
                Pose(evt.time);
                dispatch(cue);
            }
            Pose(time);
        }

        void Pose(float time)
        {
            float charge=Smooth(.13f,.64f,time);
            float recover=Smooth(Duration-.38f,Duration,time);
            Vector3 targetGround=Aim;targetGround.y=startingPosition.y;
            float reach=IsRanged?Mathf.Min(3.9f,Move.range):slot==1?2.55f:1.24f;
            Vector3 requested=targetGround-facing*reach;
            float approach=IsMelee?Smooth(.18f,FirstHitTime-.08f,time):!IsRanged?Smooth(.28f,FirstHitTime+.40f,time):Smooth(.05f,.47f,time);
            if(IsMelee && slot==2)
                approach=.47f*Smooth(.18f,.77f,time)+.53f*Smooth(FirstHitTime-.24f,FirstHitTime,time);
            float maxTravel=profile.moveSpeed*(IsMelee?.86f:.38f);
            if(IsRanged && Vector3.Distance(startingPosition,targetGround)<reach)requested=startingPosition;
            Vector3 attackPosition=Vector3.Lerp(startingPosition,Vector3.MoveTowards(startingPosition,requested,maxTravel),approach);
            float retreat=Smooth(recoveryStartTime,Duration,time);
            var parent=transform.parent;
            Vector3 localAttack=parent?parent.InverseTransformPoint(attackPosition):attackPosition;
            Quaternion attackRotation=Quaternion.LookRotation(facing);
            Quaternion localFacing=parent?Quaternion.Inverse(parent.rotation)*attackRotation:attackRotation;
            transform.SetLocalPositionAndRotation(Vector3.Lerp(localAttack,home,retreat),Quaternion.Slerp(localFacing,homeRotation,retreat));
            float source=Move.sourceTime?.Evaluate(time)??0;
            for(int i=0;i<nodes.Length;i++)
            {
                nodes[i].SetLocalPositionAndRotation(restPositions[i],restRotations[i]);
                nodes[i].localScale=restScales[i];
            }
            profile.clip.SampleAnimation(animatedModel,Mathf.Clamp(source,0,profile.clip.length));
            for(int i=0;i<nodes.Length;i++)
            {
                var node=nodes[i];
                node.localPosition=Vector3.Lerp(node.localPosition,restPositions[i],recover);
                node.localRotation=Quaternion.Slerp(node.localRotation,restRotations[i],recover);
                node.localScale=Vector3.Lerp(node.localScale,restScales[i],recover);
            }
            if(IsRanged)
            {
                float recoil=(1-Smooth(0,.21f,time-firedAt))*.09f;
                if(rifle && slot==0)
                {
                    rifle.rotation=Quaternion.Slerp(rifle.rotation,Quaternion.LookRotation(Aim-rifle.position),charge*(1-recover));
                    rifle.position-=facing*recoil;
                }
                if(IsBlast)
                {
                    if(rifle)rifle.localScale*=1-.999f*Smooth(.23f,.65f,time)*(1-recover);
                    Place(bazooka,bazookaDock,rightHand,Mathf.Max(equip,charge)*(1-recover));
                    if(bazooka && charge>.55f)
                        bazooka.rotation=Quaternion.Slerp(bazooka.rotation,
                            Quaternion.LookRotation(Aim-bazooka.position)*Quaternion.Euler(90,0,0),charge*(1-recover));
                }
                if(slot==2 && shield)
                    PoseShield(Guarding?1:charge*(1-recover),Guarding?1:Mathf.Max(equip,charge)*(1-recover));
            }
            if(blade)
            {
                blade.SetActive(lit);
                if(lit)
                {
                    if(slot==2 && swordBone && time<Duration-.34f)
                    {
                        float extension=Smooth(.64f,FirstHitTime-.15f,time)*(1-recover);
                        var scale=swordBone.localScale;
                        // Extend the animated blade on its local long axis, keeping the hilt fixed.
                        scale.y*=1+extension*.75f;
                        swordBone.localScale=scale;
                    }
                    CaptureBladeSegment();
                    GuideSword(time);
                }
            }
        }

        void PoseShield(float guardPose,float transfer)
        {
            if(!shield || !leftHand)return;
            Vector3 guardGoal=transform.position+facing*.48f+Vector3.up*1.23f-transform.right*.19f;
            AimJoint(shieldShoulder,leftHand,guardGoal,guardPose*.92f);
            AimJoint(shieldElbow,leftHand,guardGoal,guardPose*.92f);
            Place(shield,shieldDock,leftHand,transfer);
            shield.rotation=Quaternion.Slerp(shield.rotation,Quaternion.LookRotation(facing),guardPose);
            if(!shieldSkinRoot || !shieldBody || transfer<=0)return;
            // The imported shield's broad face is on its local YZ plane.
            Quaternion bodyTarget=Quaternion.LookRotation(facing)*Quaternion.Euler(0,90,0);
            Quaternion boneOffset=Quaternion.Inverse(shieldSkinRoot.rotation)*shieldBody.rotation;
            Vector3 restCenter=shieldBody.TransformPoint(new Vector3(0,.24f,0));
            shieldSkinRoot.rotation=Quaternion.Slerp(shieldSkinRoot.rotation,bodyTarget*Quaternion.Inverse(boneOffset),transfer);
            Vector3 heldCenter=shield.position+facing*.16f;
            shieldSkinRoot.position+=Vector3.Lerp(restCenter,heldCenter,transfer)-shieldBody.TransformPoint(new Vector3(0,.24f,0));
        }

        void GuideSword(float time)
        {
            if(!swordBone || !rightHand || hitTimes.Length==0 || (BladeTip-BladeBase).sqrMagnitude<.02f)return;
            int nearest=0;
            for(int i=1;i<hitTimes.Length;i++)
                if(Mathf.Abs(time-hitTimes[i])<Mathf.Abs(time-hitTimes[nearest]))nearest=i;
            float offset=time-hitTimes[nearest];
            float guided=1-Smooth(.075f,.24f,Mathf.Abs(offset));
            if(guided<=0)return;
            Vector3 gripGoal=transform.position+Vector3.up*1.22f+facing*.48f+transform.right*.24f;
            AimJoint(shoulder,rightHand,gripGoal,guided*.88f);
            AimJoint(elbow,rightHand,gripGoal,guided*.88f);
            Vector3 grip=rightHand.position;
            Vector3 incoming=(Aim-grip).normalized;
            Vector3 axis=nearest%2==0?Vector3.up:-Vector3.up;
            if(nearest==2)axis=transform.right;
            float sweep=slot==2?20:100;
            incoming=Quaternion.AngleAxis(Mathf.Clamp(offset*5,-1,1)*sweep,axis)*incoming;
            Vector3 current=BladeTip-BladeBase;
            swordBone.rotation=Quaternion.Slerp(Quaternion.identity,
                Quaternion.FromToRotation(current,incoming),guided)*swordBone.rotation;
            CaptureBladeSegment();
            swordBone.position+=Vector3.Lerp(Vector3.zero,grip-BladeBase,guided);
            CaptureBladeSegment();
        }

        static void AimJoint(Transform joint,Transform hand,Vector3 goal,float weight)
        {
            if(!joint)return;
            var current=hand.position-joint.position;
            var desired=goal-joint.position;
            if(current.sqrMagnitude>.001f && desired.sqrMagnitude>.001f)
                joint.rotation=Quaternion.Slerp(Quaternion.identity,
                    Quaternion.FromToRotation(current,desired),weight)*joint.rotation;
        }

        void CaptureBladeSegment()
        {
            if(!bladeSkin || !bladeMesh)return;
            bladeSkin.BakeMesh(bladeMesh);
            bladeMesh.GetVertices(bladeVertices);
            if(bladeVertices.Count==0)return;
            if(bladeBaseIndex<0)
            {
                Vector3 pivot=swordBone?swordBone.position:rightHand.position;
                float nearest=float.PositiveInfinity,farthest=-1;
                int start=0,end=0;
                for(int i=0;i<bladeVertices.Count;i++)
                {
                    Vector3 point=bladeSkin.transform.TransformPoint(bladeVertices[i]);
                    float d=(point-pivot).sqrMagnitude;
                    if(d<nearest){nearest=d;start=i;}
                    if(d>farthest){farthest=d;end=i;}
                }
                if((bladeSkin.transform.TransformPoint(bladeVertices[end])-
                    bladeSkin.transform.TransformPoint(bladeVertices[start])).sqrMagnitude<.02f)return;
                bladeBaseIndex=start;bladeTipIndex=end;
            }
            BladeBase=bladeSkin.transform.TransformPoint(bladeVertices[bladeBaseIndex]);
            BladeTip=bladeSkin.transform.TransformPoint(bladeVertices[bladeTipIndex]);
        }

        public Vector3 VulcanOrigin(int index)
        {
            if(!head)return fighter.AimPoint;
            return head.position+facing*.135f+Vector3.up*.065f+transform.right*(index%2==0?.083f:-.083f);
        }

        public bool BladeContacts(Vector3 point,float radius)
        {
            Vector3 delta=BladeTip-BladeBase;
            if(delta.sqrMagnitude<.02f)return false;
            float t=Mathf.Clamp01(Vector3.Dot(point-BladeBase,delta)/delta.sqrMagnitude);
            return (point-(BladeBase+t*delta)).sqrMagnitude<=radius*radius;
        }

        public void Finish()
        {
            transform.SetLocalPositionAndRotation(home,homeRotation);
            CurrentState=State.Idle;
            lit=false;
            if(animatedModel && profile && profile.clip)profile.clip.SampleAnimation(animatedModel,0);
            ResetEquipment();
        }
        void ResetEquipment()
        {
            Place(bazooka,bazookaDock,rightHand,0);
            if(Guarding)PoseShield(1,1);
            else Place(shield,shieldDock,leftHand,0);
            if(blade)blade.SetActive(false);
        }
        static void Place(Transform item,Transform dock,Transform hand,float t)
        {
            if(!item||!dock||!hand)return;
            var parent=t>=.999f?hand:dock;
            if(item.parent!=parent)item.SetParent(parent,true);
            item.SetPositionAndRotation(Vector3.Lerp(dock.position,hand.position,t),Quaternion.Slerp(dock.rotation,hand.rotation,t));
        }
        static float Smooth(float from,float to,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(from,to,value));
        void OnDestroy(){if(bladeMesh)Destroy(bladeMesh);}
    }
}

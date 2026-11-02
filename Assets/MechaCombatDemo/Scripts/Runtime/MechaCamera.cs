using UnityEngine;
using UnityEngine.InputSystem;

namespace MechaDemo
{
    [DefaultExecutionOrder(200)]
    public sealed class MechaCamera : MonoBehaviour
    {
        public Transform follow;
        public MechaPlayer player;
        public LayerMask environmentMask=1<<8;
        public float distance=6.6f, height=1.65f, sensitivity=.13f;
        public bool cinematic=true;
        [Range(0,1)]public float shakeAmount=.6f;
        float yaw,pitch=14;
        Vector3 smoothVelocity;
        Camera lens;
        bool initialized;
        void Awake(){lens=GetComponent<Camera>();}
        void LateUpdate()
        {
            if(follow==null)return;
            if(player!=null&&player.IsPaused)return;
            float dt=Time.unscaledDeltaTime;if(dt<=0)return;
            if(!initialized){yaw=follow.eulerAngles.y;initialized=true;}
            if(Mouse.current!=null&&Cursor.lockState==CursorLockMode.Locked)
            {
                Vector2 delta=Mouse.current.delta.ReadValue();yaw+=delta.x*sensitivity;pitch=Mathf.Clamp(pitch-delta.y*sensitivity,-15,64);
            }
            Vector3 focus=follow.position+Vector3.up*height;
            float desiredDistance=distance;
            if(player!=null&&player.Target!=null&&!player.Target.IsDead)
            {
                Vector3 target=player.Target.AimPosition;
                Vector3 direction=target-focus;direction.y=0;
                if(direction.sqrMagnitude>.1f)yaw=Mathf.LerpAngle(yaw,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,1-Mathf.Exp(-dt*4));
                focus=Vector3.Lerp(focus,target,.28f);desiredDistance=Mathf.Clamp(distance+direction.magnitude*.12f,distance,9.5f);
            }
            Quaternion orbit=Quaternion.Euler(pitch,yaw,0);
            Vector3 gameplayPosition=focus+orbit*(Vector3.back*desiredDistance);
            Vector3 desired=gameplayPosition;float fov=player!=null&&player.IsBoosting?67:60;
            if(cinematic&&player!=null&&player.ActionName=="R")
            {
                float t=player.ActionTime;Vector3 center=follow.position+Vector3.up*1.65f;
                Vector3 f=follow.forward,r=follow.right;
                if(t<.45f){desired=center+f*3.8f+r*3.3f+Vector3.up*.7f;fov=55;focus=center;}
                else if(t<1.15f){desired=center+f*4.1f+r*3.6f+Vector3.up*1.1f;fov=55;focus=center+Vector3.up*.35f;}
                else if(t<2.35f)
                {
                    if(player.Target!=null&&!player.Target.IsDead)focus=Vector3.Lerp(center,player.Target.AimPosition,.35f);else focus=center+f;
                    desired=focus-f*4.8f+r*4.3f+Vector3.up*1.2f;fov=64;
                }
                else if(t<3.2f){desired=center-f*4.8f+r*3.2f+Vector3.up*.75f;focus=center;fov=58;}
                else {float blend=Mathf.SmoothStep(0,1,(t-3.2f)/.8f);desired=Vector3.Lerp(center-f*4.8f+r*3.2f+Vector3.up*.75f,gameplayPosition,blend);focus=Vector3.Lerp(center,focus,blend);fov=Mathf.Lerp(58,60,blend);}
            }
            Vector3 offset=desired-focus;float length=offset.magnitude;
            if(length>.01f&&Physics.SphereCast(focus,.22f,offset/length,out RaycastHit hit,length,environmentMask,QueryTriggerInteraction.Ignore))desired=focus+offset.normalized*Mathf.Max(.5f,hit.distance-.14f);
            transform.position=Vector3.SmoothDamp(transform.position,desired,ref smoothVelocity,.11f,100,dt);
            Vector3 look=focus-transform.position;
            if(look.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look),1-Mathf.Exp(-dt*16));
            if(player!=null&&player.effects!=null)
            {
                float shake=player.effects.Shake*shakeAmount;
                transform.position+=new Vector3(Mathf.PerlinNoise(Time.unscaledTime*47,1)-.5f,Mathf.PerlinNoise(2,Time.unscaledTime*53)-.5f,0)*shake*.15f;
            }
            if(lens!=null)lens.fieldOfView=Mathf.Lerp(lens.fieldOfView,fov,1-Mathf.Exp(-dt*7));
        }
        public void Snap()
        {
            if(follow==null)return;yaw=follow.eulerAngles.y;pitch=14;smoothVelocity=Vector3.zero;initialized=true;
            Vector3 focus=follow.position+Vector3.up*height;transform.position=focus+Quaternion.Euler(pitch,yaw,0)*Vector3.back*distance;transform.LookAt(focus);
        }
    }
}


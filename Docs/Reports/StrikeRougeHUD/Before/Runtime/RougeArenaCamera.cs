using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GundamGame.CombatDemo
{
    /// <summary>Orbit the virtual board; leave the tracked phone camera to AR Foundation.</summary>
    [DefaultExecutionOrder(150)]
    public sealed class RougeArenaCamera : MonoBehaviour
    {
        public Transform arenaRoot;
        public RougeSkillPresentation presentation;
        public RougeArenaPlacement placement;
        public Vector3 pivot = new Vector3(0,.5f,.4f);
        public float minimumDistance=5.5f, maximumDistance=28;
        public float minimumPitch=10, maximumPitch=75;
        public float mouseSensitivity=.18f, touchSensitivity=.16f;
        public bool CanControl => initialized && isActiveAndEnabled && gameObject.activeInHierarchy &&
            (placement==null || !placement.IsARMode) && (presentation==null || !presentation.CinematicActive);
        public float Yaw => yaw;
        public float Pitch => pitch;
        public float Distance => distance;
        float yaw,pitch,distance,homeYaw,homePitch,homeDistance;
        bool initialized,mouseDragging,touchBlocked;
        int previousCount,firstTouchId,secondTouchId;
        Vector2 previousPosition;
        float previousPinch;
        readonly System.Collections.Generic.List<RaycastResult> uiHits = new System.Collections.Generic.List<RaycastResult>();

        void Start(){Initialize();}
        public void Initialize()
        {
            if(initialized || arenaRoot==null)return;
            Vector3 offset=arenaRoot.InverseTransformPoint(transform.position)-pivot;
            distance=Mathf.Max(.01f,offset.magnitude);
            yaw=Mathf.Atan2(offset.x,offset.z)*Mathf.Rad2Deg;
            pitch=Mathf.Asin(Mathf.Clamp(offset.y/distance,-1,1))*Mathf.Rad2Deg;
            distance=Mathf.Clamp(distance,minimumDistance,maximumDistance);pitch=Mathf.Clamp(pitch,minimumPitch,maximumPitch);
            homeYaw=yaw;homePitch=pitch;homeDistance=distance;initialized=true;
        }
        void LateUpdate()
        {
            Initialize();
            if(!CanControl){ClearGestures();return;}
            HandleMouse();HandleTouch();
            if(Keyboard.current!=null && Keyboard.current.fKey.wasPressedThisFrame)ResetView();
            ApplyPose();
        }
        public bool Orbit(float yawDegrees,float pitchDegrees,float zoomFactor=1)
        {
            if(!CanControl)return false;
            yaw=Mathf.Repeat(yaw+yawDegrees+180,360)-180;
            pitch=Mathf.Clamp(pitch+pitchDegrees,minimumPitch,maximumPitch);
            distance=Mathf.Clamp(distance*Mathf.Max(.01f,zoomFactor),minimumDistance,maximumDistance);
            ApplyPose();return true;
        }
        public void ResetView()
        {
            if(!CanControl)return;
            yaw=homeYaw;pitch=homePitch;distance=homeDistance;ClearGestures();ApplyPose();
        }
        void ApplyPose()
        {
            float a=yaw*Mathf.Deg2Rad,b=pitch*Mathf.Deg2Rad;
            Vector3 offset=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Sin(b),Mathf.Cos(a)*Mathf.Cos(b))*distance;
            Vector3 center=arenaRoot.TransformPoint(pivot);
            transform.SetPositionAndRotation(arenaRoot.TransformPoint(pivot+offset),Quaternion.LookRotation(center-arenaRoot.TransformPoint(pivot+offset),arenaRoot.up));
        }
        void HandleMouse()
        {
            var mouse=Mouse.current;if(mouse==null)return;
            Vector2 position=mouse.position.ReadValue();
            if(mouse.rightButton.wasPressedThisFrame)mouseDragging=!OverUI(position);
            if(!mouse.rightButton.isPressed)mouseDragging=false;
            if(mouseDragging){Vector2 delta=mouse.delta.ReadValue();Orbit(delta.x*mouseSensitivity,-delta.y*mouseSensitivity);}
            float scroll=mouse.scroll.ReadValue().y;
            if(Mathf.Abs(scroll)>.01f && !OverUI(position))Orbit(0,0,Mathf.Exp(-scroll*.0012f));
        }
        void HandleTouch()
        {
            var screen=Touchscreen.current;if(screen==null)return;
            int count=0,idA=0,idB=0;Vector2 a=Vector2.zero,b=Vector2.zero;
            foreach(var touch in screen.touches)
            {
                if(!touch.press.isPressed)continue;
                Vector2 position=touch.position.ReadValue();
                if(touch.press.wasPressedThisFrame && OverUI(position))touchBlocked=true;
                if(count==0){a=position;idA=touch.touchId.ReadValue();}
                else if(count==1){b=position;idB=touch.touchId.ReadValue();}
                count++;
            }
            if(count==0){previousCount=0;touchBlocked=false;return;}
            if(touchBlocked){previousCount=0;return;}
            if(count!=previousCount || idA!=firstTouchId || idB!=secondTouchId)
            {previousCount=count;firstTouchId=idA;secondTouchId=idB;previousPosition=a;previousPinch=Vector2.Distance(a,b);return;}
            if(count==1){Vector2 delta=a-previousPosition;Orbit(delta.x*touchSensitivity,-delta.y*touchSensitivity);}
            else if(count==2)
            {float pinch=Vector2.Distance(a,b);if(previousPinch>1 && pinch>1)Orbit(0,0,previousPinch/pinch);previousPinch=pinch;}
            previousPosition=a;
        }
        bool OverUI(Vector2 position)
        {
            if(EventSystem.current==null)return false;
            uiHits.Clear();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},uiHits);return uiHits.Count>0;
        }
        void ClearGestures(){mouseDragging=false;previousCount=0;touchBlocked=false;}
        void OnDisable(){ClearGestures();}
    }
}

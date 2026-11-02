using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TurnBasedBattle
{
    /// <summary>Orbit independent of combat motion. External/AR cameras retain full pose ownership.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BattleOrbitCamera : MonoBehaviour
    {
        public TurnBattleManager battle;
        public Transform arenaRoot;
        public Vector3 localPivot=new Vector3(0,1.85f,0);
        public float homeYaw=90,homePitch=18,homeDistance=6.8f;
        public float minPitch=-85,maxPitch=89,minDistance=2.2f,maxDistance=80;
        public float pointerSensitivity=.14f,keyboardSpeed=75,smoothTime=.08f;
        public float followSmoothTime=.14f,zoomSmoothTime=.12f,shoulderOffset=.55f,actionFocus=.22f;
        public LayerMask obstructionMask=1<<2;
        public bool manualSimulation;
        public bool inputEnabled=true;
        [Tooltip("Open-space orbit: allow the camera below the formation without the ground clamp.")]
        public bool freeOrbit;
        public float environmentFarClip=600;
        public bool ExternallyDriven {get;private set;}
        public float Yaw=>yaw;
        public float Pitch=>pitch;
        public float Distance=>distance;
        public Vector3 Pivot=>arenaRoot?arenaRoot.TransformPoint(followLocal):followLocal;
        Vector3 followLocal,followVelocity;
        float boom,boomVelocity;
        float airFocus,airFocusVelocity;
        Camera lens,externalCamera;
        float yaw,pitch,distance,targetYaw,targetPitch,targetDistance,yawVelocity,pitchVelocity,zoomVelocity;
        bool mouseDrag,touchDrag;
        int activeFinger=-1;
        Vector2 lastTouch;
        float pinchDistance;
        readonly List<RaycastResult> uiHits=new List<RaycastResult>();
        void Awake(){lens=GetComponent<Camera>();ResetView(true);}
        public void ResetView(bool immediate=false)
        {
            targetYaw=homeYaw;targetPitch=homePitch;targetDistance=homeDistance;
            if(immediate){yaw=targetYaw;pitch=targetPitch;distance=targetDistance;yawVelocity=pitchVelocity=zoomVelocity=0;airFocus=battle && battle.player?battle.player.AirborneWeight:0;airFocusVelocity=0;followLocal=DesiredPivot();followVelocity=Vector3.zero;boom=homeDistance+airFocus*2.2f;ApplyPose(.016f);}
        }
        public void Orbit(float degrees,float elevation){if(ExternallyDriven)return;targetYaw+=degrees;targetPitch=Mathf.Clamp(targetPitch+elevation,minPitch,maxPitch);}
        public void Zoom(float amount){if(!ExternallyDriven)targetDistance=Mathf.Clamp(targetDistance+amount,minDistance,maxDistance);}
        public void Step(float dt)
        {
            if(ExternallyDriven)return;
            yaw=Mathf.SmoothDamp(yaw,targetYaw,ref yawVelocity,smoothTime,Mathf.Infinity,dt);
            pitch=Mathf.SmoothDamp(pitch,targetPitch,ref pitchVelocity,smoothTime,Mathf.Infinity,dt);
            distance=Mathf.SmoothDamp(distance,targetDistance,ref zoomVelocity,zoomSmoothTime,Mathf.Infinity,dt);
            airFocus=Mathf.SmoothDamp(airFocus,battle && battle.player?battle.player.AirborneWeight:0,ref airFocusVelocity,.3f,Mathf.Infinity,dt);
            followLocal=Vector3.SmoothDamp(followLocal,DesiredPivot(),ref followVelocity,followSmoothTime,Mathf.Infinity,dt);ApplyPose(dt);
        }
        Vector3 DesiredPivot()
        {
            if(!battle || !battle.player)return localPivot;
            Vector3 playerPosition=arenaRoot?arenaRoot.InverseTransformPoint(battle.player.transform.position):battle.player.transform.position;
            Vector3 focus=playerPosition+localPivot;
            if(battle.InAction && battle.ActionTarget!=null)
            {
                Vector3 target=battle.Source==battle.player?battle.ActionTarget.transform.position:battle.Source.transform.position;
                if(arenaRoot)target=arenaRoot.InverseTransformPoint(target);
                Vector3 delta=Vector3.ClampMagnitude(target-playerPosition,4);
                focus+=delta*Mathf.Lerp(actionFocus,.48f,airFocus);
                focus.y=Mathf.Lerp(focus.y,Mathf.Lerp(playerPosition.y+localPivot.y,target.y+1.5f,.45f),airFocus);
            }
            return focus;
        }
        void LateUpdate(){if(ExternallyDriven || manualSimulation)return;if(inputEnabled)ReadInput();Step(Mathf.Min(Time.unscaledDeltaTime,.1f));}
        void ApplyPose(float dt)
        {
            if(!lens)lens=GetComponent<Camera>();
            lens.orthographic=false;lens.fieldOfView=48;
            Quaternion basis=arenaRoot?arenaRoot.rotation:Quaternion.identity;
            float elevation=Mathf.Clamp(pitch+airFocus*5,minPitch,maxPitch);
            Quaternion orientation=basis*Quaternion.Euler(elevation,yaw,0);
            float scale=arenaRoot?Mathf.Abs(arenaRoot.lossyScale.x):1;
            // A narrow display gets more distance without changing the user's zoom setting.
            float fit=Mathf.Max(1,1.55f/Mathf.Max(.2f,lens.aspect));
            Vector3 pivot=Pivot+orientation*Vector3.right*shoulderOffset*scale;
            float desired=(distance+airFocus*2.2f)*fit;
            // Keep the orbit boom above the floor while allowing the lens to look up at the sky.
            // Clamping the final position alone would collapse the boom beneath the player.
            float pivotHeight=arenaRoot?arenaRoot.InverseTransformPoint(pivot).y:pivot.y;
            float floorPitch=-Mathf.Asin(Mathf.Clamp((pivotHeight-.25f)/Mathf.Max(.01f,desired),-1,1))*Mathf.Rad2Deg;
            Vector3 backward=basis*Quaternion.Euler(freeOrbit?elevation:Mathf.Max(elevation,floorPitch),yaw,0)*Vector3.back;
            if(Physics.SphereCast(pivot,.18f*scale,backward,out RaycastHit hit,desired*scale,obstructionMask,QueryTriggerInteraction.Ignore))desired=Mathf.Max(.8f,hit.distance/scale-.2f);
            // Move in promptly at an obstruction, recover more slowly to avoid oscillation.
            boom=Mathf.SmoothDamp(boom,desired,ref boomVelocity,desired<boom?.04f:.22f,Mathf.Infinity,dt);
            float actual=Mathf.Min(boom,desired);
            Vector3 cameraPosition=pivot+backward*actual*scale;
            if(arenaRoot&&!freeOrbit){Vector3 p=arenaRoot.InverseTransformPoint(cameraPosition);p.y=Mathf.Max(.25f,p.y);cameraPosition=arenaRoot.TransformPoint(p);}
            transform.SetPositionAndRotation(cameraPosition,orientation);
            lens.nearClipPlane=Mathf.Max(.005f,.05f*scale);lens.farClipPlane=Mathf.Max(environmentFarClip,environmentFarClip*scale);
        }
        bool OverUI(Vector2 position)
        {
            if(!EventSystem.current)return false;
            var data=new PointerEventData(EventSystem.current){position=position};uiHits.Clear();EventSystem.current.RaycastAll(data,uiHits);
            foreach(var hit in uiHits)if(hit.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>())return true;
            return battle && (battle.Paused || (battle.hud && battle.hud.debugPanel.activeSelf));
        }
        void ReadInput()
        {
            var keys=Keyboard.current;
            if(keys!=null)
            {
                float x=(keys.eKey.isPressed?1:0)-(keys.qKey.isPressed?1:0);
                float y=(keys.downArrowKey.isPressed?1:0)-(keys.upArrowKey.isPressed?1:0);
                Orbit(x*keyboardSpeed*Time.unscaledDeltaTime,y*keyboardSpeed*Time.unscaledDeltaTime);
                if(keys.homeKey.wasPressedThisFrame)ResetView();
            }
            var touch=Touchscreen.current;int count=0;UnityEngine.InputSystem.Controls.TouchControl first=null,second=null;
            if(touch!=null)foreach(var t in touch.touches)if(t.press.isPressed){if(count==0)first=t;else if(count==1)second=t;count++;}
            if(count>0)
            {
                var p=first.position.ReadValue();
                if(count>=2)
                {
                    float span=Vector2.Distance(p,second.position.ReadValue());
                    if(pinchDistance>0 && touchDrag)Zoom((pinchDistance-span)*.012f);
                    if(pinchDistance==0)touchDrag=!OverUI(p)&&!OverUI(second.position.ReadValue());
                    pinchDistance=span;activeFinger=-1;return;
                }
                pinchDistance=0;
                if(activeFinger!=first.touchId.ReadValue()){activeFinger=first.touchId.ReadValue();touchDrag=!OverUI(p);lastTouch=p;}
                if(touchDrag){var delta=p-lastTouch;Orbit(delta.x*pointerSensitivity,-delta.y*pointerSensitivity);}lastTouch=p;return;
            }
            activeFinger=-1;pinchDistance=0;touchDrag=false;
            var mouse=Mouse.current;if(mouse==null)return;
            if(mouse.rightButton.wasPressedThisFrame)mouseDrag=!OverUI(mouse.position.ReadValue());
            if(mouse.rightButton.wasReleasedThisFrame || !mouse.rightButton.isPressed)mouseDrag=false;
            if(mouseDrag){var delta=mouse.delta.ReadValue();Orbit(delta.x*pointerSensitivity,-delta.y*pointerSensitivity);}
            if(!OverUI(mouse.position.ReadValue()))
            {
                // Input System 1.20 may normalize a wheel notch to 1; older/raw devices report 120.
                float wheel=mouse.scroll.ReadValue().y;
                float notches=Mathf.Abs(wheel)>10?wheel/120f:wheel;
                if(Mathf.Abs(notches)>.001f)Zoom(-Mathf.Clamp(notches,-4,4)*Mathf.Max(.8f,targetDistance*.1f));
            }
        }
        public void UseExternalCamera(Camera camera)
        {
            if(!lens)lens=GetComponent<Camera>();
            if(!camera || camera==lens)return;
            externalCamera=camera;ExternallyDriven=true;mouseDrag=touchDrag=false;lens.enabled=false;
            var listener=GetComponent<AudioListener>();if(listener)listener.enabled=false;
            if(battle){battle.battleCamera=camera;if(battle.earthPrison)battle.earthPrison.viewCamera=camera;}
            // Do not change the tracked camera transform, projection, enabled state or AR components.
        }
        public void RestoreDesktopCamera()
        {
            ExternallyDriven=false;externalCamera=null;lens.enabled=true;
            var listener=GetComponent<AudioListener>();if(listener)listener.enabled=true;
            if(battle){battle.battleCamera=lens;if(battle.earthPrison)battle.earthPrison.viewCamera=lens;}ResetView(true);
        }
        void OnApplicationFocus(bool focus){if(!focus){mouseDrag=touchDrag=false;activeFinger=-1;pinchDistance=0;}}
    }
}

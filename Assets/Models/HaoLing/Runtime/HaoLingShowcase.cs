using UnityEngine;
using UnityEngine.InputSystem;

namespace HaoLing
{
    public sealed class HaoLingShowcase : MonoBehaviour
    {
        public Camera viewCamera;
        public Transform display;
        public Transform feng,huang;
        public Vector3 target=new Vector3(0,2.25f,0);
        public float distance=11.8f,yaw=9,pitch=8;
        Vector2 last; bool dragging; Vector3 fengHome,huangHome; bool animate;
        void Start(){fengHome=feng?feng.localPosition:Vector3.zero;huangHome=huang?huang.localPosition:Vector3.zero;Apply();}
        void Update()
        {
            var mouse=Mouse.current;var touch=Touchscreen.current;
            if(mouse!=null)
            {
                if(mouse.leftButton.wasPressedThisFrame){dragging=true;last=mouse.position.ReadValue();}
                if(mouse.leftButton.wasReleasedThisFrame)dragging=false;
                if(dragging){Vector2 pos=mouse.position.ReadValue();Orbit(pos-last);last=pos;}
                float scroll=mouse.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.1f){distance=Mathf.Clamp(distance-scroll*.006f,4,22);Apply();}
            }
            if(touch!=null && touch.primaryTouch.press.isPressed)
            {
                var t=touch.primaryTouch;if(t.press.wasPressedThisFrame)last=t.position.ReadValue();
                var p=t.position.ReadValue();Orbit(p-last);last=p;
            }
            var keys=Keyboard.current;if(keys!=null)
            {
                if(keys.rKey.wasPressedThisFrame)ResetView();
                if(keys.spaceKey.wasPressedThisFrame)animate=!animate;
                if(keys.digit1Key.wasPressedThisFrame){target=new Vector3(0,2,0);distance=8;yaw=0;pitch=8;Apply();}
                if(keys.digit2Key.wasPressedThisFrame && feng){target=feng.position;distance=7.8f;Apply();}
                if(keys.digit3Key.wasPressedThisFrame && huang){target=huang.position;distance=7.8f;Apply();}
                if(keys.digit4Key.wasPressedThisFrame)ResetView();
            }
            if(feng)feng.localPosition=fengHome+Vector3.up*(animate?Mathf.Sin(Time.time*.8f)*.06f:0);
            if(huang)huang.localPosition=huangHome+Vector3.up*(animate?Mathf.Sin(Time.time*.8f+1.2f)*.06f:0);
        }
        void Orbit(Vector2 delta){yaw+=delta.x*.22f;pitch=Mathf.Clamp(pitch-delta.y*.16f,-8,65);Apply();}
        void Apply(){if(!viewCamera)return;viewCamera.transform.position=target+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);viewCamera.transform.LookAt(target);}
        public void ResetView(){target=new Vector3(0,2.25f,0);distance=11.8f;yaw=9;pitch=8;Apply();}
        void OnGUI()
        {
            GUI.color=new Color(.83f,.74f,.53f,.85f);
            var small=new GUIStyle(GUI.skin.label){fontSize=12,alignment=TextAnchor.LowerCenter};
            GUI.Label(new Rect(0,Screen.height-36,Screen.width,26),"DRAG TO ORBIT     SCROLL TO ZOOM     1  BAI DI     2  FENG     3  HUANG     4 / R  RESET     SPACE  HOVER",small);
        }
    }
}

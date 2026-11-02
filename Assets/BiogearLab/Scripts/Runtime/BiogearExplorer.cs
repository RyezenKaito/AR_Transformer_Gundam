using UnityEngine;
using UnityEngine.InputSystem;

namespace BiogearLab
{
    public sealed class BiogearExplorer : MonoBehaviour
    {
        public Transform[] viewpoints;
        public float speed=12;
        float yaw,pitch;
        bool orbit;
        void Start(){Sync();}
        void Sync(){yaw=transform.eulerAngles.y;pitch=transform.eulerAngles.x;}
        public void View(int index){if(viewpoints==null||index<0||index>=viewpoints.Length||!viewpoints[index])return;transform.SetPositionAndRotation(viewpoints[index].position,viewpoints[index].rotation);var c=GetComponent<Camera>();if(c)c.fieldOfView=(index==3||index==4)?72:55;orbit=false;Sync();}
        void Update()
        {
            var k=Keyboard.current;var m=Mouse.current;if(k==null||m==null)return;
            if(k.digit1Key.wasPressedThisFrame)View(0);if(k.digit2Key.wasPressedThisFrame)View(1);if(k.digit3Key.wasPressedThisFrame)View(2);if(k.digit4Key.wasPressedThisFrame)View(3);if(k.digit5Key.wasPressedThisFrame)View(4);if(k.digit6Key.wasPressedThisFrame)View(5);
            if(k.digit7Key.wasPressedThisFrame)View(6);if(k.digit8Key.wasPressedThisFrame)View(7);
            if(k.cKey.wasPressedThisFrame)orbit=!orbit;
            if(orbit){transform.RotateAround(new Vector3(0,8,5),Vector3.up,Time.unscaledDeltaTime*5);transform.LookAt(new Vector3(0,8,5));Sync();}
            if(m.rightButton.isPressed)
            {
                orbit=false;Vector2 delta=m.delta.ReadValue();yaw+=delta.x*.12f;pitch=Mathf.Clamp(pitch-delta.y*.12f,-85,85);transform.rotation=Quaternion.Euler(pitch,yaw,0);
                Vector3 v=new Vector3((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.eKey.isPressed?1:0)-(k.qKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
                transform.position+=transform.TransformDirection(Vector3.ClampMagnitude(v,1))*speed*(k.leftShiftKey.isPressed?3:1)*Time.unscaledDeltaTime;
            }
        }
        void OnGUI()
        {
            GUI.Box(new Rect(14,Screen.height-77,Mathf.Min(Screen.width-28,950),64),"BIOGEAR LAB  |  1 Tổng thể   2 Đấu trường   3 Nhà vòm   4 Nội thất   5 Hành lang   6 Tháp\nGiữ chuột phải + WASD: bay · Q/E: xuống/lên · Shift: nhanh · C: xoay quanh");
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace SkyDefender
{
    public sealed class SkyDefenderViewer : MonoBehaviour
    {
        public Camera viewCamera;
        public Transform wings;
        public Vector3 target = new Vector3(0, 2.7f, 0);
        public float yaw = 16, pitch = 5, distance = 13.6f;
        Vector2 previous;
        bool dragging;
        void Start() { Apply(); }
        void Update()
        {
            var m = Mouse.current;
            if (m != null)
            {
                if (m.leftButton.wasPressedThisFrame) { previous = m.position.ReadValue(); dragging = true; }
                if (m.leftButton.wasReleasedThisFrame) dragging = false;
                if (dragging) { var p = m.position.ReadValue(); Orbit(p - previous); previous = p; }
                var scroll = m.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > .1f) { distance = Mathf.Clamp(distance - scroll * .007f, 2, 24); Apply(); }
            }
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                var t = touch.primaryTouch;
                if (t.press.wasPressedThisFrame) previous = t.position.ReadValue();
                var p = t.position.ReadValue(); Orbit(p - previous); previous = p;
            }
            var k = Keyboard.current;
            if (k == null) return;
            if (k.rKey.wasPressedThisFrame || k.digit1Key.wasPressedThisFrame) SetView(16, 5, 13.6f, new Vector3(0, 2.7f, 0));
            if (k.digit2Key.wasPressedThisFrame) SetView(0, 3, 10, new Vector3(0, 2.25f, 0));
            if (k.digit3Key.wasPressedThisFrame) SetView(180, 8, 15.5f, new Vector3(0, 2.7f, 0));
            if (k.digit4Key.wasPressedThisFrame) SetView(10, 2, 4, new Vector3(0, 3.95f, 0));
            if (k.wKey.wasPressedThisFrame && wings) wings.gameObject.SetActive(!wings.gameObject.activeSelf);
        }
        void Orbit(Vector2 d) { yaw += d.x * .20f; pitch = Mathf.Clamp(pitch - d.y * .15f, -12, 75); Apply(); }
        public void SetView(float y, float p, float d, Vector3 t) { yaw = y; pitch = p; distance = d; target = t; Apply(); }
        void Apply() { if (!viewCamera) return; viewCamera.transform.position = target + Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -distance); viewCamera.transform.LookAt(target); }
        void OnGUI()
        {
            var label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter, fontSize = 12 };
            GUI.color = new Color(.8f, .76f, .65f, .85f);
            GUI.Label(new Rect(0, Screen.height - 34, Screen.width, 25), "DRAG  ORBIT     SCROLL  ZOOM     1  HERO     2  FRONT     3  REAR     4  HEAD     W  WINGS", label);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace GundamGame.UnicornPrototype
{
    public sealed class UnicornMotionViewer : MonoBehaviour
    {
        public Animator animator;
        public Camera previewCamera;
        public Vector3 cameraTarget = new Vector3(0, 1.5f, 0);
        public float playbackSpeed = 1f;
        public bool paused;
        float yaw = 25, pitch = 9, distance = 6.2f;
        GUIStyle title, subtitle, button;

        public void PlayHit() { animator.ResetTrigger("Hit"); animator.SetTrigger("Hit"); }
        public void PlayIdle() { animator.ResetTrigger("Hit"); animator.CrossFadeInFixedTime("Idle", .12f); }
        void Update()
        {
            animator.speed = paused ? 0 : playbackSpeed;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw += delta.x * .2f; pitch = Mathf.Clamp(pitch - delta.y * .15f, -10, 55);
                }
                distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y * .0025f, 3.6f, 9);
            }
        }
        void LateUpdate()
        {
            if (!previewCamera) return;
            previewCamera.transform.position = cameraTarget + Quaternion.Euler(-pitch, yaw, 0) * Vector3.forward * distance;
            previewCamera.transform.LookAt(cameraTarget);
        }
        void OnGUI()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold };
                subtitle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                button = new GUIStyle(GUI.skin.button) { fontSize = 17 };
            }
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * (Screen.width / 1280f));
            float height = Screen.height * 1280f / Screen.width;
            GUI.color = new Color(.06f,.10f,.15f,.94f); GUI.DrawTexture(new Rect(0, 0, 1280, 94), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, height - 99, 1280, 99), Texture2D.whiteTexture);
            GUI.color = new Color(.45f,.94f,1);
            GUI.Label(new Rect(28, 15, 1000, 38), "UNICORN  /  MOTION PROTOTYPE", title);
            GUI.color = Color.white;
            GUI.Label(new Rect(30, 57, 1150, 26), "Blender animation > FBX > Unity Animator  |  17 bones / rigid armor", subtitle);
            float y = height - 78;
            if (GUI.Button(new Rect(30,y,150,46), "IDLE  /  4.0s", button)) PlayIdle();
            if (GUI.Button(new Rect(194,y,200,46), "HIT REACT  /  0.8s", button)) PlayHit();
            if (GUI.Button(new Rect(408,y,135,46), paused ? "RESUME" : "PAUSE", button)) paused = !paused;
            GUI.Label(new Rect(568,y,190,26), "Speed: " + playbackSpeed.ToString("0.00") + "x", subtitle);
            playbackSpeed = GUI.HorizontalSlider(new Rect(568,y+31,200,22), playbackSpeed,.25f,1.5f);
            GUI.Label(new Rect(820,y,430,28), "Right-drag: orbit  |  Wheel: zoom", subtitle);
            GUI.Label(new Rect(820,y+30,430,26), "Hit React returns to Idle automatically", subtitle);
            GUI.matrix = old;
        }
    }
}

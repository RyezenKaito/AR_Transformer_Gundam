using UnityEngine;

namespace TurnBasedBattle
{
    [RequireComponent(typeof(Camera))]
    public sealed class FixedBattleCamera : MonoBehaviour
    {
        public float referenceAspect = 16f/9f;
        public float verticalSize = 6.15f;
        Camera lens;
        float lastAspect;
        public Vector3 SetupPosition { get; private set; }
        public Quaternion SetupRotation { get; private set; }
        void Awake()
        {
            lens=GetComponent<Camera>(); SetupPosition=transform.position; SetupRotation=transform.rotation;
            Fit();
        }
        void LateUpdate() { if(lens != null && Mathf.Abs(lens.aspect-lastAspect) > .001f) Fit(); }
        void Fit() { lastAspect=lens.aspect; lens.orthographic=true; lens.orthographicSize=verticalSize*Mathf.Max(1,referenceAspect/Mathf.Max(.1f,lastAspect)); }
    }
}

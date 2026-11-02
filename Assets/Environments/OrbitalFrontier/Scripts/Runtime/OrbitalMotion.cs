using UnityEngine;

namespace OrbitalFrontier
{
    /// <summary>Pure scenery motion. Never moves combat actors or changes global gravity.</summary>
    public sealed class OrbitalMotion : MonoBehaviour
    {
        public Vector3 angularVelocity=new Vector3(0,.35f,0);
        public Vector3 driftAxis;
        public float driftAmplitude,driftFrequency=.018f,phase;
        Vector3 origin;
        Quaternion rotation;
        float elapsed;
        void Awake(){origin=transform.localPosition;rotation=transform.localRotation;}
        public void Step(float dt)
        {
            elapsed+=Mathf.Max(0,dt);
            transform.localRotation=rotation*Quaternion.Euler(angularVelocity*elapsed);
            transform.localPosition=origin+driftAxis.normalized*driftAmplitude*(Mathf.Sin(elapsed*driftFrequency+phase)-Mathf.Sin(phase));
        }
        void Update(){Step(Time.deltaTime);}
    }
}

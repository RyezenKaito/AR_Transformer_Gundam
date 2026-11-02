using UnityEngine;

namespace OrbitalFrontier
{
    /// <summary>A pooled scenic asteroid pass. Tails are analytic, so wrapping never draws a teleport streak.</summary>
    public sealed class OrbitalFlyby : MonoBehaviour
    {
        public Vector3 start,end;
        public float duration=8,phase,tailLength=18;
        public LineRenderer core,wake;
        public bool manualSimulation;
        public float FlightPhase { get; private set; }
        public int CompletedLoops { get; private set; }
        public float VisibleTail { get; private set; }
        float elapsed;
        void OnEnable(){elapsed=0;CompletedLoops=0;Step(0);}
        public void Step(float dt)
        {
            elapsed+=Mathf.Max(0,dt);
            float cycle=elapsed/Mathf.Max(.1f,duration)+phase;
            CompletedLoops=Mathf.FloorToInt(cycle)-Mathf.FloorToInt(phase);
            FlightPhase=Mathf.Repeat(cycle,1);
            transform.localPosition=Vector3.Lerp(start,end,FlightPhase);
            Vector3 path=end-start;float length=path.magnitude;
            float fade=Mathf.SmoothStep(0,1,FlightPhase/.06f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.90f,1,FlightPhase)));
            VisibleTail=Mathf.Min(tailLength,FlightPhase*length)*fade;
            Vector3 backwards=-path.normalized;
            Draw(core,backwards);Draw(wake,backwards);
        }
        void Draw(LineRenderer line,Vector3 backwards){if(!line)return;line.enabled=VisibleTail>.03f;for(int i=0;i<line.positionCount;i++)line.SetPosition(i,backwards*(i/(float)(line.positionCount-1))*VisibleTail);}
        void Update(){if(!manualSimulation)Step(Time.deltaTime);}
    }
}

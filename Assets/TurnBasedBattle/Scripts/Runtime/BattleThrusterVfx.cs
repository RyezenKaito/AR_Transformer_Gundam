using UnityEngine;

namespace TurnBasedBattle
{
    /// <summary>Additive camera-facing jet ribbons: no opaque placeholder mesh, collider or shadow.</summary>
    public sealed class BattleThrusterVfx : MonoBehaviour
    {
        public LineRenderer core,halo,flash;
        public void Evaluate(float thrust,float time)
        {
            float intensity=Mathf.Clamp01(thrust);
            float flicker=.9f+.06f*Mathf.Sin(time*43)+.04f*Mathf.Sin(time*71);
            float length=Mathf.Lerp(.16f,.95f,intensity)*flicker;
            Set(core,.06f*intensity,new Color(.7f,2.4f,3,intensity),length);
            Set(halo,.21f*Mathf.Sqrt(intensity),new Color(.08f,.65f,1.6f,intensity*.45f),length*1.15f);
            if(flash)
            {
                flash.widthMultiplier=.16f*Mathf.Sqrt(intensity)*flicker;
                flash.startColor=flash.endColor=new Color(.4f,1.6f,2,intensity*.7f);
            }
        }
        static void Set(LineRenderer line,float width,Color color,float length)
        {
            if(!line)return;
            line.SetPosition(0,Vector3.zero);line.SetPosition(1,Vector3.forward*length);
            line.widthMultiplier=width;line.startColor=line.endColor=color;
        }
    }
}

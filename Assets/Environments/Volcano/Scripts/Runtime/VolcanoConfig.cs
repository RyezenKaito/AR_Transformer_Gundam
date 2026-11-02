using UnityEngine;

namespace VolcanoArena
{
    [CreateAssetMenu(menuName="Environments/Volcano Config")]
    public sealed class VolcanoConfig : ScriptableObject
    {
        public int seed=83417;
        public Vector2 arenaSize=new Vector2(54,40);
        public float lavaHeight=-1.4f;
        [Range(3,16)] public int mainLavaStreams=11;
        [Range(0,1)] public float surroundingLavaCoverage=.8f;
        public Vector3 mainVolcanoPosition=new Vector3(210,-9,-55);
        [Range(0,10)] public float lavaEmission=2.4f;
        [Range(0,1)] public float crustCoverage=.58f;
        [Range(0,2)] public float flowSpeed=.22f;
        [Range(0,1)] public float flowWaves=.45f;
        [Range(0,1)] public float lavaVapourDensity=.9f;
        [Range(0,1)] public float lavaVapourOpacity=.9f;
        [Range(0,1)] public float ashDensity=.4f;
        [Range(0,1)] public float smokeDensity=.8f;
        [Range(0,1)] public float mountainSmokeDensity=.85f;
        [Range(0,1)] public float skySmokeDensity=.75f;
        [Range(0,1)] public float skyAshCoverage=.82f;
        [Range(0,1)] public float ambienceVolume=.15f;
        public bool lowQuality;
        public float eruptionInterval=17;
    }
}

using UnityEngine;

namespace VolcanoArena
{
    public sealed class VolcanoAtmosphere : MonoBehaviour
    {
        public VolcanoConfig config;
        public Renderer[] lavaSurfaces;
        public Light[] lavaLights;
        public ParticleSystem eruption;
        public ParticleSystem ash,smoke;
        public ParticleSystem[] lavaVapour;
        public ParticleSystem[] mountainPlumes,skySmoke;
        public AudioSource ambience;
        float elapsed;
        MaterialPropertyBlock properties;
        void OnEnable(){ApplySettings();elapsed=0;}
        [ContextMenu("Apply runtime atmosphere settings")]
        public void ApplySettings()
        {
            if(!config)return;
            if(properties==null)properties=new MaterialPropertyBlock();
            foreach(var r in lavaSurfaces)if(r)
            {
                r.GetPropertyBlock(properties);properties.SetFloat("_Emission",config.lavaEmission);
                bool slope=r.name.StartsWith("Volcano lava descent")||r.name.Contains("cascade")||r.name.Contains("branch")||r.name.StartsWith("Lavafall");
                properties.SetFloat("_Crust",config.crustCoverage);properties.SetFloat("_Speed",config.flowSpeed*(slope?2.2f:1));properties.SetFloat("_Motion",config.flowWaves);r.SetPropertyBlock(properties);
            }
            if(lavaVapour!=null)foreach(var p in lavaVapour)if(p)
            {
                SetRate(p,config.lavaVapourDensity*(config.lowQuality?2.5f:6));
                var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.GetPropertyBlock(properties);properties.SetFloat("_Opacity",config.lavaVapourOpacity);renderer.SetPropertyBlock(properties);
            }
            if(ambience)ambience.volume=config.ambienceVolume;
            SetRate(ash,config.ashDensity*(config.lowQuality?30:65));
            SetRate(smoke,config.smokeDensity*(config.lowQuality?4:8));
            if(mountainPlumes!=null)foreach(var p in mountainPlumes)SetRate(p,config.mountainSmokeDensity*(config.lowQuality?1.5f:3.5f));
            if(skySmoke!=null)foreach(var p in skySmoke)SetRate(p,config.skySmokeDensity*(config.lowQuality?.18f:.45f));
        }
        static void SetRate(ParticleSystem ps,float rate){if(ps){var e=ps.emission;e.rateOverTime=rate;}}
        void Update()
        {
            if(!config)return;
            elapsed+=Time.deltaTime;
            if(elapsed>=Mathf.Max(6,config.eruptionInterval))
            {
                elapsed=0;if(eruption)eruption.Emit(config.lowQuality?12:35);
            }
            for(int i=0;i<lavaLights.Length;i++)if(lavaLights[i])
                lavaLights[i].intensity=1.6f+.16f*Mathf.Sin(Time.time*.7f+i*1.93f)+.08f*Mathf.Sin(Time.time*1.7f+i);
        }
    }
}

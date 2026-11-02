using UnityEngine;
namespace SunkenHarbor
{
 public sealed class HarborAtmosphere:MonoBehaviour
 {
  public HarborConfig config;public Transform ocean;public Material optics;public Material[] materials;
  public ParticleSystem[] particles;public AudioSource ambience;
  float level=-999,density=-999;Vector3 absorption;Color scatter;
  void OnEnable(){Apply();}
  [ContextMenu("Apply water settings")]
  public void Apply()
  {
   if(!config)return;level=config.waterLevel;density=config.particleDensity;absorption=config.absorption;scatter=config.scatter;
   if(ocean){var p=ocean.position;p.y=level;ocean.position=p;}
   if(optics){optics.SetFloat("_WaterLevel",level);optics.SetVector("_Absorption",absorption);optics.SetColor("_Scatter",scatter);optics.SetFloat("_Visibility",config.visibility);}
   if(materials!=null)foreach(var m in materials)if(m&&m.HasProperty("_WaterLevel"))m.SetFloat("_WaterLevel",level);
   if(particles!=null)foreach(var p in particles)if(p){var e=p.emission;e.rateOverTime=(p.name.Contains("Bubble")?1.8f:4)*density*(config.lowQuality?.5f:1);}
   if(ambience)ambience.volume=config.ambienceVolume;
  }
  void Update(){if(config&&(level!=config.waterLevel||density!=config.particleDensity||absorption!=config.absorption||scatter!=config.scatter))Apply();if(config&&ambience&&Camera.main){float factor=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(config.waterLevel-.5f,config.waterLevel+.5f,Camera.main.transform.position.y));ambience.volume=config.ambienceVolume*Mathf.Lerp(.25f,1,factor);}}
 }
}

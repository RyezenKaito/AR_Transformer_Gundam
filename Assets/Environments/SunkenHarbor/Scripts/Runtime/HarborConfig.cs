using UnityEngine;
namespace SunkenHarbor
{
 [CreateAssetMenu(menuName="Environments/Sunken Harbor Settings")]
 public sealed class HarborConfig:ScriptableObject
 {
  public int seed=73129;
  public float waterLevel=42;
  public Vector3 absorption=new Vector3(.028f,.015f,.010f);
  public Color scatter=new Color(.045f,.20f,.23f);
  [Range(30,250)]public float visibility=150;
  [Range(0,2)]public float particleDensity=1;
  [Range(0,1)]public float ambienceVolume=.12f;
  public bool lowQuality;
 }
}

using UnityEngine;
namespace GundamLobby
{
 public sealed class GundamSkinBinding : MonoBehaviour
 {
  public string unitId;public HomeLobbyDashboard dashboard;public int previewSkin=-1;
  int version=-1,lastSkin=-1,lastModel;
  public static Color Tint(int skin)=>skin==1?new Color(.25f,.42f,.61f):skin==2?new Color(.74f,.22f,.29f):Color.white;
  void LateUpdate()
  {
   string id=dashboard&&dashboard.ActiveUnit!=null?dashboard.ActiveUnit.id:unitId;
   int skin=previewSkin>=0?previewSkin:PlayerProgress.EquippedSkin(id);
   int model=dashboard&&dashboard.lobby.PreviewModel?dashboard.lobby.PreviewModel.GetInstanceID():transform.childCount>0?transform.GetChild(0).GetInstanceID():GetInstanceID();
   if(version==PlayerProgress.Version&&lastSkin==skin&&lastModel==model)return;
   version=PlayerProgress.Version;lastSkin=skin;lastModel=model;Apply(gameObject,skin);
   if(Application.isPlaying&&!string.IsNullOrEmpty(id)){var rig=GetComponent<GundamPartRig>();if(!rig)rig=gameObject.AddComponent<GundamPartRig>();rig.Configure(id,GetComponent<WeaponryModelMarker>()!=null);}
  }
  public static void Apply(GameObject root,int skin)
  {
   var block=new MaterialPropertyBlock();
   foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
   {
    if(renderer.GetComponentInParent<PartAddon>())continue;
    if(renderer is ParticleSystemRenderer||renderer is LineRenderer||renderer is TrailRenderer)continue;
    var materials=renderer.sharedMaterials;
    for(int i=0;i<materials.Length;i++)
    {
     var material=materials[i];if(!material)continue;string colorProperty=null;foreach(string candidate in new[]{"_BaseColor","baseColorFactor","_BaseColorFactor","_Color"})if(material.HasProperty(candidate)){colorProperty=candidate;break;}if(colorProperty==null)continue;
     string name=material.name.ToLowerInvariant();if(name.Contains("eye")||name.Contains("psycho")||name.Contains("saber")||name.Contains("glow"))continue;
     block.Clear();renderer.GetPropertyBlock(block,i);block.SetColor(colorProperty,material.GetColor(colorProperty)*Tint(skin));renderer.SetPropertyBlock(block,i);
    }
   }
  }
 }
}

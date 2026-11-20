using System.Linq;
using UnityEngine;
namespace GundamLobby
{
 // Anchors are derived from model proportions and follow the existing animated bones.
 // Assign replacement anchors in the Inspector when using a different model rig.
 public sealed class GundamPartRig:MonoBehaviour
 {
  public Transform[] anchors=new Transform[7];
  public BoxCollider[] targets=new BoxCollider[7];
  public string unitId;
  public Bounds modelBounds;
  GameObject attachments;int version=-1;bool picking;
  public static Bounds BoundsOf(Transform root)
  {
   bool set=false;var bounds=new Bounds();
   foreach(var r in root.GetComponentsInChildren<Renderer>()){
    if(r is ParticleSystemRenderer||r is TrailRenderer||r is LineRenderer||r.GetComponentInParent<PartAddon>())continue;
    var b=r.bounds;for(int i=0;i<8;i++){var p=root.InverseTransformPoint(new Vector3((i&1)==0?b.min.x:b.max.x,(i&2)==0?b.min.y:b.max.y,(i&4)==0?b.min.z:b.max.z));if(!set){bounds=new Bounds(p,Vector3.zero);set=true;}else bounds.Encapsulate(p);}
   }
   return bounds;
  }
  public void Configure(string id,bool enablePicking=false)
  {
   unitId=id;picking=enablePicking;
   if(!anchors[0])Build();
   foreach(var target in targets)if(target)target.enabled=picking;
   Refresh();
  }
  void Build()
  {
   modelBounds=BoundsOf(transform);float h=modelBounds.size.y;
   var positions=new[]{new Vector3(0,.92f,-.015f),new Vector3(0,.65f,-.07f),new Vector3(.18f,.60f,-.03f),new Vector3(-.18f,.60f,-.03f),new Vector3(.08f,.25f,-.015f),new Vector3(-.08f,.25f,-.015f),new Vector3(0,.68f,.11f)};
   var sizes=new[]{new Vector3(.15f,.16f,.19f),new Vector3(.25f,.30f,.22f),new Vector3(.17f,.42f,.22f),new Vector3(.17f,.42f,.22f),new Vector3(.16f,.50f,.20f),new Vector3(.16f,.50f,.20f),new Vector3(.28f,.30f,.16f)};
   var bones=GetComponentsInChildren<Transform>(true);
   for(int i=0;i<7;i++){
    if(!anchors[i]){
     var go=new GameObject("Mount / "+GundamEquipment.Name((EquipmentSlot)i));go.transform.SetParent(transform,false);
     go.transform.localPosition=new Vector3(modelBounds.center.x+positions[i].x*h,modelBounds.min.y+positions[i].y*h,modelBounds.center.z+positions[i].z*h);
     var bone=bones.FirstOrDefault(t=>Matches(t.name,(EquipmentSlot)i));if(bone)go.transform.SetParent(bone,true);anchors[i]=go.transform;
    }
    var hit=new GameObject("Pick / "+(EquipmentSlot)i);hit.layer=31;hit.transform.SetParent(anchors[i],false);
    // Keep proxy dimensions in model space even when bones use imported scales.
    hit.transform.localScale=Vector3.one;var scale=hit.transform.lossyScale;var rootScale=transform.lossyScale;
    var box=hit.AddComponent<BoxCollider>();box.size=new Vector3(h*sizes[i].x*rootScale.x/Mathf.Max(.0001f,Mathf.Abs(scale.x)),h*sizes[i].y*rootScale.y/Mathf.Max(.0001f,Mathf.Abs(scale.y)),h*sizes[i].z*rootScale.z/Mathf.Max(.0001f,Mathf.Abs(scale.z)));box.isTrigger=true;box.enabled=picking;targets[i]=box;
   }
  }
  static bool Matches(string name,EquipmentSlot s)
  {
   var n=name.ToLowerInvariant();return s switch{
    EquipmentSlot.Head=>n=="head"||n.StartsWith("head_"),
    EquipmentSlot.Core=>n=="body"||n.StartsWith("chest_"),
    EquipmentSlot.LeftArm=>n=="forearml"||n.StartsWith("lower_arm.l"),
    EquipmentSlot.RightArm=>n=="forearmr"||n.StartsWith("lower_arm.r"),
    EquipmentSlot.LeftLeg=>n=="shinl"||n.StartsWith("lower_leg.l"),
    EquipmentSlot.RightLeg=>n=="shinr"||n.StartsWith("lower_leg.r"),
    EquipmentSlot.Backpack=>n=="backpack"||n.StartsWith("chest_"),_=>false};
  }
  public void Refresh()
  {
   if(!anchors[0]||string.IsNullOrEmpty(unitId))return;
   if(attachments){attachments.SetActive(false);Destroy(attachments);}
   attachments=new GameObject("Loadout Add-ons");attachments.transform.SetParent(transform,false);version=PlayerProgress.Version;
   for(int i=0;i<7;i++){
    string id=PlayerProgress.PartModule(unitId,(EquipmentSlot)i);var prefab=MechaItemVisuals.Prefab(id);if(!prefab)continue;
    var holder=new GameObject("Mounted / "+(EquipmentSlot)i);holder.transform.SetParent(attachments.transform,false);holder.AddComponent<PartAddon>();
    var model=Instantiate(prefab,holder.transform);foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=gameObject.layer;
    foreach(var c in model.GetComponentsInChildren<Collider>())c.enabled=false;
    var bounds=BoundsOfAddon(model);float extent=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
    float size=modelBounds.size.y*(i==0?.045f:i==6?.085f:.06f)*Mathf.Abs(transform.lossyScale.y);
    float ratio=size/Mathf.Max(.001f,extent);model.transform.localScale*=ratio;model.transform.position-=bounds.center*ratio-model.transform.position*ratio;
    holder.AddComponent<PartAddonFollower>().anchor=anchors[i];holder.transform.SetPositionAndRotation(anchors[i].position,anchors[i].rotation);
   }
  }
  static Bounds BoundsOfAddon(GameObject root){var rs=root.GetComponentsInChildren<Renderer>();var b=new Bounds(root.transform.position,Vector3.zero);if(rs.Length>0){b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);}return b;}
  void LateUpdate(){if(version!=PlayerProgress.Version)Refresh();}
  public bool Pick(Camera camera,Vector2 screen,out EquipmentSlot slot)
  {
   Physics.SyncTransforms();var hits=Physics.RaycastAll(camera.ScreenPointToRay(screen),camera.farClipPlane,1<<31,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance);
   foreach(var hit in hits)for(int i=0;i<targets.Length;i++)if(hit.collider==targets[i]){slot=(EquipmentSlot)i;return true;}
   slot=EquipmentSlot.Core;return false;
  }
 }
 public sealed class PartAddon:MonoBehaviour{}
 public sealed class PartAddonFollower:MonoBehaviour
 {
  public Transform anchor;
  void LateUpdate(){if(anchor)transform.SetPositionAndRotation(anchor.position,anchor.rotation);}
 }
}

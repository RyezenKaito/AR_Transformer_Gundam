using System;
using UnityEngine;
namespace GundamLobby
{
 [CreateAssetMenu(menuName="Gundam/Mecha Item Visuals")]
 public sealed class MechaItemVisuals:ScriptableObject
 {
  [Serializable] public class Entry{public string id;public Sprite icon;public GameObject prefab;}
  public Entry[] items;
  static MechaItemVisuals cached;
  public static GameObject Prefab(string id){if(!cached)cached=Resources.Load<MechaItemVisuals>("MechaItemVisuals");return cached&&cached.items!=null?Array.Find(cached.items,t=>t.id==id)?.prefab:null;}
  public static Sprite Icon(string id){if(!cached)cached=Resources.Load<MechaItemVisuals>("MechaItemVisuals");if(!cached||cached.items==null)return null;return Array.Find(cached.items,t=>t.id==id)?.icon;}
 }
}
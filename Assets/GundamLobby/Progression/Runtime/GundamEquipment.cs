using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GundamLobby
{
 public enum EquipmentSlot {Head,Core,LeftArm,RightArm,LeftLeg,RightLeg,Backpack}
 public static class GundamEquipment
 {
  public static readonly EquipmentSlot[] Slots=(EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
  public static string Name(EquipmentSlot s)=>s switch{EquipmentSlot.Core=>"TORSO / REACTOR",EquipmentSlot.LeftArm=>"LEFT ARM",EquipmentSlot.RightArm=>"RIGHT ARM",EquipmentSlot.LeftLeg=>"LEFT LEG",EquipmentSlot.RightLeg=>"RIGHT LEG",_=>s.ToString().ToUpperInvariant()};
  public static string Short(EquipmentSlot s)=>s switch{EquipmentSlot.Core=>"TORSO",EquipmentSlot.LeftArm=>"L. ARM",EquipmentSlot.RightArm=>"R. ARM",EquipmentSlot.LeftLeg=>"L. LEG",EquipmentSlot.RightLeg=>"R. LEG",_=>Name(s)};
  public static bool Fits(string id,EquipmentSlot s)=>id switch{
   "precision_module"=>s==EquipmentSlot.Head||s==EquipmentSlot.RightArm,
   "balanced_module"=>s==EquipmentSlot.Head||s==EquipmentSlot.Core||s==EquipmentSlot.Backpack,
   "defense_module"=>s==EquipmentSlot.Core||s==EquipmentSlot.LeftArm,
   "bulwark_module"=>s==EquipmentSlot.LeftArm||s==EquipmentSlot.LeftLeg||s==EquipmentSlot.RightLeg,
   "assault_module"=>s==EquipmentSlot.RightArm||s==EquipmentSlot.Backpack,
   "breaker_module"=>s==EquipmentSlot.RightArm,
   "thruster_module"=>s==EquipmentSlot.LeftLeg||s==EquipmentSlot.RightLeg||s==EquipmentSlot.Backpack,_=>false};
  public static EquipmentSlot Preferred(string id)=>id switch{"precision_module"=>EquipmentSlot.Head,"bulwark_module"=>EquipmentSlot.LeftArm,"thruster_module"=>EquipmentSlot.Backpack,"defense_module" or "balanced_module"=>EquipmentSlot.Core,_=>EquipmentSlot.RightArm};
  public static string Compatible(string id)=>string.Join(" / ",Slots.Where(s=>Fits(id,s)).Select(Short));
  public static float Bonus(string id,string stat)=>id switch{
   "assault_module" when stat=="attack"=>.15f,
   "precision_module" when stat=="attack"=>.20f,
   "bulwark_module" when stat=="hp"=>.15f,
   "bulwark_module" when stat=="armor"=>.20f,
   "breaker_module" when stat=="break"=>.30f,
   "defense_module" when stat=="defense"=>.20f,
   "balanced_module" when stat=="hp"||stat=="attack"||stat=="defense"=>.08f,_=>0};
  public static int Speed(string id)=>id=="thruster_module"?10:id=="precision_module"?-5:id=="balanced_module"?4:0;
 }
 public static partial class PlayerProgress
 {
  [Serializable] public sealed class PartEquipment {public EquipmentSlot slot;public string itemId;public PartEquipment(EquipmentSlot s,string id){slot=s;itemId=id;}}
  static List<PartEquipment> Parts(string unitId)
  {
   var u=Unit(unitId);u.parts??=new List<PartEquipment>();
   if(!string.IsNullOrEmpty(u.module)){
    if(!u.parts.Any(p=>p.slot==GundamEquipment.Preferred(u.module))&&GameItems.IsEquipment(GameItems.Find(u.module)))u.parts.Add(new PartEquipment(GundamEquipment.Preferred(u.module),u.module));
    u.module="";
   }
   return u.parts;
  }
  public static string PartModule(string unitId,EquipmentSlot slot)=>Parts(unitId).Find(p=>p.slot==slot)?.itemId??"";
  public static int EquippedPartCount(string unitId)=>Parts(unitId).Count(p=>!string.IsNullOrEmpty(p.itemId));
  public static int MountedCopies(string unitId,string itemId,EquipmentSlot except)=>Parts(unitId).Count(p=>p.slot!=except&&p.itemId==itemId);
  public static bool CanEquipPart(string unitId,EquipmentSlot slot,string id,out string message)
  {
   if(!GundamEquipment.Slots.Contains(slot)||!OwnsGundam(unitId)){message="Own this Gundam first.";return false;}
   if(string.IsNullOrEmpty(id)){message="Return to stock configuration.";return true;}
   if(!GundamEquipment.Fits(id,slot)){message="This module is incompatible with "+GundamEquipment.Name(slot)+".";return false;}
   if(Count(id)<=MountedCopies(unitId,id,slot)){message="No spare copy. Remove it from another part or buy another copy.";return false;}
   message="Ready to install.";return true;
  }
  public static bool EquipPart(string unitId,EquipmentSlot slot,string id,out string message)
  {
   id??="";if(!CanEquipPart(unitId,slot,id,out message))return false;
   if(PartModule(unitId,slot)==id){message="This configuration is already installed.";return true;}
   var parts=Parts(unitId);parts.RemoveAll(p=>p.slot==slot);if(id!="")parts.Add(new PartEquipment(slot,id));
   Data.uses++;Log(unitId+" / "+GundamEquipment.Name(slot)+": "+(id==""?"STOCK":GameItems.Find(id).name));Save();
   message=GundamEquipment.Name(slot)+(id==""?" restored to stock. Module returned to storage.":" equipped. Stats updated for combat.");return true;
  }
  static int LoadoutStat(string id,int value,string stat,EquipmentSlot? replace=null,string candidate="")
  {
   float bonus=0;int speed=0;
   foreach(var slot in GundamEquipment.Slots){string item=replace.HasValue&&replace.Value==slot?candidate:PartModule(id,slot);bonus+=GundamEquipment.Bonus(item,stat);speed+=GundamEquipment.Speed(item);}
   if(stat=="speed")return Mathf.Max(1,value+Rank(id)/2+speed);
   float multiplier=(1+Rank(id)*.05f)*(1+bonus);
   if(SortieSession.BoosterUnitId==id){if(stat=="hp"&&SortieSession.BoosterId=="repair_gel")multiplier*=1.15f;if(stat=="attack"&&SortieSession.BoosterId=="overdrive_amp")multiplier*=1.12f;}
   return Mathf.RoundToInt(value*multiplier);
  }
  public static int PreviewPartStat(string id,int value,string stat,EquipmentSlot slot,string candidate)=>LoadoutStat(id,value,stat,slot,candidate);
 }
}

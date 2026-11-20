using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GundamLobby
{
 public static partial class PlayerProgress
 {
  [Serializable] public sealed class ItemStack {public string id;public int count;public ItemStack(string id,int count){this.id=id;this.count=count;}}
  [Serializable] public sealed class UnitUpgrade {public string id,module,booster;public int rank;public List<PartEquipment> parts;}
  static void InitializeItems(SaveData s)
  {
   s.items??=new List<ItemStack>();s.units??=new List<UnitUpgrade>();s.claims??=new List<string>();s.history??=new List<string>();
   if(s.schema<2){s.items.Add(new ItemStack("xp_chip",3));s.items.Add(new ItemStack("credit_cache",1));s.items.Add(new ItemStack("assault_module",1));s.schema=2;}
   if(s.schema<3)
   {
    long refund=s.items.Where(t=>t.id=="energy_cell").Sum(t=>(long)Mathf.Max(0,t.count)*250);
    s.items.RemoveAll(t=>t.id=="energy_cell");s.credits=(int)Math.Min(int.MaxValue,(long)s.credits+refund);s.schema=3;
    if(refund>0)s.history.Insert(0,"REACTOR CELL RETIRED  /  +"+refund+" CREDITS REFUNDED");
   }
  }
  static void Log(string text){Data.history.Insert(0,DateTime.Now.ToString("MM-dd HH:mm")+"  "+text);if(Data.history.Count>50)Data.history.RemoveAt(50);}
  public static string[] History=>Data.history.ToArray();
  public static string CommanderName=>string.IsNullOrWhiteSpace(Data.commanderName)?"AntAkA":Data.commanderName;
  public static void Rename(string name){name=(name??"").Trim();if(name.Length==0)return;Data.commanderName=name.Substring(0,Math.Min(20,name.Length));Save();}
  public static int Count(string id)=>id=="fcore"?FCores:Data.items.Where(s=>s.id==id).Sum(s=>s.count);
  public static int UsedSlots=>1+Data.items.Count(t=>t.count>0);
  public static bool CanReceive(params ItemStack[] stacks)=>CanReceiveAfterConsuming(null,stacks);
  static bool CanReceiveAfterConsuming(string consumed,ItemStack[] stacks)
  {
   var amounts=new Dictionary<string,long>();foreach(var stack in Data.items){if(stack.count>0)amounts[stack.id]=amounts.TryGetValue(stack.id,out var current)?current+stack.count:stack.count;}
   amounts["fcore"]=FCores;if(!string.IsNullOrEmpty(consumed)&&amounts.ContainsKey(consumed))amounts[consumed]--;
   foreach(var stack in stacks){if(stack.count<0||GameItems.Find(stack.id)==null)return false;long next=(amounts.TryGetValue(stack.id,out var current)?current:0)+stack.count;if(next>9999)return false;amounts[stack.id]=next;}
   return 1+amounts.Count(t=>t.Key!="fcore"&&t.Value>0)<=InventorySlots;
  }
  static void Add(string id,int amount)
  {
   if(id=="fcore"){Data.fCores+=amount;return;}
   var s=Data.items.Find(t=>t.id==id);if(s==null){s=new ItemStack(id,0);Data.items.Add(s);}s.count+=amount;
  }
  static UnitUpgrade Unit(string id){var u=Data.units.Find(t=>t.id==id);if(u==null){u=new UnitUpgrade{id=id,module=""};Data.units.Add(u);}return u;}
  public static int Rank(string id)=>Unit(id).rank;
  public static int UnitLevel(string id)=>20+Rank(id);
  public static string Module(string id)=>Parts(id).FirstOrDefault()?.itemId??"";
  public static int UpgradeCredits(string id)=>400+Rank(id)*150;
  public static int UpgradeCores(string id)=>2+Rank(id)/3;
  public static bool BuyItem(string id,int quantity,out string message)
  {
   var item=GameItems.Find(id);if(item==null||GameItems.IsRewardOnly(item)||quantity<1||quantity>99){message="Invalid item or quantity.";return false;}
   long cost=(long)item.price*quantity;if(Credits<cost){message="Not enough Credits.";return false;}
   if(!CanReceive(new ItemStack(id,quantity))){message="Storage full. Unlock a slot or use an existing item.";return false;}
   Data.credits-=(int)cost;Add(id,quantity);Data.purchases++;Log("Bought "+quantity+" x "+item.name+" (-"+cost+")");Save();message="Added "+quantity+" x "+item.name+" to Inventory.";return true;
  }
  public static bool UpgradeUnit(string id,out string message)
  {
   if(!OwnsGundam(id)){message="Own this Gundam first.";return false;}
   if(Rank(id)>=20){message="Maximum level 40 reached.";return false;}
   int price=UpgradeCredits(id),cores=UpgradeCores(id);
   if(Credits<price||FCores<cores||Count("xp_chip")<1){message="Requires "+price+" Credits, "+cores+" F-Cores and 1 Combat Data Chip.";return false;}
   Data.credits-=price;Data.fCores-=cores;Add("xp_chip",-1);Unit(id).rank++;Data.upgrades++;Log("Upgraded "+id+" to Lv. "+UnitLevel(id));Save();message=id+" upgraded to Lv. "+UnitLevel(id)+". Battle stats increased.";return true;
  }
  public static bool UseItem(string id,string unitId,out string message)
  {
   var item=GameItems.Find(id);if(item==null||Count(id)<1){message="Item is unavailable.";return false;}
   if(item.effect==ItemEffect.Upgrade)return UpgradeUnit(unitId,out message);
   if(GameItems.IsEquipment(item))return EquipPart(unitId,GundamEquipment.Preferred(id),id,out message);
   if(GameItems.IsPreparation(item))return TogglePreparation(unitId,id,out message);
   var contents=GameItems.Contents(id);
   if(item.effect==ItemEffect.Supply&&(contents.Length==0||!CanReceiveAfterConsuming(id,contents))){message="Free storage slots for all crate contents before opening.";return false;}
   Add(id,-1);if(item.effect==ItemEffect.Credits)Data.credits+=item.amount;else foreach(var stack in contents)Add(stack.id,stack.count);
   Data.uses++;Log("Used "+item.name);Save();message=item.effect==ItemEffect.Credits?"Credits received +"+item.amount.ToString("N0")+".":"Received "+string.Join(", ",contents.Select(t=>t.count+" "+GameItems.Find(t.id).name))+".";return true;
  }
  public static string PreparedItem(string unitId)=>Unit(unitId).booster??"";
  public static bool IsArmed(string itemId)=>Data.units.Any(u=>u.booster==itemId);
  public static bool TogglePreparation(string unitId,string id,out string message)
  {
   if(!OwnsGundam(unitId)||!GameItems.IsPreparation(GameItems.Find(id))){message="Preparation item is unavailable.";return false;}
   if(PreparedItem(unitId)==id){Unit(unitId).booster="";message="Preparation cancelled. No item consumed.";}
   else{int reserved=Data.units.Count(u=>u.id!=unitId&&u.booster==id);if(Count(id)<=reserved){message="No unreserved copy is available for this Gundam.";return false;}Unit(unitId).booster=id;message="Prepared for "+unitId+". One item is consumed on real sortie entry.";}
   Log(message);Save();return true;
  }
  public static void PrepareSortie(string unitId)
  {
   ClearBattlePreparation();string id=PreparedItem(unitId);if(!GameItems.IsPreparation(GameItems.Find(id))||Count(id)<1)return;
   SortieSession.BoosterId=id;SortieSession.BoosterUnitId=unitId;Version++;
  }
  public static bool CommitSortie(string unitId)
  {
   string id=SortieSession.BoosterId;
   if(SortieSession.Practice||SortieSession.BoosterUnitId!=unitId||string.IsNullOrEmpty(id)||PreparedItem(unitId)!=id||Count(id)<1)return false;
   Add(id,-1);Unit(unitId).booster="";Data.uses++;Log("Sortie preparation used: "+GameItems.Find(id).name);Save();return true;
  }
  public static void ClearBattlePreparation(){SortieSession.BoosterId=SortieSession.BoosterUnitId="";Version++;}
  public static bool EquipModule(string unitId,string id,out string message)
  {
   if(!string.IsNullOrEmpty(id))return EquipPart(unitId,GundamEquipment.Preferred(id),id,out message);
   if(!OwnsGundam(unitId)){message="Own this Gundam first.";return false;}Parts(unitId).Clear();Log("Cleared loadout: "+unitId);Save();message="All modules returned to storage.";return true;
  }
  public static bool IsEquipped(string id)=>Data.units.Any(u=>u.module==id||u.parts!=null&&u.parts.Any(p=>p.itemId==id));
  public static bool CanSell(string id)=>GameItems.Find(id)!=null&&GameItems.Find(id).sell>0&&Count(id)>0&&!IsEquipped(id)&&!IsArmed(id);
  public static bool SellItems(string[] ids,bool discard,out string message)
  {
   var distinct=(ids??Array.Empty<string>()).Distinct().ToArray();
   if(distinct.Length==0||distinct.Any(id=>!CanSell(id))){message="Select tradable items. Equipped modules, armed supplies and F-Cores are protected.";return false;}
   int total=distinct.Sum(id=>GameItems.Find(id).sell*Count(id));
   foreach(var id in distinct)Add(id,-Count(id));if(!discard)Data.credits+=total;Log((discard?"Discarded ":"Sold ")+distinct.Length+" stacks"+(!discard?" (+"+total+" Credits)":""));Save();message=discard?"Selected stacks discarded.":"Sold for "+total.ToString("N0")+" Credits.";return true;
  }
  public static bool Claimed(string id)=>Data.claims.Contains(id);
  public static int MissionProgress(int mission)=>mission==0?Data.victories:mission==1?Data.purchases:mission==2?Data.uses:Data.upgrades;
  public static bool Claim(string id,out string message)
  {
   if(Claimed(id)){message="Reward already claimed.";return false;}
   int credits;ItemStack[] items;
   if(id=="parts-delivery"){credits=0;items=new[]{new ItemStack("precision_module",1),new ItemStack("defense_module",1),new ItemStack("bulwark_module",1),new ItemStack("thruster_module",2),new ItemStack("balanced_module",1),new ItemStack("breaker_module",1)};}
   else if(id=="armory-delivery"){credits=0;items=new[]{new ItemStack("core_case",1),new ItemStack("data_archive",1),new ItemStack("repair_gel",2),new ItemStack("overdrive_amp",2)};}
   else if(id=="welcome"){credits=500;items=new[]{new ItemStack("supply_crate",1)};}
   else if(id.StartsWith("mission-")&&int.TryParse(id.Substring(8),out int m)&&m>=0&&m<4&&MissionProgress(m)>0){credits=500+m*100;items=new[]{new ItemStack(m==3?"supply_crate":"xp_chip",1)};}
   else{message="Complete this mission first.";return false;}
   if(!CanReceive(items)){message="Inventory full. Free or unlock a slot before claiming.";return false;}
   Data.credits+=credits;foreach(var item in items)Add(item.id,item.count);Data.claims.Add(id);Log("Claimed "+id+" (+"+credits+" Credits)");Save();message="Rewards received: "+credits+" Credits and "+items.Length+" supply stack(s).";return true;
  }
  public static int Scaled(string id,int value,string stat)=>LoadoutStat(id,value,stat);

 }
 public static class SortieSession
 {
  public static int Mode; public static bool Advanced;public static string BoosterId="",BoosterUnitId="";
  public static bool Practice=>Mode==3||Mode==4;
  public static float EnemyScale=>(Mode==2?1.25f:1)*(Advanced?1.15f:1);
  public static int Reward(int baseReward)=>Practice?0:Mathf.RoundToInt(baseReward*(Mode==2?1.5f:1));
 }
}
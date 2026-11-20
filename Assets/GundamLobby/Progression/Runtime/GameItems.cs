using System;
using UnityEngine;
namespace GundamLobby
{
 public enum ItemEffect {Upgrade=0,Credits=2,Supply=3,Attack=4,Armor=5,Speed=6,Break=7,Defense=8,Precision=9,Balanced=10,PrepHP=11,PrepAttack=12}
 [Serializable] public sealed class GameItem
 {
  public string id,name,category,rarity,description;public int price,sell,amount,tier;public ItemEffect effect;public Color color;
  public GameItem(string id,string name,string category,int tier,int price,int sell,int amount,ItemEffect effect,string description,Color color)
  {this.id=id;this.name=name;this.category=category;this.tier=tier;this.price=price;this.sell=sell;this.amount=amount;this.effect=effect;this.description=description;this.color=color;rarity=new[]{"RARE","EPIC","LEGENDARY"}[tier];}
  public string Action=>GameItems.IsEquipment(this)?"CONFIGURE BODY PART":GameItems.IsPreparation(this)?"PREPARE FOR SORTIE":effect==ItemEffect.Upgrade?"UPGRADE GUNDAM":effect==ItemEffect.Supply?"OPEN SUPPLY":"OPEN CREDIT CACHE";
 }
 public static class GameItems
 {
  public static readonly GameItem[] All={
   new("fcore","F-CORE","MODS",1,500,0,0,ItemEffect.Upgrade,"Calibration core. Combine with a Data Chip and Credits in Laboratory > Update to permanently improve Gundam stats.",new(.40f,.84f,1)),
   new("xp_chip","COMBAT DATA CHIP","DEVICES",0,300,90,1,ItemEffect.Upgrade,"Tactical training data. Each upgrade consumes 1 chip, F-Cores and Credits. Improves combat stats. Maximum level: 40.",new(.30f,.77f,1)),
   new("credit_cache","CREDIT CACHE","SUPPLIES",1,750,225,750,ItemEffect.Credits,"Open this reward pack to receive 750 Credits. Credits buy Gundam, modules, supplies, upgrades and storage.",new(1,.77f,.28f)),
   new("supply_crate","MAINTENANCE CRATE","SUPPLIES",1,1200,300,1,ItemEffect.Supply,"Guaranteed contents: 3 Data Chips, 1 Credit Cache and 2 F-Cores. Contents are stored in Inventory.",new(.77f,.48f,1)),
   new("assault_module","ASSAULT CIRCUIT","WEAPON",1,2400,600,15,ItemEffect.Attack,"Reusable module: +15% ATK in every battle. Install in a compatible body-part slot. Equip or unequip without consuming it.",new(1,.43f,.34f)),
   new("bulwark_module","AEGIS PLATING","MODS",1,2200,550,15,ItemEffect.Armor,"Reusable module: +15% HP and +20% armor. Fits arms and legs; each part has its own slot.",new(.42f,.68f,1)),
   new("thruster_module","VECTOR THRUSTER","DEVICES",1,2000,500,10,ItemEffect.Speed,"Reusable module: +10 Speed, improving turn order. Fits legs or backpack.",new(.36f,1,.88f)),
   new("core_case","F-CORE CONTAINER","SUPPLIES",1,2250,675,5,ItemEffect.Supply,"Sealed calibration cassette. Open to receive 5 F-Cores for permanent Gundam upgrades. Costs less than five individual cores.",new(.33f,.83f,1)),
   new("data_archive","TACTICAL DATA ARCHIVE","SUPPLIES",1,2600,780,10,ItemEffect.Supply,"Military data rack. Open to receive 10 Combat Data Chips. Combine chips with F-Cores and Credits in Update.",new(.38f,.67f,1)),
   new("credit_vault","TREASURY VAULT","SUPPLIES",2,2500,750,2500,ItemEffect.Credits,"Rare reward-only reserve. Open for 2,500 Credits. Found inside Frontline Supply Caches; cannot be bought directly.",new(1,.80f,.36f)),
   new("frontline_crate","FRONTLINE SUPPLY CACHE","SUPPLIES",2,4800,1200,1,ItemEffect.Supply,"Guaranteed contents: 4 F-Cores, 4 Data Chips and 1 Treasury Vault. The vault opens separately for 2,500 Credits.",new(1,.64f,.25f)),
   new("breaker_module","BREACH COUPLER","MODS",1,2600,650,30,ItemEffect.Break,"Reusable module: +30% Break Damage. Helps strip enemy armor. Fits the right arm; does not change direct ATK.",new(1,.60f,.23f)),
   new("defense_module","GUARD MATRIX","MODS",1,2500,625,20,ItemEffect.Defense,"Reusable module: +20% DEF. Improves damage mitigation and parry effectiveness. Install in a compatible body-part slot.",new(.30f,.67f,.96f)),
   new("precision_module","PRECISION ARRAY","WEAPON",2,3000,750,20,ItemEffect.Precision,"Reusable module: +20% ATK with a -5 Speed trade-off. Suits heavy damage builds. Fits the head or right arm.",new(1,.79f,.30f)),
   new("balanced_module","SERAPH CONTROLLER","DEVICES",2,3200,800,8,ItemEffect.Balanced,"Reusable module: +8% HP, ATK and DEF, plus +4 Speed. A balanced alternative to specialist equipment.",new(.50f,.92f,1)),
   new("repair_gel","NANO REPAIR GEL","SUPPLIES",0,550,165,15,ItemEffect.PrepHP,"Prepare +15% maximum HP for the next sortie. One preparation per Gundam. Consumed on Campaign, Skirmish or Challenge entry; Training previews it free.",new(.30f,.91f,.57f)),
   new("overdrive_amp","OVERDRIVE AMPOULE","SUPPLIES",0,650,195,12,ItemEffect.PrepAttack,"Prepare +12% ATK for the next sortie. One preparation per Gundam. Consumed on real sortie entry; can be cancelled before deployment.",new(1,.38f,.25f))
  };
  public static GameItem Find(string id)=>Array.Find(All,t=>t.id==id);
  public static bool IsEquipment(GameItem i)=>i!=null&&(i.effect==ItemEffect.Attack||i.effect==ItemEffect.Armor||i.effect==ItemEffect.Speed||i.effect==ItemEffect.Break||i.effect==ItemEffect.Defense||i.effect==ItemEffect.Precision||i.effect==ItemEffect.Balanced);
  public static bool IsPreparation(GameItem i)=>i!=null&&(i.effect==ItemEffect.PrepHP||i.effect==ItemEffect.PrepAttack);
  public static bool IsSupply(GameItem i)=>i!=null&&(i.effect==ItemEffect.Supply||i.effect==ItemEffect.Credits||IsPreparation(i));
  public static bool IsRewardOnly(GameItem i)=>i!=null&&i.effect==ItemEffect.Credits;
  public static PlayerProgress.ItemStack[] Contents(string id)=>id=="supply_crate"?new PlayerProgress.ItemStack[]{new PlayerProgress.ItemStack("xp_chip",3),new("credit_cache",1),new("fcore",2)}:id=="core_case"?new PlayerProgress.ItemStack[]{new PlayerProgress.ItemStack("fcore",5)}:id=="data_archive"?new PlayerProgress.ItemStack[]{new PlayerProgress.ItemStack("xp_chip",10)}:id=="frontline_crate"?new PlayerProgress.ItemStack[]{new PlayerProgress.ItemStack("fcore",4),new("xp_chip",4),new("credit_vault",1)}:Array.Empty<PlayerProgress.ItemStack>();
  public static string Summary(GameItem i)
  {
   if(i==null)return "";return i.effect switch{ItemEffect.Upgrade=>"GUNDAM CALIBRATION",ItemEffect.Credits=>"OPEN: "+i.amount.ToString("N0")+" CREDITS",ItemEffect.Attack=>"+15% ATK",ItemEffect.Armor=>"+15% HP / +20% ARMOR",ItemEffect.Speed=>"+10 SPEED",ItemEffect.Break=>"+30% BREAK DMG",ItemEffect.Defense=>"+20% DEF",ItemEffect.Precision=>"+20% ATK / -5 SPD",ItemEffect.Balanced=>"BALANCED STATS / +4 SPD",ItemEffect.PrepHP=>"NEXT SORTIE: +15% HP",ItemEffect.PrepAttack=>"NEXT SORTIE: +12% ATK",_=>i.id=="core_case"?"OPEN: 5 F-CORES":i.id=="data_archive"?"OPEN: 10 DATA CHIPS":i.id=="frontline_crate"?"CORES / DATA / VAULT":"DATA / CORES / CREDITS"};
  }
 }
}
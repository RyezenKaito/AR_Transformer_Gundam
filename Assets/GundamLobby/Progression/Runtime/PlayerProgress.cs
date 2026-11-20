using System;
using System.Collections.Generic;
using UnityEngine;
namespace GundamLobby
{
 public static partial class PlayerProgress
 {
  public const string SaveKey="Gundam.Progression.v1";
  [Serializable] public sealed class SaveData
  {
   public int credits,crystals,fCores,inventorySlots,unicornSkin,rougeSkin;
   public int schema,purchases,uses,upgrades,victories;
   public string commanderName;
   public List<ItemStack> items=new List<ItemStack>(); public List<UnitUpgrade> units=new List<UnitUpgrade>();
   public List<string> claims=new List<string>(),history=new List<string>();
   public List<string> gundams=new List<string>(),skins=new List<string>(),rewardClaims=new List<string>();
  }
  static SaveData data;static EconomyRules rules;
  public static int Version {get;private set;}
  public static EconomyRules Rules {get {if(!rules)rules=Resources.Load<EconomyRules>("GundamEconomyRules");if(!rules)rules=ScriptableObject.CreateInstance<EconomyRules>();return rules;}}
  static SaveData Data
  {
   get
   {
    if(data!=null)return data;
    string json=PlayerPrefs.GetString(SaveKey,"");
    try{if(json.Length>0)data=JsonUtility.FromJson<SaveData>(json);}catch(ArgumentException){data=null;}
    if(data==null)
    {
     data=new SaveData{credits=Rules.startingCredits,crystals=Rules.startingCrystals,inventorySlots=Rules.initialInventorySlots};
     data.gundams.Add("Unicorn");
     if(PlayerPrefs.GetString(StrikerSelectionController.PreferenceKey,"")=="StrikeRouge")data.gundams.Add("StrikeRouge");
    }
    data.gundams??=new List<string>();data.skins??=new List<string>();data.rewardClaims??=new List<string>();
    data.credits=Mathf.Max(0,data.credits);data.crystals=Mathf.Max(0,data.crystals);data.fCores=Mathf.Max(0,data.fCores);
    data.inventorySlots=Mathf.Clamp(data.inventorySlots,Rules.initialInventorySlots,Rules.maximumInventorySlots);
    if(!data.gundams.Contains("Unicorn"))data.gundams.Add("Unicorn");InitializeItems(data);return data;
   }
  }
  public static int Credits=>Data.credits;
  public static int Crystals=>Data.crystals;
  public static int FCores=>Data.fCores;
  public static int InventorySlots=>Data.inventorySlots;
  public static int NextSlotPrice=>Rules.slotBasePrice+(InventorySlots-Rules.initialInventorySlots)*Rules.slotPriceIncrement;
  public static bool OwnsGundam(string id)=>Data.gundams.Contains(id);
  public static bool OwnsSkin(string id,int skin)=>skin==0||Data.skins.Contains(id+":"+skin);
  public static int EquippedSkin(string id)=>id=="Unicorn"?Data.unicornSkin:Data.rougeSkin;
  public static void Reload(){data=null;Version++;}
  static void Save(){PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(Data));PlayerPrefs.Save();Version++;}
  public static bool BuyGundam(string id,out string message)
  {
   if(id!="Unicorn"&&id!="StrikeRouge"){message="Gundam is unavailable.";return false;}
   if(OwnsGundam(id)){message="Gundam already owned.";return false;}
   int price=Rules.GundamPrice(id);if(Credits<price){message="Not enough Credits.";return false;}
   Data.credits-=price;Data.gundams.Add(id);Data.purchases++;Log("Purchased "+id);Save();message="Gundam unlocked.";return true;
  }
  public static bool BuySkin(string id,int skin,out string message)
  {
   if(skin<0||skin>2||!OwnsGundam(id)){message="Own this Gundam before unlocking its skin.";return false;}
   if(OwnsSkin(id,skin)){message="Skin already owned.";return false;}
   int price=Rules.SkinPrice(skin);if(Crystals<price){message="Not enough Crystals.";return false;}
   Data.crystals-=price;Data.skins.Add(id+":"+skin);Data.purchases++;Log("Unlocked skin for "+id);Save();message="Skin unlocked.";return true;
  }
  public static bool EquipSkin(string id,int skin)
  {
   if(!OwnsSkin(id,skin)||skin<0||skin>2)return false;
   if(id=="Unicorn")Data.unicornSkin=skin;else if(id=="StrikeRouge")Data.rougeSkin=skin;else return false;
   Save();return true;
  }
  public static bool ExpandInventory(out string message)
  {
   if(InventorySlots>=Rules.maximumInventorySlots){message="All inventory slots are unlocked.";return false;}
   int price=NextSlotPrice;if(Credits<price){message="Not enough Credits to unlock a slot.";return false;}
   Data.credits-=price;Data.inventorySlots++;Save();message="Inventory slot "+InventorySlots+" unlocked.";return true;
  }
  public static bool AwardVictory(int credits,int cores,string claim)
  {
   if(string.IsNullOrEmpty(claim)||Data.rewardClaims.Contains(claim))return false;
   Data.credits+=Mathf.Clamp(credits,0,1000000);Data.fCores+=Mathf.Clamp(cores,0,10000);Data.rewardClaims.Add(claim);Data.victories++;Log("Victory: +"+credits+" Credits, +"+cores+" F-Core");
   if(Data.rewardClaims.Count>100)Data.rewardClaims.RemoveAt(0);Save();return true;
  }
 }
}

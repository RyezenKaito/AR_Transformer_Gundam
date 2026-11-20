using UnityEngine;
namespace GundamLobby
{
    [CreateAssetMenu(menuName="Gundam/Economy Rules")]
    public sealed class EconomyRules : ScriptableObject
    {
        public int startingCredits=17020,startingCrystals=925;
        public int initialInventorySlots=10,maximumInventorySlots=40;
        public int slotBasePrice=1000,slotPriceIncrement=250;
        public int strikeRougePrice=10000;
        public int midnightSkinPrice=300,crimsonSkinPrice=450;
        public int GundamPrice(string id)=>id=="Unicorn"?0:strikeRougePrice;
        public int SkinPrice(int index)=>index==0?0:index==1?midnightSkinPrice:crimsonSkinPrice;
    }
}

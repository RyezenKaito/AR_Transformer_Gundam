using System;
using UnityEngine;

namespace GundamLobby
{
    [Serializable]
    public class ShopItemDefinition
    {
        public string title,description,code;
        public int category,rarity=2,price=1600,unitPrice=1600,quantity=1,bids=1,remainingMinutes=1327;
        public bool auction=true,isMyListing,isMyBid,frameArtwork;
        public Sprite icon;
    }
    [CreateAssetMenu(menuName="Gundam Lobby/Shop Catalog")]
    public class ShopCatalog:ScriptableObject
    {
        public string[] categories;
        [Tooltip("Prototype listings only. No payment, wallet or auction backend.")]
        public ShopItemDefinition[] items;
    }
}

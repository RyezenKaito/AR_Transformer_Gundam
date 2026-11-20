using System;
using UnityEngine;

namespace GundamLobby
{
    [Serializable]
    public class StrikerDefinition
    {
        public string id, displayName, code, role;
        public string gradeBadge;
        public int armor, breakDamage;
        public string[] skillNames = new string[4];
        public int[] burstCosts = {0,1,2,5};
        public GameObject prefab;
        public Sprite portrait;
        public Sprite cardPortrait;
        public Sprite CardPortrait => cardPortrait?cardPortrait:portrait;
        public int level = 20, hp = 18240, attack = 2480, defense = 1220, speed = 112, power = 18460;
        [Range(0, 100)] public float critRate = 12, critDamage = 60, breakEffect = 24, effectResistance = 8;
        public string damageType = "BEAM";
        public int shieldCapacity = 2600, maxEnergy = 120;
    }

    [CreateAssetMenu(menuName = "Gundam Lobby/Striker Catalog")]
    public class StrikerCatalog : ScriptableObject
    {
        [Tooltip("Presentation data only. Replace sample balance values before combat implementation.")]
        public StrikerDefinition[] units;
    }
}

using System;
using UnityEngine;

namespace GundamLobby
{
    [Serializable]
    public class BattleModeDefinition
    {
        public string title, subtitle, description, code;
        public int recommendedLevel=20, energyCost=20;
        public BattleModeIcon.Kind icon;
    }
    [CreateAssetMenu(menuName="Gundam Lobby/Battle Mode Catalog")]
    public class BattleModeCatalog : ScriptableObject
    {
        [Tooltip("Presentation data only; combat, matchmaking and networking are not implemented.")]
        public BattleModeDefinition[] modes;
        public string[] locationNames;
        public Vector3[] locationPoints;
    }
}

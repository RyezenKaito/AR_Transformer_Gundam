using System;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    public static class PlayerSelectionData
    {
        public const string PreferenceKey = "GundamLobby.ActiveStriker";
        public const string DefaultId = "Unicorn";
        public const string RougeId = "StrikeRouge";

        public static bool Supports(string id) => id == DefaultId || id == RougeId;
        public static string EquippedId
        {
            get
            {
                string id = PlayerPrefs.GetString(PreferenceKey, DefaultId);
                return Supports(id) ? id : DefaultId;
            }
        }

        public static string ResourcePath(string id) => $"CombatProfiles/{(Supports(id) ? id : DefaultId)}/Profile";
        public static CharacterCombatProfile Load(string id) => Resources.Load<CharacterCombatProfile>(ResourcePath(id));

        public static void Equip(string id)
        {
            if (!Supports(id)) throw new ArgumentException("This Gundam has no playable combat profile.", nameof(id));
            PlayerPrefs.SetString(PreferenceKey, id);
            PlayerPrefs.Save();
        }
    }
}

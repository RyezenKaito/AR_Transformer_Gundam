using System;
using UnityEngine;

namespace GundamLobby
{
    [Serializable]
    public class WeaponrySkill
    {
        public string category, title, description, badge;
        public WeaponryGlyph.Kind glyph;
        public string[] statNames, statValues;
    }

    [CreateAssetMenu(menuName = "Gundam Lobby/Weaponry Skill Catalog")]
    public class WeaponryCatalog : ScriptableObject
    {
        [Tooltip("Prototype presentation values. No combat or balance implementation.")]
        public WeaponrySkill[] skills;
    }
}

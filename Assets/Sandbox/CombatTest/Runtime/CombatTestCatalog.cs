using System;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    [CreateAssetMenu(menuName="Gundam/Combat/Test Catalog")]
    public sealed class CombatTestCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string label;
            [Tooltip("Path to the CharacterCombatProfile relative to a Resources folder, without extension.")]
            public string resourcePath;
        }

        public Entry[] characters = Array.Empty<Entry>();
    }
}

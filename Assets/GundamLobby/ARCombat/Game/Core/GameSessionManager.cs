using GundamGame.CombatDemo;
using UnityEngine;

namespace GundamGame
{
    public static class GameSessionManager
    {
        public static CharacterCombatProfile EquippedGundam =>
            PlayerSelectionData.Load(PlayerSelectionData.EquippedId);

        public static ResourceRequest LoadEquippedGundamAsync() =>
            Resources.LoadAsync<CharacterCombatProfile>(PlayerSelectionData.ResourcePath(PlayerSelectionData.EquippedId));
    }
}

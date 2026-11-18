using UnityEngine;

namespace GundamGame.Unicorn
{
    [CreateAssetMenu(menuName="Gundam/Unicorn/Psycho-Frame Buff")]
    public sealed class UnicornBuffSettings : ScriptableObject
    {
        [Min(0)] public float attackBonus = .15f;
        [Min(0)] public float defenseBonus = .10f;
        [Min(1)] public int followingTurns = 1;
    }
}

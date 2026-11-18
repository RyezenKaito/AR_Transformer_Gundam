using System;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    [CreateAssetMenu(menuName="Gundam/Combat/Character Profile")]
    public sealed class CharacterCombatProfile : ScriptableObject
    {
        public const int MaximumBurst = 5;
        public const int StartingBurst = 2;
        public const int TurnEndBurst = 1;

        [Serializable]
        public struct Skill
        {
            public string displayName;
            [Range(0,5)] public int burstCost;
            public AnimationClip animation;
            [Min(0)] public float duration;
            [Min(0)] public float damageMultiplier, armorMultiplier;
        }

        public GameObject combatPrefab;
        public string displayName;
        public string gradeBadge;
        [Min(1)] public int maxHealth;
        [Min(0)] public int maxArmor, attack, defense, breakDamage;
        [Min(1)] public int speed;
        public Skill[] skills = new Skill[4];

        public void ApplyStats(DemoFighter fighter)
        {
            fighter.displayName=displayName;
            fighter.maxHealth=maxHealth;
            fighter.maxArmor=maxArmor;
            fighter.attack=attack;
            fighter.defense=defense;
            fighter.speed=speed;
            fighter.breakDamage=breakDamage;
        }
    }
}

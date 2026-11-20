using System;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    [CreateAssetMenu(menuName="Gundam/Combat/Enemy Profile")]
    public sealed class EnemyProfile : ScriptableObject
    {
        public enum Role { Ranged, Melee }
        public enum Cue { Warn=1, Equip, BladeOn, Fire, Hit, Guard, Recover, BladeOff }
        [Serializable]
        public struct Move
        {
            public string label;
            public int hits, damage, cooldownTurns;
            public float range, telegraph, recovery, armorMultiplier;
            public AnimationClip timeline;
            public AnimationCurve sourceTime;
        }
        public Role role;
        public GameObject prefab;
        public Sprite portrait;
        public AnimationClip clip;
        public Move[] moves;
        public int hp=180, armor=60, attack=85, defense=65, speed=135, breakDamage=40;
        public float moveSpeed=5;
    }
}

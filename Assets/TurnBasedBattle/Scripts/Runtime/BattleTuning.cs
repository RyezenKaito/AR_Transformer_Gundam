using System;
using UnityEngine;
namespace TurnBasedBattle
{
    [CreateAssetMenu(menuName = "Turn Battle/Tuning")]
    public sealed class BattleTuning : ScriptableObject
    {
        public float playerHealth = 1000f, enemyHealth = 600f;
        public int startingUltimate;
        public int dodgeGain = 15, damagedGain = 10;
        [Range(0, 1)] public float guardMultiplier = .5f;
        public bool enemyCombos;
        [Range(0, .2f)] public float extraDodgeWindow;
        public BattleActionDefinition attack, skill, guard, ultimate, enemyStrike, enemyCombo;

        public BattleActionDefinition Get(BattleActionKind kind)
        {
            switch(kind)
            {
                case BattleActionKind.Attack: return attack;
                case BattleActionKind.Skill: return skill;
                case BattleActionKind.Guard: return guard;
                case BattleActionKind.EarthPrison: return ultimate;
                case BattleActionKind.EnemyCombo: return enemyCombo;
                default: return enemyStrike;
            }
        }
    }

}

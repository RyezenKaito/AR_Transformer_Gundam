using System;
using UnityEngine;

namespace TurnBasedBattle
{
    public enum BattlePhase { PlayerCommand, PlayerTargeting, PlayerAction, EnemyAction, Victory, Defeat }
    public enum BattleActionKind { Attack, Skill, Guard, EarthPrison, EnemyStrike, EnemyCombo }
    public enum DodgeOutcome { Pending, Dodged, Failed }
    public enum BattleWeaponKind { None, Saber, Cannon }
    public enum BattleTravelStyle { Stationary, Run, Fly }

    [Serializable]
    public sealed class BattleHit
    {
        public float time = 1.2f;
        public float damage = 80f;
        public float cue = .3f;
        public float open = .95f;
        public float close = 1.15f;
    }

    /// <summary>Pure per-hit adjudication. The first edge during the cue consumes the attempt.</summary>
    public sealed class DodgeAttempt
    {
        public DodgeOutcome outcome;
        public bool pressed, resolved;
        public double inputTime = -1;
        public bool Submit(double timestamp, BattleHit hit, double extra = 0)
        {
            if(resolved || pressed || timestamp < hit.cue - 1e-6 || timestamp > hit.time + 1e-6) return false;
            pressed = true; inputTime = timestamp;
            outcome = timestamp >= hit.open - extra - 1e-6 && timestamp <= hit.close + extra + 1e-6
                ? DodgeOutcome.Dodged : DodgeOutcome.Failed;
            return true;
        }
        public DodgeOutcome Resolve()
        {
            resolved = true;
            if(outcome == DodgeOutcome.Pending) outcome = DodgeOutcome.Failed;
            return outcome;
        }
    }

    [Serializable]
    public struct BattleHitRecord
    {
        public int battleId, actionId, hitIndex, targetId;
        public float actionTime, damage, hpAfter;
        public bool dodged;
    }
}

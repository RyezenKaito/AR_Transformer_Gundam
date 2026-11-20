using System;
using System.Linq;
using GundamGame.Unicorn;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    internal static class EnemyAttackPresentation
    {
        static readonly Color Rose = new(1f, .29f, .42f);
        static readonly Color Amber = new(1f, .69f, .24f);

        public static void Play(EnemyProfile.Cue cue, EnemyBrain brain, DemoFighter target, UnicornCombatEffects effects,
            Func<float> clock, int hits, bool dodged)
        {
            switch (cue)
            {
                case EnemyProfile.Cue.Warn:
                    float warningTime = Mathf.Max(.18f, brain.FirstHitTime - clock());
                    if (brain.IsMelee)
                        effects.EnemyLane(brain.transform.position, brain.GroundAim, brain.Slot == 2, warningTime);
                    else if (brain.Move.hits > 0)
                        effects.EnemyWarning(() => brain.Origin, brain.IsBlast ? brain.GroundAim : brain.Aim,
                            brain.IsBlast ? EnemyBrain.BlastRadius : 0, warningTime);
                    break;
                case EnemyProfile.Cue.BladeOn:
                    effects.Impact(brain.BladeBase, .20f, Rose, false);
                    break;
                case EnemyProfile.Cue.Fire:
                    var aim = dodged ? target.AimPoint + target.visual.right * .88f : brain.Aim;
                    if (brain.IsBlast)
                    {
                        float firedAt = brain.Move.timeline.events.First(e => e.intParameter == (int)EnemyProfile.Cue.Fire).time;
                        effects.EnemyShell(brain.Origin, brain.GroundAim, brain.FirstHitTime - firedAt, () => clock() - firedAt);
                    }
                    else if (brain.IsRanged) effects.Beam(brain.Origin, aim, true);
                    else effects.Tracer(brain.VulcanOrigin(hits), aim);
                    break;
                case EnemyProfile.Cue.Hit:
                    if (brain.IsBlast)
                    {
                        effects.Impact(brain.GroundAim, 1.05f, Amber, true);
                        effects.Ring(brain.GroundAim, .24f, 1.8f, Amber, .42f);
                    }
                    else if (brain.IsMelee)
                    {
                        if (brain.Slot == 2) effects.EnemyThrust(brain.BladeBase, brain.BladeTip);
                        else effects.EnemySlash(brain.BladeBase, brain.BladeTip, brain.BladeWindupDirection);
                    }
                    break;
                case EnemyProfile.Cue.Guard:
                    if (brain.shield) effects.Shield(brain.shield.position, brain.transform.forward);
                    break;
            }
        }

        public static bool Contacts(EnemyBrain brain, DemoFighter target)
        {
            if (brain.IsBlast)
                return Vector3.ProjectOnPlane(target.AimPoint - brain.Aim, Vector3.up).sqrMagnitude <=
                    EnemyBrain.BlastRadius * EnemyBrain.BlastRadius;
            return !brain.IsMelee || brain.BladeContacts(target.AimPoint, .64f);
        }
    }
}

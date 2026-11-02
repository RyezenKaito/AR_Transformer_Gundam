using System;
using UnityEngine;

namespace MechaDemo
{
    [Serializable]
    public sealed class MechaActionConfig
    {
        public string id;
        public float duration;
        public float activeStart;
        public float activeEnd;
        public float damage;
        public float advance;
        public float queueOpen;
        public float queueClose;
        public float linkTime;
        public float dashCancel;
        public float cost;
        public float cooldown;
        public float hitStop;
        public MechaActionConfig(string name, float length, float start, float end, float power, float travel, float queueStart, float queueEnd, float link, float cancel, float energy, float cd, float stop)
        { id = name; duration = length; activeStart = start; activeEnd = end; damage = power; advance = travel; queueOpen = queueStart; queueClose = queueEnd; linkTime = link; dashCancel = cancel; cost = energy; cooldown = cd; hitStop = stop; }
    }
    [CreateAssetMenu(menuName = "Mecha Demo/Combat Tuning")]
    public sealed class MechaTuning : ScriptableObject
    {
        [Header("Motor (metres / seconds)")]
        public float walkSpeed = 3f;
        public float runSpeed = 6f;
        public float boostSpeed = 12f;
        public float acceleration = 20f;
        public float braking = 25f;
        public float airAcceleration = 10f;
        public float gravity = -22f;
        public float jumpHeight = 2f;
        public float turnSpeed = 540f;
        public float slopeLimit = 45f;
        [Header("Energy")]
        public float maxEnergy = 100f;
        public float regeneration = 18f;
        public float regenerationDelay = .8f;
        public float boostDrain = 14f;
        public float hoverDrain = 18f;
        [Header("Weapon")]
        public float gunDamage = 45f;
        public float fireInterval = .18f;
        public float gunRange = 60f;
        public float beamRange = 35f;
        public float swordRadius = .13f;
        public float targetRange = 22f;
        public float inputBuffer = .15f;
        [Header("Action timeline: authored clips use these timings")]
        public MechaActionConfig attack1 = new MechaActionConfig("Attack1", .55f, .12f, .23f, 100f, .45f, .18f, .40f, .32f, .25f, 0f, 0f, .035f);
        public MechaActionConfig attack2 = new MechaActionConfig("Attack2", .65f, .16f, .30f, 110f, .35f, .24f, .48f, .39f, .32f, 0f, 0f, .045f);
        public MechaActionConfig attack3 = new MechaActionConfig("Attack3", .90f, .28f, .43f, 180f, .65f, 0f, 0f, 0f, .58f, 0f, 0f, .065f);
        public MechaActionConfig dash = new MechaActionConfig("Dash", .22f, 0f, 0f, 0f, 4.6f, 0f, 0f, 0f, .22f, 12f, .65f, 0f);
        public MechaActionConfig q = new MechaActionConfig("Q", .95f, .38f, .52f, 240f, 8f, 0f, 0f, 0f, .70f, 20f, 6f, .07f);
        public MechaActionConfig e = new MechaActionConfig("E", 1.25f, .65f, .66f, 360f, 0f, 0f, 0f, 0f, .95f, 30f, 9f, .06f);
        public MechaActionConfig r = new MechaActionConfig("R", 4f, 1.96f, 2.10f, 800f, 10f, 0f, 0f, 0f, 4f, 80f, 25f, .08f);
        public MechaActionConfig Get(string action)
        {
            switch (action)
            {
                case "Attack1": return attack1;
                case "Attack2": return attack2;
                case "Attack3": return attack3;
                case "Dash": return dash;
                case "Q": return q;
                case "E": return e;
                case "R": return r;
                default: return null;
            }
        }
    }
}

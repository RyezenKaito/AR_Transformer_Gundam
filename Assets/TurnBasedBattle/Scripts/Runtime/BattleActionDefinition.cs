using System;
using UnityEngine;
namespace TurnBasedBattle
{
    [CreateAssetMenu(menuName = "Turn Battle/Action")]
    public sealed class BattleActionDefinition : ScriptableObject
    {
        public BattleActionKind kind;
        public string displayName;
        public string clipName;
        public float duration = 2f;
        public float releaseTime = .7f;
        public int ultimateCost;
        public int ultimateGain;
        public int cooldownTurns;
        public bool allEnemies;
        [Header("Approach only while this action is executing")]
        public bool approachTarget;
        public BattleTravelStyle travelStyle=BattleTravelStyle.Fly;
        [Range(1.0f,2.2f)] public float runStride=2.1f;
        [Header("Airborne casting: clearance above the board in actor heights")]
        public float castHeightRatio=1.15f;
        public bool airborneCast;
        public float travelStart=.18f,arriveTime=.75f,returnStart=1.45f,returnEnd=2.3f;
        public float stopDistance=1.65f,travelLift=.14f;
        public float vfxStart;
        public BattleWeaponKind weapon;
        public float gripTime=.42f,drawEnd=.66f,bladeOn=.72f,bladeOff=2.05f,sheatheStart=3.05f,holsterTime=3.48f;
        public bool melee;
        public float HitTime(int index)=>hits[index].time+(kind==BattleActionKind.EarthPrison?vfxStart:0);
        public BattleHit[] hits = Array.Empty<BattleHit>();
    }

}

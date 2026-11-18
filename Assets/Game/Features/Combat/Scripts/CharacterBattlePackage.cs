using System;
using GundamGame.Unicorn;
using UnityEngine;

namespace GundamGame.CombatDemo
{
    public sealed class CharacterBattlePackage : MonoBehaviour
    {
        public DemoFighter player;
        public Camera battleCamera;
        public UnicornCombatDemo unicorn;
        public RougeBattleDemo rouge;

        public bool Cinematic => unicorn ? unicorn.Cinematic : rouge.presentation && rouge.presentation.CinematicActive;
        public bool Victory => unicorn ? unicorn.State==UnicornCombatDemo.Phase.Victory : rouge.Phase==RougeBattleDemo.BattlePhase.Victory;
        public bool Defeat => unicorn ? unicorn.State==UnicornCombatDemo.Phase.Defeat : rouge.Phase==RougeBattleDemo.BattlePhase.Defeat;
        public DemoFighter[] Opponents => unicorn ? unicorn.enemies : rouge.enemies;

        public void SetPaused(bool paused)
        {
            if (unicorn) unicorn.SetPaused(paused);
            else rouge.SetGameplayEnabled(!paused);
        }

        public void ConfigureStory(Vector3 focus, Vector2 pitchYaw, float distance)
        {
            battleCamera.clearFlags = CameraClearFlags.Skybox;
            battleCamera.farClipPlane = 160;
            if (unicorn)
            {
                unicorn.IsStoryBattle = true;
                unicorn.ConfigureArenaCamera(focus, pitchYaw, distance);
            }
            else
            {
                rouge.IsStoryBattle = true;
                var orbit = rouge.presentation.arenaCamera;
                orbit.arenaRoot = transform;
                orbit.pivot = transform.InverseTransformPoint(focus);
                orbit.minimumDistance = 7;
                orbit.maximumDistance = 17;
                Vector3 position = focus + Quaternion.Euler(pitchYaw.x,pitchYaw.y,0) * Vector3.back * distance;
                battleCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(focus-position));
            }
        }

        public void Configure(CharacterCombatProfile profile,DemoFighter[] opponents)
        {
            if(!player || !battleCamera || (unicorn!=null)==(rouge!=null))
                throw new InvalidOperationException("The test package needs one combat controller, a player and a camera.");
            profile.ApplyStats(player);
            if(unicorn)
            {
                unicorn.profile=profile;
                unicorn.enemies=opponents;
            }
            else
            {
                rouge.profile=profile;
                rouge.enemies=opponents;
                rouge.arenaRoot=transform;
            }
        }
    }
}

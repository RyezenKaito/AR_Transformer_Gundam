using System.Collections.Generic;
using GundamGame.CombatDemo;
using UnityEngine;

namespace GundamLobby
{
    public static class GundamStatPresentation
    {
        public static IReadOnlyList<(string name, int rows)> Sections { get; } =
            new[] { ("CORE STATS", 6), ("SKILL", 4), ("TURN ECONOMY", 4) };

        public static (string name, string radarName, int value, float score)[] CoreStats(CharacterCombatProfile profile) => new[]
        {
            ("Max HP", "HP", profile.maxHealth, Mathf.InverseLerp(300, 750, profile.maxHealth)),
            ("Attack", "Attack", profile.attack, Mathf.InverseLerp(50, 130, profile.attack)),
            ("Defense", "Defense", profile.defense, Mathf.InverseLerp(50, 110, profile.defense)),
            ("Speed", "Speed", profile.speed, Mathf.InverseLerp(120, 160, profile.speed)),
            ("Break Dmg", "Break Dmg", profile.breakDamage, Mathf.InverseLerp(20, 80, profile.breakDamage)),
            ("Armor", "Armor", profile.maxArmor, Mathf.InverseLerp(60, 340, profile.maxArmor))
        };

        public static string Grade(float score)
        {
            if (score >= .90f) return "S";
            if (score >= .75f) return "A";
            if (score >= .60f) return "B";
            return score >= .40f ? "C" : "D";
        }

        public static (string label, string value)[] Specification(CharacterCombatProfile profile)
        {
            var rows = new List<(string label, string value)>(14);
            foreach (var stat in CoreStats(profile)) rows.Add((stat.name, stat.value.ToString("N0")));

            var skills = profile.skills;
            for (int i = 0; i < skills.Length; i++)
            {
                string name = i == 3 ? "Full Burst" : $"Skill {i + 1}";
                rows.Add(($"{name} ({skills[i].burstCost} Burst)", skills[i].displayName));
            }

            rows.Add(("Max Burst", $"{CharacterCombatProfile.MaximumBurst}   ·   START {CharacterCombatProfile.StartingBurst} / {CharacterCombatProfile.MaximumBurst}"));
            rows.Add(("Skill 1 / 2 / 3 Cost", $"{skills[0].burstCost} / {skills[1].burstCost} / {skills[2].burstCost} BURST"));
            rows.Add(("Turn End Gain", $"+{CharacterCombatProfile.TurnEndBurst} BURST / TURN"));
            rows.Add(("Ultimate Cost", $"{skills[3].burstCost} BURST (FULL BURST)"));
            return rows.ToArray();
        }
    }
}

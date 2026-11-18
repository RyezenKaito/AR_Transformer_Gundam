using System;
using GundamGame.CombatDemo;
using UnityEngine;
using TMPro;

namespace GundamLobby
{
    [AddComponentMenu("Gundam Lobby/Lobby Screen Data")]
    public class LobbyScreenData : MonoBehaviour
    {
        [Header("Player")]
        public string playerName = "AntAkA";
        public string playerTitle = "Sapphire Seahawk";
        public string[] currencyValues = { "1,960", "17,020", "0", "925" };
        [Header("Gundam")]
        public string strikerName = "ALYSNES";
        public string strikerClass = "Medium Attacker";
        [Header("Matchmaking")]
        public string modeName = "OPERATION VERGE";
        public string modeSubtitle = "MATCHMAKING MODE";
        public string strikeKey = "Space";
        public string strikeText = "STRIKE";
        [Header("System info (vertical text, right edge)")]
        public string accountId = "6665920640";
        public string buildVersion = "10.0.781944";
        public bool liveClock = true;
        [Header("Scene bindings (set by the layout builder)")]
        public TMP_Text playerNameLabel;
        public TMP_Text playerTitleLabel;
        public TMP_Text strikerNameLabel;
        public TMP_Text strikerClassLabel;
        [UnityEngine.Serialization.FormerlySerializedAs("abilitiesLabel")]
        public TMP_Text gradeBadgeLabel;
        public TMP_Text modeNameLabel;
        public TMP_Text modeSubtitleLabel;
        public TMP_Text strikeKeyLabel;
        public TMP_Text strikeTextLabel;
        public TMP_Text systemLabel;
        public TMP_Text[] currencyLabels;
        public TMP_Text[] radarLabels;
        public LobbyShape radar;

        int lastSecond = -1;

        static string At(string[] a, int i) { return a != null && i < a.Length ? a[i] : string.Empty; }
        static void Set(TMP_Text t, string s) { if (t) t.text = s; }

        void Awake() { Apply(); }
        void OnEnable() { Apply(); }

        public void Apply()
        {
            Set(playerNameLabel, playerName);
            Set(playerTitleLabel, playerTitle);
            if (!Application.isPlaying)
            {
                Set(strikerNameLabel, strikerName);
                Set(strikerClassLabel, strikerClass);
            }
            Set(modeNameLabel, modeName);
            Set(modeSubtitleLabel, modeSubtitle);
            Set(strikeKeyLabel, strikeKey);
            Set(strikeTextLabel, strikeText);
            for (int i = 0; currencyLabels != null && i < currencyLabels.Length; i++) Set(currencyLabels[i], At(currencyValues, i));
            ApplyGundam(PlayerSelectionData.Load(Application.isPlaying ? PlayerSelectionData.EquippedId : PlayerSelectionData.DefaultId));
            UpdateSystem(true);
        }

        public void ApplyGundam(CharacterCombatProfile profile)
        {
            if (!profile) return;
            Set(gradeBadgeLabel, profile.gradeBadge);
            var stats = GundamStatPresentation.CoreStats(profile);
            var scores = new float[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                var stat = stats[i];
                scores[i] = stat.score;
                if (radarLabels != null && i < radarLabels.Length)
                    Set(radarLabels[i], $"{stat.radarName} <b><color=#82DDF8>{GundamStatPresentation.Grade(stat.score)}</color></b>\n{stat.value:N0}");
            }
            if (!radar) return;
            radar.radarValues = scores;
            radar.SetVerticesDirty();
        }

        void UpdateSystem(bool force)
        {
            if (!systemLabel) return;
            DateTime now = DateTime.Now;
            if (!force && now.Second == lastSecond) return;
            lastSecond = now.Second;
            string time = liveClock ? now.ToString("HH:mm:ss") : "11:15:27";
            systemLabel.text = "ID " + accountId + "      " + buildVersion + "      System Time " + time;
        }

        void Update() { if (liveClock) UpdateSystem(false); }

#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () => { if (this) Apply(); };
        }
#endif
    }
}

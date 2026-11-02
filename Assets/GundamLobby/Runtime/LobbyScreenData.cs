using System;
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
        [Header("Striker (grades: S A B C D E F)")]
        public string strikerName = "ALYSNES";
        public string strikerClass = "Medium Attacker";
        public string[] abilities = { "Armor Purge", "Weapon Switch" };
        public string[] defNames = { "Ballistic DEF", "Energy DEF", "Blast DEF", "Physical DEF" };
        public string[] defGrades = { "C", "D", "B", "B" };
        public string[] radarNames = { "Solo Combat", "Team Combat", "Support", "Handling", "Combat Range", "Air Combat", "Armor" };
        public string[] radarGrades = { "B", "D", "E", "A", "D", "D", "B" };
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
        public TMP_Text abilitiesLabel;
        public TMP_Text modeNameLabel;
        public TMP_Text modeSubtitleLabel;
        public TMP_Text strikeKeyLabel;
        public TMP_Text strikeTextLabel;
        public TMP_Text systemLabel;
        public TMP_Text[] currencyLabels;
        public TMP_Text[] defNameLabels;
        public TMP_Text[] defGradeLabels;
        public TMP_Text[] radarLabels;
        public LobbyShape radar;

        int lastSecond = -1;

        public static float GradeValue(string grade)
        {
            if (string.IsNullOrEmpty(grade)) return .1f;
            switch (grade.Trim().ToUpperInvariant()[0])
            {
                case 'S': return 1f;
                case 'A': return 1f;
                case 'B': return .8f;
                case 'C': return .6f;
                case 'D': return .4f;
                case 'E': return .2f;
                default: return .1f;
            }
        }
        static string At(string[] a, int i) { return a != null && i < a.Length ? a[i] : string.Empty; }
        static void Set(TMP_Text t, string s) { if (t) t.text = s; }

        void Awake() { Apply(); }
        void OnEnable() { Apply(); }

        public void Apply()
        {
            Set(playerNameLabel, playerName);
            Set(playerTitleLabel, playerTitle);
            Set(strikerNameLabel, strikerName);
            Set(strikerClassLabel, strikerClass);
            if (abilitiesLabel) abilitiesLabel.text = abilities == null ? string.Empty : string.Join("   <color=#5d7686>|</color>   ", abilities);
            Set(modeNameLabel, modeName);
            Set(modeSubtitleLabel, modeSubtitle);
            Set(strikeKeyLabel, strikeKey);
            Set(strikeTextLabel, strikeText);
            for (int i = 0; currencyLabels != null && i < currencyLabels.Length; i++) Set(currencyLabels[i], At(currencyValues, i));
            for (int i = 0; defNameLabels != null && i < defNameLabels.Length; i++) Set(defNameLabels[i], At(defNames, i));
            for (int i = 0; defGradeLabels != null && i < defGradeLabels.Length; i++) Set(defGradeLabels[i], At(defGrades, i));
            for (int i = 0; radarLabels != null && i < radarLabels.Length; i++)
                Set(radarLabels[i], At(radarNames, i) + " <size=118%><b><color=#f4f8fa>" + At(radarGrades, i) + "</color></b></size>");
            if (radar)
            {
                int n = radarGrades != null ? radarGrades.Length : 0;
                if (n >= 3)
                {
                    var v = new float[n];
                    for (int i = 0; i < n; i++) v[i] = GradeValue(radarGrades[i]);
                    radar.radarValues = v;
                    radar.SetVerticesDirty();
                }
            }
            UpdateSystem(true);
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

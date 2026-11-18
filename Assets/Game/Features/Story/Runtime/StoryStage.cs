using GundamGame.CombatDemo;
using UnityEngine;

namespace GundamGame.Story
{
    [CreateAssetMenu(menuName = "Gundam/Story Stage")]
    public sealed class StoryStage : ScriptableObject
    {
        public const string LobbyScenePath = "Assets/Game/Scenes/HomeLobby.unity";
        public const int FormationSize = 3;

        public string stageId;
        public string locationName;
        public string scenePath;
        public GameObject battlefieldPrefab;
        public Sprite thumbnail;
        [TextArea] public string briefing;
        public EnemyProfile[] opponents = new EnemyProfile[FormationSize];
        public StoryStage nextStage;

        public bool Cleared => PlayerPrefs.GetInt("Gundam.Story.Cleared." + stageId) != 0;
        public void MarkCleared()
        {
            PlayerPrefs.SetInt("Gundam.Story.Cleared." + stageId, 1);
            PlayerPrefs.Save();
        }
    }
}

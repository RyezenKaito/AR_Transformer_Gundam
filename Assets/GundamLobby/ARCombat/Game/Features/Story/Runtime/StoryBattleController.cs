using System;
using System.Collections;
using System.Linq;
using GundamGame.CombatDemo;
using GundamGame.Unicorn;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GundamGame.Story
{
    public sealed class StoryBattleController : MonoBehaviour
    {
        [SerializeField] StoryStage stage;
        [SerializeField] GameObject battlefield;
        [SerializeField] Transform[] playerSlots = new Transform[StoryStage.FormationSize];
        [SerializeField] Transform[] opponentSlots = new Transform[StoryStage.FormationSize];
        [SerializeField] Vector3 cameraFocus = new(0, .85f, 0);
        [SerializeField] Vector2 cameraPitchYaw = new(24, 76);
        [SerializeField] float cameraDistance = 13.5f;

        [SerializeField] GameObject resultPanel;
        [SerializeField] StoryBattleHud battleHudPrefab;
        [SerializeField] TMP_Text stageLabel, resultTitle, resultDetail, primaryLabel;
        [SerializeField] Button primaryButton, lobbyButton, retryButton, resumeButton;

        bool resultShown, changingScene;
        readonly System.Collections.Generic.List<EnemyProfile> runtimeEnemyProfiles=new();
        readonly string rewardRunId=Guid.NewGuid().ToString("N");
        string primaryDestination;
        public CharacterBattlePackage ActivePackage { get; private set; }
        bool paused;

        IEnumerator Start()
        {
            primaryButton.onClick.AddListener(() => Open(primaryDestination));
            lobbyButton.onClick.AddListener(() => Open(StoryStage.LobbyScenePath));
            if (retryButton) retryButton.onClick.AddListener(() => Open(stage.scenePath));
            if (resumeButton) resumeButton.onClick.AddListener(TogglePause);

            if (!stage || !stage.battlefieldPrefab || !ValidFormation())
            {
                ShowResult("Unable to start battle", "Battlefield or formation data is missing.");
                yield break;
            }

            stageLabel.text = stage.stageId + "  /  " + stage.locationName;
            stageLabel.transform.parent.gameObject.SetActive(false);
            ShowResult("Loading", stage.locationName, showLobby: false);
            if (!battlefield) battlefield = Instantiate(stage.battlefieldPrefab, transform);

            var request = GameSessionManager.LoadEquippedGundamAsync();
            yield return request;
            if (request.asset is not CharacterCombatProfile profile || !profile.combatPrefab)
            {
                ShowResult("Unable to start battle", "Gundam combat package was not found.");
                yield break;
            }

            var root = new GameObject("Battle Roster");
            root.SetActive(false);
            root.transform.SetParent(transform, false);
            try
            {
                var packageObject = Instantiate(profile.combatPrefab, root.transform);
                var package = packageObject.GetComponent<CharacterBattlePackage>();
                if (!package)
                    throw new InvalidOperationException("Missing Gundam battle package.");

                var opponents = new DemoFighter[StoryStage.FormationSize];
                for (int i = 0; i < opponents.Length; i++)
                {
                    var slot = opponentSlots[i];
                    var enemy = Instantiate(stage.opponents[i].prefab, slot.position, slot.rotation, root.transform);
                    opponents[i] = enemy.GetComponent<DemoFighter>();
                    if (!opponents[i]) throw new InvalidOperationException("Enemy prefab has no DemoFighter.");
                    if(!Mathf.Approximately(GundamLobby.SortieSession.EnemyScale,1)&&enemy.TryGetComponent<EnemyBrain>(out var brain))
                    {
                     var adjusted=Instantiate(brain.profile);adjusted.moves=(EnemyProfile.Move[])brain.profile.moves.Clone();
                     for(int m=0;m<adjusted.moves.Length;m++)adjusted.moves[m].damage=Mathf.RoundToInt(adjusted.moves[m].damage*GundamLobby.SortieSession.EnemyScale);
                     runtimeEnemyProfiles.Add(adjusted);brain.profile=adjusted;
                    }
                    opponents[i].maxHealth=Mathf.RoundToInt(opponents[i].maxHealth*GundamLobby.SortieSession.EnemyScale);
                    opponents[i].attack=Mathf.RoundToInt(opponents[i].attack*GundamLobby.SortieSession.EnemyScale);
                    opponents[i].displayName += $" {i + 1:00}";
                }

                var playerSlot = playerSlots[1];
                package.player.transform.SetPositionAndRotation(playerSlot.position, playerSlot.rotation);
                GundamLobby.PlayerProgress.PrepareSortie(PlayerSelectionData.EquippedId);
                package.Configure(profile, opponents);
                package.ConfigureStory(cameraFocus, cameraPitchYaw, cameraDistance);
                Instantiate(battleHudPrefab, root.transform).Bind(package,profile,stage,TogglePause);
                var skin=package.player.gameObject.AddComponent<GundamLobby.GundamSkinBinding>();skin.unitId=GundamGame.CombatDemo.PlayerSelectionData.EquippedId;
                ActivePackage = package;
                root.SetActive(true);
                GundamLobby.PlayerProgress.CommitSortie(PlayerSelectionData.EquippedId);
                resultPanel.SetActive(false);
            }
            catch (Exception exception)
            {
                GundamLobby.PlayerProgress.ClearBattlePreparation();
                Destroy(root);
                Debug.LogException(exception, this);
                ShowResult("Unable to start battle", "Unable to load the battle formation.");
            }
        }

        void OnDestroy(){GundamLobby.PlayerProgress.ClearBattlePreparation();foreach(var profile in runtimeEnemyProfiles)if(profile)Destroy(profile);}

        bool ValidFormation()
        {
            int size = StoryStage.FormationSize;
            return playerSlots.Length == size && playerSlots.All(slot => slot)
                && opponentSlots.Length == size && opponentSlots.All(slot => slot)
                && stage.opponents.Length == size && stage.opponents.All(enemy => enemy && enemy.prefab);
        }

        void LateUpdate()
        {
            if (paused || resultShown || !ActivePackage) return;
            if (ActivePackage.Victory)
            {
                if(GundamLobby.SortieSession.Mode==0)stage.MarkCleared();
                int credits=GundamLobby.SortieSession.Reward(stage.creditReward),cores=GundamLobby.SortieSession.Reward(stage.fCoreReward);
                bool rewarded=!GundamLobby.SortieSession.Practice&&GundamLobby.PlayerProgress.AwardVictory(credits,cores,rewardRunId);
                resultShown = true;
                ShowResult("Victory", stageLabel.text + "\nTargets defeated   3 / 3"+(rewarded?"\nREWARDS  +"+credits.ToString("N0")+" CREDITS  /  +"+cores+" F-CORE":""),
                    stage.nextStage ? "Next stage" : null, stage.nextStage ? stage.nextStage.scenePath : null, showRetry: true);
            }
            else if (ActivePackage.Defeat)
            {
                resultShown = true;
                int defeated = ActivePackage.Opponents.Count(enemy => !enemy.IsAlive);
                ShowResult("Defeat", stageLabel.text + "\nTargets defeated   " + defeated + " / 3", showRetry: true);
            }
        }

        public void TogglePause()
        {
            if (changingScene || resultShown || !ActivePackage || ActivePackage.Cinematic ||
                ActivePackage.Victory || ActivePackage.Defeat) return;
            paused = !paused;
            ActivePackage.SetPaused(paused);
            if (paused) ShowResult("Paused", stageLabel.text, showRetry: true, showResume: true);
            else resultPanel.SetActive(false);
        }

        void ShowResult(string title, string detail, string primary = null, string destination = null,
            bool showLobby = true, bool showRetry = false, bool showResume = false)
        {
            primaryDestination = destination;
            resultTitle.text = title;
            resultDetail.text = detail;
            primaryButton.gameObject.SetActive(primary != null);
            primaryLabel.text = primary ?? string.Empty;
            lobbyButton.gameObject.SetActive(showLobby);
            if (retryButton) retryButton.gameObject.SetActive(showRetry);
            if (resumeButton) resumeButton.gameObject.SetActive(showResume);
            resultPanel.SetActive(true);
            var visible = new[]{resumeButton,retryButton,primaryButton,lobbyButton}
                .Where(button=>button && button.gameObject.activeSelf).ToArray();
            float step=.84f/Mathf.Max(1,visible.Length);
            for(int i=0;i<visible.Length;i++)
            {
                var rect=(RectTransform)visible[i].transform;
                rect.anchorMin=new Vector2(.08f+step*i+.009f,.15f);
                rect.anchorMax=new Vector2(.08f+step*(i+1)-.009f,.32f);
                rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
            if (visible.Length>0 && EventSystem.current) EventSystem.current.SetSelectedGameObject(visible[0].gameObject);
        }

        void Open(string scenePath)
        {
            if (changingScene) return;
            if (!Application.CanStreamedLevelBeLoaded(scenePath))
            {
                resultDetail.text = "Scene is not enabled in Build Settings.";
                return;
            }
            changingScene = true;
            resultTitle.text = "Loading";
            primaryButton.interactable = lobbyButton.interactable = false;
            if (retryButton) retryButton.interactable = false;
            if (resumeButton) resumeButton.interactable = false;
            SceneManager.LoadSceneAsync(scenePath);
        }
    }
}

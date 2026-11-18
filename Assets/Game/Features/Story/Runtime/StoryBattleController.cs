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
                ShowResult("Không thể mở trận", "Thiếu map hoặc dữ liệu đội hình.");
                yield break;
            }

            stageLabel.text = stage.stageId + "  /  " + stage.locationName;
            stageLabel.transform.parent.gameObject.SetActive(false);
            ShowResult("Đang tải", stage.locationName, showLobby: false);
            if (!battlefield) battlefield = Instantiate(stage.battlefieldPrefab, transform);

            var request = GameSessionManager.LoadEquippedGundamAsync();
            yield return request;
            if (request.asset is not CharacterCombatProfile profile || !profile.combatPrefab)
            {
                ShowResult("Không thể mở trận", "Không tìm thấy gói dữ liệu Gundam.");
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
                    opponents[i].displayName += $" {i + 1:00}";
                }

                var playerSlot = playerSlots[1];
                package.player.transform.SetPositionAndRotation(playerSlot.position, playerSlot.rotation);
                package.Configure(profile, opponents);
                package.ConfigureStory(cameraFocus, cameraPitchYaw, cameraDistance);
                Instantiate(battleHudPrefab, root.transform).Bind(package,profile,stage,TogglePause);
                ActivePackage = package;
                root.SetActive(true);
                resultPanel.SetActive(false);
            }
            catch (Exception exception)
            {
                Destroy(root);
                Debug.LogException(exception, this);
                ShowResult("Không thể mở trận", "Không tải được đội hình chiến đấu.");
            }
        }

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
                stage.MarkCleared();
                resultShown = true;
                ShowResult("Chiến thắng", stageLabel.text + "\nMục tiêu bị hạ   3 / 3",
                    stage.nextStage ? "Màn tiếp theo" : null, stage.nextStage ? stage.nextStage.scenePath : null, showRetry: true);
            }
            else if (ActivePackage.Defeat)
            {
                resultShown = true;
                int defeated = ActivePackage.Opponents.Count(enemy => !enemy.IsAlive);
                ShowResult("Thất bại", stageLabel.text + "\nMục tiêu bị hạ   " + defeated + " / 3", showRetry: true);
            }
        }

        public void TogglePause()
        {
            if (changingScene || resultShown || !ActivePackage || ActivePackage.Cinematic ||
                ActivePackage.Victory || ActivePackage.Defeat) return;
            paused = !paused;
            ActivePackage.SetPaused(paused);
            if (paused) ShowResult("Tạm dừng", stageLabel.text, showRetry: true, showResume: true);
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
                resultDetail.text = "Scene chưa được bật trong Build Settings.";
                return;
            }
            changingScene = true;
            resultTitle.text = "Đang tải";
            primaryButton.interactable = lobbyButton.interactable = false;
            if (retryButton) retryButton.interactable = false;
            if (resumeButton) resumeButton.interactable = false;
            SceneManager.LoadSceneAsync(scenePath);
        }
    }
}

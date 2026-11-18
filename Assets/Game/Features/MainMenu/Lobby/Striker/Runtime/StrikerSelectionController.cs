using TMPro;
using System.Linq;
using GundamGame.CombatDemo;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GundamLobby
{
    [DefaultExecutionOrder(100)]
    public class StrikerSelectionController : MonoBehaviour
    {
        public const string PreferenceKey = PlayerSelectionData.PreferenceKey;
        [Header("Editable roster and presentation")]
        public StrikerCatalog catalog;
        public int defaultIndex = 1;
        public StrikerRosterCard[] cards;
        public GameObject selectionCanvas, lobbyCanvas, previewMount;
        public HomeLobbyController lobby;
        public InventoryPanel inventory;
        public Camera lobbyCamera;
        public Button confirmButton;
        public TMP_Text nameLabel, serialLabel, roleLabel, statusLabel, counterLabel;
        public TMP_Text[] statValues;
        public CanvasGroup toast;
        public TMP_Text toastLabel;
        public int SelectedIndex { get; private set; }
        public int EquippedIndex { get; private set; }
        public bool IsOpen => selectionCanvas && selectionCanvas.activeSelf;
        public GameObject PreviewModel => previewModel;
        int[] playable;
        GameObject previewModel;
        Vector3 cameraPosition;
        Quaternion cameraRotation;
        Vector3 homeBaseCameraPosition;
        Quaternion homeBaseCameraRotation;
        float homeBaseFov;
        float cameraFov, toastUntil;
        bool pivotWasActive, lobbyWasEnabled;
        int screenW, screenH;

        void Awake()
        {
            selectionCanvas.SetActive(false);
            previewMount.SetActive(false);
            playable = Enumerable.Range(0, catalog.units.Length).Where(i => PlayerSelectionData.Supports(catalog.units[i].id))
                .OrderBy(i => catalog.units[i].id == PlayerSelectionData.DefaultId ? 0 : 1).ToArray();
            int saved = System.Array.FindIndex(playable, i => catalog.units[i].id == PlayerSelectionData.EquippedId);
            SelectedIndex = EquippedIndex = playable[Mathf.Max(0, saved)];
            foreach (var card in cards) card.gameObject.SetActive(playable.Contains(card.index));
            foreach (int index in playable) cards[index].transform.SetAsLastSibling();
            homeBaseCameraPosition = lobbyCamera.transform.position;
            homeBaseCameraRotation = lobbyCamera.transform.rotation;
            homeBaseFov = lobbyCamera.fieldOfView;
        }

        void Start()
        {
            ApplyToLobby(catalog.units[EquippedIndex]);
            FrameLobbyUnit(catalog.units[EquippedIndex]);
        }

        public void Open()
        {
            if (IsOpen) return;
            if (inventory && inventory.IsOpen) inventory.Close();
            cameraPosition = lobbyCamera.transform.position;
            cameraRotation = lobbyCamera.transform.rotation;
            cameraFov = lobbyCamera.fieldOfView;
            lobbyWasEnabled = lobby.enabled;
            pivotWasActive = lobby.modelPivot.gameObject.activeSelf;
            lobby.enabled = false;
            lobby.modelPivot.gameObject.SetActive(false);
            lobbyCanvas.SetActive(false);
            selectionCanvas.SetActive(true);
            previewMount.SetActive(true);
            FitScreen();
            Select(EquippedIndex);
            toast.alpha = 0;
        }

        public void Select(int index)
        {
            if (!playable.Contains(index)) return;
            SelectedIndex = index;
            StrikerDefinition unit = catalog.units[index];
            if (previewModel) { previewModel.SetActive(false); Destroy(previewModel); }
            previewMount.transform.position = lobby.modelPivot.position;
            previewMount.transform.rotation = Quaternion.Euler(0, -12, 0);
            previewModel = Instantiate(unit.prefab, previewMount.transform);
            previewModel.name = "Striker Preview / " + unit.id;
            previewModel.transform.localPosition = Vector3.zero;
            previewModel.transform.localRotation = Quaternion.identity;
            nameLabel.text = unit.displayName;
            serialLabel.text = unit.code;
            roleLabel.text = unit.role + "  /  Lv. " + unit.level;
            var profile = PlayerSelectionData.Load(unit.id);
            counterLabel.text = $"{playable.Length:00}/{playable.Length:00}";
            ApplySpecification(profile);
            statusLabel.text = index == EquippedIndex ? "ACTIVE GUNDAM" : "GUNDAM PREVIEW";
            confirmButton.interactable = index != EquippedIndex;
            var hover = confirmButton.GetComponent<LobbyHover>();
            hover.normal = index == EquippedIndex ? new Color(.063f, .165f, .216f) : new Color(.031f, .486f, .671f);
            hover.enabled = index != EquippedIndex;
            confirmButton.targetGraphic.color = hover.normal;
            confirmButton.GetComponentInChildren<TMP_Text>().text = index == EquippedIndex ? "GUNDAM EQUIPPED" : "EQUIP GUNDAM";
            foreach (int visible in playable) cards[visible].Bind(catalog.units[visible], visible == index, visible == EquippedIndex);
            FramePreview();
        }

        public void ApplySpecification(CharacterCombatProfile profile)
        {
            var rows = GundamStatPresentation.Specification(profile);
            for (int i = 0; i < rows.Length; i++)
            {
                var value = statValues[i];
                value.transform.parent.Find("Label").GetComponent<TMP_Text>().text = rows[i].label;
                value.text = rows[i].value;
            }
        }

        void FramePreview()
        {
            var renderers = previewModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            Vector3 target = new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * .56f, bounds.center.z);
            float aspect = Mathf.Max(.8f, Screen.width / (float)Mathf.Max(1, Screen.height));
            float distance = Mathf.Max(bounds.size.y * 1.75f, bounds.size.x * 2.6f / aspect);
            lobbyCamera.transform.position = target + new Vector3(1.1f, -.5f, -distance);
            lobbyCamera.transform.LookAt(target);
            lobbyCamera.fieldOfView = 34;
        }

        public void RotatePreview(float degrees) { if (IsOpen) previewMount.transform.Rotate(0, degrees, 0, Space.Self); }
        public void ZoomPreview(float amount) { if (IsOpen) lobbyCamera.fieldOfView = Mathf.Clamp(lobbyCamera.fieldOfView + amount, 24, 48); }

        public void ConfirmSelection()
        {
            if (!IsOpen) return;
            EquippedIndex = SelectedIndex;
            var unit = catalog.units[EquippedIndex];
            PlayerSelectionData.Equip(unit.id);
            ApplyToLobby(unit);
            Close();
            FrameLobbyUnit(unit);
            lobby.ShowComingSoon();
            if (lobby.toastLabel) lobby.toastLabel.text = "ĐÃ CHỌN " + unit.displayName;
        }

        void ApplyToLobby(StrikerDefinition unit)
        {
            lobby.ReplaceGundam(unit.prefab, unit.displayName, unit.role, unit.level);
            if (lobby.TryGetComponent<LobbyScreenData>(out var screen))
                screen.ApplyGundam(PlayerSelectionData.Load(unit.id));
            if (inventory) inventory.SetPreviewModel(unit.prefab);
        }

        void FrameLobbyUnit(StrikerDefinition unit)
        {
            var model = lobby.PreviewModel ? lobby.PreviewModel : unit.prefab;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float ratio = Mathf.Clamp(bounds.size.y / 8.65f, .5f, 1.15f);
            Vector3 ground = lobby.modelPivot.position;
            lobbyCamera.transform.SetPositionAndRotation(ground + (homeBaseCameraPosition - ground) * ratio, homeBaseCameraRotation);
            lobbyCamera.fieldOfView = homeBaseFov;
        }

        public void Close()
        {
            if (!IsOpen) return;
            selectionCanvas.SetActive(false);
            previewMount.SetActive(false);
            if (previewModel) { previewModel.SetActive(false); Destroy(previewModel); previewModel = null; }
            lobbyCamera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            lobbyCamera.fieldOfView = cameraFov;
            lobby.modelPivot.gameObject.SetActive(pivotWasActive);
            lobbyCanvas.SetActive(true);
            lobby.enabled = lobbyWasEnabled;
        }

        public void Notify() { toastLabel.text = "CHỨC NĂNG ĐANG PHÁT TRIỂN"; toastUntil = Time.unscaledTime + 2.5f; }
        void FitScreen()
        {
            screenW = Screen.width; screenH = Screen.height;
            selectionCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight = screenW / (float)Mathf.Max(1, screenH) >= 16f / 9f ? 1 : 0;
        }
        void Update()
        {
            if (!IsOpen) return;
            if (screenW != Screen.width || screenH != Screen.height) FitScreen();
            toast.alpha = Mathf.MoveTowards(toast.alpha, Time.unscaledTime < toastUntil ? 1 : 0, Time.unscaledDeltaTime * 7);
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
            else if (Keyboard.current.enterKey.wasPressedThisFrame) ConfirmSelection();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame) StepSelection(1);
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame) StepSelection(-1);
#endif
        }

        void StepSelection(int offset)
        {
            int slot = System.Array.IndexOf(playable, SelectedIndex);
            Select(playable[(slot + offset + playable.Length) % playable.Length]);
        }
    }
}

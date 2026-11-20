using TMPro;
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
        public const string PreferenceKey = "GundamLobby.ActiveStriker";
        [Header("Editable roster and presentation")]
        public StrikerCatalog catalog;
        public int defaultIndex = 1;
        public StrikerRosterCard[] cards;
        public GameObject selectionCanvas, lobbyCanvas, previewMount;
        public HomeLobbyController lobby;
        public InventoryPanel inventory;
        public Camera lobbyCamera;
        public Button confirmButton;
        public TMP_Text nameLabel, serialLabel, roleLabel, powerLabel, statusLabel, counterLabel;
        public TMP_Text[] statValues;
        public CanvasGroup toast;
        public TMP_Text toastLabel;
        public int SelectedIndex { get; private set; }
        public int EquippedIndex { get; private set; }
        public bool IsOpen => selectionCanvas && selectionCanvas.activeSelf;
        public GameObject PreviewModel => previewModel;
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
            SelectedIndex = EquippedIndex = Mathf.Clamp(defaultIndex, 0, catalog.units.Length - 1);
            homeBaseCameraPosition = lobbyCamera.transform.position;
            homeBaseCameraRotation = lobbyCamera.transform.rotation;
            homeBaseFov = lobbyCamera.fieldOfView;
        }

        void Start()
        {
            string stored = PlayerPrefs.GetString(PreferenceKey, "");
            int saved = System.Array.FindIndex(catalog.units, u => u.id == stored);
            if (saved >= 0) SelectedIndex = EquippedIndex = saved;
            ApplyToLobby(catalog.units[EquippedIndex]); FrameLobbyUnit(catalog.units[EquippedIndex]);
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
            if (index < 0 || index >= catalog.units.Length) return;
            SelectedIndex = index;
            StrikerDefinition unit = catalog.units[index];
            if (previewModel) { previewModel.SetActive(false); Destroy(previewModel); }
            previewMount.transform.position = lobby.modelPivot.position;
            previewMount.transform.rotation = Quaternion.Euler(0, -12, 0);
            previewModel = Instantiate(unit.prefab, previewMount.transform);
            previewModel.name = "Striker Preview / " + unit.id;var skin=previewModel.AddComponent<GundamSkinBinding>();skin.unitId=unit.id;
            previewModel.transform.localPosition = Vector3.zero;
            previewModel.transform.localRotation = Quaternion.identity;
            nameLabel.text = unit.displayName;
            serialLabel.text = unit.code;
            roleLabel.text = unit.role + "  /  LV. " + PlayerProgress.UnitLevel(unit.id);
            powerLabel.text = unit.power.ToString("N0");
            counterLabel.text = (index + 1).ToString("00") + " / " + catalog.units.Length.ToString("00");
            var values = EnglishLobbyStats.Specification(unit);
            string[] names={"MAX HP","ATTACK","DEFENSE","SPEED","BREAK DAMAGE","ARMOR","SKILL 1","SKILL 2","SKILL 3","FULL BURST","MAX BURST","SKILL COSTS","TURN END GAIN","ULTIMATE COST"};
            for(int i=0;i<statValues.Length;i++)
            {
                statValues[i].transform.parent.gameObject.SetActive(i<values.Length);
                if(i>=values.Length)continue;
                statValues[i].text=values[i];
                var label=statValues[i].transform.parent.Find("Label");
                if(label)label.GetComponent<TMP_Text>().text=names[i];
                statValues[i].enableAutoSizing=true;statValues[i].fontSizeMin=13;statValues[i].fontSizeMax=20;
            }
            statusLabel.text = index == EquippedIndex ? "ACTIVE GUNDAM" : "GUNDAM PREVIEW";
            confirmButton.interactable = index != EquippedIndex;
            var hover = confirmButton.GetComponent<LobbyHover>();
            hover.normal = index == EquippedIndex ? new Color(.063f, .165f, .216f) : new Color(.031f, .486f, .671f);
            hover.enabled = index != EquippedIndex;
            confirmButton.targetGraphic.color = hover.normal;
            confirmButton.GetComponentInChildren<TMP_Text>().text = !PlayerProgress.OwnsGundam(unit.id)?"PURCHASE  "+PlayerProgress.Rules.GundamPrice(unit.id).ToString("N0")+" CREDITS":index == EquippedIndex ? "EQUIPPED" : "EQUIP GUNDAM";
            for (int i = 0; i < cards.Length; i++) cards[i].Bind(catalog.units[i], i == index, i == EquippedIndex);
            FramePreview();
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
            var unit = catalog.units[SelectedIndex];
            if(!PlayerProgress.OwnsGundam(unit.id)&&!PlayerProgress.BuyGundam(unit.id,out var purchaseMessage)){toastLabel.text=purchaseMessage;toastUntil=Time.unscaledTime+3;return;}
            EquippedIndex = SelectedIndex;
            PlayerPrefs.SetString(PreferenceKey, unit.id);
            PlayerPrefs.Save();
            ApplyToLobby(unit);
            Close();
            FrameLobbyUnit(unit);
            lobby.ShowComingSoon();
            if (lobby.toastLabel) lobby.toastLabel.text = "EQUIPPED " + unit.displayName;
        }

        void ApplyToLobby(StrikerDefinition unit)
        {
            lobby.ReplaceGundam(unit.prefab, unit.displayName, unit.role, unit.level);
            if (inventory) inventory.SetPreviewModel(unit.prefab);
        }

        void FrameLobbyUnit(StrikerDefinition unit)
        {
            var model = lobby.PreviewModel ? lobby.PreviewModel : unit.prefab;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float ratio = Mathf.Clamp(bounds.size.y / 8.65f, .72f, 1.05f);
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

        public void Notify() { LobbyServices.For(FindFirstObjectByType<HomeLobbyDashboard>()).Open("ITEM GUIDE"); }
        void FitScreen()
        {
            screenW = Screen.width; screenH = Screen.height;
            selectionCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight = screenW / (float)Mathf.Max(1, screenH) >= 16f / 9f ? 1 : 0;
        }
        void Update()
        {
            if (!IsOpen || LobbyServices.BlocksInput) return;
            if (screenW != Screen.width || screenH != Screen.height) FitScreen();
            toast.alpha = Mathf.MoveTowards(toast.alpha, Time.unscaledTime < toastUntil ? 1 : 0, Time.unscaledDeltaTime * 7);
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) Close();
            else if (Keyboard.current.enterKey.wasPressedThisFrame) ConfirmSelection();
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame) Select((SelectedIndex + 1) % catalog.units.Length);
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame) Select((SelectedIndex + catalog.units.Length - 1) % catalog.units.Length);
#endif
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GundamLobby
{
    public class HomeLobbyController : MonoBehaviour
    {
        [Header("Replace presentation assets here")]
        public Transform modelPivot;
        public GameObject placeholderModel;
        public GameObject gundamPrefab;
        public GameObject placeholderHangar;
        public Transform environmentMount;
        public GameObject hangarPrefab;
        public Image avatarImage;
        public Sprite avatarSprite;
        public Image bannerImage;
        public Sprite bannerSprite;
        public Image[] skillImages;
        public Sprite[] skillSprites;
        [Header("Sample data")]
        public string playerName = "COMMANDER";
        public string gundamName = "RX-78 / AEGIS";
        public string role = "TẤN CÔNG";
        public int level = 20;
        public string[] statValues = { "12,480", "2,460", "1,180", "112" };
        [Header("Scene bindings")]
        public TMP_Text playerLabel;
        public TMP_Text nameLabel;
        public TMP_Text roleLabel;
        public TMP_Text presentationClassLabel;
        public TMP_Text[] statLabels;
        public CanvasGroup toast;
        public TMP_Text toastLabel;
        [Header("Preview motion")]
        public float rotationSpeed = .22f;
        public float idleAmplitude = .018f;
        public float maximumYaw = 65;
        float toastUntil, yaw;
        Vector3 basePosition;
        Quaternion baseRotation;
        bool dragging;
        GameObject spawnedModel, spawnedHangar;
        public int feedbackCount { get; private set; }

        void Awake()
        {
            if (modelPivot) { basePosition = modelPivot.localPosition; baseRotation = modelPivot.localRotation; }
            if (gundamPrefab && modelPivot)
            {
                if (placeholderModel) placeholderModel.SetActive(false);
                spawnedModel = Instantiate(gundamPrefab, modelPivot);
                spawnedModel.transform.localPosition = Vector3.zero;
                spawnedModel.transform.localRotation = Quaternion.identity;
            }
            if (hangarPrefab && environmentMount)
            {
                if (placeholderHangar) placeholderHangar.SetActive(false);
                spawnedHangar = Instantiate(hangarPrefab, environmentMount);
            }
            ApplyPresentation();
            if (toast) { toast.alpha = 0; toast.blocksRaycasts = false; toast.interactable = false; }
        }
        public void ReplaceGundam(GameObject prefab, string title, string unitRole, int unitLevel)
        {
            if (!prefab || !modelPivot) return;
            if (spawnedModel) { spawnedModel.SetActive(false); Destroy(spawnedModel); }
            if (placeholderModel) placeholderModel.SetActive(false);
            gundamPrefab = prefab;
            spawnedModel = Instantiate(prefab, modelPivot);
            spawnedModel.transform.localPosition = Vector3.zero;
            spawnedModel.transform.localRotation = Quaternion.identity;
            gundamName = title; role = unitRole; level = unitLevel;
            ApplyPresentation();
        }
        public void ApplyPresentation()
        {
            if (playerLabel) playerLabel.text = playerName;
            if (nameLabel) nameLabel.text = gundamName;
            if (roleLabel) roleLabel.text = role + "   /   Lv. " + level;
            if (presentationClassLabel) presentationClassLabel.text = role + "  /  Lv. " + level;
            for (int i = 0; statLabels != null && i < statLabels.Length; i++) if (statLabels[i] && i < statValues.Length) statLabels[i].text = statValues[i];
            if (avatarImage) { avatarImage.sprite = avatarSprite; avatarImage.enabled = avatarSprite; }
            if (bannerImage) { bannerImage.sprite = bannerSprite; bannerImage.enabled = bannerSprite; }
            for (int i = 0; skillImages != null && i < skillImages.Length; i++)
                if (skillImages[i]) { var s = skillSprites != null && i < skillSprites.Length ? skillSprites[i] : null; skillImages[i].sprite = s; skillImages[i].enabled = s; }
        }
        public void ShowComingSoon()
        {
            feedbackCount++;
            if (toastLabel) toastLabel.text = "CHỨC NĂNG ĐANG PHÁT TRIỂN";
            toastUntil = Time.unscaledTime + 2.6f;
        }
        public void ShowFeedback(string title)
        {
            ShowComingSoon();
            if (toastLabel && !string.IsNullOrEmpty(title)) toastLabel.text = title + "  ·  Chức năng đang phát triển";
        }
        void Update()
        {
            if (toast) toast.alpha = Mathf.MoveTowards(toast.alpha, Time.unscaledTime < toastUntil ? 1 : 0, Time.unscaledDeltaTime * 7);
            bool pressed = false, held = false;
            float delta = 0;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                pressed = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                delta = Mouse.current.delta.ReadValue().x;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            pressed = Input.GetMouseButtonDown(0); held = Input.GetMouseButton(0); delta = Input.GetAxis("Mouse X") * 12;
#endif
            if (pressed) dragging = EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
            if (!held) dragging = false;
            if (dragging) yaw = Mathf.Clamp(yaw - delta * rotationSpeed, -maximumYaw, maximumYaw);
            if (modelPivot)
            {
                modelPivot.localRotation = baseRotation * Quaternion.Euler(0, yaw + Mathf.Sin(Time.unscaledTime * .32f) * .6f, 0);
                modelPivot.localPosition = basePosition + Vector3.up * Mathf.Sin(Time.unscaledTime * .8f) * idleAmplitude;
            }
        }
    }
}

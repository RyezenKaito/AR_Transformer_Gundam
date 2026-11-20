using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace GundamGame.CombatDemo
{
    /// <summary>
    /// Places the complete battle board in AR; unsupported devices and Editor use the authored camera.
    /// Keep this component outside arenaRoot and xrRoot so it can always restore virtual mode.
    /// Battle availability is sent to an optional controller through SetGameplayEnabled(bool).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class RougeArenaPlacement : MonoBehaviour
    {
        public Transform arenaRoot;
        public Camera virtualCamera;
        public Camera arCamera;
        public GameObject xrRoot;
        public ARSession session;
        public ARPlaneManager planes;
        public ARRaycastManager raycasts;
        public ARAnchorManager anchors;
        public RougeBattleDemo battleController;
        [Range(0.03f, 0.3f)] public float arScale = 0.1f;
        public bool startWithAROnDevice = true;
        public bool showOverlay = true;

        public bool IsARMode { get; private set; }
        public bool IsPlaced { get; private set; }
        public bool CanBattle { get; private set; } = true;
        public bool IsChecking { get; private set; }
        public string Status { get; private set; } = "Virtual arena";
        public bool HasTrackedAnchor => anchor != null && anchor.trackingState == TrackingState.Tracking;
        public event Action<bool> BattleAvailabilityChanged;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
        readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        Transform originalParent;
        Vector3 originalPosition;
        Quaternion originalRotation;
        Vector3 originalScale;
        ARAnchor anchor;
        Canvas overlay;
        Text statusText;
        Button modeButton;
        Text modeLabel;
        Button replaceButton;
        int generation;
        bool placing;
        bool hasInitialized;
        Coroutine checkRoutine;
        Material planeOutlineMaterial;

        void Awake()
        {
            if (arenaRoot != null)
            {
                originalParent = arenaRoot.parent;
                originalPosition = arenaRoot.localPosition;
                originalRotation = arenaRoot.localRotation;
                originalScale = arenaRoot.localScale;
            }
            hasInitialized = true;
            ApplyVirtualMode("Virtual arena | Editor / unsupported phones");
        }

        IEnumerator Start()
        {
            if (showOverlay) CreateOverlay();
            // Let the battle's Start finish before sending its initial availability.
            yield return null;
            PublishAvailability(true, true);
            if (startWithAROnDevice && !Application.isEditor && Application.isMobilePlatform)
                RequestARMode();
        }

        void Update()
        {
            if (IsARMode)
            {
                bool tracking = ARSession.state == ARSessionState.SessionTracking;
                bool anchorTracking = anchor == null || anchor.trackingState == TrackingState.Tracking;
                PublishAvailability(IsPlaced && tracking && anchorTracking);
                if (!tracking || (IsPlaced && !anchorTracking))
                    Status = "Tracking paused | aim at the original surface";
                else if (!IsPlaced)
                    Status = placing ? "Placing board..." : "Move phone slowly, then tap a horizontal surface";
                else
                    Status = anchor != null ? "AR board locked | battle ready" : "AR board placed | battle ready";

                if (!IsPlaced && !placing && tracking && TryGetPress(out Vector2 screenPosition) && !OverUI(screenPosition))
                {
                    if (raycasts != null && raycasts.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
                    {
                        var hit = hits[0];
                        // Reject walls; this board is a tabletop / floor arena.
                        if (Vector3.Dot(hit.pose.rotation * Vector3.up, Vector3.up) > 0.8f)
                            PlaceAt(hit.pose);
                    }
                }
                ShowPlanes(!IsPlaced);
            }
            UpdateOverlay();
        }

        public void RequestARMode()
        {
            if (IsChecking || IsARMode) return;
            checkRoutine = StartCoroutine(CheckAndStartAR());
        }

        public void UseVirtualMode()
        {
            generation++;
            if (checkRoutine != null) StopCoroutine(checkRoutine);
            checkRoutine = null;
            IsChecking = false;
            ApplyVirtualMode("Virtual arena | battle ready");
        }

        public void Reposition()
        {
            if (!IsARMode || placing) return;
            generation++;
            RemoveAnchor();
            IsPlaced = false;
            arenaRoot.gameObject.SetActive(false);
            if (planes != null) planes.enabled = true;
            PublishAvailability(false);
            ShowPlanes(true);
        }

        IEnumerator CheckAndStartAR()
        {
            IsChecking = true;
            if (Application.isEditor || !Application.isMobilePlatform)
            {
                FinishFallback("AR is available on supported Android / iOS devices; using virtual arena");
                yield break;
            }
            if (session == null || xrRoot == null || arCamera == null || raycasts == null || arenaRoot == null)
            {
                FinishFallback("AR references are incomplete; using virtual arena");
                yield break;
            }

            Status = "Checking AR support...";
            // XR loader initialization normally happens before scene Start; allow a delayed loader to settle.
            var manager = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
            float until = Time.realtimeSinceStartup + 6f;
            while (manager != null && !manager.isInitializationComplete && Time.realtimeSinceStartup < until)
                yield return null;
            if (manager == null || manager.activeLoader == null)
            {
                FinishFallback("No AR provider on this device; using virtual arena");
                yield break;
            }

            bool availabilityDone = false;
            Coroutine availability = StartCoroutine(WaitForAvailability(() => availabilityDone = true));
            until = Time.realtimeSinceStartup + 10f;
            while (!availabilityDone && Time.realtimeSinceStartup < until) yield return null;
            if (!availabilityDone)
            {
                StopCoroutine(availability);
                FinishFallback("AR check timed out; using virtual arena");
                yield break;
            }
            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                FinishFallback("Install or update Google Play Services for AR, then retry AR");
                yield break;
            }
            if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None)
            {
                FinishFallback("AR is not supported on this device; using virtual arena");
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                until = Time.realtimeSinceStartup + 30f;
                while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera)
                       && Time.realtimeSinceStartup < until) yield return null;
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                {
                    FinishFallback("Camera permission is required for AR; virtual arena is ready");
                    yield break;
                }
            }
#elif UNITY_IOS && !UNITY_EDITOR
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                FinishFallback("Camera permission is required for AR; virtual arena is ready");
                yield break;
            }
#endif
            generation++;
            IsARMode = true;
            IsPlaced = false;
            placing = false;
            PublishAvailability(false);
            arenaRoot.gameObject.SetActive(false);
            if (virtualCamera != null) virtualCamera.gameObject.SetActive(false);
            xrRoot.SetActive(true);
            arCamera.gameObject.SetActive(true);
            arCamera.enabled = true;
            session.gameObject.SetActive(true);
            session.enabled = true;
            if (planes != null)
            {
                planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                planes.enabled = true;
            }
            raycasts.enabled = true;
            if (anchors != null) anchors.enabled = true;
            IsChecking = false;
            checkRoutine = null;
            Status = "Move phone slowly, then tap a horizontal surface";
        }

        IEnumerator WaitForAvailability(Action completed)
        {
            yield return ARSession.CheckAvailability();
            completed();
        }

        void FinishFallback(string reason)
        {
            IsChecking = false;
            checkRoutine = null;
            ApplyVirtualMode(reason);
        }

        void ApplyVirtualMode(string reason)
        {
            if (!hasInitialized) return;
            RemoveAnchor();
            IsARMode = false;
            IsPlaced = true;
            placing = false;
            if (session != null) session.enabled = false;
            if (xrRoot != null) xrRoot.SetActive(false);
            if (arCamera != null) arCamera.enabled = false;
            if (virtualCamera != null)
            {
                virtualCamera.gameObject.SetActive(true);
                virtualCamera.enabled = true;
            }
            if (arenaRoot != null)
            {
                arenaRoot.SetParent(originalParent, false);
                arenaRoot.localPosition = originalPosition;
                arenaRoot.localRotation = originalRotation;
                arenaRoot.localScale = originalScale;
                arenaRoot.gameObject.SetActive(true);
            }
            Status = reason;
            PublishAvailability(true);
        }

        async void PlaceAt(Pose pose)
        {
            placing = true;
            int thisGeneration = generation;
            ARAnchor created = null;
            // Flatten the board so combat local Y remains its up axis, and face the user.
            Vector3 towardCamera = arCamera.transform.position - pose.position;
            towardCamera.y = 0;
            pose.rotation = towardCamera.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(-towardCamera.normalized, Vector3.up)
                : Quaternion.identity;
            if (anchors != null && anchors.subsystem != null && anchors.subsystem.running)
            {
                try
                {
                    var result = await anchors.TryAddAnchorAsync(pose);
                    if (result.status.IsSuccess()) created = result.value;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Rouge AR anchor unavailable; using the raycast pose. " + exception.Message);
                }
            }
            if (this == null || !isActiveAndEnabled || thisGeneration != generation || !IsARMode)
            {
                if (created != null) Destroy(created.gameObject);
                return;
            }
            anchor = created;
            arenaRoot.SetParent(anchor != null ? anchor.transform : null, false);
            if (anchor != null)
            {
                arenaRoot.localPosition = Vector3.zero;
                arenaRoot.localRotation = Quaternion.identity;
            }
            else arenaRoot.SetPositionAndRotation(pose.position, pose.rotation);
            arenaRoot.localScale = originalScale * arScale;
            arenaRoot.gameObject.SetActive(true);
            IsPlaced = true;
            placing = false;
            ShowPlanes(false);
            // Preserve plane tracking itself: the anchor still needs its world tracking updates.
            PublishAvailability(ARSession.state == ARSessionState.SessionTracking);
        }

        void RemoveAnchor()
        {
            // Always detach the arena before destroying the tracked anchor.
            if (arenaRoot != null) arenaRoot.SetParent(originalParent, true);
            if (anchor != null) Destroy(anchor.gameObject);
            anchor = null;
        }

        void PublishAvailability(bool available, bool force = false)
        {
            if (!force && CanBattle == available) return;
            CanBattle = available;
            BattleAvailabilityChanged?.Invoke(available);
            if (battleController != null) battleController.SetGameplayEnabled(available);
        }

        void ShowPlanes(bool visible)
        {
            if (planes == null) return;
            foreach (ARPlane plane in planes.trackables)
            {
                // A detected plane has no renderer unless a prefab is supplied.
                // Draw its actual boundary so the placement surface is visible.
                if (visible && plane.boundary.IsCreated && plane.boundary.Length >= 3)
                {
                    var guide = plane.transform.Find("Rouge placement outline");
                    LineRenderer outline;
                    if (guide == null)
                    {
                        if (planeOutlineMaterial == null)
                        {
                            planeOutlineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                            planeOutlineMaterial.color = new Color(.1f, .85f, 1f);
                        }
                        var go = new GameObject("Rouge placement outline");
                        go.transform.SetParent(plane.transform, false);
                        outline = go.AddComponent<LineRenderer>();
                        outline.sharedMaterial = planeOutlineMaterial;
                        outline.useWorldSpace = false; outline.loop = true;
                        outline.startWidth = outline.endWidth = .004f;
                    }
                    else outline = guide.GetComponent<LineRenderer>();
                    outline.positionCount = plane.boundary.Length;
                    for (int i = 0; i < plane.boundary.Length; i++)
                    {
                        Vector2 p = plane.boundary[i];
                        outline.SetPosition(i, new Vector3(p.x, .003f, p.y));
                    }
                }
                foreach (Renderer renderer in plane.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = visible;
            }
        }

        bool TryGetPress(out Vector2 position)
        {
            position = Vector2.zero;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }
            return false;
        }

        bool OverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }

        void CreateOverlay()
        {
            var go = new GameObject("AR placement controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            overlay = go.GetComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.sortingOrder = 35;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
            var panel = new GameObject("Placement HUD", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(go.transform, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.70f);
            rt.anchorMax = new Vector2(0.45f, 0.82f);
            rt.pivot = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.075f, 0.92f);
            statusText = MakeText(panel.transform, "Placement status", 15);
            RectTransform statusRect = statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0, 0.48f);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.offsetMin = new Vector2(10, 5);
            statusRect.offsetMax = new Vector2(-6, -5);
            modeButton = MakeButton(panel.transform, "AR", new Vector2(0.02f, 0.06f), new Vector2(0.49f, 0.47f), () =>
            {
                if (IsARMode || IsChecking) UseVirtualMode(); else RequestARMode();
            });
            modeLabel = modeButton.GetComponentInChildren<Text>();
            replaceButton = MakeButton(panel.transform, "Place again", new Vector2(0.51f, 0.06f), new Vector2(0.98f, 0.47f), Reposition);
        }

        static Text MakeText(Transform parent, string name, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        static Button MakeButton(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.12f, 0.3f, 0.4f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text text = MakeText(go.transform, "Label", 15);
            text.text = label;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(4, 4);
            text.rectTransform.offsetMax = new Vector2(-4, -4);
            return button;
        }

        void UpdateOverlay()
        {
            if (statusText != null) statusText.text = Status;
            if (modeLabel != null) modeLabel.text = IsARMode || IsChecking ? "Virtual" : "AR";
            if (replaceButton != null) replaceButton.interactable = IsARMode && IsPlaced && !placing;
        }

        void OnDestroy()
        {
            generation++;
            if (planeOutlineMaterial != null) Destroy(planeOutlineMaterial);
            if (anchor != null && arenaRoot != null) arenaRoot.SetParent(originalParent, true);
            if (anchor != null) Destroy(anchor.gameObject);
        }
    }
}


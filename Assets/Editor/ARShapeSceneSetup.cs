using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Creates Assets/Scenes/ARShapeScene.unity from ARScene and adds:
/// shape placer (tap to place + drag to move), transparent shape menu, EventSystem.
/// </summary>
public static class ARShapeSceneSetup
{
    const string SourceScene = "Assets/Scenes/ARScene.unity";
    const string TargetScene = "Assets/Scenes/ARShapeScene.unity";

    struct ShapeInfo
    {
        public string name;
        public Color color;
        public ShapeInfo(string n, Color c) { name = n; color = c; }
    }

    [MenuItem("AR/Setup Shape Scene")]
    public static void Setup()
    {
        ARShapeAssetBuilder.Build();

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) == null)
            AssetDatabase.CopyAsset(SourceScene, TargetScene);

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);

        // ---- XR Origin: replace the old single-prefab placer by the shape placer
        var raycast = Object.FindFirstObjectByType<ARRaycastManager>();
        GameObject origin = raycast.gameObject;

        var old = origin.GetComponent<ARTapToPlaceObject>();
        if (old != null) Object.DestroyImmediate(old);

        var placer = origin.GetComponent<ARShapePlacer>();
        if (placer == null) placer = origin.AddComponent<ARShapePlacer>();
        var placerSo = new SerializedObject(placer);
        placerSo.FindProperty("arCamera").objectReferenceValue = Object.FindFirstObjectByType<Camera>();
        placerSo.ApplyModifiedPropertiesWithoutUndo();

        // ---- EventSystem (needed for the UI buttons, new Input System)
        foreach (var es in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            Object.DestroyImmediate(es.gameObject);
        var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        esGo.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        // ---- Menu canvas (transparent background)
        var oldCanvas = GameObject.Find("Menu Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        var canvasGo = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var panelGo = new GameObject("Menu Panel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelRt = (RectTransform)panelGo.transform;
        panelRt.anchorMin = new Vector2(0f, 0f);
        panelRt.anchorMax = new Vector2(1f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0f, 40f);
        panelRt.sizeDelta = new Vector2(0f, 220f);
        var layout = panelGo.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 0, 0);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        // No Image on the panel: its background stays fully transparent.

        var shapes = new List<ShapeInfo>
        {
            new ShapeInfo("Torus", new Color(0.62f, 0.22f, 0.86f)),
            new ShapeInfo("Cone", new Color(1f, 0.62f, 0.1f)),
            new ShapeInfo("Triangular Prism", new Color(0.15f, 0.35f, 0.95f)),
            new ShapeInfo("Dodecahedron", new Color(0.5f, 0.85f, 0.1f)),
        };

        Sprite roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var menu = canvasGo.AddComponent<ARShapeMenu>();
        var menuSo = new SerializedObject(menu);
        menuSo.FindProperty("placer").objectReferenceValue = placer;
        var items = menuSo.FindProperty("items");
        items.arraySize = shapes.Count;

        for (int i = 0; i < shapes.Count; i++)
        {
            var info = shapes[i];

            var btnGo = new GameObject(info.name + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panelGo.transform, false);
            var img = btnGo.GetComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.2f);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            var swatchGo = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            swatchGo.transform.SetParent(btnGo.transform, false);
            var swRt = (RectTransform)swatchGo.transform;
            swRt.anchorMin = swRt.anchorMax = new Vector2(0.5f, 1f);
            swRt.pivot = new Vector2(0.5f, 1f);
            swRt.anchoredPosition = new Vector2(0f, -22f);
            swRt.sizeDelta = new Vector2(96f, 96f);
            var swImg = swatchGo.GetComponent<Image>();
            swImg.sprite = circleSprite;
            swImg.color = info.color;
            swImg.raycastTarget = false;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(btnGo.transform, false);
            var lbRt = (RectTransform)labelGo.transform;
            lbRt.anchorMin = new Vector2(0f, 0f);
            lbRt.anchorMax = new Vector2(1f, 0f);
            lbRt.pivot = new Vector2(0.5f, 0f);
            lbRt.anchoredPosition = new Vector2(0f, 12f);
            lbRt.sizeDelta = new Vector2(-12f, 64f);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = info.name;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18f;
            tmp.fontSizeMax = 32f;
            tmp.raycastTarget = false;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ARShapeAssetBuilder.PrefabPath(info.name));
            var item = items.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("label").stringValue = info.name;
            item.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            item.FindPropertyRelative("button").objectReferenceValue = btn;
        }
        menuSo.ApplyModifiedPropertiesWithoutUndo();

        // ---- HUD: use the new placer and make its panel translucent
        var hud = Object.FindFirstObjectByType<ARDetectionHUD>();
        if (hud != null)
        {
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("placer").objectReferenceValue = placer;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            var panelImg = hud.GetComponentInChildren<Image>();
            if (panelImg != null)
            {
                Color c = panelImg.color;
                c.a = 0.2f;
                panelImg.color = c;
            }
        }

        // ---- Build settings: new scene first
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        list.RemoveAll(s => s.path == TargetScene);
        list.Insert(0, new EditorBuildSettingsScene(TargetScene, true));
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("ARShapeSceneSetup: ARShapeScene ready (menu, placer, drag, EventSystem).");
    }
}

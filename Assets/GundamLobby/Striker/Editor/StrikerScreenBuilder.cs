using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

namespace GundamLobby.Editor
{
    public static class StrikerScreenBuilder
    {
        const string Root = StrikerModelBaker.Root;
        static TMP_FontAsset font;
        static StrikerSelectionController controller;
        static Color bg = Hex("071117"), surface = Hex("0B1B25"), panel = Hex("102A37"), line = Hex("214654"), cyan = Hex("20C8F6"), white = Hex("E7F0F4"), muted = Hex("829EAC");
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h); return rect;
        }
        static LobbyShape Shape(string name, Transform parent, float x, float y, float w, float h, Color fill, Color border, LobbyShape.Geometry geometry = LobbyShape.Geometry.Box)
        {
            var rect = Rect(name, parent, x, y, w, h); var shape = rect.gameObject.AddComponent<LobbyShape>(); shape.geometry = geometry; shape.color = fill; shape.border = border; shape.thickness = border.a > 0 ? 1 : 0; shape.raycastTarget = false; return shape;
        }
        static TMP_Text Label(string name, string value, Transform parent, float x, float y, float w, float h, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var rect = Rect(name, parent, x, y, w, h); var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = size; text.text = value; text.color = color; text.alignment = align; text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow; text.raycastTarget = false; text.margin = Vector4.zero; return text;
        }
        static Button Button(string name, string label, Transform parent, float x, float y, float w, float h, UnityAction click, bool primary = false)
        {
            var shape = Shape(name, parent, x, y, w, h, primary ? Hex("087CAB") : bg, primary ? cyan : line, primary ? LobbyShape.Geometry.Tab : LobbyShape.Geometry.Box); shape.raycastTarget = true;
            var button = shape.gameObject.AddComponent<Button>(); button.targetGraphic = shape; button.transition = Selectable.Transition.None;
            var hover = shape.gameObject.AddComponent<LobbyHover>(); hover.surface = shape; hover.normal = shape.color; hover.hover = primary ? Hex("1098C4") : Hex("123948"); hover.pressed = Hex("175468");
            if (click != null) UnityEventTools.AddPersistentListener(button.onClick, click);
            Label("Label", label, shape.transform, 10, 0, w - 20, h, primary ? 25 : 19, white, TextAlignmentOptions.Center); return button;
        }

        [MenuItem("Tools/Gundam Lobby/Install Switch Break Striker")]
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before installing Striker selection");
            var scene = SceneManager.GetActiveScene(); if (scene.path != "Assets/GundamLobby/Scenes/HomeLobby.unity") throw new InvalidOperationException("Open HomeLobby first");
            if (UnityEngine.Object.FindFirstObjectByType<StrikerSelectionController>()) throw new InvalidOperationException("Striker screen already installed");
            Directory.CreateDirectory(Root + "/Prefabs");
            EditorSceneManager.SaveScene(scene, "Assets/GundamLobby/Backups/HomeLobby_BeforeStriker_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GundamLobby/Inventory/Fonts/Inventory Barlow Condensed SDF.asset"); if (!font) throw new InvalidOperationException("Existing inventory font missing");
            var models = StrikerModelBaker.BakeAll();
            var catalog = ScriptableObject.CreateInstance<StrikerCatalog>(); catalog.units = Definitions(models);
            for (int i = 0; i < catalog.units.Length; i++) catalog.units[i].portrait = StrikerModelBaker.Portrait(models[i], catalog.units[i].id);
            AssetDatabase.CreateAsset(catalog, Root + "/StrikerCatalog.asset");
            var system = new GameObject("Striker Selection Systems"); Undo.RegisterCreatedObjectUndo(system, "Install Striker selection"); controller = system.AddComponent<StrikerSelectionController>();
            controller.catalog = catalog; controller.lobby = UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>(); controller.inventory = UnityEngine.Object.FindFirstObjectByType<InventoryPanel>(); controller.lobbyCanvas = GameObject.Find("Home Lobby Canvas"); controller.lobbyCamera = GameObject.Find("Lobby Camera").GetComponent<Camera>();
            controller.previewMount = new GameObject("Striker Preview Mount"); controller.previewMount.transform.position = controller.lobby.modelPivot.position;
            var subtitle = controller.lobby.nameLabel.transform.parent.Find("Striker class");
            if (subtitle) { controller.lobby.presentationClassLabel = subtitle.GetComponent<TMP_Text>(); var rt = controller.lobby.presentationClassLabel.rectTransform; rt.anchoredPosition = new Vector2(31,-269); rt.sizeDelta = new Vector2(430,24); controller.lobby.presentationClassLabel.fontSize=19; controller.lobby.role=catalog.units[1].role; }
            controller.lobby.nameLabel.rectTransform.sizeDelta=new Vector2(430,64); controller.lobby.nameLabel.enableAutoSizing=true; controller.lobby.nameLabel.fontSizeMax=47; controller.lobby.nameLabel.fontSizeMin=30;
            var abilities=controller.lobby.nameLabel.transform.parent.Find("Abilities"); if(abilities)abilities.GetComponent<RectTransform>().anchoredPosition+=new Vector2(0,-10);
            controller.lobby.ApplyPresentation();
            var canvas = new GameObject("Switch Break Striker Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); Undo.RegisterCreatedObjectUndo(canvas, "Create Striker UI"); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; canvas.GetComponent<Canvas>().sortingOrder = 50;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f; controller.selectionCanvas = canvas;
            var root = Rect("Switch Break Striker Screen", canvas.transform, 0, 0, 1920, 1080); root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, .5f); root.anchoredPosition = Vector2.zero;
            var left = Shape("Roster readability shade", root, 0, 60, 690, 963, new Color(bg.r, bg.g, bg.b, .98f), Color.clear); left.gradient = true; left.gradientEnd = new Color(bg.r, bg.g, bg.b, 0);
            Shape("Specification shade", root, 1480, 60, 440, 963, new Color(bg.r, bg.g, bg.b, .94f), Color.clear);
            var dragZone = Rect("3D Preview - Drag and Zoom", root, 490, 65, 982, 948); var image = dragZone.gameObject.AddComponent<Image>(); image.color = Color.clear; var drag = dragZone.gameObject.AddComponent<StrikerPreviewDrag>(); drag.owner = controller;
            Header(root); Roster(root); Details(root); Footer(root); Toast(root);
            var switchButton = controller.lobbyCanvas.GetComponentsInChildren<Button>(true).First(b => b.name == "Switch Break Striker"); var old = switchButton.GetComponent<LobbyButtonAction>(); if (old) Undo.DestroyObjectImmediate(old); Undo.RecordObject(switchButton, "Connect Striker selection"); UnityEventTools.AddPersistentListener(switchButton.onClick, controller.Open); EditorUtility.SetDirty(switchButton);
            controller.selectionCanvas.SetActive(false); controller.previewMount.SetActive(false);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            File.WriteAllText(Root + "/README_VI.md", "# SWITCH BREAK STRIKER\n\nMở HomeLobby, Play, bấm SWITCH BREAK STRIKER. Bấm thẻ để xem model thật, kéo vùng giữa để xoay và cuộn để zoom. Enter hoặc CONFIRM STRIKER xác nhận; Esc/RETURN hủy xem và quay lại. Lựa chọn được lưu cục bộ bằng PlayerPrefs (GundamLobby.ActiveStriker), áp dụng cho robot ở lobby và preview inventory.\n\nCatalog: Assets/GundamLobby/Striker/StrikerCatalog.asset — thay model, portrait, tên, vai trò, cấp và chỉ số trong Inspector. 10 model được chuẩn hóa chiều cao tối đa 8.66 và ngang tối đa 8.8 để vừa hangar. Prefab roster và stat row có thể sửa riêng. Canvas uGUI/TMP 1920x1080; không dùng ảnh toàn giao diện.\n\nGiá trị và vai trò là dữ liệu mẫu cho game theo lượt. 5 GLB được đưa sang prefab hình học ở tư thế animation; chưa chuyển toàn bộ animation chiến đấu. SkyDefender, BlueFlame, HaoLing được gom mesh theo material để giảm draw calls. Model nguồn giữ nguyên ở D:/AR_Transformer_Gundam-main/Assets/Models. Không đổi render pipeline và giữ Mobile URP Soft Shadows để tránh lỗi đốm trắng Intel.\n");
            return "Installed Switch Break Striker: 10 real 3D models, selection, rotation/zoom, confirmation, saved local choice, inventory sync.";
        }

        static void Header(Transform root)
        {
            Shape("Top bar", root, 0, 0, 1920, 60, bg, Color.clear); Shape("Title plate", root, 0, 0, 464, 59, panel, line, LobbyShape.Geometry.Panel); Shape("Header cyan rule", root, 0, 59, 1920, 1, line, Color.clear);
            Label("Title", "SWITCH BREAK STRIKER", root, 38, 0, 420, 59, 28, white);
            string[] values = { "1,960", "17,020", "0", "925" }; Color[] colors = { Hex("3E9E79"), Hex("C9A846"), Hex("C9A846"), cyan };
            for (int i = 0; i < values.Length; i++) { float x = 1340 + i * 140; Shape("Resource icon " + i, root, x, 19, 22, 22, colors[i] * .6f, colors[i], LobbyShape.Geometry.Hexagon); Label("Resource amount " + i, values[i], root, x + 30, 0, 106, 60, 24, white); }
        }
        static void Roster(Transform root)
        {
            Label("Roster title", "STRIKER ROSTER", root, 24, 85, 280, 26, 20, muted); controller.counterLabel = Label("Roster counter", "02 / 10", root, 366, 85, 100, 26, 20, cyan, TextAlignmentOptions.Right);
            var area = Rect("Roster Scroll", root, 24, 126, 442, 852); var scroll = area.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 40;
            var viewport = Rect("Viewport", area, 0, 0, 442, 852); viewport.gameObject.AddComponent<RectMask2D>(); var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var content = Rect("Roster cards", viewport, 0, 0, 442, 852); var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 8; layout.childControlHeight = false; layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true; var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; scroll.viewport = viewport; scroll.content = content;
            var shape = Shape("StrikerRosterCard", null, 0, 0, 442, 78, surface, line); shape.raycastTarget = true; shape.gameObject.AddComponent<RectMask2D>(); var button = shape.gameObject.AddComponent<Button>(); button.targetGraphic = shape; button.transition = Selectable.Transition.None; var hover = shape.gameObject.AddComponent<LobbyHover>(); hover.surface = shape; hover.normal = surface; hover.hover = Hex("123948"); hover.pressed = Hex("175468");
            var card = shape.gameObject.AddComponent<StrikerRosterCard>(); var picture = Rect("Model portrait", shape.transform, 182, 0, 260, 78).gameObject.AddComponent<Image>(); picture.raycastTarget = false; picture.preserveAspect = false; card.portrait = picture;
            var shade = Shape("Text legibility", shape.transform, 0, 0, 340, 78, surface, Color.clear); shade.gradient = true; shade.gradientEnd = new Color(surface.r, surface.g, surface.b, 0);
            var selected = Shape("Selected border", shape.transform, 1, 1, 440, 76, new Color(cyan.r, cyan.g, cyan.b, .06f), cyan); selected.thickness = 2; card.selectedBorder = selected.gameObject;
            card.titleLabel = Label("Unit name", "UNICORN", shape.transform, 17, 9, 340, 33, 25, white); card.roleLabel = Label("Role", "BALANCED ATTACKER", shape.transform, 17, 45, 295, 24, 17, muted);
            var check = Shape("Selection check", shape.transform, 406, 10, 22, 22, Color.clear, cyan, LobbyShape.Geometry.Hexagon);
            Shape("Check short stroke", check.transform, 4, 9, 6, 2, cyan, Color.clear).rectTransform.localEulerAngles = new Vector3(0,0,-45);
            Shape("Check long stroke", check.transform, 7, 14, 14, 2, cyan, Color.clear).rectTransform.localEulerAngles = new Vector3(0,0,45); card.selectedCheck = check.gameObject;
            card.activeLabel = Label("Equipped", "ACTIVE", shape.transform, 357, 50, 73, 20, 15, cyan, TextAlignmentOptions.Right); shape.gameObject.AddComponent<LayoutElement>().preferredHeight = 78;
            var prefab = PrefabUtility.SaveAsPrefabAsset(shape.gameObject, Root + "/Prefabs/StrikerRosterCard.prefab"); UnityEngine.Object.DestroyImmediate(shape.gameObject);
            controller.cards = new StrikerRosterCard[controller.catalog.units.Length];
            for (int i = 0; i < controller.cards.Length; i++) { var c = (GameObject)PrefabUtility.InstantiatePrefab(prefab, content); c.name = "Choose " + controller.catalog.units[i].id; var view = c.GetComponent<StrikerRosterCard>(); view.owner = controller; view.index = i; view.Bind(controller.catalog.units[i], i == 1, i == 1); controller.cards[i] = view; }
            Shape("Roster edge", root, 480, 83, 1, 919, line, Color.clear);
        }
        static void Details(Transform root)
        {
            controller.serialLabel = Label("Unit serial", "RX-0 / PSYCHO FRAME", root, 520, 90, 800, 25, 19, cyan);
            controller.nameLabel = Label("Selected unit name", "UNICORN", root, 520, 122, 920, 58, 47, white);
            Label("Specification title", "UNIT SPECIFICATION", root, 1520, 93, 365, 25, 20, muted);
            controller.roleLabel = Label("Role and level", "BALANCED ATTACKER  /  Lv. 20", root, 1520, 144, 365, 42, 21, cyan);
            string[] headings = { "CORE STATS", "OFFENSE", "SURVIVABILITY", "TURN ECONOMY" };
            string[] names = { "Max HP", "Attack", "Defense", "Speed", "Crit Rate", "Crit DMG", "Break Effect", "Damage Type", "Effect Resistance", "Energy Regeneration", "Aggro", "Shield Capacity", "Max Energy", "Skill Cost", "Basic Attack Gain", "Ultimate Target" };
            var row = Rect("StrikerStatRow", null, 0, 0, 362, 29); Label("Label", "Max HP", row, 0, 0, 226, 28, 20, muted); Label("Value", "18,240", row, 228, 0, 134, 28, 22, white, TextAlignmentOptions.Right); Shape("Rule", row, 0, 28, 362, 1, line, Color.clear);
            var prefab = PrefabUtility.SaveAsPrefabAsset(row.gameObject, Root + "/Prefabs/StrikerStatRow.prefab"); UnityEngine.Object.DestroyImmediate(row.gameObject); controller.statValues = new TMP_Text[16];
            for (int g = 0; g < 4; g++)
            {
                float y = 222 + g * 164; Label("Heading " + headings[g], headings[g], root, 1520, y, 362, 28, 19, cyan, TextAlignmentOptions.Center);
                for (int j = 0; j < 4; j++) { int index = g * 4 + j; var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root); go.name = "Stat " + names[index]; var r = go.GetComponent<RectTransform>(); r.anchoredPosition = new Vector2(1520, -(y + 34 + j * 29)); go.transform.Find("Label").GetComponent<TMP_Text>().text = names[index]; controller.statValues[index] = go.transform.Find("Value").GetComponent<TMP_Text>(); }
            }
            Shape("Power panel", root, 1520, 912, 362, 52, panel, line); Label("Power label", "COMBAT POWER", root, 1536, 912, 205, 52, 21, muted); controller.powerLabel = Label("Power", "18,460", root, 1740, 912, 126, 52, 30, cyan, TextAlignmentOptions.Right);
            controller.statusLabel = Label("Selection status", "ACTIVE STRIKER", root, 520, 978, 700, 32, 22, cyan);
            controller.confirmButton = Button("Confirm Striker", "CONFIRM STRIKER", root, 1520, 974, 362, 48, controller.ConfirmSelection, true);
        }
        static void Footer(Transform root)
        {
            Shape("Footer", root, 0, 1027, 1920, 53, new Color(bg.r, bg.g, bg.b, .95f), Color.clear); Shape("Footer rule", root, 0, 1027, 1920, 1, line, Color.clear);
            Button("Striker Comms", "COMMS", root, 24, 1036, 126, 34, controller.Notify); Label("Chat hint", "Enter    START CHAT", root, 170, 1036, 250, 34, 18, muted);
            Label("Preview controls", "DRAG  ROTATE     SCROLL  ZOOM     ↑ ↓  SELECT     ENTER  CONFIRM", root, 650, 1036, 940, 34, 18, muted, TextAlignmentOptions.Center); Button("Striker Return", "Esc   RETURN", root, 1740, 1036, 142, 34, controller.Close);
        }
        static void Toast(Transform root)
        {
            var box = Shape("Striker Toast", root, 620, 240, 700, 48, panel, cyan); controller.toast = box.gameObject.AddComponent<CanvasGroup>(); controller.toast.alpha = 0; controller.toast.blocksRaycasts = false; controller.toastLabel = Label("Message", "", box.transform, 12, 0, 676, 48, 23, white, TextAlignmentOptions.Center);
        }
        static StrikerDefinition[] Definitions(GameObject[] models)
        {
            string[] id = { "SkyDefender", "Unicorn", "Justice", "BlueFlame", "Extreme", "HaoLing", "Kamiki", "StrikeRouge", "RX78", "RX78Saber" };
            string[] names = { "SKY DEFENDER", "UNICORN", "JUSTICE GUNDAM", "BLUE FLAME", "EXTREME GUNDAM", "HAO LING", "KAMIKI BURNING", "STRIKE ROUGE", "RX-78-2 GUNDAM", "RX-78-2 / SABER" };
            string[] roles = { "HEAVY ASSAULT", "BALANCED ATTACKER", "MOBILE STRIKER", "ENERGY ATTACKER", "BURST ATTACKER", "TACTICAL SUPPORT", "MELEE BRAWLER", "RANGED ASSAULT", "RIFLE SPECIALIST", "CLOSE COMBAT" };
            string[] codes = { "SKY / DEFENDER", "RX-0 / PSYCHO FRAME", "ZGMF-X09A / JUSTICE", "BF / BLUE FLAME", "EXTREME / APPLICATION", "EMP02 / HAO LING", "KAMIKI / BURNING GUNDAM", "MBF-02 / OOTORI", "RX-78-2 / RIFLE", "RX-78-2 / BEAM SABER" };
            int[,] values = { {21600,2760,1380,104,19820},{18240,2480,1220,112,18460},{17800,2540,1110,118,18720},{16900,2880,1040,116,19140},{18600,2940,1180,108,20120},{19400,2210,1320,114,18980},{18900,2820,1160,120,19660},{17600,2620,1090,115,18840},{17200,2360,1260,110,17980},{17600,2500,1180,114,18220} };
            return Enumerable.Range(0, 10).Select(i => new StrikerDefinition { id = id[i], displayName = names[i], code = codes[i], role = roles[i], prefab = models[i], hp = values[i,0], attack = values[i,1], defense = values[i,2], speed = values[i,3], power = values[i,4], damageType = i == 6 || i == 9 ? "MELEE" : "BEAM" }).ToArray();
        }
    }
}

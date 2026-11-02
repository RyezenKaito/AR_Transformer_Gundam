using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GundamLobby.Editor
{
    [InitializeOnLoad]
    public static class LobbyCanvasRepair
    {
        const string Root = "Assets/GundamLobby";
        const string Request = Root + "/RepairCanvas.request";
        static double nextCheck;
        static bool repairedInPlay;
        static LobbyCanvasRepair() { EditorApplication.update += Check; }
        static void Check()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 2;
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (EditorApplication.isPlaying)
            {
                if (repairedInPlay) return;
                try { RepairLoaded(); Validate(true); repairedInPlay = true; }
                catch (Exception e) { File.WriteAllText(Root + "/RepairStatus.txt", "ERROR " + e); Debug.LogException(e); }
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            try { RepairAssets(); File.Delete(Request); EditorApplication.update -= Check; }
            catch (Exception e)
            {
                File.WriteAllText(Root + "/RepairStatus.txt", "ERROR " + e);
                Debug.LogException(e); File.Delete(Request); EditorApplication.update -= Check;
            }
        }
        static int Fix(GameObject root)
        {
            int count = 0;
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.GetComponent<CanvasRenderer>()) continue;
                graphic.gameObject.AddComponent<CanvasRenderer>();
                EditorUtility.SetDirty(graphic.gameObject);
                graphic.SetAllDirty(); count++;
            }
            return count;
        }
        static int RepairLoaded()
        {
            int count = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || scene.path != Root + "/Scenes/HomeLobby.unity") continue;
                foreach (var root in scene.GetRootGameObjects()) count += Fix(root);
            }
            Canvas.ForceUpdateCanvases();
            return count;
        }
        [MenuItem("Tools/Gundam Lobby/Repair Missing CanvasRenderers")]
        public static void RepairAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { RepairLoaded(); Validate(true); return; }
            Directory.CreateDirectory(Root + "/Backups");
            string backupRoot = Root + "/Backups/CanvasRepair_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Directory.CreateDirectory(backupRoot);
            int prefabCount = 0, fixedCount = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Root + "/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                File.Copy(path, backupRoot + "/" + Path.GetFileName(path) + ".bak");
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int count = Fix(root);
                    if (count > 0) { PrefabUtility.SaveAsPrefabAsset(root, path); fixedCount += count; prefabCount++; }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string scenePath = Root + "/Scenes/HomeLobby.unity";
            var scene = SceneManager.GetSceneByPath(scenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, backupRoot + "/HomeLobby_before_repair.unity", true);
            foreach (var root in scene.GetRootGameObjects()) fixedCount += Fix(root);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Canvas.ForceUpdateCanvases();
            string validation = Validate(false);
            if (opened) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            string report = "SUCCESS " + DateTime.Now + "\nAdded CanvasRenderers: " + fixedCount + "\nRepaired prefabs: " + prefabCount + "\n" + validation;
            File.WriteAllText(Root + "/RepairStatus.txt", report);
            Debug.Log(report);
        }
        static string Validate(bool play)
        {
            var graphics = UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(g => g.gameObject.scene.path == Root + "/Scenes/HomeLobby.unity").ToArray();
            var missing = graphics.Where(g => !g.GetComponent<CanvasRenderer>()).Select(g => g.name).ToArray();
            if (missing.Length > 0) throw new Exception("Still missing CanvasRenderer: " + string.Join(", ", missing));
            int raycasts = 0;
            var system = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (system)
            {
                foreach (var button in graphics.Select(g => g.GetComponent<Button>()).Where(b => b && b.isActiveAndEnabled))
                {
                    var data = new PointerEventData(system);
                    data.position = RectTransformUtility.WorldToScreenPoint(null, button.GetComponent<RectTransform>().TransformPoint(button.GetComponent<RectTransform>().rect.center));
                    var hits = new System.Collections.Generic.List<RaycastResult>();
                    system.RaycastAll(data, hits); raycasts++;
                }
            }
            if (play)
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<HomeLobbyController>();
                var settings = graphics.Select(g => g.GetComponent<Button>()).FirstOrDefault(b => b && b.name == "Settings");
                if (controller && settings)
                {
                    int before = controller.feedbackCount; settings.onClick.Invoke();
                    if (controller.feedbackCount != before + 1) throw new Exception("Settings feedback did not execute.");
                }
                File.WriteAllText(Root + "/RepairStatus.txt", "PLAY MODE REPAIRED\nValidated graphics: " + graphics.Length + "\nRaycasts without exception: " + raycasts + "\nExit Play Mode to persist scene/prefab repairs.");
            }
            return "Validated graphics: " + graphics.Length + "\nMissing CanvasRenderer: 0\nRaycasts without exception: " + raycasts;
        }
    }
}

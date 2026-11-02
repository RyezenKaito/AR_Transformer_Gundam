using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ProjectEnvironments;

namespace ProjectEnvironments.Editor
{
    /// <summary>Read-only checks for a project relocated to another drive.</summary>
    [InitializeOnLoad]
    public static class ProjectMoveValidation
    {
        const string Pending = "Temp/ProjectMoveValidation.pending";
        const string Report = "Logs/ProjectMoveValidation.json";
        const string Readme = "Assets/TheTalesFactory/Photoscanned MoutainsRocks PBR/_ReadMe & Publisher Window/Inspector Info/PublisherReadme.asset";

        [Serializable] sealed class Result
        {
            public string projectRoot, unityVersion, checkedAtUtc;
            public bool publisherReadmeLoaded, catalogLoaded, allSceneAssetsLoaded;
            public int readmeSections, environmentCount;
            public string[] missingScenes;
        }

        static ProjectMoveValidation()
        {
            // Only the explicit migration request schedules a check after compilation.
            if (File.Exists(Pending)) EditorApplication.delayCall += RunWhenReady;
        }

        static void RunWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunWhenReady;
                return;
            }
            if (!File.Exists(Pending)) return;
            Validate();
            File.Delete(Pending);
        }

        [MenuItem("Tools/Environment System/Validate Project After Move")]
        public static void Validate()
        {
            var report = new Result
            {
                projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                unityVersion = Application.unityVersion,
                checkedAtUtc = DateTime.UtcNow.ToString("O")
            };
            var readme = AssetDatabase.LoadAssetAtPath<ScriptableObject>(Readme);
            report.publisherReadmeLoaded = readme;
            if (readme)
            {
                var sections = new SerializedObject(readme).FindProperty("sections");
                report.readmeSections = sections != null && sections.isArray ? sections.arraySize : 0;
            }
            var catalog = AssetDatabase.LoadAssetAtPath<EnvironmentCatalog>(EnvironmentTools.CatalogPath);
            report.catalogLoaded = catalog;
            var missing = new System.Collections.Generic.List<string>();
            if (catalog)
            {
                report.environmentCount = catalog.environments.Length;
                foreach (var definition in catalog.environments)
                {
                    if (!definition) { missing.Add("Null environment definition"); continue; }
                    if (definition.available && !AssetDatabase.LoadAssetAtPath<SceneAsset>(definition.ScenePath))
                        missing.Add(definition.id + ": " + definition.ScenePath);
                }
                if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(catalog.hubScenePath)) missing.Add(catalog.hubScenePath);
            }
            report.missingScenes = missing.ToArray();
            report.allSceneAssetsLoaded = catalog && missing.Count == 0;
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, JsonUtility.ToJson(report, true));
            bool pass = report.publisherReadmeLoaded && report.readmeSections == 6 && report.allSceneAssetsLoaded;
            if (pass) Debug.Log("PROJECT_MOVE_VALIDATION_PASS: publisher readme and " + report.environmentCount + " environment definitions loaded from " + report.projectRoot);
            else Debug.LogWarning("PROJECT_MOVE_VALIDATION: inspect " + Report);
            // An explicit one-shot recovery request can reopen the Hub after relocation.
            // Never replace a scene the user has edited or an existing battle scene.
            const string openHubRequest = "Temp/ProjectMoveValidation.openhub";
            var current = SceneManager.GetActiveScene();
            bool emptyPreview = string.IsNullOrEmpty(current.path) || current.name == "Untitled" ||
                                current.path.StartsWith("Temp/__Backupscenes/", StringComparison.OrdinalIgnoreCase);
            if (pass && File.Exists(openHubRequest) && !current.isDirty &&
                (emptyPreview || current.path == EnvironmentTools.Hub))
            {
                if (EditorApplication.isPlaying) SceneManager.LoadScene(EnvironmentTools.Hub);
                else EditorSceneManager.OpenScene(EnvironmentTools.Hub);
                File.Delete(openHubRequest);
                if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
                Debug.Log("PROJECT_MOVE_HUB_RESTORED: " + EnvironmentTools.Hub);
            }
        }
    }
}

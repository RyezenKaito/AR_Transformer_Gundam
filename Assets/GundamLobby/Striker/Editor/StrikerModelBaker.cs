using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GundamLobby.Editor
{
    public static class StrikerModelBaker
    {
        public const string Root = "Assets/GundamLobby/Striker";
        [Serializable] class PoseMaterial { public float[] color, emission; public string texture; }
        [Serializable] class PoseDescription { public string source; public PoseMaterial[] parts; }
        static Material fallback;

        public static GameObject[] BakeAll()
        {
            foreach (string dir in new[] { "Prefabs", "Meshes", "Materials", "Portraits" }) Directory.CreateDirectory(Root + "/" + dir);
            var result = new GameObject[10];
            result[0] = BakePrefab("SkyDefender", Root + "/SourceModels/SkyDefender/Prefabs/SkyDefender_Detailed.prefab");
            result[1] = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GundamLobby/Prefabs/UnicornGundam_Lobby.prefab");
            result[2] = BakePose("Justice");
            result[3] = BakePrefab("BlueFlame", Root + "/SourceModels/BlueFlame/Prefabs/BlueFlame_Detailed.prefab");
            result[4] = BakePose("Extreme");
            result[5] = BakePrefab("HaoLing", Root + "/SourceModels/HaoLing/Prefabs/EMP02_HaoLing_Detailed.prefab");
            var importer = AssetImporter.GetAtPath("Assets/GundamLobby/FigmaArtSource/Kamiki/Kamiki.fbx") as ModelImporter;
            if (importer && !importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            result[6] = BakePrefab("Kamiki", "Assets/GundamLobby/FigmaArtSource/Kamiki/Kamiki.fbx", 180, "Assets/GundamLobby/FigmaArtSource/Kamiki/Image_0.png");
            result[7] = BakePose("StrikeRouge");
            result[8] = BakePose("RX78");
            result[9] = BakePose("RX78Saber");
            if (result.Any(p => !p)) throw new InvalidOperationException("A Striker model failed to load");
            AssetDatabase.SaveAssets();
            return result;
        }

        static GameObject BakePose(string id)
        {
            string target = Root + "/Prefabs/" + id + ".prefab";
            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(target); if (exists) return exists;
            var data = JsonUtility.FromJson<PoseDescription>(File.ReadAllText(Root + "/PoseSources/" + id + ".json"));
            var model = new GameObject(id); var geometry = new GameObject("Original model geometry"); geometry.transform.SetParent(model.transform, false);
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(Root + "/PoseSources/" + id + ".bytes")))
                {
                    if (new string(reader.ReadChars(4)) != "STRK") throw new InvalidDataException(id);
                    int count = reader.ReadInt32();
                    for (int p = 0; p < count; p++)
                    {
                        int vc = reader.ReadInt32(), ic = reader.ReadInt32();
                        var verts = new Vector3[vc]; var normals = new Vector3[vc]; var uv = new Vector2[vc]; var indices = new int[ic];
                        for (int i = 0; i < vc; i++) verts[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        for (int i = 0; i < vc; i++) normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        for (int i = 0; i < vc; i++) uv[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        for (int i = 0; i < ic; i++) indices[i] = reader.ReadInt32();
                        var mesh = new Mesh { name = id + " source part " + p, indexFormat = IndexFormat.UInt32 };
                        mesh.vertices = verts; mesh.normals = normals; mesh.uv = uv; mesh.triangles = indices; mesh.RecalculateBounds();
                        AssetDatabase.CreateAsset(mesh, Root + "/Meshes/" + id + "_" + p + ".asset");
                        var matData = data.parts[p]; var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = id + " material " + p };
                        if (matData.color != null && matData.color.Length >= 4) material.SetColor("_BaseColor", new Color(matData.color[0], matData.color[1], matData.color[2], matData.color[3]));
                        if (!string.IsNullOrEmpty(matData.texture)) material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/PoseSources/" + matData.texture));
                        material.SetFloat("_Smoothness", .32f); material.SetFloat("_Metallic", .12f);
                        if (matData.emission != null && matData.emission.Any(v => v > .01f)) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(matData.emission[0], matData.emission[1], matData.emission[2])); }
                        AssetDatabase.CreateAsset(material, Root + "/Materials/" + id + "_" + p + ".mat");
                        AddPart(geometry.transform, mesh, material);
                    }
                }
                Normalize(geometry);
                return PrefabUtility.SaveAsPrefabAsset(model, target);
            }
            finally { UnityEngine.Object.DestroyImmediate(model); }
        }

        static GameObject BakePrefab(string id, string sourcePath, float yaw = 0, string texturePath = null)
        {
            string target = Root + "/Prefabs/" + id + ".prefab";
            var exists = AssetDatabase.LoadAssetAtPath<GameObject>(target); if (exists) return exists;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath); if (!source) throw new InvalidOperationException("Missing source " + sourcePath);
            var instance = UnityEngine.Object.Instantiate(source); instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, yaw, 0));
            var model = new GameObject(id); var geometry = new GameObject("Original model geometry"); geometry.transform.SetParent(model.transform, false);
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var temporaryMeshes = new List<Mesh>();
            if (!fallback) fallback = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Material textureOverride = null;
            if (texturePath != null) { textureOverride = new Material(fallback); textureOverride.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath)); textureOverride.SetFloat("_Smoothness", .28f); }
            try
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    if (!renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer is ParticleSystemRenderer) continue;
                    Mesh mesh = null;
                    if (renderer is SkinnedMeshRenderer skinned) { mesh = new Mesh(); skinned.BakeMesh(mesh); temporaryMeshes.Add(mesh); }
                    else { var filter = renderer.GetComponent<MeshFilter>(); if (filter) mesh = filter.sharedMesh; }
                    if (!mesh) continue;
                    var materials = renderer.sharedMaterials;
                    for (int s = 0; s < mesh.subMeshCount; s++)
                    {
                        var material = textureOverride ? textureOverride : s < materials.Length && materials[s] ? materials[s] : fallback;
                        if (!groups.ContainsKey(material)) groups[material] = new List<CombineInstance>();
                        groups[material].Add(new CombineInstance { mesh = mesh, subMeshIndex = s, transform = renderer.localToWorldMatrix });
                    }
                }
                int index = 0;
                foreach (var group in groups)
                {
                    var mesh = new Mesh { name = id + " combined " + index, indexFormat = IndexFormat.UInt32 };
                    mesh.CombineMeshes(group.Value.ToArray(), true, true); mesh.RecalculateBounds();
                    if (mesh.vertexCount == 0) throw new InvalidOperationException("Empty model mesh: " + id);
                    AssetDatabase.CreateAsset(mesh, Root + "/Meshes/" + id + "_" + index + ".asset");
                    var material = new Material(group.Key) { name = id + " / " + group.Key.name };
                    AssetDatabase.CreateAsset(material, Root + "/Materials/" + id + "_" + index + ".mat");
                    AddPart(geometry.transform, mesh, material); index++;
                }
                Normalize(geometry);
                return PrefabUtility.SaveAsPrefabAsset(model, target);
            }
            finally
            {
                foreach (var mesh in temporaryMeshes) UnityEngine.Object.DestroyImmediate(mesh);
                if (textureOverride) UnityEngine.Object.DestroyImmediate(textureOverride);
                UnityEngine.Object.DestroyImmediate(instance); UnityEngine.Object.DestroyImmediate(model);
            }
        }

        static void AddPart(Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
        static void Normalize(GameObject geometry)
        {
            var renderers = geometry.GetComponentsInChildren<Renderer>(); if (renderers.Length == 0) throw new InvalidOperationException("No model renderers");
            Bounds bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float scale = Mathf.Min(8.66f / Mathf.Max(.01f, bounds.size.y), 8.8f / Mathf.Max(.01f, bounds.size.x));
            geometry.transform.localScale = Vector3.one * scale;
            geometry.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * scale;
        }

        public static Sprite Portrait(GameObject prefab, string id, bool force = false)
        {
            string path = Root + "/Portraits/" + id + ".png";
            var found = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (found && !force) return found;
            var model = UnityEngine.Object.Instantiate(prefab); model.transform.position = new Vector3(4000, 0, 0); model.transform.rotation = Quaternion.Euler(0, -12, 0);
            foreach (var t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 29;
            var rs = model.GetComponentsInChildren<Renderer>(); Bounds bounds = rs[0].bounds; foreach (var r in rs) bounds.Encapsulate(r.bounds);
            var camGo = new GameObject("Temporary Striker Portrait Camera", typeof(Camera)); var cam = camGo.GetComponent<Camera>();
            Vector3 target = new Vector3(bounds.center.x, bounds.max.y - (id == "SkyDefender" ? 2.45f : 1.15f), bounds.center.z);
            cam.transform.position = target + new Vector3(.4f, .1f, -12); cam.transform.LookAt(target); cam.cullingMask = 1 << 29;
            cam.orthographic = true; cam.orthographicSize = 1.65f; cam.aspect = 600f / 190; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.clear;
            cam.allowHDR = false; cam.allowMSAA = false; cam.useOcclusionCulling = false; cam.stereoTargetEye = StereoTargetEyeMask.None; cam.farClipPlane = 40;
            var data = cam.GetUniversalAdditionalCameraData(); data.allowXRRendering = false; data.renderPostProcessing = false; data.requiresColorOption = CameraOverrideOption.Off; data.requiresDepthOption = CameraOverrideOption.Off;
            var rt = new RenderTexture(600, 190, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 }; var old = RenderTexture.active; var tex = new Texture2D(600, 190, TextureFormat.RGBA32, false);
            try
            {
                cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 600, 190), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG());
            }
            finally
            {
                cam.targetTexture = null; RenderTexture.active = old; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(model); UnityEngine.Object.DestroyImmediate(camGo);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}

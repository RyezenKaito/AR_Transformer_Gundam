using UnityEditor;
using UnityEngine;

/// <summary>Builds the mesh, material and prefab assets for the four AR shapes.</summary>
public static class ARShapeAssetBuilder
{
    public const string Root = "Assets/Prefabs/AR/Shapes";

    [MenuItem("AR/Build Shape Prefabs")]
    public static void Build()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/AR");
        EnsureFolder(Root);
        EnsureFolder(Root + "/Meshes");
        EnsureFolder(Root + "/Materials");

        BuildOne("Torus", ShapeMeshFactory.CreateTorus(), new Color(0.62f, 0.22f, 0.86f));
        BuildOne("Cone", ShapeMeshFactory.CreateCone(), new Color(1f, 0.62f, 0.1f));
        BuildOne("Triangular Prism", ShapeMeshFactory.CreatePrism(), new Color(0.15f, 0.35f, 0.95f));
        BuildOne("Dodecahedron", ShapeMeshFactory.CreateDodecahedron(), new Color(0.5f, 0.85f, 0.1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ARShapeAssetBuilder: 4 shape prefabs built in " + Root);
    }

    public static string PrefabPath(string shapeName)
    {
        return Root + "/" + shapeName.Replace(" ", "_") + ".prefab";
    }

    static void BuildOne(string shapeName, Mesh mesh, Color color)
    {
        string file = shapeName.Replace(" ", "_");
        string meshPath = Root + "/Meshes/" + file + ".asset";
        string matPath = Root + "/Materials/" + file + ".mat";
        string prefabPath = PrefabPath(shapeName);

        AssetDatabase.DeleteAsset(meshPath);
        AssetDatabase.CreateAsset(mesh, meshPath);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0.45f);
        EditorUtility.SetDirty(mat);

        var go = new GameObject(shapeName);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        var col = go.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;
        col.convex = shapeName != "Torus"; // torus hull would exceed the 256 polygon limit

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int i = path.LastIndexOf("/");
        AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
    }
}

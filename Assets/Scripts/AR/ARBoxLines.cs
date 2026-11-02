using UnityEngine;

/// <summary>Helper that draws a wireframe box on a LineRenderer (16 points cover all 12 edges).</summary>
public static class ARBoxLines
{
    static readonly int[] k_Order = { 0, 1, 2, 3, 0, 4, 5, 1, 5, 6, 2, 6, 7, 3, 7, 4 };

    public static LineRenderer CreateLine(Transform parent, string name, Material material, Color color, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = false;
        line.positionCount = k_Order.Length;
        line.widthMultiplier = width;
        line.numCornerVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        if (material != null) line.sharedMaterial = material;
        line.startColor = color;
        line.endColor = color;
        return line;
    }

    public static void SetBox(LineRenderer line, Vector3 min, Vector3 max)
    {
        var c = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z)
        };
        for (int i = 0; i < k_Order.Length; i++)
            line.SetPosition(i, c[k_Order[i]]);
    }
}

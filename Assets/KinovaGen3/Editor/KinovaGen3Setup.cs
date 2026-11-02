using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Tools > Kinova Gen3 > Setup (b, c, d)
public static class KinovaGen3Setup
{
    public const float TargetYawDeg = 50f;   // nhấn mũi tên lên khoảng 5 lần là tới đối tượng

    [MenuItem("Tools/Kinova Gen3/Setup (b, c, d)")]
    public static void Setup()
    {
        var rig = Object.FindFirstObjectByType<KinovaGen3Rig>();
        if (rig == null)
        {
            Debug.LogError("[KinovaGen3] Không tìm thấy KinovaGen3Rig trong scene.");
            return;
        }

        Transform j1 = rig.joint1 != null ? rig.joint1 : FindDeep(rig.transform, "J1_BaseYaw_Y");
        Transform j7 = rig.joint7 != null ? rig.joint7 : FindDeep(rig.transform, "J7_ToolRoll_Y");
        if (j1 == null || j7 == null)
        {
            Debug.LogError("[KinovaGen3] Không tìm thấy J1_BaseYaw_Y / J7_ToolRoll_Y.");
            return;
        }
        rig.Apply();

        // b) điều khiển khớp base
        if (rig.GetComponent<KinovaGen3BaseController>() == null)
            Undo.AddComponent<KinovaGen3BaseController>(rig.gameObject);

        // c) + d) Head
        Transform head = j7.Find("Head");
        if (head == null)
        {
            var go = new GameObject("Head");
            Undo.RegisterCreatedObjectUndo(go, "Create Head");
            head = go.transform;
            head.SetParent(j7, false);
        }
        head.localRotation = Quaternion.identity;
        head.localPosition = new Vector3(0f, ComputeToolTipY(j7), 0f);

        if (head.GetComponent<KinovaGen3HeadPusher>() == null)
            Undo.AddComponent<KinovaGen3HeadPusher>(head.gameObject);
        if (head.GetComponent<KinovaGen3PointCloudEffect>() == null)
            Undo.AddComponent<KinovaGen3PointCloudEffect>(head.gameObject);

        // đối tượng để đẩy
        GameObject target = GameObject.Find("TargetObject");
        if (target == null)
        {
            target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "TargetObject";
            Undo.RegisterCreatedObjectUndo(target, "Create TargetObject");
            target.transform.localScale = Vector3.one * 0.05f;

            var rb = target.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.useGravity = false;   // lơ lửng cho tới khi bị đẩy
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = 1f;
            rb.angularDamping = 0.5f;
#else
            rb.drag = 1f;
            rb.angularDrag = 0.5f;
#endif
            var mat = GetOrCreateMaterial();
            if (mat != null) target.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // đặt đối tượng trên quỹ đạo tròn của đầu quanh trục J1
        Vector3 axis = rig.transform.up;
        Vector3 axisPoint = j1.position;
        Vector3 rel = head.position - axisPoint;
        target.transform.position =
            Quaternion.AngleAxis(TargetYawDeg - rig.j1_baseYaw, axis) * rel + axisPoint;

        EditorSceneManager.MarkSceneDirty(rig.gameObject.scene);
        Selection.activeGameObject = head.gameObject;
        Debug.Log("[KinovaGen3] Setup xong. Head local y = " + head.localPosition.y.ToString("F3") +
                  ", TargetObject tại " + target.transform.position.ToString("F3"));
    }

    static float ComputeToolTipY(Transform j7)
    {
        float maxY = float.MinValue;
        foreach (Transform c in j7)
        {
            if (!c.name.StartsWith("Flange")) continue;
            var mf = c.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Bounds b = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = b.center + new Vector3(
                    (i & 1) == 0 ? -b.extents.x : b.extents.x,
                    (i & 2) == 0 ? -b.extents.y : b.extents.y,
                    (i & 4) == 0 ? -b.extents.z : b.extents.z);
                float y = j7.InverseTransformPoint(c.TransformPoint(corner)).y;
                if (y > maxY) maxY = y;
            }
        }
        return maxY > float.MinValue ? maxY : 0.08f;
    }

    static Material GetOrCreateMaterial()
    {
        const string folder = "Assets/KinovaGen3/Materials";
        const string path = folder + "/TargetObject.mat";

        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/KinovaGen3", "Materials");

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        if (sh == null) return null;

        var m = new Material(sh);
        Color col = new Color(1f, 0.45f, 0.1f);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
        if (m.HasProperty("_Color")) m.SetColor("_Color", col);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Transform FindDeep(Transform p, string n)
    {
        if (p.name == n) return p;
        for (int i = 0; i < p.childCount; i++)
        {
            var r = FindDeep(p.GetChild(i), n);
            if (r != null) return r;
        }
        return null;
    }
}

using UnityEngine;

// Kinova Gen3 (7-DOF) joint rig. Drag the sliders in the Inspector to pose the arm.
[ExecuteAlways]
[DisallowMultipleComponent]
public class KinovaGen3Rig : MonoBehaviour
{
    [Header("Joint angles (degrees)")]
    [Range(-180f, 180f)] public float j1_baseYaw = 0f;
    [Range(-128.9f, 128.9f)] public float j2_shoulder = -36f;
    [Range(-180f, 180f)] public float j3_upperArmRoll = 0f;
    [Range(-147.8f, 147.8f)] public float j4_elbow = 118f;
    [Range(-180f, 180f)] public float j5_forearmRoll = 0f;
    [Range(-120.3f, 120.3f)] public float j6_wrist = 50f;
    [Range(-180f, 180f)] public float j7_toolRoll = 0f;

    [Header("Joint transforms (auto-assigned)")]
    public Transform joint1, joint2, joint3, joint4, joint5, joint6, joint7;

    void OnValidate() { Apply(); }
    void Update() { Apply(); }

    public void Apply()
    {
        Set(joint1, j1_baseYaw, Vector3.up);
        Set(joint2, j2_shoulder, Vector3.forward);
        Set(joint3, j3_upperArmRoll, Vector3.up);
        Set(joint4, j4_elbow, Vector3.forward);
        Set(joint5, j5_forearmRoll, Vector3.up);
        Set(joint6, j6_wrist, Vector3.forward);
        Set(joint7, j7_toolRoll, Vector3.up);
    }

    static void Set(Transform t, float deg, Vector3 axis)
    {
        if (t != null) t.localRotation = Quaternion.AngleAxis(deg, axis);
    }

    [ContextMenu("Pose: Showcase (like reference photo)")]
    public void PoseShowcase()
    {
        j1_baseYaw = 0; j2_shoulder = -36; j3_upperArmRoll = 0; j4_elbow = 118; j5_forearmRoll = 0; j6_wrist = 50; j7_toolRoll = 0;
        Apply();
    }

    [ContextMenu("Pose: Home (straight up)")]
    public void PoseHome()
    {
        j1_baseYaw = 0; j2_shoulder = 0; j3_upperArmRoll = 0; j4_elbow = 0; j5_forearmRoll = 0; j6_wrist = 0; j7_toolRoll = 0;
        Apply();
    }

    void OnDrawGizmosSelected()
    {
        DrawAxis(joint1, Vector3.up); DrawAxis(joint2, Vector3.forward); DrawAxis(joint3, Vector3.up);
        DrawAxis(joint4, Vector3.forward); DrawAxis(joint5, Vector3.up); DrawAxis(joint6, Vector3.forward); DrawAxis(joint7, Vector3.up);
    }

    static void DrawAxis(Transform t, Vector3 localAxis)
    {
        if (t == null) return;
        Gizmos.color = Color.yellow;
        Vector3 a = t.TransformDirection(localAxis) * 0.09f;
        Gizmos.DrawLine(t.position - a, t.position + a);
        Gizmos.DrawWireSphere(t.position, 0.006f);
    }
}

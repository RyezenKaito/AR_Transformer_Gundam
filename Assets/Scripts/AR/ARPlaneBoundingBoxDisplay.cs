using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Draws the oriented bounding box of a detected AR plane (works on every ARCore / ARKit device).
/// Add to the AR Plane prefab.
/// </summary>
[RequireComponent(typeof(ARPlane))]
public class ARPlaneBoundingBoxDisplay : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Color color = new Color(0f, 0.9f, 1f, 1f);
    [SerializeField] float lineWidth = 0.006f;
    [SerializeField, Tooltip("Height of the box above the plane (meters).")]
    float boxHeight = 0.05f;

    ARPlane m_Plane;
    LineRenderer m_Line;
    Vector3 m_LastCenter;
    Vector2 m_LastSize;

    void Awake()
    {
        m_Plane = GetComponent<ARPlane>();
        m_Line = ARBoxLines.CreateLine(transform, "PlaneBoundingBox", lineMaterial, color, lineWidth);
    }

    void LateUpdate()
    {
        Vector3 center = m_Plane.center;
        Vector2 size = m_Plane.size;
        if (size == Vector2.zero || (center == m_LastCenter && size == m_LastSize))
            return;

        m_LastCenter = center;
        m_LastSize = size;

        // Plane space: x/z lie on the plane, y is the plane normal.
        var min = new Vector3(center.x - size.x * 0.5f, 0.002f, center.z - size.y * 0.5f);
        var max = new Vector3(center.x + size.x * 0.5f, boxHeight, center.z + size.y * 0.5f);
        ARBoxLines.SetBox(m_Line, min, max);
    }
}

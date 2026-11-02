using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Visualizes an ARBoundingBox (3D object bounding boxes reported by the AR provider).
/// Add to the bounding box prefab used by ARBoundingBoxManager.
/// </summary>
[RequireComponent(typeof(ARBoundingBox))]
public class ARBoundingBoxVisualizer : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Color color = new Color(1f, 0.85f, 0f, 1f);
    [SerializeField] float lineWidth = 0.006f;

    ARBoundingBox m_Box;
    LineRenderer m_Line;
    Vector3 m_LastSize;

    void Awake()
    {
        m_Box = GetComponent<ARBoundingBox>();
        m_Line = ARBoxLines.CreateLine(transform, "DetectedBoundingBox", lineMaterial, color, lineWidth);
    }

    void LateUpdate()
    {
        Vector3 size = m_Box.size;
        if (size == m_LastSize)
            return;

        m_LastSize = size;
        ARBoxLines.SetBox(m_Line, -size * 0.5f, size * 0.5f);
    }
}

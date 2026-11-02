using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>Small on-screen status: AR session state, detected planes, selected shape and placed objects.</summary>
public class ARDetectionHUD : MonoBehaviour
{
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ARBoundingBoxManager boundingBoxManager;
    [SerializeField] ARShapePlacer placer;
    [SerializeField] TMP_Text label;

    float m_NextUpdate;

    void Update()
    {
        if (label == null || Time.unscaledTime < m_NextUpdate)
            return;
        m_NextUpdate = Time.unscaledTime + 0.2f;

        int planes = planeManager != null ? planeManager.trackables.count : 0;

        string boxes = "not supported on this device";
        if (boundingBoxManager != null && boundingBoxManager.subsystem != null)
            boxes = boundingBoxManager.trackables.count.ToString();

        int placed = placer != null ? placer.PlacedCount : 0;
        string selected = (placer != null && placer.SelectedName != null) ? placer.SelectedName : "none";

        string hint;
        if (planes == 0)
            hint = "Move your phone slowly to scan a surface...";
        else if (placer == null || placer.SelectedPrefab == null)
            hint = "Choose a shape from the menu below";
        else
            hint = "Tap a plane to place it, drag an object to move it";

        label.text = $"Session: {ARSession.state}\nPlanes: {planes}\nBounding boxes: {boxes}\nSelected: {selected}\nObjects placed: {placed}\n{hint}";
    }
}

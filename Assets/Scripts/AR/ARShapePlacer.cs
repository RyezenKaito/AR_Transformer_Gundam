using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// 1) Keeps the shape the user picked in the menu.
/// 2) Places that shape on top of a detected AR plane when the screen is touched.
/// 3) Moves an already placed shape over the plane when the user drags it.
/// Attach to the XR Origin (next to ARRaycastManager).
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARShapePlacer : MonoBehaviour
{
    [SerializeField, Tooltip("Camera used to pick placed objects with a physics ray.")]
    Camera arCamera;

    [SerializeField, Tooltip("Only accept floors / tables (horizontal planes facing up).")]
    bool onlyHorizontalUpPlanes = true;

    [SerializeField, Tooltip("Rotate a new object around its up axis so that it faces the camera.")]
    bool faceCamera = true;

    [SerializeField, Tooltip("Maximum number of objects kept in the scene (0 = unlimited). Oldest is removed first.")]
    int maxObjects = 20;

    [SerializeField, Tooltip("Scale multiplier while an object is being dragged (visual feedback).")]
    float dragScale = 1.15f;

    struct Pointer
    {
        public Vector2 position;
        public bool pressed, held, released;
        public int id;
    }

    static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    readonly List<GameObject> m_Placed = new List<GameObject>();
    ARRaycastManager m_Raycast;

    GameObject m_SelectedPrefab;
    GameObject m_Dragged;
    Vector3 m_DragOffset;
    Vector3 m_DragBaseScale;

    public GameObject SelectedPrefab => m_SelectedPrefab;
    public string SelectedName => m_SelectedPrefab != null ? m_SelectedPrefab.name : null;
    public int PlacedCount => m_Placed.Count;
    public bool IsDragging => m_Dragged != null;

    public event Action<GameObject> SelectionChanged;
    public event Action<GameObject> ObjectPlaced;

    void Awake()
    {
        m_Raycast = GetComponent<ARRaycastManager>();
        if (arCamera == null) arCamera = Camera.main;
    }

    void OnDisable()
    {
        EndDrag();
    }

    /// <summary>Called by the menu. Pass null to deselect.</summary>
    public void SelectPrefab(GameObject prefab)
    {
        m_SelectedPrefab = prefab;
        SelectionChanged?.Invoke(prefab);
    }

    public void ClearAll()
    {
        EndDrag();
        foreach (var go in m_Placed)
            if (go != null) Destroy(go);
        m_Placed.Clear();
    }

    void Update()
    {
        if (!ReadPointer(out Pointer p))
            return;

        if (p.pressed && !IsOverUI(p.id))
            BeginPress(p.position);

        if (p.held && m_Dragged != null)
            Drag(p.position);

        if (p.released)
            EndDrag();
    }

    void BeginPress(Vector2 screenPos)
    {
        // Touching an existing object starts a drag, touching a plane places the selected shape.
        GameObject picked = PickPlaced(screenPos);
        if (picked != null)
        {
            StartDrag(picked, screenPos);
            return;
        }

        if (m_SelectedPrefab == null)
            return;

        if (TryRaycastPlane(screenPos, out Pose pose))
            Place(pose);
    }

    GameObject PickPlaced(Vector2 screenPos)
    {
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null || m_Placed.Count == 0)
            return null;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 50f);
        Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        foreach (RaycastHit hit in hits)
            foreach (GameObject go in m_Placed)
                if (go != null && hit.transform.IsChildOf(go.transform))
                    return go;
        return null;
    }

    bool TryRaycastPlane(Vector2 screenPos, out Pose pose)
    {
        pose = default;
        if (!m_Raycast.Raycast(screenPos, s_Hits, TrackableType.PlaneWithinPolygon))
            return false;

        foreach (var hit in s_Hits)
        {
            if (onlyHorizontalUpPlanes && hit.trackable is ARPlane plane && plane.alignment != PlaneAlignment.HorizontalUp)
                continue;
            pose = hit.pose;
            return true;
        }
        return false;
    }

    void Place(Pose pose)
    {
        Quaternion rotation = pose.rotation;
        if (faceCamera && arCamera != null)
        {
            Vector3 toCamera = Vector3.ProjectOnPlane(arCamera.transform.position - pose.position, pose.up);
            if (toCamera.sqrMagnitude > 1e-4f)
                rotation = Quaternion.LookRotation(toCamera, pose.up);
        }

        if (maxObjects > 0 && m_Placed.Count >= maxObjects)
        {
            if (m_Placed[0] != null) Destroy(m_Placed[0]);
            m_Placed.RemoveAt(0);
        }

        GameObject go = Instantiate(m_SelectedPrefab, pose.position, rotation);
        m_Placed.Add(go);
        ObjectPlaced?.Invoke(go);
    }

    void StartDrag(GameObject go, Vector2 screenPos)
    {
        m_Dragged = go;
        m_DragBaseScale = go.transform.localScale;
        go.transform.localScale = m_DragBaseScale * dragScale;

        // Keep the point where the finger grabbed the object so it does not jump to the finger.
        m_DragOffset = Vector3.zero;
        if (TryRaycastPlane(screenPos, out Pose pose))
            m_DragOffset = Vector3.ProjectOnPlane(go.transform.position - pose.position, Vector3.up);
    }

    void Drag(Vector2 screenPos)
    {
        if (TryRaycastPlane(screenPos, out Pose pose))
            m_Dragged.transform.position = pose.position + m_DragOffset;
    }

    void EndDrag()
    {
        if (m_Dragged != null)
            m_Dragged.transform.localScale = m_DragBaseScale;
        m_Dragged = null;
    }

    static bool IsOverUI(int pointerId)
    {
        var es = EventSystem.current;
        if (es == null) return false;
        return pointerId < 0 ? es.IsPointerOverGameObject() : es.IsPointerOverGameObject(pointerId);
    }

    static bool ReadPointer(out Pointer p)
    {
        p = default;

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var t = touchscreen.primaryTouch;
            bool pr = t.press.wasPressedThisFrame, hd = t.press.isPressed, rl = t.press.wasReleasedThisFrame;
            if (pr || hd || rl)
            {
                p.position = t.position.ReadValue();
                p.pressed = pr; p.held = hd; p.released = rl;
                p.id = t.touchId.ReadValue();
                return true;
            }
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            bool pr = mouse.leftButton.wasPressedThisFrame, hd = mouse.leftButton.isPressed, rl = mouse.leftButton.wasReleasedThisFrame;
            if (pr || hd || rl)
            {
                p.position = mouse.position.ReadValue();
                p.pressed = pr; p.held = hd; p.released = rl;
                p.id = -1;
                return true;
            }
        }
        return false;
    }
}

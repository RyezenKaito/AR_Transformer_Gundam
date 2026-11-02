using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Places a 3D prefab on top of a detected AR plane when the player touches the screen.
/// Attach to the XR Origin (next to ARRaycastManager).
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARTapToPlaceObject : MonoBehaviour
{
    [SerializeField, Tooltip("The 3D object to place on the plane.")]
    GameObject objectPrefab;

    [SerializeField, Tooltip("If false, the first placed object is moved instead of spawning a new one.")]
    bool allowMultiple = true;

    [SerializeField, Tooltip("Maximum number of objects kept in the scene (0 = unlimited). Oldest is removed first.")]
    int maxObjects = 10;

    [SerializeField, Tooltip("Only accept floors / tables (horizontal planes facing up).")]
    bool onlyHorizontalUpPlanes = true;

    [SerializeField, Tooltip("Rotate the object around its up axis so that its front faces the camera.")]
    bool faceCamera = true;

    static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    readonly List<GameObject> m_Placed = new List<GameObject>();
    ARRaycastManager m_RaycastManager;
    Camera m_Camera;

    public int placedCount => m_Placed.Count;
    public event Action<GameObject> objectPlaced;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_Camera = Camera.main;
    }

    void Update()
    {
        if (!TryGetPress(out Vector2 screenPos))
            return;

        if (m_Camera == null) m_Camera = Camera.main;

        // Hits are sorted by distance, nearest first.
        if (!m_RaycastManager.Raycast(screenPos, s_Hits, TrackableType.PlaneWithinPolygon))
            return;

        foreach (var hit in s_Hits)
        {
            if (onlyHorizontalUpPlanes && hit.trackable is ARPlane plane && plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            Place(hit.pose);
            break;
        }
    }

    static bool TryGetPress(out Vector2 position)
    {
        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            position = touch.primaryTouch.position.ReadValue();
            return true;
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    void Place(Pose pose)
    {
        if (objectPrefab == null)
        {
            Debug.LogWarning("ARTapToPlaceObject: no prefab assigned.");
            return;
        }

        Quaternion rotation = pose.rotation;
        if (faceCamera && m_Camera != null)
        {
            Vector3 toCamera = Vector3.ProjectOnPlane(m_Camera.transform.position - pose.position, pose.up);
            if (toCamera.sqrMagnitude > 1e-4f)
                rotation = Quaternion.LookRotation(toCamera, pose.up);
        }

        GameObject go;
        if (!allowMultiple && m_Placed.Count > 0)
        {
            go = m_Placed[0];
            go.transform.SetPositionAndRotation(pose.position, rotation);
        }
        else
        {
            if (maxObjects > 0 && m_Placed.Count >= maxObjects)
            {
                Destroy(m_Placed[0]);
                m_Placed.RemoveAt(0);
            }
            go = Instantiate(objectPrefab, pose.position, rotation);
            m_Placed.Add(go);
        }

        objectPlaced?.Invoke(go);
    }

    public void ClearAll()
    {
        foreach (var go in m_Placed)
            if (go != null) Destroy(go);
        m_Placed.Clear();
    }
}

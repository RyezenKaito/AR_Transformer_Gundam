using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacement : MonoBehaviour
{
    public GameObject gundamPrefab; // Kéo Prefab Gundam vào đây
    private GameObject spawnedObject;
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start() { raycastManager = GetComponent<ARRaycastManager>(); }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                if (spawnedObject == null)
                    spawnedObject = Instantiate(gundamPrefab, hitPose.position, hitPose.rotation);
                else
                    spawnedObject.transform.position = hitPose.position;
            }
        }
    }
}
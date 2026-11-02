using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteInEditMode]
public class CameraDOFUpdate : MonoBehaviour
{
    Camera cam;
    HDAdditionalCameraData hdCam;

    public float aperture;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        hdCam = cam.GetComponent<HDAdditionalCameraData>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.aperture = aperture;
    }
}

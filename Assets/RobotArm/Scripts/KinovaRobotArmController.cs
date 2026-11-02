using UnityEngine;

/// <summary>
/// Bo dieu khien toan dien cho Canh tay Robot Kinova Gen3
/// Gom 6 khop dong hoc, cam bien khoang cach quang hoc va cac tu the chuan (presets)
/// </summary>
public class KinovaRobotArmController : MonoBehaviour
{
    [Header("Cac Khop Noi Chinh (Joints)")]
    [Tooltip("Khop 1: Xoay de (Yaw)")]
    public Transform joint1_BaseRotate;

    [Tooltip("Khop 2: Khop vai (Shoulder Pitch)")]
    public Transform joint2_Shoulder;

    [Tooltip("Khop 3: Khop khuu (Elbow Pitch)")]
    public Transform joint3_Elbow;

    [Tooltip("Khop 4: Co tay 1 (Wrist Pitch/Roll)")]
    public Transform joint4_Wrist;

    [Tooltip("Dau thao tac & Bo phan dau (Head / Tool Flange)")]
    public Transform head;

    [Header("Cam Bien Nhan Biet Khoang Cach (Distance Sensor)")]
    [Tooltip("Dau phat cam bien thi giac / laser")]
    public Transform sensorTip;
    public float maxSensorRange = 3.0f;
    public LayerMask detectionLayers = ~0;

    [Header("Goc Khop Hien Tai (Degrees)")]
    [Range(-180f, 180f)] public float j1_BaseAngle = 0f;
    [Range(260f, 380f)] public float j2_ShoulderAngle = 328f;
    [Range(40f, 200f)] public float j3_ElbowAngle = 122f;
    [Range(-180f, 180f)] public float j4_WristAngle = -90f;
    [Range(-180f, 180f)] public float head_Angle = 0f;

    [Header("Thong Tin Cam Bien (Realtime Telemetry)")]
    public bool objectDetected = false;
    public float detectedDistance = 0f;
    public string detectedObjectName = "None";

    [Header("Giao Dien Dieu Khien (GUI)")]
    public bool showOnScreenGUI = true;

    // Vi tri tu the anh mau (Reference Pose)
    private readonly float ref_j1 = 0f;
    private readonly float ref_j2 = 328f;
    private readonly float ref_j3 = 122f;
    private readonly float ref_j4 = -90f;
    private readonly float ref_head = 0f;

    void Start()
    {
        AutoFindReferences();
        ApplyPoseInstant(ref_j1, ref_j2, ref_j3, ref_j4, ref_head);
    }

    void AutoFindReferences()
    {
        if (joint1_BaseRotate == null) joint1_BaseRotate = transform.Find("Joint1_BaseRotate");
        if (joint1_BaseRotate != null && joint2_Shoulder == null) joint2_Shoulder = joint1_BaseRotate.Find("Joint2_Shoulder");
        if (joint2_Shoulder != null && joint3_Elbow == null) joint3_Elbow = joint2_Shoulder.Find("Joint3_Elbow");
        if (joint3_Elbow != null && joint4_Wrist == null) joint4_Wrist = joint3_Elbow.Find("Joint4_Wrist");
        if (joint4_Wrist != null && head == null) head = joint4_Wrist.Find("Head");

        if (head != null && sensorTip == null)
        {
            var s = head.Find("Sensor_Camera/Sensor_Body/Sensor_Lens");
            if (s != null) sensorTip = s;
            else sensorTip = head;
        }
    }

    void Update()
    {
        UpdateJointTransforms();
        UpdateDistanceSensor();
        HandleKeyboardInput();
    }

    void UpdateJointTransforms()
    {
        if (joint1_BaseRotate != null)
            joint1_BaseRotate.localRotation = Quaternion.Euler(0f, j1_BaseAngle, 0f);

        if (joint2_Shoulder != null)
            joint2_Shoulder.localRotation = Quaternion.Euler(0f, 0f, j2_ShoulderAngle);

        if (joint3_Elbow != null)
            joint3_Elbow.localRotation = Quaternion.Euler(0f, 0f, j3_ElbowAngle);

        if (joint4_Wrist != null)
            joint4_Wrist.localRotation = Quaternion.Euler(0f, 0f, j4_WristAngle);
    }

    void UpdateDistanceSensor()
    {
        if (sensorTip == null) return;

        Vector3 origin = sensorTip.position;
        Vector3 forward = sensorTip.forward;

        RaycastHit hit;
        if (Physics.Raycast(origin, forward, out hit, maxSensorRange, detectionLayers))
        {
            if (!hit.transform.IsChildOf(transform))
            {
                objectDetected = true;
                detectedDistance = hit.distance;
                detectedObjectName = hit.collider.gameObject.name;
                Debug.DrawLine(origin, hit.point, Color.green);
                return;
            }
        }

        objectDetected = false;
        detectedDistance = maxSensorRange;
        detectedObjectName = "None";
        Debug.DrawRay(origin, forward * maxSensorRange, Color.red);
    }

    void HandleKeyboardInput()
    {
        // Phim 1: Tu the anh mau (Reference Pose)
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyPose(ref_j1, ref_j2, ref_j3, ref_j4, ref_head);
        // Phim 2: Tu the Home thang dung
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyPose(0f, 360f, 180f, 0f, 0f);
        // Phim 3: Tu the gap / voi ve phia truoc (Reach)
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyPose(0f, 310f, 75f, -25f, 0f);
        // Phim 4: Tu the soi / kiem tra mat dat (Inspect Down)
        if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyPose(0f, 320f, 110f, -50f, 0f);
    }

    public void ApplyPose(float j1, float j2, float j3, float j4, float h)
    {
        j1_BaseAngle = j1;
        j2_ShoulderAngle = j2;
        j3_ElbowAngle = j3;
        j4_WristAngle = j4;
        head_Angle = h;
    }

    public void ApplyPoseInstant(float j1, float j2, float j3, float j4, float h)
    {
        ApplyPose(j1, j2, j3, h, h);
        UpdateJointTransforms();
    }

    void OnGUI()
    {
        if (!showOnScreenGUI) return;

        GUI.Box(new Rect(15, 15, 340, 310), "BIEU KHIEN CANH TAY ROBOT KINOVA GEN3");

        int y = 45;
        GUI.Label(new Rect(25, y, 320, 22), $"1. Xoay De (Joint 1): {j1_BaseAngle:F1} deg");
        j1_BaseAngle = GUI.HorizontalSlider(new Rect(25, y + 20, 310, 18), j1_BaseAngle, -180f, 180f);

        y += 45;
        GUI.Label(new Rect(25, y, 320, 22), $"2. Khop Vai (Joint 2): {j2_ShoulderAngle:F1} deg");
        j2_ShoulderAngle = GUI.HorizontalSlider(new Rect(25, y + 20, 310, 18), j2_ShoulderAngle, 260f, 380f);

        y += 45;
        GUI.Label(new Rect(25, y, 320, 22), $"3. Khop Khuu (Joint 3): {j3_ElbowAngle:F1} deg");
        j3_ElbowAngle = GUI.HorizontalSlider(new Rect(25, y + 20, 310, 18), j3_ElbowAngle, 40f, 200f);

        y += 45;
        GUI.Label(new Rect(25, y, 320, 22), $"4. Co Tay (Joint 4): {j4_WristAngle:F1} deg");
        j4_WristAngle = GUI.HorizontalSlider(new Rect(25, y + 20, 310, 18), j4_WristAngle, -180f, 180f);

        y += 45;
        // Cam bien khoang cach
        string sensorText = objectDetected 
            ? $"[CAM BIEN] Phat hien: {detectedObjectName} ({detectedDistance:F2}m)" 
            : "[CAM BIEN] Khong co vat the gan (< 3.0m)";
        GUI.color = objectDetected ? Color.green : Color.white;
        GUI.Label(new Rect(25, y, 320, 22), sensorText);
        GUI.color = Color.white;

        y += 30;
        if (GUI.Button(new Rect(25, y, 150, 26), "1. Tu The Anh Mau")) ApplyPose(ref_j1, ref_j2, ref_j3, ref_j4, ref_head);
        if (GUI.Button(new Rect(185, y, 150, 26), "2. Tu The Vuon (Reach)")) ApplyPose(0f, 310f, 75f, -25f, 0f);
    }
}

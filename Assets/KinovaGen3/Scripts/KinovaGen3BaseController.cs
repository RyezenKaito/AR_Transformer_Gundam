using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Câu b: điều khiển khớp cơ sở (base) bằng phím mũi tên.
//   Mũi tên lên  : xoay +10°
//   Mũi tên xuống: xoay -10°
// Gắn vào đối tượng KinovaGen3 (cùng đối tượng với KinovaGen3Rig).
// KinovaGen3Rig ghi đè localRotation của các khớp mỗi frame,
// nên script này đổi GÓC trong Rig chứ không xoay Transform trực tiếp.
[DisallowMultipleComponent]
[RequireComponent(typeof(KinovaGen3Rig))]
public class KinovaGen3BaseController : MonoBehaviour
{
    public enum BaseJoint { J1_BaseYaw, J2_Shoulder }

    [Tooltip("Khớp được điều khiển bằng phím mũi tên. J1 = khớp đế (xoay quanh trục dọc).")]
    public BaseJoint joint = BaseJoint.J1_BaseYaw;

    [Tooltip("Góc xoay mỗi lần nhấn phím (độ)")]
    public float step = 10f;

    [Tooltip("Tốc độ xoay để chuyển động mượt (độ/giây). Kết quả sau mỗi lần nhấn luôn là đúng step độ.")]
    public float speed = 90f;

    [Tooltip("Bật nếu thấy chiều quay bị ngược so với yêu cầu")]
    public bool invert = false;

    KinovaGen3Rig rig;
    float currentAngle;
    float targetAngle;

    void Start()
    {
        rig = GetComponent<KinovaGen3Rig>();
        currentAngle = targetAngle = GetAngle();
    }

    void Update()
    {
        bool up = UpPressed();
        bool down = DownPressed();

        if (up || down)
        {
            // Đồng bộ lại với Rig khi đang đứng yên (phòng khi kéo slider trong Inspector)
            if (Mathf.Approximately(currentAngle, targetAngle))
                currentAngle = targetAngle = GetAngle();

            float delta = (up ? step : 0f) - (down ? step : 0f);
            targetAngle += invert ? -delta : delta;

            if (joint == BaseJoint.J2_Shoulder)
                targetAngle = Mathf.Clamp(targetAngle, -128.9f, 128.9f);
        }

        if (!Mathf.Approximately(currentAngle, targetAngle))
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, speed * Time.deltaTime);
            SetAngle(currentAngle);
        }
    }

    float GetAngle()
    {
        return joint == BaseJoint.J1_BaseYaw ? rig.j1_baseYaw : rig.j2_shoulder;
    }

    void SetAngle(float a)
    {
        if (joint == BaseJoint.J1_BaseYaw) rig.j1_baseYaw = Mathf.DeltaAngle(0f, a); // giữ trong [-180, 180]
        else rig.j2_shoulder = a;
        rig.Apply();
    }

    bool UpPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }

    bool DownPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.DownArrow);
#endif
    }
}

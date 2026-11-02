using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Gan vao doi tuong Base
public class BaseController : MonoBehaviour
{
    public float step = 10f;   // goc xoay moi lan nhan phim (do)

    void Update()
    {
        if (UpPressed())   transform.Rotate(0f, step, 0f, Space.Self);
        if (DownPressed()) transform.Rotate(0f, -step, 0f, Space.Self);
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

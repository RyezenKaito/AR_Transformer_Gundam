using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 720f;
    public float gravity = -20f;

    CharacterController controller;
    float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    Vector2 ReadInput()
    {
        float x = 0f, y = 0f;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  y -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
        }
#else
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  y -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  x -= 1f;
#endif
        return new Vector2(x, y);
    }

    void Update()
    {
        Vector2 input = ReadInput();

        // Move relative to the camera, flattened onto the ground plane
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 dir = fwd * input.y + right * input.x;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = dir * moveSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
    }
}

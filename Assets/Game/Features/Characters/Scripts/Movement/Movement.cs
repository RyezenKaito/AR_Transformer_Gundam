using UnityEngine;
using UnityEngine.InputSystem; // Khai báo sử dụng hệ thống điều khiển mới

public class MechaMovement : MonoBehaviour
{
    public float speed = 1.5f;

    void Update()
    {
        // Kiểm tra xem thiết bị có bàn phím cứng không (tránh lỗi khi đưa lên điện thoại)
        if (Keyboard.current == null) return;

        float x = 0;
        float z = 0;    

        // Bắt tín hiệu phím mũi tên bằng New Input System
        if (Keyboard.current.upArrowKey.isPressed) z = 1;
        if (Keyboard.current.downArrowKey.isPressed) z = -1;
        if (Keyboard.current.rightArrowKey.isPressed) x = 1;
        if (Keyboard.current.leftArrowKey.isPressed) x = -1;

        // Tính toán vector và di chuyển
        Vector3 movement = new Vector3(x, 0, z).normalized * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Xoay mặt cỗ máy theo hướng đi
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }
    }
}
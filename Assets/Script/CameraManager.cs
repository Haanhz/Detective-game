using UnityEngine;

public class CameraMove2D : MonoBehaviour
{
    [Header("Tốc độ di chuyển của camera")]
    public float moveSpeed = 5f;

    [Header("Giới hạn vùng di chuyển (tuỳ chọn)")]
    public bool limitMovement = false;
    public Vector2 minPosition; // Giới hạn vị trí nhỏ nhất (X, Y)
    public Vector2 maxPosition; // Giới hạn vị trí lớn nhất (X, Y)

    void Update()
    {
        // Nhận input bàn phím
        float moveX = Input.GetAxis("Horizontal"); // A/D hoặc mũi tên trái/phải
        float moveY = Input.GetAxis("Vertical");   // W/S hoặc mũi tên lên/xuống

        // Tạo vector di chuyển
        Vector3 move = new Vector3(moveX, moveY, 0f) * moveSpeed * Time.deltaTime;

        // Cập nhật vị trí
        transform.position += move;

        // Nếu có bật giới hạn vùng di chuyển
        if (limitMovement)
        {
            float clampedX = Mathf.Clamp(transform.position.x, minPosition.x, maxPosition.x);
            float clampedY = Mathf.Clamp(transform.position.y, minPosition.y, maxPosition.y);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }
}

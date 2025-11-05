using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Player cần theo dõi")]
    public Transform player;

    [Header("Tốc độ di chuyển mượt")]
    public float smoothSpeed = 5f;

    [Header("Độ lệch so với player")]
    public Vector3 offset;

    // Giới hạn camera hiện tại (nếu có)
    private BoxCollider2D currentRoomBounds;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            transform.position.z
        );

        if (currentRoomBounds != null)
        {
            Bounds bounds = currentRoomBounds.bounds;
            float camHeight = Camera.main.orthographicSize;
            float camWidth = camHeight * Camera.main.aspect;

            // 🔹 Kiểm tra nếu phòng nhỏ hơn vùng nhìn của camera
            bool roomTooSmallX = (bounds.size.x <= camWidth * 2);
            bool roomTooSmallY = (bounds.size.y <= camHeight * 2);

            if (roomTooSmallX && roomTooSmallY)
            {
                // Phòng nhỏ cả 2 chiều → camera ở giữa phòng
                targetPos.x = bounds.center.x;
                targetPos.y = bounds.center.y;
            }
            else
            {
                // 🔹 Chỉ clamp nếu phòng lớn hơn camera
                if (!roomTooSmallX)
                {
                    targetPos.x = Mathf.Clamp(targetPos.x,
                        bounds.min.x + camWidth,
                        bounds.max.x - camWidth);
                }
                else
                {
                    targetPos.x = bounds.center.x;
                }

                if (!roomTooSmallY)
                {
                    targetPos.y = Mathf.Clamp(targetPos.y,
                        bounds.min.y + camHeight,
                        bounds.max.y - camHeight);
                }
                else
                {
                    targetPos.y = bounds.center.y;
                }
            }
        }

        // Di chuyển mượt đến vị trí mới
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    // Khi player vào phòng
    public void SetCurrentRoom(BoxCollider2D roomBounds)
    {
        currentRoomBounds = roomBounds;
    }

    // Khi player rời khỏi phòng
    public void ClearCurrentRoom(BoxCollider2D roomBounds)
    {
        if (currentRoomBounds == roomBounds)
            currentRoomBounds = null;
    }
}

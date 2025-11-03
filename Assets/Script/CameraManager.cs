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

        // Nếu camera đang trong một phòng → giới hạn trong vùng đó
        if (currentRoomBounds != null)
        {
            Bounds bounds = currentRoomBounds.bounds;
            float camHeight = Camera.main.orthographicSize;
            float camWidth = camHeight * Camera.main.aspect;

            targetPos.x = Mathf.Clamp(targetPos.x,
                bounds.min.x + camWidth,
                bounds.max.x - camWidth);

            targetPos.y = Mathf.Clamp(targetPos.y,
                bounds.min.y + camHeight,
                bounds.max.y - camHeight);
        }

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

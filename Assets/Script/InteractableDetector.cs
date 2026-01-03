using UnityEngine;
using System.Collections.Generic;

public class InteractableDetector : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 2.0f; // Khoảng cách để hiện dấu chấm than
    public GameObject alertUI;          // Kéo cái Image dấu chấm than vào đây

    [Header("Target Tags")]
    public List<string> interactableTags = new List<string> 
    { 
        "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", 
        "Limit1", "Limit2", "Hide", "Bed", "Murder", "NPC",
        "Limit3", "Limit4", "Limit5", "Limit6",
        "Rope", "Crack", "StrangeTable", "OpenWindow",
        "Fridge"
    };

    void Update()
    {
        CheckNearbyInteractables();
    }

    void CheckNearbyInteractables()
    {
        // Tìm tất cả Collider trong phạm vi detectionRange xung quanh Player
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        
        bool isNearInteractable = false;

        foreach (var hitCollider in hitColliders)
        {
            // Bỏ qua nếu object không active
            if (!hitCollider.gameObject.activeInHierarchy)
                continue;

            // Kiểm tra xem Tag có nằm trong danh sách cho phép không
            if (interactableTags.Contains(hitCollider.tag))
            {
                // Kiểm tra xem vật phẩm này đã được nhặt chưa (nếu có EvidenceManager)
                if (EvidenceManager.Instance != null)
                {
                    // Nếu đã nhặt rồi thì bỏ qua
                    if (EvidenceManager.Instance.HasEvidence(hitCollider.tag))
                        continue;
                }

                // Nếu đến đây nghĩa là: active + chưa nhặt + có tag hợp lệ
                isNearInteractable = true;
                break; // Tìm thấy 1 cái là đủ
            }
        }

        // Bật/Tắt UI dựa trên kết quả quét
        if (alertUI != null)
        {
            alertUI.SetActive(isNearInteractable);
        }
    }

    // Vẽ vòng tròn trong Scene để bạn dễ căn chỉnh khoảng cách
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
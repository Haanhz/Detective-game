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
        "Limit1", "Limit2", "Hide", "Bed", "Murder", "NPC" 
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
            // Kiểm tra xem Tag của vật thể có nằm trong danh sách cho phép không
            if (interactableTags.Contains(hitCollider.tag))
            {
                isNearInteractable = true;
                break; // Tìm thấy 1 cái là đủ để hiện UI rồi
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
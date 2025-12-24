using UnityEngine;

public class AtticDoor : MonoBehaviour
{
    public string targetNPC = "Mai"; // NPC chứa thông tin chìa khóa
    public int requiredKey = 2;      // Info Key cần thiết để mở cửa
    
    private BoxCollider2D doorCollider;
    private bool isUnlocked = false;

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Nếu đã mở khóa rồi thì không cần kiểm tra nữa
        if (isUnlocked) return;

        // Kiểm tra xem trong Dictionary của NPC tương ứng đã có key này chưa
        if (DialogueManager.Instance != null)
        {
            if (CheckKeyInManager())
            {
                UnlockDoor();
            }
        }
    }

    bool CheckKeyInManager()
    {
        // Sử dụng hàm HasImportantInfo có sẵn trong DialogueManager của bạn
        // Lưu ý: Đảm bảo hàm HasImportantInfo trong DialogueManager đã được sửa thành public
        
        switch (targetNPC)
        {
            case "Sang": return DialogueManager.Instance.Sang.ContainsKey(requiredKey);
            case "Mai":  return DialogueManager.Instance.Mai.ContainsKey(requiredKey);
            case "Tan":  return DialogueManager.Instance.Tan.ContainsKey(requiredKey);
            case "May":  return DialogueManager.Instance.May.ContainsKey(requiredKey);
            default: return false;
        }
    }

    void UnlockDoor()
    {
        isUnlocked = true;
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true; // Chuyển sang Trigger để người chơi đi qua được
            Debug.Log("Cửa gác mái đã được mở khóa!");
        }
    }
}
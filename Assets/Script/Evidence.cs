using UnityEngine;
using TMPro;
using System;
using System.Collections;

// Script này được gắn vào từng vật phẩm bằng chứng
public class Evidence : MonoBehaviour
{
    // Tham chiếu UI (Gắn DialogueBox và TextMeshProUGUI vào đây)
    public GameObject DialogueBox; 
    public TextMeshProUGUI CatchName;
    
    // Cài đặt hiển thị
    public float displayTime = 1f; // Thời gian hộp thoại tồn tại sau khi gõ xong
    public float typingSpeed = 0.01f; // Tốc độ gõ chữ
    
    // Quản lý Coroutine
    private Coroutine displayCoroutine;

    // Dữ liệu Evidence
    [Tooltip("Nếu để trống, sẽ dùng tag của GameObject")]
    public string evidenceTag;
    private float weight = 0f;

    public int spawnNight; 
    
    // Logic Thu thập
    public KeyCode pickupKey = KeyCode.F;
    private bool collected = false;
    private readonly string announceText = "You have found: ";

    void Start()
    {
        // Khởi tạo UI
        if (DialogueBox != null) DialogueBox.SetActive(false);
        if (CatchName != null) CatchName.text = string.Empty;
        
        // Thiết lập Tag nếu chưa được gán trong Inspector
        if (string.IsNullOrEmpty(evidenceTag))
            evidenceTag = gameObject.tag;
    }

    // --- LOGIC THU THẬP ---

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Chỉ xử lý khi va chạm với Player, nhấn phím, và chưa được thu thập
        if (collision.gameObject.CompareTag("Player") && Input.GetKey(pickupKey) && !collected)
        {
            // 1. Đánh dấu đã thu thập và tính toán dữ liệu
            collected = true;
            weight = CalculateWeight(evidenceTag);
            string evidenceName = GetEvidenceName(evidenceTag); 

            // 2. Thêm vào EvidenceManager
            if (EvidenceManager.Instance != null)
            {
                 EvidenceManager.Instance.AddEvidence(evidenceTag, weight);
            }

            // 3. Nếu tag là "Hide" thì ẩn ngay lập tức và không hiện dialogue
            if (evidenceTag == "Hide")
            {
                gameObject.SetActive(false);
                return; // Kết thúc hàm, không chạy phần hiển thị UI
            }

            // 4. HIỂN THỊ UI THÔNG BÁO (Gõ chữ) - chỉ cho các evidence khác "Hide"
            if (DialogueBox != null && CatchName != null && evidenceName != "Hide")
            {
                // Dừng Coroutine cũ (gõ chữ hoặc ẩn) để bắt đầu cái mới
                if (displayCoroutine != null)
                {
                    StopCoroutine(displayCoroutine);
                }
                
                DialogueBox.SetActive(true);
                string fullMessage = announceText + evidenceName;
                
                // Bắt đầu Coroutine gõ chữ
                displayCoroutine = StartCoroutine(TypeEvidenceFound(fullMessage)); 
            } 
        }
    }

    // --- COROUTINE HIỂN THỊ UI CÓ HIỆU ỨNG GÕ CHỮ ---

    /// <summary>
    /// Hiển thị thông báo bằng cách gõ từng ký tự.
    /// </summary>
    private IEnumerator TypeEvidenceFound(string fullMessage)
    {
        CatchName.text = string.Empty; 
        
        foreach (char c in fullMessage.ToCharArray())
        {
            CatchName.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Sau khi gõ xong, bắt đầu Coroutine ẩn hộp thoại
        displayCoroutine = StartCoroutine(HideDialogueAfterTime(displayTime));
    }

    /// <summary>
    /// Chờ thời gian quy định rồi ẩn hộp thoại.
    /// </summary>
    private IEnumerator HideDialogueAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        
        // Ẩn hộp thoại
        if (DialogueBox != null)
        {
            DialogueBox.SetActive(false);
            CatchName.text = string.Empty; // Xóa text khi ẩn
        }
        
        // Ẩn GameObject nếu cần (sau khi dialogue đã hiện xong)
        if (ShouldHide(evidenceTag))
        {
            gameObject.SetActive(false);
        }
        
        displayCoroutine = null;
    }

    // --- HÀM HỖ TRỢ DỮ LIỆU ---
    
    string GetEvidenceName(string evidenceTag)
    {
        switch (evidenceTag)
        {
            case "LivingCorner": return "Living Corner";
            case "Ultimatum": return "Ultimatum";
            case "HangPhone": return "Hang's Phone";
            case "HangNoteBook": return "Hang's Notebook";
            case "Limit1": return "May's Diary";
            case "Limit2": return "The torn sheet in Hang's Notebook";
            case "Hide": return "Hide";
            default: return "Unknown Evidence";
        }
    }

    float CalculateWeight(string tagName)
    {
        switch (tagName)
        {
            case "LivingCorner": return 1.0f;
            case "Ultimatum": return 2.0f;
            case "HangPhone": return 3.0f;
            case "HangNoteBook": return 3.0f;
            case "Limit1": return 5.0f;
            case "Limit2": return 5.0f;
            case "Hide": return 0.0f;
            default: return 0f;
        }
    }

    bool ShouldHide(string tagName)
    {
        switch (tagName)
        {
            case "HangNoteBook":
            case "Limit1":
            case "Limit2":
            case "Hide":
                return true;

            default:
                return false;   
        }
    }
}
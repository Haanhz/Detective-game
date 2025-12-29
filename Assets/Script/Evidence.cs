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

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
        if (other.CompareTag("UVLight"))
        {
            GameManager.Instance.isLight= true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
        if (other.CompareTag("UVLight"))
        {
            GameManager.Instance.isLight= false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey) && !collected)
        {
            CollectEvidence();
        }
    }

    private void CollectEvidence()
    {
        collected = true;

        weight = CalculateWeight(evidenceTag);
        string evidenceName = GetEvidenceName(evidenceTag);

        if (EvidenceManager.Instance != null)
        {
            EvidenceManager.Instance.AddEvidence(evidenceTag, weight);
        }

        if (evidenceTag == "Hide")
        {
            gameObject.SetActive(false);
            return;
        }

        if (DialogueBox != null && CatchName != null)
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);

            DialogueBox.SetActive(true);
            string fullMessage = announceText + evidenceName;
            displayCoroutine = StartCoroutine(TypeEvidenceFound(fullMessage));
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
            Destroy(gameObject);
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
            case "Crack": return "Crack outside the attic";
            case "StrangeTable": return "Four chairs around the table, three of them fall";
            case "OpenWindow": return "Open window in the attic";
            case "Rope": return "Rope in the attic";
            case "Limit1": return "Footprint in the attic";
            case "Limit2": return "Hang the Ghost";
            case "Limit3": return "Mai's Diary";
            case "Limit4": return "Family Photo";
            case "Limit5": return "May's Diary";
            case "Limit6": return "The ghost mom";
            case "Hide": return "Hide";
            case "SangStuff": return "Mr.Sang precious thing";
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
            case "Limit3":
            case "Limit4":
            case "Limit5":
            case "Limit6":
            case "Hide":
            case "SangStuff":
                return true;

            default:
                return false;
        }
    }
}
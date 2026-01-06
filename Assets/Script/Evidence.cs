using UnityEngine;
using TMPro;
using System;
using System.Collections;

// Script này gắn vào Evidence
public class Evidence : MonoBehaviour
{
    public GameObject DialogueBox;
    public TextMeshProUGUI CatchName;

    public float displayTime = 1f;
    public float typingSpeed = 0.01f;

    private Coroutine displayCoroutine;

    [Tooltip("Nếu để trống, sẽ dùng tag của GameObject")]
    public string evidenceTag;
    private float weight = 0f;

    public int spawnNight;

    public KeyCode pickupKey = KeyCode.F;
    private bool collected = false;
    private readonly string announceText = "Recorded in case file: ";

    // Tham chiếu LimitController nếu đây là Limit Evidence
    private LimitController limitController;

    void Start()
    {
        if (DialogueBox != null) DialogueBox.SetActive(false);
        if (CatchName != null) CatchName.text = string.Empty;

        if (string.IsNullOrEmpty(evidenceTag))
            evidenceTag = gameObject.tag;
        // KIỂM TRA KHI VỪA XUẤT HIỆN
        if (EvidenceManager.Instance != null && EvidenceManager.Instance.permanentlyRemovedEvidence.Contains(evidenceTag))
        {
            // Kiểm tra ShouldHide để quyết định destroy hay lock
            if (ShouldHide(evidenceTag))
            {
                // ShouldHide = true → Destroy (Limit1, HangNoteBook...)
                Destroy(gameObject);
                return;
            }
            else
            {
                // ShouldHide = false → Lock (LivingCorner, Ultimatum...)
                collected = true;
                Debug.Log($"[Evidence.Start] Locked permanently removed evidence: {evidenceTag}");
                // Không return, tiếp tục chạy để giữ object
            }
        }
        else if (EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence(evidenceTag))
        {
            // Nếu đã nhặt rồi, kiểm tra xem loại này có phải loại biến mất không
            if (ShouldHide(evidenceTag))
            {
                Destroy(this.gameObject); // Xóa ngay lập tức nếu là SangStuff, Limit...
                return;
            }
            else
            {
                collected = true; // Ở lại làm cảnh nhưng khóa tương tác
            }
        }
        else
        {
            collected = false; // Chưa nhặt
        }
        // Kiểm tra xem có phải Limit không
        limitController = GetComponent<LimitController>();
    }

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Nếu là Limit, chỉ cho collect khi visible
        if (limitController != null)
        {
            if (playerInRange && limitController.IsVisible && Input.GetKeyDown(pickupKey) && !collected)
            {
                CollectEvidence();
            }
        }
        else
        {
            // Evidence bình thường
            if (playerInRange && Input.GetKeyDown(pickupKey) && !collected)
            {
                CollectEvidence();
            }
        }

        if (DialogueManager.Instance.HasImportantInfo("Mai", 2))
        {
            if (EvidenceManager.Instance != null)
        {
            EvidenceManager.Instance.AddEvidence("Key", 0);
        }
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

        // Nếu là Hide thì ẩn ngay không cần dialogue
        if (evidenceTag == "Hide")
        {
            gameObject.SetActive(false);
            return;
        }

        // Hiển thị dialogue
        if (DialogueBox != null && CatchName != null)
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);

            DialogueBox.SetActive(true);
            string fullMessage = announceText + evidenceName;
            displayCoroutine = StartCoroutine(TypeEvidenceFound(fullMessage));
        }
        else
        {
            // Nếu không có dialogue box, destroy luôn
            if (ShouldHide(evidenceTag))
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }

    private IEnumerator TypeEvidenceFound(string fullMessage)
    {
        CatchName.text = string.Empty;

        // Gõ từng ký tự
        foreach (char c in fullMessage.ToCharArray())
        {
            CatchName.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Chờ thêm thời gian displayTime
        yield return new WaitForSeconds(displayTime);

        // Ẩn dialogue box
        if (DialogueBox != null)
        {
            DialogueBox.SetActive(false);
            CatchName.text = string.Empty;
        }

        // Destroy GameObject nếu cần
        if (ShouldHide(evidenceTag))
        {
            Destroy(gameObject);
        }

        displayCoroutine = null;
    }

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
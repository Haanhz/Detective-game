using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CaseFileUI : MonoBehaviour
{
    public static CaseFileUI Instance;

    [Header("Panel Reference")]
    public GameObject caseFilePanel;

    [Header("Evidence Section")]
    public Transform evidenceGrid;
    public GameObject evidenceCardPrefab;
    
    [Header("Information Section")]
    public Transform informationGrid;
    public GameObject informationCardPrefab;

    [Header("Selection Display")]
    public TextMeshProUGUI selectedCountText;
    public TextMeshProUGUI instructionText;

    [Header("Submission")]
    public Button submitButton;
    public TextMeshProUGUI submitButtonText;
    public int maxSelection = 10; // Tổng số evidence + info có thể chọn
    public int minSelection = 3;

    // Dữ liệu lưu trữ
    private List<string> selectedEvidenceNames = new List<string>();
    private List<string> selectedInformationKeys = new List<string>(); // Format: "NPCName-Key"
    private List<GameObject> spawnedEvidenceCards = new List<GameObject>();
    private List<GameObject> spawnedInfoCards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (caseFilePanel != null) caseFilePanel.SetActive(false);
    }

    void Start()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
    }

    public void OpenCaseFile()
    {
        if (caseFilePanel != null) caseFilePanel.SetActive(true);
        ResetSelection();
        RefreshUI();
    }

    public void CloseCaseFile()
    {
        if (caseFilePanel != null) caseFilePanel.SetActive(false);
    }

    void ResetSelection()
    {
        selectedEvidenceNames.Clear();
        selectedInformationKeys.Clear();
        UpdateSelectionUI();
    }

    void RefreshUI()
    {
        RefreshEvidence();
        RefreshInformation();
        UpdateSelectionUI();
    }

    /// <summary>
    /// Hiển thị evidence từ EvidenceManager
    /// </summary>
    void RefreshEvidence()
    {
        // Xóa các card cũ
        foreach (GameObject card in spawnedEvidenceCards)
            Destroy(card);
        spawnedEvidenceCards.Clear();

        if (EvidenceManager.Instance == null) return;

        foreach (string evName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evName == "Hide") continue;

            GameObject card = Instantiate(evidenceCardPrefab, evidenceGrid);
            spawnedEvidenceCards.Add(card);

            // Set icon
            Image icon = card.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
                icon.sprite = EvidenceManager.Instance.GetEvidenceSprite(evName);

            // Set tên
            TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = GetEvidenceName(evName);

            // Checkmark (ban đầu tắt)
            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) checkmark.gameObject.SetActive(false);

            // Gán sự kiện click
            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ToggleEvidence(evName, card));
        }
    }

    /// <summary>
    /// Hiển thị information từ 4 dictionary của DialogueManager
    /// </summary>
    void RefreshInformation()
    {
        // Xóa các card cũ
        foreach (GameObject card in spawnedInfoCards)
            Destroy(card);
        spawnedInfoCards.Clear();

        if (DialogueManager.Instance == null) return;

        // Lấy thông tin từ 4 dictionary
        CreateInfoCards("Sang", DialogueManager.Instance.Sang);
        CreateInfoCards("Mai", DialogueManager.Instance.Mai);
        CreateInfoCards("Tan", DialogueManager.Instance.Tan);
        CreateInfoCards("May", DialogueManager.Instance.May);
    }

    void CreateInfoCards(string npcName, Dictionary<int, string> infoDict)
    {
        foreach (var kvp in infoDict)
        {
            int key = kvp.Key;
            string value = kvp.Value;
            string uniqueKey = $"{npcName}-{key}"; // Format: "Sang-1", "Mai-3"

            GameObject card = Instantiate(informationCardPrefab, informationGrid);
            spawnedInfoCards.Add(card);

            // Set portrait (lấy từ ProfileUI)
            Image portrait = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null && ProfileUI.Instance != null)
            {
                int profileIndex = GetProfileIndex(npcName);
                if (profileIndex >= 0 && profileIndex < ProfileUI.Instance.characterPortraits.Length)
                {
                    portrait.sprite = ProfileUI.Instance.characterPortraits[profileIndex];
                }
            }

            // Set text (NPC name + info)
            TextMeshProUGUI infoText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (infoText != null)
            {
                // Rút gọn text nếu quá dài
                string displayText = value.Length > 50 ? value.Substring(0, 47) + "..." : value;
                infoText.text = $"<b>{npcName}:</b> {displayText}";
            }

            // Checkmark (ban đầu tắt)
            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) checkmark.gameObject.SetActive(false);

            // Gán sự kiện click
            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ToggleInformation(uniqueKey, card));
        }
    }

    int GetProfileIndex(string npcName)
    {
        switch (npcName)
        {
            case "Sang": return 0;
            case "Mai": return 1;
            case "Tan": return 2;
            case "May": return 3;
            default: return -1;
        }
    }

    /// <summary>
    /// Toggle chọn/bỏ chọn evidence
    /// </summary>
    void ToggleEvidence(string evName, GameObject card)
    {
        Transform checkmark = card.transform.Find("Checkmark");
        int totalSelected = selectedEvidenceNames.Count + selectedInformationKeys.Count;

        if (selectedEvidenceNames.Contains(evName))
        {
            // Bỏ chọn
            selectedEvidenceNames.Remove(evName);
            if (checkmark != null) checkmark.gameObject.SetActive(false);
        }
        else
        {
            // Chọn (nếu chưa đạt giới hạn)
            if (totalSelected >= maxSelection)
            {
                Debug.Log($"Đã đạt giới hạn! Tối đa {maxSelection} items.");
                return;
            }

            selectedEvidenceNames.Add(evName);
            if (checkmark != null) checkmark.gameObject.SetActive(true);
        }

        UpdateSelectionUI();
    }

    /// <summary>
    /// Toggle chọn/bỏ chọn information
    /// </summary>
    void ToggleInformation(string uniqueKey, GameObject card)
    {
        Transform checkmark = card.transform.Find("Checkmark");
        int totalSelected = selectedEvidenceNames.Count + selectedInformationKeys.Count;

        if (selectedInformationKeys.Contains(uniqueKey))
        {
            // Bỏ chọn
            selectedInformationKeys.Remove(uniqueKey);
            if (checkmark != null) checkmark.gameObject.SetActive(false);
        }
        else
        {
            // Chọn (nếu chưa đạt giới hạn)
            if (totalSelected >= maxSelection)
            {
                Debug.Log($"Đã đạt giới hạn! Tối đa {maxSelection} items.");
                return;
            }

            selectedInformationKeys.Add(uniqueKey);
            if (checkmark != null) checkmark.gameObject.SetActive(true);
        }

        UpdateSelectionUI();
    }

    void UpdateSelectionUI()
    {
        int totalSelected = selectedEvidenceNames.Count + selectedInformationKeys.Count;

        // Hiển thị số lượng đã chọn
        if (selectedCountText != null)
        {
            selectedCountText.text = $"Selected: {totalSelected}/{maxSelection} " +
                                    $"(Evidence: {selectedEvidenceNames.Count}, Info: {selectedInformationKeys.Count})";
        }

        // Cập nhật instruction text
        if (instructionText != null)
        {
            if (totalSelected < minSelection)
            {
                instructionText.text = $"Select at least {minSelection} items to submit your case.";
            }
            else
            {
                instructionText.text = "Ready to submit. Choose carefully!";
            }
        }

        // Bật/tắt nút Submit
        if (submitButton != null)
        {
            bool canSubmit = totalSelected >= minSelection;
            submitButton.interactable = canSubmit;

            if (submitButtonText != null)
            {
                submitButtonText.text = canSubmit ? "Submit Case" : "Select Items First";
            }
        }
    }

    void OnSubmitClicked()
    {
        int totalSelected = selectedEvidenceNames.Count + selectedInformationKeys.Count;
        
        if (totalSelected < minSelection)
        {
            Debug.LogWarning("Chưa chọn đủ items!");
            return;
        }

        ProcessCaseSubmission();
    }

    void ProcessCaseSubmission()
    {
        Debug.Log("=== CASE SUBMITTED ===");
        Debug.Log($"Total Selected: {selectedEvidenceNames.Count + selectedInformationKeys.Count}");
        
        Debug.Log("Evidence:");
        foreach (string ev in selectedEvidenceNames)
        {
            Debug.Log($"  - {ev}");
        }

        Debug.Log("Information:");
        foreach (string info in selectedInformationKeys)
        {
            Debug.Log($"  - {info}");
        }

        CloseCaseFile();

        // TODO: Xử lý ending ở script khác (EndingManager, GameManager, etc.)
        // EndingManager.Instance.DetermineEnding(selectedEvidenceNames, selectedInformationKeys);
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
            case "StrangeTable": return "Four chairs around the table";
            case "OpenWindow": return "Open window in the attic";
            case "Rope": return "Rope in the attic";
            case "Limit1": return "Footprint in the attic";
            case "Limit2": return "Hang the Ghost";
            case "Limit3": return "Mai's Diary";
            case "Limit4": return "Family Photo";
            case "Limit5": return "May's Diary";
            case "Limit6": return "The ghost mom";
            case "Hide": return "Hide";
            case "SangStuff": return "Mr.Sang precious duck";
            default: return "Unknown Evidence";
        }
    }

    // === API Helper Functions ===

    public bool HasEvidence(string evidenceName)
    {
        return selectedEvidenceNames.Contains(evidenceName);
    }

    public bool HasInformation(string npcName, int key)
    {
        return selectedInformationKeys.Contains($"{npcName}-{key}");
    }

    public List<string> GetSelectedEvidence()
    {
        return new List<string>(selectedEvidenceNames);
    }

    public List<string> GetSelectedInformation()
    {
        return new List<string>(selectedInformationKeys);
    }

    public int GetTotalSelectedCount()
    {
        return selectedEvidenceNames.Count + selectedInformationKeys.Count;
    }
}
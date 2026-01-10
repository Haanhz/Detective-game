using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CaseFileUI : MonoBehaviour
{
    public static CaseFileUI Instance;

    [Header("Panel Reference")]
    public GameObject caseFilePanel;

    [Header("Accusation UI")]
    public Image npcAvatarImage;
    public TextMeshProUGUI npcDialogueText;
    
    [Header("Evidence Section")]
    public Transform evidenceGrid;
    public GameObject evidenceCardPrefab;
    
    [Header("Information Section")]
    public Transform informationGrid;
    public GameObject informationCardPrefab;

    [Header("Buttons")]
    public Button submitButton;
    public Button closeButton;
    public TextMeshProUGUI submitButtonText;

    // Accusation data
    private int currentPhase = 0;
    private bool accusationMode = false;
    private NPC accusedNPC; // Lưu NPC đang bị buộc tội
    
    // Dữ liệu tích lũy qua các phase (để EndingManager check)
    private List<string> accumulatedEvidenceNames = new List<string>();
    private List<string> accumulatedInformationKeys = new List<string>();
    
    // Selections của phase hiện tại
    private List<string> currentPhaseEvidenceNames = new List<string>();
    private List<string> currentPhaseInformationKeys = new List<string>();
    
    private List<GameObject> spawnedEvidenceCards = new List<GameObject>();
    private List<GameObject> spawnedInfoCards = new List<GameObject>();

    // Accusation phases
    private static readonly AccusationPhase[] phases = new AccusationPhase[]
    {
        new AccusationPhase
        {
            dialogue = "Me? The murderer? You're talking nonsense… There's a rope mark on Hang's neck, tell me where I got the rope from?",
            requiredEvidence = new string[] { "Rope" },
            requiredInfo = new string[] { },
            wrongEndings = new string[] { "HangPhone", "HangNoteBook", "Tan-0", "Tan-2","Mai-1","Mai-2" },
            correctResponse = "The rope in the attic? I've never been up there!",
            wrongResponse = "What are you even talking about? That proves nothing!"
        },
        new AccusationPhase
        {
            dialogue = "The rope in the attic? I've never been up there!",
            requiredEvidence = new string[] { "Crack", "OpenWindow" },
            requiredInfo = new string[] { "May-1", "Tan-1" },
            optionalEvidence = new string[] { "Limit1" },
            wrongEndings = new string[] {"HangPhone", "HangNoteBook", "Tan-0", "Tan-2","Mai-1","Mai-2"  },
            correctResponse = "Uh... fine, maybe I was there... but tell me, where is the murder weapon now?",
            wrongResponse = "Ridiculous! That doesn't prove I went in there!"
        },
        new AccusationPhase
        {
            dialogue = "Tell me, where is the murder weapon now?",
            requiredEvidence = new string[] { },
            requiredInfo = new string[] { "Tan-2", "Mai-3" },
            wrongEndings = new string[] {"HangPhone", "HangNoteBook", "Tan-0","Mai-1","Mai-2" },
            correctResponse = "I had no reason to kill Hang, why would I do that?",
            wrongResponse = "You can't find the weapon! This case has nothing to do with me!"
        },
        new AccusationPhase
        {
            dialogue = "I had no reason to kill Hang, why would I do that?",
            requiredEvidence = new string[] { },
            requiredInfo = new string[] { },
            optionalEvidence = new string[] { "Limit2", "Limit3", "Limit4", "Limit5", "Limit6" },
            wrongEndings = new string[] {"HangPhone", "HangNoteBook", "Tan-0", "Tan-2","Mai-1","Mai-2"  },
            correctResponse = "You... you know too much already...",
            wrongResponse = "That's just speculation! You have no real evidence!"
        }
    };

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
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCaseFile);
    }

    // ===== NORMAL MODE (Xem evidence/info thường) =====
    
    public void OpenCaseFile()
    {
        accusationMode = false;
        
        if (caseFilePanel != null) caseFilePanel.SetActive(true);
        
        // Ẩn accusation UI
        if (npcAvatarImage != null) npcAvatarImage.gameObject.SetActive(false);
        if (npcDialogueText != null) npcDialogueText.gameObject.SetActive(false);
        
        // Hiển thị tất cả evidence/info (không filter)
        RefreshEvidenceNormalMode();
        RefreshInformationNormalMode();
        
        // Update button
        if (submitButtonText != null)
            submitButtonText.text = "Close";
    }
    
    // ===== ACCUSATION MODE =====
    
    public void StartAccusation(NPC npc)
    {
        accusationMode = true;
        currentPhase = 0;
        accusedNPC = npc;
        
        // Reset tất cả selections
        accumulatedEvidenceNames.Clear();
        accumulatedInformationKeys.Clear();
        currentPhaseEvidenceNames.Clear();
        currentPhaseInformationKeys.Clear();
        
        if (caseFilePanel != null) caseFilePanel.SetActive(true);
        
        // Hiện accusation UI
        if (npcAvatarImage != null) 
        {
            npcAvatarImage.sprite = npc.portrait;
            npcAvatarImage.gameObject.SetActive(true);
        }
        if (npcDialogueText != null) npcDialogueText.gameObject.SetActive(true);
        
        ShowPhase(currentPhase);
    }

    void ShowPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Length)
        {
            // Hết phases -> check ending
            FinishAccusation();
            return;
        }

        AccusationPhase phase = phases[phaseIndex];
        
        // Update dialogue ngay lập tức
        if (npcDialogueText != null)
            npcDialogueText.text = phase.dialogue;
        
        // Clear selections của phase hiện tại (giữ nguyên accumulated)
        currentPhaseEvidenceNames.Clear();
        currentPhaseInformationKeys.Clear();
        
        // Hiển thị evidence & info cards
        RefreshEvidence();
        RefreshInformation();
        
        // Update button - DISABLE cho đến khi chọn gì đó
        if (submitButtonText != null)
            submitButtonText.text = "Continue";
        if (submitButton != null)
            submitButton.interactable = false;
    }

    void OnSubmitClicked()
    {
        if (!accusationMode)
        {
            CloseCaseFile();
            return;
        }

        AccusationPhase phase = phases[currentPhase];
        
        // ===== KIỂM TRA 1: Có chọn thông tin WRONG không? =====
        bool hasWrongInfo = false;
        foreach (string wrong in phase.wrongEndings)
        {
            if (wrong.Contains("-")) // Info
            {
                if (currentPhaseInformationKeys.Contains(wrong))
                {
                    hasWrongInfo = true;
                    break;
                }
            }
            else // Evidence
            {
                if (currentPhaseEvidenceNames.Contains(wrong))
                {
                    hasWrongInfo = true;
                    break;
                }
            }
        }

        if (hasWrongInfo)
        {
            // Tích lũy selections trước khi kết thúc
            AccumulateCurrentSelections();
            
            // Hiện phản ứng sai
            if (npcDialogueText != null)
                npcDialogueText.text = phase.wrongResponse;
            
            // Delay rồi show WRONG ending
            StartCoroutine(DelayedEnding(1.5f, "WRONG"));
            return;
        }

        // ===== KIỂM TRA 2: Có chọn ít nhất 1 evidence/info ĐÚNG không? =====
        bool hasAnyCorrect = false;
        
        // Check evidence đúng
        foreach (string ev in phase.requiredEvidence)
        {
            if (currentPhaseEvidenceNames.Contains(ev))
            {
                hasAnyCorrect = true;
                break;
            }
        }
        
        // Check info đúng
        if (!hasAnyCorrect)
        {
            foreach (string info in phase.requiredInfo)
            {
                if (currentPhaseInformationKeys.Contains(info))
                {
                    hasAnyCorrect = true;
                    break;
                }
            }
        }
        
        // Check optional evidence (vẫn tính là đúng)
        if (!hasAnyCorrect && phase.optionalEvidence != null)
        {
            foreach (string opt in phase.optionalEvidence)
            {
                if (currentPhaseEvidenceNames.Contains(opt))
                {
                    hasAnyCorrect = true;
                    break;
                }
            }
        }

        if (!hasAnyCorrect)
        {
            // Tích lũy selections trước khi kết thúc
            AccumulateCurrentSelections();
            
            // Hiện phản ứng sai
            if (npcDialogueText != null)
                npcDialogueText.text = phase.wrongResponse;
            
            // Delay rồi show NOBODY ending
            StartCoroutine(DelayedEnding(1.5f, "NOBODY"));
            return;
        }

        // ===== ĐÚNG -> Tích lũy selections và chuyển phase =====
        AccumulateCurrentSelections();
        
        // Hiện phản ứng đúng
        if (npcDialogueText != null)
            npcDialogueText.text = phase.correctResponse;
        
        // Delay rồi chuyển phase
        StartCoroutine(DelayedPhaseChange(1.5f));
    }
    
    void AccumulateCurrentSelections()
    {
        // Thêm selections của phase hiện tại vào accumulated (không trùng lặp)
        foreach (string ev in currentPhaseEvidenceNames)
        {
            if (!accumulatedEvidenceNames.Contains(ev))
                accumulatedEvidenceNames.Add(ev);
        }
        
        foreach (string info in currentPhaseInformationKeys)
        {
            if (!accumulatedInformationKeys.Contains(info))
                accumulatedInformationKeys.Add(info);
        }
    }
    
    System.Collections.IEnumerator DelayedPhaseChange(float delay)
    {
        if (submitButton != null) submitButton.interactable = false;
        
        yield return new WaitForSeconds(delay);
        
        if (submitButton != null) submitButton.interactable = true;
        
        currentPhase++;
        ShowPhase(currentPhase);
    }
    
    System.Collections.IEnumerator DelayedEnding(float delay, string endingType)
    {
        if (submitButton != null) submitButton.interactable = false;
        
        yield return new WaitForSeconds(delay);
        
        FinishAccusation(endingType);
    }

    void FinishAccusation(string forcedEndingType = null)
    {
        CloseCaseFile();
        
        if (EndingManager.Instance != null)
        {
            if (forcedEndingType == "WRONG")
            {
                // Force WRONG ending
                EndingManager.Instance.ShowWrongEnding();
            }
            else if (forcedEndingType == "NOBODY")
            {
                // Force NOBODY ending
                EndingManager.Instance.ShowNobodyEnding();
            }
            else
            {
                // Hoàn thành hết phases -> để EndingManager check FULL/HALF
                EndingManager.Instance.ShowEnding(playerDead: false);
            }
        }
    }

    public void CloseCaseFile()
    {
        if (caseFilePanel != null) caseFilePanel.SetActive(false);
        accusationMode = false;
    }

    // ===== REFRESH UI (NORMAL MODE - Hiển thị tất cả) =====
    
    void RefreshEvidenceNormalMode()
    {
        foreach (GameObject card in spawnedEvidenceCards)
            Destroy(card);
        spawnedEvidenceCards.Clear();

        if (EvidenceManager.Instance == null) return;

        foreach (string evName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evName == "Hide") continue;

            GameObject card = Instantiate(evidenceCardPrefab, evidenceGrid);
            spawnedEvidenceCards.Add(card);

            Image icon = card.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
                icon.sprite = EvidenceManager.Instance.GetEvidenceSprite(evName);

            TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = GetEvidenceName(evName);

            // Normal mode - không có checkmark, không clickable
            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) 
                checkmark.gameObject.SetActive(false);

            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.interactable = false; // Không cho click trong normal mode
        }
    }

    void RefreshInformationNormalMode()
    {
        foreach (GameObject card in spawnedInfoCards)
            Destroy(card);
        spawnedInfoCards.Clear();

        if (DialogueManager.Instance == null) return;

        CreateInfoCardsNormalMode("Sang", DialogueManager.Instance.Sang);
        CreateInfoCardsNormalMode("Mai", DialogueManager.Instance.Mai);
        CreateInfoCardsNormalMode("Tan", DialogueManager.Instance.Tan);
        CreateInfoCardsNormalMode("May", DialogueManager.Instance.May);
    }

    void CreateInfoCardsNormalMode(string npcName, Dictionary<int, string> infoDict)
    {
        foreach (var kvp in infoDict)
        {
            int key = kvp.Key;
            string value = kvp.Value;

            GameObject card = Instantiate(informationCardPrefab, informationGrid);
            spawnedInfoCards.Add(card);

            Image portrait = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null && ProfileUI.Instance != null)
            {
                int profileIndex = GetProfileIndex(npcName);
                if (profileIndex >= 0 && profileIndex < ProfileUI.Instance.characterPortraits.Length)
                {
                    portrait.sprite = ProfileUI.Instance.characterPortraits[profileIndex];
                }
            }

            TextMeshProUGUI infoText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (infoText != null)
            {
                string displayText = value.Length > 50 ? value.Substring(0, 47) + "..." : value;
                infoText.text = $"<b>{npcName}:</b> {displayText}";
            }

            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) 
                checkmark.gameObject.SetActive(false);

            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.interactable = false;
        }
    }
    
    // ===== REFRESH UI (ACCUSATION MODE - Chỉ hiển thị chưa chọn) =====
    
    void RefreshEvidence()
    {
        foreach (GameObject card in spawnedEvidenceCards)
            Destroy(card);
        spawnedEvidenceCards.Clear();

        if (EvidenceManager.Instance == null) return;

        foreach (string evName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evName == "Hide") continue;
            
            // Bỏ qua evidence đã chọn ở phase trước
            if (accumulatedEvidenceNames.Contains(evName)) continue;

            GameObject card = Instantiate(evidenceCardPrefab, evidenceGrid);
            spawnedEvidenceCards.Add(card);

            Image icon = card.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
                icon.sprite = EvidenceManager.Instance.GetEvidenceSprite(evName);

            TextMeshProUGUI nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = GetEvidenceName(evName);

            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) 
                checkmark.gameObject.SetActive(currentPhaseEvidenceNames.Contains(evName));

            Button btn = card.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ToggleEvidence(evName, card));
        }
    }

    void RefreshInformation()
    {
        foreach (GameObject card in spawnedInfoCards)
            Destroy(card);
        spawnedInfoCards.Clear();

        if (DialogueManager.Instance == null) return;

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
            string uniqueKey = $"{npcName}-{key}";
            
            // Bỏ qua info đã chọn ở phase trước
            if (accumulatedInformationKeys.Contains(uniqueKey)) continue;

            GameObject card = Instantiate(informationCardPrefab, informationGrid);
            spawnedInfoCards.Add(card);

            Image portrait = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null && ProfileUI.Instance != null)
            {
                int profileIndex = GetProfileIndex(npcName);
                if (profileIndex >= 0 && profileIndex < ProfileUI.Instance.characterPortraits.Length)
                {
                    portrait.sprite = ProfileUI.Instance.characterPortraits[profileIndex];
                }
            }

            TextMeshProUGUI infoText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (infoText != null)
            {
                string displayText = value.Length > 50 ? value.Substring(0, 47) + "..." : value;
                infoText.text = $"<b>{npcName}:</b> {displayText}";
            }

            Transform checkmark = card.transform.Find("Checkmark");
            if (checkmark != null) 
                checkmark.gameObject.SetActive(currentPhaseInformationKeys.Contains(uniqueKey));

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

    void ToggleEvidence(string evName, GameObject card)
    {
        Transform checkmark = card.transform.Find("Checkmark");

        if (currentPhaseEvidenceNames.Contains(evName))
        {
            // Bỏ chọn
            currentPhaseEvidenceNames.Remove(evName);
            if (checkmark != null) checkmark.gameObject.SetActive(false);
        }
        else
        {
            // Thêm vào selection (cho phép nhiều evidence)
            currentPhaseEvidenceNames.Add(evName);
            if (checkmark != null) checkmark.gameObject.SetActive(true);
        }
        
        // Enable/Disable button dựa vào có chọn gì không
        UpdateSubmitButton();
    }

    void ToggleInformation(string uniqueKey, GameObject card)
    {
        Transform checkmark = card.transform.Find("Checkmark");

        if (currentPhaseInformationKeys.Contains(uniqueKey))
        {
            // Bỏ chọn
            currentPhaseInformationKeys.Remove(uniqueKey);
            if (checkmark != null) checkmark.gameObject.SetActive(false);
        }
        else
        {
            // Thêm vào selection (cho phép nhiều info)
            currentPhaseInformationKeys.Add(uniqueKey);
            if (checkmark != null) checkmark.gameObject.SetActive(true);
        }
        
        // Enable/Disable button dựa vào có chọn gì không
        UpdateSubmitButton();
    }
    
    void UpdateSubmitButton()
    {
        bool hasSelection = currentPhaseEvidenceNames.Count > 0 || currentPhaseInformationKeys.Count > 0;
        if (submitButton != null)
            submitButton.interactable = hasSelection;
    }
    
    // Helper: Lấy tag từ display name
    string GetEvidenceTag(string displayName)
    {
        switch (displayName)
        {
            case "Living Corner": return "LivingCorner";
            case "Ultimatum": return "Ultimatum";
            case "Hang's Phone": return "HangPhone";
            case "Hang's Notebook": return "HangNoteBook";
            case "Crack outside the attic": return "Crack";
            case "Strange table": return "StrangeTable";
            case "Open window in the attic": return "OpenWindow";
            case "Rope in the attic": return "Rope";
            case "Footprint in the attic": return "Limit1";
            case "Hang the Ghost": return "Limit2";
            case "Mai's Diary": return "Limit3";
            case "Family Photo": return "Limit4";
            case "May's Diary": return "Limit5";
            case "The ghost mom": return "Limit6";
            case "Mr.Sang precious duck": return "SangStuff";
            case "Key": return "Key";
            default: return displayName;
        }
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
            case "StrangeTable": return "Strange table";
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
            case "Key": return "Key";
            default: return "Unknown Evidence";
        }
    }

    // ===== API Helper Functions cho EndingManager =====

    public bool HasEvidence(string evidenceName)
    {
        return accumulatedEvidenceNames.Contains(evidenceName);
    }

    public bool HasInformation(string npcName, int key)
    {
        return accumulatedInformationKeys.Contains($"{npcName}-{key}");
    }

    public List<string> GetSelectedEvidence()
    {
        return new List<string>(accumulatedEvidenceNames);
    }

    public List<string> GetSelectedInformation()
    {
        return new List<string>(accumulatedInformationKeys);
    }

    public int GetTotalSelectedCount()
    {
        return accumulatedEvidenceNames.Count + accumulatedInformationKeys.Count;
    }
}

[System.Serializable]
public class AccusationPhase
{
    public string dialogue;
    public string[] requiredEvidence;
    public string[] requiredInfo;
    public string[] optionalEvidence;
    public string[] wrongEndings;
    public string correctResponse;
    public string wrongResponse;
}
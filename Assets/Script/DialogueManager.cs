using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public AudioSource typeAudioSource;
    public AudioClip typewriterSound;
    public static DialogueManager Instance;
    public GameObject player;
    public GameObject dialogueBox;
    public TextMeshProUGUI DialogueText;

    public GameObject StartConversationButton;
    public GameObject PointMurderButton;
    public TextMeshProUGUI NameText;
    public Image AvatarImage;

    [Header("Player Settings")]
    public string playerName = "Player";
    public Sprite playerPortrait;

    [Header("Settings")]
    public float interactionDistance = 3f;
    public float textSpeed = 0.05f;

    private int index;
    private NPC currentNPC;
    private bool isInteracting = false;

    // Biến static để script di chuyển của Player có thể truy cập mà không cần reference phức tạp
    public static bool IsMenuOpen = false;

    private bool chooseRightMurderer = false;

    public Dictionary<int, string> Sang = new Dictionary<int, string>();
    public Dictionary<int, string> Mai = new Dictionary<int, string>();
    public Dictionary<int, string> Tan = new Dictionary<int, string>();
    public Dictionary<int, string> May = new Dictionary<int, string>();
    private NPC.DialogueBlock currentBlock;
    private NPC.DialogueLine[] currentLines;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        DialogueText.text = string.Empty;
        dialogueBox.SetActive(false);
        StartConversationButton.SetActive(false);
        PointMurderButton.SetActive(false);

        if (NameText != null) NameText.text = string.Empty;
        if (AvatarImage != null) AvatarImage.gameObject.SetActive(false);
    }

    void ChooseRightMurderer(string NPCTag)
    {
        if (NPCTag == "Murder")
        {
            chooseRightMurderer = true;
        }
        else chooseRightMurderer = false;
    }

    void Update()
    {
        if (GameManager.Instance.isNight)
        {
            if (isInteracting) CleanupState();
            if (IsMenuOpen) CleanupState();
            return;
        }

        // Chặn phím mũi tên khi đang mở Menu để ép người dùng dùng W/S (tùy chọn)
        // Hoặc đơn giản là để tránh xung đột di chuyển
        if (IsMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(currentSelected);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
                if (selectedButton != null)
                {
                    // Giả lập hành động Click chuột vào Button đang được chọn
                    ExecuteEvents.Execute(selectedButton, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                    return; // Thoát để tránh xung đột với logic F ở dưới
                }
            }
        }

        currentNPC = FindClosestNPC();

        if (currentNPC != null)
        {
            ChooseRightMurderer(currentNPC.tag);
        }

        if (isInteracting)
        {
            if (dialogueBox.activeInHierarchy && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return)))
            {
                if (DialogueText.text == currentLines[index].text)
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    DialogueText.text = currentLines[index].text;
                }
            }
            return;
        }

        if (currentNPC != null)
        {
            if (Input.GetKeyDown(KeyCode.F) && !IsMenuOpen)
            {
                OpenSelectionMenu(currentNPC);
            }
        }
        else
        {
            if (IsMenuOpen || StartConversationButton.activeInHierarchy)
            {
                CleanupState();
            }
        }
    }

    void OpenSelectionMenu(NPC npc)
    {
        if (npc == null) return;

        IsMenuOpen = true; // Kích hoạt trạng thái mở Menu
        dialogueBox.SetActive(true);
        StartConversationButton.SetActive(true);
        PointMurderButton.SetActive(true);

        if (NameText != null) NameText.text = npc.npcName;
        if (AvatarImage != null && npc.portrait != null)
        {
            AvatarImage.sprite = npc.portrait;
            AvatarImage.gameObject.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(StartConversationButton);
    }

    public void UpdateImportantInfo(Dictionary<int, string> dict, int key, string value, bool condition)
    {
        if (condition)
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
    }

    public bool HasImportantInfo(string npcName, int key)
    {
        switch (npcName)
        {
            case "Sang": return Sang.ContainsKey(key);
            case "Mai": return Mai.ContainsKey(key);
            case "Tan": return Tan.ContainsKey(key);
            case "May": return May.ContainsKey(key);
            default: return false;
        }
    }

    public bool CheckEndingConversation()
    {
        bool tanCondition = Tan.ContainsKey(1) && Tan.ContainsKey(2);
        bool mayCondition = May.ContainsKey(1);
        bool maiCondition = Mai.ContainsKey(3);
        return tanCondition && mayCondition && maiCondition;
    }

    public bool ChooseRightMurderer()
    {
        return chooseRightMurderer;
    }

    public void OnStartConversationButtonClicked()
    {
        if (currentNPC != null && !isInteracting)
        {
            IsMenuOpen = false; // Tắt menu để bắt đầu hội thoại
            CharacterUnlockManager.UnlockCharacter(currentNPC.profileIndex);
            isInteracting = true;
            StartConversationButton.SetActive(false);
            PointMurderButton.SetActive(false);

            DialogueText.text = string.Empty;
            StartDialogue(currentNPC);
        }
    }

    private NPC FindClosestNPC()
    {
        NPC[] npcs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        NPC closestNPC = null;
        float minDistance = interactionDistance;

        foreach (NPC npc in npcs)
        {
            float distance = Vector3.Distance(player.transform.position, npc.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNPC = npc;
            }
        }
        return closestNPC;
    }

    public void StartDialogue(NPC npc)
    {
        NPC.DialogueBlock blockToPlay = null;
        if (npc.dialogueStage == -1 || npc.dialogueStage == 0) blockToPlay = npc.introBlock;
        else if (npc.dialogueStage == 1) blockToPlay = npc.followUpBlock;
        else blockToPlay = GetConditionalDialogue(npc);

        currentBlock = blockToPlay;
        currentLines = blockToPlay.lines;
        index = 0;

        StartCoroutine(TypeLine());
    }

    void UpdateSpeakerUI(NPC.DialogueLine line)
    {
        if (line.speaker == NPC.DialogueLine.Speaker.NPC)
        {
            NameText.text = currentNPC.npcName;
            AvatarImage.sprite = currentNPC.portrait;
        }
        else
        {
            NameText.text = playerName;
            AvatarImage.sprite = playerPortrait;
        }
        AvatarImage.gameObject.SetActive(true);
    }

    IEnumerator TypeLine()
    {
        if (currentLines == null || currentLines.Length == 0)
        {
            FinishDialogueSequence();
            yield break;
        }

        UpdateSpeakerUI(currentLines[index]);
        DialogueText.text = "";
        if (typeAudioSource != null && typewriterSound != null)
        {
            typeAudioSource.clip = typewriterSound;
            if (!typeAudioSource.isPlaying)
                typeAudioSource.Play();
        }

        foreach (char c in currentLines[index].text.ToCharArray())
        {
            DialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        if (typeAudioSource != null && typeAudioSource.isPlaying)
        {
            typeAudioSource.Stop();
        }
    }

    void NextLine()
    {
        if (index < currentLines.Length - 1)
        {
            index++;
            DialogueText.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            FinishDialogueSequence();
        }
    }

    int GetProfileIndexByName(string name)
    {
        switch (name)
        {
            case "Sang": return 0;
            case "Mai": return 1;
            case "Tan": return 2;
            case "May": return 3;
            default: return -1;
        }
    }

    void FinishDialogueSequence()
    {
        NPC savedNPC = currentNPC;

        if (currentBlock != null)
        {
            Dictionary<int, string> targetDict = null;
            switch (currentBlock.targetNPC)
            {
                case "Sang": targetDict = Sang; break;
                case "Mai": targetDict = Mai; break;
                case "Tan": targetDict = Tan; break;
                case "May": targetDict = May; break;
            }

            if (targetDict != null)
            {
                UpdateImportantInfo(targetDict, currentBlock.infoKey, currentBlock.infoValue, true);
            }

            int pIndex = GetProfileIndexByName(currentBlock.targetNPC);
            if (pIndex != -1 && ProfileUI.Instance != null)
            {
                ProfileUI.Instance.AddInfoToDescription(pIndex, currentBlock.infoValue);
            }
        }

        if (savedNPC != null && !savedNPC.lockDialogueStage)
        {
            // if (savedNPC.dialogueStage == -1)
            // {
            //     // Giữ nguyên stage -1, không tăng
            // }
            if (savedNPC.dialogueStage == 0) savedNPC.dialogueStage = 1;
            else if (savedNPC.dialogueStage == 1) savedNPC.dialogueStage = 2;
        }

        CleanupState();

        // if (savedNPC != null)
        // {
        //     OpenSelectionMenu(savedNPC);
        // }
    }

    NPC.DialogueBlock GetConditionalDialogue(NPC npc)
    {
        NPC.DialogueBlock lastReadBlock = null;

        foreach (var block in npc.conditionalBlocks)
        {
            // Check evidence conditions
            bool hasAllEvidence = true;
            foreach (string ev in block.requiredEvidenceTags)
            {
                if (!EvidenceManager.Instance.HasEvidence(ev))
                {
                    hasAllEvidence = false;
                    break;
                }
            }
            if (!hasAllEvidence)
            {
                // Nếu block này đã đọc rồi, lưu lại để backup
                if (block.hasRead) lastReadBlock = block;
                continue;
            }

            // Check NPC info conditions
            bool hasAllInfo = true;
            if (!string.IsNullOrEmpty(block.requiredNPC))
            {
                foreach (int key in block.requiredKeys)
                {
                    if (!HasImportantInfo(block.requiredNPC, key))
                    {
                        hasAllInfo = false;
                        break;
                    }
                }
            }
            if (!hasAllInfo)
            {
                // Nếu block này đã đọc rồi, lưu lại để backup
                if (block.hasRead) lastReadBlock = block;
                continue;
            }

            if (!hasAllEvidence || !hasAllInfo)
            {
                // CHỈ lưu làm backup nếu thực sự ĐÃ ĐỌC và KHÔNG thỏa mãn điều kiện hiện tại
                // Nhưng tốt nhất là bỏ qua để tìm các block khác thỏa mãn hơn
                continue; 
            }

            // Block này thỏa mãn tất cả điều kiện
            if (!block.hasRead)
            {
                // Block mới chưa đọc -> đánh dấu và trả về
                block.hasRead = true;
                return block;
            }
            else
            {
                // Block đã đọc nhưng vẫn thỏa mãn -> lưu lại
                lastReadBlock = block;
            }
        }

        // Nếu có block cũ đã đọc -> trả về block đó (không đánh dấu lại)
        if (lastReadBlock != null)
        {
            return lastReadBlock;
        }

        // Không có block nào -> trả về followUpBlock
        return npc.followUpBlock;
    }
    public void OnPointMurderButtonClicked()
    {
        // Kết thúc mọi trạng thái hội thoại
        CleanupState();
        CaseFileUI.Instance.OpenCaseFile();

        // Báo cho game biết: GAME ĐÃ KẾT THÚC
        GameManager.Instance.gameEnded = true;
    }
    void CleanupState()
    {
        IsMenuOpen = false;
        isInteracting = false;
        dialogueBox.SetActive(false);
        StartConversationButton.SetActive(false);
        PointMurderButton.SetActive(false);
        StopAllCoroutines();
        DialogueText.text = string.Empty;

        if (NameText != null) NameText.text = string.Empty;
        if (AvatarImage != null) AvatarImage.gameObject.SetActive(false);

        currentLines = null;
        currentNPC = null;

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
}
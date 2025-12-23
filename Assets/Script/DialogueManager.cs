using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject player;
    public GameObject dialogueBox;
    public TextMeshProUGUI DialogueText;

    public GameObject StartConversationButton;
    public GameObject PointMurderButton;
    public TextMeshProUGUI NameText;
    public Image AvatarImage;

    // SETTINGS
    //public string NPCTag;
    public float interactionDistance = 3f;
    public float textSpeed = 0.05f;

    private string[] currentLines;
    private int index;
    private NPC currentNPC;
    private bool isInteracting = false;

    private bool chooseRightMurderer = false;

    public Dictionary<int, string> Sang = new Dictionary<int, string>();
    public Dictionary<int, string> Mai = new Dictionary<int, string>();
    public Dictionary<int, string> Tan = new Dictionary<int, string>();
    public Dictionary<int, string> May = new Dictionary<int, string>();
    private NPC.DialogueBlock currentBlock;

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

        // Đảm bảo UI ẩn ngay từ đầu
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
            if (isInteracting)
            {

                CleanupState();
            }
            return; // không cho dialogue hoạt động
        }
        // 1. Tìm NPC gần đó
        currentNPC = FindClosestNPC();

        if (currentNPC != null)
        {
            ChooseRightMurderer(currentNPC.tag);
        }
        // 2. Xử lý logic Hội thoại (Nếu đang tương tác, bỏ qua kiểm tra khoảng cách/hiện nút)
        if (isInteracting)
        {
            // Xử lý nhấp chuột (LMB) để chuyển dòng/kết thúc
            if (dialogueBox.activeInHierarchy && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)))
            {
                if (DialogueText.text == currentLines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    DialogueText.text = currentLines[index];
                }
            }
            return;
        }

        // --- Logic HIỂN THỊ NÚT, TÊN, ẢNH (Khi nhấn Z và không nói chuyện) ---

        if (currentNPC != null)
        {
            // NPC đang trong phạm vi tương tác, hội thoại chưa bắt đầu

            // KIỂM TRA PHÍM Z ĐỂ HIỆN NÚT VÀ THÔNG TIN CÁ NHÂN CỦA NPC
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!StartConversationButton.activeInHierarchy)
                {
                    // LẤY DỮ LIỆU TỪ NPC GẦN NHẤT
                    dialogueBox.SetActive(true);
                    StartConversationButton.SetActive(true);
                    PointMurderButton.SetActive(true);

                    // HIỂN THỊ TÊN VÀ ẢNH CÙNG LÚC VỚI NÚT
                    if (NameText != null) NameText.text = currentNPC.npcName;
                    if (AvatarImage != null && currentNPC.portrait != null)
                    {
                        AvatarImage.sprite = currentNPC.portrait;
                        AvatarImage.gameObject.SetActive(true);
                    }
                }
            }
        }
        else // Người chơi đi xa hoặc không có NPC gần
        {
            // Nếu nút hoặc thông tin hiển thị đang hoạt động, tắt tất cả
            if (StartConversationButton.activeInHierarchy || (NameText != null && !string.IsNullOrEmpty(NameText.text)))
            {
                CleanupState();
            }
        }
    }

    public void UpdateImportantInfo(Dictionary<int, string> dict, int key, string value, bool condition)
    {
        if (condition)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value; // update nếu key đã tồn tại
            }
            else
            {
                dict.Add(key, value); // thêm mới nếu key chưa có
            }
        }
    }

        bool HasImportantInfo(string npcName, int key)
    {
        switch (npcName)
        {
            case "Sang": return Sang.ContainsKey(key);
            case "Mai":  return Mai.ContainsKey(key);
            case "Tan":  return Tan.ContainsKey(key);
            case "May":  return May.ContainsKey(key);
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
        currentNPC = FindClosestNPC();

        if (currentNPC != null && !isInteracting)
        {
            CharacterUnlockManager.UnlockCharacter(currentNPC.profileIndex);
            isInteracting = true;
            StartConversationButton.SetActive(false);
            PointMurderButton.SetActive(false);

            // dialogueBox đã được bật khi nhấn Z

            // ẨN TÊN VÀ ẢNH ĐẠI DIỆN KHI BẮT ĐẦU HỘI THOẠI
            if (NameText != null)
            {
                NameText.text = string.Empty; // Xóa text
            }
            if (AvatarImage != null)
            {
                AvatarImage.gameObject.SetActive(false); // Ẩn GameObject
            }

            // Xóa DialogueText để bắt đầu gõ dòng thoại
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

    void StartDialogue(NPC npc)
    {
        NPC.DialogueBlock blockToPlay = null;

        if (npc.dialogueStage == 0)
        {
            blockToPlay = npc.introBlock;
        }
        else if (npc.dialogueStage == 1)
        {
            blockToPlay = npc.followUpBlock;
        }
        else
        {
            blockToPlay = GetConditionalDialogue(npc);
        }

        currentBlock = blockToPlay;
        currentLines = blockToPlay.lines;

        index = 0;
        DialogueText.text = "";
        StartCoroutine(TypeLine());
    }



    IEnumerator TypeLine()
    {
        if (currentLines.Length == 0)
        {
            FinishDialogueSequence();
            yield break;
        }

        foreach (char c in currentLines[index].ToCharArray())
        {
            DialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
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
        // Bạn cần đảm bảo thứ tự này khớp với mảng characterNames trong ProfileUI
        switch (name)
        {
            case "Sang": return 0; // Giả sử Sang ở vị trí 0
            case "Mai":  return 1;
            case "Tan":  return 2;
            case "May":  return 3;
            default: return -1;
        }
    }

    // Kết thúc chuỗi hội thoại
    void FinishDialogueSequence()
{
    // ===== 0. UPDATE IMPORTANT INFO =====
    if (currentBlock != null && currentBlock.infoKey != 0)
    {
        Dictionary<int, string> targetDict = null;

        switch (currentBlock.targetNPC)
        {
            case "Sang": targetDict = Sang; break;
            case "Mai":  targetDict = Mai;  break;
            case "Tan":  targetDict = Tan;  break;
            case "May":  targetDict = May;  break;
        }

        if (targetDict != null)
        {
            UpdateImportantInfo(
                targetDict,
                currentBlock.infoKey,
                currentBlock.infoValue,
                true
            );
        }

        int pIndex = GetProfileIndexByName(currentBlock.targetNPC);
        if (pIndex != -1 && ProfileUI.Instance != null)
        {
            ProfileUI.Instance.AddInfoToDescription(pIndex, currentBlock.infoValue);
        }
    }

    // ===== 1. UPDATE DIALOGUE STAGE =====
    if (currentNPC != null)
    {
        if (currentNPC.dialogueStage == 0)
        {
            currentNPC.dialogueStage = 1;
        }
        else if (currentNPC.dialogueStage == 1)
        {
            currentNPC.dialogueStage = 2;
        }
        // stage >= 2: KHÔNG tự tăng
    }

    // ===== 2. CLEANUP UI =====
    CleanupState();

    // ===== 3. HIỂN THỊ LẠI UI =====
    NPC npc = FindClosestNPC();
    if (npc != null)
    {
        dialogueBox.SetActive(true);
        StartConversationButton.SetActive(true);
        PointMurderButton.SetActive(true);

        if (NameText != null) NameText.text = npc.npcName;
        if (AvatarImage != null && npc.portrait != null)
        {
            AvatarImage.sprite = npc.portrait;
            AvatarImage.gameObject.SetActive(true);
        }
    }
}


    NPC.DialogueBlock GetConditionalDialogue(NPC npc)
    {
        foreach (var block in npc.conditionalBlocks)
        {
            if (block.readOnceOnly && block.hasRead)
                continue;

            foreach (string ev in block.requiredEvidenceTags)
            {
                if (!EvidenceManager.Instance.HasEvidence(ev))
                    goto NextBlock;
            }

            if (!string.IsNullOrEmpty(block.requiredNPC))
            {
                foreach (int key in block.requiredKeys)
                {
                    if (!HasImportantInfo(block.requiredNPC, key))
                        goto NextBlock;
                }
            }

            block.hasRead = true;
            return block;

        NextBlock:;
        }

        return npc.followUpBlock; // fallback đúng
    }





    // Dọn dẹp trạng thái chung (tắt tất cả UI liên quan)
    void CleanupState()
    {
        dialogueBox.SetActive(false);
        StartConversationButton.SetActive(false);
        PointMurderButton.SetActive(false);
        StopAllCoroutines();
        DialogueText.text = string.Empty;

        // XÓA TÊN VÀ ẢNH
        if (NameText != null) NameText.text = string.Empty;
        if (AvatarImage != null) AvatarImage.gameObject.SetActive(false);

        currentLines = null;
        currentNPC = null;
        isInteracting = false;
    }
}
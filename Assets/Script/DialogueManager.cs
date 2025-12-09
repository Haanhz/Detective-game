using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;


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

        if(currentNPC != null){
            ChooseRightMurderer(currentNPC.tag);
        }
        // 2. Xử lý logic Hội thoại (Nếu đang tương tác, bỏ qua kiểm tra khoảng cách/hiện nút)
        if (isInteracting)
        {
            // Xử lý nhấp chuột (LMB) để chuyển dòng/kết thúc
            if (dialogueBox.activeInHierarchy && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z)))
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
            if (Input.GetKeyDown(KeyCode.Z))
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

    public bool ChooseRightMurderer()
    {
        return chooseRightMurderer;
    }
    public void OnStartConversationButtonClicked()
    {
        currentNPC = FindClosestNPC();

        if (currentNPC != null && !isInteracting)
        {
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
        currentLines = npc.lines;
        index = 0;
        DialogueText.text = string.Empty;
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

    // Kết thúc chuỗi hội thoại
    void FinishDialogueSequence()
    {
        CleanupState();

        // Kiểm tra xem người chơi có còn gần NPC không để hiển thị lại thông tin tương tác
        if (FindClosestNPC() != null)
        {
            // Hiển thị lại nút, tên, và ảnh
            dialogueBox.SetActive(true);

            StartConversationButton.SetActive(true);
            PointMurderButton.SetActive(true);

            NPC npc = FindClosestNPC();
            if (NameText != null) NameText.text = npc.npcName;
            if (AvatarImage != null && npc.portrait != null)
            {
                AvatarImage.sprite = npc.portrait;
                AvatarImage.gameObject.SetActive(true);
            }
        }
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
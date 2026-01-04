using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public AudioSource audioSource;
    public AudioSource typeAudioSource;
    public AudioClip thumbnailMusic;
    public AudioClip typewriterSound;

    [Header("Gameplay UI")]
    public TextMeshProUGUI dayRemainText;

    [Header("Note UI")]
    public GameObject notePanel;
    public TextMeshProUGUI noteText;

    // Stamina UI
    public Slider staminaSlider;
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;
    private Image staminaFill;
    private bool isFlashing = false;
    

    // Lose + Replay
    // public TextMeshProUGUI loseText;
    public Button replayButton;

    // ====== NEW: START + CUTSCENE ======
    [Header("Start + Cutscene")]
    public GameObject startPanel;          // Panel chứa nút Start
    public Button startButton;             // Nút Start
    public Button continueButton;         // Nút Continue
    public GameObject cutscenePanel;       // Panel nền đen
    public TextMeshProUGUI cutsceneText;   // Text chạy chữ
    public GameObject menuButtonObject;
    public float textSpeed = 0.03f;

    private bool gameStarted = false;
    private bool canReplay = false;

    private ChaseManager chase => ChaseManager.instance;
    private GameManager gm => GameManager.Instance;
    private static bool cutscenePlayed = false;

    private static bool isLoadingSave = false;


    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

        // void Start()
        // {
        //     Time.timeScale = 1f;
        //     // 1. Khởi tạo tham chiếu UI Stamina
        //     if (staminaSlider != null && staminaSlider.fillRect != null)
        //     {
        //         staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        //     }



        //     // 3. THIẾT LẬP MẶC ĐỊNH MÀN HÌNH CHỜ (Start Menu)
            
        //     // Ẩn các UI gameplay không cần thiết lúc này
        //     if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        //     if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
        //     if (menuButtonObject != null) menuButtonObject.SetActive(false);
        //     if (cutscenePanel != null) cutscenePanel.SetActive(false);

        //     // DỌN DẸP VÀ GÁN LẠI SỰ KIỆN NÚT BẤM (Sửa lỗi nút không bấm được lần 2)
        //     if (startButton != null)
        //     {
        //         startButton.onClick.RemoveAllListeners();
        //         startButton.onClick.AddListener(OnStartPressed);
        //     }

        //     if (continueButton != null)
        //     {
        //         bool hasSaved = PlayerPrefs.GetInt("HasSavedGame", 0) == 1;
        //         continueButton.gameObject.SetActive(hasSaved);
        //         continueButton.onClick.RemoveAllListeners();
        //         continueButton.onClick.AddListener(OnContinuePressed);
        //     }

        //     if (replayButton != null)
        //     {
        //         // Nút Replay chỉ hiện khi chết, lúc Start game thì ẩn đi
        //         replayButton.gameObject.SetActive(false); 
        //         replayButton.onClick.RemoveAllListeners();
        //         replayButton.onClick.AddListener(ReplayScene);
        //     }

        //     if (isLoadingSave)
        //     {
        //         isLoadingSave = false;

        //         if (chase.player != null)
        //             SaveSystem.LoadAll(chase.player.gameObject);

        //         EvidenceManager.Instance.LockCollectedItemsInScene();
        //         if (EvidenceManager.Instance != null)
        //             EvidenceManager.Instance.CleanUpCollectedItemsInScene();

        //         if (ProfileUI.Instance != null)
        //             ProfileUI.Instance.UpdateUI();

        //         startPanel.SetActive(false);
        //         cutscenePanel.SetActive(false);

        //         StartGameplay();
        //         return;
        //     }

        //     // 4. QUẢN LÝ THỜI GIAN VÀ NHẠC
        //     if (startPanel != null && startPanel.activeSelf)
        //     {
        //         Time.timeScale = 0f; // Dừng game khi đang ở menu
        //     }

        //     if (!cutscenePlayed && audioSource != null && thumbnailMusic != null)
        //     {
        //         if (audioSource.clip != thumbnailMusic) // Tránh việc nhạc bị load lại từ đầu nếu đã đang chạy
        //         {
        //             audioSource.clip = thumbnailMusic;
        //             audioSource.loop = true;
        //             audioSource.Play();
        //         }
        //     }
        // }
    void Start()
    {
        // 1. Khởi tạo cơ bản
        Time.timeScale = 1f;

        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        }

        // 2. Thiết lập Menu mặc định
        if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
        if (menuButtonObject != null) menuButtonObject.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        // Gán lại sự kiện nút bấm
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartPressed);
        }

        if (continueButton != null)
        {
            bool hasSaved = PlayerPrefs.GetInt("HasSavedGame", 0) == 1;
            continueButton.gameObject.SetActive(hasSaved);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        if (replayButton != null)
        {

            replayButton.gameObject.SetActive(false); 
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayScene);
        }

        // 3. XỬ LÝ LOGIC NẠP GAME (CONTINUE HOẶC NEW GAME)
        if (isLoadingSave) 
        {
            // TRƯỜNG HỢP: CONTINUE
            isLoadingSave = false;

            if (chase.player != null)
                SaveSystem.LoadAll(chase.player.gameObject);

            // Dọn dẹp Evidence theo Save
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.CleanUpCollectedItemsInScene();
                EvidenceManager.Instance.LockCollectedItemsInScene();
            }

            if (ProfileUI.Instance != null) ProfileUI.Instance.UpdateUI();

            startPanel.SetActive(false);

            StartGameplay();
            
            // Nạp trễ Stage NPC để tránh bị reset
            Invoke("LateLoadNPCStage", 0.1f);
            return;
        }
        else if (PlayerPrefs.GetInt("IsNewGameFlag", 0) == 1)
        {
            // TRƯỜNG HỢP: NEW GAME (Sau khi vừa nạp lại Scene)
            PlayerPrefs.SetInt("IsNewGameFlag", 0); // Tắt cờ New Game ngay
            PlayerPrefs.Save();

            // Đưa người chơi về vị trí mặc định phòng khách
            if (chase.player != null)
                chase.player.transform.position = new Vector2(-17.58f, -30.6f);

            startPanel.SetActive(false);
            StartCoroutine(PlayCutscene()); // Chạy Intro từ đầu
            return;
        }

        // 4. TRẠNG THÁI CHỜ Ở MENU CHÍNH (Lần đầu mở game)
        if (startPanel != null && startPanel.activeSelf)
        {
            Time.timeScale = 0f; 
        }

        // Quản lý nhạc nền Menu
        if (!cutscenePlayed && audioSource != null && thumbnailMusic != null)
        {
            if (audioSource.clip != thumbnailMusic)
            {
                audioSource.clip = thumbnailMusic;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    // Thêm vào UIManager.cs
    public void ShowStartMenuCustom()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);

            // CẬP NHẬT NÚT CONTINUE NGAY LẬP TỨC
            if (continueButton != null)
            {
                // Kiểm tra trực tiếp từ bộ nhớ
                bool hasSaved = PlayerPrefs.GetInt("HasSavedGame", 0) == 1;
                continueButton.gameObject.SetActive(hasSaved);
            }
        }
    }
    void Update()
    {
        // if (notePanel != null && notePanel.activeSelf)
        // {
        //     if (Input.GetKeyDown(KeyCode.V))
        //     {
        //         CloseNote();
        //     }
        //     return;
        // }
        if (canReplay && Input.GetKeyDown(KeyCode.F))
    {
        ReplayScene();
        return;
    }

        if (!gameStarted) return; // gameplay UI chưa chạy

        UpdateDayRemain();
        UpdateStamina();
        CheckPlayerDeath();
    }

    //===========================================
    // START GAME
    //===========================================
        // void OnStartPressed() 
        // {
        //     // 1. Reset các biến điều hướng
        //     isLoadingSave = false;
        //     cutscenePlayed = false;

        //     // 2. Xóa sạch ổ cứng hoàn toàn
        //     PlayerPrefs.DeleteAll(); 
        //     PlayerPrefs.Save();
            
        //     // 3. Xóa sạch dữ liệu trong RAM (Dictionary, List, Unlocks)
        //     if (DialogueManager.Instance != null) {
        //         DialogueManager.Instance.Sang.Clear();
        //         DialogueManager.Instance.Mai.Clear();
        //         DialogueManager.Instance.Tan.Clear();
        //         DialogueManager.Instance.May.Clear();
        //     }
        //     if (EvidenceManager.Instance != null) {
        //         EvidenceManager.Instance.collectedEvidence.Clear();
        //         EvidenceManager.Instance.evidenceWeights.Clear();
        //     }
        //     CharacterUnlockManager.unlockedIndices.Clear();

        //     // 4. RESET TRẠNG THÁI NPC TRONG SCENE HIỆN TẠI
        //     NPC[] allNPCs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        //     foreach (NPC npc in allNPCs) {
        //         npc.dialogueStage = 0; // Đưa về Intro
        //         foreach (var block in npc.conditionalBlocks) {
        //             block.hasRead = false; // Xóa trạng thái đã đọc
        //         }
        //     }

        //     if (ChaseManager.instance != null && ChaseManager.instance.player != null)
        //     {
        //         // 👉 TỌA ĐỘ SPAWN PHÒNG KHÁCH
        //         ChaseManager.instance.player.transform.position = new Vector2(-17.58f, -30.6f);
        //     }

        //     // Reset camera confiner về phòng khách
        //     MapTransition[] transitions = Object.FindObjectsByType<MapTransition>(FindObjectsSortMode.None);
        //     foreach (var tr in transitions)
        //     {
        //         if (tr.areaName == "Living room 1")
        //         {
        //             var confiner = Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineConfiner2D>();
        //             if (confiner != null)
        //                 confiner.BoundingShape2D = tr.mapBoundary;

        //             PlayerPrefs.SetString("CurrentRoomName", "Living room 1");
        //             break;
        //         }
        //     }
            
        //     // 5. Bắt đầu Cutscene mới
        //     Time.timeScale = 1f;
        //     startPanel.SetActive(false);
        //     if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        //     StartCoroutine(PlayCutscene());
        // }
    void OnStartPressed() 
    {
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.SetInt("IsNewGameFlag", 1); // Đánh dấu đây là New Game
        PlayerPrefs.Save();
        
        isLoadingSave = false;
        cutscenePlayed = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Hàm mới cho nút Continue để nhảy cóc qua Cutscene
    void OnContinuePressed() 
    {
        Time.timeScale = 1f; // BẮT BUỘC: Mở khóa thời gian trước
        isLoadingSave = true; // Đánh dấu để hàm Start biết đường nạp dữ liệu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator PlayCutscene()
    {
        cutscenePanel.SetActive(true);

        string[] lines =
        {
            "Hang: The case from 10 years ago ... and the mystery of this house ... I already know ...",
            "Hang: You should come here quickly ...",
            "Hang: If not ...AAAAAAAAAAAAA",
            "Hello ... Helllo ... Hang ... Are you there? Hello..."
        };
        if (typeAudioSource != null && typewriterSound != null)
        {
            typeAudioSource.clip = typewriterSound;
            if (!typeAudioSource.isPlaying)
                typeAudioSource.Play();
        }

        foreach (string line in lines)
        {
            // Hiện chữ từ từ
            yield return StartCoroutine(TypeSentence(line));

            // Chờ người chơi bấm Z mới chuyển
            yield return StartCoroutine(WaitForNext());
        }
        if (typeAudioSource != null && typeAudioSource.isPlaying)
        {
            typeAudioSource.Stop();
        }

        // Hết cutscene
        // Khi kết thúc Cutscene, lưu lại trạng thái
        PlayerPrefs.SetInt("CutscenePlayed", 1);
        PlayerPrefs.Save();
        cutscenePanel.SetActive(false);
        cutscenePlayed = true;
        StartGameplay();
    }

    IEnumerator TypeSentence(string sentence)
    {
        cutsceneText.text = "";

        foreach (char c in sentence)
        {
            cutsceneText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    IEnumerator WaitForNext()
    {
        // Hiện dòng “Press Z to continue…” nếu cần
        // (tùy bạn có muốn thêm không)
        // hintText.SetActive(true);

        while (!Input.GetKeyDown(KeyCode.F))
            yield return null;

        // hintText.SetActive(false);
    }


    void StartGameplay()
    {
        gameStarted = true;
        GameManager.Instance.StartDay();

        // Bật UI gameplay
        dayRemainText.gameObject.SetActive(true);
        staminaSlider.gameObject.SetActive(true);
        if (menuButtonObject != null) menuButtonObject.SetActive(true);

        // Cưỡng ép tắt nút Replay và trạng thái canReplay khi bắt đầu
        if (replayButton != null) replayButton.gameObject.SetActive(false);
        canReplay = false;
    }

    // ===========================================
    // GAMEPLAY UI
    // ===========================================

    void UpdateDayRemain()
    {
        dayRemainText.text = $"days remain: {gm.daysRemaining}";
    }

    void UpdateStamina()
    {
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;

        float pct = chase.player.currentStamina / chase.player.maxStamina;

        if (pct > 0.6f)
            staminaFill.color = fullColor;
        else if (pct > 0.3f)
            staminaFill.color = midColor;
        else
            staminaFill.color = lowColor;

        if (pct < 0.2f && !isFlashing)
            StartCoroutine(FlashStaminaBar());
    }

    IEnumerator FlashStaminaBar()
    {
        isFlashing = true;
        for (int i = 0; i < 6; i++)
        {
            staminaFill.enabled = false;
            yield return new WaitForSeconds(0.15f);
            staminaFill.enabled = true;
            yield return new WaitForSeconds(0.15f);
        }
        isFlashing = false;
    }

    void CheckPlayerDeath() {
        if (chase.player.killed && !replayButton.gameObject.activeSelf) {
            replayButton.gameObject.SetActive(true);
            canReplay = true;
        }
    }

    public void ReplayScene() 
    {
        Time.timeScale = 1f; // BẮT BUỘC: Mở khóa thời gian trước
        isLoadingSave = true; 
        
        if (replayButton != null) replayButton.gameObject.SetActive(false);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenNote(string content)
    {
        notePanel.SetActive(true);
        noteText.text = content;
        Time.timeScale = 0f; // tạm dừng game nếu muốn
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

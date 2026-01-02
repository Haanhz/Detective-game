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

    void Start()
    {
        Time.timeScale = 1f;
        // 1. Khởi tạo tham chiếu UI Stamina
        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        }

        // 2. XỬ LÝ KHI LOAD GAME (Continue / Replay)
        // if (isLoadingSave)
        // {
        //     isLoadingSave = false; // Reset ngay để tránh vòng lặp

        //     // Nạp dữ liệu từ SaveSystem
        //     if (chase.player != null)
        //     {
        //         SaveSystem.LoadAll(chase.player.gameObject);
        //     }

        //     // --- QUAN TRỌNG: Cập nhật lại giao diện Profile sau khi Load xong ---
        //     // Điều này sửa lỗi "Knowledge Gain" bị trống khi Continue
        //     if (ProfileUI.Instance != null)
        //     {
        //         ProfileUI.Instance.UpdateUI(); 
        //     }

        //     // Tắt các panel chờ, vào thẳng game
        //     if (startPanel != null) startPanel.SetActive(false);
        //     if (cutscenePanel != null) cutscenePanel.SetActive(false);
            
        //     StartGameplay(); 
        //     Time.timeScale = 1f; // Đảm bảo thời gian chạy lại, sửa lỗi "đơ" nút
        //     return; 
        // }

        // 3. THIẾT LẬP MẶC ĐỊNH MÀN HÌNH CHỜ (Start Menu)
        
        // Ẩn các UI gameplay không cần thiết lúc này
        if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
        if (menuButtonObject != null) menuButtonObject.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        // DỌN DẸP VÀ GÁN LẠI SỰ KIỆN NÚT BẤM (Sửa lỗi nút không bấm được lần 2)
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
            // Nút Replay chỉ hiện khi chết, lúc Start game thì ẩn đi
            replayButton.gameObject.SetActive(false); 
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayScene);
        }

        if (isLoadingSave)
        {
            isLoadingSave = false;

            if (chase.player != null)
                SaveSystem.LoadAll(chase.player.gameObject);

            EvidenceManager.Instance.LockCollectedItemsInScene();

            if (ProfileUI.Instance != null)
                ProfileUI.Instance.UpdateUI();

            startPanel.SetActive(false);
            cutscenePanel.SetActive(false);

            StartGameplay();
            return;
        }

        // 4. QUẢN LÝ THỜI GIAN VÀ NHẠC
        if (startPanel != null && startPanel.activeSelf)
        {
            Time.timeScale = 0f; // Dừng game khi đang ở menu
        }

        if (!cutscenePlayed && audioSource != null && thumbnailMusic != null)
        {
            if (audioSource.clip != thumbnailMusic) // Tránh việc nhạc bị load lại từ đầu nếu đã đang chạy
            {
                audioSource.clip = thumbnailMusic;
                audioSource.loop = true;
                audioSource.Play();
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
    void OnStartPressed() 
    {
        // 1. Reset các biến điều hướng
        isLoadingSave = false;
        cutscenePlayed = false;

        // 2. Xóa sạch ổ cứng hoàn toàn
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
        
        // 3. Xóa sạch dữ liệu trong RAM (Dictionary, List, Unlocks)
        if (DialogueManager.Instance != null) {
            DialogueManager.Instance.Sang.Clear();
            DialogueManager.Instance.Mai.Clear();
            DialogueManager.Instance.Tan.Clear();
            DialogueManager.Instance.May.Clear();
        }
        if (EvidenceManager.Instance != null) {
            EvidenceManager.Instance.collectedEvidence.Clear();
            EvidenceManager.Instance.evidenceWeights.Clear();
        }
        CharacterUnlockManager.unlockedIndices.Clear();

        // 4. RESET TRẠNG THÁI NPC TRONG SCENE HIỆN TẠI
        NPC[] allNPCs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        foreach (NPC npc in allNPCs) {
            npc.dialogueStage = 0; // Đưa về Intro
            foreach (var block in npc.conditionalBlocks) {
                block.hasRead = false; // Xóa trạng thái đã đọc
            }
        }
        
        // 5. Bắt đầu Cutscene mới
        Time.timeScale = 1f;
        startPanel.SetActive(false);
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        StartCoroutine(PlayCutscene());
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
            "Hang: The case from long ago ... and the mystery of this house ... I already know ...",
            "Hang: You should come here quickly ...",
            "Hang: If not ... he ... ... ... ...",
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

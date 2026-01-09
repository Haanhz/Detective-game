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

    [Header("Radial Progress UI")]
    public Image dayProgressImage;   // Kéo object 'day' vào đây
    public Image nightProgressImage; // Kéo object 'night' vào đây

    private bool gameStarted = false;
    private bool canReplay = false;

    private ChaseManager chase => ChaseManager.instance;
    private GameManager gm => GameManager.Instance;
    private static bool cutscenePlayed = false;

    private static bool isLoadingSave = false;


    void Awake()
    {
        Instance = this;
        Time.timeScale = 0f;
    }

    
    void Start()
    {
        // 1. Khởi tạo cơ bản
        
        if (PlayerPrefs.GetInt("ShowDayDeduction", 0) == 1)
        {
            StartCoroutine(ShowDayDeductionDelayed());
        PlayerPrefs.SetInt("ShowDayDeduction", 0); // Reset flag
        }
        Time.timeScale = 0f;

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
                EvidenceManager.Instance.CleanUpPermanentlyRemovedEvidence();
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
    IEnumerator ShowDayDeductionDelayed()
{
    // Đợi cho đến khi GameManager đã sẵn sàng
    yield return new WaitUntil(() => gm != null && gm.dayDeductionText != null);
    
    Debug.Log("Showing Day Deduction UI");
    yield return StartCoroutine(gm.ShowDayDeduction());
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
    
 
        if (canReplay && Input.GetKeyDown(KeyCode.F))
        {
            ReplayScene();
            return;
        }

        if (!gameStarted) return; // gameplay UI chưa chạy

        UpdateDayRemain();
        UpdateStamina();
        UpdateRadialProgress();
        CheckPlayerDeath();
    }


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
        Time.timeScale = 1f;
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
        Time.timeScale = 1f;
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
        dayRemainText.text = $"{gm.daysRemaining}";
    }

    void UpdateStamina()
    {
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;

        float pct = chase.player.currentStamina / chase.player.maxStamina;

        // if (pct > 0.6f)
        //     staminaFill.color = fullColor;
        // else if (pct > 0.3f)
        //     staminaFill.color = midColor;
        // else
        //     staminaFill.color = lowColor;

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

    void UpdateRadialProgress()
    {
        if (gm == null) return;

        // 1. Xác định Max Duration và Image mục tiêu dựa trên trạng thái
        float maxDuration = gm.isNight ? gm.nightDuration : gm.dayDuration;
        float currentFill = 1f - (gm.timer / maxDuration);

        // 2. Bật/Tắt và cập nhật Fill Amount
        if (gm.isNight)
        {
            if (dayProgressImage != null) dayProgressImage.gameObject.SetActive(false);
            if (nightProgressImage != null) 
            {
                nightProgressImage.gameObject.SetActive(true);
                nightProgressImage.fillAmount = currentFill;
            }
        }
        else
        {
            if (nightProgressImage != null) nightProgressImage.gameObject.SetActive(false);
            if (dayProgressImage != null) 
            {
                dayProgressImage.gameObject.SetActive(true);
                dayProgressImage.fillAmount = currentFill;
            }
        }
    }

    void CheckPlayerDeath()
    {
        if (chase.player.killed && !replayButton.gameObject.activeSelf)
        {
            replayButton.gameObject.SetActive(true);
            canReplay = true;
        }
    }

    public void ReplayScene()
    {
        Time.timeScale = 1f;
        bool shouldShowDayDeduction = false;

        // TRƯỜNG HỢP 1: CHẾT (BỊ GIẾT HOẶC KIỆT SỨC)
        if (chase.player.killed || chase.player.exhausted)
        {
            // Nếu chết vào ban đêm, xử lý xóa bằng chứng tạm thời
            if (gm.isNight && EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.RevertNightlyEvidence();
            }

            // Cả 2 loại chết đều bị trừ ngày và ép về ban ngày
            gm.daysRemaining--;
            gm.isNight = false;

            EndingManager.IsKilledByBlack = false;

            // Lưu trạng thái Reset (Vị trí mặc định sẽ được nạp lại do nạp Scene)
            SaveSystem.SaveAll(chase.player.gameObject);
            shouldShowDayDeduction = true;
        }
        // TRƯỜNG HỢP 2: CÁC ENDING SUY LUẬN (GIỮ NGUYÊN EVIDENCE & STAMINA)
        else if (EndingManager.IsDetectiveEnding)
        {
            gm.daysRemaining--; // Chỉ trừ ngày
            EndingManager.IsDetectiveEnding = false;

            // Lưu lại để giữ nguyên Stamina và Inventory hiện tại
            SaveSystem.SaveAll(chase.player.gameObject);
            shouldShowDayDeduction = true;
        }
        PlayerPrefs.SetInt("ShowDayDeduction", shouldShowDayDeduction ? 1 : 0);
        PlayerPrefs.Save();

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

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
    }

    void Start()
    {
        // 1. Khởi tạo tham chiếu UI cho thanh Stamina
        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        }

        // 2. KIỂM TRA TRẠNG THÁI TẢI DỮ LIỆU (Continue/Replay)
        // Nếu biến static isLoadingSave là true, nghĩa là chúng ta vừa nạp lại Scene để chơi tiếp
        if (isLoadingSave)
        {
            isLoadingSave = false; // Reset ngay lập tức để tránh vòng lặp
            
            // Nạp lại toàn bộ dữ liệu từ ổ cứng vào RAM (Vị trí, Đồ, Hội thoại, Profile)
            if (chase.player != null)
            {
                SaveSystem.LoadAll(chase.player.gameObject);
            }

            // Tắt các màn hình chờ để vào thẳng Gameplay
            if (startPanel != null) startPanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);
            
            StartGameplay(); // Kích hoạt các Manager khác (GameManager, v.v.)
            Time.timeScale = 1f; // Chạy lại thời gian
            return; // Thoát hàm Start, không chạy logic màn hình tiêu đề phía dưới
        }

        // 3. THIẾT LẬP MẶC ĐỊNH CHO MÀN HÌNH TIÊU ĐỀ (Start Menu)
        
        // Ẩn toàn bộ UI gameplay và các panel không liên quan lúc đầu game
        if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
        if (menuButtonObject != null) menuButtonObject.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        // Xóa các sự kiện cũ và gán sự kiện mới cho các nút bấm để tránh lỗi nạp chồng
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(ReplayScene);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartPressed);
        }

        if (continueButton != null)
        {
            // Kiểm tra xem máy đã có file lưu chưa để hiện nút Continue
            bool hasSaved = PlayerPrefs.GetInt("HasSavedGame", 0) == 1;
            continueButton.gameObject.SetActive(hasSaved);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        // Đảm bảo dừng thời gian nếu Start Panel đang hiện
        if (startPanel != null && startPanel.activeSelf)
        {
            Time.timeScale = 0f;
        }

        // 4. CHƠI NHẠC NỀN CHO MÀN HÌNH CHỜ
        if (!cutscenePlayed && audioSource != null && thumbnailMusic != null)
        {
            audioSource.clip = thumbnailMusic;
            audioSource.loop = true;
            audioSource.Play();
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
        // 1. Reset trạng thái Static
        isLoadingSave = false;
        cutscenePlayed = false;

        // 2. Xóa sạch ổ cứng
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
        
        // 3. Xóa sạch RAM hội thoại
        DialogueManager.Instance.Sang.Clear();
        DialogueManager.Instance.Mai.Clear();
        DialogueManager.Instance.Tan.Clear();
        DialogueManager.Instance.May.Clear();
        EvidenceManager.Instance.collectedEvidence.Clear();
        CharacterUnlockManager.unlockedIndices.Clear();
        
        Time.timeScale = 1f;
        startPanel.SetActive(false);
        if (audioSource.isPlaying) audioSource.Stop();
        StartCoroutine(PlayCutscene());
    }

    // Hàm mới cho nút Continue để nhảy cóc qua Cutscene
    void OnContinuePressed() 
    {
        isLoadingSave = true; // Đánh dấu để nạp save sau khi load scene
        Time.timeScale = 1f;
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
        if (chase.player.dead && !replayButton.gameObject.activeSelf) {
            replayButton.gameObject.SetActive(true);
            canReplay = true;
        }
    }

    public void ReplayScene() 
    {
        isLoadingSave = true; // Đánh dấu để nạp save sau khi load scene
        Time.timeScale = 1f;
        
        // Tắt nút ngay lập tức để không bị hiện lỗi
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

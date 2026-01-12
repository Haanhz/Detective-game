using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    // Biến static để kiểm tra xem đây có phải là lần đầu tiên chạy code trong phiên này không
    // Biến này sẽ reset về true mỗi khi bạn tắt hẳn Game và mở lại (exe/app)
    private static bool isFirstTimeSession = true;

    public AudioSource audioSource;
    public AudioSource typeAudioSource;
    public AudioClip thumbnailMusic;
    public AudioClip typewriterSound;

    [Header("Gameplay UI")]
    public TextMeshProUGUI dayRemainText;

    [Header("Note UI")]
    public GameObject notePanel;
    public TextMeshProUGUI noteText;

    [Header("Stamina UI")]
    public Slider staminaSlider;
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;
    private Image staminaFill;
    private bool isFlashing = false;

    [Header("Lose + Replay")]
    public Button replayButton;

    [Header("Start + Cutscene")]
    public GameObject startPanel;           
    public Button startButton;              
    public Button continueButton;          
    public GameObject cutscenePanel;        
    public TextMeshProUGUI cutsceneText;    
    public GameObject menuButtonObject;
    public float textSpeed = 0.03f;

    [Header("Radial Progress UI")]
    public Image dayProgressImage;   
    public Image nightProgressImage; 

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

        // KIỂM TRA LẦN ĐẦU MỞ GAME TRONG PHIÊN (SESSION) NÀY
        if (isFirstTimeSession)
        {
            Debug.Log("First time opening game in this session. Clearing all data...");
            PlayerPrefs.DeleteAll(); // Xóa sạch PlayerPrefs bao gồm HasSavedGame
            PlayerPrefs.Save();
            isFirstTimeSession = false; // Đánh dấu đã qua bước kiểm tra đầu tiên
        }
    }

    void Start()
    {
        // 1. Khởi tạo cơ bản
        if (PlayerPrefs.GetInt("ShowDayDeduction", 0) == 1)
        {
            StartCoroutine(ShowDayDeductionDelayed());
            PlayerPrefs.SetInt("ShowDayDeduction", 0); 
        }

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
            // Kiểm tra trạng thái lưu sau khi đã xử lý DeleteAll ở Awake
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

        // 3. XỬ LÝ LOGIC NẠP GAME
        if (isLoadingSave)
        {
            isLoadingSave = false;
            if (chase.player != null) SaveSystem.LoadAll(chase.player.gameObject);

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.CleanUpPermanentlyRemovedEvidence();
                EvidenceManager.Instance.CleanUpCollectedItemsInScene();
                EvidenceManager.Instance.LockCollectedItemsInScene();
            }

            if (ProfileUI.Instance != null) ProfileUI.Instance.UpdateUI();

            startPanel.SetActive(false);
            StartGameplay();
            Invoke("LateLoadNPCStage", 0.1f);
            return;
        }
        else if (PlayerPrefs.GetInt("IsNewGameFlag", 0) == 1)
        {
            PlayerPrefs.SetInt("IsNewGameFlag", 0); 
            PlayerPrefs.Save();

            if (chase.player != null)
                chase.player.transform.position = new Vector2(-17.58f, -30.6f);

            startPanel.SetActive(false);
            StartCoroutine(PlayCutscene()); 
            return;
        }

        // 4. TRẠNG THÁI CHỜ Ở MENU CHÍNH
        if (startPanel != null && startPanel.activeSelf)
        {
            Time.timeScale = 0f;
        }

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
        yield return new WaitUntil(() => gm != null && gm.dayDeductionText != null);
        yield return StartCoroutine(gm.ShowDayDeduction());
    }

    public void ShowStartMenuCustom()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            if (continueButton != null)
            {
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

        if (!gameStarted) return; 

        UpdateDayRemain();
        UpdateStamina();
        UpdateRadialProgress();
        CheckPlayerDeath();
    }

    void OnStartPressed()
    {
        // Khi bấm Start (New Game), dọn dẹp mọi thứ cũ
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("IsNewGameFlag", 1); 
        PlayerPrefs.Save();

        isLoadingSave = false;
        cutscenePlayed = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnContinuePressed()
    {
        Time.timeScale = 1f; 
        isLoadingSave = true; 
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
            if (!typeAudioSource.isPlaying) typeAudioSource.Play();
        }

        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeSentence(line));
            yield return StartCoroutine(WaitForNext());
        }

        if (typeAudioSource != null && typeAudioSource.isPlaying) typeAudioSource.Stop();

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
        while (!Input.GetKeyDown(KeyCode.F))
            yield return null;
    }

    void StartGameplay()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        GameManager.Instance.StartDay();

        dayRemainText.gameObject.SetActive(true);
        staminaSlider.gameObject.SetActive(true);
        if (menuButtonObject != null) menuButtonObject.SetActive(true);

        if (replayButton != null) replayButton.gameObject.SetActive(false);
        canReplay = false;
    }

    // --- CÁC HÀM UI GAMEPLAY KHÁC GIỮ NGUYÊN ---
    void UpdateDayRemain() { dayRemainText.text = $"{gm.daysRemaining}"; }

    void UpdateStamina()
    {
        if (chase.player == null) return;
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;
        float pct = chase.player.currentStamina / chase.player.maxStamina;
        if (pct < 0.2f && !isFlashing) StartCoroutine(FlashStaminaBar());
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
        float maxDuration = gm.isNight ? gm.nightDuration : gm.dayDuration;
        float currentFill = 1f - (gm.timer / maxDuration);

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
        if (chase.player != null && chase.player.killed && !replayButton.gameObject.activeSelf)
        {
            replayButton.gameObject.SetActive(true);
            canReplay = true;
        }
    }

    public void ReplayScene()
    {
        Time.timeScale = 1f;
        bool shouldShowDayDeduction = false;

        if (chase.player.killed || chase.player.exhausted)
        {
            if (gm.isNight && EvidenceManager.Instance != null)
                EvidenceManager.Instance.RevertNightlyEvidence();

            gm.daysRemaining--;
            gm.isNight = false;
            EndingManager.IsKilledByBlack = false;

            SaveSystem.SaveAll(chase.player.gameObject);
            shouldShowDayDeduction = true;
        }
        else if (EndingManager.IsDetectiveEnding)
        {
            gm.daysRemaining--;
            EndingManager.IsDetectiveEnding = false;
            SaveSystem.SaveAll(chase.player.gameObject);
            shouldShowDayDeduction = true;
        }

        PlayerPrefs.SetInt("ShowDayDeduction", shouldShowDayDeduction ? 1 : 0);
        PlayerPrefs.SetInt("HasSavedGame", 1); // Đánh dấu có file lưu sau khi chết/qua ngày
        PlayerPrefs.Save();

        isLoadingSave = true;
        if (replayButton != null) replayButton.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenNote(string content)
    {
        notePanel.SetActive(true);
        noteText.text = content;
        Time.timeScale = 0f;
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
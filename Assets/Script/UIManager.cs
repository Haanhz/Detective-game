using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
        // Mặc định dừng thời gian khi khởi tạo UI
        Time.timeScale = 0f;

        if (isFirstTimeSession)
        {
            Debug.Log("First time opening game in this session. Clearing all data...");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            isFirstTimeSession = false;
        }
    }

    void Start()
    {
        // 1. Khởi tạo cơ bản
        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        }

        // 2. Thiết lập Menu mặc định (Ẩn các UI gameplay trước)
        if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);
        if (menuButtonObject != null) menuButtonObject.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        // Gán sự kiện nút bấm
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

        // 3. XỬ LÝ LOGIC NẠP GAME (Ưu tiên kiểm tra load trước)
        if (isLoadingSave)
        {
            isLoadingSave = false; // Reset cờ load

            // Xử lý nạp dữ liệu
            if (chase.player != null) SaveSystem.LoadAll(chase.player.gameObject);

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.CleanUpPermanentlyRemovedEvidence();
                EvidenceManager.Instance.CleanUpCollectedItemsInScene();
                EvidenceManager.Instance.LockCollectedItemsInScene();
            }

            if (ProfileUI.Instance != null) ProfileUI.Instance.UpdateUI();

            // Ẩn panel và bắt đầu chơi ngay
            if (startPanel != null) startPanel.SetActive(false);

            // Kiểm tra hiển thị thông báo trừ ngày nếu cần
            if (PlayerPrefs.GetInt("ShowDayDeduction", 0) == 1)
            {
                StartCoroutine(ShowDayDeductionDelayed());
                PlayerPrefs.SetInt("ShowDayDeduction", 0);
            }

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

            if (startPanel != null) startPanel.SetActive(false);
            StartCoroutine(PlayCutscene());
            return;
        }

        // 4. TRẠNG THÁI CHỜ Ở MENU CHÍNH (Nếu không load/new game)
        if (startPanel != null)
        {
            startPanel.SetActive(true);
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
        isLoadingSave = true;
        Time.timeScale = 1f;
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
        Time.timeScale = 1f; // Đảm bảo thời gian chạy để người chơi di chuyển được
        if (GameManager.Instance != null) GameManager.Instance.StartDay();

        if (dayRemainText != null) dayRemainText.gameObject.SetActive(true);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(true);
        if (menuButtonObject != null) menuButtonObject.SetActive(true);

        if (replayButton != null) replayButton.gameObject.SetActive(false);
        canReplay = false;
    }

    void UpdateDayRemain() { if (gm != null) dayRemainText.text = $"{gm.daysRemaining}"; }

    void UpdateStamina()
    {
        if (chase.player == null || staminaSlider == null) return;
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;
        float pct = chase.player.currentStamina / chase.player.maxStamina;
        if (pct < 0.2f && !isFlashing) StartCoroutine(FlashStaminaBar());
    }

    IEnumerator FlashStaminaBar()
    {
        if (staminaFill == null) yield break;
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
            // ← Xử lý stamina cho trường hợp chết trong game
            if (chase.player.exhausted)
            {
                // Chết do cạn stamina → Replay với 50% max
                chase.player.currentStamina = chase.player.maxStamina * 0.5f;
            }
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
        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.Save();

        isLoadingSave = true; // Cờ này quan trọng để Start() biết cần nạp lại dữ liệu sau khi load scene
        if (replayButton != null) replayButton.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenNote(string content)
    {
        notePanel.SetActive(true);
        noteText.text = content;

    }

    public void CloseNote()
    {
        notePanel.SetActive(false);

    }
    void LateLoadNPCStage()
    {
        NPC[] allNPCs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        Debug.Log($"[LateLoad] Found {allNPCs.Length} NPCs to load");

        if (allNPCs.Length == 0)
        {
            Debug.LogError("[LateLoad] NO NPCs FOUND! They might not be spawned yet!");
            return;
        }

        foreach (NPC npc in allNPCs)
        {
            string stageKey = "NPCStage_" + npc.npcName;
            int savedStage = PlayerPrefs.GetInt(stageKey, 0);
            npc.dialogueStage = savedStage;
            Debug.Log($"[LateLoad] {npc.npcName}: Loaded stage={savedStage}");

            // NẠP LẠI TRẠNG THÁI ĐÃ ĐỌC
            for (int i = 0; i < npc.conditionalBlocks.Count; i++)
            {
                string key = "NPCCond_" + npc.npcName + "_" + i;
                bool savedHasRead = PlayerPrefs.GetInt(key, 0) == 1;
                npc.conditionalBlocks[i].hasRead = savedHasRead;
                Debug.Log($"[LateLoad] {npc.npcName} block[{i}]: hasRead={savedHasRead}");
            }
        }
    }


}
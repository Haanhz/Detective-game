using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
    public GameObject cutscenePanel;       // Panel nền đen
    public TextMeshProUGUI cutsceneText;   // Text chạy chữ
    public float textSpeed = 0.03f;

    private bool gameStarted = false;

    private ChaseManager chase => ChaseManager.instance;
    private GameManager gm => GameManager.Instance;
    private static bool cutscenePlayed = false;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        staminaFill = staminaSlider.fillRect.GetComponent<Image>();

        // Ẩn UI gameplay và lose lúc chưa bắt đầu
        if (dayRemainText != null) dayRemainText.gameObject.SetActive(false);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(false);

        // if (loseText != null) loseText.gameObject.SetActive(false);
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
            replayButton.onClick.AddListener(ReplayScene);
        }

        // Nút Start
        if (startButton != null)
            startButton.onClick.AddListener(OnStartPressed);

        // Cutscene off đầu game
        cutscenePanel.SetActive(false);
         if (cutscenePlayed)
    {
        startPanel.SetActive(false);
        StartGameplay();
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
        startPanel.SetActive(false);
        StartCoroutine(PlayCutscene());
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

        foreach (string line in lines)
        {
            // Hiện chữ từ từ
            yield return StartCoroutine(TypeSentence(line));

            // Chờ người chơi bấm Z mới chuyển
            yield return StartCoroutine(WaitForNext());
        }

        // Hết cutscene
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

        while (!Input.GetKeyDown(KeyCode.Z))
            yield return null;

        // hintText.SetActive(false);
    }


    void StartGameplay()
    {
        gameStarted = true;

        // bật UI
        dayRemainText.gameObject.SetActive(true);
        staminaSlider.gameObject.SetActive(true);
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

    void CheckPlayerDeath()
    {
        if (chase.player.dead)
        {
            // loseText.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(true);
        }
    }

    void ReplayScene()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
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

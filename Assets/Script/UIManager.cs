using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // 🔹 thêm để load lại scene

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI timePlayedText;
    public TextMeshProUGUI chaseTimerText;
    public TextMeshProUGUI evidenceText;
    public TextMeshProUGUI missionText;
    public Slider probSlider;
    public Slider staminaSlider;

    public TextMeshProUGUI loseText;
    public Button replayButton; // 🔹 thêm nút Replay

    // Color stamina
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;

    private Image staminaFill;
    private bool isFlashing = false;
    private float playTime = 0f;

    private ChaseManager chase => ChaseManager.instance;

    // ✅ Timer nội bộ UI
    private float chaseCountdown = 0f;
    private bool isChasing = false;
    private float delayTimer = 0f;
    private bool waitingForChase = false;

    void Start()
    {
        // Lấy Fill của slider
        staminaFill = staminaSlider.fillRect.GetComponent<Image>();

        // 🔹 Ẩn LoseText và ReplayButton lúc đầu
        if (loseText != null) loseText.gameObject.SetActive(false);
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
            replayButton.onClick.AddListener(ReplayScene);
        }
    }

    void Update()
    {
        UpdatePlayTime();
        UpdateChaseState();
        UpdateProbBar();
        UpdateStamina();
        CheckPlayerDeath();
        UpdateEvidenceUI();
    }

    // 🕒 Thời gian chơi
    void UpdatePlayTime()
    {
        playTime += Time.deltaTime;
        int m = Mathf.FloorToInt(playTime / 60f);
        int s = Mathf.FloorToInt(playTime % 60f);
        timePlayedText.text = $"time: {m:00}:{s:00}";
    }

    // 😈 Đếm ngược chase
    void UpdateChaseState()
    {
        if (chase.player.dead)
        {
            ResetChaseUI();
            return;
        }

        if (chase.blackSpawned && !isChasing && !waitingForChase)
        {
            waitingForChase = true;
            delayTimer = chase.chaseDelay;
            chaseTimerText.text = $"chase in: {Mathf.CeilToInt(delayTimer)}s";
        }

        if (waitingForChase)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer > 0)
            {
                chaseTimerText.text = $"chase in: {Mathf.CeilToInt(delayTimer)}s";
                return;
            }
            else
            {
                waitingForChase = false;
                isChasing = true;
                chaseCountdown = chase.chaseDur;
            }
        }

        if (isChasing)
        {
            chaseCountdown -= Time.deltaTime;
            int sec = Mathf.CeilToInt(chaseCountdown);
            if (sec < 0) sec = 0;
            chaseTimerText.text = $"chase: {sec}s";

            if (!chase.blackSpawned || sec <= 0)
            {
                ResetChaseUI();
            }
        }

        if (!chase.blackSpawned && !isChasing && !waitingForChase)
        {
            chaseTimerText.text = "chase: --";
        }
    }

    void UpdateProbBar()
    {
        probSlider.value = chase.probAppear;
    }

    void UpdateEvidenceUI()
    {
        int current = ScoreBoard.scoreValue;
        evidenceText.text = $"evidences: {current}/7";
        if (current == 7)
        {
            missionText.gameObject.SetActive(true);
        }
    }

    void ResetChaseUI()
    {
        isChasing = false;
        waitingForChase = false;
        chaseCountdown = 0f;
        delayTimer = 0f;
        chaseTimerText.text = "chase: --";
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

    // 🔹 Khi player chết
    void CheckPlayerDeath()
    {
        if (chase.player.dead)
        {
            if (loseText != null) loseText.gameObject.SetActive(true);
            if (replayButton != null) replayButton.gameObject.SetActive(true);
        }
    }

    // 🔁 Hàm replay scene
    void ReplayScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}

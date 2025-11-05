using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI timePlayedText;
    public TextMeshProUGUI chaseTimerText;
    public Slider probSlider;
    public Slider staminaSlider;

    private float playTime = 0f;

    private ChaseManager chase => ChaseManager.instance;

    // ✅ Timer nội bộ UI
    private float chaseCountdown = 0f;
    private bool isChasing = false;
    private float delayTimer = 0f;
    private bool waitingForChase = false;

    void Update()
    {
        UpdatePlayTime();
        UpdateChaseState();
        UpdateProbBar();
        UpdateStamina();
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
        // Nếu player chết
        if (chase.player.dead)
        {
            ResetChaseUI();
            return;
        }

        // Khi Black vừa spawn → bắt đầu đếm delay
        if (chase.blackSpawned && !isChasing && !waitingForChase)
        {
            waitingForChase = true;
            delayTimer = chase.chaseDelay; // ví dụ 2s delay trước khi bắt đầu chase
            chaseTimerText.text = $"chase in: {Mathf.CeilToInt(delayTimer)}s";
        }

        // Khi đang chờ delay
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
                chaseCountdown = chase.chaseDur; // 10s chase
            }
        }

        // Khi đang chase
        if (isChasing)
        {
            chaseCountdown -= Time.deltaTime;
            int sec = Mathf.CeilToInt(chaseCountdown);
            if (sec < 0) sec = 0;
            chaseTimerText.text = $"chase: {sec}s";

            // Kết thúc chase
            if (!chase.blackSpawned || sec <= 0)
            {
                ResetChaseUI();
            }
        }

        // Khi không chase và không spawn quái
        if (!chase.blackSpawned && !isChasing && !waitingForChase)
        {
            chaseTimerText.text = "chase: --";
        }
    }

    // Thanh xác suất
    void UpdateProbBar()
    {
        probSlider.value = chase.probAppear;
    }

    // Reset về mặc định
    void ResetChaseUI()
    {
        isChasing = false;
        waitingForChase = false;
        chaseCountdown = 0f;
        delayTimer = 0f;
        chaseTimerText.text = "chase: --";
    }

    //Thanh stamina
    void UpdateStamina()
    {
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;
    }
}

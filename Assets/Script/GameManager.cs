using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dayDuration = 60f;   // 5 phút = 300s
    public float nightDuration = 60f;

    public int daysRemaining = 3;      // đếm ngược 3 ngày
    public bool isNight = false;       // bắt đầu = ban ngày

    private float timer = 0f;
    public CanvasGroup nightPanel;

    public event Action OnDayStart;
    public event Action OnNightStart;
    public event Action OnDayEnded;    // khi hết 1 ngày
    bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartDay();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isNight)
        {
            // đang ban ngày
            if (timer >= dayDuration)
            {
                StartNight();
            }
        }
        else
        {
            // đang ban đêm
            if (timer >= nightDuration)
            {
                EndNightAndNextDay();
            }
        }
        if (gameEnded) return;
    }

    // ----- LOGIC -----

    void StartDay()
    {
        isNight = false;
        timer = 0f;
        OnDayStart?.Invoke();
        nightPanel.alpha = 0;
        Debug.Log("GOOD MORNING!");
    }

    void StartNight()
    {
        isNight = true;
        timer = 0f;
        OnNightStart?.Invoke();
        nightPanel.alpha = 1;
        Debug.Log("IS NIGHT ALREADY?");
    }

    void EndNightAndNextDay()
    {
        daysRemaining--;

        OnDayEnded?.Invoke();

        if (daysRemaining <= 0)
        {
            Debug.Log("TIME OUT! GAME OVER");
            gameEnded = true;
            // mày có thể gọi UI Game Over ở đây
            return;
        }

        Debug.Log("DAYS REMAINING: " + daysRemaining);
        StartDay();
    }
}

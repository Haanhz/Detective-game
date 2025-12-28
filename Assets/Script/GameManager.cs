using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    public static GameManager Instance;
    public GameObject limit1;
    public GameObject limit2;
    public GameObject limit3;
    public GameObject limit4;
    public GameObject limit5;
    public GameObject limit6;

    public float dayDuration = 60f;
    public float nightDuration = 60f;

    public int daysRemaining = 7;
    public bool isNight = false;
    public bool isLight = false;
    public int currentNight = 0;
    private float timer = 0f;
    // public CanvasGroup nightPanel;
    public Light2D environmentLight;

    // --- CÁC BIẾN CHO ĐẾM NGƯỢC ---
    public TextMeshProUGUI countdownText; 
    public float countdownThreshold = 3f; 
    private bool isCountingDown = false;
    // ------------------------------

    public event Action OnDayStart;
    public event Action OnNightStart;
    public event Action OnDayEnded;
    public bool gameEnded = false;

    private GameObject[] allNPCs;
    private GameObject[] allMurders;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        allNPCs = GameObject.FindGameObjectsWithTag("NPC");
        allMurders = GameObject.FindGameObjectsWithTag("Murder");

        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    // void Start()
    // {
    //     StartDay();
    // }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isNight)
        {
            // Đếm ngược từ Sáng -> Tối
            float timeLeftDay = dayDuration - timer;
            if (timeLeftDay <= countdownThreshold && !isCountingDown && timeLeftDay > 0)
            {
                StartCoroutine(CountdownSequence(dayDuration));
            }

            if (timer >= dayDuration)
            {
                StartNight();
            }
        }
        else
        {
            // Đếm ngược từ Tối -> Sáng
            float timeLeftNight = nightDuration - timer;
            if (timeLeftNight <= countdownThreshold && !isCountingDown && timeLeftNight > 0)
            {
                StartCoroutine(CountdownSequence(nightDuration));
            }

            if (timer >= nightDuration)
            {
                EndNightAndNextDay();
            }
        }
        
        if (gameEnded) return;
        limit1.SetActive(isNight && isLight);
        limit2.SetActive(isNight && isLight);
        limit3.SetActive(isNight && isLight);
        limit4.SetActive(isNight && isLight);
        limit5.SetActive(isNight && isLight);
        limit6.SetActive(isNight && isLight);
    }

    // Coroutine dùng chung cho cả sáng và tối
    IEnumerator CountdownSequence(float targetDuration)
    {
        isCountingDown = true;
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        while (timer < targetDuration)
        {
            if (countdownText != null)
            {
                int secondsDisplay = Mathf.CeilToInt(targetDuration - timer);
                // Đảm bảo không hiện số <= 0 trước khi chuyển giao diện
                if (secondsDisplay > 0) 
                    countdownText.text = secondsDisplay.ToString();
            }
            yield return null; 
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        isCountingDown = false;
    }

    public void StartDay()
    {
        isNight = false;
        timer = 0f;
        
        // Dừng đếm ngược cũ nếu có và ẩn UI
        StopAllCoroutines();
        isCountingDown = false;
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        OnDayStart?.Invoke();
        //nightPanel.alpha = 0;
        environmentLight.color = Color.white;
        Debug.Log("GOOD MORNING!");
        SetNPCActive(true);
        if (audioSource != null && dayMusic != null)
        {
            audioSource.Stop();              // chắc kèo tắt nhạc cũ
            audioSource.clip = dayMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void StartNight()
    {
        isNight = true;
        timer = 0f;
        currentNight++;
        SetNPCActive(false);

        // Dừng đếm ngược cũ nếu có và ẩn UI
        StopAllCoroutines(); 
        isCountingDown = false;
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        OnNightStart?.Invoke();
        //nightPanel.alpha = 1;
        environmentLight.color = new Color(
            31f / 255f,
            31f / 255f,
            61f / 255f
        );
        Debug.Log("IS NIGHT ALREADY? Night = "+ currentNight);
        if (audioSource != null && nightMusic != null)
        {
            audioSource.Stop();              // chắc kèo tắt nhạc cũ
            audioSource.clip = nightMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // Các hàm còn lại giữ nguyên logic cũ của bạn
    void EndNightAndNextDay()
    {
        daysRemaining--;
        OnDayEnded?.Invoke();

        if (daysRemaining <= 0)
        {
            Debug.Log("TIME OUT! GAME OVER");
            gameEnded = true;
            return;
        }

        Debug.Log("DAYS REMAINING: " + daysRemaining);
        StartDay();
    }

    public void ForceSkipNight()
    {
        if (!isNight) return;
        daysRemaining--;
        OnDayEnded?.Invoke();
        if (daysRemaining <= 0)
        {
            gameEnded = true;
            return;
        }
        StartDay();
    }

    void SetNPCActive(bool active)
    {
        foreach (var npc in allNPCs)
        {
            if (npc != null) npc.SetActive(active);
        }
        foreach (var murder in allMurders)
        {
            if (murder != null) murder.SetActive(active);
        }
    }
}
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

    public float dayDuration = 60f;
    public float nightDuration = 60f;

    public int daysRemaining = 7;
    public bool isNight = false;
    public int currentNight = 0;
    private float timer = 0f;
    
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

    void Update()
    {
        // DỪNG THỜI GIAN KHI GAME KẾT THÚC
        if (gameEnded) return;

        timer += Time.deltaTime;

        if (!isNight)
        {
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
    }

    IEnumerator CountdownSequence(float targetDuration)
    {
        isCountingDown = true;
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        while (timer < targetDuration)
        {
            // Kiểm tra nếu game kết thúc thì dừng countdown
            if (gameEnded)
            {
                if (countdownText != null) countdownText.gameObject.SetActive(false);
                isCountingDown = false;
                yield break;
            }

            if (countdownText != null)
            {
                int secondsDisplay = Mathf.CeilToInt(targetDuration - timer);
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

        // THÊM DÒNG NÀY: Xác nhận đồ đã nhặt đêm qua là vĩnh viễn, không phải đồ "tạm" nữa
        if (EvidenceManager.Instance != null)
        {
            EvidenceManager.Instance.nightlyEvidenceTags.Clear(); 
        }
        
        StopAllCoroutines();
        isCountingDown = false;
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        OnDayStart?.Invoke();
        environmentLight.color = Color.white;
        Debug.Log("GOOD MORNING!");
        SetNPCActive(true);
        
        if (audioSource != null && dayMusic != null)
        {
            audioSource.Stop();
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

        StopAllCoroutines(); 
        isCountingDown = false;
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        OnNightStart?.Invoke();
        environmentLight.color = new Color(31f / 255f, 31f / 255f, 61f / 255f);
        Debug.Log("IS NIGHT ALREADY? Night = "+ currentNight);
        
        if (audioSource != null && nightMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = nightMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        if (currentNight == 1)
        {
            PlayerMonologue.Instance.Say("Night is falling... I need to investigate using my UV light...Let me see...Press V right?", onceOnly: true, id: "first_night");
        }
    }

    public void ResumeNightMusic()
    {
        if (!isNight) return;

        if (audioSource && nightMusic)
        {
            audioSource.Stop();
            audioSource.clip = nightMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void EndNightAndNextDay()
    {
        daysRemaining--;
        OnDayEnded?.Invoke();

        if (daysRemaining <= 0)
        {
            Debug.Log("TIME OUT! GAME OVER");
            gameEnded = true;
            // Dừng countdown nếu đang chạy
            StopAllCoroutines();
            if (countdownText != null) countdownText.gameObject.SetActive(false);
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
            // Dừng countdown nếu đang chạy
            StopAllCoroutines();
            if (countdownText != null) countdownText.gameObject.SetActive(false);
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
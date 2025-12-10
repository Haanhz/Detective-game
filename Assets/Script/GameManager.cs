using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject limit1;
    public GameObject limit2;

    public float dayDuration = 60f;
    public float nightDuration = 60f;

    public int daysRemaining = 3;
    public bool isNight = false;
    public int currentNight = 0;
    private float timer = 0f;
    public CanvasGroup nightPanel;

    public event Action OnDayStart;
    public event Action OnNightStart;
    public event Action OnDayEnded;
    public bool gameEnded = false;

    // LƯU REFERENCE CÁC NPC
    private GameObject[] allNPCs;
    private GameObject[] allMurders;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Tìm và lưu tất cả NPC ngay từ đầu (kể cả inactive)
        allNPCs = GameObject.FindGameObjectsWithTag("NPC");
        allMurders = GameObject.FindGameObjectsWithTag("Murder");
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
            if (timer >= dayDuration)
            {
                StartNight();
            }
        }
        else
        {
            if (timer >= nightDuration)
            {
                EndNightAndNextDay();
            }
        }
        
        if (gameEnded) return;
        limit1.SetActive(isNight && currentNight == 1);
        limit2.SetActive(isNight && currentNight == 2);
    }

    void StartDay()
    {
        isNight = false;
        timer = 0f;
        OnDayStart?.Invoke();
        nightPanel.alpha = 0;
        Debug.Log("GOOD MORNING!");
        SetNPCActive(true);
    }

    void StartNight()
    {
        isNight = true;
        timer = 0f;
        currentNight++;
        SetNPCActive(false);

        OnNightStart?.Invoke();
        nightPanel.alpha = 1;
        Debug.Log("IS NIGHT ALREADY? Night = "+ currentNight);
    }

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
            Debug.Log("TIME OUT! GAME OVER");
            gameEnded = true;
            return;
        }

        StartDay();
    }

    void SetNPCActive(bool active)
    {
        // Dùng array đã lưu thay vì tìm lại
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
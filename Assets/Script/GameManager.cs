using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject limit1;
    public GameObject limit2;

    public float dayDuration = 60f;   // 5 phút = 300s
    public float nightDuration = 60f;

    public int daysRemaining = 3;      // đếm ngược 3 ngày
    public bool isNight = false;       // bắt đầu = ban ngày
    public int currentNight = 0;
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
        limit1.SetActive(isNight && currentNight == 1);
        limit2.SetActive(isNight && currentNight == 2);

    }

    // ----- LOGIC -----

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
        // ToggleEvidenceForNight();
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
            // mày có thể gọi UI Game Over ở đây
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
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        GameObject[] murders = GameObject.FindGameObjectsWithTag("Murder");

        foreach (var npc in npcs)
        {
            npc.SetActive(active);
        }
        foreach (var obj in murders)
        {
            obj.SetActive(active);
        }
        
    }

// void ToggleEvidenceForNight()
// {
//     Evidence[] allEvidences = UnityEngine.Object.FindObjectsByType<Evidence>(
//         FindObjectsInactive.Include,
//         FindObjectsSortMode.None
//     );

//     foreach (var ev in allEvidences)
//     {
//         if (ev == null) continue;

//         if (ev.spawnNight == 0)
//         {
//             ev.gameObject.SetActive(true);
//         }
//         else
//         {
//             if (ev.spawnNight == currentNight)
//                 ev.gameObject.SetActive(true);
//             else
//                 ev.gameObject.SetActive(false);
//         }
//     }
// }



}

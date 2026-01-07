using System.Collections;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip chaseMusic;
    public float loseDistance = 10f;
    public Player player;
    public GameObject black;
    public Animator blackAnimator;
    public float probAppear;
    public float killDistance = 1.0f;

    public float moveSpeed = 5f;
    public float spawnDistance = 3f;
    public float chaseDur = 10f;
    public float chaseDelay = 2f;
    private float timer = 0f;
    private float timeAtNight = 0f;
    public bool blackSpawned = false;
    private Rigidbody2D rb;
    private Transform target;
    public float spawnIdleTime = 1.5f;

    private enum State
    {
        EndChase,
        SpawnBlack,
        Chase,
        Kill
    }
    private State currentState = State.EndChase;

    public static ChaseManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        rb = black.GetComponent<Rigidbody2D>();
        if (blackAnimator == null) blackAnimator = black.GetComponent<Animator>();

        target = player.transform;
        EndChase();
        
        GameManager.Instance.OnNightStart += OnNightStart;
        GameManager.Instance.OnDayStart += OnDayStart;
        
        StartCoroutine(CheckAppear());
        StartCoroutine(CalculateProbAppear());
    }

    void OnNightStart()
    {
        timer = 0f;
        timeAtNight = 0f;
    }

    void OnDayStart()
    {
        StopAllCoroutines();
        if (rb) rb.linearVelocity = Vector2.zero;
        
        if (blackAnimator)
        {
            blackAnimator.SetFloat("MoveX", 0);
            blackAnimator.SetFloat("MoveY", 0);
        }

        currentState = State.EndChase;
        EndChase();
        
        StartCoroutine(CheckAppear());
        StartCoroutine(CalculateProbAppear());
    }

    void Update()
    {
        if (!GameManager.Instance.isNight)
        {
            if (currentState != State.EndChase || black.activeSelf)
            {
                currentState = State.EndChase;
                EndChase();
            }
            return;
        }

        switch (currentState)
        {
            case State.SpawnBlack:
                SpawnBlack();
                break;
            case State.Chase:
                Chase();
                break;
            case State.EndChase:
                break;
            case State.Kill:
                break;
        }
    }
    void StopChaseMusic()
{
    if (audioSource && audioSource.isPlaying)
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
    if (GameManager.Instance)
    {
        GameManager.Instance.ResumeNightMusic();
    }
}


    IEnumerator CalculateProbAppear()
    {
        while (true)
        {
            if (!GameManager.Instance.isNight)
            {
                timeAtNight = 0f;
                probAppear = 0f;
                yield return new WaitUntil(() => GameManager.Instance.isNight == true);
            }

            timeAtNight += Time.deltaTime;
            probAppear = 0.1f;
            yield return null;
        }
    }

    IEnumerator CheckAppear()
    {
        WaitForSeconds waitTime = new WaitForSeconds(3f);

        while (true)
        {
            yield return waitTime;
            if (!GameManager.Instance.isNight) continue;
            
            if (currentState == State.EndChase && !blackSpawned)
            {
                if (Random.value < probAppear)
                {
                    currentState = State.SpawnBlack;
                }
            }
        }
    }

    // void SpawnBlack()
    // {
    //     if (!blackSpawned)
    //     {
    //         timer = 0f;
    //         Vector3 playerPos = target.position;
    //         float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    //         Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
    //         Vector3 offset = direction * spawnDistance;
            
    //         black.transform.position = playerPos + offset;
    //         black.SetActive(true);
    //         blackSpawned = true;
            
    //         if(blackAnimator) blackAnimator.ResetTrigger("Kill");
    //         if (audioSource != null && chaseMusic != null && audioSource.isPlaying)
    //     {
    //         audioSource.Stop();   
    //         audioSource.clip = chaseMusic;
    //         audioSource.loop = true;
    //         audioSource.Play();
    //     }
    //     }


    //     timer += Time.deltaTime;
    //     if (timer >= chaseDelay)
    //     {
    //         timer = 0f;
    //         currentState = State.Chase;
    //     }
    // }

    void SpawnBlack()
    {
        if (blackSpawned) return;

        MapTransition[] transitions = FindObjectsByType<MapTransition>(FindObjectsSortMode.None);
        if (transitions.Length == 0) return;

        Transform playerTf = player.transform;

        MapTransition closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tr in transitions)
        {
            float dist = Vector2.Distance(tr.transform.position, playerTf.position);
            if (dist < 2.5f) continue;

            if (dist < minDist)
            {
                minDist = dist;
                closest = tr;
            }
        }

        if (closest == null)
            closest = transitions[Random.Range(0, transitions.Length)];

        black.transform.position = closest.transform.position;
        black.SetActive(true);
        blackSpawned = true;

        if (rb) rb.linearVelocity = Vector2.zero;

        // 👁️ đứng yên nhìn player
        Vector2 lookDir = (playerTf.position - black.transform.position).normalized;
        if (blackAnimator)
        {
            blackAnimator.SetFloat("MoveX", lookDir.x);
            blackAnimator.SetFloat("MoveY", lookDir.y);
        }

        // Nhạc chase (chưa cần gấp, có thể bật sau)
        if (audioSource && chaseMusic)
        {
            audioSource.Stop();
            audioSource.clip = chaseMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        currentState = State.SpawnBlack;
        StartCoroutine(SpawnDelayThenChase());
    }

    IEnumerator SpawnDelayThenChase()
    {
        yield return new WaitForSeconds(spawnIdleTime);
        timer = 0f;
        currentState = State.Chase;
    }

    // Hàm kiểm tra vật cản bằng Raycast
    void Chase()
    {
        float distanceToPlayer = Vector3.Distance(black.transform.position, target.position);

        if (distanceToPlayer > loseDistance)
        {
            StopChaseMusic();
            EndChase();
            currentState = State.EndChase;
            return;
        }

        if (distanceToPlayer <= killDistance && !player.killed)
        {
            currentState = State.Kill;
            if (rb) rb.linearVelocity = Vector2.zero;
            if (blackAnimator) blackAnimator.SetTrigger("Kill");
            StartCoroutine(KillProcess());
            return;
        }

        // --- LOGIC DI CHUYỂN 4 HƯỚNG ---
        Vector3 diff = target.position - black.transform.position;
        Vector2 primaryDir = (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) ? new Vector2(Mathf.Sign(diff.x), 0) : new Vector2(0, Mathf.Sign(diff.y));
        Vector2 secondaryDir = (primaryDir.x != 0) ? new Vector2(0, Mathf.Sign(diff.y)) : new Vector2(Mathf.Sign(diff.x), 0);

        Vector2 finalDir = primaryDir;

        if (IsPathBlocked(primaryDir))
        {
            // Nếu hướng chính bị chặn, thử hướng phụ
            if (!IsPathBlocked(secondaryDir))
            {
                finalDir = secondaryDir;
            }
            else
            {
                // NẾU CẢ 2 BỊ CHẶN: Thử "lách" bằng cách đi ngược lại một chút hoặc đứng yên
                finalDir = Vector2.zero; 
            }
        }

        if (rb) rb.linearVelocity = finalDir * moveSpeed;

        // Cập nhật hiển thị
        black.transform.rotation = Quaternion.identity;
        if (finalDir.x != 0) 
        {
            black.transform.localScale = new Vector3(finalDir.x > 0 ? 1 : -1, 1, 1);
        }

        if (blackAnimator != null)
        {
            blackAnimator.SetFloat("MoveX", finalDir.x);
            blackAnimator.SetFloat("MoveY", finalDir.y);
        }

        timer += Time.deltaTime;
        if (timer >= chaseDur || player.killed)
        {
            timer = 0f;
            StopChaseMusic();
            EndChase();
            currentState = State.EndChase;
        }
    }
    bool IsPathBlocked(Vector2 dir)
    {
        if (dir == Vector2.zero) return false;
        float checkDistance = 0.6f; 
        // Dùng LayerMask để chỉ check va chạm với Tường (Layer Default hoặc Tilemap)
        Vector2 rayStart = (Vector2)black.transform.position + (dir * 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, dir, checkDistance);
        
        if (hit.collider != null && !hit.collider.CompareTag("Player") && hit.collider.gameObject != black)
        {
            return true; 
        }
        return false;
    }
    
    IEnumerator KillProcess()
    {
        yield return new WaitForSeconds(0.5f); 
        
        player.killed = true;
        player.gameObject.SetActive(false);
        Debug.Log("You died by animation!");
        StopChaseMusic();
        EndChase();
        currentState = State.EndChase;
    }

    void EndChase()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
        black.transform.rotation = Quaternion.identity;
        black.transform.localScale = new Vector3(1, 1, 1);
        black.SetActive(false);
        blackSpawned = false;
        timer = 0f;
    }
}
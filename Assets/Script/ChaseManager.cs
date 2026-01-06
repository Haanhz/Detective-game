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

    void SpawnBlack()
    {
        if (!blackSpawned)
        {
            timer = 0f;
            Vector3 playerPos = target.position;
            float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
            Vector3 offset = direction * spawnDistance;
            
            black.transform.position = playerPos + offset;
            black.SetActive(true);
            blackSpawned = true;
            
            if(blackAnimator) blackAnimator.ResetTrigger("Kill");
            if (audioSource != null && chaseMusic != null && audioSource.isPlaying)
        {
            audioSource.Stop();   
            audioSource.clip = chaseMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        }


        timer += Time.deltaTime;
        if (timer >= chaseDelay)
        {
            timer = 0f;
            currentState = State.Chase;
        }
    }

    // void SpawnBlack()
    // {
    //     if (!blackSpawned)
    //     {
    //         timer = 0f;
    //         Vector3 spawnPos = Vector3.zero;
    //         bool validSpot = false;
    //         int attempts = 0;

    //         // Thử tìm vị trí trống tối đa 10 lần
    //         while (!validSpot && attempts < 20) // Thử nhiều lần hơn
    //         {
    //             // Spawn xa hơn một chút để tránh kẹt vào các căn phòng nhỏ
    //             float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    //             Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
    //             spawnPos = target.position + (direction * (spawnDistance + 2f)); 

    //             // Kiểm tra một vùng rộng (radius 1.0f) xem có chạm Collider-Wall không
    //             Collider2D hit = Physics2D.OverlapCircle(spawnPos, 1.0f); 
    //             if (hit == null) validSpot = true;
    //             attempts++;
    //         }

    //         black.transform.position = spawnPos;
    //         black.transform.rotation = Quaternion.identity; // Reset xoay để tránh bị ngược
    //         black.SetActive(true);
    //         blackSpawned = true;
            
    //         if(blackAnimator) blackAnimator.ResetTrigger("Kill");
            
    //         // Sửa lỗi logic âm thanh (kiểm tra audioSource và phát nhạc)
    //         if (audioSource != null && chaseMusic != null)
    //         {
    //             audioSource.clip = chaseMusic;
    //             audioSource.loop = true;
    //             if (!audioSource.isPlaying) audioSource.Play();
    //         }
    //     }

    //     timer += Time.deltaTime;
    //     if (timer >= chaseDelay)
    //     {
    //         timer = 0f;
    //         currentState = State.Chase;
    //     }
    // }
//         void SpawnBlack()
// {
//     if (!blackSpawned)
//     {
//         timer = 0f;
//         Vector3 playerPos = target.position;
        
//         // Lấy hướng player đang nhìn từ animator
//         float lastX = target.GetComponent<Animator>().GetFloat("LastInputX");
//         float lastY = target.GetComponent<Animator>().GetFloat("LastInputY");
        
//         // Hướng ngược lại = sau lưng player
//         Vector3 direction = new Vector3(-lastX, -lastY, 0).normalized;
        
//         // NẾU KHÔNG CÓ HƯỚNG (player đứng yên), random 1 hướng
//         if (direction == Vector3.zero)
//         {
//             float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
//             direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
//         }
        
//         Vector3 spawnPos = Vector3.zero;
//         bool foundValidSpot = false;
//         int maxAttempts = 20;
//         int attempts = 0;
        
//         // Thử tìm vị trí hợp lệ
//         while (!foundValidSpot && attempts < maxAttempts)
//         {
//             // Tính vị trí spawn
//             if (attempts == 0)
//             {
//                 // Lần đầu: thử sau lưng player
//                 spawnPos = playerPos + (direction * spawnDistance);
//             }
//             else
//             {
//                 // Các lần sau: thử random xung quanh player
//                 float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
//                 Vector3 randomDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
//                 spawnPos = playerPos + (randomDir * spawnDistance);
//             }
            
//             // Kiểm tra xem vị trí có trống không (radius 0.5f để check vùng xung quanh)
//             Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.5f);
            
//             // Nếu không có collider HOẶC chỉ va chạm với Player thì OK
//             if (hit == null || hit.CompareTag("Player"))
//             {
//                 foundValidSpot = true;
//             }
            
//             attempts++;
//         }
        
//         // Nếu sau 20 lần vẫn không tìm được → spawn xa hơn
//         if (!foundValidSpot)
//         {
//             spawnPos = playerPos + (direction * (spawnDistance + 3f));
//         }
        
//         black.transform.position = spawnPos;
//         black.transform.rotation = Quaternion.identity; // Reset rotation
//         black.SetActive(true);
//         blackSpawned = true;
        
//         if(blackAnimator) blackAnimator.ResetTrigger("Kill");
        
//         // Sửa logic âm thanh
//         if (audioSource != null && chaseMusic != null)
//         {
//             if (audioSource.isPlaying) audioSource.Stop();
//             audioSource.clip = chaseMusic;
//             audioSource.loop = true;
//             audioSource.Play();
//         }
//     }

//     timer += Time.deltaTime;
//     if (timer >= chaseDelay)
//     {
//         timer = 0f;
//         currentState = State.Chase;
//     }
// }

    // void Chase()
    // {
    //     float distanceToPlayer = Vector3.Distance(black.transform.position, target.position);
    //     if (distanceToPlayer > loseDistance)
    //     {
    //         StopChaseMusic();
    //         EndChase();
    //         currentState = State.EndChase;
    //         return;
    //     }
    //     if (distanceToPlayer <= killDistance && !player.killed)
    //     {
    //         currentState = State.Kill;
    //         if(rb) rb.linearVelocity = Vector2.zero;

    //         if (blackAnimator)
    //         {
    //             blackAnimator.SetFloat("MoveX", 0);
    //             blackAnimator.SetFloat("MoveY", 0);
    //             blackAnimator.SetTrigger("Kill");
    //         }
            
    //         StartCoroutine(KillProcess()); 
    //         return;
    //     }

    //     Vector3 diff = target.position - black.transform.position;
    //     Vector2 moveDir = Vector2.zero;

    //     if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
    //     {
    //         moveDir = new Vector2(Mathf.Sign(diff.x), 0);
    //     }
    //     else
    //     {
    //         moveDir = new Vector2(0, Mathf.Sign(diff.y));
    //     }

    //     if(rb) rb.linearVelocity = moveDir * moveSpeed;

    //     if (moveDir.x != 0) 
    //     {
    //         black.transform.localScale = new Vector3(moveDir.x > 0 ? 1 : -1, 1, 1);
    //     }

    //     if (blackAnimator != null)
    //     {
    //         blackAnimator.SetFloat("MoveX", moveDir.x);
    //         blackAnimator.SetFloat("MoveY", moveDir.y);
    //     }

    //     timer += Time.deltaTime;
        
    //     if (timer >= chaseDur || player.killed)
    //     {
    //         timer = 0f;
    //         if(rb) rb.linearVelocity = Vector2.zero;
            
    //         if(blackAnimator) 
    //         {
    //             blackAnimator.SetFloat("MoveX", 0);
    //             blackAnimator.SetFloat("MoveY", 0);
    //         }
    //         StopChaseMusic();
    //         EndChase();
    //         currentState = State.EndChase;
    //     }
    // }

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
using System.Collections;
using System.Collections.Generic;
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
            // ✅ XÓA case SpawnBlack vì chỉ cần gọi 1 lần
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
            probAppear = 0.2f;
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
                    // ✅ Gọi SpawnBlack() trực tiếp
                    SpawnBlack();
                }
            }
        }
    }

    // void SpawnBlack()
    // {
    //     if (blackSpawned) return;

    //     Transform playerTf = player.transform;

    //     // ✅ Lấy hướng từ Animator của Player
    //     Vector2 playerFacing = Vector2.down; // Default

    //     if (player.animator != null)
    //     {
    //         float lastX = player.animator.GetFloat("LastInputX");
    //         float lastY = player.animator.GetFloat("LastInputY");

    //         if (lastX != 0 || lastY != 0)
    //         {
    //             playerFacing = new Vector2(lastX, lastY).normalized;
    //         }
    //         else
    //         {
    //             float inputX = player.animator.GetFloat("InputX");
    //             float inputY = player.animator.GetFloat("InputY");
    //             if (inputX != 0 || inputY != 0)
    //             {
    //                 playerFacing = new Vector2(inputX, inputY).normalized;
    //             }
    //         }
    //     }

    //     // ✅ Spawn NGƯỢC hướng player đang nhìn, cách 2f
    //     Vector2 spawnDirection = -playerFacing;
    //     Vector3 spawnPos = playerTf.position + (Vector3)(spawnDirection * 3f);

    //     // ✅ CHECK VA CHẠM - nếu có tường phía sau → spawn bên cạnh
    //     RaycastHit2D hit = Physics2D.Raycast(playerTf.position, spawnDirection, 2f);

    //     if (hit.collider != null && !hit.collider.CompareTag("Player"))
    //     {
    //         // Thử spawn bên cạnh thay vì phía sau
    //         Vector2[] altDirections = {
    //             new Vector2(-spawnDirection.y, spawnDirection.x),  // Vuông góc phải
    //             new Vector2(spawnDirection.y, -spawnDirection.x)   // Vuông góc trái
    //         };

    //         bool foundSpot = false;
    //         foreach (var altDir in altDirections)
    //         {
    //             RaycastHit2D altHit = Physics2D.Raycast(playerTf.position, altDir, 2f);
    //             if (altHit.collider == null || altHit.collider.CompareTag("Player"))
    //             {
    //                 spawnPos = playerTf.position + (Vector3)(altDir * 2f);
    //                 foundSpot = true;
    //                 break;
    //             }
    //         }

    //         // Nếu tất cả hướng đều bị chặn → spawn random
    //         if (!foundSpot)
    //         {
    //             float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    //             Vector2 randomDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    //             spawnPos = playerTf.position + (Vector3)(randomDir * 2f);
    //         }
    //     }

    //     black.transform.position = spawnPos;
    //     black.SetActive(true);
    //     blackSpawned = true;

    //     if (rb) rb.linearVelocity = Vector2.zero;

    //     // 👁️ Black nhìn về phía player
    //     Vector2 lookDir = (playerTf.position - black.transform.position).normalized;
    //     if (blackAnimator)
    //     {
    //         blackAnimator.SetFloat("MoveX", lookDir.x);
    //         blackAnimator.SetFloat("MoveY", lookDir.y);
    //     }

    //     // 🎵 Nhạc chase
    //     if (audioSource && chaseMusic)
    //     {
    //         audioSource.Stop();
    //         audioSource.clip = chaseMusic;
    //         audioSource.loop = true;
    //         audioSource.Play();
    //     }

    //     currentState = State.SpawnBlack;
    //     StartCoroutine(SpawnDelayThenChase());
    // }
    // ✅ Thêm biến này vào class
    public List<Transform> spawnPoints = new List<Transform>(); // Gán trong Inspector
    public float maxSpawnDistance = 10f; // Khoảng cách tối đa để chọn spawn point
    public float minSpawnDistance = 3f;
    void SpawnBlack()
    {
        if (blackSpawned) return;

        Transform playerTf = player.transform;
        Vector3 spawnPos = playerTf.position;

        // ✅ PRIORITY 1: Spawn ở telepoint an toàn
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            List<Transform> validPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;

                float distance = Vector2.Distance(playerTf.position, point.position);

                // ✅ CHECK: Phải trong khoảng MIN và MAX
                if (distance >= minSpawnDistance && distance <= maxSpawnDistance)
                {
                    // Check tường chặn
                    Vector2 direction = (point.position - playerTf.position).normalized;
                    RaycastHit2D hit = Physics2D.Raycast(playerTf.position, direction, distance);

                    if (hit.collider == null || hit.collider.CompareTag("Player"))
                    {
                        validPoints.Add(point);
                    }
                }
            }

            if (validPoints.Count > 0)
            {
                Transform chosenPoint = validPoints[Random.Range(0, validPoints.Count)];
                spawnPos = chosenPoint.position;
                Debug.Log($"Spawned at telepoint: {chosenPoint.name}");
            }
            else
            {
                Debug.Log("No valid spawn points (too close or too far), using fallback");
                spawnPos = GetFallbackSpawnPosition(playerTf);
            }
        }
        else
        {
            Debug.Log("No spawn points configured, using fallback method");
            spawnPos = GetFallbackSpawnPosition(playerTf);
        }

        // ✅ SAFETY CHECK CUỐI: Nếu spawn pos quá gần player → delay spawn
        float finalDistance = Vector2.Distance(playerTf.position, spawnPos);
        if (finalDistance < minSpawnDistance)
        {
            Debug.LogWarning($"Spawn position too close ({finalDistance:F1}m), aborting spawn");
            blackSpawned = false; // Reset để thử lại lần sau
            return;
        }

        // Spawn Black
        black.transform.position = spawnPos;
        black.SetActive(true);
        blackSpawned = true;

        if (rb) rb.linearVelocity = Vector2.zero;

        // 👁️ Black nhìn về phía player
        // 👁️ Black nhìn về phía player (ĐẢO NGƯỢC)
        Vector2 lookDir = (black.transform.position - playerTf.position).normalized; // ✅ Đổi chiều
        if (blackAnimator)
        {
            blackAnimator.SetFloat("MoveX", lookDir.x);
            blackAnimator.SetFloat("MoveY", lookDir.y);
        }

        // 🎵 Nhạc chase
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

    // ✅ Tách logic cũ thành hàm fallback
    Vector3 GetFallbackSpawnPosition(Transform playerTf)
    {
        Vector2 playerFacing = Vector2.down; // Default

        if (player.animator != null)
        {
            float lastX = player.animator.GetFloat("LastInputX");
            float lastY = player.animator.GetFloat("LastInputY");

            if (lastX != 0 || lastY != 0)
            {
                playerFacing = new Vector2(lastX, lastY).normalized;
            }
            else
            {
                float inputX = player.animator.GetFloat("InputX");
                float inputY = player.animator.GetFloat("InputY");
                if (inputX != 0 || inputY != 0)
                {
                    playerFacing = new Vector2(inputX, inputY).normalized;
                }
            }
        }

        Vector2 spawnDirection = -playerFacing;
        Vector3 spawnPos = playerTf.position + (Vector3)(spawnDirection * 3f);

        RaycastHit2D hit = Physics2D.Raycast(playerTf.position, spawnDirection, 2f);

        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            Vector2[] altDirections = {
            new Vector2(-spawnDirection.y, spawnDirection.x),
            new Vector2(spawnDirection.y, -spawnDirection.x)
        };

            bool foundSpot = false;
            foreach (var altDir in altDirections)
            {
                RaycastHit2D altHit = Physics2D.Raycast(playerTf.position, altDir, 2f);
                if (altHit.collider == null || altHit.collider.CompareTag("Player"))
                {
                    spawnPos = playerTf.position + (Vector3)(altDir * 2f);
                    foundSpot = true;
                    break;
                }
            }

            if (!foundSpot)
            {
                float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 randomDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                spawnPos = playerTf.position + (Vector3)(randomDir * 2f);
            }
        }

        return spawnPos;
    }

    IEnumerator SpawnDelayThenChase()
    {
        yield return new WaitForSeconds(spawnIdleTime);
        timer = 0f;
        currentState = State.Chase;
    }

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
                // NẾU CẢ 2 BỊ CHẶN: đứng yên
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
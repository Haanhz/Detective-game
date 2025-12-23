using System.Collections;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public Player player;
    public GameObject black;
    public Animator blackAnimator;
    public float probAppear;
    public float killDistance = 1.0f; // Khoảng cách để trigger Kill

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


    //Singleton pattern
    public static ChaseManager instance;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance.gameObject);
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
        // reset cơ hội xuất hiện mỗi đêm nếu muốn
        timer = 0f;
        timeAtNight = 0f;
    }

    void OnDayStart()
    {
        EndChase(); // ban ngày thì tắt quái
    }

    void Update()
    {
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

            //probAppear = Mathf.Pow(1.22f, timeAtNight / 60f) / 100f;
            probAppear = 0.5f;
            yield return null;
        }
    }


    IEnumerator CheckAppear()
    {
        WaitForSeconds waitTime = new WaitForSeconds(3f); // Cache to avoid GC

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
            // Spawn random quanh player nhưng làm tròn vị trí để khớp pixel (tuỳ chọn)
            float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
            Vector3 offset = direction * spawnDistance;
            
            black.transform.position = playerPos + offset;
            black.SetActive(true);
            blackSpawned = true;
            
            // Reset animation
            if(blackAnimator) blackAnimator.ResetTrigger("Kill");
        }

        timer += Time.deltaTime;
        if (timer >= chaseDelay)
        {
            timer = 0f;
            currentState = State.Chase;
        }
    }

    void Chase()
    {
        // 1. LOGIC KILL (Nếu quá gần thì giết)
        float distanceToPlayer = Vector3.Distance(black.transform.position, target.position);
        
        // Nếu gần player và player chưa chết
        if (distanceToPlayer <= killDistance && !player.dead)
        {
            currentState = State.Kill;
            if(rb) rb.linearVelocity = Vector2.zero; // Dừng lại

            // Báo animator chuyển sang kill
            if (blackAnimator)
            {
                blackAnimator.SetFloat("MoveX", 0);
                blackAnimator.SetFloat("MoveY", 0);
                blackAnimator.SetTrigger("Kill");
            }
            
            StartCoroutine(KillProcess()); 
            return;
        }

        // 2. TÍNH TOÁN DI CHUYỂN 4 HƯỚNG
        Vector3 diff = target.position - black.transform.position;
        Vector2 moveDir = Vector2.zero;

        // So sánh khoảng cách X và Y để chọn hướng đi (Manhattan Movement)
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // Đi theo chiều Ngang (Trái/Phải)
            moveDir = new Vector2(Mathf.Sign(diff.x), 0);
        }
        else
        {
            // Đi theo chiều Dọc (Lên/Xuống)
            moveDir = new Vector2(0, Mathf.Sign(diff.y));
        }

        // Áp dụng vận tốc cho Rigidbody
        if(rb) rb.linearVelocity = moveDir * moveSpeed;


        // 3. XỬ LÝ ANIMATION VÀ FLIP (LẬT MẶT)
        // Đây là đoạn code giúp Black lật mặt giống Player
        if (moveDir.x != 0) 
        {
            if (moveDir.x > 0)
            {
                // Chạy sang PHẢI -> Scale (1, 1, 1)
                black.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                // Chạy sang TRÁI -> Scale (-1, 1, 1) -> Lật ngược lại
                black.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        // Lưu ý: Nếu đi lên/xuống (moveDir.x == 0) thì giữ nguyên hướng mặt hiện tại


        // Cập nhật tham số cho Animator (để chạy Blend Tree)
        if (blackAnimator != null)
        {
            blackAnimator.SetFloat("MoveX", moveDir.x);
            blackAnimator.SetFloat("MoveY", moveDir.y);
        }


        // 4. KIỂM TRA THỜI GIAN CHASE
        timer += Time.deltaTime;
        
        // Hết thời gian đuổi hoặc Player đã chết thì dừng
        if (timer >= chaseDur || player.dead)
        {
            timer = 0f;
            if(rb) rb.linearVelocity = Vector2.zero;
            
            // Reset Animator về Idle
            if(blackAnimator) 
            {
                blackAnimator.SetFloat("MoveX", 0);
                blackAnimator.SetFloat("MoveY", 0);
            }

            EndChase();
            currentState = State.EndChase;
        }
    }
    
    // Coroutine xử lý sau khi kill (chờ animation chạy xong một chút rồi mới End game)
    IEnumerator KillProcess()
    {
        // Chờ khoảng 0.5s hoặc độ dài animation kill
        yield return new WaitForSeconds(0.5f); 
        
        // Logic chết cũ của bạn
        player.dead = true;
        player.gameObject.SetActive(false);
        // ScoreBoard.scoreValue = 0; // Nếu có script này
        Debug.Log("You died by animation!");
        
        // Sau khi kill xong thì end chase
        EndChase();
        currentState = State.EndChase;
    }

    void EndChase()
    {
        black.SetActive(false);
        blackSpawned = false;
        timer = 0f;
    }


}

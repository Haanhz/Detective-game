using System.Collections;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
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
            probAppear = 0.3f;
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
        float distanceToPlayer = Vector3.Distance(black.transform.position, target.position);
        
        if (distanceToPlayer <= killDistance && !player.dead)
        {
            currentState = State.Kill;
            if(rb) rb.linearVelocity = Vector2.zero;

            if (blackAnimator)
            {
                blackAnimator.SetFloat("MoveX", 0);
                blackAnimator.SetFloat("MoveY", 0);
                blackAnimator.SetTrigger("Kill");
            }
            
            StartCoroutine(KillProcess()); 
            return;
        }

        Vector3 diff = target.position - black.transform.position;
        Vector2 moveDir = Vector2.zero;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            moveDir = new Vector2(Mathf.Sign(diff.x), 0);
        }
        else
        {
            moveDir = new Vector2(0, Mathf.Sign(diff.y));
        }

        if(rb) rb.linearVelocity = moveDir * moveSpeed;

        if (moveDir.x != 0) 
        {
            black.transform.localScale = new Vector3(moveDir.x > 0 ? 1 : -1, 1, 1);
        }

        if (blackAnimator != null)
        {
            blackAnimator.SetFloat("MoveX", moveDir.x);
            blackAnimator.SetFloat("MoveY", moveDir.y);
        }

        timer += Time.deltaTime;
        
        if (timer >= chaseDur || player.dead)
        {
            timer = 0f;
            if(rb) rb.linearVelocity = Vector2.zero;
            
            if(blackAnimator) 
            {
                blackAnimator.SetFloat("MoveX", 0);
                blackAnimator.SetFloat("MoveY", 0);
            }

            EndChase();
            currentState = State.EndChase;
        }
    }
    
    IEnumerator KillProcess()
    {
        yield return new WaitForSeconds(0.5f); 
        
        player.dead = true;
        player.gameObject.SetActive(false);
        Debug.Log("You died by animation!");
        
        EndChase();
        currentState = State.EndChase;
    }

    void EndChase()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
        black.SetActive(false);
        blackSpawned = false;
        timer = 0f;
    }
}
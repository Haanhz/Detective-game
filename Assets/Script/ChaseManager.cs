using System.Collections;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public Player player;
    public GameObject black;
    public Evidence evidence;
    public float probAppear;
    public float timeAtNight = 0f;
    public float nightLength = 300f;
    public bool isNight = true;
    public float moveSpeed = 5f;
    public float spawnDistance = 3f;
    public float chaseDur = 10f;
    public float chaseDelay = 2f;
    private float timer = 0f;
    public bool blackSpawned = false;
    private Rigidbody2D rb;
    private Transform target;
    private enum State
    {
        EndChase,
        SpawnBlack,
        Chase
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
        target = player.transform;
        EndChase();
        StartCoroutine(CheckAppear());
        StartCoroutine(CalculateProbAppear());
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
                //EndChase(); 
                break;

        }
    }

    IEnumerator CalculateProbAppear()
    {
        while (true)
        {
            if (isNight)
            {
                timeAtNight += Time.deltaTime;
                //probAppear = Mathf.Pow(1.22f, timeAtNight / 60f) / 100f;
                probAppear = 0.5f;
                if (timeAtNight >= nightLength)
                {
                    isNight = false;
                    timeAtNight = 0f;
                    probAppear = 0f;
                }
            }
            else
            {
                yield return new WaitUntil(() => isNight == true);
            }

            yield return null;
        }
    }

    IEnumerator CheckAppear()
    {
        WaitForSeconds waitTime = new WaitForSeconds(3f); // Cache to avoid GC

        while (true)
        {
            yield return waitTime;

            if (currentState == State.EndChase && isNight)
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
        //chase logic
        Vector3 direction = (target.position - black.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
        rb.linearVelocity = new Vector2(direction.x, direction.y) * moveSpeed;

        timer += Time.deltaTime;
        if (timer >= chaseDur)
        {
            timer = 0f;
            rb.linearVelocity = Vector2.zero;
            EndChase();
            currentState = State.EndChase;
        }
        else if (timer < chaseDur && player.dead == true)
        {
            timer = 0f;
            rb.linearVelocity = Vector2.zero;
        }

    }

    void EndChase()
    {
        black.SetActive(false);
        blackSpawned = false;
        timer = 0f;
    }


}

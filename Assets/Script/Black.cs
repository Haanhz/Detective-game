using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Black : MonoBehaviour
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = ChaseManager.instance.black.GetComponent<Rigidbody2D>();
        target = ChaseManager.instance.player.transform;
        EndChase();
        StartCoroutine(CheckAppear());
    }

    IEnumerator CheckAppear()
    {
        WaitForSeconds waitTime = new WaitForSeconds(3f); // Cache to avoid GC
        
        while (true)
        {
            yield return waitTime;
            
            if (currentState == State.EndChase && ChaseManager.instance.isNight)
            {
                if (Random.value < ChaseManager.instance.probAppear)
                {
                    currentState = State.SpawnBlack; 
                }
            }
        }
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
  

    void SpawnBlack()
    {
        if (!blackSpawned)
        {
            timer = 0f;
            Vector3 playerPos = target.position;
            float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
            Vector3 offset = direction * spawnDistance;
            ChaseManager.instance.black.transform.position = playerPos + offset;
            ChaseManager.instance.black.SetActive(true);
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
        //......chase logic
        Vector3 direction = (target.position - ChaseManager.instance.black.transform.position).normalized;
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
        
    }
    
    void EndChase()
    {
        ChaseManager.instance.black.SetActive(false);
        blackSpawned = false;
        timer = 0f;
    }
}
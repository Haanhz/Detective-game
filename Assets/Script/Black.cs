using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Black : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float spawnDistance = 3f;
    public float chaseDur = 10f;
    public float chaseDelay = 2f;
    public int currentState = 2;
    public GameObject black;
    private float timer = 0f;
    public bool blackSpawned = false;
    private Dictionary<string, int> state = new Dictionary<string, int>()
    {
        ["SpawnBlack"] = 0,
        ["Chase"] = 1,
        ["EndChase"] = 2
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = state["EndChase"];
        StartCoroutine(CheckAppear());
    }

    IEnumerator CheckAppear()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f); // mỗi 3s mới check 1 lần
            if (currentState == state["EndChase"] && ChaseManager.instance.isNight)
            {
                if (Random.value < ChaseManager.instance.probAppear)
                {
                    currentState = state["SpawnBlack"];
                }
            }
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case 0:
                SpawnBlack();
                break;
            case 1:
                Chase();
                break;
            case 2:
                EndChase();
                break;
        }
    }
  

    void SpawnBlack()
    {
        if (!blackSpawned)
        {
            Vector3 playerPos = ChaseManager.instance.player.transform.position;
            float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
            Vector3 offset = direction * spawnDistance;
            transform.position = playerPos + offset;
            black.SetActive(true);
            blackSpawned = true;
        }
        
        timer += Time.deltaTime;
        if (timer >= chaseDelay)
        {
            timer = 0f;
            currentState = state["Chase"];
        }
    }


    void Chase()
    {
        //......chase logic
        timer += Time.deltaTime;
        if (timer >= chaseDur)
        {
            timer = 0f;
            currentState = state["EndChase"];
        }
        
    }
    
    void EndChase()
    {
        black.SetActive(false);
        blackSpawned = false;
    }
}

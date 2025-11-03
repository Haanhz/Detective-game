using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Black : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float spawnDistance = 5f;
    public float chaseDur = 10f;
    public float chaseDelay = 2f;
    public int currentState = 2;
    public GameObject black;
    private float timer = 0f;
    private Dictionary<string, int> state = new Dictionary<string, int>()
    {
        ["Appear"] = 0,
        ["Chase"] = 1,
        ["EndChase"] = 2
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = state["EndChase"];
    }

    // Update is called once per frame
    void Update()
    {
        
        //nếu random 1 value nhỏ hơn probAppear thì black xuất hiện
        if (Random.value < ChaseManager.instance.probAppear && currentState == 2 && ChaseManager.instance.isNight)
        {
            currentState = state["Appear"];
        }
        switch (currentState)
        {
            case 0:
                Appear();
                break;
            case 1:
                Chase();
                break;
            case 2:
                EndChase();
                break;
        }
    }

    void Appear()
    {
        Vector3 playerPos = ChaseManager.instance.player.transform.position;
        //float rad = spawnAngle * Mathf.Deg2Rad;
        float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * spawnDistance;
        transform.position = playerPos + offset;
        black.SetActive(true);
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
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public float maxStamina = 100f;
    public float currentStamina;
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float tiredSpeed = 3.5f;
    public float staminaMoveSpeed = 0.1f;
    public float staminaRunSpeed = 0.15f;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina;
        Console.WriteLine("current stamina: ", currentStamina);
    }

    // Update is called once per frame
    void Update()
    {
        Move(moveSpeed);
        if (Input.GetKeyDown(KeyCode.X))
        {
            Run();
        }
    }

    public void Move(float speed)
    {
        float moveX = 0f;
        float moveY = 0f;
        //Khi người chơi ấn phím thì nhân vật di chuyển
        if (Input.GetKey(KeyCode.W) | Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1f;
        }
        if (Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow))
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow))
        {
            moveY = +1f;
        }

        Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = moveDir * speed;

        currentStamina -= staminaMoveSpeed * Time.deltaTime;
        Console.WriteLine("current stamina: ", currentStamina);
    }
    
    public void Run()
    {
        Move(runSpeed);
        currentStamina -= staminaRunSpeed * Time.deltaTime;
        Console.WriteLine("current stamina: ", currentStamina);
    }
}

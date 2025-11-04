using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public float maxStamina = 100f;
    public float currentStamina;
    public float moveSpeed = 4f;
    public float runSpeed = 10f;
    public float tiredSpeed = 1f;
    public float staminaMoveSpeed = 0.1f;
    public float staminaRunSpeed = 0.15f;
    public bool idle = true;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.X))
        {
            Run();
            idle = false;
        }
        else
        {
            Move(moveSpeed);
            idle = true;
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
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow))
        {
            moveX = +1f;
        }

        Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        transform.Translate(moveDir * speed * Time.deltaTime);

        if (IsMoving())
        {
            currentStamina -= staminaMoveSpeed * Time.deltaTime;
        }
    }

    public void Run()
    {
        Move(runSpeed);
        if (IsMoving())
        {
            currentStamina -= staminaRunSpeed * Time.deltaTime;
        }
    }
    
    private bool IsMoving()
    {
    return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
           Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
           Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
           Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
    }
}

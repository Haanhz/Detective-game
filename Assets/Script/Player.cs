using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public Vector2Int direction = Vector2Int.right;// (1,0)
    private Vector2Int input;
    public float maxStamina = 100f;
    public float currentStamina;
    public float moveSpeed = 4f;
    public float runSpeed = 10f;
    public float tiredSpeed = 1f;
    public float staminaMoveSpeed = 0.1f;
    public float staminaRunSpeed = 0.15f;
    public bool dead = false;
    private float timer = 0f;
    private bool canRun = true;
    private bool isRunning = false;
    public float runDuration = 5f;
    public float cooldown = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStamina = maxStamina;
    }

    // Update is called once per frame
void Update()
{
    if (dead) return;

    if (!IsMoving())
    {
        currentStamina += 0.01f * Time.deltaTime;   // hồi hợp lý hơn
    }
    else
    {
        if (Input.GetKey(KeyCode.X) && canRun)
        {
            isRunning = true;
            timer += Time.deltaTime;

            Move(runSpeed);

            if (timer >= runDuration)
            {
                canRun = false;
                isRunning = false;
                timer = 0f;
            }
        }
        else
        {
            isRunning = false;

            if (currentStamina <= 0.3f * maxStamina)
                Move(tiredSpeed);
            else
                Move(moveSpeed);

            if (!canRun)
            {
                timer += Time.deltaTime;
                if (timer >= cooldown)
                {
                    canRun = true;
                    timer = 0f;
                }
            }
        }
    }
}


public void Move(float speed)
{
    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        input = Vector2Int.up;
    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        input = Vector2Int.down;
    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        input = Vector2Int.right;
    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        input = Vector2Int.left;

    if (input != Vector2Int.zero)
        direction = input;

    float x = transform.position.x + direction.x * speed * Time.deltaTime;
    float y = transform.position.y + direction.y * speed * Time.deltaTime;
    transform.position = new Vector2(x, y);

    RotateHead();

    if (IsMoving())
    {
        if (Input.GetKey(KeyCode.X) && canRun)
            currentStamina -= staminaRunSpeed * Time.deltaTime;
        else
            currentStamina -= staminaMoveSpeed * Time.deltaTime;
    }
}

    
    void RotateHead() {
        if (direction == Vector2Int.up) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (direction == Vector2Int.down) {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        } else if (direction == Vector2Int.left) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (direction == Vector2Int.right) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
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

    void OnCollisionStay2D(Collision2D collision)
{
    if (!collision.collider.CompareTag("Bed")) return;

    // Nhấn F khi đang chạm giường
    if (Input.GetKeyDown(KeyCode.F))
    {
        if (GameManager.Instance.isNight)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + 20f);
            GameManager.Instance.ForceSkipNight();
            Debug.Log("You go to sleep and wake up the next morning!");
        }
        else
        {
            Debug.Log("You are not sleepy!");
        }
    }
}

}

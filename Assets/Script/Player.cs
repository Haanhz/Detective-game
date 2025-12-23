using System;
using System.Threading;
using JetBrains.Annotations;
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
    public float runDuration = 5f;
    public float cooldown = 5f;

    public Animator animator;

    // Public getter for animator to ensure safe external access
    public Animator GetAnimator()
    {
        return animator;
    }

    void Start()
    {
        currentStamina = maxStamina;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (dead) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Catch");
        }

        bool moving = IsMoving();

        // Lấy input thô
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        if (moving)
        {
            // Move params
            animator.SetFloat("InputX", inputX);
            animator.SetFloat("InputY", inputY);

            // Lưu hướng cuối cho Idle
            animator.SetFloat("LastInputX", inputX); 
            animator.SetFloat("LastInputY", inputY); 
        }
        else
        {
            // Không di chuyển -> cho Move = 0
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
        }

        if (!moving)
        {
            currentStamina += 0.01f * Time.deltaTime;
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
        else
        {
            bool isPressingRun = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && canRun;

            if (isPressingRun)
            {
                timer += Time.deltaTime;
                Move(runSpeed);

                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);

                if (timer >= runDuration)
                {
                    canRun = false;
                    timer = 0f;
                }
            }
            else
            {
                if (currentStamina <= 0.3f * maxStamina)
                    Move(tiredSpeed);
                else
                    Move(moveSpeed);

                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);

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
        Vector2Int currentInput = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) currentInput = Vector2Int.up;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) currentInput = Vector2Int.down;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) currentInput = Vector2Int.right;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) currentInput = Vector2Int.left;

        if (currentInput != Vector2Int.zero)
            direction = currentInput;

        float x = transform.position.x + direction.x * speed * Time.deltaTime;
        float y = transform.position.y + direction.y * speed * Time.deltaTime;
        transform.position = new Vector2(x, y);

        RotateHead();

        if (IsMoving())
        {
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && canRun)
                currentStamina -= staminaRunSpeed * Time.deltaTime;
            else
                currentStamina -= staminaMoveSpeed * Time.deltaTime;
        }
    }

    void RotateHead()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }

    private bool IsMoving()
    {
        return Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Bed")) return;

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

using System;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Interaction UI")]
    [Header("Fridge Settings")]
    public float fridgeCooldown = 10f; // Thời gian chờ giữa 2 lần ăn (giây)
    private float lastEatTime = -100f; // Mốc thời gian lần cuối ăn (để mặc định đủ xa)
    public GameObject interactIndicator; // Kéo Sprite dấu chấm than vào đây
    public float detectionRange = 1.5f;   // Khoảng cách phát hiện
    public System.Collections.Generic.List<string> interactableTags = new System.Collections.Generic.List<string> 
    { 
        "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", 
        "Limit1", "Limit2", "Hide", "Bed", "Murder", "NPC" 
    };
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
        if (interactIndicator != null) interactIndicator.SetActive(false);
    }

    void Update()
    {
        CheckForInteractables();

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
        string hitTag = collision.collider.tag;

        if (hitTag == "Fridge")
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Kiểm tra Cooldown: Thời gian hiện tại phải lớn hơn (lần ăn cuối + thời gian chờ)
                if (Time.time >= lastEatTime + fridgeCooldown)
                {
                    if (currentStamina < maxStamina)
                    {
                        currentStamina = Mathf.Min(maxStamina, currentStamina + 5f);
                        Debug.Log("You ate. Stamina restored!");
                        
                        // CẬP NHẬT MỐC THỜI GIAN VỪA ĂN
                        lastEatTime = Time.time;
                    }
                    else
                    {
                        Debug.Log("You are full!");
                    }
                }
                else
                {
                    // Tính toán thời gian còn lại để in ra console
                    float timeLeft = (lastEatTime + fridgeCooldown) - Time.time;
                    Debug.Log("You are still full! Wait " + Mathf.Ceil(timeLeft) + " seconds.");
                }
            }
        }
        else if (hitTag == "Bed")
        {
            // ... logic cho Bed giữ nguyên ...
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (GameManager.Instance.isNight)
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + 20f);
                    GameManager.Instance.ForceSkipNight();
                }
                else
                {
                    Debug.Log("You are not sleepy!");
                }
            }
        }
    }

    void CheckForInteractables()
    {
        if (interactIndicator == null) return;

        // Quét các Collider2D trong phạm vi hình tròn xung quanh Player
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        bool isNear = false;

        foreach (var hit in hits)
        {
            if (interactableTags.Contains(hit.tag))
            {
                isNear = true;
                break;
            }
        }

        interactIndicator.SetActive(isNear);
        
        // Giữ dấu chấm than không bị lật ngược khi Player đổi hướng scale
        if (isNear)
        {
            interactIndicator.transform.localScale = new Vector3(transform.localScale.x > 0 ? 1 : -1, 1, 1);
        }
    }

    // Vẽ vòng tròn phạm vi trong Scene để bạn dễ căn chỉnh (không bắt buộc)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

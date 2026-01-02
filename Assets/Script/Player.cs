using System;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Interaction UI")]
    [Header("Fridge Settings")]
    public float fridgeCooldown = 10f;
    private float lastEatTime = -100f;
    public GameObject interactIndicator;
    public float detectionRange = 1.5f;
    public System.Collections.Generic.List<string> interactableTags = new System.Collections.Generic.List<string>
    {
        "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook",
        "Limit1", "Limit2", "Hide", "Bed", "Murder", "NPC"
    };

    public Vector2Int direction = Vector2Int.right;
    private Vector2Int input;

    public float maxStamina = 100f;
    public float currentStamina;

    public float moveSpeed = 4f;
    public float runSpeed = 10f;
    public float tiredSpeed = 1f;

    public float staminaMoveSpeed = 0.1f;
    public float staminaRunSpeed = 0.15f;

    public bool killed = false;
    public bool exhausted = false;

    private float timer = 0f;
    private bool canRun = true;
    public float runDuration = 5f;
    public float cooldown = 5f;

    public Animator animator;

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

        if (killed || exhausted)
        {
            return;
        }

        CheckStaminaDeath();

        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Catch");
        }

        bool moving = IsMoving();

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        if (DialogueManager.IsMenuOpen)
        {
            inputY = 0;
        }

        if (moving)
        {
            animator.SetFloat("InputX", inputX);
            animator.SetFloat("InputY", inputY);
            animator.SetFloat("LastInputX", inputX);
            animator.SetFloat("LastInputY", inputY);
        }
        else
        {
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

    void CheckStaminaDeath()
    {
        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            exhausted = true;

            GameManager.Instance.gameEnded = true;
        }
    }

    public void Move(float speed)
    {
        Vector2Int currentInput = Vector2Int.zero;

        if (!DialogueManager.IsMenuOpen)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) currentInput = Vector2Int.up;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) currentInput = Vector2Int.down;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) currentInput = Vector2Int.right;
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
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (DialogueManager.IsMenuOpen)
        {
            vertical = 0;
        }

        return horizontal != 0 || vertical != 0;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        string hitTag = collision.tag;

        if (hitTag == "Fridge")
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (Time.time >= lastEatTime + fridgeCooldown)
                {
                    if (currentStamina < maxStamina)
                    {
                        currentStamina = Mathf.Min(maxStamina, currentStamina + 5f);
                        lastEatTime = Time.time;
                    }
                }
            }
        }
        else if (hitTag == "Bed")
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (GameManager.Instance.isNight)
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + 20f);
                    GameManager.Instance.ForceSkipNight();
                }
            }
        }

    }



    void CheckForInteractables()
    {
        if (interactIndicator == null) return;

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

        if (isNear)
        {
            interactIndicator.transform.localScale =
                new Vector3(transform.localScale.x > 0 ? 1 : -1, 1, 1);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

using System;
using System.Collections;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
// using UnityEngine.UIElements;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Interaction UI")]
    [Header("Fridge Settings")]
    public float fridgeCooldown = 10f;
    private float lastEatTime = -100f;
    [Header("Eating UI")]
    public GameObject eatingText;
    public Image eatingProgressCircle;
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

    private bool isInteracting = false;

    public Animator GetAnimator()
    {
        return animator;
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSavedGame", 0) == 0)
        {
            currentStamina = maxStamina;
        }
        animator = GetComponent<Animator>();
        if (interactIndicator != null) interactIndicator.SetActive(false);
        if (eatingText != null) eatingText.SetActive(false);
        if (eatingProgressCircle != null)
        eatingProgressCircle.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckForInteractables();

        if (killed || exhausted || isInteracting)
        {
            return;
        }

        CheckStaminaDeath();

        if (Input.GetKeyDown(KeyCode.F))
        {
            // Check xem có Fridge hoặc Bed trong range không
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);

            bool foundFridge = false;
            bool foundBed = false;

            foreach (var hit in hits)
            {
                if (hit.tag == "Fridge")
                {
                    foundFridge = true;
                    break;
                }
                else if (hit.tag == "Bed")
                {
                    foundBed = true;
                    break;
                }
            }

            if (foundFridge)
            {
                if (Time.time >= lastEatTime + fridgeCooldown)
                {
                    if (currentStamina < maxStamina)
                    {
                        StartCoroutine(EatCoroutine());
                    }
                }
                else
                {
                    PlayerMonologue.Instance.Say("I can not eat anymore!", onceOnly: false, id: "not_eat");
                }
            }
            else if (foundBed)
            {
                StartCoroutine(SleepCoroutine());
            }
            else
            {
                animator.SetTrigger("Catch");
            }
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
            // currentStamina += 0.01f * Time.deltaTime;
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

    private IEnumerator EatCoroutine()
    {
        isInteracting = true;

        PlayerMonologue.Instance.Say("This food is so gooooood! I feel refreshing!", onceOnly: false, id: "eat");

        if (eatingText != null) eatingText.SetActive(true);

        // ✅ HIỆN VÒNG TRÒN LOADING
        if (eatingProgressCircle != null)
        {
            eatingProgressCircle.gameObject.SetActive(true);
            eatingProgressCircle.fillAmount = 1f; // Bắt đầu từ đầy
        }

        float eatDuration = 10f;
        float staminaToRecover = Mathf.Min(10f, maxStamina - currentStamina);
        float elapsed = 0f;
        float startStamina = currentStamina;

        while (elapsed < eatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / eatDuration;

            // ✅ CẬP NHẬT VÒNG TRÒN (GIẢM DẦN TỪ 1 → 0)
            if (eatingProgressCircle != null)
            {
                eatingProgressCircle.fillAmount = 1f - t; // Quay ngược từ 1 → 0
            }

            currentStamina = Mathf.Lerp(startStamina, startStamina + staminaToRecover, t);
            currentStamina = Mathf.Min(maxStamina, currentStamina);

            yield return null;
        }

        currentStamina = Mathf.Min(maxStamina, startStamina + staminaToRecover);

        if (eatingText != null) eatingText.SetActive(false);

        // ✅ ẨN VÒNG TRÒN
        if (eatingProgressCircle != null)
        {
            eatingProgressCircle.gameObject.SetActive(false);
        }

        PlayerMonologue.Instance.Say("Ahh! That was delicious!", onceOnly: false, id: "eat_finish");

        lastEatTime = Time.time;
        isInteracting = false;
    }

    private IEnumerator SleepCoroutine()
    {
        if (GameManager.Instance.isNight)
        {
            isInteracting = true;


            Time.timeScale = 0f;
            GameManager.Instance.audioSource.Stop();

            ScreenFader.Instance.FadeOut();
            yield return new WaitForSecondsRealtime(3f);

            currentStamina = Mathf.Min(maxStamina, currentStamina + 40f);
            GameManager.Instance.ForceSkipNight();

            ScreenFader.Instance.FadeIn();
            // yield return new WaitForSecondsRealtime(3f);

            isInteracting = false;
            Time.timeScale = 1f;
            GameManager.Instance.audioSource.Play();

            PlayerMonologue.Instance.Say("What a good sleep!", onceOnly: false, id: "sleep");


        }
        else
        {
            PlayerMonologue.Instance.Say("I am not sleepy, better go investigate!", onceOnly: false, id: "not_sleep");
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



    void CheckForInteractables()
    {
        if (interactIndicator == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        bool isNear = false;

        foreach (var hit in hits)
        {
            // Kiểm tra tag có trong danh sách không
            if (interactableTags.Contains(hit.tag))
            {
                // Bỏ qua nếu sprite renderer bị ẩn
                SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
                if (sr != null && !sr.enabled)
                    continue;

                // Với các tag đặc biệt (Bed, Fridge, NPC, Hide, Murder) → luôn hiện UI
                if (hit.tag == "Bed" || hit.tag == "Fridge" || hit.tag == "NPC" ||
                    hit.tag == "Hide" || hit.tag == "Murder")
                {
                    isNear = true;
                    break;
                }

                // Với evidence tags → kiểm tra xem đã nhặt chưa
                if (EvidenceManager.Instance != null)
                {
                    // Nếu đã nhặt rồi thì bỏ qua
                    if (EvidenceManager.Instance.HasEvidence(hit.tag))
                        continue;

                    if (EvidenceManager.Instance.permanentlyRemovedEvidence.Contains(hit.tag))
                        continue;
                }

                // Nếu chưa nhặt hoặc không thuộc evidence → hiện UI
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
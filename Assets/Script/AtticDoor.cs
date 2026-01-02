using UnityEngine;

public class AtticDoor : MonoBehaviour
{
    public bool useImportantInfo = true;
    public bool useEvidenceTag = false;
    
    [Header("Important Info Settings")]
    public string targetNPC = "Mai";
    public int requiredKey = 2;
    
    [Header("Evidence Tag Settings")]
    public string evidenceTag = "AtticKey";
    
    [Header("Interaction Settings")]
    public float interactionDistance = 2f;
    public string lockedMessage = "Cửa bị khóa!";
    
    private BoxCollider2D doorCollider;
    private Transform playerTransform;
    private bool isUnlocked = false;
    private bool hasShownMessage = false; // Tránh spam message

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }
    }

    void Update()
    {
        // Check unlock condition
        if (!isUnlocked && DialogueManager.Instance != null)
        {
            if (CheckUnlockCondition())
            {
                UnlockDoor();
            }
        }

        // Check dialogue interaction
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distance <= interactionDistance)
            {
                if (!hasShownMessage) // Chỉ hiện 1 lần khi vào vùng
                {
                    if (isUnlocked)
                    {
                        Debug.Log("Cửa đã mở!");
                    }
                    else
                    {
                        // Lấy tag của GameObject này
                        if (gameObject.CompareTag("AtticDoor"))
                        {
                            PlayerMonologue.Instance.Say("This door is locked... I should go ask for a key.", onceOnly: false, id: "attic_door");
                        }
                        else if (gameObject.CompareTag("BeginDoor"))
                        {
                            PlayerMonologue.Instance.Say("I should go investigating the scene first!", onceOnly: false, id: "begin_door");
                        }
                    }
                    hasShownMessage = true;
                }
            }
            else
            {
                // Reset khi player ra khỏi vùng
                hasShownMessage = false;
            }
        }
    }

    bool CheckUnlockCondition()
    {
        bool hasImportantInfo = false;
        bool hasEvidence = false;

        if (useImportantInfo)
        {
            hasImportantInfo = DialogueManager.Instance.HasImportantInfo(targetNPC, requiredKey);
        }

        if (useEvidenceTag)
        {
            hasEvidence = EvidenceManager.Instance.HasEvidence(evidenceTag);
        }

        if (useImportantInfo && useEvidenceTag)
        {
            return hasImportantInfo && hasEvidence;
        }
        else if (useImportantInfo)
        {
            return hasImportantInfo;
        }
        else if (useEvidenceTag)
        {
            return hasEvidence;
        }

        return false;
    }

    void UnlockDoor()
    {
        isUnlocked = true;
        
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
            Debug.Log("Door Unlocked!");
        }
    }
}
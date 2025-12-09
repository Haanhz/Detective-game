using UnityEngine;

public class Evidence_old : MonoBehaviour
{
    public string evidenceTag;   // Tag hoặc type của evidence
    public float weight = 0f;    // Weight được tính dựa trên tag
    public int spawnNight; 
    public KeyCode pickupKey = KeyCode.F;
    private bool collected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (string.IsNullOrEmpty(evidenceTag))
            evidenceTag = gameObject.tag;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.tag == "Player")
    //     {
    //         evidence.SetActive(false);
    //         ScoreBoard.scoreValue += 1;
    //         Debug.Log("Get 1 evidence, Score: " + ScoreBoard.scoreValue);
    //     }
    // }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Input.GetKey(pickupKey) && !collected)
        {
            collected = true;
            weight = CalculateWeight(evidenceTag);
            EvidenceManager.Instance.AddEvidence(evidenceTag, weight);
            if (ShouldHide(evidenceTag))
                gameObject.SetActive(false);
            
            //ScoreBoard.scoreValue += 1;
            //Debug.Log("Get 1 evidence, Score: " + ScoreBoard.scoreValue);
        }
    }
    float CalculateWeight(string tagName)
    {
        switch (tagName)
        {
            case "LivingCorner":
                return 1.0f;
            case "Ultimatum":
                return 2.0f;
            case "HangPhone":
                return 3.0f;
            case "HangNoteBook":
                return 3.0f;
            case "Limit1":
                return 5.0f;
            case "Limit2":
                return 5.0f;
            case "Hide":
                return 0.0f;
            default:
                return 0f; // default weight
        }
    }

    bool ShouldHide(string tagName)
    {
        switch (tagName)
        {
            case "HangNoteBook":
            case "Limit1":
            case "Limit2":
            case "Hide":
                return true;

            default:
                return false;   // các tag còn lại KHÔNG ẩn
        }
    }


}

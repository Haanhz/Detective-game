using UnityEngine;

public class Evidence : MonoBehaviour
{
    public GameObject evidence;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player")
        {
            evidence.SetActive(false);
            ScoreBoard.scoreValue += 10;
            Debug.Log("Get 1 evidence, Score: " + ScoreBoard.scoreValue);
        }
    }
}

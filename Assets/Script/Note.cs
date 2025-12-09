using UnityEngine;

public class Note : MonoBehaviour
{
    [TextArea]
    public string content;
    public float interactRange = 2f;

    private Transform player;
    private bool isNoteOpen = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
{
    float dist = Vector2.Distance(player.position, transform.position);

    if (dist <= interactRange && Input.GetKeyDown(KeyCode.F))
    {
        if (!isNoteOpen)
        {
            UIManager.Instance.OpenNote(content);
            isNoteOpen = true;
        }
        else
        {
            UIManager.Instance.CloseNote();
            isNoteOpen = false;
        }
    }
}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}

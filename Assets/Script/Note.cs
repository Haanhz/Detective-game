using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    [TextArea]
    public string content;
    public float interactRange = 2f;

    private Transform player;
    private bool isNoteOpen = false;
    public Button closeBtn;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Đăng ký sự kiện click cho close button
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseNoteButton);
        }
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
        
        // Có thể thêm phím ESC để đóng note
        if (isNoteOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.CloseNote();
            isNoteOpen = false;
        }
    }

    // Hàm xử lý khi click close button
    void CloseNoteButton()
    {
        if (isNoteOpen)
        {
            UIManager.Instance.CloseNote();
            isNoteOpen = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
    
    // Cleanup khi destroy object
    void OnDestroy()
    {
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveListener(CloseNoteButton);
        }
    }
}
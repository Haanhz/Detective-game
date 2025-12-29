using UnityEngine;

// Script này gắn vào TỪNG GameObject Limit (limit1, limit2, ...)
public class LimitController : MonoBehaviour
{
    private bool isLitByUV = false;
    private SpriteRenderer spriteRenderer;
    
    // Biến public để Evidence script check
    public bool IsVisible { get; private set; }
    
    void Start()
    {
        // Lấy SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ban đầu luôn ẩn visual
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        
        IsVisible = false;
    }

    void Update()
    {
        // Chỉ kiểm tra khi ban đêm
        if (GameManager.Instance == null || !GameManager.Instance.isNight)
        {
            // Ban ngày: ẩn hết
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            IsVisible = false;
            return;
        }

        // Ban đêm: hiện/ẩn dựa trên UV
        bool shouldShow = isLitByUV;
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = shouldShow;
        
        IsVisible = shouldShow;
    }

    void FixedUpdate()
    {
        // Reset mỗi physics frame
        isLitByUV = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("UVLight"))
        {
            isLitByUV = true;
        }
    }
}
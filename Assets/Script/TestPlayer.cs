using UnityEngine;
using TMPro;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5f;
    
    public TextMeshProUGUI textFollow;   // UI Text
    public Camera cam;                   // Camera chính
    public Vector3 offset = new Vector3(0, 50, 0); // offset tính bằng pixel

    void Start()
    {
        if (cam == null)
            cam = Camera.main;  // tự lấy camera
    }

    void Update()
    {
        // Điều khiển
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, v, 0f);
        transform.Translate(move * speed * Time.deltaTime);

        // UI Text follow
        if (textFollow != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            textFollow.transform.position = screenPos + offset;
        }
    }
}

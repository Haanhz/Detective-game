using UnityEngine;
using TMPro;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5f;
       
    void Start()
    {
        
    }

    void Update()
    {
        // Điều khiển
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, v, 0f);
        transform.Translate(move * speed * Time.deltaTime);

      
    }
}

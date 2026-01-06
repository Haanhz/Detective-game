using UnityEngine;

public class UVLightController : MonoBehaviour
{
    public Player player;        // kéo Player script vào
    public GameObject lightObject;

    public float offset = 0.5f;  // khoảng cách đèn trước mặt

    public bool isOn = false;

    void Start()
    {
        if (lightObject == null)
            lightObject = gameObject;

        lightObject.SetActive(false);
    }

    void Update()
    {
        HandleToggle();

        if (isOn)
        {
            FollowPlayerDirection();
        }
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!GameManager.Instance.isNight)
            {
                Debug.Log("You can not turn on UV light in the morning!");
                PlayerMonologue.Instance.Say("Can not turn on UV Light in the morning!", onceOnly: false, id: "not_light");
                return;
            }

            isOn = !isOn;
            lightObject.SetActive(isOn);
        }
        if (!GameManager.Instance.isNight)
        {
            lightObject.SetActive(false);
        }
    }

    void FollowPlayerDirection()
    {
        Vector2Int dir = player.direction;

        // ROTATION – quay đúng hướng
        float angle = 0f;

        if (dir == Vector2Int.right) angle = -90f;
        else if (dir == Vector2Int.up) angle = 0f;
        else if (dir == Vector2Int.down) angle = 180f;
        else if (dir == Vector2Int.left) angle = 90f;

        lightObject.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}

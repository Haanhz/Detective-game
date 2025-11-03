using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager cam = Camera.main.GetComponent<CameraManager>();
            cam.SetCurrentRoom(GetComponent<BoxCollider2D>());
            Debug.Log("Player entered room: " + gameObject.name);

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager cam = Camera.main.GetComponent<CameraManager>();
            cam.ClearCurrentRoom(GetComponent<BoxCollider2D>());
        }
    }
}

using Unity.Cinemachine;
using UnityEngine;
using System.Collections;


public class MapTransition : MonoBehaviour
{
    public PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;
    public Direction direction;
    public Transform teleportTargetPos;
    public int additivePos = 4;
    public enum Direction {Up, Down, Left, Right, Teleport}
    public CinemachineCamera vcam;

    [Header("Area Settings")]
    public string areaName;

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // FadeTransition(collision.gameObject);
            StartCoroutine(FadeTransition(collision.gameObject));
            if (!string.IsNullOrEmpty(areaName) && AreaManager.Instance != null)
            {
                AreaManager.Instance.ShowAreaName(areaName);
            }
            // confiner.BoundingShape2D = mapBoundary;
            // UpdatePlayerPosition(collision.gameObject);
        }
    }

    IEnumerator FadeTransition(GameObject player)
{
    ScreenFader.Instance.FadeOut();

    // chờ cho fade xong
    yield return new WaitForSeconds(ScreenFader.Instance.fadeDuration);

    confiner.BoundingShape2D = mapBoundary;
    UpdatePlayerPosition(player);
    vcam.PreviousStateIsValid = false;
    vcam.UpdateCameraState(Vector3.zero, Time.deltaTime); 

    ScreenFader.Instance.FadeIn();

    // chờ fade in kết thúc (nếu cần)
    yield return new WaitForSeconds(ScreenFader.Instance.fadeDuration);
}
    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPos.position;
            return;
        }
        Vector3 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up:
                newPos.y +=additivePos;
                break;
            case Direction.Down:
                newPos.y -=additivePos;
                break;
            case Direction.Left:
                newPos.x -=additivePos;
                break;
            case Direction.Right:
                newPos.x +=additivePos;
                break;
            
        }
        player.transform.position = newPos;
    }

}

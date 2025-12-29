using UnityEngine;
using TMPro;
using System.Collections;

public class AreaManager : MonoBehaviour
{
    public static AreaManager Instance;
    public TextMeshProUGUI areaText;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    private Coroutine currentCoroutine;

    void Awake() => Instance = this;

    public void ShowAreaName(string name)
    {
        // Mỗi khi sang khu vực mới, bắt đầu lại hiệu ứng Fade In cho tên mới
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeInOnly(name));
    }

    IEnumerator FadeInOnly(string name)
    {
        areaText.text = name;
        
        // Bắt đầu Fade In từ độ mờ hiện tại hoặc từ 0
        float elapsed = 0;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            // Tăng dần Alpha lên 1
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1; // Đảm bảo luôn hiện hoàn toàn
        // KHÔNG có đoạn yield return WaitForSeconds hay Fade Out ở đây nữa
    }
}
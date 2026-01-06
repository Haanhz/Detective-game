using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;
    public CinemachineCamera vcam;

    CinemachinePositionComposer composer;
    Vector3 originalDamping;

    Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        composer = vcam.GetComponent<CinemachinePositionComposer>();

        if (composer != null)
            originalDamping = composer.Damping;
    }

    IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            // Dùng Time.unscaledDeltaTime thay vì Time.deltaTime
            // để vẫn hoạt động khi Time.timeScale = 0
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    public void FadeOut()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(1, Vector3.zero));
    }

    public void FadeIn()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(0, originalDamping));
    }

    IEnumerator FadeRoutine(float alpha, Vector3 damping)
    {
        SetDamping(damping);
        yield return Fade(alpha);
    }

    void SetDamping(Vector3 d)
    {
        if (composer == null) return;

        composer.Damping = d;
    }
}
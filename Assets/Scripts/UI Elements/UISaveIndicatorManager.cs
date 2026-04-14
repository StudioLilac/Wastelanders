using System.Collections;
using TMPro;
using UnityEngine;

public class UISaveIndicatorManager : PersistentSingleton<UISaveIndicatorManager>
{
    [SerializeField] private Canvas saveIndicatorCanvas;
    [SerializeField] private TMP_Text saveText;

    private const float FadeInDuration = 0.3f;
    private const float DisplayDuration = 1.5f;
    private const float FadeOutDuration = 0.5f;

    private Coroutine activeCoroutine;

    protected override void Awake()
    {
        base.Awake();
        SetTextAlpha(0f);
        saveIndicatorCanvas.enabled = false;
        saveIndicatorCanvas.sortingOrder = UISortOrder.SaveIndicator.GetOrder();
    }

    public void Show()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        saveIndicatorCanvas.enabled = true;

        yield return Fade(0f, 1f, FadeInDuration);
        yield return new WaitForSecondsRealtime(DisplayDuration);
        yield return Fade(1f, 0f, FadeOutDuration);

        saveIndicatorCanvas.enabled = false;
        activeCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        SetTextAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetTextAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetTextAlpha(to);
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = saveText.color;
        color.a = alpha;
        saveText.color = color;
    }
}

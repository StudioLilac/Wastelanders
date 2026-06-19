using TMPro;
using System.Collections;
using UnityEngine;

public class PopUpNotification : MonoBehaviour
{
    public float fadeInDurationS = 0.2f;
    public float fadeOutDurationS = 0.5f;
    public float floatDurationS = 1.0f;
    public float floatDistance = 50f;

    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void Initialize(Vector2 anchoredPos, string text)
    {
        rectTransform.anchoredPosition = anchoredPos;
        textMesh.text = text;
        StartCoroutine(AnimateCoroutine());
    }

    public Vector2 Measure(string text) => textMesh.GetPreferredValues(text, rectTransform.rect.width, 0f);

    private IEnumerator AnimateCoroutine()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeInDurationS));

        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = start + Vector2.up * floatDistance;
        float t = 0f;
        while (t < floatDurationS)
        {
            t += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, t / floatDurationS);
            yield return null;
        }

        yield return StartCoroutine(Fade(1f, 0f, fadeOutDurationS));
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}

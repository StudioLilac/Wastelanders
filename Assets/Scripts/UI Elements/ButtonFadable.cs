using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonFadable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private CanvasGroup _canvasGroup;

    // Configurable opacity levels
    public float normalAlpha = 1f;
    public float hoverAlpha = 0.8f;
    public float pressAlpha = 0.6f;
    public float fadeDuration = 0.1f;

    [CanBeNull] public event Action OnClick;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(hoverAlpha));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(normalAlpha));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(pressAlpha));
        OnClick?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(hoverAlpha));
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = _canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = targetAlpha;
    }
}
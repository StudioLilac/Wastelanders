using System.Collections;
using UnityEngine;

public abstract class FadeHandlerBase : MonoBehaviour
{
    private Coroutine _animationCoroutine = null;
    protected abstract float CurrentAlpha { get; }
    protected abstract void SetAlpha(float alpha);

    /// <summary>
    /// Evaluates the animation curve. Override this in subclasses for non-linear fading.
    /// </summary>
    protected virtual float EvaluateCurve(float fraction) => fraction;

    /// <summary>
    /// Immediately sets the screen to be fully opaque (black).
    /// </summary>
    public void SetDarkScreen()
    {
        StopCurrentFadeAnimation();
        SetAlpha(1f);
    }

    /// <summary>
    /// Immediately sets the screen to be fully transparent.
    /// </summary>
    public void SetLightScreen()
    {
        StopCurrentFadeAnimation();
        SetAlpha(0f);
    }

    /// <summary>
    /// Starts a fade to a specific alpha value over a duration.
    /// </summary>
    /// <returns>An IEnumerator that completes when the fade is finished.</returns>
    public IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        RequestFade(targetAlpha, duration);
        // This coroutine waits until the animation coroutine has set itself to null
        yield return new WaitUntil(() => _animationCoroutine == null);
    }

    /// <summary>
    /// Starts a fade to fully transparent over a duration.
    /// </summary>
    /// <returns>An IEnumerator that completes when the fade is finished.</returns>
    public IEnumerator FadeInLightScreen(float duration)
    {
        yield return FadeToAlpha(0f, duration);
    }

    /// <summary>
    /// Starts a fade to fully opaque over a duration.
    /// </summary>
    /// <returns>An IEnumerator that completes when the fade is finished.</returns>
    public IEnumerator FadeInDarkScreen(float duration)
    {
        yield return FadeToAlpha(1f, duration);
    }

    /// <summary>
    /// Internal method to handle starting the fade animation coroutine.
    /// </summary>
    private void RequestFade(float targetAlpha, float duration)
    {
        float clampedTargetAlpha = Mathf.Clamp01(targetAlpha);
        float effectiveDuration = Mathf.Max(duration, 0f);

        if (!isActiveAndEnabled)
        {
            SetAlpha(clampedTargetAlpha);
            return;
        }

        StopCurrentFadeAnimation();
        _animationCoroutine = StartCoroutine(AnimateFade(CurrentAlpha, clampedTargetAlpha, effectiveDuration));
    }

    /// <summary>
    /// The coroutine that animates the alpha value over time.
    /// </summary>
    private IEnumerator AnimateFade(float startAlpha, float endAlpha, float duration)
    {
        // If duration is zero, just set the final alpha and finish
        if (duration <= 0f)
        {
            SetAlpha(endAlpha);
            _animationCoroutine = null;
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float fraction = elapsedTime / duration;

            float easedFraction = EvaluateCurve(fraction);
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, easedFraction);
            
            SetAlpha(newAlpha);
            yield return null;
        }

        // Ensure the final alpha value is set precisely
        SetAlpha(endAlpha);
        _animationCoroutine = null;
    }

    /// <summary>
    /// Stops any currently running fade animation.
    /// </summary>
    private void StopCurrentFadeAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }
}
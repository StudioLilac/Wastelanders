using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "UI/Smoke Screen Overlay" shader.
/// Put this on a full-screen Image that uses a material with that shader.
///
///   smoke.Deploy(canisterScreenPoint);   // canister lands, smoke rolls in
///   smoke.Dissipate();                   // smoke clears
///
/// Both return a Coroutine, so from your VN sequencer you can do:
///   yield return smoke.Deploy(point);
/// </summary>
[RequireComponent(typeof(Image))]
public class SmokeScreenOverlay : MonoBehaviour
{
    static readonly int AmountID = Shader.PropertyToID("_Amount");
    static readonly int OriginID = Shader.PropertyToID("_Origin");
    static readonly int OpacityID = Shader.PropertyToID("_Opacity");

    [Header("Timing")]
    [Tooltip("Seconds for the smoke to fill the screen.")]
    public float deployDuration = 2.5f;

    [Tooltip("Seconds for the smoke to clear.")]
    public float dissipateDuration = 4f;

    [Tooltip("Eases the roll-in. Fast at first, then creeping, reads best.")]
    public AnimationCurve deployCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 2f, 2f),
        new Keyframe(1f, 1f, 0f, 0f));

    public AnimationCurve dissipateCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("Use unscaled time so the effect still plays if the VN pauses Time.timeScale.")]
    public bool useUnscaledTime = true;

    [Header("State")]
    [Range(0f, 1f)] public float amount;

    Image _image;
    Material _material;
    Coroutine _running;

    void Awake()
    {
        _image = GetComponent<Image>();

        // Instance the material so tweaking _Amount doesn't dirty the shared asset.
        if (_image.material != null)
        {
            _material = new Material(_image.material);
            _image.material = _material;
        }

        SetAmount(amount);
        _image.raycastTarget = false;   // smoke shouldn't eat clicks on the VN UI
    }

    void OnDestroy()
    {
        if (_material != null) Destroy(_material);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && _material != null) SetAmount(amount);
    }
#endif

    /// <summary>Where the smoke blooms from, in viewport space (0-1, origin bottom-left).</summary>
    public void SetOrigin(Vector2 viewportPoint)
    {
        if (_material != null)
            _material.SetVector(OriginID, new Vector4(viewportPoint.x, viewportPoint.y, 0f, 0f));
    }

    /// <summary>Same, but takes a screen-space pixel position (e.g. from a canister RectTransform).</summary>
    public void SetOriginFromScreenPoint(Vector2 screenPoint)
    {
        SetOrigin(new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height));
    }

    public void SetOpacity(float opacity)
    {
        opacity = Mathf.Clamp01(opacity);
        if (_material != null) _material.SetFloat(OpacityID, Mathf.Clamp01(opacity));
    }

    public void SetAmount(float value)
    {
        amount = Mathf.Clamp01(value);
        if (_material != null) _material.SetFloat(AmountID, amount);
    }

    public Coroutine Deploy(Vector2? viewportOrigin = null, float? duration = null)
    {
        if (viewportOrigin.HasValue) SetOrigin(viewportOrigin.Value);
        return Run(Animate(amount, 1f, duration ?? deployDuration, deployCurve));
    }

    public Coroutine Dissipate(float? duration = null)
    {
        return Run(Animate(amount, 0f, duration ?? dissipateDuration, dissipateCurve));
    }

    public Coroutine FadeOut(float? duration = null)
    {
        return Run(Animate(amount, 0f, duration ?? dissipateDuration, dissipateCurve, SetOpacity));
    }

    /// <summary>Roll in, hold, then clear. Yield on this from a story sequence.</summary>
    public Coroutine DeployAndClear(Vector2 viewportOrigin, float holdSeconds)
    {
        SetOrigin(viewportOrigin);
        return Run(DeployHoldClear(holdSeconds));
    }

    Coroutine Run(IEnumerator routine)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(routine);
        return _running;
    }

    IEnumerator DeployHoldClear(float holdSeconds)
    {
        yield return Animate(amount, 1f, deployDuration, deployCurve);
        yield return Wait(holdSeconds);
        yield return Animate(1f, 0f, dissipateDuration, dissipateCurve);
    }

    IEnumerator Animate(float from, float to, float duration, AnimationCurve curve, Action<float> onProgress = null)
    {
        onProgress ??= SetAmount;
        if (duration <= 0f) { onProgress(to); yield break; }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            float eased = curve != null && curve.length > 0 ? curve.Evaluate(k) : k;
            onProgress(Mathf.LerpUnclamped(from, to, Mathf.InverseLerp(
                curve.Evaluate(0f), curve.Evaluate(1f), eased)));
            yield return null;
        }
        onProgress(to);
    }

    IEnumerator Wait(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }
}
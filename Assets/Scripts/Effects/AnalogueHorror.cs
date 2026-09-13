using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the "Hidden/FX/Analogue Horror" post process.
///
///   horror.RampTo(0.4f, 6f);        // creep in over 6 seconds
///   horror.Burst(1f, 0.25f);        // hard glitch stab.
///   yield return horror.RampTo(0f, 3f);   // back to clean
///
/// Set pulseAmount above 0 for a slow breathing wobble on top of the
/// base level, which reads as much more unsettling than a static amount.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Analogue Horror")]
public class AnalogueHorrorEffect : MonoBehaviour
{
    static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    static readonly int SnowID = Shader.PropertyToID("_Snow");

    [Header("Level")]
    [Range(0f, 1f)] public float intensity = 0f;

    [Tooltip("Extra signal-loss snow layered on top. Keep at 0 until you want a total blackout moment.")]
    [Range(0f, 1f)] public float snow = 0f;

    [Header("Pulse")]
    [Tooltip("How much the intensity wanders above and below the base level.")]
    [Range(0f, 0.5f)] public float pulseAmount = 0.08f;
    public float pulseSpeed = 0.6f;

    [Header("Setup")]
    public Shader shader;
    public bool useUnscaledTime = true;

    Material _material;
    Coroutine _running;

    Material Material
    {
        get
        {
            if (_material == null)
            {
                if (shader == null) shader = Shader.Find("Hidden/FX/Analogue Horror");
                if (shader == null || !shader.isSupported) return null;
                _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            return _material;
        }
    }

    void OnDisable()
    {
        if (_material != null)
        {
            if (Application.isPlaying) Destroy(_material);
            else DestroyImmediate(_material);
            _material = null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        float level = CurrentLevel();
        Material mat = Material;

        if (mat == null || level <= 0.001f)
        {
            Graphics.Blit(src, dst);
            return;
        }

        mat.SetFloat(IntensityID, level);
        mat.SetFloat(SnowID, snow);
        Graphics.Blit(src, dst, mat);
    }

    float CurrentLevel()
    {
        if (intensity <= 0.001f) return intensity;
        if (pulseAmount <= 0f) return Mathf.Clamp01(intensity);

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        // Two detuned sines so the wobble never settles into an obvious loop.
        float wobble = Mathf.Sin(t * pulseSpeed) * 0.6f + Mathf.Sin(t * pulseSpeed * 1.73f) * 0.4f;
        return Mathf.Clamp01(intensity + wobble * pulseAmount);
    }

    // ------------------------------------------------------------------
    //  Scripting API
    // ------------------------------------------------------------------

    public void SetIntensity(float value) => intensity = Mathf.Clamp01(value);

    /// <summary>Fade the base level to a target over time. Yield on it to wait.</summary>
    public Coroutine RampTo(float target, float duration)
    {
        return Run(RampRoutine(intensity, Mathf.Clamp01(target), duration));
    }

    /// <summary>A hard spike that snaps in and falls off. Good on a single beat of dialogue.</summary>
    public Coroutine Burst(float strength = 1f, float duration = 0.25f)
    {
        return Run(BurstRoutine(Mathf.Clamp01(strength), duration, intensity));
    }

    /// <summary>Total signal loss, then back. Use sparingly, it is the loudest card in the deck.</summary>
    public Coroutine SignalLoss(float holdSeconds = 0.6f)
    {
        return Run(SignalLossRoutine(holdSeconds));
    }

    Coroutine Run(IEnumerator routine)
    {
        if (!Application.isPlaying) return null;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(routine);
        return _running;
    }

    IEnumerator RampRoutine(float from, float to, float duration)
    {
        if (duration <= 0f) { intensity = to; yield break; }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Delta();
            intensity = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        intensity = to;
    }

    IEnumerator BurstRoutine(float strength, float duration, float returnTo)
    {
        intensity = strength;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Delta();
            float k = Mathf.Clamp01(elapsed / duration);
            // Fall off fast, so it snaps rather than fades.
            intensity = Mathf.Lerp(strength, returnTo, k * k);
            yield return null;
        }
        intensity = returnTo;
    }

    IEnumerator SignalLossRoutine(float holdSeconds)
    {
        float baseline = intensity;
        intensity = 1f;
        snow = 1f;

        float elapsed = 0f;
        while (elapsed < holdSeconds)
        {
            elapsed += Delta();
            yield return null;
        }

        // Snap back like the signal reacquired.
        snow = 0f;
        intensity = baseline;
    }

    float Delta() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
}
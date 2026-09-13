using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Drives the whole climax beat. Everything here runs on UNSCALED time so the
/// sequence keeps its own clock while Time.timeScale is crushed for the freeze.
///
/// Gameplay objects (crown rigidbody, cone particles) stay on SCALED time, which
/// is what makes them creep in slow motion during the hold for free.
/// </summary>
[AddComponentMenu("Impact/Impact Sequencer")]
public class ImpactSequencer : MonoBehaviour
{
    [Header("References")]
    public ImpactFrameController impact;
    [Tooltip("HUD canvas group. Faded out for the duration -- a Screen Space - " +
             "Overlay canvas renders after everything and cannot be shaded over.")]
    public CanvasGroup hud;
    public float preFreeze = 0.05f;

    [Header("Freeze")]
    [Tooltip("0 is a true freeze. 0.02-0.05 lets the crown drift back a couple " +
             "of units during the hold, which reads much better.")]
    [Range(0f, 0.2f)] public float freezeTimeScale = 0.03f;
    public float freezeHold = 0.55f;

    [Header("Flicker")]
    [Tooltip("Number of inverted frames at the start of the hold. 2-4 is the " +
             "classic look; 0 disables.")]
    public int flickerCount = 3;
    public float flickerStep = 0.045f;

    [Header("Settle")]
    public float frameFadeOut = 0.12f;
    public float timeRampBack = 0.22f;
    [Tooltip("Realtime beat between the frame clearing and the burst firing.")]
    public float beatBeforeBurst = 0.04f;

    [Header("Hooks")]
    public UnityEvent onImpact;   // sfx, controller rumble, hit stop
    public UnityEvent onSettle;   // frame has cleared, time ramping back
    public UnityEvent onBurst;    // screen shake + enemy dissolve go here

    const float kDefaultFixedDelta = 0.02f;

    Coroutine _running;

    public void Play()
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        // ---------------------------------------------------------- contact
        if (hud != null) StartCoroutine(FadeCanvas(hud, 0f, preFreeze));

        onImpact?.Invoke();

        yield return new WaitForSecondsRealtime(preFreeze);

        // ----------------------------------------------------------- freeze
        SetTimeScale(freezeTimeScale);

        impact.SetIntensity(1f);
        impact.invert = 0f;

        for (int i = 0; i < flickerCount; i++)
        {
            impact.invert = (i % 2 == 0) ? 1f : 0f;
            yield return new WaitForSecondsRealtime(flickerStep);
        }
        impact.invert = 0f;


        float hold = Mathf.Max(0f, freezeHold - flickerCount * flickerStep);
        yield return new WaitForSecondsRealtime(hold);

        // ----------------------------------------------------------- settle

        float t = 0f;
        while (t < frameFadeOut)
        {
            t += Time.unscaledDeltaTime;
            impact.SetIntensity(1f - (t / frameFadeOut));
            yield return null;
        }
        impact.SetIntensity(0f);

        onSettle?.Invoke();

        t = 0f;
        while (t < timeRampBack)
        {
            t += Time.unscaledDeltaTime;
            // ease out -- snapping straight back to 1 feels mechanical
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / timeRampBack), 3f);
            SetTimeScale(Mathf.Lerp(freezeTimeScale, 1f, k));
            yield return null;
        }
        SetTimeScale(1f);

        yield return new WaitForSecondsRealtime(beatBeforeBurst);

        // ------------------------------------------------------------ burst
        onBurst?.Invoke();

        if (hud != null) StartCoroutine(FadeCanvas(hud, 1f, 0.3f));

        _running = null;
    }

    static void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        // keeps physics stepping smoothly in slow motion instead of stuttering
        Time.fixedDeltaTime = kDefaultFixedDelta * Mathf.Max(scale, 0.0001f);
    }

    static IEnumerator FadeCanvas(CanvasGroup g, float target, float duration)
    {
        float start = g.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        g.alpha = target;
        g.blocksRaycasts = target > 0.5f;
    }

    void OnDisable()
    {
        // never strand the game in slow motion
        if (_running != null)
        {
            SetTimeScale(1f);
            if (impact != null) impact.SetIntensity(0f);
        }
    }
}

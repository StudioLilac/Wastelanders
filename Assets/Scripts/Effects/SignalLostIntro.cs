using System.Collections;
using UnityEngine;

/// <summary>
/// The intro beat of a game over screen: everything that happens between
/// the loss firing and the dialogue starting.
///
/// GameOver keeps owning the buttons, sort orders and restart flow. Only
/// the presentation of the loss varies, so that is the part we inject.
/// </summary>
public interface IGameOverIntro
{
    IEnumerator Play();

    /// <summary>Undo anything global. Called before restart or scene load.</summary>
    void Release();
}

/// <summary>
/// "Signal Lost" — the alternate loss when Ives decoheres.
///
/// Unlike the standard loss we do NOT fade to black. The battlefield stays
/// visible behind a partial scrim, still glitching, for the whole screen.
/// The Signal Lost UI lives on a Screen Space - Overlay canvas so the post
/// process cannot touch it: corrupted world, pristine interface.
///
/// Wire this into the Ives death listener:
///
///   GameOver.Instance.FadeInWithDialogue(dialogue, signalLostIntro);
/// </summary>
[AddComponentMenu("UI/Signal Lost Intro")]
public class SignalLostIntro : MonoBehaviour, IGameOverIntro
{
    [Header("References")]
    [SerializeField] private AnalogueHorrorEffect horror;

    [Tooltip("Own scrim: a full-screen black Image with a CanvasGroup, on the " +
             "same camera-space canvas as the battlefield. Starts at alpha 0.")]
    [SerializeField] private CanvasGroup partialScrim;

    [Tooltip("Carrier hum / tape hiss that replaces the music. Looping.")]
    [SerializeField] private AudioSource carrierHum;

    [Header("Beats")]
    [SerializeField] private float stabDuration = 0.22f;
    [SerializeField] private float rampDuration = 1.6f;
    [SerializeField] private float snowHold = 0.45f;
    [SerializeField] private float settleDuration = 0.8f;

    [Header("Levels")]
    [Tooltip("Sustained corruption while the player sits on the game over screen. " +
             "Keep this low — it has to be tolerable for a long time.")]
    [Range(0f, 1f)][SerializeField] private float sustainedIntensity = 0.32f;

    [Tooltip("How much of the battlefield stays readable. 1 = full black.")]
    [Range(0f, 1f)][SerializeField] private float scrimAlpha = 0.62f;

    [SerializeField] private float humFadeIn = 1.2f;

    [Header("Idle")]
    [Tooltip("Occasional glitch stabs while the player reads. 0 to disable.")]
    [SerializeField] private float idleStabInterval = 6f;
    [Range(0f, 1f)][SerializeField] private float idleStabStrength = 0.55f;

    Coroutine _idle;

    void Awake()
    {
        if (partialScrim != null) partialScrim.alpha = 0f;
        if (carrierHum != null) carrierHum.volume = 0f;
    }

    public IEnumerator Play()
    {
        StopIdle();

        // --- Beat 1: the stab. Signal breaks, music cuts. ---------------
        AudioManager.Instance.StopMusic();          // NOTE: adjust to your API
        if (horror != null) horror.Burst(1f, stabDuration);
        if (carrierHum != null) { carrierHum.volume = 0f; carrierHum.Play(); }
        yield return WaitUnscaled(stabDuration);

        // --- Beat 2: degradation creeps in, battlefield still visible ---
        float elapsed = 0f;
        while (elapsed < rampDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / rampDuration);

            if (horror != null) horror.SetIntensity(Mathf.Lerp(0f, 0.75f, k));
            if (partialScrim != null) partialScrim.alpha = Mathf.Lerp(0f, scrimAlpha, k);
            if (carrierHum != null)
                carrierHum.volume = Mathf.Clamp01(elapsed / Mathf.Max(humFadeIn, 0.01f));

            yield return null;
        }

        // --- Beat 3: total signal loss ----------------------------------
        if (horror != null) yield return horror.SignalLoss(snowHold);

        // --- Beat 4: settle to a sustained, liveable level ---------------
        // The clean UI arrives out of the snow on the far side of this.
        float from = horror != null ? horror.intensity : 0f;
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / settleDuration);
            if (horror != null) horror.SetIntensity(Mathf.Lerp(from, sustainedIntensity, k));
            yield return null;
        }

        if (horror != null)
        {
            horror.SetIntensity(sustainedIntensity);
            horror.pulseAmount = 0.06f;     // slow breathing so the eye never settles
            horror.snow = 0f;
        }

        StartIdle();
    }

    public void Release()
    {
        StopIdle();
        if (horror != null)
        {
            horror.SetIntensity(0f);
            horror.snow = 0f;
        }
        if (partialScrim != null) partialScrim.alpha = 0f;
        if (carrierHum != null) carrierHum.Stop();
    }

    void StartIdle()
    {
        if (idleStabInterval <= 0f) return;
        _idle = StartCoroutine(IdleStabs());
    }

    void StopIdle()
    {
        if (_idle != null) { StopCoroutine(_idle); _idle = null; }
    }

    IEnumerator IdleStabs()
    {
        while (true)
        {
            // Irregular spacing reads as a fault, regular spacing reads as an animation.
            yield return WaitUnscaled(idleStabInterval * Random.Range(0.6f, 1.7f));
            if (horror != null) horror.Burst(idleStabStrength, 0.15f);
        }
    }

    static IEnumerator WaitUnscaled(float seconds)
    {
        float t = 0f;
        while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
    }
}

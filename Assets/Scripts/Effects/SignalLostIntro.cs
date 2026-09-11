using System.Collections;
using UnityEngine;

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
    [SerializeField] private AudioClip carrierHum;

    [Header("Beats")]
    [SerializeField] private float stabDuration = 0.22f;
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

#nullable enable
    Coroutine? idle;
    ControllableAudioChannel? audioChannel;

    void Awake()
    {
        if (partialScrim != null) partialScrim.alpha = 0f;
    }

    public string DeathMessage() => "Signal Lost...";

    public IEnumerator Play()
    {
        StopIdle();
        
        var activeCamera = new GetActiveCamera().Query();
        if (activeCamera != null) activeCamera.m_Lens.OrthographicSize += 1.5f;
        if (carrierHum != null)
        {
            audioChannel = AudioManager.Instance.CreateChannel(carrierHum, AudioCategory.Music, level: 0f);
            audioChannel.Play();
        }

        {
            float elapsed = 0f;
            while (elapsed < stabDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / stabDuration);

                if (horror != null) horror.SetIntensity(Mathf.Lerp(0f, 0.75f, k));
                if (partialScrim != null) partialScrim.alpha = Mathf.Lerp(0f, scrimAlpha, k);
                if (audioChannel != null)
                    audioChannel.SetLevel(Mathf.Clamp01(elapsed / Mathf.Max(humFadeIn, 0.01f)));

                yield return null;
            }
        }

        {
            if (horror != null) yield return horror.SignalLoss(snowHold);
        }

        {
            float from = horror != null ? horror.intensity : 0f;
            float elapsed = 0f;
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
                horror.pulseAmount = 0.06f;
                horror.snow = 0f;
            }
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
        if (audioChannel != null) audioChannel.Stop();
    }

    void StartIdle()
    {
        if (idleStabInterval <= 0f) return;
        idle = StartCoroutine(IdleStabs());
    }

    void StopIdle()
    {
        if (idle != null) { StopCoroutine(idle); idle = null; }
    }

    IEnumerator IdleStabs()
    {
        while (true)
        {
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

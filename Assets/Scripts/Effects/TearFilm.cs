using System.Collections;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Owns the tear film as a single global state that every affected shader
    /// reads. Nothing needs wiring per object: the moon glow and every star streak
    /// pick up the same three globals, which is what guarantees they share an axis.
    /// Vary the axis per source and the effect reads as glitter rather than vision.
    ///
    /// The model is one sawtooth. Distortion rises as the film thins and breaks up,
    /// then drops fast on a clear event. A blink and a shed tear are the same
    /// waveform at different drop speeds, so <see cref="clearSeconds"/> is the only
    /// thing separating them:
    ///
    ///   0.06  a blink. Reads as someone looking at the moon.
    ///   0.30  fluid shedding. A body event, not a gaze event.
    ///
    /// The second is the safer default here, because the camera is on the moon
    /// while Jackie is looking at Ives, and a blink asserts a gaze that is not hers.
    /// </summary>
    public class TearFilm : MonoBehaviour
    {
        [Header("Level")]
        [Tooltip("Peak distortion once the film has fully broken up.")]
        [SerializeField, Range(0f, 1f)] private float peakAmount = 0.55f;

        [Tooltip("Distortion immediately after a clear event. Never zero: the film is thin, not absent.")]
        [SerializeField, Range(0f, 1f)] private float clearedAmount = 0.15f;

        [Header("Axis")]
        [Tooltip("Degrees. 0 is horizontal. This is the dominant prong; the minor one sits 45 off it.")]
        [SerializeField, Range(0f, 180f)] private float axisDegrees = 0f;

        [Tooltip("Slow wander so the axis never sits perfectly still.")]
        [SerializeField, Range(0f, 15f)] private float axisWanderDegrees = 4f;

        [Header("Breakup")]
        [Tooltip("Seconds from a clear event back up to peak.")]
        [SerializeField] private float breakupSeconds = 3.2f;

        [Tooltip("Seconds for a clear event to take effect. 0.06 blink, 0.30 shed.")]
        [SerializeField] private float clearSeconds = 0.30f;

        [Tooltip("Average seconds between spontaneous clear events, randomised +/- 40%.")]
        [SerializeField] private float clearInterval = 5.5f;

        [Header("Onset")]
        [SerializeField] private float onsetSeconds = 2.5f;

        [Header("Debug")]
        [Tooltip("Ignore all state and drive the global directly. Use this to tune prong shape without running the scene.")]
        [SerializeField] private bool debugOverride = false;

        [SerializeField, Range(0f, 1f)] private float debugStrength = 0.5f;

        [Tooltip("Logs every state change, so you can see whether the cue arrived.")]
        [SerializeField] private bool logEvents = true;

        [Tooltip("Read-only. The value actually being written to the shader.")]
        [SerializeField] private float currentStrength;

        private static readonly int StrengthId = Shader.PropertyToID("_TearStrength");
        private static readonly int AxisId = Shader.PropertyToID("_TearAxis");

        // 0 while dry, 1 while crying. Multiplies everything.
        private float envelope;

        // 0 just cleared, 1 fully broken up.
        private float breakup = 1f;

        private float clearVelocity;
        private float nextClearAt;
        private bool running;
        private float wanderSeed;

        private void Awake()
        {
            wanderSeed = Random.Range(0f, 100f);
            Apply();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (running && Time.unscaledTime >= nextClearAt) Clear();

            // Film thins back out toward full breakup.
            if (breakup < 1f && clearVelocity <= 0f)
                breakup = Mathf.Min(1f, breakup + dt / Mathf.Max(breakupSeconds, 0.01f));

            Apply();
        }

        private void Apply()
        {
            float amount = Mathf.Lerp(clearedAmount, peakAmount, breakup);
            currentStrength = debugOverride ? debugStrength : envelope * amount;

            Shader.SetGlobalFloat(StrengthId, currentStrength);

            float wander = Mathf.Sin((Time.unscaledTime + wanderSeed) * 0.21f) * axisWanderDegrees;
            Shader.SetGlobalFloat(AxisId, (axisDegrees + wander) * Mathf.Deg2Rad);
        }

        // Globals persist in the editor after play mode ends, which makes a stale
        // nonzero value look like a working effect in material previews while the
        // game shows nothing. Clearing on disable keeps the two honest.
        private void OnDisable() => Shader.SetGlobalFloat(StrengthId, 0f);

        [ContextMenu("Test / Begin")]
        private void TestBegin() => Begin();

        [ContextMenu("Test / Clear")]
        private void TestClear() => Clear();

        [ContextMenu("Test / End")]
        private void TestEnd() => End();

        /// <summary>Fired by the tearsbegin cue.</summary>
        public void Begin()
        {
            if (logEvents) Debug.Log($"[TearFilm] Begin at {Time.unscaledTime:F2}");
            running = true;
            ScheduleNextClear();
            StopAllCoroutines();
            StartCoroutine(RampEnvelope(1f, onsetSeconds));
        }

        /// <summary>
        /// Fired by the tearsend cue. Steps down rather than fading, because
        /// composure returns in stages and a linear ramp is the one place this
        /// would feel authored.
        /// </summary>
        public void End()
        {
            if (logEvents) Debug.Log($"[TearFilm] End at {Time.unscaledTime:F2}");
            running = false;
            StopAllCoroutines();
            StartCoroutine(ReleaseStaircase());
        }

        /// <summary>
        /// Forces a clear event. Call this on a beat you want it to land on,
        /// such as Jackie's nod, rather than leaving it to the timer.
        /// </summary>
        public void Clear()
        {
            StartCoroutine(ClearRoutine());
            ScheduleNextClear();
        }

        private void ScheduleNextClear() =>
            nextClearAt = Time.unscaledTime + clearInterval * Random.Range(0.6f, 1.4f);

        private IEnumerator ClearRoutine()
        {
            clearVelocity = 1f;

            float from = breakup;
            float t = 0f;
            float duration = Mathf.Max(clearSeconds, 0.01f);

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                breakup = Mathf.Lerp(from, 0f, Mathf.Clamp01(t / duration));
                yield return null;
            }

            breakup = 0f;
            clearVelocity = 0f;
        }

        private IEnumerator ReleaseStaircase()
        {
            // Three clears, each recovering less than the last.
            float[] ceilings = { 0.6f, 0.3f, 0f };
            float[] gaps = { 1.3f, 1.2f, 1.0f };

            for (int i = 0; i < ceilings.Length; i++)
            {
                yield return ClearRoutine();

                float start = envelope;
                float target = envelope * ceilings[i];
                float t = 0f;

                while (t < gaps[i])
                {
                    t += Time.unscaledDeltaTime;
                    envelope = Mathf.Lerp(start, target, Mathf.Clamp01(t / gaps[i]));
                    yield return null;
                }

                envelope = target;
            }

            envelope = 0f;
            breakup = 1f;
        }

        private IEnumerator RampEnvelope(float to, float duration)
        {
            float from = envelope;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                envelope = Mathf.Lerp(from, to, k * k * (3f - 2f * k));
                yield return null;
            }

            envelope = to;
        }
    }
}
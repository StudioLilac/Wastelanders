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
        [SerializeField, Range(0f, 1f)] private float clearedAmount = 0.15f;
        [Header("Axis")]
        [Tooltip("0 is a horizontal major prong, 90 is vertical.")]
        [SerializeField, Range(0f, 180f)] private float axisDegrees = 0f;

        [Tooltip("Slow wander so the axis never sits perfectly still.")]
        [SerializeField, Range(0f, 15f)] private float axisWanderDegrees = 4f;

        [Header("Breakup")]
        [Tooltip("Seconds from a clear event back up to peak.")]
        [SerializeField] private float breakupSeconds = 3.2f;

        [Tooltip("Seconds for a clear event to take effect. 0.06 blink, 0.30 shed.")]
        [SerializeField] private float clearSeconds = 0.30f;

        [Header("Onset")]
        [SerializeField] private float onsetSeconds = 2.5f;

        [Header("Debug")]
        [Tooltip("Ignore all state and drive the global directly. Use this to tune prong shape without running the scene.")]
        [SerializeField] private bool debugOverride = false;

        [SerializeField, Range(0f, 1f)] private float debugStrength = 0.5f;

        [Tooltip("Read-only. The value actually being written to the shaders.")]
        [SerializeField] private float currentStrength;

        private float currentAxis;

        // 0 while dry, 1 while crying. Multiplies everything.
        private float envelope;

        // 0 just cleared, 1 fully broken up.
        private float breakup = 1f;
        private float clearVelocity;
        private float wanderSeed;

        private void Awake()
        {
            wanderSeed = Random.Range(0f, 100f);
            Apply();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // Film thins back out toward full breakup.
            if (breakup < 1f && clearVelocity <= 0f)
                breakup = Mathf.Min(1f, breakup + dt / Mathf.Max(breakupSeconds, 0.01f));

            Apply();
        }

        /// <summary>Current distortion, 0 to 1. Read by TearTarget each frame.</summary>
        public float Strength => currentStrength;

        /// <summary>Film axis in radians. Every source must share it.</summary>
        public float AxisRadians => currentAxis;

        private void Apply()
        {
            float amount = Mathf.Lerp(clearedAmount, peakAmount, breakup);
            currentStrength = debugOverride ? debugStrength : envelope * amount;

            float wander = Mathf.Sin((Time.unscaledTime + wanderSeed) * 0.21f) * axisWanderDegrees;
            currentAxis = (axisDegrees + wander) * Mathf.Deg2Rad;
        }

        [ContextMenu("Test / Begin")]
        private void TestBegin() => Begin();

        [ContextMenu("Test / End")]
        private void TestEnd() => End();

        /// <summary>Fired by the tearsbegin cue.</summary>
        public void Begin()
        {
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
            StopAllCoroutines();
            StartCoroutine(ReleaseStaircase());
        }

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
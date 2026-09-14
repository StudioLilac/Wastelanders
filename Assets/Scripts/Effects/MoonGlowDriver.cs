using System;
using System.Collections;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Feeds the MoonGlow shader its time and intensity.
    ///
    /// Two reasons this exists rather than letting the shader read _Time directly:
    ///
    ///  - _Time.y is scaled by Time.timeScale. The caption performer runs on
    ///    unscaled time, so a timescale change would drift the glow out of step
    ///    with everything else in the scene.
    ///  - You want a hook for the music. Set <see cref="TimeSource"/> to an
    ///    FMOD timeline position and the pulse phase locks to the track instead of
    ///    the wall clock, which is free rhythm you would otherwise have to fake.
    ///
    /// Writes through a MaterialPropertyBlock so the shared material asset is never
    /// mutated, which matters because otherwise editor play sessions leave dirty
    /// material files in version control.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MoonGlowDriver : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField, Range(0f, 4f)] private float intensity = 1f;

        [Tooltip("Multiplied into intensity. Ramp this from a cue to fade the glow up when the moon shot begins.")]
        [SerializeField, Range(0f, 1f)] private float envelope = 1f;

        [Header("Pulse")]
        [Tooltip("Extra pulse amplitude you can drive from the music, added to the shader's own.")]
        [SerializeField, Range(0f, 0.5f)] private float musicPulse = 0f;

        [SerializeField, Range(0f, 0.5f)] private float basePulseAmount = 0.12f;

        [Header("Tear Film")]
        [Tooltip("Direct reference. Strength and axis are pushed into this renderer's property block each frame.")]
        [SerializeField] private TearFilm tearFilm;

        [Header("Startup")]
        [Tooltip("Randomise the starting phase so the pulse is not identical on every playthrough.")]
        [SerializeField] private bool randomisePhase = true;

        private static readonly int TimeId = Shader.PropertyToID("_GlowTime");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int PulseId = Shader.PropertyToID("_PulseAmount");
        private static readonly int UseCustomId = Shader.PropertyToID("_UseCustomTime");
        private static readonly int TearStrengthId = Shader.PropertyToID("_TearStrength");
        private static readonly int TearAxisId = Shader.PropertyToID("_TearAxis");

        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock block;
        private float phaseOffset;
        private float elapsed;

        /// <summary>
        /// Seconds driving the pulse. Defaults to unscaled time accumulated from
        /// enable. Replace with an FMOD timeline position in seconds to lock the
        /// breathing to the ending track.
        /// </summary>
        public Func<float> TimeSource { get; set; }

        /// <summary>Envelope, for cue-driven ramps. See <see cref="Ramp"/>.</summary>
        public float Envelope
        {
            get => envelope;
            set => envelope = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
            phaseOffset = randomisePhase ? UnityEngine.Random.Range(0f, 120f) : 0f;
        }

        private void OnEnable() => elapsed = 0f;

        private void LateUpdate()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = (TimeSource?.Invoke() ?? elapsed) + phaseOffset;

            spriteRenderer.GetPropertyBlock(block);
            block.SetFloat(UseCustomId, 1f);
            block.SetFloat(TimeId, t);
            block.SetFloat(IntensityId, intensity * envelope);
            block.SetFloat(PulseId, basePulseAmount + musicPulse);

            // Note _Intensity above scales the aura only. The flare has its own
            // brightness path in the shader, so fading the moon's glow in and out
            // never dims the prongs.
            if (tearFilm != null)
            {
                block.SetFloat(TearStrengthId, tearFilm.Strength);
                block.SetFloat(TearAxisId, tearFilm.AxisRadians);
            }

            spriteRenderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Ramps the envelope. Call from a cue so the moon wakes up as the scene
        /// cuts to it rather than being at full brightness on the first frame.
        /// </summary>
        public IEnumerator Ramp(float to, float duration)
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
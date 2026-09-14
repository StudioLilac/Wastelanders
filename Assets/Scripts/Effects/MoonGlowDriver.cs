using System;
using System.Collections;
using UnityEngine;

namespace Cinematics
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MoonGlowDriver : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField, Range(0f, 4f)] private float intensity = 1f;

        [Tooltip("Multiplied into intensity. Ramp this from a cue to fade the glow up when the moon shot begins.")]
        [SerializeField, Range(0f, 1f)] private float envelope = 1f;

        [Header("Pulse")]
        [SerializeField, Range(0f, 0.5f)] private float basePulseAmount = 0.12f;

        [Header("Startup")]
        [Tooltip("Randomise the starting phase so the pulse is not identical on every playthrough.")]
        [SerializeField] private bool randomisePhase = true;

        private static readonly int TimeId = Shader.PropertyToID("_GlowTime");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int PulseId = Shader.PropertyToID("_PulseAmount");
        private static readonly int UseCustomId = Shader.PropertyToID("_UseCustomTime");

        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock block;
        private float phaseOffset;
        private float elapsed;

        public Func<float> TimeSource { get; set; }

        /// Envelope, for cue-driven ramp ing.
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
            block.SetFloat(PulseId, basePulseAmount);
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

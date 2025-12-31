using UnityEngine;

namespace Particles {
    /// <summary>
    /// Controls a blizzard/snow particle system effect with realistic snow physics.
    /// Snow is affected more by wind, tumbles as it falls, and has a longer lifetime than rain.
    /// Attach this to a GameObject with a ParticleSystem component.
    /// </summary>
    public class Blizzard : Particles {
        [Header("Snow Settings")]
        [Tooltip("Size of snowflakes")]
        [SerializeField]
        private float flakeSize = 0.2f;

        [Tooltip("Variation in snowflake size for natural look")]
        [SerializeField]
        private float flakeSizeVariation = 0.1f;

        [Tooltip("Color of the snow")]
        [SerializeField]
        private Color snowColor = new Color(1f, 1f, 1f, 0.6f);

        [Header("Wind Settings")]
        [Tooltip("Wind is more pronounced on snow - multiplier for wind effect")]
        [SerializeField]
        private float windInfluence = 2f;

        [Tooltip("How much wind gusts affect the snow")]
        [SerializeField]
        private float windGustAmount = 0f;

        [Tooltip("Speed of wind gusts")]
        [SerializeField]
        private float windGustSpeed = 1f;

        [Header("Fluttering Effect")]
        [Tooltip("Amount of side-to-side fluttering motion")]
        [SerializeField]
        private float flutterAmount = 0.5f;

        [Tooltip("Speed of the fluttering motion")]
        [SerializeField]
        private float flutterSpeed = 2f;

        [Header("Rotation")]
        [Tooltip("Enable snowflake rotation for tumbling effect")]
        [SerializeField]
        private bool enableRotation = true;

        [Tooltip("Angular velocity of rotating snowflakes")]
        [SerializeField]
        private float rotationSpeed = 180f;

        private ParticleSystem.SizeOverLifetimeModule sizeModule;
        private ParticleSystem.RotationOverLifetimeModule rotationModule;

        private float windTimer;

        protected override void Awake() {
            base.Awake();
            StartSnow();
        }

        private void Update() {
            if (windGustAmount > 0f) {
                windTimer += Time.deltaTime * windGustSpeed;
                float gustWind = windStrength + Mathf.Sin(windTimer) * windGustAmount;
                velocityModule.x = gustWind * windInfluence;
            }
        }

        protected override void InitializeParticleSystem() {
            base.InitializeParticleSystem();

            // Cache additional modules
            sizeModule = particleSystem.sizeOverLifetime;
            rotationModule = particleSystem.rotationOverLifetime;

            // Configure main module specifics
            mainModule.startSize = new ParticleSystem.MinMaxCurve(flakeSize * (1f - flakeSizeVariation), flakeSize * (1f + flakeSizeVariation));
            mainModule.startColor = snowColor;
            mainModule.gravityModifier = 0.1f; // Slight gravity for natural settling
            mainModule.maxParticles = 5000;

            // Configure velocity with wind influence
            velocityModule.x = windStrength * windInfluence;

            // Configure size over lifetime (fade in/out for visual effect)
            sizeModule.enabled = true;
            var curve = AnimationCurve.EaseInOut(0, 0.3f, 1, 0.1f);
            sizeModule.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // Configure rotation for tumbling effect
            rotationModule.enabled = enableRotation;
            if (enableRotation) {
                rotationModule.z = new ParticleSystem.MinMaxCurve(0f, rotationSpeed);
            }

            // Configure renderer for billboard particles (snowflakes)
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null) {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.World;
            }
        }

        /// <summary>
        /// Starts the blizzard effect
        /// </summary>
        public void StartSnow() {
            StartEffect();
        }

        /// <summary>
        /// Stops the blizzard effect
        /// </summary>
        public void StopSnow() {
            StopEffect();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (particleSystem == null)
                particleSystem = GetComponent<ParticleSystem>();

            if (particleSystem != null) {
                InitializeParticleSystem();
            }
        }
#endif
    }
}

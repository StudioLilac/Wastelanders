using UnityEngine;

namespace Particles {
    /// <summary>
    /// Controls a blizzard/snow particle system effect with realistic snow physics.
    /// Snow is affected more by wind, tumbles as it falls, and has a longer lifetime than rain.
    /// Attach this to a GameObject with a ParticleSystem component.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class Blizzard : MonoBehaviour {
        [Header("Snow Settings")]
        [Tooltip("Number of snow particles emitted per second")]
        [SerializeField]
        private float emissionRate = 50f;

        [Tooltip("Lifetime of each snowflake in seconds (longer than rain)")]
        [SerializeField]
        private float lifetime = 4f;

        [Tooltip("Base fall speed of snow (slower than rain)")]
        [SerializeField]
        private float fallSpeed = 3f;

        [Tooltip("Width of the snow spawn area")]
        [SerializeField]
        private float spawnWidth = 50f;

        [Tooltip("Height of the snow spawn area")]
        [SerializeField]
        private float spawnHeight = 3f;

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
        [Tooltip("Horizontal wind force affecting snow (snow is very affected by wind)")]
        [SerializeField]
        private float windStrength = 0f;

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

        [Header("Intensity")]
        [Range(0f, 1f)]
        [Tooltip("Overall snow intensity (0 = no snow, 1 = heavy blizzard)")]
        [SerializeField]
        private float intensity = 1f;

        private ParticleSystem snowParticleSystem;
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.ShapeModule shapeModule;
        private ParticleSystem.VelocityOverLifetimeModule velocityModule;
        private ParticleSystem.SizeOverLifetimeModule sizeModule;
        private ParticleSystem.RotationOverLifetimeModule rotationModule;

        private float baseEmissionRate;
        private float windTimer;

        private void Awake() {
            snowParticleSystem = GetComponent<ParticleSystem>();
            InitializeParticleSystem();
            StartSnow();
        }

        private void Update() {
            // Simulate wind gusts
            if (windGustAmount > 0f) {
                windTimer += Time.deltaTime * windGustSpeed;
                float gustWind = windStrength + Mathf.Sin(windTimer) * windGustAmount;
                velocityModule.x = gustWind * windInfluence;
            }
        }

        private void InitializeParticleSystem() {
            // Cache modules
            mainModule = snowParticleSystem.main;
            emissionModule = snowParticleSystem.emission;
            shapeModule = snowParticleSystem.shape;
            velocityModule = snowParticleSystem.velocityOverLifetime;
            sizeModule = snowParticleSystem.sizeOverLifetime;
            rotationModule = snowParticleSystem.rotationOverLifetime;

            // Configure main module
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            mainModule.startLifetime = lifetime;
            mainModule.startSpeed = 0f; // We control speed via velocity over lifetime
            // Use size range for variation
            mainModule.startSize = new ParticleSystem.MinMaxCurve(flakeSize * (1f - flakeSizeVariation), flakeSize * (1f + flakeSizeVariation));
            mainModule.startColor = snowColor;
            mainModule.gravityModifier = 0.1f; // Slight gravity for natural settling
            mainModule.maxParticles = 5000;

            // Configure emission
            baseEmissionRate = emissionRate;
            emissionModule.rateOverTime = emissionRate * intensity;

            // Configure shape (box emitter above the scene)
            shapeModule.shapeType = ParticleSystemShapeType.Box;
            shapeModule.scale = new Vector3(spawnWidth, spawnHeight, 1f);

            // Configure velocity for falling snow
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.World;
            velocityModule.x = windStrength * windInfluence;
            velocityModule.y = -fallSpeed;
            velocityModule.z = 0f;

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
            var renderer = snowParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null) {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.World;
            }
        }

        /// <summary>
        /// Sets the snow intensity (0 = no snow, 1 = heavy blizzard)
        /// </summary>
        public void SetIntensity(float newIntensity) {
            intensity = Mathf.Clamp01(newIntensity);
            emissionModule.rateOverTime = baseEmissionRate * intensity;
        }

        /// <summary>
        /// Sets the wind strength affecting snow (snow is very wind-sensitive)
        /// </summary>
        public void SetWindStrength(float strength) {
            windStrength = strength;
            if (windGustAmount <= 0f) {
                velocityModule.x = windStrength * windInfluence;
            }
        }

        /// <summary>
        /// Sets wind gust parameters for dynamic wind changes
        /// </summary>
        public void SetWindGust(float gustAmount, float gustSpeed) {
            windGustAmount = gustAmount;
            windGustSpeed = gustSpeed;
            windTimer = 0f;
        }

        /// <summary>
        /// Sets the fall speed of snowflakes
        /// </summary>
        public void SetFallSpeed(float speed) {
            fallSpeed = speed;
            velocityModule.y = -fallSpeed;
        }

        /// <summary>
        /// Sets the emission rate of snow particles
        /// </summary>
        public void SetEmissionRate(float rate) {
            emissionRate = rate;
            baseEmissionRate = rate;
            emissionModule.rateOverTime = baseEmissionRate * intensity;
        }

        /// <summary>
        /// Sets the color of the snow
        /// </summary>
        public void SetSnowColor(Color color) {
            snowColor = color;
            if (snowParticleSystem != null) {
                mainModule.startColor = snowColor;
            }
        }

        /// <summary>
        /// Sets the spawn area dimensions
        /// </summary>
        public void SetSpawnArea(float width, float height) {
            spawnWidth = width;
            spawnHeight = height;
            shapeModule.scale = new Vector3(spawnWidth, spawnHeight, 1f);
        }

        /// <summary>
        /// Sets the snowflake size
        /// </summary>
        public void SetFlakeSize(float size) {
            flakeSize = size;
            if (snowParticleSystem != null) {
                mainModule.startSize = new ParticleSystem.MinMaxCurve(flakeSize * (1f - flakeSizeVariation), flakeSize * (1f + flakeSizeVariation));
            }
        }

        /// <summary>
        /// Sets the fluttering effect of snowflakes
        /// </summary>
        public void SetFlutterAmount(float amount) {
            flutterAmount = amount;
        }

        /// <summary>
        /// Starts the blizzard effect
        /// </summary>
        public void StartSnow() {
            if (!snowParticleSystem.isPlaying) {
                snowParticleSystem.Play();
            }
        }

        /// <summary>
        /// Stops the blizzard effect
        /// </summary>
        public void StopSnow() {
            if (snowParticleSystem.isPlaying) {
                snowParticleSystem.Stop();
            }
        }

        /// <summary>
        /// Checks if snow is currently falling
        /// </summary>
        public bool IsSnowing => snowParticleSystem.isPlaying && intensity > 0f;

#if UNITY_EDITOR
        private void OnValidate() {
            if (snowParticleSystem == null)
                snowParticleSystem = GetComponent<ParticleSystem>();

            if (snowParticleSystem != null) {
                InitializeParticleSystem();
            }
        }
#endif
    }
}

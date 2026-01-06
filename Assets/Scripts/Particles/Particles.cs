using UnityEngine;

namespace Particles {
    /// <summary>
    /// Base class for particle effects like rain and snow.
    /// Attach this to a GameObject with a ParticleSystem component.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public abstract class Particles : MonoBehaviour {
        [Header("Particle Settings")]
        [Tooltip("Number of particles emitted per second")]
        [SerializeField]
        protected float emissionRate = 100f;

        [Tooltip("Lifetime of each particle in seconds")]
        [SerializeField]
        protected float lifetime = 1.5f;

        [Tooltip("Speed of falling particles")]
        [SerializeField]
        protected float fallSpeed = 20f;

        [Tooltip("Width of the particle spawn area")]
        [SerializeField]
        protected float spawnWidth = 40f;

        [Tooltip("Height of the particle spawn area")]
        [SerializeField]
        protected float spawnHeight = 2f;

        [Header("Wind Settings")]
        [Tooltip("Horizontal wind force affecting particles")]
        [SerializeField]
        protected float windStrength = 0f;

        [Header("Intensity")]
        [Range(0f, 1f)]
        [Tooltip("Overall particle intensity (0 = no particles, 1 = full effect)")]
        [SerializeField]
        protected float intensity = 1f;

        protected ParticleSystem mainParticleSystem;
        protected ParticleSystem.MainModule mainModule;
        protected ParticleSystem.EmissionModule emissionModule;
        protected ParticleSystem.ShapeModule shapeModule;
        protected ParticleSystem.VelocityOverLifetimeModule velocityModule;

        protected float baseEmissionRate;

        protected virtual void Awake() {
            mainParticleSystem = GetComponent<ParticleSystem>();
            InitializeParticleSystem();
        }

        protected virtual void InitializeParticleSystem() {
            // Each piece of code below initializes a "module" (part) of the particle system.
            mainModule = mainParticleSystem.main;
            emissionModule = mainParticleSystem.emission;
            shapeModule = mainParticleSystem.shape;
            velocityModule = mainParticleSystem.velocityOverLifetime;

            // Main module
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            mainModule.startLifetime = lifetime;
            mainModule.startSpeed = 0f;
            mainModule.gravityModifier = 0f;
            mainModule.maxParticles = 10000;

            // Emission module
            baseEmissionRate = emissionRate;
            emissionModule.rateOverTime = emissionRate * intensity;

            // Shape module
            shapeModule.shapeType = ParticleSystemShapeType.Box;
            shapeModule.scale = new Vector3(spawnWidth, spawnHeight, 1f);

            // Velocity module
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.World;
            velocityModule.x = windStrength;
            velocityModule.y = -fallSpeed;
            velocityModule.z = 0f;
        }

        /// <summary>
        /// Sets the particle intensity (0 = no particles, 1 = full effect). This is typically what you'll use to interact
        /// with the particle system at runtime, e.g. rain should start pouring after a certain event.
        /// </summary>
        public void SetIntensity(float newIntensity) {
            intensity = Mathf.Clamp01(newIntensity);
            emissionModule.rateOverTime = baseEmissionRate * intensity;
        }

        /// <summary>
        /// Starts the particle effect
        /// </summary>
        protected void StartEffect() {
            if (!mainParticleSystem.isPlaying) {
                mainParticleSystem.Play();
            }
        }

        /// <summary>
        /// Stops the particle effect
        /// </summary>
        protected void StopEffect() {
            if (mainParticleSystem.isPlaying) {
                mainParticleSystem.Stop();
            }
        }

        /// <summary>
        /// Checks if the effect is currently active
        /// </summary>
        public virtual bool IsActive => mainParticleSystem.isPlaying && intensity > 0f;
    }
}
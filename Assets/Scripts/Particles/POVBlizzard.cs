using UnityEngine;

namespace Particles {
    /// <summary>
    /// Blizzard effect where snowflakes fly into the camera (POV effect).
    /// Works in 2D by using Z-depth, size growth, and alpha fading.
    /// </summary>
    public class POVBlizzard : Particles {
        [Header("Snowflake")]
        [SerializeField]
        private float flakeSize = 0.15f;

        [SerializeField]
        private float flakeSizeVariation = 0.5f;

        [SerializeField]
        private Color snowColor = new Color(1f, 1f, 1f, 0.6f);

        [Header("Depth")]
        [Tooltip("How far back snowflakes spawn (negative Z)")]
        [SerializeField]
        private float depthRange = 25f;

        [Tooltip("Speed at which snow moves toward the camera")]
        [SerializeField]
        private float intoCameraSpeed = 12f;

        [Header("Drift / Wind")]
        [SerializeField]
        private float lateralDrift = 0.5f;

        [SerializeField]
        private float verticalDrift = 0.3f;

        [SerializeField]
        private float driftFrequency = 1.2f;

        [Header("Rotation")]
        [SerializeField]
        private bool enableRotation = true;

        [SerializeField]
        private float rotationSpeed = 180f;

        private ParticleSystem.SizeOverLifetimeModule sizeModule;
        private ParticleSystem.RotationOverLifetimeModule rotationModule;
        private ParticleSystem.ColorOverLifetimeModule colorModule;
        private ParticleSystem.ShapeModule shapeModuleBlizzard;

        private float driftTimer;

        protected override void Awake() {
            base.Awake();
            StartSnow();
        }

        private void Update() {
            driftTimer += Time.deltaTime * driftFrequency;

            // Subtle swirling motion
            velocityModule.x = Mathf.Sin(driftTimer) * lateralDrift;
            velocityModule.y = Mathf.Cos(driftTimer * 0.8f) * verticalDrift;

            // Constant motion into camera
            velocityModule.z = intoCameraSpeed;
        }

        protected override void InitializeParticleSystem() {
            base.InitializeParticleSystem();

            sizeModule = mainParticleSystem.sizeOverLifetime;
            rotationModule = mainParticleSystem.rotationOverLifetime;
            colorModule = mainParticleSystem.colorOverLifetime;
            shapeModuleBlizzard = mainParticleSystem.shape;

            // ───────── MAIN ─────────
            mainModule.startSize = new ParticleSystem.MinMaxCurve(
                flakeSize * (1f - flakeSizeVariation),
                flakeSize * (1f + flakeSizeVariation)
            );

            mainModule.startColor = snowColor;
            mainModule.gravityModifier = 0f;
            mainModule.maxParticles = Mathf.FloorToInt(emissionRate * 5);
            mainModule.startLifetime = depthRange / intoCameraSpeed;

            // ───────── SHAPE (DEPTH SPAWN) ─────────
            shapeModuleBlizzard.enabled = true;
            shapeModuleBlizzard.shapeType = ParticleSystemShapeType.Box;
            shapeModuleBlizzard.scale = new Vector3(20f, 12f, depthRange);
            shapeModuleBlizzard.position = new Vector3(0f, 0f, -depthRange * 0.5f);

            // ───────── VELOCITY ─────────
            velocityModule.z = intoCameraSpeed;

            // ───────── SIZE OVER LIFETIME ─────────
            sizeModule.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.05f),   // far away
                new Keyframe(0.6f, 1.0f),
                new Keyframe(1f, 1.2f)     // near camera
            );
            sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // ───────── COLOR / FADE ─────────
            colorModule.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(snowColor, 0f),
                    new GradientColorKey(snowColor, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(snowColor.a, 0.2f),
                    new GradientAlphaKey(snowColor.a, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );

            colorModule.color = gradient;

            // ───────── ROTATION ─────────
            rotationModule.enabled = enableRotation;
            if (enableRotation) {
                rotationModule.z = new ParticleSystem.MinMaxCurve(-rotationSpeed, rotationSpeed);
            }

            // ───────── RENDERER ─────────
            var renderer = mainParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null) {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.View;
            }
        }

        public void StartSnow() {
            StartEffect();
        }

        public void StopSnow() {
            StopEffect();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (mainParticleSystem == null)
                mainParticleSystem = GetComponent<ParticleSystem>();

            if (mainParticleSystem != null) {
                InitializeParticleSystem();
            }
        }
#endif
    }
}

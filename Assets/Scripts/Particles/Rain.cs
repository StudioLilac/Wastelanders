using UnityEngine;

namespace Particles {
    /// <summary>
    /// Controls a rain particle system effect in the foreground of the scene.
    /// Attach this to a GameObject with a ParticleSystem component.
    /// </summary>
    public class Rain : Particles {
        [Header("Rain Settings")]
        [Tooltip("Length/stretch of rain drops")]
        [SerializeField]
        private float dropLength = 100f;

        [Tooltip("Width of rain drops")]
        [SerializeField]
        private float dropWidth = 0.02f;

        [Tooltip("Color of the rain")]
        [SerializeField]
        private Color rainColor = new Color(0.7f, 0.8f, 1f, 0.5f);

        protected override void InitializeParticleSystem() {
            base.InitializeParticleSystem();
            
            mainModule.startSize = dropWidth; // Width of the rain drop (length controlled by renderer lengthScale)
            mainModule.startColor = rainColor;

            // Configure renderer for stretched billboards (rain effect)
            var renderer = mainParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null) {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.velocityScale = 0f;
                renderer.lengthScale = dropLength;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (mainParticleSystem == null)
                mainParticleSystem = GetComponent<ParticleSystem>();

            if (mainParticleSystem != null && Application.isPlaying) {
                InitializeParticleSystem();
            }
        }
#endif
    }
}
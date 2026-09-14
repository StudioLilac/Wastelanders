#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace UI_Elements.FadeScreen {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Animation))]
    public class AnimatedUIFadeHandler : FadeHandlerBase
    {
        [SerializeField] private Image uiImage = null!;
        [SerializeField] private Animation uiAnimation = null!;

        public Image Image => uiImage;
        public Animation Animation => uiAnimation;

        private AnimationClip? currentClip;
        public AnimationClip? CurrentClip => currentClip;

        protected override float CurrentAlpha => uiImage.color.a;

        protected override void SetAlpha(float alpha)
        {
            if (uiImage != null)
            {
                Color currentColor = uiImage.color;
                currentColor.a = Mathf.Clamp01(alpha);
                uiImage.color = currentColor;
            }
        }

        /// <summary>Assigns and plays the given clip, and records it as this layer's current clip.</summary>
        public void PlayClip(AnimationClip clip)
        {
            currentClip = clip;
            uiAnimation.clip = clip;
            uiAnimation.Play();
        }

        /// <summary>Stops playback. Called on the outgoing layer once it's fully faded out.</summary>
        public void Stop()
        {
            uiAnimation.Stop();
        }
    }
}
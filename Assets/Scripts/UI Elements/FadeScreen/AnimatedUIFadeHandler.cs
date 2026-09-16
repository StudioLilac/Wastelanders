#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace UI_Elements.FadeScreen {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Animator))]
    public class AnimatedUIFadeHandler : FadeHandlerBase
    {
        [SerializeField] private Image uiImage = null!;
        [SerializeField] private Animator uiAnimator = null!;

        public Image Image => uiImage;

        private AnimationClip? currentClip;
        private AnimationClip? templateClip;
        private AnimatorOverrideController? overrideController;
        public AnimationClip? CurrentClip => currentClip;

        private void EnsureOverrideController()
        {
            if (overrideController != null) return;

            templateClip = uiAnimator.runtimeAnimatorController.animationClips[0];
            overrideController = new AnimatorOverrideController(uiAnimator.runtimeAnimatorController);
            uiAnimator.runtimeAnimatorController = overrideController;
        }

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
            EnsureOverrideController();
            overrideController![templateClip!] = clip;
            uiAnimator.enabled = true;
            uiAnimator.Play("Background", 0, 0f);
        }

        /// <summary>Stops playback. Called on the outgoing layer once it's fully faded out.</summary>
        public void Stop()
        {
            uiAnimator.enabled = false;
        }
    }
}

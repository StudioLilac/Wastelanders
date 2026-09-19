#nullable enable
using System.Collections;
using UnityEngine;
namespace UI_Elements.FadeScreen {

    /// <summary>
    /// Crossfades between two AnimatedUIFadeHandler layers, each playing an AnimationClip,
    /// mirroring CrossFadeHandler's alpha-blend approach but for animated content instead
    /// of static sprites.
    /// </summary>
    public class AnimationCrossFadeHandler : MonoBehaviour
    {
        [SerializeField] private AnimatedUIFadeHandler frontLayer = null!;
        [SerializeField] private AnimatedUIFadeHandler backLayer = null!;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Coroutine? fade;
        private AnimationClip? currentTarget;

        public AnimationClip? CurrentClip => currentTarget;

        public void CrossFadeTo(AnimationClip? targetClip, float duration)
        {
            if (targetClip == currentTarget) return;
            currentTarget = targetClip;

            AnimatedUIFadeHandler inLayer;  // fades up to fully visible
            AnimatedUIFadeHandler outLayer; // fades out

            if (targetClip == frontLayer.CurrentClip)
            {
                inLayer = frontLayer;
                outLayer = backLayer;
            }
            else if (targetClip == backLayer.CurrentClip)
            {
                inLayer = backLayer;
                outLayer = frontLayer;
            }
            else
            {
                bool frontIsFainter = frontLayer.Image.color.a <= backLayer.Image.color.a;
                inLayer = frontIsFainter ? frontLayer : backLayer;
                outLayer = frontIsFainter ? backLayer : frontLayer;
                if (targetClip != null)
                {
                    inLayer.PlayClip(targetClip);
                }
            }

            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Crossfade(inLayer, outLayer, duration));
        }

        private IEnumerator Crossfade(AnimatedUIFadeHandler inLayer, AnimatedUIFadeHandler outLayer, float duration)
        {
            float startIn = inLayer.Image.color.a;
            float startOut = outLayer.Image.color.a;

            if (duration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = fadeCurve.Evaluate(elapsed / duration);
                    inLayer.SetAlphaImmediate(Mathf.Lerp(startIn, 1f, t));
                    outLayer.SetAlphaImmediate(Mathf.Lerp(startOut, 0f, t));
                    yield return null;
                }
            }

            inLayer.SetAlphaImmediate(1f);
            outLayer.SetAlphaImmediate(0f);
            outLayer.Stop(); // no need to keep an invisible layer animating
            fade = null;
        }
    }
}
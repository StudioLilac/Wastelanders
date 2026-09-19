using System.Collections;
using UnityEngine;

#nullable enable
public class CrossFadeHandler : MonoBehaviour
{
    [SerializeField] private UIFadeHandler frontLayer = null!;
    [SerializeField] private UIFadeHandler backLayer = null!;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine? fade;
    private Sprite? currentTarget;

    public Sprite? CurrentSprite => currentTarget;

    public void CrossFadeTo(Sprite? targetSprite, float duration)
    {
        if (targetSprite == currentTarget) return;
        currentTarget = targetSprite;

        UIFadeHandler inLayer;  // fades up to fully visible
        UIFadeHandler outLayer; // fades out

        if (targetSprite == frontLayer.Image.sprite)
        {
            inLayer = frontLayer;
            outLayer = backLayer;
        }
        else if (targetSprite == backLayer.Image.sprite)
        {
            inLayer = backLayer;
            outLayer = frontLayer;
        }
        else
        {
            bool frontIsFainter = frontLayer.Image.color.a <= backLayer.Image.color.a;
            inLayer = frontIsFainter ? frontLayer : backLayer;
            outLayer = frontIsFainter ? backLayer : frontLayer;
            inLayer.Image.sprite = targetSprite;
        }

        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(inLayer, outLayer, duration));
    }

    private IEnumerator Crossfade(UIFadeHandler inLayer, UIFadeHandler outLayer, float duration)
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
        fade = null;
    }
}

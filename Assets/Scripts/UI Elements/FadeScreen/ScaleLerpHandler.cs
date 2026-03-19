using UnityEngine;

public class ScalingLerpHandler : FadeHandlerBase
{
    [Header("UI References")]
    [Tooltip("The RectTransform to scale. Does not need to be an Image.")]
    [SerializeField] private RectTransform targetTransform = null!;

    [Header("Scaling Settings")]
    [SerializeField] private float fullScale = 4f;
    [SerializeField] private float closedScale = 0.5f;

    protected override float CurrentAlpha => Mathf.InverseLerp(fullScale, closedScale, targetTransform.localScale.x);

    protected override void SetAlpha(float alpha)
    {
        float currentScale = Mathf.Lerp(fullScale, closedScale, alpha);
        targetTransform.localScale = new Vector3(currentScale, currentScale, 1f);
    }

    // Feel free to change this easing curve to a field if necessary.
    protected override float EvaluateCurve(float fraction)
    {
        return Mathf.SmoothStep(0f, 1f, fraction);
    }
}
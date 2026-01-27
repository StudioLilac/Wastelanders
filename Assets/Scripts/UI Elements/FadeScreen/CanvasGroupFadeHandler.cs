using UnityEngine;

public class CanvasGroupFadeHandler : FadeHandlerBase
{
    [SerializeField] private CanvasGroup targetCanvasGroup;
    protected override float CurrentAlpha => targetCanvasGroup.alpha;
    protected override void SetAlpha(float alpha)
    {
        targetCanvasGroup.alpha = alpha;
    }
}

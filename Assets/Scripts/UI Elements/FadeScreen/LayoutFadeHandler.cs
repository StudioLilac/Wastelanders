using UnityEngine;
using UnityEngine.UI;

public class LayoutWidthFader : FadeHandlerBase
{
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float fullWidth = 85f;

    protected override float CurrentAlpha =>
        Mathf.Clamp01(layoutElement.preferredWidth / fullWidth);

    protected override void SetAlpha(float alpha)
    {
        layoutElement.preferredWidth = alpha * fullWidth;
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIFadeHandler : FadeHandlerBase
{
    [SerializeField] private Image uiImage;
    public Image Image => uiImage;
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
}
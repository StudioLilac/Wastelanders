using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPFadeHandler : FadeHandlerBase
{
    [SerializeField] private TMP_Text tmpText;
    public TMP_Text Text => tmpText;
    protected override float CurrentAlpha => tmpText.color.a;

    protected override void SetAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color currentColor = tmpText.color;
            currentColor.a = Mathf.Clamp01(alpha);
            tmpText.color = currentColor;
        }
    }
}
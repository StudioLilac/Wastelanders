using UnityEngine;
#nullable enable
public class SwordIcon: MonoBehaviour
{
    private static readonly int ClashStateHash = Animator.StringToHash("ClashState");
    [SerializeField] private SpriteRenderer swordsIcon = null!;
    [SerializeField] private SpriteFadeHandler swordFader = null!;
    [SerializeField] private Animator swordsAnimator = null!;
    private ClashResultType currentClashType = default;

    public void Awake()
    {
        Emphasize();
    }

    public void Emphasize()
    {
        swordsIcon.sortingLayerName = CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;
        swordsIcon.sortingOrder = CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER + 6;
    }

    public void DeEmphasize()
    {
        swordsIcon.sortingOrder = CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER - 1;
    }

    public void FadeIn(float duration) => StartCoroutine(swordFader.FadeInDarkScreen(duration));
    public void FadeOut(float duration) => StartCoroutine(swordFader.FadeInLightScreen(duration));


    public void OnMouseEnter() => new TooltipEvent(
        TextTipDisplayStyle.Display, 
        Title: currentClashType.ToString().ToUpper(),
        Body: currentClashType.GetDescription(),
        Icon: swordsIcon.sprite
        ).Invoke();

    public void OnMouseExit() => new TooltipEvent(TextTipDisplayStyle.None).Invoke();
    
    public void SetClashState(ClashResultType result)
    {
        currentClashType = result;
        swordsAnimator.SetInteger(ClashStateHash, (int)result);
    }
}
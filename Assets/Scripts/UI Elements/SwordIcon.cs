using UnityEngine;
#nullable enable
public class SwordIcon: MonoBehaviour
{
    private static readonly int ClashStateHash = Animator.StringToHash("ClashState");
    [SerializeField] private SpriteRenderer swordsIcon = null!;
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

    public void OnMouseEnter() => new TooltipText(currentClashType.ToString().ToUpper(), TextTipDisplayStyle.BottomRight).Invoke();

    public void OnMouseExit() => new TooltipText(currentClashType.ToString().ToUpper(), TextTipDisplayStyle.None).Invoke();
    
    public void SetClashState(ClashResultType result)
    {
        currentClashType = result;
        swordsAnimator.SetInteger(ClashStateHash, (int)result);
    }
}
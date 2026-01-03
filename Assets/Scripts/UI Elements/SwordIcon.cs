using UnityEngine;
#nullable enable
public class SwordIcon: MonoBehaviour
{
    private static readonly int ClashStateHash = Animator.StringToHash("ClashState");
    [SerializeField] private SpriteRenderer swordsIcon = null!;
    [SerializeField] private Animator swordsAnimator = null!;
    private ClashResultType currentClashType = default;

    public void Start()
    {
        swordsIcon.sortingLayerName = CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;
        swordsIcon.sortingOrder = CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER + 6;
    }

    public void OnMouseEnter() => new TooltipText(currentClashType.ToString().ToUpper(), true).Invoke();
    

    public void OnMouseExit() => new TooltipText(currentClashType.ToString().ToUpper(), false).Invoke();
    
    public void SetClashState(ClashResultType result)
    {
        currentClashType = result;
        swordsAnimator.SetInteger(ClashStateHash, (int)result);
    }
}
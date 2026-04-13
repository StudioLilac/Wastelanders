using System.Collections;
using UnityEngine;
using static IBattleQueueDisplayable;

#nullable enable
public class SwordIcon : MonoBehaviour, IBattleQueueDisplayable
{
    private static readonly int ClashStateHash = Animator.StringToHash("ClashState");
    [SerializeField] private SpriteRenderer swordsIcon = null!;
    [SerializeField] private SpriteFadeHandler swordFader = null!;
    [SerializeField] private Animator swordsAnimator = null!;
    private ClashResultType currentClashType = default;
    private bool isActive = true;
    public GameObject GameObject => gameObject;

    public void Awake()
    {
        Emphasize();
    }

    public void Emphasize()
    {
        swordsIcon.sortingLayerName = new GetFadeSortingLayer().Query() ?? string.Empty;
        swordsIcon.sortingOrder = (new GetFadeSortingOrder().Query() ?? 0) + 6;
    }

    public void DeEmphasize()
    {
        swordsIcon.sortingOrder = (new GetFadeSortingOrder().Query() ?? 0) - 1;
    }

    public void SetFullyTransparent()
    {
        swordFader.SetLightScreen();
    }

    public void SetFullyOpaque()
    {
        swordFader.SetDarkScreen();
    }

    public IEnumerator FadeIn()
    {
        if (!isActiveAndEnabled)
        {
            SetFullyOpaque();
            yield break;
        }
        SetFullyTransparent();
        StartCoroutine(swordFader.FadeInDarkScreen(FADE_DURATION));
    }
    public IEnumerator FadeOut() 
    {
        isActive = false;
        if (!isActiveAndEnabled)
        {
            SetFullyTransparent();
            yield break;
        }
        StartCoroutine(swordFader.FadeInLightScreen(FADE_DURATION)); 
    }

    public void ShakePlayerAction() { }

    public void OnMouseEnter()
    {
        if (!isActive) return;
        new TooltipEvent(
            TextTipDisplayStyle.Display,
            Title: currentClashType.ToString().ToUpper(),
            Body: currentClashType.GetDescription(),
            Icon: swordsIcon.sprite
        ).Invoke();
    }

    public void OnMouseExit() => new TooltipEvent(TextTipDisplayStyle.None).Invoke();
    
    public void SetClashState(ClashResultType result)
    {
        currentClashType = result;
        swordsAnimator.SetInteger(ClashStateHash, (int)result);
    }
}
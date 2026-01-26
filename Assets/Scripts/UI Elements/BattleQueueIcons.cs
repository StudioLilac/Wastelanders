using System.Collections;
using System.Collections.Generic;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UI;
using static IBattleQueueDisplayable;
public interface IBattleQueueDisplayable
{
    void Emphasize();
    void DeEmphasize();
    void SetFullyTransparent();
    IEnumerator FadeIn();
    IEnumerator FadeOut();
    GameObject GameObject { get; }

    public const float EXPAND_DURATION = 0.25f;

    public const float FADE_DURATION = 0.15f;
}

public record BattleQueueIconClick(BattleQueueIcons Icon) : IEvent { }

public class BattleQueueIcons : DisplayableClass, IBattleQueueDisplayable
{
    public GameObject GameObject => gameObject;
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] SpriteRenderer iconRenderer;

    [Header("Animation Handlers")]
    [SerializeField] SpriteFadeHandler cardIconFader;
    [SerializeField] SpriteFadeHandler targetIconFader;
    [SerializeField] SpriteFadeHandler unseenActionFader;
    [SerializeField] LayoutWidthFader widthFader;

    private bool isActive = true;

    private int FadeSortingOrder => CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER;
    private string FadeSortingLayer => CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;

    public void RenderBQIcon(ActionClass ac)
    {
        ActionClass = ac;
        targetRenderer.sprite = ac.Target.icon;
        iconRenderer.sprite = ac.GetIcon();

        Emphasize();
    }

    public void SetFullyOpaque()
    {
        widthFader.SetDarkScreen();
        cardIconFader.SetDarkScreen();
        targetIconFader.SetDarkScreen();
        unseenActionFader.SetDarkScreen();
    }

    public void SetFullyTransparent()
    {
        widthFader.SetLightScreen();
        cardIconFader.SetLightScreen();
        targetIconFader.SetLightScreen();
        unseenActionFader.SetLightScreen();
    }

    public IEnumerator FadeIn()
    {
        if (!isActiveAndEnabled)
        {
            SetFullyOpaque();
            yield break;
        }

        SetFullyTransparent();
        StartCoroutine(cardIconFader.FadeInDarkScreen(FADE_DURATION));
        StartCoroutine(targetIconFader.FadeInDarkScreen(FADE_DURATION));
        StartCoroutine(unseenActionFader.FadeInDarkScreen(FADE_DURATION));

        yield return widthFader.FadeInDarkScreen(EXPAND_DURATION);
    }

    public IEnumerator FadeOut()
    {
        isActive = false;
        if (!isActiveAndEnabled)
        {
            SetFullyTransparent(); 
            yield break;
        }
            
        StartCoroutine(unseenActionFader.FadeInLightScreen(FADE_DURATION));
        StartCoroutine(cardIconFader.FadeInLightScreen(FADE_DURATION));
        StartCoroutine(targetIconFader.FadeInLightScreen(FADE_DURATION));

        yield return widthFader.FadeInLightScreen(EXPAND_DURATION);
    }

    public void Emphasize()
    {
        iconRenderer.sortingOrder = FadeSortingOrder + 4;
        iconRenderer.sortingLayerName = FadeSortingLayer;
        targetRenderer.sortingOrder = FadeSortingOrder + 5;
        targetRenderer.sortingLayerName = FadeSortingLayer;
        unseenEnemyActionIndicator.sortingOrder = FadeSortingOrder + 5;
        unseenEnemyActionIndicator.sortingLayerName = FadeSortingLayer;
    }

    public void DeEmphasize()
    {
        iconRenderer.sortingOrder = FadeSortingOrder - 1;
        targetRenderer.sortingOrder = FadeSortingOrder - 1;
        unseenEnemyActionIndicator.sortingOrder = FadeSortingOrder - 1;
    }

    private void OnMouseDown()
    {
        if (ActionClass.Origin is PlayerClass && CombatManager.Instance.CanHighlight() && isActive)
        {
            new BattleQueueIconClick(this).Invoke();
            DeHighlightTarget();
            HideCard();
        }
    }
}

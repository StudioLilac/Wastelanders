using System.Collections;
using System.Collections.Generic;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UI;

public interface IBattleQueueDisplayable
{
    void Emphasize();
    void DeEmphasize();
    IEnumerator FadeIn();
    IEnumerator FadeOut();
    GameObject GameObject { get; }
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
    
    [Header("Settings")]
    [SerializeField] private float expandDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.15f;

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

    public IEnumerator FadeIn()
    {
        widthFader.SetLightScreen();       
        cardIconFader.SetLightScreen();    
        targetIconFader.SetLightScreen();  
        unseenActionFader.SetLightScreen();

        yield return widthFader.FadeInDarkScreen(expandDuration);
        
        StartCoroutine(cardIconFader.FadeInDarkScreen(fadeDuration));
        StartCoroutine(targetIconFader.FadeInDarkScreen(fadeDuration));
        StartCoroutine(unseenActionFader.FadeInDarkScreen(fadeDuration));
    }

    public IEnumerator FadeOut()
    {
        StartCoroutine(unseenActionFader.FadeInLightScreen(fadeDuration));
        StartCoroutine(cardIconFader.FadeInLightScreen(fadeDuration));
        yield return targetIconFader.FadeInLightScreen(fadeDuration);
        yield return widthFader.FadeInLightScreen(expandDuration);
    }

    public void Emphasize()
    {
        iconRenderer.sortingOrder = FadeSortingOrder + 4;
        iconRenderer.sortingLayerName = FadeSortingLayer;
        targetRenderer.sortingOrder = FadeSortingOrder + 5;
        targetRenderer.sortingLayerName = FadeSortingLayer;
    }

    public void DeEmphasize()
    {
        iconRenderer.sortingOrder = FadeSortingOrder - 1;
        targetRenderer.sortingOrder = FadeSortingOrder - 1;
    }

    private void OnMouseDown()
    {
        if (ActionClass.Origin is PlayerClass && CombatManager.Instance.CanHighlight())
        {
            new BattleQueueIconClick(this).Invoke();
            DeHighlightTarget();
            HideCard();
        }
    }
}

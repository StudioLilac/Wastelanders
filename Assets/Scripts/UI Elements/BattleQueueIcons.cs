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
    void ShakePlayerAction();
    GameObject GameObject { get; }
    string Name { get; }

    public const float EXPAND_DURATION = 0.25f;

    public const float FADE_DURATION = 0.15f;
}
public record BattleQueueIconClick(BattleQueueIcons Icon) : IEvent { }


// If you're noticing the battle queue icons can't be scrolled, that could be because you don't have a physics 2d raycaster on the main camera in your scene.
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


#nullable enable
    private bool isActive = true;
    private bool isShaking = false;
    private int FadeSortingOrder => new GetFadeSortingOrder().Query() ?? 0;
    private string FadeSortingLayer => new GetFadeSortingLayer().Query() ?? string.Empty;
    public string Name => ActionClass?.GetName() ?? string.Empty;

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

    public void ShakePlayerAction()
    {
        StartCoroutine(Shake());
    }

    private IEnumerator Shake() {
        if (isShaking) yield break;
        
        isShaking = true;
        if (!isActiveAndEnabled)
            yield break;
    
        Vector3 originalPosition = iconRenderer.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.5f;
        float amplitude = 10f;
        float frequency = 20f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
        
            float x = amplitude * Mathf.Sin(frequency * elapsed) * (1f - t);
        
            iconRenderer.transform.localPosition = originalPosition + new Vector3(x, 0f, 0f);
        
            yield return null;
        }
    
        iconRenderer.transform.localPosition = originalPosition;
        isShaking = false;
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
        if (ActionClass == null) return;

        if (ActionClass.Origin is PlayerClass && new CanHighlight().Query() == true && isActive)
        {
            new BattleQueueIconClick(this).Invoke();
            DeHighlightTarget();
            HideCard();
        }
        else if (ActionClass.Origin is EnemyClass && new CanHighlight().Query() == true && isActive)
        {
            new ActionIconClicked(ActionClass).Invoke();
        }
    }
}

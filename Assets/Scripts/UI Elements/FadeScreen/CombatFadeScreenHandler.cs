using System.Collections;
using UnityEngine;

#nullable enable
public record GetFadeSortingLayer() : IQuery<string?>;
public record GetFadeSortingLayerId() : IQuery<int?>;
public record GetFadeSortingOrder() : IQuery<int?>;
public record GetFadeScreenZValue() : IQuery<float?>;

public class CombatFadeScreenHandler : MonoBehaviour
{
    public static CombatFadeScreenHandler Instance { get; private set; } = null!;

    [SerializeField] private SpriteFadeHandler spriteFadeHandler = null!;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        this.Answer<GetFadeSortingLayer, string?>(_ => spriteFadeHandler.FadeSortingLayer);
        this.Answer<GetFadeSortingLayerId, int?>(_ => spriteFadeHandler.FadeSortingLayerId);
        this.Answer<GetFadeSortingOrder, int?>(_ => spriteFadeHandler.FadeSortingOrder);
        this.Answer<GetFadeScreenZValue, float?>(_ => spriteFadeHandler.FadeScreenZValue);
    }

    public void SetDarkScreen() => spriteFadeHandler.SetDarkScreen();

    public void SetLightScreen() => spriteFadeHandler.SetLightScreen();

    public IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        yield return spriteFadeHandler.FadeToAlpha(targetAlpha, duration);
    }

    public IEnumerator FadeInLightScreen(float duration)
    {
        yield return spriteFadeHandler.FadeInLightScreen(duration);
    }

    public IEnumerator FadeInDarkScreen(float duration)
    {
        yield return spriteFadeHandler.FadeInDarkScreen(duration);
    }
}
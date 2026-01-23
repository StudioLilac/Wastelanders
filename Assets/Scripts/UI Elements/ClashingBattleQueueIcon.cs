using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#nullable enable
public class ClashingBattleQueueIcon : MonoBehaviour, IBattleQueueDisplayable
{
    public GameObject GameObject => gameObject;
    [SerializeField]
    private BattleQueueIcons leftClashingAction = null!;
    [SerializeField]
    private BattleQueueIcons rightClashingAction = null!;
    [SerializeField] private SwordIcon swordIcon = null!;
    [SerializeField] private LayoutWidthFader widthFader = null!;

    private static readonly ClashCalculator Calculator = new();
    private ActionClass leftClashing = null!;
    private ActionClass rightClashing = null!;

    [Header("Settings")]
    private readonly float expandDuration = 0.25f;
    private readonly float fadeDuration = 0.15f;

    // Public initializer for this icon
    public void RenderClashingIcons(ActionClass leftClashingItem,  ActionClass rightClashingItem)
    {
        leftClashingAction.RenderBQIcon(leftClashingItem);
        rightClashingAction.RenderBQIcon(rightClashingItem);
        rightClashingAction.RenderUnseenIndicator(); // Right clashing action is conventionally the enemy action

        leftClashing = leftClashingItem;
        rightClashing = rightClashingItem;
        leftClashingItem.CardValuesUpdating += UpdateSwordIcon;
        rightClashingItem.CardValuesUpdating += UpdateSwordIcon;

        Emphasize();
        UpdateSwordIcon(null);
    }

    private void OnDisable()
    {
        leftClashing.CardValuesUpdating -= UpdateSwordIcon;
        rightClashing.CardValuesUpdating -= UpdateSwordIcon;
    }

    public void Emphasize()
    {
        swordIcon.Emphasize();
        leftClashingAction.Emphasize();
        rightClashingAction.Emphasize();
    }

    public void DeEmphasize()
    {
        swordIcon.DeEmphasize();
        leftClashingAction.DeEmphasize();
        rightClashingAction.DeEmphasize();
    }

    private void UpdateSwordIcon(ActionClass? _) =>
        swordIcon.SetClashState(Calculator.CompareRange(GetRange(leftClashing), GetRange(rightClashing)));

    private (int, int) GetRange(ActionClass actionClass) => ((actionClass.GetRolledStats().RollFloor), (actionClass.GetRolledStats().RollCeiling));

    public IEnumerator FadeIn()
    {
        swordIcon.FadeIn(fadeDuration);
        widthFader.SetLightScreen();
        StartCoroutine(leftClashingAction.FadeIn());
        StartCoroutine(rightClashingAction.FadeIn());
        yield return StartCoroutine(widthFader.FadeInDarkScreen(expandDuration));
    }

    public IEnumerator FadeOut()
    {
        swordIcon.FadeOut(fadeDuration);
        StartCoroutine(leftClashingAction.FadeOut());
        StartCoroutine(rightClashingAction.FadeOut());
        yield return StartCoroutine(widthFader.FadeInLightScreen(expandDuration));
    }
}

public enum ClashResultType
{
    None = 0,
    Dominating = 1,
    Favourable = 2,
    Even = 3,
    Unfavourable = 4,
    Hopeless = 5
}

public static class ClashResultExtensions
{
    public static string GetDescription(this ClashResultType clashResultType) => clashResultType switch
    {
        ClashResultType.Dominating => "You are overwhelmingly likely to win this clash.",
        ClashResultType.Favourable => "You have a strong chance of winning this clash.",
        ClashResultType.Even => "Both sides have an equal chance of winning this clash.",
        ClashResultType.Unfavourable => "You are unlikely to win this clash.",
        ClashResultType.Hopeless => "You are overwhelmingly unlikely to win this clash.",
        _ => "No clash data available."
    };
}

public class ClashCalculator
{
    private readonly List<ClashBracket> _brackets = new()
    {
        new ClashBracket(0.2f, ClashResultType.Hopeless),
        new ClashBracket(0.4f, ClashResultType.Unfavourable),
        new ClashBracket(0.6f, ClashResultType.Even),
        new ClashBracket(0.8f, ClashResultType.Favourable), 
        new ClashBracket(1.0f, ClashResultType.Dominating)
    };

    [SerializeField] private bool _winsIncludeTies = true;

    public ClashResultType CompareRange((int min, int max) leftRange, (int min, int max) rightRange)
    {
        float winChance = CalculateWinProbability(leftRange, rightRange, _winsIncludeTies);

        foreach (var bracket in _brackets)
        {
            if (winChance <= bracket.Threshold)
            {
                return bracket.Result;
            }
        }
        return ClashResultType.Dominating;
    }

    private float CalculateWinProbability((int min, int max) left, (int min, int max) right, bool includeTies)
    {
        float totalCombinations = (left.max - left.min + 1) * (right.max - right.min + 1);

        if (totalCombinations <= 0) return 0f; 

        int winningCombinations = 0;

        for (int l = left.min; l <= left.max; l++)
        {
            int maxWinningRightRoll = includeTies ? l : l - 1;
            int effectiveMaxR = Mathf.Min(maxWinningRightRoll, right.max);
            if (effectiveMaxR < right.min) continue;
            int winsForThisRoll = effectiveMaxR - right.min + 1;

            winningCombinations += winsForThisRoll;
        }

        return winningCombinations / totalCombinations;
    }


    private record ClashBracket(float Threshold, ClashResultType Result) { }
}
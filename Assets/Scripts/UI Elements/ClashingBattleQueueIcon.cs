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

    private static readonly ClashCalculator Calculator = new();
    private ActionClass leftClashing = null!;
    private ActionClass rightClashing = null!;


    // Public initializer for this icon
    public void RenderClashingIcons(ActionClass leftClashingItem,  ActionClass rightClashingItem)
    {
        leftClashingAction.RenderBQIcon(leftClashingItem);
        rightClashingAction.RenderBQIcon(rightClashingItem);

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
}

public enum ClashResultType
{
    None = 0,
    Dominating = 1,
    Favored = 2,
    Even = 3,
    Unfavored = 4,
    Futile = 5
}

public class ClashCalculator
{
    private readonly List<ClashBracket> _brackets = new()
    {
        new ClashBracket(0.2f, ClashResultType.Futile),
        new ClashBracket(0.4f, ClashResultType.Unfavored),
        new ClashBracket(0.6f, ClashResultType.Even),
        new ClashBracket(0.8f, ClashResultType.Favored), 
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public class ClashingBattleQueueIcon : MonoBehaviour
{
    [SerializeField]
    private BattleQueueIcons leftClashingAction = null!;
    [SerializeField]
    private BattleQueueIcons rightClashingAction = null!;
    [SerializeField]
    private SpriteRenderer swordsIcon = null!;

    [SerializeField] private Animator swordsAnimator = null!;
    private static readonly int ClashStateHash = Animator.StringToHash("ClashState");
    private static readonly ClashCalculator Calculator = new();
    private ActionClass leftClashing = null!;
    private ActionClass rightClashing = null!;


    public void Start()
    {
        swordsIcon.sortingLayerName = CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;
        swordsIcon.sortingOrder = CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER + 6;
    }

    // Public initializer for this icon
    public void RenderClashingIcons(ActionClass leftClashingItem,  ActionClass rightClashingItem)
    {
        leftClashingAction.RenderBQIcon(leftClashingItem);
        rightClashingAction.RenderBQIcon(rightClashingItem);

        leftClashing = leftClashingItem;
        rightClashing = rightClashingItem;
        leftClashingItem.CardValuesUpdating += UpdateSwordIcon;
        rightClashingItem.CardValuesUpdating += UpdateSwordIcon;

        UpdateSwordIcon(null);
    }

    private void OnDisable()
    {
        leftClashing.CardValuesUpdating -= UpdateSwordIcon;
        rightClashing.CardValuesUpdating -= UpdateSwordIcon;
    }

    private void SetClashState(ClashResultType result)
    {
        swordsAnimator.SetInteger(ClashStateHash, (int) result);
    }

    private void UpdateSwordIcon(ActionClass? _)
    {
        SetClashState(Calculator.CompareRange(GetRange(leftClashing), GetRange(rightClashing)));
    }

    private (int, int) GetRange(ActionClass actionClass) => ((actionClass.GetRolledStats().RollFloor), (actionClass.GetRolledStats().RollCeiling));
}

public enum ClashResultType
{
    None = 0,
    Dominating = 1,
    Favoured = 2,
    Neutral = 3,
    Struggling = 4,
    Hopeless = 5
}

public class ClashCalculator
{
    private readonly List<ClashBracket> _brackets = new()
    {
        new ClashBracket { Threshold = 0.2f, Result = ClashResultType.Hopeless },   // 0% - 20%
        new ClashBracket { Threshold = 0.4f, Result = ClashResultType.Struggling }, // 20% - 40%
        new ClashBracket { Threshold = 0.6f, Result = ClashResultType.Neutral },    // 40% - 60%
        new ClashBracket { Threshold = 0.8f, Result = ClashResultType.Favoured },   // 60% - 80%
        new ClashBracket { Threshold = 1.0f, Result = ClashResultType.Dominating }  // 80% - 100%
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

    [System.Serializable]
    private struct ClashBracket
    {
        public float Threshold;
        public ClashResultType Result;
    }
}
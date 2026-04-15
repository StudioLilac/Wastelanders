using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ActionClass;

public class CombatCardUI : DisplayableClass
{
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] TextMeshPro rangeText;

    [SerializeField] SpriteRenderer oneTimeUseBuff;
    [SerializeField] TextMeshPro buffIncreaseText;

    [SerializeField] GameObject oneTimeBuffObj;
    [SerializeField] GameObject buffFlipPreserver;
#nullable enable
    private int FadeSortingOrder => new GetFadeSortingOrder().Query() ?? 0;
    private string FadeSortingLayer => new GetFadeSortingLayer().Query() ?? string.Empty;

    protected override void OnDestroy()
    {
        if (ActionClass != null)
        {
            ActionClass.TargetChanged -= SetTargetIcon;
            ActionClass.CardValuesUpdating -= UpdateRangeText;
        }
        
        base.OnDestroy();
    }

    private void OnMouseDown()
    {
        if (ActionClass !=null && new CanHighlight().Query() == true)
        {
            new ActionIconClicked(ActionClass).Invoke();
        }
    }

    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().sortingLayerName = FadeSortingLayer;
        targetRenderer.sortingLayerName = FadeSortingLayer;
        unseenEnemyActionIndicator.sortingLayerName = FadeSortingLayer;

        rangeText.GetComponent<MeshRenderer>().sortingLayerName = targetRenderer.sortingLayerName;
        rangeText.GetComponent<MeshRenderer>().sortingOrder = targetRenderer.sortingOrder;

        buffIncreaseText.GetComponent<MeshRenderer>().sortingLayerName = targetRenderer.sortingLayerName;
        buffIncreaseText.GetComponent<MeshRenderer>().sortingOrder = targetRenderer.sortingOrder;
    }

    public void FaceRight()
    {/*
        FlipTransform(oneTimeBuffObj.transform, true);
        FlipTransform(buffFlipPreserver.transform, true);*/
    }

    public void FaceLeft()
    {/*
        FlipTransform(oneTimeBuffObj.transform, false);
        FlipTransform(buffFlipPreserver.transform, false);*/
    }

    public void SetBuffIcon(ActionClass.RolledStats cardDup)
    {
        (StatusEffect? buff, int lowerBound, int upperBound) = cardDup.OneTimeBuffs;
        if (lowerBound > 0 || upperBound > 0)
        {
            buffIncreaseText.text = "+" + lowerBound + "-" + upperBound;
            oneTimeUseBuff.sprite = buff?.GetIcon();
        } else
        {
            buffIncreaseText.text = "";
            oneTimeUseBuff.sprite = null;
        }
    }

    public void SetActionClass(ActionClass actionClass)
    {
        if (ActionClass != null)
        {
            ActionClass.TargetChanged -= SetTargetIcon;
            ActionClass.CardValuesUpdating -= UpdateRangeText;
        }
        ActionClass = actionClass;
        ActionClass.TargetChanged += SetTargetIcon;
        ActionClass.CardValuesUpdating += UpdateRangeText;
        SetTargetIcon(ActionClass);
        UpdateRangeText(ActionClass);
        GetComponent<SpriteRenderer>().sprite = actionClass.GetIcon();
        if (!ActionClass.IsPlayedByPlayer()) RenderUnseenIndicator();
    }

    void UpdateRangeText(ActionClass actionClass)
    {
        (_, int buffLowerBound, int buffUpperBound) = actionClass.GetRolledStats().OneTimeBuffs;
        rangeText.text = (actionClass.GetRolledStats().RollFloor - buffLowerBound) + "-" + (actionClass.GetRolledStats().RollCeiling - buffUpperBound);
        if (actionClass.Origin is EnemyClass)
        {
            rangeText.color = Color.red;
        }
        else
        {
            rangeText.color = Color.green;
        }

        SetBuffIcon(actionClass.GetRolledStats());
    }

    public void Emphasize()
    {
        GetComponent<SpriteRenderer>().sortingOrder = FadeSortingOrder + 1;

        rangeText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder + 2;
        targetRenderer.GetComponent<SpriteRenderer>().sortingOrder = FadeSortingOrder + 2;
        unseenEnemyActionIndicator.sortingOrder = FadeSortingOrder + 2;

        oneTimeUseBuff.sortingOrder = FadeSortingOrder + 2;
        buffIncreaseText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder + 2;

    }

    public void DeEmphasize()
    {
        GetComponent<SpriteRenderer>().sortingOrder = FadeSortingOrder - 3;

        rangeText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder - 1;
        targetRenderer.GetComponent<SpriteRenderer>().sortingOrder = FadeSortingOrder - 1;
        unseenEnemyActionIndicator.sortingOrder = FadeSortingOrder - 2;

        oneTimeUseBuff.sortingOrder = FadeSortingOrder - 1;
        buffIncreaseText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder - 1;
    }

    private void SetTargetIcon(ActionClass actionClass)
    {
        targetRenderer.sprite = actionClass.Target.icon;
    }

    public void FlipTransform(Transform transform, bool faceRight)
    {
        if (faceRight) //Face Right
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
        }
        else
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = -Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UI_Toolkit;
using UnityEngine;

public class BattleQueueIcons : DisplayableClass
{
    [SerializeField] SpriteRenderer targetRenderer;
    private SpriteRenderer iconRenderer;


    private int FadeSortingOrder => CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER;
    private string FadeSortingLayer => CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;

    private void Awake()
    {
        iconRenderer = GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
        iconRenderer.sortingOrder = FadeSortingOrder + 4;
        iconRenderer.sortingLayerName = FadeSortingLayer;
        targetRenderer.sortingOrder = FadeSortingOrder + 5;
        targetRenderer.sortingLayerName = FadeSortingLayer;
    }
    public void OnMouseDown()
    {
        if (ActionClass.Origin is PlayerClass && CombatManager.Instance.CanHighlight())
        {
            DeleteFromBQ();
            HideCard();
        }
    }

    private void DeleteFromBQ()
    {
        if (CombatManager.Instance.CanHighlight())
        {
            DeHighlightTarget();
            BattleQueue.BattleQueueInstance.DeletePlayerAction(ActionClass);
        }  
    }

    public void RenderBQIcon(ActionClass ac)
    {
        ActionClass = ac;
        targetRenderer.sprite = ac.Target.icon;
        GetComponent<SpriteRenderer>().sprite = ac.GetIcon();
    }
}

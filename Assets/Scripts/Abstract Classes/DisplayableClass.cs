using System;
using UI_Toolkit;
using UnityEngine;

// This class is a parent class of both BattleQueueIcons and CombatCardUI. This is because both of them
// are "displayable" in the upper right window when clicked.
public abstract class DisplayableClass : SelectClass
{
#nullable enable
    public ActionClass? ActionClass { get; protected set; }   
    protected bool targetHighlighted = false;
    private bool grewLarger;
    
    public static event Action<ActionClass>? OnShowCard;
    public static event Action<ActionClass>? OnHideCard;
    
    public delegate void HoveredHandler(ActionClass actionClass);
    
    public static event HoveredHandler? OnHovered;
    
    public static event Action? OnUnhovered;

    protected void ShowCard()
    {
        if (ActionClass != null)
        {
            OnShowCard?.Invoke(ActionClass);
        }
    }

    protected void HideCard()
    {
        if (ActionClass != null)
        {
            OnHideCard?.Invoke(ActionClass);
        }
    }

    protected void HighlightTarget()
    {
        if (!targetHighlighted)
        {
            ActionClass?.Target?.Highlight();
        }
        targetHighlighted = true;
    }

    protected void DeHighlightTarget()
    {
        if (targetHighlighted)
        {
            ActionClass?.Target?.DeHighlight();
        }
        targetHighlighted = false;
    }

    public virtual void OnMouseEnter()
    {
        if (CombatManager.Instance.CanHighlight() && !grewLarger && !PauseMenuV2.IsPaused) {
            transform.localScale *= 1.25f;
            grewLarger = true;
            HighlightTarget();
            ShowCard();
            if (ActionClass != null) {
                OnHovered?.Invoke(ActionClass);
            }
        }
    }

    public virtual void OnMouseExit()
    {
        if (grewLarger)
        {
            transform.localScale /= 1.25f;
            grewLarger = false;
            DeHighlightTarget();
            HideCard();
            OnUnhovered?.Invoke();
        }
    }
}


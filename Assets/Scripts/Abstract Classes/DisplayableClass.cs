using System;
using UI_Toolkit;
using UnityEngine;

// This class is a parent class of both BattleQueueIcons and CombatCardUI. This is because both of them
// are "displayable" in the upper right window when clicked.
public abstract class DisplayableClass : SelectClass
{
#nullable enable
    public ActionClass? ActionClass { get; protected set; }   
    [SerializeField] protected SpriteRenderer? unseenEnemyActionIndicator;
    protected bool targetHighlighted = false;
    private bool grewLarger;
    
    public static event Action<ActionClass>? OnShowCard;
    public static event Action<ActionClass>? OnHideCard;
    public static event Action<string>? OnEnemyActionSeen; // If there are multiple identical enemy actions, hovering one should remove indicator on all
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
            if (UnseenIndicatorVisible())
            {
                GameStateManager.Instance.AddEnemyActionToSeen(ActionClass);
                OnEnemyActionSeen?.Invoke(ActionClass.GetName());
            }
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
                new DisplayableHoveredEvent(ActionClass).Invoke();
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
            new DisplayableUnhoveredEvent().Invoke();
        }
    }
    
    // Should be invoked if this Displayable is showing an enemy action
    public void RenderUnseenIndicator() {
        if (!ActionClass || GameStateManager.Instance.HasSeenEnemyAction(ActionClass)) return;
        if (unseenEnemyActionIndicator) {
            unseenEnemyActionIndicator.gameObject.SetActive(true);
            OnEnemyActionSeen += OnEnemyActionMarkedScene;
        }
    }

    private void OnEnemyActionMarkedScene(string actionName) {
        if (!ActionClass || ActionClass.GetName() != actionName) return;
        HideUnseenIndicator();
    }

    private bool UnseenIndicatorVisible() {
        return (unseenEnemyActionIndicator && unseenEnemyActionIndicator.gameObject.activeSelf);
    }

    private void HideUnseenIndicator() {
        if (!ActionClass || !UnseenIndicatorVisible()) return;
        unseenEnemyActionIndicator!.gameObject.SetActive(false);
    }
}

public record DisplayableHoveredEvent(ActionClass ActionClass) : IEvent;
public record DisplayableUnhoveredEvent() : IEvent;


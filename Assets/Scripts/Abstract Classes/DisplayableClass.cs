using System;
using System.Collections;
using UI_Toolkit;
using UnityEngine;

// This class is a parent class of both BattleQueueIcons and CombatCardUI. This is because both of them
// are "displayable" in the upper right window when clicked.
public abstract class DisplayableClass : SelectClass
{
#nullable enable
    public ActionClass? ActionClass { get; protected set; }   
    [SerializeField] protected SpriteRenderer unseenEnemyActionIndicator = null!;
    protected EntityClass? currentHighlightedTarget = null;
    private bool grewLarger;
    
    private readonly float scaleEaseDuration = 0.12f;

    private Coroutine? scaleCoroutine;
    private Vector3 baseScale = Vector3.one;
    private Vector3 enlargedScale;
    
    public static event Action<ActionClass>? OnShowCard;
    public static event Action<ActionClass>? OnHideCard;
    public static event Action<string>? OnEnemyActionSeen; // If there are multiple identical enemy actions, hovering one should remove indicator on all
    
    protected virtual void Start()
    {
        baseScale = transform.localScale;
        enlargedScale = baseScale * 1.25f;
    }

    protected void ShowCard()
    {
        if (ActionClass != null)
        {
            OnShowCard?.Invoke(ActionClass);
            if (UnseenIndicatorVisible())
            {
                GameStateManager.Instance.AddEnemyActionToSeen(ActionClass);
                OnEnemyActionSeen?.Invoke(ActionClass.GetName());
            }
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
        if (currentHighlightedTarget != null)
        {
            DeHighlightTarget();
        }
        ActionClass?.Target?.Highlight();
        currentHighlightedTarget = ActionClass?.Target;
    }

    protected void DeHighlightTarget()
    {
        if (currentHighlightedTarget != null)
        {
            currentHighlightedTarget?.DeHighlight();
            currentHighlightedTarget = null;
        }
    }
    
    public virtual void OnMouseEnter()
    {
        if (new CanHighlight().Query() == true && !grewLarger && !PauseMenuV2.IsPaused)
        {
            StartScale(true);

            grewLarger = true;
            HighlightTarget();
            ShowCard();

            if (ActionClass != null)
            {
                new DisplayableHoveredEvent(ActionClass).Invoke();
            }
        }
    }

    public virtual void OnMouseExit()
    {
        if (grewLarger)
        {
            StartScale(false);

            grewLarger = false;
            DeHighlightTarget();
            HideCard();
            new DisplayableUnhoveredEvent().Invoke();
        }
    }
    
    private void StartScale(bool grow)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        Vector3 from = transform.localScale;
        Vector3 to = grow ? enlargedScale : baseScale;

        scaleCoroutine = StartCoroutine(EaseScale(from, to, scaleEaseDuration));
    }
    
    private IEnumerator EaseScale(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        transform.localScale = to;
    }


    protected virtual void OnDestroy() {
        OnEnemyActionSeen -= OnEnemyActionMarkedScene;
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


using System.Collections.Generic;
using UnityEngine;

public class HoverIndicatorManager : MonoBehaviour 
{
    [Header("Hover Indicator Assets")]
    [SerializeField] private GameObject hoverIndicatorObj;
    [SerializeField] private GameObject unopposedAttack;
    [SerializeField] private GameObject clashAttack;
    [SerializeField] private GameObject doesNotClash;
    [SerializeField] private GameObject redirect;
    [SerializeField] private GameObject tooSlow;
    [SerializeField] private GameObject clashingDefense;
    [SerializeField] private GameObject unopposedDefense;

#nullable enable
    private EntityClass? hoveredEntity = null;
    private ActionClass? hoveredActionIcon = null;
    private List<GameObject> Cursors => new() { unopposedAttack, clashAttack, doesNotClash, redirect, tooSlow, clashingDefense, unopposedDefense };

    private void Start()
    {
        this.Subscribe<EntityHovered>(e => OnEntityHovered(e.Entity));
        this.Subscribe<EntityUnhovered>(e => OnEntityUnhovered(e.Entity));
        this.Subscribe<DisplayableHoveredEvent>(e => OnIconHovered(e.ActionClass));
        this.Subscribe<DisplayableUnhoveredEvent>(e => OnIconUnhovered(e.ActionClass));
        this.Subscribe<PlayerManuallyInsertedAction>(e => UpdateHoverIndicator());
    }

    private void OnEntityHovered(EntityClass entity)
    {
        hoveredEntity = entity;
        UpdateHoverIndicator();
    }
    
    private void OnEntityUnhovered(EntityClass entity)
    {
        if (hoveredEntity == entity) 
        {
            hoveredEntity = null;
            UpdateHoverIndicator();
        }
    }

    private void OnIconHovered(ActionClass icon)
    {
        hoveredActionIcon = icon;
        UpdateHoverIndicator();
    }
    
    private void OnIconUnhovered(ActionClass icon)
    {
        if (hoveredActionIcon == icon)
        {
            hoveredActionIcon = null;
            UpdateHoverIndicator();
        }
    }

    private void Update()
    {        
        if (hoverIndicatorObj != null && hoverIndicatorObj.activeSelf)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            hoverIndicatorObj.transform.position = mousePos;
        }
    }

    private void SetIndicatorActive(bool active)
    {
        if (hoverIndicatorObj != null) 
        {
            hoverIndicatorObj.SetActive(active);
        }
        Cursor.visible = !active;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    private void UpdateHoverIndicator()
    {
        ActionClass? currentHighlightedAction = new GetCurrentHighlightedAction().Query();
        if (currentHighlightedAction == null || (hoveredEntity == null && hoveredActionIcon == null) || hoveredEntity is PlayerClass)
        {
            SetIndicatorActive(false);
            return;
        }

        SetIndicatorActive(true);
        var result = PredictOutcome(currentHighlightedAction, hoveredActionIcon, hoveredEntity);
        SetOn(ClashResultToCursorObject(currentHighlightedAction, result));
    }

    private BattleQueue.ClashResult PredictOutcome(ActionClass incomingAction, ActionClass? existingIcon, EntityClass? entity)
    {
        if (existingIcon != null)
        {
            return BattleQueue.BattleQueueInstance.GetWrapperForAction(existingIcon)?.CheckClashValidity(incomingAction, simulatedTarget: existingIcon.Origin) ?? BattleQueue.ClashResult.Unopposed;
        }
        else if (entity != null)
        {
            return BattleQueue.BattleQueueInstance.FindClashingWrapper(incomingAction, allowRedirect: false, simulatedTarget: entity)?.CheckClashValidity(incomingAction, simulatedTarget: entity) ?? BattleQueue.ClashResult.Unopposed;
        }
        return BattleQueue.ClashResult.Unopposed; 
    }

    private void ResetCursors() => Cursors.ForEach(c => c.SetActive(false));
    private void SetOn(CursorObject cursor)
    {
        ResetCursors();
        GameObject turnOn = cursor switch
        {
            CursorObject.UnopposedAttack => unopposedAttack,
            CursorObject.ClashingAttack => clashAttack,
            CursorObject.DoesNotClash => doesNotClash,
            CursorObject.Redirect => redirect,
            CursorObject.TooSlow => tooSlow,
            CursorObject.ClashingDefense => clashingDefense,
            CursorObject.UnopposedDefense => unopposedDefense,
            _ => doesNotClash,
        };

        if (cursor is CursorObject.DoesNotClash dnc && doesNotClash != null)
        {
            var textComp = doesNotClash.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComp != null) textComp.text = dnc.Message;
        }

        if (turnOn != null) turnOn.SetActive(true);
    }

    private abstract record CursorObject
    {
        public record UnopposedAttack : CursorObject;
        public record ClashingAttack : CursorObject;
        public record Redirect : CursorObject;
        public record TooSlow : CursorObject;
        public record ClashingDefense : CursorObject;
        public record UnopposedDefense : CursorObject;
        public record DoesNotClash(string Message) : CursorObject;
    }

    private CursorObject ClashResultToCursorObject(ActionClass currentHighlightedAction, BattleQueue.ClashResult result) => result switch
    {
        BattleQueue.ClashResult.ValidRedirect => new CursorObject.Redirect(),
        BattleQueue.ClashResult.SpeedTooSlow => new CursorObject.TooSlow(),
        BattleQueue.ClashResult.Unopposed => currentHighlightedAction.CardType == CardType.Defense ? new CursorObject.UnopposedDefense() : new CursorObject.UnopposedAttack(),
        BattleQueue.ClashResult.ValidClash => currentHighlightedAction.CardType == CardType.Defense ? new CursorObject.ClashingDefense() : new CursorObject.ClashingAttack(),
        BattleQueue.ClashResult.SameTeam => new CursorObject.DoesNotClash("Same team!"),
        BattleQueue.ClashResult.TargetUnclashable => new CursorObject.DoesNotClash("Unclashable!"),
        BattleQueue.ClashResult.SourceUnclashable => new CursorObject.DoesNotClash("Cannot clash!"),
        BattleQueue.ClashResult.TargetMismatch => new CursorObject.DoesNotClash("Target mismatch!"),
        BattleQueue.ClashResult.TargetAlreadyClashing => new CursorObject.DoesNotClash("Already clashing!"),
        _ => new CursorObject.DoesNotClash("Invalid!"),
    };
}






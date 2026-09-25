using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoverIndicatorManager : MonoBehaviour 
{
    [Header("Hover Indicator Assets")]
    [SerializeField] private GameObject hoverIndicatorObj;
    [SerializeField] private GameObject unopposedAttack;
    [SerializeField] private GameObject clashAttack;
    [SerializeField] private Animator clashAnimator;
    [SerializeField] private Animator clashingDefenseAnimator;
    [SerializeField] private GameObject doesNotClash;
    [SerializeField] private TextMeshPro doesNotClashText;
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
        var clashResult = PredictOutcome(currentHighlightedAction, hoveredActionIcon, hoveredEntity);
        SetOn(ClashResultToCursorObject(currentHighlightedAction, clashResult));
    }

    private ClashResult PredictOutcome(ActionClass incomingAction, ActionClass? existingIcon, EntityClass? entity)
    {
        if (existingIcon != null)
        {
            return BattleQueue.BattleQueueInstance.GetWrapperForAction(existingIcon)?.CheckClashValidity(incomingAction, simulatedTarget: existingIcon.Origin) ?? new ClashResult.Unopposed();
        }
        else if (entity != null)
        {
            return BattleQueue.BattleQueueInstance.FindClashingWrapper(incomingAction, allowRedirect: false, simulatedTarget: entity)?.CheckClashValidity(incomingAction, simulatedTarget: entity) ?? new ClashResult.Unopposed();
        }
        return new ClashResult.Unopposed(); 
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

        if (turnOn != null) turnOn.SetActive(true);

        if (cursor is CursorObject.DoesNotClash dnc) 
            doesNotClashText.text = dnc.Message;
        if (cursor is CursorObject.ClashingAttack attack) 
            clashAnimator.SetInteger(SwordIcon.ClashStateHash, (int)ClashCalculator.CompareRange(attack.Left, attack.Right));
        if (cursor is CursorObject.ClashingDefense defense)
            clashingDefenseAnimator.SetInteger(SwordIcon.ClashStateHash, (int)ClashCalculator.CompareRange(defense.Left, defense.Right));
    }

    private abstract record CursorObject
    {
        public record UnopposedAttack : CursorObject;
        public record ClashingDefense(ActionClass Left, ActionClass Right) : CursorObject;
        public record ClashingAttack(ActionClass Left, ActionClass Right) : CursorObject;
        public record Redirect(ActionClass Left, ActionClass Right) : CursorObject;
        public record TooSlow : CursorObject;
        public record UnopposedDefense : CursorObject;
        public record DoesNotClash(string Message) : CursorObject;
    }

    private CursorObject ClashResultToCursorObject(ActionClass currentHighlightedAction, ClashResult result) => result switch
    {
        ClashResult.ValidRedirect vr => new CursorObject.Redirect(vr.Left, vr.Right),
        ClashResult.Unopposed => currentHighlightedAction.CardType == CardType.Defense ? new CursorObject.UnopposedDefense() : new CursorObject.UnopposedAttack(),
        ClashResult.ValidClash vc => currentHighlightedAction.CardType == CardType.Defense ? new CursorObject.ClashingDefense(vc.Left, vc.Right) : new CursorObject.ClashingAttack(vc.Left, vc.Right),
        ClashResult.SpeedTooSlow => new CursorObject.TooSlow(),
        ClashResult.SameTeam => new CursorObject.DoesNotClash("Same team!"),
        ClashResult.TargetUnclashable => new CursorObject.DoesNotClash("Unclashable!"),
        ClashResult.SourceUnclashable => new CursorObject.DoesNotClash("Cannot clash!"),
        ClashResult.TargetMismatch => new CursorObject.DoesNotClash("Target mismatch!"),
        ClashResult.TargetAlreadyClashing => new CursorObject.DoesNotClash("Already clashing!"),
        _ => new CursorObject.DoesNotClash("Invalid!"),
    };
}









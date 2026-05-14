using System;
using System.Collections.Generic;
using Steamworks;
using Systems.Persistence;
using UI_Toolkit;
using UnityEngine;
using UtilClass;

public record CardUsed<T>() : IEvent where T : ActionClass;
public abstract class ActionClass : SelectClass, IBind<ActionData>, ITooltipable
{
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    private EntityClass target;

    public EntityClass Target
    {
        get { return target; }
        set
        {
            TargetChanging?.Invoke(this);
            target = value;
            TargetChanged?.Invoke(this);
        }
    }

    private EntityClass origin;

    public EntityClass Origin
    {
        get { return origin; }
        set
        {
            if (origin != null)
            {
                origin.BuffsUpdatedEvent -= UpdateBuffValue;
            }

            origin = value;
            if (origin != null)
            {
                origin.BuffsUpdatedEvent += UpdateBuffValue;
            }

            UpdateDup();
        }
    }

    public int CostToAddToDeck { get; set; } = 2;

    protected int lowerBound;
    protected int upperBound;
    
    public bool Clashable { get; protected set; } = true;

    protected RolledStats rolledCardStats = new (0, 0);

    public RolledStats GetRolledStats()
    {
        return rolledCardStats;
    }

    public enum CardState
    {
        NORMAL,
        HOVER,
        CANT_PLAY,
        CLICKED_STATE,
    }

    public CardState cardState { get; private set; } = CardState.NORMAL;
    public CardType CardType { get; protected set; }


    public int Speed { get; set; }
    public string description;
    public string Tooltip => description;
    
    public IReadOnlyList<ITooltipable> Children { get; protected set; } = new List<ITooltipable>();

    public string evolutionDescription { get; protected set; }
    public string evolutionCriteria{ get; protected set; }

    public bool IsEvolved = false; // Stores whether the user has decided to select the evolved version of the card or not
    public bool IsFlipped { get; set; } = false; // Should we display the card as flipped?
    [SerializeField]
    private Sprite icon;
    public Sprite cardBack;
    public Sprite evolvedCardBack;
    public CardUI cardUI;

#nullable enable
    [SerializeField] protected ActionData? data;
    protected int CurrentEvolutionProgress
    {
        get { return data?.CurrentProgress ?? 0; }
        set { if (data != null) data.CurrentProgress = Math.Min(value, MaxEvolutionProgress); }
    }
    protected int MaxEvolutionProgress { get; set; }

    public delegate void ActionClassDelegate(ActionClass target);
    public event ActionClassDelegate? TargetChanged;
    public event ActionClassDelegate? TargetChanging;
    public event ActionClassDelegate? CardValuesUpdating;
    public static event ActionClassDelegate? CardClickedEvent;
    public static event ActionClassDelegate? CardRightClickedEvent;
    public static event ActionClassDelegate? CardHighlightedEvent;
    public static event ActionClassDelegate? CardUnhighlightedEvent;
    public delegate void CardStateDelegate(CardState previousState, CardState currentState);
    public static event CardStateDelegate? CardStateChange;

    public event Action<CardState>? OnCardStateChanged;

    public bool IsSelectedForDeck => cardUI.isSelectedForDeck;
    public event Action<bool>? OnIsSelectedForDeckChanged;

    public virtual void OnQueue() { }
    public virtual void OnRetrieveFromQueue() { }
    public virtual void OnCardStagger() { }
    public virtual void CardIsUnstaggered() { }
    public virtual void ClashWon() => Origin.combatInfo.setDiceColor(Color.green);
    public virtual void ClashTied() => Origin.combatInfo.setDiceColor(Color.white);
    public virtual void ClashLost() => Origin.combatInfo.setDiceColor(Color.red);
    

    public virtual void Awake()
    {
        Initialize();
    }

    public virtual void Start() { }

    private void OnEnable()
    {
        PauseMenuV2.DidPause += OnMouseExit;
    }

    private void OnDisable()
    {
        if (origin != null)
        {
            origin.BuffsUpdatedEvent -= UpdateBuffValue;
        }
        PauseMenuV2.DidPause -= OnMouseExit;
    }

    private void UpdateBuffValue(EntityClass origin)
    {
        UpdateDup();
    }

    //Called when this card hits the enemy, runs any on hit buffs or effects given.
    //Note: that OnHit implies CardIsUnstaggered, thus it calls it. Please be **very careful** about the timing that CardIsUnstaggered is called. 
    // This also implies that OnHit is highly unlikely to be overriden in ANY derived class: tge emphasis should almost entirely be on CardIsUnstaggered
    // (excepting few cases like StackSmash that has a unique Animation)
    public virtual void OnHit()
    {
        Vector3 diffInLocation = Target.myTransform.position - Origin.myTransform.position;
        Origin.UpdateFacing(diffInLocation, null);
        CardIsUnstaggered();
        this.Target.TakeDamage(Origin, rolledCardStats.ActualRoll);

        if (IsPlayedByPlayer()) {
            new OnPlayerHit(rolledCardStats.ActualRoll).Invoke();
        }
    }

    //Only called in a clash
    public virtual void OnDefendClash(ActionClass opposingCard)
    {
        opposingCard.ReduceRoll(GetRolledStats().ActualRoll);
    }

    public virtual bool IsPlayableByPlayer(out PopupType popupType)
    {
        popupType = BattleQueue.BattleQueueInstance.ValidatePlayerInsertion(this);
        return popupType is PopupType.None;
    }

    public bool IsPlayedByPlayer()
    {
        return Origin.Team == EntityTeam.PlayerTeam;
    }

    public virtual void Initialize()
    {
        UpdateDup();
    }

    public int getRolledDamage()
    {
        return rolledCardStats.ActualRoll;
    }

    // Initializes a CardDup struct with the given stats of the Card to 
    // modify further based on buffs
    private void DupInit()
    {
        RolledStats oldDup = rolledCardStats;
        rolledCardStats = new RolledStats(lowerBound, upperBound);
        rolledCardStats.ActualRoll = oldDup.ActualRoll;
    }

    public void ReduceRoll(int byValue)
    {
        rolledCardStats.ActualRoll = Mathf.Clamp(rolledCardStats.ActualRoll - byValue, 0, rolledCardStats.ActualRoll);
    }

    public void IncrementRoll(int byValue)
    {
        rolledCardStats.ActualRoll += byValue;
    }

    public void UpdateDup()
    {
        DupInit();
        Origin?.ApplyAllBuffsToCard(rolledCardStats);
        UpdateText();
    }

    public virtual void ApplyEffect()
    {
        UpdateDup();
        Origin?.ApplySingleUseEffects(rolledCardStats);
        UpdateText();
    }

    // Calculates Actual Damage/Block After Applying Buffs
    public virtual void RollDice()
    {
        rolledCardStats.ActualRoll = UnityEngine.Random.Range(rolledCardStats.RollFloor, rolledCardStats.RollCeiling + 1);

        Origin.SetDice(rolledCardStats.ActualRoll);
    }

    public Sprite? GetIcon()
    {
        if (icon)
        {
            return icon;
        }
        else
        {
            Debug.LogWarning("ActionClass icon is Missing for " + name);
            return null;
        }
    }

    private void UpdateText()
    {
        cardUI?.RenderCard(this);
        CardValuesUpdating?.Invoke(this);
    }
    public void OnMouseDown()
    {
        CardClickedEvent?.Invoke(this);
    }

    // To handle right click detection for the Deck Selection scene
    public void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CardRightClickedEvent?.Invoke(this);
        }
    }

    // Can we evolve this card?
    public bool CanEvolve()
    {
        return CurrentEvolutionProgress >= MaxEvolutionProgress;
    }

    public void ToggleSelected()
    {
        SetCardState(CardState.CLICKED_STATE);
    }

    public void ToggleUnSelected()
    {
        SetCardState(CardState.HOVER);
    }

    public void OnMouseEnter()
    {
        if (PauseMenuV2.IsPaused) return;
        if (cardState == CardState.NORMAL)
        {
            SetCardState(CardState.HOVER);
        }

        CardHighlightedEvent?.Invoke(this);
    }

    public void OnMouseExit()
    {
        if (PauseMenuV2.IsPaused) return;
        if (cardState == CardState.HOVER)
        {
            SetCardState(CardState.NORMAL);
        }

        CardUnhighlightedEvent?.Invoke(this);
    }

    public void SetCanPlay(bool canPlay)
    {
        if (canPlay)
        {
            SetCardState(CardState.NORMAL);
        }
        else
        {
            SetCardState(CardState.CANT_PLAY);
        }
    }

    public void ForceNormalState()
    {
        if (cardState == CardState.CLICKED_STATE)
        {
            SetCardState(CardState.HOVER);
            SetCardState(CardState.NORMAL);
        }
    }
    public void SetCardState(CardState nextState)
    {
        CardState previousState = cardState;

        //Transition Diagram
        // CANT_PLAY <-> NORMAL <-> HOVER <-> CLICKED_STATE

        //Bounces all the bad state transitions
        if ((previousState == CardState.CANT_PLAY && nextState != CardState.NORMAL) ||
            (previousState == CardState.NORMAL && (nextState != CardState.CANT_PLAY && nextState != CardState.HOVER)) ||
            (previousState == CardState.HOVER && (nextState != CardState.NORMAL && nextState != CardState.CLICKED_STATE)) ||
            (previousState == CardState.CLICKED_STATE && (nextState != CardState.HOVER)))
        {
            return;
        }

        // Determine the color and position changes based on the current and new states
        Color newColor = Color.white;
        Vector3 positionChange = Vector3.zero;

        switch (nextState)
        {
            case CardState.NORMAL:
                newColor = Color.white;
                if (previousState == CardState.HOVER)
                {
                    positionChange = new Vector3(0f, -0.4f, 0);
                    ExtendBoxCollider(gameObject.GetComponent<BoxCollider>(), -1.6f);
                }
                break;
            case CardState.HOVER:
                newColor = new Color(0.6f, 0.6f, 0.6f, 1);
                if (previousState == CardState.NORMAL)
                {
                    positionChange = new Vector3(0f, 0.4f, 0);
                    ExtendBoxCollider(gameObject.GetComponent<BoxCollider>(), 1.6f);
                }
                break;
            case CardState.CANT_PLAY:
                newColor = new Color(1f, 200f / 255f, 200f / 255f, 1);
                if (cardState == CardState.HOVER)
                {
                    positionChange = new Vector3(0f, -0.4f, 0); // This state can't happen?
                }
                break;
            case CardState.CLICKED_STATE:
                newColor = new Color(0.5f, 0.5f, 0.5f, 1);
                break;

        }

        // Apply the new state
        cardState = nextState;

        // Apply the color and position changes
        GetComponent<SpriteRenderer>().color = newColor;
        transform.position += positionChange;

        // Apply the same color change to child SpriteRenderers
        SpriteRenderer[] childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in childSpriteRenderers)
        {
            spriteRenderer.color = newColor;
        }
        CardStateChange?.Invoke(previousState, nextState);
        OnCardStateChanged?.Invoke(cardState);
    }

    //extends the boxcollider downwards
    void ExtendBoxCollider(BoxCollider boxCollider, float extendAmount)
    {
        // Store the original size and center
        Vector3 originalSize = boxCollider.size;
        Vector3 originalCenter = boxCollider.center;

        // Extend the size of the BoxCollider along the y-axis
        boxCollider.size = new Vector3(originalSize.x, originalSize.y + extendAmount, originalSize.z);

        // Adjust the center of the BoxCollider to keep the bottom end at the same place
        boxCollider.center = new Vector3(originalCenter.x, originalCenter.y - extendAmount / 2, originalCenter.z);
    }

    //Will activate the checkmark on card UI for indication that it is in the player's deck
    public void SetSelectedForDeck(bool isSelectedForDeck)
    {
        cardUI.SetSelectedForDeck(isSelectedForDeck);
        OnIsSelectedForDeckChanged?.Invoke(isSelectedForDeck);
    }

    public void SetRenderCost(bool renderCost)
    {
        cardUI.shouldRenderCost = renderCost;
    }

    public void Bind(ActionData data)
    {
        this.data = data;
        this.data.Id = Id;
    }

    // Returns a description based on whether the card is flipped and evolved or not
    public string GenerateCardDescription()
    {
        if (description != null && !IsFlipped)
        {
            return description;
        }
        else if (evolutionDescription != null && IsFlipped)
        {
            string tempDescription = evolutionDescription;
            if (!CanEvolve())
                tempDescription = "LOCKED: " + evolutionDescription;
            return tempDescription;
        }
        return "";
    }


    // class to Apply Buffs to so as to avoid modifying the cards
    public class RolledStats
    {
        private readonly int baseRollFloor;
        private readonly int baseRollCeiling;
        public int ActualRoll { get; set; } = 0;
        public int FloorBuffs { get; set; } = 0;
        public int CeilingBuffs { get; set; } = 0;
        public int RollFloor => Math.Clamp(value: baseRollFloor + FloorBuffs, min: 0 , max: RollCeiling);
        public int RollCeiling => baseRollCeiling + CeilingBuffs;

        //We want to render these one time buffs so we keep track of its name, lower and upper bound buffs to this card.
        public (StatusEffect?, int floorBuff, int ceilingBuff) OneTimeBuffs { get; set; } = (null, 0, 0); 
        public RolledStats(int rollFloor, int rollCeiling)
        {
            this.baseRollFloor = rollFloor;
            this.baseRollCeiling = rollCeiling;
        }
    }

}

[Serializable]
public class ActionData : ISaveable
{
    public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    [field: SerializeField] public string ActionClassName { get; set; } = "";
    [field: SerializeField] public int CurrentProgress { get; set; } = 0;
}

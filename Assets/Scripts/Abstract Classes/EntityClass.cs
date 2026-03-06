using System;
using System.Collections;
using System.Collections.Generic;
using UI_Toolkit;
using Unity.VisualScripting;
using UnityEngine;
using static CardComparator;
using static StatusEffect;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public record OnBuffsUpdatedEvent(EntityClass WhoAmI) : IEvent;
public abstract class EntityClass : SelectClass
{
    public const string STAGGERED_ANIMATION_NAME = "IsStaggered";
    public const string BLOCK_ANIMATION_NAME = "IsBlocking";
    protected int MAX_HEALTH;
    protected int MaxHealth
    {
        get => MAX_HEALTH;
        set
        {
            MAX_HEALTH = value;
            combatInfo.SetMaxHealth(MAX_HEALTH);
        }
    }

    private int health;
    public int Health
    {
        get => health;
        protected set
        {
            health = value;
            combatInfo.SetHealth(health);
        }
    }

    public EntityTeam Team { get; set; } = EntityTeam.NoTeam;
    public bool IsDead { get; set; }
    protected Vector3 initialPosition;
    private bool crosshairStaysActive = false;
    public DeadEntities DeathHandler { get; set; }
    protected readonly Dictionary<string, StatusEffect> statusEffects = new();

    // Set in the editor
    public Sprite icon;
    public Animator animator;
    public CombatInfo combatInfo;
    [SerializeField] 
    protected BoxCollider boxCollider;

#nullable enable
    private Vector3 damageSourceDirection = default;

    #if UNITY_EDITOR
    // Used to support handling arbitrary foreign animations 
    [field: SerializeField]
    public AnimatorController? AnimatorController { get; private set; }
    #endif
    public delegate void DamageDelegate(int damage);
    public event DamageDelegate? EntityTookDamage;

    public delegate void EntityDelegate(EntityClass player);
    public static event EntityDelegate? OnEntitySpawn;
    public static event EntityDelegate? OnEntityDeath;
    public static event EntityDelegate? OnEntityClicked;
    public event EntityDelegate? BuffsUpdatedEvent;

    private string FadeSortingLayer => CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;
    private int FadeSortingOrder => CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER;

    private EntityMovementHandler entityMovement = null!;

    public virtual void Start()
    {
        entityMovement = this.gameObject.AddComponent<EntityMovementHandler>();
        initialPosition = myTransform.position;

        DeEmphasize();
        DisableDice();
        AssignTeam();
        GetComponent<SpriteRenderer>().sortingLayerName = FadeSortingLayer;
        DeathHandler = Die;

        OnEntitySpawn?.Invoke(this);
    }

    protected virtual void OnEnable()
    {
        CombatManager.OnGameStateChanging += UpdateBuffsNewRound;
    }

    protected virtual void OnDisable()
    {
        CombatManager.OnGameStateChanging -= UpdateBuffsNewRound;
    }

    /*
     Purpose: Deals damage to this entity and staggers it back 
     Requires: This Entity is not dead
     */

    public virtual void TakeDamage(EntityClass source, int damage)
    {
        BuffsOnDamageEvent(ref damage);

        Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
        float percentageDone = 1; //Testing different powered knockbacks
        if (Health != 0)
        {
            percentageDone = Mathf.Clamp(damage / (float)Health, 0f, 1f);
        } 
        else
        {
            IsDead = true;
            RemoveEntityFromCombat();
            OnEntityDeath?.Invoke(this);
        }

        EntityTookDamage?.Invoke(damage);
        combatInfo.DisplayDamage(damage);
        if (damage > 0)
        {
            StartCoroutine(PlayHitAnimation(source, this, percentageDone));
        }
    }

    //Plays both first the stagger entities then 
    //Requires: Entities are not dead
    private IEnumerator PlayHitAnimation(EntityClass origin, EntityClass target, float percentageDone)
    {
        new ShakeScreen(Intensity: percentageDone).Invoke();
        yield return StartCoroutine(StaggerEntities(origin, target, percentageDone));
    }

    /* 
    Purpose: Staggers an entity back 
    origin: The origin of the damage/attack is coming from
    target: The target being staggered back
    percentageDone: Percentage health done to the target
    Requires: Entities are not dead
     */
    public IEnumerator StaggerEntities(EntityClass origin, EntityClass target, float percentageDone)
    {
        damageSourceDirection = (target.myTransform.position - origin.myTransform.position).normalized;
        float staggerPower = StaggerPowerCalculation(percentageDone);
        yield return StartCoroutine(target.StaggerBack(target.myTransform.position + damageSourceDirection * staggerPower));
    }

    //Calculates the power of the stagger based on the percentage health done
    protected virtual float StaggerPowerCalculation(float percentageDone)
    {
        float minimumPush = 0.8f;
        float pushSlope = 1.8f;
        float percentageUntilMaxPush = 1f / 3f; //Reaches Max push at 33% hp lost
        return minimumPush + pushSlope * Mathf.Clamp(percentageDone / percentageUntilMaxPush, 0f, 1.5f);
    }

    /*
    float radius: Radius is the radius right before the destination the entity will stop at.
    Requires: Entity is not dead
     */
    public virtual IEnumerator MoveToPosition(Vector3 destination, float radius, float duration, Vector3? lookAtPosition = null)
    {
        yield return StartCoroutine(entityMovement.MoveToPosition(destination, radius, duration, lookAtPosition));
    }

    //Removes entity cards and self from BQ and combat manager. Kills itself
    public virtual IEnumerator Die()
    {
        float runDirection = (Team == EntityTeam.PlayerTeam) ? -1f : 1f;
        
        if (damageSourceDirection != default)
            runDirection = Mathf.Sign(damageSourceDirection.x);

        float runDistance = 10f * runDirection;

        BattleQueue.BattleQueueInstance.RemoveAllInstancesOfEntity(this);
        DestroyDeck();

        yield return StartCoroutine(MoveToPosition(myTransform.position + new Vector3(runDistance, 0, 0), 0, 0.8f));
        this.gameObject.SetActive(false);
    }

    public void FaceRight()
    {
        FlipTransform(this.transform, true);
        combatInfo.FaceRight();
    }

    public void FaceLeft()
    {
        FlipTransform(this.transform, false);
        combatInfo.FaceLeft();
    }

    public bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }
    public void FlipTransform(Transform transform, bool faceRight)
    {
        if (faceRight) //Face Right
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
            if (boxCollider)
            {
                Vector3 currentScale = boxCollider.size;
                boxCollider.size = new Vector3(Mathf.Abs(currentScale.x), Mathf.Abs(currentScale.y), Mathf.Abs(currentScale.z));
            }
        }
        else
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = -Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
            if (boxCollider)
            {
                Vector3 currentScale = boxCollider.size;
                boxCollider.size = new Vector3(-Mathf.Abs(currentScale.x), Mathf.Abs(currentScale.y), Mathf.Abs(currentScale.z));
            }
        }
    }

    /*
     * Purpose: Updates the entitiy's direction to face a target only when called. (Target.position - my position)
     * diffInLocation: Will face the entity Right if the Target is to its right (positive diffInLocation)
        Left if other way around  
        Note: If you want to reverse the results, add a negative to diffInLocation before calling.
    lookAtPosition: null if you want to use default movement facing, provide a value if you want the player to face a certain direction when called
     */
    public void UpdateFacing(Vector3 diffInLocation, Vector3? lookAtPosition)
    {
        if (lookAtPosition != null)
        {
            diffInLocation = lookAtPosition.Value - myTransform.position;
        }
        if (diffInLocation.x > 0)
        {
            FaceRight();
        }
        else if (diffInLocation.x < 0)
        {
            FaceLeft();
        }
    }


    /* Requires: "IsStaggered" bool exists on the animator controller attatched to this
     * 
     * Purpose: Plays when the enemy is staggered
     * Pushes the enemy back to staggeredPosition
     * Plays the stagger animation
     * 
     * Params:
     * staggeredPosition is the location we want the enemy to stop at. 
     * 
     * Modifies: this.myTransform.position
     * Requires: Entity is not dead
     */
    public virtual IEnumerator StaggerBack(Vector3 staggeredPosition)
    {
        yield return StartCoroutine(entityMovement.StaggerBack(staggeredPosition));
    }

    public virtual void Heal(int val)
    {
        Health = Mathf.Clamp(Health + val, 0, MaxHealth);
    }

    public void SetUnstaggered()
    {
        if (HasAnimationParameter(STAGGERED_ANIMATION_NAME))
        {
            animator.SetBool(STAGGERED_ANIMATION_NAME, false);
        }
    }

    protected virtual void OnMouseEnter()
    {
        if (PauseMenuV2.IsPaused) return;
        Highlight();
    }

    private void OnMouseExit()
    {
        if (PauseMenuV2.IsPaused) return;
        DeHighlight();
    }

    public void CrossHair(float speed = 0.55f)
    {
        crosshairStaysActive = true;
        Highlight(speed);
    }

    public void UnCrossHair()
    {
        crosshairStaysActive = false;
        DeHighlight();
    }

    public void Highlight(float speed = 0.55f)
    {
        if (CombatManager.Instance.CanHighlight())
        {
            combatInfo.ActivateCrosshair(speed);
        }
    }

    public void DeHighlight()
    {
        if (!crosshairStaysActive)
        {
            combatInfo.DeactivateCrosshair();
        }
    }

    public void OnMouseDown()
    {
        if (PauseMenuV2.IsPaused) return;
        OnEntityClicked?.Invoke(this);
    }

    //Run this to reset the entity position back to its starting position
    public abstract IEnumerator ResetPosition();
    public abstract void DestroyDeck();
    public abstract void PerformSelection();

    private void AssignTeam()
    {
        Action<EntityClass> action = Team switch
        {
            EntityTeam.PlayerTeam => CombatManager.Instance.AddPlayer,
            EntityTeam.EnemyTeam=> CombatManager.Instance.AddEnemy,
            EntityTeam.NeutralTeam => CombatManager.Instance.AddNeutral,
            _ => throw new ArgumentOutOfRangeException()
        };

        action(this);
    }
    public void RemoveEntityFromCombat()
    {
        Action<EntityClass> action = Team switch
        {
            EntityTeam.PlayerTeam => CombatManager.Instance.RemovePlayer,
            EntityTeam.EnemyTeam => CombatManager.Instance.RemoveEnemy,
            EntityTeam.NeutralTeam => CombatManager.Instance.RemoveNeutral,
            _ => throw new ArgumentOutOfRangeException()
        };

        action(this);
    }

    protected void FaceOpponent()
    {
        Action faceAction = Team switch
        {
            EntityTeam.PlayerTeam => FaceRight,
            EntityTeam.EnemyTeam => FaceLeft,
            EntityTeam.NeutralTeam => FaceLeft,
            _ => throw new ArgumentOutOfRangeException()
        };

        faceAction();
    }


    private void CheckBuff(string buffType)
    {
        if (!statusEffects.ContainsKey(buffType))
        {
            statusEffects[buffType] = BuffFactory.GetStatusEffect(buffType);
        }
    }

    // Adds the Stacks of the Card to the Relevant Buff Stacks of the Player    
    public virtual void AddStacks(string buffType, int stacks)
    {
        CheckBuff(buffType);
        statusEffects[buffType].GainStacks(stacks);
        UpdateBuffs();
    }

    public void ReduceStacks(string buffType, int stacks)
    {
        if (statusEffects.ContainsKey(buffType))
        {
            statusEffects[buffType].LoseStacks(stacks);
            UpdateBuffs();
        }
    }

    // Applies the Stacks of the Specified Buff to the Card Roll Limits
    private void ApplyBuffsToCard(ActionClass.RolledStats dup, string buffType)
    {
        CheckBuff(buffType);
        statusEffects[buffType].ApplyStacks(dup);
    }

    // Applies the Stacks of all Buffs to the Card Roll Limits
    public void ApplyAllBuffsToCard(ActionClass.RolledStats dup)
    {
        foreach (string buff in  statusEffects.Keys)
        {
            ApplyBuffsToCard(dup, buff);
        }
    }

    public void ApplySingleUseEffects(ActionClass.RolledStats duplicateCard)
    {
        foreach (string buff in statusEffects.Keys)
        {
            statusEffects[buff].ApplySingleUseEffects(duplicateCard);
        }
        //Single application will not be rendered in the hand but at runtime, so do not call the BuffUpdatedEvent 
        combatInfo.UpdateBuffs(statusEffects); 
    }

    public int GetBuffStacks(string s)
    {
        return statusEffects.TryGetValue(s, out var effect) ? effect.Stacks : 0;
    }

    // Updates buffs affected by player taking damage
    protected void BuffsOnDamageEvent(ref int damage)
    {
        foreach (string s in statusEffects.Keys)
        {
            statusEffects[s].OnEntityHitHandler(ref damage);
        }
        UpdateBuffs();
    }

    // Updates buffs that change when a new round begins
    private void UpdateBuffsNewRound(GameState newState)
    {
        if (newState == GameState.SELECTION)
        {
            foreach (string s in statusEffects.Keys)
            {
                statusEffects[s].NewRound();
            }
            UpdateBuffs();
        }
    }

    public virtual void AttackAnimation(string animationName)
    {
        if (HasAnimationParameter(animationName))
        {
            animator.SetTrigger(animationName);
        }
    }

    public void SetAnimationBool(string paramName, bool state, Animator? paramAnimator = null)
    {
        if (HasAnimationParameter(paramName, paramAnimator))
        {
            animator.SetBool(paramName, state);
        }
    }
    public bool HasAnimationParameter(string paramName, Animator? paramAnimator = null)
    {
        if (!paramAnimator)
        {
            paramAnimator = animator;
        }
        foreach (AnimatorControllerParameter param in paramAnimator!.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    public void ActivateCombatInfo(ActionClass actionClass)
    {
        combatInfo.ActivateCombatSprite(actionClass);
    }

    public void DeactivateCombatInfo(ActionClass actionClass)
    {
        combatInfo.DeactivateCombatSprite(actionClass);
    }

    //Increases this Entity Class' sorting layer (negative number is higher up)
    public void Emphasize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 largeTransform = transform.position;
        largeTransform.z = FadeSortingOrder - 3 + ZOffset(spriteRenderer.bounds.min.y);
        transform.position = largeTransform;
        spriteRenderer.sortingOrder = FadeSortingOrder + 1;
        combatInfo.Emphasize();
    }

    //Decreases this Entity Class' sorting layer. (Standardizes Sorting Layers for entities)
    public void DeEmphasize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 largeTransform = transform.position;
        largeTransform.z = FadeSortingOrder - 1 + ZOffset(spriteRenderer.bounds.min.y);
        transform.position = largeTransform;
        spriteRenderer.sortingOrder = FadeSortingOrder - 10;
        combatInfo.DeEmphasize();
    }

    public float GetSortingZ()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        return ZOffset(spriteRenderer.bounds.min.y);
    }

    // Workaround to prevent z clipping for entities
    // The furthur 'down' a entity is, the more in the foreground it is. 
    // Thus, apply a small offset that is greater for entites farther down in the scene so they appear in front.
    private float ZOffset(float yPosition)
    {
        // Small delta values may still clip during screen shakes :(
        float delta = 0.1f; 
        return yPosition * delta;
    }

    public void OutOfCombat()
    {
        DisableHealthBar();
        DisableDice();
        statusEffects.Clear();
        UpdateBuffs();
    }

    public void InCombat()
    {
        EnableHealthBar();
    }

    public void UnTargetable()
    {
        boxCollider.enabled = false;
    }

    public void Targetable()
    {
        boxCollider.enabled = true;
    }

    public void SetDice(int value)
    {
        combatInfo.SetDice(value);
    }

    public void UpdateBuffs()
    {
        combatInfo.UpdateBuffs(statusEffects);
        BuffsUpdatedEvent?.Invoke(this);
        new OnBuffsUpdatedEvent(this).Invoke();
    }

    //Please use the originalHandler to resubscribe when you are done :3
    public StatusEffectModifyValueDelegate SetBuffsOnHitHandler(string buff, StatusEffectModifyValueDelegate handler)
    {
        CheckBuff(buff);
        StatusEffectModifyValueDelegate originalHandler = statusEffects[buff].OnEntityHitHandler;
        statusEffects[buff].OnEntityHitHandler = handler;
        Debug.Log("A buff handler is being reassigned, be careful!");
        return originalHandler;
    }

    public void EnableDice()
    {
        combatInfo.EnableDice();
    }
    public void DisableDice()
    {
        combatInfo.DisableDice();
    }
    private void EnableHealthBar()
    {
        combatInfo.EnableHealthBar();
    }

    private void DisableHealthBar()
    {
        combatInfo.DisableHealthBar();
    }

    //Sets the location an entity returns to after fighting ends.
    public void SetReturnPosition(Vector3 newReturningPosition)
    {
        initialPosition = newReturningPosition;
    }
}

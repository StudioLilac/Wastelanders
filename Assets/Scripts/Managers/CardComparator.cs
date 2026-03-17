using System;
using System.Collections;
using System.Collections.Generic;
using UI_Toolkit;
using UnityEngine;


public class CardComparator : MonoBehaviour
{
    public const string CLASH_TIE_SOUND_FX_NAME = "ClashTie";
    public static CardComparator Instance { get; private set; }
#nullable enable
    public static readonly float COMBAT_BUFFER_TIME = 1f;
    public delegate IEnumerator DeadEntities();
    private event DeadEntities? PlayEntityDeaths;

    public delegate IEnumerator ClashersAreReadyToRoll();
    public event ClashersAreReadyToRoll? playersAreRollingDiceEvent;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

    }

    void Start()
    {
        EntityClass.OnEntityDeath += SubscribeEntityDeath;
    }

    private void OnDestroy()
    {
        EntityClass.OnEntityDeath -= SubscribeEntityDeath;
    }


    /*
     * Clashes two cards together handling logic calls and activating the Combat Info
     */
    public IEnumerator ClashCards(BattleQueue.ActionWrapper actionWrapper)
    {
        ActionClass card1 = actionWrapper.PlayerAction!;
        ActionClass card2 = actionWrapper.EnemyAction!;
        CombatManager.Instance.SetCameraCenter(card1.Origin);
        ActivateInfo(card1, card2);
        EnableDice(card1.Origin, card1.Target);
        card1.ApplyEffect();
        card2.ApplyEffect();
        yield return StartCoroutine(ClashBothEntities(card1, card2)); // for animation purposes 
        BattleQueue.BattleQueueInstance.RemoveActionWrapperFromQueue(actionWrapper);

        card1.RollDice();
        card2.RollDice();
        DeactivateInfo(card1, card2);

        int cardOneGreater = ClashCompare(card1, card2);

        if (cardOneGreater < 0)
        {
            card1.ClashLost();
            card2.ClashWon();
        }
        else if (cardOneGreater == 0)
        {
            card1.ClashTied();
            card2.ClashTied();
        }
        else
        {
            card2.ClashLost();
            card1.ClashWon();
        }

        if (IsAttack(card1) && IsAttack(card2))
        {
            //Debug.Log(cardOneStaggered);
            if (cardOneGreater == 0) //Clash ties
            {
                card1.CardIsUnstaggered();
                card2.CardIsUnstaggered();
                AudioManager.Instance.PlaySFX(SoundID.CB_clash_tie);

            } else if (cardOneGreater < 0) //Card2 wins clash
            {
                card2.OnHit();
                card1.OnCardStagger();
            } else if (cardOneGreater > 0) //Card1 wins clash
            {
                card1.OnHit();
                card2.OnCardStagger();
            }
        } else if (card1.CardType == CardType.Defense && IsAttack(card2))
        {
            card1.OnDefendClash(card2); // Card 1 may modify card2
            card2.OnHit();
            if (cardOneGreater >= 0)
                card1.CardIsUnstaggered();
            else
                card1.OnCardStagger();
        } 
        else if (IsAttack(card1) && card2.CardType == CardType.Defense)
        {
            card2.OnDefendClash(card1); // Card 2 may modify card 1
            card1.OnHit();
            if (cardOneGreater <= 0)
                card2.CardIsUnstaggered();
            else
                card2.OnCardStagger();
        } 
        else
        {
            card1.CardIsUnstaggered();
            card2.CardIsUnstaggered();
        }
        
        yield return new WaitForSeconds(COMBAT_BUFFER_TIME);
        if (PlayEntityDeaths != null)
        {
            yield return StartCoroutine(PlayEntityDeaths());
            PlayEntityDeaths = null;
        }
        DeEmphasizeClashers(card1.Origin, card1.Target);
        DisableDice(card1.Origin, card1.Target);
    }

    //Produces a positive value if Card1 is staggered by Card2
    private int ClashCompare(ActionClass card1, ActionClass card2)
    {
        int cardOneGreater;
        

        if (card1.getRolledDamage() > card2.getRolledDamage())
        {;
            cardOneGreater = 1; //Card 1 is greater than card 2
        } else if (card1.getRolledDamage() < card2.getRolledDamage())
        {
            cardOneGreater = -1;
        } else 
        {
            cardOneGreater = 0;
        }
        return cardOneGreater;
    }

    public IEnumerator OneSidedAttack(BattleQueue.ActionWrapper actionWrapper)
    {
        ActionClass actionClass = actionWrapper.GetTheOnlyExistingAction();
        CombatManager.Instance.SetCameraCenter(actionClass.Origin);
        ActivateInfo(actionClass);
        EnableDice(actionClass.Origin);
        actionClass.ApplyEffect();
        yield return StartCoroutine(ClashBothEntities(actionClass, actionClass));
        BattleQueue.BattleQueueInstance.RemoveActionWrapperFromQueue(actionWrapper);
        actionClass.RollDice();
        DeactivateInfo(actionClass);

        //Hit and feel effects
        if (actionClass.CardType == CardType.Defense)
        {
            actionClass.CardIsUnstaggered();
        } else
        {
            actionClass.OnHit();
        }
        actionClass.Origin.combatInfo.setDiceColor(Color.green);
        yield return new WaitForSeconds(COMBAT_BUFFER_TIME);

        //Reset the Scene
        if (PlayEntityDeaths != null)
        {
            yield return StartCoroutine(PlayEntityDeaths());
            PlayEntityDeaths = null;
        }
        DeEmphasizeClashers(actionClass.Origin, actionClass.Target);
        DisableDice(actionClass.Origin);
    }

    /*
 ActionClass card1: Card1 in the clash (usually the player) target should be origin of card2
 ActionClass card2: Card2 in the clash (usually the enemy) target should be origin of card1
    bufferedRadius: The buffer circle in which the entity will stop before that circle. 

 Purpose: The two clashing enemies come together to clash, their positions will ideally be based off their speed
 Then, whoever wins the clash should stagger the opponent backwards. 
  */
    public static readonly float X_BUFFER = 0.8f;
    private IEnumerator ClashBothEntities(ActionClass card1, ActionClass card2)
    {
        bool oneSidedEnemyAttack = card1 == card2 && !card1.IsPlayedByPlayer();
        EntityClass origin = card1.Origin;
        EntityClass target = card1.Target;
        EmphasizeClashers(origin, target);
        CalculateSpeedRatio(card1, card2, out float originRatio, out float targetRatio);
        //The Distance weighting will be calculated based on speeds of the two clashing cards
        Vector3 centeredDistance = (origin.myTransform.position * originRatio + targetRatio * target.myTransform.position);
        float bufferedRadius = 0.25f;
        float duration = 0.6f;
        
        float xBuffer = (card1.CardType == CardType.RangedAttack || card1.CardType == CardType.Defense) &&
                        (card2.CardType == CardType.RangedAttack || card2.CardType == CardType.Defense) ? X_BUFFER * 3 : X_BUFFER; //Calculates how far away clashers should be when striking
        
        Coroutine playerMove = StartCoroutine(origin?.MoveToPosition(HorizontalProjector(centeredDistance, origin.myTransform.position, xBuffer), bufferedRadius, duration, centeredDistance));
        if (!target.IsDead) 
            yield return StartCoroutine(target?.MoveToPosition(HorizontalProjector(centeredDistance, target.myTransform.position, xBuffer), bufferedRadius, duration, centeredDistance));
        yield return playerMove;

        if (playersAreRollingDiceEvent != null)
        {
            yield return StartCoroutine(playersAreRollingDiceEvent.Invoke());
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) && !PauseMenuV2.IsPaused); //Necessary to not immediately roll the dice
        }
        if (!oneSidedEnemyAttack) yield return new WaitUntil(() => Input.GetMouseButtonDown(0) && !PauseMenuV2.IsPaused);
    }


    //The higher the ratio, the "slower" you will feel. i.e. the closer the middle point is to you so you move less
    private void CalculateSpeedRatio(ActionClass card1, ActionClass card2, out float originRatio, out float targetRatio)
    {
        originRatio = 1f - ((float) card1.Speed / (float) (card1.Speed + card2.Speed));
        targetRatio = 1f - originRatio;

        float rangedSpeedReduction = 0.5f;



        if (card1.CardType == CardType.RangedAttack || card1.CardType == CardType.Defense)
        {
            targetRatio *= rangedSpeedReduction;
            originRatio = 1f - targetRatio;
        }
        if (card2.CardType == CardType.RangedAttack || card2.CardType == CardType.Defense)
        {
            originRatio *= rangedSpeedReduction;
        }
        if (card1 == card2)
        {
            originRatio = 0.1f;
        }


        targetRatio = 1f - originRatio;

    }
    /*
     * 
     * Projects A Character's position onto the same x-axis as centeredDistnace but is 'xBuffer' x-distance away from the 'centeredDistance'
     */
    private Vector3 HorizontalProjector(Vector3 centeredDistance, Vector3 currentPosition, float xBuffer)
    {
        Vector3 vectorToCenter = (centeredDistance - currentPosition);
        
        return vectorToCenter.x > 0 ?
            currentPosition + vectorToCenter - new Vector3(xBuffer, 0f, 0f) :
            currentPosition + vectorToCenter + new Vector3(xBuffer, 0f, 0f);
    }
    private void SubscribeEntityDeath(EntityClass entity)
    {
        // Used to ensure most updated death handler from entity is capture, instead of the one used upon subscription
        IEnumerator DeathHandler() => entity.DeathHandler();
        PlayEntityDeaths += DeathHandler;
    }

    private void ActivateInfo(params ActionClass[] cards)
    {
        foreach (ActionClass card in cards)
        {
            card.Origin.ActivateCombatInfo(card);
        }
    }

    private void DeactivateInfo(params ActionClass[] cards)
    {
        foreach (ActionClass card in cards)
        {
            card.Origin.DeactivateCombatInfo(card);
        }
    }
    private void EmphasizeClashers(params EntityClass[] entities)
    {
        foreach (EntityClass entity in entities)
        {
            entity.Emphasize();
        }
    }

    private void DeEmphasizeClashers(params EntityClass[] entities)
    {
        foreach (EntityClass entity in entities)
        {
            entity.DeEmphasize();
        }
    }

    private void EnableDice(params EntityClass[] entities)
    {
        foreach (EntityClass entity in entities)
        {
            entity.EnableDice();
        }
    }

    private void DisableDice(params EntityClass[] entities)
    {
        foreach (EntityClass entity in entities)
        {
            entity.DisableDice();
        }
    }


    private bool IsAttack(ActionClass card)
    {
        return card.CardType == CardType.MeleeAttack || card.CardType == CardType.RangedAttack;
    }
}

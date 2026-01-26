using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowThrough : FistCards
{
#nullable enable
    private FollowThrough? activeDuplicateInstance = null;
    private bool originalCopy = true;
    CombatManager.GameStateChangedHandler? GameStateChangedHandler;

    private EntityClass.DamageDelegate? attackAgainDelegate;
    private void OnDestroy()
    {
        if (GameStateChangedHandler != null)
        {
            CombatManager.OnGameStateChanged -= GameStateChangedHandler;
        }
        EntityClass.OnEntityDeath -= HandleTargetOrOriginDeath;
    }
    public override void Initialize()
    {

        CardType = CardType.MeleeAttack;
        myName = "Follow Through";
        description = "If not staggered, make this attack against the target again everytime they are hit this turn.";
        lowerBound = 1;
        upperBound = 2;
        Speed = 5;
        attackAgainDelegate = MakeAnotherAttack;
        base.Initialize();
    }
    public override void CardIsUnstaggered()
    {

        if (originalCopy)
        {
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<FollowThrough>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
            }
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = Target;
            activeDuplicateInstance.attackAgainDelegate = attackAgainDelegate;

            Target.EntityTookDamage += attackAgainDelegate;
            EntityClass.OnEntityDeath += HandleTargetOrOriginDeath;
            CombatManager.OnGameStateChanged += ResetBuffHandler;
            void ResetBuffHandler(GameState gameState)
            {
                if (gameState != GameState.FIGHTING)
                {
                    CombatManager.OnGameStateChanged -= ResetBuffHandler;
                    Target.EntityTookDamage -= MakeAnotherAttack;
                }
            }

            GameStateChangedHandler = ResetBuffHandler;
        }

        base.CardIsUnstaggered();
    }

    void HandleTargetOrOriginDeath(EntityClass entity)
    {
        if (entity == Target || entity == Origin)
        {
            Target.EntityTookDamage -= attackAgainDelegate;
        }
    }
    void MakeAnotherAttack(int damage)
    {
        if (damage > 0)
        {
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance!);
        }
    }

    public override void OnHit()
    {
        AudioManager.Instance?.PlaySFX(SoundID.CB_fist_hit);
        Vector3 diffInLocation = Target.myTransform.position - Origin.myTransform.position;
        Origin.UpdateFacing(diffInLocation, null);
        if (!originalCopy)
        {
            Target.EntityTookDamage -= attackAgainDelegate;
        }
        // Have to swap the order of cardIsUnstaggered and TakeDamage here to prevent infinity chain.
        this.Target.TakeDamage(Origin, rolledCardStats.ActualRoll);
        CardIsUnstaggered();
        if (!originalCopy)
        {
            Target.EntityTookDamage += attackAgainDelegate;
        }
    }

}

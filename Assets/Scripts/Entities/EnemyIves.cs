using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyIves : EnemyClass
{
    private readonly Dictionary<Type, List<ActionClass>> instantiatedActions = new();
    private readonly List<ActionClass> currentHand = new();
    private bool firstTurn = true;

    public override void Start()
    {
        base.Start();
        MaxHealth = 35;
        Health = MaxHealth;
        myName = "Le Ives";
        AddStacks(Decohering.buffName, 1);
        AddStacks(Resonate.buffName, 3);
        AddStacks(Dissonance.buffName, 3);
        TargetingWeights = (entity => entity.Team == EntityTeam.PlayerTeam ? 30 : 10);
    }

    
    public override void AddAttack(List<EntityClass> targets)
    {
        var opponents = targets.Where(entity => entity.Team == EntityTeam.PlayerTeam).ToList();
        var neutral = targets.Where(entity => entity.Team == EntityTeam.NeutralTeam).ToList();
     
        if (opponents.Count == 0) return;
        
        int currentResonate = GetBuffStacks(Resonate.buffName);
        int currentDissonance = GetBuffStacks(Dissonance.buffName);
        int ivesAdvantage = currentResonate - currentDissonance;
        int hpWindow = Health - currentDissonance;
        EntityClass RandomOpponent() => opponents[UnityEngine.Random.Range(0, opponents.Count)];
        
        if (firstTurn)
        {
            firstTurn = false;
            AttackWith(GetAction<LowBlow>(), RandomOpponent());
            return;
        }

        ActionClass topCard = GetTop();
        currentHand.Add(topCard);

        switch (topCard)
        {
            case Brace:
                AttackWith(topCard, RandomOpponent());
                break;
            case RazorGuard:
                AttackWith(topCard, RandomOpponent());
                break;
            default:
                currentHand.ForEach(c => AttackWith(c, CalculateAttackTarget(targets)));
                break;
        };

        currentHand.Clear();
    }

    private ActionClass GetTop()
    {
        if (pool.Count == 0)
        {
            Reshuffle();
        }
        var action = pool[0];
        pool.RemoveAt(0);
        return action.GetComponent<ActionClass>();
    }

    protected override void Reshuffle() {
        base.Reshuffle();
        var cheapStrike = GetAction<LowBlow>();
        pool.Remove(cheapStrike.gameObject);
        currentHand.ForEach(h => pool.Remove(h.gameObject));
    }
    
    public override void InstantiateDeck()
    {
        instantiatedActions.Clear();

        foreach (GameObject gameObject in availableActions)
        {
            var prefab = gameObject.GetComponent<ActionClass>();
            if (prefab == null) continue;

            Type actionType = prefab.GetType();
            instantiatedActions[actionType] = new List<ActionClass>();

            ActionClass newAction = Instantiate(prefab);
            newAction.Origin = this;
            newAction.transform.position = new Vector3(-10, 10, 10);

            instantiatedActions[actionType].Add(newAction);
            deck.Add(newAction.gameObject);
        }
    }

    private T GetAction<T>(int index = 0) where T : ActionClass
    {
        return GetAction(typeof(T), index) as T;
    }

    private ActionClass GetAction(Type actionType, int index = 0)
    {
        if (instantiatedActions.TryGetValue(actionType, out List<ActionClass> actions))
        {
            if (index >= 0 && index < actions.Count)
            {
                return actions[index];
            }
        }
        return null;
    }


    public override IEnumerator Die()
    {
        DestroyDeck();
        UnTargetable();
        OutOfCombat();

        yield return null;

        if (HasAnimationParameter(STAGGERED_ANIMATION_NAME))
        {
            animator.SetBool(STAGGERED_ANIMATION_NAME, true);
        }
    }
}
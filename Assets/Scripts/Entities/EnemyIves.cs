using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using static Entities.PrincessFrog;

public class EnemyIves : EnemyClass
{
    private readonly Dictionary<Type, List<ActionClass>> instantiatedActions = new();

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

    private bool firstTurn = true;
    public override void AddAttack(List<EntityClass> targets)
    {
        var opponents = targets.Where(entity => entity.Team == EntityTeam.PlayerTeam).ToList();
        var neutral = targets.Where(entity => entity.Team == EntityTeam.NeutralTeam).ToList();
        int currentResonate = GetBuffStacks(Resonate.buffName);
        int currentDissonance = GetBuffStacks(Dissonance.buffName);
        int ivesAdvantage = currentResonate - currentDissonance;
        int hpWindow = Health - currentDissonance;

        if (firstTurn)
        {
            firstTurn = false;
            AttackWith(GetAction<Haymaker>(), opponents.FirstOrDefault());
            return;
        }

        List<ActionClass> choices = instantiatedActions.Keys.Select(c => GetAction(c, i)).ToList();
        List<ActionClass> chosenActions = new();


        chosenActions.ForEach(c => AttackWith(c, CalculateAttackTarget(targets)));
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
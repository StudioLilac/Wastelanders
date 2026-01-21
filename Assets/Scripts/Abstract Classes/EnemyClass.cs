using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EnemyClass : EntityClass
{
    // This is the deck of the enemy, which does not change as they reshuffle/play cards.
    protected List<GameObject> deck = new List<GameObject>();

    // This is the pool, which is initialized to the deck in a random order. Then cards will be taken from here. Cards are taken from index 0.
    protected List<GameObject> pool = new List<GameObject>();
    
    // Initialized in editor
    public List<GameObject> availableActions;

    public delegate int AttackTargetDelegate(EntityClass targets);

    // Default Attack weight for all opponents gives equal chance for all oppoenents to be picked
    public AttackTargetDelegate TargetingWeights{ get; set; } = delegate { return 100; };

    public virtual void Awake()
    {
        if (Team == EntityTeam.NoTeam) Team = EntityTeam.EnemyTeam;
    }
    public override void Start()
    {
        base.Start();
        InstantiateDeck();

        Reshuffle();
    }

    public virtual void InstantiateDeck()
    {
        for (int i = 0; i < availableActions.Count; i++)
        {
            GameObject toAdd = Instantiate(availableActions[i]);
            ActionClass addedClass = toAdd.GetComponent<ActionClass>();
            addedClass.Origin = this;

            deck.Add(toAdd);
        }
    }

    //An attack that pops the top card on the pool and plays it. use this when you attack with pool.
    public virtual void AddAttack(List<EntityClass> targets)
    {
        if (targets.Count == 0 || pool.Count == 0) return;

        AttackWith(pool[0], CalculateAttackTarget(targets));
        pool.RemoveAt(0);

        if (pool.Count == 0)
        {
            Reshuffle();
        }
    }

    // Use this if you would like to directly attack using deck
    public void AttackWith(GameObject attack, EntityClass target)
    {
        var action = attack.GetComponent<ActionClass>();
        action.Target = target;
        action.Origin = this;
        combatInfo.AddCombatSprite(action);
        BattleQueue.BattleQueueInstance.AddAction(action);
    }

    protected EntityClass CalculateAttackTarget(List<EntityClass> potentialTargets)
    {
        int totalWeight = potentialTargets.Sum(target => TargetingWeights(target));
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int cumulativeWeight = 0;
        var selectedTarget = potentialTargets.First(target =>
        {
            cumulativeWeight += TargetingWeights(target);
            return cumulativeWeight > randomValue;
        });

        return selectedTarget;
    }

    protected virtual void Reshuffle()
    {
        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < deck.Count; i++)
        {
            temp.Add(deck[i]);
        }

        while (temp.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, temp.Count);
            pool.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
    }

    public override void DestroyDeck()
    {
        foreach (GameObject card in deck)
        {
            Destroy(card);
        }
        deck.Clear();
        pool.Clear();
    }

    public override IEnumerator ResetPosition()
    {
        yield return StartCoroutine(MoveToPosition(initialPosition, 0f, 0.8f));
        FaceOpponent();
    }

    public override void PerformSelection()
    {
        AddAttack(GetOpponents());
        StartCoroutine(ResetPosition());
    }

    public void InjectCard(ActionClass card)
    {
        ActionClass toAdd = Instantiate(card);
        toAdd.Origin = this;
        pool.Insert(0, toAdd.gameObject);
        toAdd.transform.position = new Vector3(-100, -100, 1);
    }

    protected virtual List<EntityClass> GetOpponents()
    {
        return Team switch
        {
            EntityTeam.PlayerTeam => new List<EntityClass>(CombatManager.Instance.GetEnemies().Concat(CombatManager.Instance.GetNeutral())),
            EntityTeam.EnemyTeam => new List<EntityClass>(CombatManager.Instance.GetPlayers().Concat(CombatManager.Instance.GetNeutral())),
            EntityTeam.NeutralTeam => new(),
            _ => throw new ArgumentOutOfRangeException("Team possibly not initialized, this is my team: " + Team)
        };
    }
}

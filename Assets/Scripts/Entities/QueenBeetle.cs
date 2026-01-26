using Cards.EnemyCards.FrogCards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public record HatcheryUsed() : IEvent;
public class QueenBeetle : EnemyClass
{
#nullable enable
    private const int MAX_BEETLES = 4;
    [SerializeField] 
    private Beetle?[]? availability;
    [SerializeField] private Transform[] beetleReturnLocationPreset = new Transform[MAX_BEETLES];
    private readonly Vector3[] beetleLocations = new Vector3[MAX_BEETLES];

    public void IntializeChildBeetles(List<Beetle> guardBeetles)
    {
        availability = new Beetle[MAX_BEETLES];

        for (int i = 0; i < beetleReturnLocationPreset.Length; i++)
        {
            beetleLocations[i] = beetleReturnLocationPreset[i].position;
        }

        for (int i = 0; i < guardBeetles.Count; ++i)
        {
            availability[i] = guardBeetles[i];
            beetleLocations[i] = guardBeetles[i].transform.position;
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        MaxHealth = 50;
        Health = MaxHealth;
        myName = "The Queen";
        TargetingWeights = (entity => entity.Team == EntityTeam.PlayerTeam ? 100 : 0);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Beetle.OnGainBuffs += HandleGainedBuffs;
        OnEntityDeath += HandleBeetleDied;
        OnEntitySpawn += RegisterBeetle;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Beetle.OnGainBuffs -= HandleGainedBuffs;
        OnEntityDeath -= HandleBeetleDied;
        OnEntitySpawn -= RegisterBeetle;
    }

    // This is called whenever a beetle tries to add a buff.
    // Adds the stacks that were directed to the beetle to the Queen instead.
    private void HandleGainedBuffs(string buffType, int stacks, Beetle beetle)
    {
        if (buffType == Resonate.buffName && beetle.Team == Team)
        {
            AddStacks(buffType, stacks);
            beetle.ReduceStacks(buffType, stacks);
        }
    }

    private void HandleBeetleDied(EntityClass victim)
    {
        for (int i = 0; i < availability?.Length; i++)
        {
            if (availability[i] == victim)
            {
                availability[i] = null;
            }
        }
    }

    private int FindFirstOpenSlot()
    {
        return Array.FindIndex(availability ?? Array.Empty<Beetle>(), slot => slot == null);
    }

    private int NumberOfAvailableSlots()
    {
        return availability?.Count(slot => slot == null) ?? 0;
    }



    // adds the queens attacks according to the following rules:
    //  1. If the queen has 2 or more stacks of resonate, it will use hatchery.
    //  2. Otherwise, the queen will use another attack. As of now, the only other
    //      attack is fragment, so it will be that one.
    //  3. The queen repeats this process twice, attacking twice in one turn.
    //  In the inspector, assign Hatchery to index 0 of the Queen's deck, and any other
    //     attacks after that.
    public override void AddAttack(List<EntityClass> targets)
    {
        bool usedSpawnThisRound = false;
        var hatchery = deck[0];
        var hatchery2 = deck[1];
        for (int i = 0; i < 2; i++)
        {
            if (GetBuffStacks(Resonate.buffName) >= 2 && FindFirstOpenSlot() != -1 && (!usedSpawnThisRound || NumberOfAvailableSlots() > 1)) //Last condition fixes a bug where the queen can try to spawn 2 beetles but then hit the max spawn cap
            {
                AttackWith(usedSpawnThisRound ? hatchery2 : hatchery, CalculateAttackTarget(targets));
                usedSpawnThisRound = true;
                new HatcheryUsed().Invoke();
            }
            else
            {
                // Deck[2] and deck[3] are both fragments. They must be different otherwise retargeting one will retarget both.
                AttackWith(deck[i + 2], CalculateAttackTarget(targets));
            }
        }
    }


    public void RegisterBeetle(EntityClass enemyClass)
    {
        if (enemyClass is not Beetle beetle) return;
        if (beetle.Team != EntityTeam.EnemyTeam) return;
        if (availability == null) return;

        var slot = FindFirstOpenSlot();
        if (slot == -1) return;

        availability[slot] = beetle;
        beetle.gameObject.transform.position = beetleLocations[slot];
        beetle.SetReturnPosition(beetleLocations[slot]);
    }

    private readonly float cycleScaling = 2f; // Higher the number, the faster one phase is 
    private readonly float bobbingAmount = 0.1f; //Amplitude
    private float timer = 0;
    private float verticalOffset = 0;

    private void Update()
    {
        float previousOffset = verticalOffset;
        float waveslice = Mathf.Sin(cycleScaling * timer);
        timer += Time.deltaTime;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        verticalOffset = waveslice * bobbingAmount;
        float translateChange = verticalOffset - previousOffset;
        transform.position = new Vector3(transform.position.x, transform.position.y + translateChange, transform.position.z);
    }

}

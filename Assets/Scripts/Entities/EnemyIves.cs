using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record EnemyIvesDied() : IEvent;
public class EnemyIves : EnemyClass
{
    public override void Start()
    {
        base.Start();
        MaxHealth = 35;
        Health = MaxHealth;
        myName = "Le Ives";
        AddStacks(Resonate.buffName, 3);
        AddStacks(Dissonance.buffName, 3);
        AddStacks(Decohering.buffName, 1);
    }

    public void InjectDeck(List<GameObject> actions)
    {
        foreach (GameObject action in actions)
        {
            GameObject toAdd = Instantiate(action);
            ActionClass addedClass = toAdd.GetComponent<ActionClass>();
            toAdd.transform.position = new Vector3(-10, -10, -10);
            addedClass.Origin = this;

            deck.Add(toAdd);
            pool.Add(toAdd);
        }
    }

    public override IEnumerator Die()
    {
        new EnemyIvesDied().Invoke();
        BattleQueue.BattleQueueInstance.RemoveAllInstancesOfEntity(this);
        DestroyDeck();
        combatInfo.DeactivateCardIcon();

        if (HasAnimationParameter("IsStaggered"))
        {
            animator.SetBool("IsStaggered", true);
        }
        yield break;
    }

}

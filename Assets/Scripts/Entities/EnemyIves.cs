using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIves : EnemyClass
{
    [SerializeField] private Brace brace;
    [SerializeField] private FollowThrough followThrough;
    [SerializeField] private LeftHook leftHook;
    [SerializeField] private RightHook rightHook;
    [SerializeField] private Haymaker haymaker;
    [SerializeField] private Pummel pummel;
    [SerializeField] private Execute execute;
    [SerializeField] private Cleave cleave;
    [SerializeField] private Decimate decimate;
    [SerializeField] private Whirl whirl;
    [SerializeField] private Mutilate mutilate;
    [SerializeField] private RazorGuard razorGuard;
    

    public override void Start()
    {
        base.Start();
        MaxHealth = 35;
        Health = MaxHealth;
        myName = "Le Ives";
        AddStacks(Decohering.buffName, 1);
        AddStacks(Resonate.buffName, 3);
        AddStacks(Dissonance.buffName, 3);
    }

    public override void AddAttack(List<EntityClass> targets)
    {

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackSmash : SlimeAttacks, IPlayableSlimeCard
{
#nullable enable
    [SerializeField]
    private List<Sprite> animationFrame = new();
#nullable enable
    StackSmash? activeDuplicateInstance = null;
    bool originalCopy = true;
      

    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 4;
        Speed = 2;
        CostToAddToDeck = 1;

        myName = "Stack Smash";
        description = "If this attack is unstaggered, attack again";
        CardType = CardType.MeleeAttack;
    }


    //@Author: Anrui. Called by ActionClass.OnHit() 
    public override void CardIsUnstaggered()
    {
        if (!onHitWasCalled) InsertDuplicate();
        base.CardIsUnstaggered();
    }
    public override void OnHit()
    {
        onHitWasCalled = true;
        InsertDuplicate();
        StartCoroutine(AttackAnimation(base.OnHit));
    }

    private void InsertDuplicate()
    {
        if (originalCopy)
        {
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<StackSmash>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
            }
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = Target;
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance!);
        }
    }

    protected override IEnumerator AttackAnimation(AttackCallback? attackCallback)
    {
        Origin.animator.enabled = false;
        SpriteRenderer spriteRenderer = Origin.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = animationFrame[0];
        yield return new WaitForSeconds(0.10f);
        spriteRenderer.sprite = animationFrame[1];
        yield return new WaitForSeconds(0.16f);
        Vector3 originalPosition = Origin.myTransform.position;
        Origin.myTransform.position = originalPosition + new Vector3(0, 1.5f, 0);
        spriteRenderer.sprite = animationFrame[2];
        yield return new WaitForSeconds(0.28f);
        spriteRenderer.sprite = animationFrame[3];
        Origin.myTransform.position = originalPosition;
        attackCallback?.Invoke();
        yield return new WaitForSeconds(0.20f);
        spriteRenderer.sprite = animationFrame[4];
        Origin.animator.enabled = true;
    }
}

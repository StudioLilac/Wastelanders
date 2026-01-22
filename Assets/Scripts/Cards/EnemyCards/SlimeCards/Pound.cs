using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Pound : SlimeAttacks, IPlayableSlimeCard
{
#nullable enable
    [SerializeField]
    private List<Sprite> animationFrame = new();

    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 2;
        upperBound = 4;
        Speed = 2;
        CostToAddToDeck = 1;

        myName = "Pound";
        description = "I wouldn't stand still if I were you!";
        CardType = CardType.MeleeAttack;
    }


    public override void OnHit()
    {
        onHitWasCalled = true;
        StartCoroutine(AttackAnimation(base.OnHit));
    }


    protected override IEnumerator AttackAnimation(AttackCallback? attackCallback)
    {
        Vector3 originalPosition = Origin.myTransform.position;
        Origin.animator.enabled = false;
        SpriteRenderer spriteRenderer = Origin.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = animationFrame[0];
        Origin.myTransform.position = originalPosition;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.sprite = animationFrame[1];
        attackCallback?.Invoke();
        yield return new WaitForSeconds(0.010f);
        spriteRenderer.sprite = animationFrame[2];
        yield return new WaitForSeconds(0.32f);
        spriteRenderer.sprite = animationFrame[3];
        yield return new WaitForSeconds(0.20f);
        spriteRenderer.sprite = animationFrame[4];
        Origin.animator.enabled = true;
        Origin.myTransform.position = originalPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Fling : BeetleAttacks, IPlayableBeetleCard
{
    [SerializeField] private ProjectileBehaviour projectileBehaviour;
    public const string FLING_ANIMATION_NAME = "IsFlinging";
    [SerializeField] private AnimationClip flingClip;

    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 4;
        
        Speed = 3;

        description = "If this card hits an opponent, gain +1 resonance.";

        myName = "Fling";
        CardType = CardType.RangedAttack;
        CostToAddToDeck = 2;
    }

    public override void OnHit()
    {
        IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, flingClip, FLING_ANIMATION_NAME);
        Origin.AttackAnimation(FLING_ANIMATION_NAME);
        StartCoroutine(HandleProjectile());
    }

    private IEnumerator HandleProjectile()
    {
        var scaledAnimationTime = 0.6f;
        var firstAttackFrame = 0.1f;
        yield return new WaitForSeconds(firstAttackFrame / scaledAnimationTime);
        yield return projectileBehaviour.ProjectileAnimation(Origin.gameObject.transform.position + new Vector3(0, 0.25f), Target.gameObject.transform.position);
        OnProjectileHit();
    }

    private void OnProjectileHit()
    {

        if (Target.Team == Origin.Team.OppositeTeam())
        {
            Origin.AddStacks(Resonate.buffName, 1);
        }
        AudioManager.Instance.PlaySFX(Fragment.FRAGMENT_SOUND_EFFECT_NAME);
        base.OnHit();
    }
}

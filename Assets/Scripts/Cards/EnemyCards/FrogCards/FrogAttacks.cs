using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public abstract class FrogAttacks : ActionClass
{
    public const string SPIT_SOUND_EFFECT_NAME = "Frog Hit";
    public const string FROG_ATTACK_NAME = "IsFrogging";
    public const string PRINCESS_FROG_ATTACK_NAME = "IsPrincessing";

    protected string frogAttackAnimationName = FROG_ATTACK_NAME;

    [SerializeField] private ProjectileBehaviour? projectileBehaviour;


    public override void Initialize()
    {
        base.Initialize();
        CardType = CardType.RangedAttack;
        if (projectileBehaviour != null)
            projectileBehaviour.projSpeed = 12f;
    }

    public override void OnHit()
    {
        if (Origin.HasAnimationParameter(frogAttackAnimationName))
        {
            Origin.AttackAnimation(frogAttackAnimationName);
        }
        if (projectileBehaviour != null)
        {
            StartCoroutine(HandleProjectile());
        } else
        {
            AudioManager.Instance.PlaySFX(SPIT_SOUND_EFFECT_NAME); // temp sound effect.
            base.OnHit();
        }
    }
    private IEnumerator HandleProjectile()
    {

        var scaledAnimationTime = 0.6f;
        var firstAttackFrame = 0.18f;
        // Most practical way to handle the delay for frog spitting before the projectile actually comes out. 
        yield return new WaitForSeconds(firstAttackFrame / scaledAnimationTime);
        yield return projectileBehaviour!.ProjectileAnimation(Origin.gameObject.transform.position, Target.gameObject.transform.position);
        OnProjectileHit();
    }
    protected virtual void OnProjectileHit()
    {
        AudioManager.Instance.PlaySFX(SPIT_SOUND_EFFECT_NAME);
        base.OnHit();
    }
}
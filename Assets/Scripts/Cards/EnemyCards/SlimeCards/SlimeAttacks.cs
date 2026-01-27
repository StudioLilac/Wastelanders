using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public abstract class SlimeAttacks : ActionClass
{
    public const string SLIME_SOUND_EFFECT_NAME = "Slime Hit";
#nullable enable
    protected bool onHitWasCalled = false;
    protected delegate void AttackCallback();

    public override void CardIsUnstaggered()
    {
        if (!onHitWasCalled)
        {
            StartCoroutine(AttackAnimation(null));
        }
        onHitWasCalled=false;
    }

    public override void OnHit()
    {
        AudioManager.Instance.PlaySFX(SoundID.CB_slime_hit);
        base.OnHit();
    }

    protected abstract IEnumerator AttackAnimation(AttackCallback? attackCallback);
}

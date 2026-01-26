using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PistolCards : ActionClass
{
    public const string PISTOL_ANIMATION_NAME = "IsShooting";
    public const string PISTOL_SOUND_FX_NAME = "Gun Shot";
    public override sealed void Start()
    {
        //No initialization code here
    }

    public override void Initialize()
    {
        CardType = CardType.RangedAttack;
        base.Initialize();
    }

    public override void CardIsUnstaggered()
    {
        if (Origin.HasAnimationParameter(PISTOL_ANIMATION_NAME))
        {
            Origin.AttackAnimation(PISTOL_ANIMATION_NAME);
        }

        base.CardIsUnstaggered();
    }
    public override void OnHit()
    {
        AudioManager.Instance?.PlaySFX(SoundID.CB_gun_hit);
        base.OnHit();
    }
}

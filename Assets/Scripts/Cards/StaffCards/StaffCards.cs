using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaffCards : ActionClass
{
    public const string STAFF_ANIMATION_NAME = "IsStaffing";
    public const string STAFF_SOUND_FX_NAME = "Staff Hit";
    public override sealed void Start()
    {
        //No initialization code here
    }

    public override void CardIsUnstaggered()
    {
        if (Origin.HasAnimationParameter(STAFF_ANIMATION_NAME))
        {
            Origin.AttackAnimation(STAFF_ANIMATION_NAME);
        }
        base.CardIsUnstaggered();
    }
    public override void OnHit()
    {
        AudioManager.Instance?.PlaySFX(SoundID.CB_staff_hit);
        base.OnHit();
    }
}

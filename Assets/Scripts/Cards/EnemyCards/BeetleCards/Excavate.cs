using UnityEngine;
using UtilClass;

public class Excavate : BeetleAttacks, IPlayableBeetleCard
{
    public const string EXCAVATE_SOUND_EFFECT_NAME = "Excavate Cut";
    public const string EXCAVATE_ANIMATION_NAME = "IsExcavating";
    [SerializeField] private AnimationClip excavateClip;


    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 2;
        upperBound = 4;
        
        Speed = 2;

        CostToAddToDeck = 1;

        description = "Deals 2x damage to crystals.";

        myName = "Excavate";
        CardType = CardType.MeleeAttack;
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { Keywords.Crystals };

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, excavateClip, EXCAVATE_ANIMATION_NAME);
        Origin.AttackAnimation(EXCAVATE_ANIMATION_NAME);
    }

    // does 2x damage if target is crystal
    public override void OnHit()
    {
        AudioManager.Instance.PlaySFX(SoundID.CB_excavate);
 
        if (Target is Crystals) rolledCardStats.ActualRoll = 2 * rolledCardStats.ActualRoll;
        base.OnHit();
    }
}

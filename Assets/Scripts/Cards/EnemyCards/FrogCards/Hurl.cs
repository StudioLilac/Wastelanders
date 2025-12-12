
using UnityEngine;

public class Hurl : FrogAttacks, IPlayableFrogCard
{
    [SerializeField] private AnimationClip animationClip;
    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 7;
        
        Speed = 5;

        CostToAddToDeck = 2;

        myName = "Hurl";
        description = "Watch out!";
    }

    public override void OnHit()
    {
        IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, animationClip, FROG_ATTACK_NAME);
        base.OnHit();
    }
}


using UnityEngine;
using UtilClass;

public class Hurl : FrogAttacks, IPlayableFrogCard
{
    public const string NAME = "Hurl";
    public const string DESCRIPTION = "Watch out!";
    private const int LOWER = 1, UPPER = 7;

    [SerializeField] private AnimationClip animationClip;
    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = LOWER;
        upperBound = UPPER;

        Speed = 5;

        CostToAddToDeck = 2;

        myName = NAME;
        description = DESCRIPTION;
    }

    public static readonly GlossaryNode Glossary = new(
        NAME,
        DESCRIPTION,
        null,
        new CardStats(LOWER, UPPER)
    );

    public override void OnHit()
    {
        IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, animationClip, FROG_ATTACK_NAME);
        base.OnHit();
    }
}

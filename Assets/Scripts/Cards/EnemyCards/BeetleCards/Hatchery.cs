using Cards.EnemyCards.FrogCards;
using UnityEngine;


public class Hatchery : ActionClass, IPlayableQueenCard
{
    public GameObject[] beetlePrefabs;
    private const int HATCHERY_COST = 2;
    private const string HATCHERY_ANIMATION = "IsHatchery";
    private const string HATCHERY_SOUND = "WL_BeetleSummon";
    [SerializeField] private AnimationClip animationClip;

    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 2;
        upperBound = 4;
        
        Speed = 1;
        
        description = $"Spend +{HATCHERY_COST} resonance to play this card. If this card is not staggered, spawn 1 random beetle.";

        CostToAddToDeck = 2;
        myName = "Hatchery";
        CardType = CardType.Defense;
    }

    public override void OnQueue()
    {
        Origin.ReduceStacks(Resonate.buffName, HATCHERY_COST);
    }

    public override void OnRetrieveFromQueue()
    {
        Origin.AddStacks(Resonate.buffName, HATCHERY_COST);
    }

    public override bool IsPlayableByPlayer(out PopupType popupType)
    {
        bool isPlayable = base.IsPlayableByPlayer(out popupType);
        int stackCount = Origin.GetBuffStacks(Resonate.buffName);
        bool enoughStacks = stackCount >= HATCHERY_COST;

        popupType = enoughStacks ? popupType : new PopupType.InsufficientResources(stackCount, HATCHERY_COST);

        return isPlayable && enoughStacks;
    }

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();

        IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, animationClip, HATCHERY_ANIMATION); 
        Origin.AttackAnimation(HATCHERY_ANIMATION);
        SoundID.CB_hatchery_summon.Play();
        GameObject beetle = Instantiate(beetlePrefabs[Random.Range(0, beetlePrefabs.Length)]);
        beetle.transform.SetParent(Origin.transform.parent);
        beetle.transform.transform.position = Origin.transform.position - new Vector3(0, 2, 0);
        beetle.GetComponent<EntityClass>().Team = Origin.Team;
    }

}

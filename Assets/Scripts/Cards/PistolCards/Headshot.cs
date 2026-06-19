public class Headshot : PistolCards
{

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 4;
        Speed = 2;

        myName = "Headshot";
        description = "On hit, deal +1 damage for each stack of Accuracy and gain 1 accuracy on kill.";
        CardType = CardType.RangedAttack;
        base.Initialize();
    }

    public override void OnHit()
    {
        IncrementRoll(Origin.GetBuffStacks(Accuracy.buffName));
        base.OnHit();

        if (Target.IsDead)
        {
            Origin.AddStacks(Accuracy.buffName, 1);
        }
    }
}

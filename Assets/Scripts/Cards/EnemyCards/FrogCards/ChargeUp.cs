using UnityEngine;

public class ChargeUp : FrogAttacks
{
    public const string CHARGE_UP_ANIMATION_NAME = "IsCharging";
    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 1;
        
        Speed = 1;

        description = "Block, if unstaggered, use 'Hurl' next turn";

        myName = "Charge Up";
        CardType = CardType.Defense;
        Renderer renderer = GetComponent<Renderer>();
    }



    public override void CardIsUnstaggered()
    {
        WasteFrog frog = (WasteFrog)this.Origin;
        frog.UseHurl = true;
        Origin.AttackAnimation(CHARGE_UP_ANIMATION_NAME);
        
    }

}

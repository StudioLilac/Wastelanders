using UnityEngine;
using UtilClass;

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

        description = "Block, if not staggered, use 'Hurl' next turn";

        myName = "Charge Up";
        CardType = CardType.Defense;
        Renderer renderer = GetComponent<Renderer>();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { Hurl.Glossary };

    public override void CardIsUnstaggered()
    {
        WasteFrog frog = (WasteFrog)this.Origin;
        frog.UseHurl = true;
        Origin.AttackAnimation(CHARGE_UP_ANIMATION_NAME);
        
    }

}

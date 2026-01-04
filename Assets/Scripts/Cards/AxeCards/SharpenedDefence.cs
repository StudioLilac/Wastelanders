using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SharpenedDefence : AxeCards
{
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 2;
        upperBound = 4;
        Speed = 3;
        CardType = CardType.Defense;    

        myName = "Sharpened Defence";
        description = "Inflict wound equal to damage blocked.";
    }

    public override void OnDefendClash(ActionClass opposingCard)
    {
        int blockedDamage = Mathf.Min(opposingCard.GetRolledStats().ActualRoll, GetRolledStats().ActualRoll);
        Target.AddStacks(Wound.buffName, blockedDamage);
        base.OnDefendClash(opposingCard);
    }
}

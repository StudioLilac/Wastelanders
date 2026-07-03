using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  Entities with wound take +1 damage per stack. Remove 1 wound per round.
public class Wound : StatusEffect
{

    public const string buffName = "Wound";
    public Wound()
    {
        OnEntityHitHandler = delegate (ref int damage)
        {
            if (damage > 0) {
                damage += Stacks;
            }
        };
    }

    public override Sprite GetIcon()
    {
        Sprite buffSprite = new GetStatusIcons().Query()?.wound;
        if (!buffSprite)
        {
            Debug.LogWarning("Buff Sprite is missing");
        }
        return buffSprite;
    }

    public override void NewRound() 
    {
        buffStacks = Stacks / 2;
    }


}

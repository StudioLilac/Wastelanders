using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class Haymaker : FistCards
{
    public const string NAME = "Haymaker";
    public const string DESCRIPTION = "Deals a solid blow!";
    private const int LOWER = 2, UPPER = 5;

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = LOWER;
        upperBound = UPPER;
        Speed = 5;

        myName = NAME;
        description = DESCRIPTION;
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

    public static readonly GlossaryNode Glossary = new(
        NAME,
        DESCRIPTION,
        null,
        new CardStats(LOWER, UPPER)
    );

}

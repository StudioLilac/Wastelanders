using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UtilClass;

public class RightHook : FistCards
{
    public const string NAME = "Right Hook";
    public const string DESCRIPTION = "If this card is not staggered, use 'Haymaker'.";
    private const int LOWER = 2, UPPER = 4;

    [SerializeField]
    private GameObject haymakerPrefab;

    private GameObject haymaker;


    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = LOWER;
        upperBound = UPPER;
        Speed = 4;

        myName = NAME;
        description = DESCRIPTION;
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { Haymaker.Glossary };

    public static readonly GlossaryNode Glossary = new(
        NAME,
        DESCRIPTION,
        null,
        new CardStats(LOWER, UPPER),
        Haymaker.Glossary
    );

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        if (haymaker == null) { haymaker = Instantiate(haymakerPrefab); haymaker.transform.position = new Vector3(-10, 10, 10); }
        ActionClass ac = haymaker.GetComponent<ActionClass>();
        ac.Origin = this.Origin;
        ac.Target = this.Target;
        ac.Speed = this.Speed; //Workaround for multispeeded queue 
        if (ac.Origin is PlayerClass)
        {
            BattleQueue.BattleQueueInstance.AddAction(ac);
        } else
        {
            BattleQueue.BattleQueueInstance.AddAction(ac);
        }

    }
}

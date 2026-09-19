using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UtilClass;
using static UnityEngine.UI.Image;

public class LeftHook : FistCards
{
    [SerializeField]
    private GameObject rightHookPrefab;

    private GameObject righthook;

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 3;
        Speed = 3;

        myName = "Left Hook";
        description = "If this attack is not staggered, use 'Right Hook'.";
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { RightHook.Glossary };


    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        if (!righthook) { righthook = Instantiate(rightHookPrefab); righthook.transform.position = new Vector3(-10, 10, 10); }
        ActionClass ac = righthook.GetComponent<ActionClass>();
        ac.Origin = this.Origin;
        ac.Target = this.Target;
        ac.Speed = this.Speed;
        BattleQueue.BattleQueueInstance.AddAction(ac);
    }
}

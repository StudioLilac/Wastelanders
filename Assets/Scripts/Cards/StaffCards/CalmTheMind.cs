using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UtilClass;

public class CalmTheMind : StaffCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 4;

        Speed = 5;
        
        myName = "Calm The Mind";
        description = "Block, then gain 1 Flow before each attack this turn.";
        
        base.Initialize();
        CardType = CardType.Defense;
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Flow };
    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        ActivateEffect();
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        ActivateEffect();
    }

    private void ActivateEffect()
    {
        this.Subscribe<DequeueEvent>(GiveStack);
        this.Subscribe<GameStateChanged>(ResetHandler);
    }

    private void GiveStack(DequeueEvent e)
    {
        if (e.Wrapper.PlayerAction != null && e.Wrapper.PlayerAction.Origin == Origin)
        {
            Origin.AddStacks(Flow.buffName, 1);
        }
    }

    private void ResetHandler(GameStateChanged e)
    {
        if (e.NewState != GameState.FIGHTING)
        {
            this.UnSubscribe<GameStateChanged>(ResetHandler);
            this.UnSubscribe<DequeueEvent>(GiveStack);
        }
    }
}

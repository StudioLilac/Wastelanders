using UnityEngine;

public class Steadied : StatusEffect
{
    public const string buffName = "Steadied";

    public override bool ShouldRenderBuffIcon() => false;
    
    public override Sprite GetIcon() => null;
    
    public override void NewRound()
    {
        ClearBuff();
    }
}

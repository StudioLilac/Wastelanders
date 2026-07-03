using UnityEngine;


public class Accuracy : StatusEffect
{
    public const string buffName = "Accuracy";

    public Accuracy()
    {
        OnEntityHitHandler = OnEntityHit;
    }
    
    public override void ApplyStacks(ActionClass.RolledStats dup)
    {
        dup.FloorBuffs += buffStacks;
    }

    public override Sprite GetIcon()
    {
        Sprite buffSprite = new GetStatusIcons().Query()?.accuracy;
        if (!buffSprite)
        {
            Debug.LogWarning("Buff Sprite is missing");
        }
        return buffSprite;
    }

    //Getting hit with accuracy halves it 
    void OnEntityHit(ref int damage)
    {
        if (damage > 0)
        {
            buffStacks = Stacks / 2;
        }
    }
}

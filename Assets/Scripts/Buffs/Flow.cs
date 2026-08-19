using Unity.VisualScripting;
using UnityEngine;


public class Flow : StatusEffect
{
    public const string buffName = "Flow";

    
    public override void ApplySingleUseEffects(ActionClass.RolledStats dup)
    {
        dup.FloorBuffs += this.buffStacks;
        dup.CeilingBuffs += this.buffStacks;
        dup.OneTimeBuffs = (this, buffStacks, buffStacks);
        ClearBuff();
    }

    public override Sprite GetIcon()
    {
        Sprite buffSprite = new GetStatusIcons().Query()?.flow;
        if (!buffSprite)
        {
            Debug.LogWarning("Buff Sprite is missing");
        }
        return buffSprite;
    }
}

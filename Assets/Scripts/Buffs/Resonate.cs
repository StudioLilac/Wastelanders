using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Resonate : StatusEffect
{
    public const string buffName = "Resonance";

    public override void ApplyStacks(ActionClass.RolledStats dup)
    {
        dup.CeilingBuffs += this.buffStacks;
    }

    public override Sprite GetIcon()
    {
        Sprite buffSprite = Resources.Load<Sprite>("StatusIcon/" + buffName);
        if (!buffSprite)
        {
            Debug.LogWarning("Buff Sprite is missing");
        }
        return buffSprite;
    }
}
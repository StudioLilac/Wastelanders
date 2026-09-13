using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffFactory : MonoBehaviour
{
    // Returns a Buff of the Specified Type
    public static StatusEffect GetStatusEffect(string buffType)
    {
        return buffType switch
        {
            Accuracy.buffName => new Accuracy(),
            Flow.buffName => new Flow(),
            Resonate.buffName => new Resonate(),
            Wound.buffName => new Wound(),
            Dissonance.buffName => new Dissonance(),
            Decohering.buffName => new Decohering(),
            Steadied.buffName => new Steadied(),
            _ => throw new System.Exception("Unkown Buff: " + buffType),
        };
    }
}

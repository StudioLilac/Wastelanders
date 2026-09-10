using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffFactory : MonoBehaviour
{
    // Returns a Buff of the Specified Type
    public static StatusEffect GetStatusEffect(string buffType)
    {
        switch (buffType)
        {
            case Accuracy.buffName:
                return new Accuracy();

            case Flow.buffName:
                return new Flow();

            case Resonate.buffName:
                return new Resonate();

            case Wound.buffName:
                return new Wound();

            case Steadied.buffName:
                return new Steadied();

            default:
                throw new System.Exception("Unkown Buff: " + buffType);
        }
    }
}

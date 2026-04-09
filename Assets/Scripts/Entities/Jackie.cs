using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jackie : PlayerClass
{
    // Start is called before the first frame update
    public override void Start()
    {
        myName = "Jackie";
        MaxHealth = 35;
        Health = MaxHealth;
        base.Start();
    }

    protected override void GrabDeck()
    {
        cardPrefabs = CombatManager.Instance.GetDeck(PlayerDatabase.PlayerName.JACKIE);
    }
}

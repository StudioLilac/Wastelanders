using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jackie : PlayerClass
{
    public override void Start()
    {
        myName = "Jackie";
        MaxHealth = 35;
        Health = MaxHealth;
        base.Start();
    }

    protected override void GrabDeck()
    {
        cardPrefabs = new GetDeck(PlayerDatabase.PlayerName.JACKIE).Query() ?? new();
    }
}

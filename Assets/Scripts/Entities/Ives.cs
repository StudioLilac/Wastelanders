using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ives : PlayerClass
{
    public override void Start()
    {
        MaxHealth = 35;
        Health = MaxHealth;
        myName = "Ives";
        base.Start();
    }

    protected override void GrabDeck()
    {
        cardPrefabs = new GetDeck(PlayerDatabase.PlayerName.IVES).Query() ?? new();
    }
}


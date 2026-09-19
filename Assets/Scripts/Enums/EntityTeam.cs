using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public record AddEntityToTeam(EntityClass Entity, EntityTeam Team) : IEvent;
public record RemoveEntityFromTeam(EntityClass Entity, EntityTeam Team) : IEvent;
public enum EntityTeam
{
    NoTeam,
    PlayerTeam,
    NeutralTeam,
    EnemyTeam,
}

public static class EntityTeamExtensions
{
    public static EntityTeam OppositeTeam(this EntityTeam entityTeam)
    {
        return entityTeam switch
        {
            EntityTeam.PlayerTeam => EntityTeam.EnemyTeam,
            EntityTeam.EnemyTeam => EntityTeam.PlayerTeam,
            EntityTeam.NeutralTeam => EntityTeam.NeutralTeam,
            _ => EntityTeam.NoTeam
        };
    }

    public static List<EntityClass> GetTeamMates(this EntityTeam entityTeam) => new GetTeammates(entityTeam).Query() ?? new();
}

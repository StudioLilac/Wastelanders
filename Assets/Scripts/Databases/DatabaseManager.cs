using UnityEngine;

#nullable enable
public record GetCardDatabase() : IQuery<CardDatabase?>;
public record GetPlayerDatabase() : IQuery<PlayerDatabase?>;
public record GetStatusIcons() : IQuery<StatusIcons?>;

public class DatabaseManager : PersistentSingleton<DatabaseManager>
{
    [SerializeField] private StatusIcons statusIcons = null!;
    [SerializeField] private CardDatabase cardDatabase = null!;
    [SerializeField] private PlayerDatabase playerDatabase = null!;

    protected override void Awake()
    {
        base.Awake();
        if (Current != this) return;

        this.Answer<GetStatusIcons, StatusIcons?>(_ => statusIcons);
        this.Answer<GetCardDatabase, CardDatabase?>(_ => cardDatabase);
        this.Answer<GetPlayerDatabase, PlayerDatabase?>(_ => playerDatabase);
    }
}

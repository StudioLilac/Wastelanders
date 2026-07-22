using LevelSelectInformation;
using System;
using System.Collections.Generic;
using System.Linq;

public delegate bool IsSubWeaponsUnlocked();

public interface ISubWeaponType
{
    public string Name { get; set; }
    public GetRenderableCards GetSubWeaponCards { get; set; }
    public IsSubWeaponsUnlocked IsUnlocked { get; set; }
}

public class PlayableEnemyWeapon : ISubWeaponType
{
    public static readonly PlayableEnemyWeapon beetleWeapons = new PlayableEnemyWeapon(
        name: "Beetle Cards",
        getSubWeaponCards: (db => db.GetCardsByType(CardDatabase.WeaponType.ENEMY).FindAll(card => card is IPlayableBeetleCard).ToList()),
        isUnlocked: () => BountyManager.Instance.IsBountyCompleted(PrincessFrogBounties.SOLO_JACKIE)
    );

    public static readonly PlayableEnemyWeapon frogWeapons = new PlayableEnemyWeapon(
        name: "Frog Cards",
        getSubWeaponCards: (db => db.GetCardsByType(CardDatabase.WeaponType.ENEMY).FindAll(card => card is IPlayableFrogCard).ToList()),
        isUnlocked: () => BountyManager.Instance.IsBountyCompleted(PrincessFrogBounties.FROG_CHALLENGE)
    );

    public static readonly PlayableEnemyWeapon slimeWeapons = new PlayableEnemyWeapon(
        name: "Slime Cards",
        getSubWeaponCards: (db => db.GetCardsByType(CardDatabase.WeaponType.ENEMY).FindAll(card => card is IPlayableSlimeCard).ToList()),
        isUnlocked: () => BountyManager.Instance.IsBountyCompleted(PrincessFrogBounties.SLIME_CHALLENGE)
    );
    public static readonly PlayableEnemyWeapon queenBeetleWeapons = new PlayableEnemyWeapon(
        name: "Queen Beetle Cards",
        getSubWeaponCards: (db => db.GetCardsByType(CardDatabase.WeaponType.ENEMY).FindAll(card => card is IPlayableQueenCard).ToList()),
        isUnlocked: () => BountyManager.Instance.IsBountyCompleted(PrincessFrogBounties.QUEEN_CHALLENGE)
    );
    public static readonly PlayableEnemyWeapon princessFrogWeapons = new PlayableEnemyWeapon(
        name: "Princess Frog Cards",
        getSubWeaponCards: (db => db.GetCardsByType(CardDatabase.WeaponType.ENEMY).FindAll(card => card is IPlayablePrincessFrogCard).ToList()),
        isUnlocked: () => GameStateManager.Instance.CurrentLevelProgress > StageInformation.Get<StageInformation.PrincessFrogFight>().LevelID
    );

    public static List<ISubWeaponType> values = new()
    {
        beetleWeapons,
        frogWeapons,
        slimeWeapons,
        queenBeetleWeapons,
        princessFrogWeapons
    };

    public static List<ISubWeaponType> UnlockedWeapons() {
        return values.Where(e => e.IsUnlocked()).ToList();
    }

    public string Name { get; set; }
    public GetRenderableCards GetSubWeaponCards { get; set; }
    public IsSubWeaponsUnlocked IsUnlocked { get; set; }
    public PlayableEnemyWeapon(string name, GetRenderableCards getSubWeaponCards, IsSubWeaponsUnlocked isUnlocked)
    {
        Name = name;
        this.GetSubWeaponCards = getSubWeaponCards;
        this.IsUnlocked = isUnlocked;
    }
}
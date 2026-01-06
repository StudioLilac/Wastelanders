using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using WeaponDeckSerialization;


/*
 * @author Anrui
 Represents All the player cards in the game.
 All Cards loaded in the corresponding scriptable object will be loaded during deck selection
 */
[CreateAssetMenu(fileName = "NewCardDatabase", menuName = "Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<StaffCards> staffCards;
    public List<PistolCards> pistolCards;
    public List<FistCards> fistCards;
    public List<AxeCards> axeCards;
    public List<ActionClass> enemyCards;

    //Grabs the corresponding weaponDeck to the (@param weaponType)
    public List<ActionClass> GetCardsByType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.STAFF: return new List<ActionClass>(staffCards);
            case WeaponType.PISTOL: return new List<ActionClass>(pistolCards);
            case WeaponType.AXE: return new List<ActionClass>(axeCards);
            case WeaponType.FIST: return new List<ActionClass>(fistCards);
            case WeaponType.ENEMY: return new List<ActionClass>(enemyCards);
            default:
                Debug.LogWarning("Weapon Type is currently unsupported");
                return null;
        }
    }

    public static List<ISubWeaponType> GetUnlockedSubFoldersFor(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.ENEMY => PlayableEnemyWeapon.UnlockedWeapons(),
            _ => new(),
        };
    }

    // Necessary to set the initial page that is loaded when we enter a subfolder
    public List<ActionClass> GetDefaultSubFolderData(WeaponType weaponType)
    {
        List<ISubWeaponType> subfolders = GetUnlockedSubFoldersFor(weaponType);
        return subfolders.Count > 0 ? GetUnlockedSubFoldersFor(weaponType)[0].GetSubWeaponCards(this) : new();
    }

    public List<ActionClass> GetAllCards()
    {
        List<ActionClass> allCards = new();
        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            allCards.AddRange(GetCardsByType(type));
        }
        return allCards;
    }

    public List<ActionData> GetDefaultActionDatas()
    {
        List<ActionData> allData = new();
        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            foreach (ActionClass action in GetCardsByType(type))
            {
                ActionData actionData = new ActionData();
                actionData.ActionClassName = action.GetType().Name;
                allData.Add(actionData);
            }
        }
        return allData;
    }


    // Converts a list of Action Class types to the actual prefab contained in this database. 
    public List<InstantiableActionClassInfo> GetPrefabInfoForDeck(List<SerializableActionClassInfo> tuples)
    {
        var instantiableCardInfos = tuples.Select(tuple => new InstantiableActionClassInfo(
            actionClass: GetAllCards().Find(actionClass => actionClass.GetType().Name == tuple.ActionClassName),
            isEvolved: tuple.IsEvolved)
        ).Where(item => item.ActionClass != null).ToList();
        return instantiableCardInfos;
    }

    // For performance reasons, use this if you know the type
    public List<ActionClass> ConvertStringsToCards(WeaponType type, List<string> types)
    {
        return GetCardsByType(type).FindAll(actionClass => types.Contains(actionClass.GetType().Name));
    }


    public enum WeaponType
    {
        STAFF,
        PISTOL,
        FIST,
        AXE,
        ENEMY
    }
}


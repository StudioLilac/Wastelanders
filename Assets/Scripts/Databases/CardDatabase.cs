using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using JetBrains.Annotations;
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
    public List<ClasslessCards> classlessCards;
    public List<ActionClass> enemyCards;
    private Dictionary<string, ActionClass> cardLookup;
    public static event Action<SerializableActionClassInfo> OnInvalidCardFound;

    //Grabs the corresponding weaponDeck to the (@param weaponType)
    public List<ActionClass> GetCardsByType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.STAFF: return new List<ActionClass>(staffCards);
            case WeaponType.PISTOL: return new List<ActionClass>(pistolCards);
            case WeaponType.AXE: return new List<ActionClass>(axeCards);
            case WeaponType.FIST: return new List<ActionClass>(fistCards);
            case WeaponType.CLASSLESS: return new List<ActionClass>(classlessCards);
            case WeaponType.ENEMY: return new List<ActionClass>(enemyCards);
            default:
                Debug.LogWarning("Weapon Type is currently unsupported");
                return null;
        }
    }
    
#nullable enable
    public ClasslessCards? GetDefaultAction(PlayerClass player) {
        return player switch
        {
            Jackie => classlessCards[0],
            Ives => classlessCards[1],
            _ => classlessCards.FirstOrDefault(),
        };
    }
#nullable disable

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

    private void InitializeLookup()
    {
        cardLookup = GetAllCards().ToDictionary(
            card => card.GetType().Name,
            card => card
        );
    }

    // Converts a list of Action Class types to the actual prefab contained in this database. 
    public List<InstantiableActionClassInfo> GetPrefabInfoForDeck(List<SerializableActionClassInfo> tuples)
    {
        if (cardLookup == null) InitializeLookup();

        var validCards = new List<InstantiableActionClassInfo>(tuples.Count);
        var iterationCopy = new List<SerializableActionClassInfo>(tuples);

        foreach (var tuple in iterationCopy)
        {
            if (cardLookup!.TryGetValue(tuple.ActionClassName, out ActionClass prefab))
            {
                validCards.Add(new InstantiableActionClassInfo(
                    actionClass: prefab,
                    isEvolved: tuple.IsEvolved
                ));
            }
            else
            {
                Debug.LogWarning($"Could not find card class with name '{tuple.ActionClassName}' in database.");
                OnInvalidCardFound?.Invoke(tuple);
            }
        }

        return validCards;
    }

    // For performance reasons, use this if you know the type
    public List<ActionClass> ConvertStringsToCards(WeaponType type, List<string> types)
    {
        return GetCardsByType(type).FindAll(actionClass => types.Contains(actionClass.GetType().Name));
    }


    public enum WeaponType
    {
        STAFF = 0,
        PISTOL = 1,
        FIST = 2,
        AXE = 3,
        ENEMY = 4,
        CLASSLESS = 5,
    }
}


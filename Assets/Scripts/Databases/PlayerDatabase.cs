using System.Collections.Generic;
using System.Linq;
using Systems.Persistence;
using UnityEngine;
using WeaponDeckSerialization;
using static CardDatabase;
using static PlayerDatabase;

/*
 * Class that holds the information of all players like level, weapon proficiency, decks. Can be edited in the unity editor
 *  */
[CreateAssetMenu(fileName = "New Player Database", menuName = "Player Database")]
public class PlayerDatabase : ScriptableObject
{
    public PlayerData JackieData;
    public PlayerData IvesData;
#nullable enable

    public SerializableGuid Id { get; set; }

    private void OnEnable()
    {
        CardDatabase.OnInvalidCardFound += HandleInvalidCardDetected;
    }

    private void OnDisable()
    {
        CardDatabase.OnInvalidCardFound -= HandleInvalidCardDetected;
    }

    private void HandleInvalidCardDetected(SerializableActionClassInfo invalidTuple)
    {
        string badCardName = invalidTuple.ActionClassName;
        Debug.LogWarning($"[PlayerDatabase] Received cleanup request for: {badCardName}. Scrubbing data...");

        int removedJackie = JackieData.PurgeCardByName(badCardName);
        int removedIves = IvesData.PurgeCardByName(badCardName);

        if (removedJackie + removedIves > 0)
        {
            Debug.Log($"[PlayerDatabase] Cleanup Complete. Removed {removedJackie + removedIves} instances of '{badCardName}' to prevent crashes.");
        }
    }

    public PlayerData GetDataByPlayerName(PlayerName player)
    {
        return player switch
        {
            PlayerName.JACKIE => JackieData,
            PlayerName.IVES => IvesData,
            _ => throw new System.Exception("No Valid Database for that player name" + player),
        };
    }

    public List<SerializableActionClassInfo> GetDeckByPlayerName(PlayerName player)
    {
        return player switch
        {
            PlayerName.JACKIE => JackieData.GetCombinedDeck(),
            PlayerName.IVES => IvesData.GetCombinedDeck(),
            _ => throw new System.Exception("No Valid Database for that player name" + player),
        };
    }

    public void Bind(PlayerInformation data)
    {
        JackieData = data.JackieData;
        IvesData = data.IvesData;
    }

    [System.Serializable]
    public class PlayerData
    {
        public static readonly PlayerData JACKIE_DEFAULT = new
            (
            name: typeof(Jackie).Name,
            playerWeaponProficiency: new() {
                new WeaponProficiency(WeaponType.PISTOL,  12),
                new WeaponProficiency(WeaponType.STAFF,  12),
                new WeaponProficiency(WeaponType.AXE, 10),
                new WeaponProficiency(WeaponType.FIST, 10),
                new WeaponProficiency(WeaponType.ENEMY, 10)
            },
            selectedWeapons: new() { WeaponType.STAFF, WeaponType.PISTOL },
            playerDeck: new()
            {
                new SerializableWeaponListEntry
                (
                    WeaponType.PISTOL,
                    new()
                    {
                        new(typeof(IronSights).Name), new(typeof(Silencer).Name),
                        new(typeof(QuickDraw).Name), new(typeof(PistolWhip).Name),
                        new(typeof(HipFire).Name), new(typeof(RapidFire).Name)
                    }
                ),
                new SerializableWeaponListEntry
                (
                    WeaponType.STAFF,
                    new()
                    {
                        new(typeof(CheapStrike).Name), new(typeof(FocusedStrike).Name),
                        new(typeof(Trip).Name), new(typeof(Flurry).Name),
                        new(typeof(Jab).Name), new(typeof(CounterStrike).Name)
                    }
                )
            }
            );

        public static readonly PlayerData IVES_DEFAULT = new(
            name: typeof(Ives).Name,
            playerWeaponProficiency: new() {
                new WeaponProficiency(WeaponType.PISTOL, 10),
                new WeaponProficiency(WeaponType.STAFF, 10),
                new WeaponProficiency(WeaponType.AXE, 12),
                new WeaponProficiency(WeaponType.FIST,  12),
                new WeaponProficiency(WeaponType.ENEMY,  10)
            },
            selectedWeapons: new() { WeaponType.AXE, WeaponType.FIST },
            playerDeck: new()
            {
                new SerializableWeaponListEntry
                (
                    WeaponType.AXE,
                    new() 
                    { 
                        new(typeof(Execute).Name), new(typeof(RazorGuard).Name),
                        new(typeof(Cleave).Name), new(typeof(Decimate).Name),
                        new(typeof(Mutilate).Name), new(typeof(Whirl).Name)
                    }
                ),
                new SerializableWeaponListEntry
                (
                    WeaponType.FIST, 
                    new() 
                    { 
                        new(typeof(Brace).Name), new(typeof(LeftHook).Name), 
                        new(typeof(RightHook).Name), new(typeof(Pummel).Name), 
                        new(typeof(Haymaker).Name), new(typeof(FollowThrough).Name)
                    }
                 )
            }
            );

        [SerializeField] private string name = "";
        [SerializeField] private List<WeaponProficiency> playerWeaponProficiency = new();
        [SerializeField] public List<WeaponType> selectedWeapons = new();
        [SerializeField] private List<SerializableWeaponListEntry> playerDeck = new();

        public PlayerData(string name, List<WeaponProficiency> playerWeaponProficiency, List<WeaponType> selectedWeapons, List<SerializableWeaponListEntry> playerDeck)
        {
            this.name = name;
            this.playerWeaponProficiency = playerWeaponProficiency;
            this.selectedWeapons = selectedWeapons;
            this.playerDeck = playerDeck;
        }

        public SerializableWeaponListEntry GetPlayerWeaponDeck(WeaponType weaponType)
        {
            SerializableWeaponListEntry playerWeaponDeck = playerDeck.FirstOrDefault(entry => entry.weapon == weaponType);

            if (playerWeaponDeck == null)
            {
                playerWeaponDeck = new SerializableWeaponListEntry(weapon: weaponType, weaponDeck: new());
                playerDeck.Add(playerWeaponDeck);
            }

            return playerWeaponDeck;
        }

        public WeaponProficiency GetProficiencyPointsTuple(WeaponType weaponType)
        {
            var proficiencyPointsTuple = playerWeaponProficiency.FirstOrDefault(entry => entry.WeaponType == weaponType);
            if (proficiencyPointsTuple == null)
            {
                proficiencyPointsTuple = new WeaponProficiency(weaponType, 0);
                playerWeaponProficiency.Add(proficiencyPointsTuple);
            }
            return proficiencyPointsTuple;
        }


        //Gets the combination of both smaller decks
        public List<SerializableActionClassInfo> GetCombinedDeck()
        {
            List<SerializableActionClassInfo> combinedDeck = new();

            foreach (var entry in playerDeck)
            {
                if (selectedWeapons.Contains(entry.weapon))
                {
                    combinedDeck.AddRange(entry.weaponDeck);
                }
            }

            return combinedDeck;
        }

        //Like a dictionary, gets the player weaponDeck by the weapon type, if not contained, return a empty list
        public List<SerializableActionClassInfo> GetDeckByWeaponType(WeaponType weaponType)
        {
            foreach (var entry in playerDeck)
            {
                if (entry.weapon == weaponType)
                {
                    return new(entry.weaponDeck);
                }
            }

            return new List<SerializableActionClassInfo>();
        }

        public int PurgeCardByName(string targetCardName)
        {
            int totalRemoved = 0;

            foreach (var weaponEntry in playerDeck)
            {
                int count = weaponEntry.weaponDeck.RemoveAll(card => card.ActionClassName == targetCardName);
                totalRemoved += count;
            }

            return totalRemoved;
        }
    }

    public enum PlayerName
    {
        JACKIE,
        IVES,
    }
}


[System.Serializable]
public class PlayerInformation
{
    [field: SerializeField] public PlayerData JackieData { get; set; }
    [field: SerializeField] public PlayerData IvesData { get; set; }

    public PlayerInformation(PlayerData jackieData, PlayerData ivesData)
    {
        JackieData = jackieData;
        IvesData = ivesData;
    }
}



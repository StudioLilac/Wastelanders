using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

namespace Systems.Persistence
{
    [Serializable] public class GameData : ISaveData {
        public string Name;
        public List<ActionData> actionData;
        public GameStateData gameStateData;
        public PlayerInformation playerInformation;
        public BountyStateData bountyStateData;
        public string SaveName => Name;
    }

    [Serializable]
    public class UserPreferences : ISaveData {
        public string Name;
        public AudioPreferences audioPreferences;
        public ScreenShakePreference screenShakePreference;

        public string SaveName => Name;
    }

    public interface ISaveable
    {
        SerializableGuid Id { get; set; }
    }

    public interface IBind<TData> where TData : ISaveable 
    {
        SerializableGuid Id { get; set; }
        void Bind(TData data);
    }

    // Singleton save load manager that shows up in unity
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        private const string SAVE_FILE_NAME = "Wastelanders Save File";
        private const string PREFERENCES_FILE_NAME = "Wastelanders User Preferences File";
        [SerializeField] private GameData gameData;
        [SerializeField] private UserPreferences userPreferences;
        private PlayerDatabase defaultPlayerDatabase;
        private CardDatabase defaultCardDatabase;

        IDataService dataService;

        protected override void Awake()
        {
            base.Awake();

            if (invalid) return; // Invalidate this singleton immediately to prevent rest of Awake() from executing
            
            if (SteamManager.Initialized) {
                dataService = new SteamCloudDataService(new JSonSerializer());
                Debug.Log("[SaveLoadSystem] Using Steam Cloud for saves.");
            }
            else {
                dataService = new FileDataService(new JSonSerializer());
                Debug.Log("[SaveLoadSystem] Steam Manager not initialized, using local file for saves.");
            }
            defaultCardDatabase = Resources.LoadAll<CardDatabase>("").First();
            defaultPlayerDatabase = Resources.LoadAll<PlayerDatabase>("").First(); // Could consider loading by name for better performance
            try
            {
                LoadGame();
            } catch (IOException e)
            {
                Debug.Log(e.Message + " Starting a new game.");
                NewGame();
                SaveGame();
            }

            try
            {
                LoadPreferences();
            }
            catch (IOException e)
            {
                NewUserPreferences();
                SavePreferences();
                Debug.Log(e.Message + " Generating a new user preferences file");
            }
            
            LoadAllInformation();
        }

        private void LoadAllInformation()
        {
            LoadPlayerInformation();
            LoadGameStateInformation();
            LoadBountyStateInformation();
        }
        
        public void LoadCardEvolutionProgress()
        {
            ActionClass[] actions = FindObjectsByType<ActionClass>(FindObjectsSortMode.None);

            foreach (ActionClass actionClass in actions)
            {
                ActionData data = gameData.actionData.FirstOrDefault(it => it.ActionClassName == actionClass.GetType().Name);
                if (data != null) actionClass.Bind(data);
            }
        }

        // Scriptable objects are not saved and loaded like MonoBehaviours are. 
        private void LoadPlayerInformation()
        {
            defaultPlayerDatabase.Bind(gameData.playerInformation);
        }

        public void LoadGameStateInformation()
        {
            Bind<GameStateManager, GameStateData>(gameData.gameStateData);
        }

        public void LoadBountyStateInformation()
        {
            Bind<BountyManager, BountyStateData>(gameData.bountyStateData);
        }

        public UserPreferences GetUserPreferences() => userPreferences;

        void Bind<T, TData>(List<TData> datas) where T: MonoBehaviour, IBind<TData> where TData : ISaveable, new() {
            T[] entities = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (T entity in entities)
            {
                TData data = datas.FirstOrDefault(it => it.Id == entity.Id);
                if (data == null)
                {
                    data = new TData { Id = entity.Id };
                    datas.Add(data);
                }
                entity.Bind(data);
            }
        }

        void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            T entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                if (data == null)
                {
                    data = new TData { Id = entity.Id };
                }
                entity.Bind(data);
            }
        }


        public void NewGame()
        {
            gameData = new GameData
            {
                Name = SAVE_FILE_NAME,
                gameStateData = new GameStateData(),
                bountyStateData = new BountyStateData(),
                actionData = defaultCardDatabase.GetDefaultActionDatas(),
                playerInformation = new PlayerInformation(PlayerDatabase.PlayerData.JACKIE_DEFAULT, PlayerDatabase.PlayerData.IVES_DEFAULT)
            };
        }

        public void SaveGame()
        {
            Debug.Log($"Saving the game, {gameData.SaveName}");
            dataService.Save(gameData);
        }

        private void LoadGame()
        {
            Debug.Log($"Loading the game: {SAVE_FILE_NAME}");
            gameData = dataService.Load<GameData>(SAVE_FILE_NAME);
        }

        private void NewUserPreferences() {
            userPreferences = new UserPreferences
            {
                Name = PREFERENCES_FILE_NAME,
                audioPreferences = new AudioPreferences(),
                screenShakePreference = new ScreenShakePreference(),
            };
        }
        
        public void SavePreferences() 
        {
            Debug.Log($"Saving user preferences: {userPreferences.Name}");
            dataService.Save(userPreferences);
        }
        
        private void LoadPreferences() 
        {
            Debug.Log($"Loading user preferences: {PREFERENCES_FILE_NAME}");
            userPreferences = dataService.Load<UserPreferences>(PREFERENCES_FILE_NAME);
        }
    }
}
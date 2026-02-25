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
        public int SaveVersion;
        public List<ActionData> actionData;
        public GameStateData gameStateData;
        public PlayerInformation playerInformation;
        public BountyStateData bountyStateData;
        public string SaveName => Name;
    }

    [Serializable]
    public class UserPreferences : ISaveData {
        public string Name;
        public int SaveVersion;
        public AudioPreferences audioPreferences;
        public ScreenShakePreference screenShakePreference;
        public bool AutoRollEnabled;

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
    public record GetSaveSystemStatus() : IQuery<SaveSystemStatus?>;

    // Singleton save load manager that shows up in unity
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        private const string SAVE_FILE_NAME = "Wastelanders Save File";
        private const string PREFERENCES_FILE_NAME = "Wastelanders User Preferences File";
        [SerializeField] private GameData gameData;
        [SerializeField] private UserPreferences userPreferences;
        private PlayerDatabase defaultPlayerDatabase;
        private CardDatabase defaultCardDatabase;
        private bool canSaveGame = true;
        private bool canSavePreferences = true;
        private SaveSystemStatus? Status => (canSaveGame, canSavePreferences) switch
        {
            (false, false) => SaveSystemStatus.CriticalError,
            (false, _) => SaveSystemStatus.GameDataError,
            (_, false) => SaveSystemStatus.PreferencesError,
            _ => SaveSystemStatus.Ok
        };

        IDataService dataService;

        protected override void Awake()
        {
            base.Awake();

            if (invalid) return; // Invalidate this singleton immediately to prevent rest of Awake() from executing

            this.Answer<GetSaveSystemStatus, SaveSystemStatus?>(_ => Status);

            if (SteamManager.Initialized)
            {
                dataService = new SteamCloudDataService(new JSonSerializer());
                Debug.Log("[SaveLoadSystem] Using Steam Cloud for saves.");
            }
            else
            {
                dataService = new FileDataService(new JSonSerializer());
                Debug.Log("[SaveLoadSystem] Steam Manager not initialized, using local file for saves.");
            }
            defaultCardDatabase = Resources.LoadAll<CardDatabase>("").First();
            defaultPlayerDatabase = Resources.LoadAll<PlayerDatabase>("").First(); // Could consider loading by name for better performance
            try
            {
                LoadGame();
            }
            catch (FileNotFoundException e)
            {
                Debug.Log(e.Message + " Starting a new game.");
                NewGame();
                SaveGame();
            }
            catch (IOException e)
            {
                Debug.LogError($"An IO exception occurred while loading the game data: {e.Message}. This may indicate a problem with the save file. Starting a new game to prevent crashes.");
                NewGame();
                canSaveGame = false;
            }

            try
            {
                LoadPreferences();
            }
            catch (FileNotFoundException e)
            {
                NewUserPreferences();
                SavePreferences();
                Debug.Log(e.Message + " Generating a new user preferences file");
            }
            catch (IOException e)
            {
                Debug.LogError($"An IO exception occurred while loading the preferences: {e.Message}. This may indicate a problem with the save file. Starting a new game to prevent crashes.");
                NewUserPreferences(); 
                canSavePreferences = false;
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

        void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
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
                SaveVersion = Versioning.CURRENT_GAMEDATA_VERSION,
                gameStateData = new GameStateData(),
                bountyStateData = new BountyStateData(),
                actionData = defaultCardDatabase.GetDefaultActionDatas(),
                playerInformation = new PlayerInformation(PlayerDatabase.PlayerData.JACKIE_DEFAULT, PlayerDatabase.PlayerData.IVES_DEFAULT)
            };
        }

        public void SaveGame()
        {
            if (!canSaveGame)
            {
                Debug.LogWarning("Skipping SaveGame: Game data is locked for this session to prevent overwriting corrupted data.");
                return;
            }

            Debug.Log($"Saving the game, {gameData.SaveName}");
            dataService.Save(gameData);
        }

        private void LoadGame()
        {
            Debug.Log($"Loading the game: {SAVE_FILE_NAME}");
            gameData = Versioning.MigrateGameData(dataService.Load<GameData>(SAVE_FILE_NAME));
        }

        private void NewUserPreferences()
        {
            userPreferences = new UserPreferences
            {
                Name = PREFERENCES_FILE_NAME,
                SaveVersion = Versioning.CURRENT_PREFERENCES_VERSION,
                audioPreferences = new AudioPreferences(),
                screenShakePreference = new ScreenShakePreference(),
                AutoRollEnabled = false
            };
        }

        public void SavePreferences()
        {
            if (!canSavePreferences)
            {
                Debug.LogWarning("Skipping SavePreferences: Preferences file is locked for this session.");
                return;
            }

            Debug.Log($"Saving user preferences: {userPreferences.Name}");
            dataService.Save(userPreferences);
        }

        private void LoadPreferences()
        {
            Debug.Log($"Loading user preferences: {PREFERENCES_FILE_NAME}");
            userPreferences = Versioning.MigratePreferences(dataService.Load<UserPreferences>(PREFERENCES_FILE_NAME));
        }
    }

    public enum SaveSystemStatus
    {
        Ok,
        GameDataError,
        PreferencesError,
        CriticalError // For when both fail
    }
}
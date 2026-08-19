
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

#nullable enable
    public record GetSaveSystemStatus() : IQuery<SaveStatus?>;
    public record GetGameStateData() : IQuery<GameStateData?>;
    public record GetBountyStateData() : IQuery<BountyStateData?>;
    public record GetActionData(string ActionClassName) : IQuery<ActionData?>;

    // Singleton save load manager that shows up in unity
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        private const string SAVE_FILE_NAME = "Wastelanders Save File";
        private const string PREFERENCES_FILE_NAME = "Wastelanders User Preferences File";
        [SerializeField] private GameData gameData = null!;
        [SerializeField] private UserPreferences userPreferences = null!;
        private PlayerDatabase defaultPlayerDatabase = null!;
        private CardDatabase defaultCardDatabase = null!;
        private SaveStatus gameDataStatus = new SaveStatus.Ok();
        private SaveStatus preferencesStatus = new SaveStatus.Ok();
        private SaveStatus? Status => (gameDataStatus, preferencesStatus) switch { 
            (SaveStatus.Error e, _) => e,
            (_, SaveStatus.Error e) => e, 
            _ => new SaveStatus.Ok() 
        };

        IDataService dataService = null!;

        public void Initialize(IDataService mockDataService) => this.dataService = mockDataService;


        protected override void Awake()
        {
            base.Awake();

            if (invalid) return; // Invalidate this singleton immediately to prevent rest of Awake() from executing

            this.Answer<GetSaveSystemStatus, SaveStatus?>(_ => Status);
            this.Answer<GetGameStateData, GameStateData?>(_ => gameData.gameStateData);
            this.Answer<GetBountyStateData, BountyStateData?>(_ => gameData.bountyStateData);
            this.Answer<GetActionData, ActionData?>(q => GetActionDataFor(q.ActionClassName));

            dataService ??= SteamManager.Initialized
                ? new SteamCloudDataService(new JSonSerializer())
                : new FileDataService(new JSonSerializer());

            Debug.Log($"[{nameof(SaveLoadSystem)}] Initialized with {dataService.GetType().Name}.");
            defaultCardDatabase = new GetCardDatabase().Query()
                ?? throw new Exception($"{nameof(CardDatabase)} unavailable — {nameof(DatabaseManager)} must be initialized before {nameof(SaveLoadSystem)}.");
            defaultPlayerDatabase = new GetPlayerDatabase().Query()
                ?? throw new Exception($"{nameof(PlayerDatabase)} unavailable — {nameof(DatabaseManager)} must be initialized before {nameof(SaveLoadSystem)}.");
            LoadAllInformation();
        }

        private void LoadAllInformation()
        {
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
            catch (Exception e) when (e is not NullReferenceException && e is not ArgumentException)
            {
                Debug.LogError($"[SaveLoadSystem] Critical failure loading game data: {e}");
                gameDataStatus = new SaveStatus.Error($"Game Data could not be loaded: {e.Message}");
                NewGame();
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
            catch (Exception e) when (e is not NullReferenceException && e is not ArgumentException)
            {
                Debug.LogError($"[SaveLoadSystem] Critical failure loading preferences: {e}");
                preferencesStatus = new SaveStatus.Error($"Preferences could not be loaded: {e.Message}");
                NewUserPreferences();
            }
            LoadPlayerInformation();
        }

        private Dictionary<string, ActionData>? _actionDataLookup;

        private ActionData GetActionDataFor(string actionClassName)
        {
            if (_actionDataLookup == null)
            {
                _actionDataLookup = new Dictionary<string, ActionData>();
                foreach (ActionData d in gameData.actionData) _actionDataLookup.TryAdd(d.ActionClassName, d);
            }

            if (!_actionDataLookup.TryGetValue(actionClassName, out ActionData data))
            {
                data = new ActionData { ActionClassName = actionClassName };
                gameData.actionData.Add(data);
                _actionDataLookup[actionClassName] = data;
            }
            return data;
        }

        private void LoadPlayerInformation()
        {
            defaultPlayerDatabase.Bind(gameData.playerInformation);
        }

        public UserPreferences GetUserPreferences() => userPreferences;


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
            _actionDataLookup = null;
        }

        public void SaveGame()
        {
            if (gameDataStatus is SaveStatus.Error err)
            {
                Debug.LogWarning($"Skipping SaveGame to prevent overwrite. Lock reason: {err.Message}");
                return;
            }

            Debug.Log($"Saving the game, {gameData.SaveName}");
            dataService.Save(gameData);
            UISaveIndicatorManager.Instance.Show();
        }

        private void LoadGame()
        {
            Debug.Log($"Loading the game: {SAVE_FILE_NAME}");
            gameData = Versioning.MigrateGameData(dataService.Load<GameData>(SAVE_FILE_NAME));
            _actionDataLookup = null;
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
            if (preferencesStatus is SaveStatus.Error err)
            {
                Debug.LogWarning($"Skipping SavePreferences to prevent overwrite. Lock reason: {err.Message}");
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
    public abstract record SaveStatus
    {
        public sealed record Ok : SaveStatus;
        public sealed record Error(string Message) : SaveStatus;
    }
}
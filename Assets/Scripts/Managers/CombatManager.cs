using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Context;
using Systems.Persistence;
using WeaponDeckSerialization;
using UI_Toolkit;

#nullable enable
public record GetGameState() : IQuery<GameState?>; 
public record DefaultCard(PlayerClass player) : IQuery<ClasslessCards?>;
public record GameStateChanged(GameState OldState, GameState NewState): IEvent;
#nullable disable

public record PlayersWin() : IEvent;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private GameState gameState;

    public CinemachineVirtualCamera baseCamera;
    public CinemachineVirtualCamera dynamicCamera;
    private List<EntityClass> playerTeam = new();
    private List<EntityClass> enemyTeam = new();
    private List<EntityClass> neutralTeam = new();

    public GameObject handContainer;
    
    [SerializeField] private PlayerDatabase playerDatabase;
    [SerializeField] private CardDatabase cardDatabase;

    private bool IsDoubleSpeedEnabled { get; set; }
    
    public List<InstantiableActionClassInfo> GetDeck(PlayerDatabase.PlayerName playerName)
    {
        return cardDatabase.GetPrefabInfoForDeck(playerDatabase.GetDeckByPlayerName(playerName));
    }

#nullable enable
    public delegate void GameStateChangedHandler(GameState newState); // Subscribe to this delegate if you want something to be run when gamestate changes
    public static event GameStateChangedHandler? OnGameStateChanged;
    public static event GameStateChangedHandler? OnGameStateChanging;

    public delegate void EntitiesWinLoseDelegate();
    public static event EntitiesWinLoseDelegate? PlayersWinEvent;
    public static event EntitiesWinLoseDelegate? EnemiesWinEvent;


    // Awake is called when the script instance is being loaded
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        this.Answer<GetGameState, GameState?>(_ => GameState);
        this.Subscribe<DoubleSpeedChangedEvent>(SetDoubleSpeed);
    }

    public static void ClearEvents()
    {
        OnGameStateChanged = null;
        OnGameStateChanging = null;
        PlayersWinEvent = null;
        EnemiesWinEvent = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameState = GameState.GAME_START; //Put game start code in the performGameStart method.
        gameObject.AddComponent<ScreenShakeHandler>();
        ActionClass.CardStateChange += HandleCrosshairEnemies;
        this.Answer<DefaultCard, ClasslessCards?>(GetDefaultAction);
        this.Answer<GetActiveCamera, CinemachineVirtualCamera?>(_ => (dynamicCamera.Priority > baseCamera.Priority) ? dynamicCamera : baseCamera);
    }

    private void OnDestroy()
    {
        ActionClass.CardStateChange -= HandleCrosshairEnemies;
        ClearEvents();
    }


    //Sets the Camera Center to the following Entity. 
    public void SetCameraCenter(EntityClass entity)
    {
        dynamicCamera.Follow = entity.transform;
        UpdateCameraBounds();
    }

    //Sets the Camera Bounds to "see more" in the direction that the following entity is facing
    //Usage: Should be called everytime an Entity changes their direction. 
    public void UpdateCameraBounds()
    {
        if (dynamicCamera.Follow?.GetComponent<EntityClass>()?.IsFacingRight() ?? false)
        {
            var transposer = dynamicCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            transposer.m_ScreenX = 0.25f;
        }
        else if (!(dynamicCamera.Follow?.GetComponent<EntityClass>()?.IsFacingRight()) ?? false)
        {
            var transposer = dynamicCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            transposer.m_ScreenX = 0.75f;
        }
    }

    //Allows players to start selection again, resets enemies attacks and position
    private void PerformSelection()
    {
        Activate(handContainer);
        baseCamera.Priority = 1;
        dynamicCamera.Priority = 0;
        PerformInCombat();
        StartCoroutine(FadeCombatBackground(false));

        // Might not capture newly spawned instances of cards, somehow they need to attract their evolved state and data binding. 
        SaveLoadSystem.Instance.LoadCardEvolutionProgress(); // Most universal place to put this is here, but tagged for performance optimizations

        foreach (EntityClass entity in GrabAllEntities())
        {
            entity.PerformSelection();
        }

        Jackie? jackie = playerTeam.OfType<Jackie>().FirstOrDefault();
        if (jackie != null)
        {
            HighlightManager.Instance.SetActivePlayer(jackie);
        }
        else
        {
            PlayerClass? firstPlayer = playerTeam.OfType<PlayerClass>().FirstOrDefault();
            HighlightManager.Instance.SetActivePlayer(firstPlayer);
        }
    }

    private List<EntityClass> GrabAllEntities()
    {
        List<EntityClass> allEntities = new();
        allEntities.AddRange(playerTeam);
        allEntities.AddRange(enemyTeam);
        allEntities.AddRange(neutralTeam);
        return allEntities;
    }

    private void Activate(GameObject gameObject)
    {
        if (gameObject.GetComponent<Collider2D>())
        {
            gameObject.GetComponent<Collider2D>().enabled = true;
        }
        Vector3 position = gameObject.GetComponent<Transform>().position;
        position.z = -1;
        gameObject.GetComponent<Transform>().position = position;
    }

    private void Deactivate(GameObject gameObject)
    {
        if (gameObject.GetComponent<Collider2D>())
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        Vector3 position = gameObject.GetComponent<Transform>().position;
        position.z = -200;
        gameObject.GetComponent<Transform>().position = position;
    }

    public void AddPlayer(EntityClass player)
    {
        playerTeam.Add(player);
    }

    public void AddEnemy(EntityClass enemy)
    {
        enemyTeam.Add(enemy);
    }

    public void AddNeutral(EntityClass neutral)
    {
        neutralTeam.Add(neutral);
    }

    //Purpose: Call this when a player is removed or killed
    public void RemovePlayer(EntityClass player)
    {
        playerTeam.Remove(player);
        if (dynamicCamera.Follow?.GetComponent<EntityClass>() == player)
        {
            dynamicCamera.Follow = null;
        }

        if (playerTeam.Count == 0)
        {
            EnemiesWinEvent?.Invoke();
        }
    }

    //Purpose: Call this when an enemy is removed or killed
    public void RemoveEnemy(EntityClass enemy)
    {
        enemyTeam.Remove(enemy);
        if (dynamicCamera.Follow?.GetComponent<EntityClass>() == enemy)
        {
            dynamicCamera.Follow = null;
        }

        if (enemyTeam.Count == 0)
        {
            PlayersWinEvent?.Invoke();
            new PlayersWin().Invoke();
        }
    }

    public void RemoveNeutral(EntityClass neutral)
    {
        neutralTeam.Remove(neutral);
        if (dynamicCamera.Follow?.GetComponent<EntityClass>() == neutral)
        {
            dynamicCamera.Follow = null;
        }
    }

    private void PerformLose()
    {
        StartCoroutine(FadeCombatBackground(false));
        baseCamera.Priority = 1;
        dynamicCamera.Priority = 0;
        PerformOutOfCombat();
        //Save game after loss too.
        SaveLoadSystem.Instance.SaveGame();
    }

    private void PerformWin()
    {
        StartCoroutine(FadeCombatBackground(false));
        baseCamera.Priority = 1;
        dynamicCamera.Priority = 0;
        PerformOutOfCombat();
        // Save game after each win 
        SaveLoadSystem.Instance.SaveGame();
    }

    private void PerformFighting()
    {
        SoundID.CB_roll_dice.Play();
        Deactivate(handContainer);
        baseCamera.Priority = 0;
        dynamicCamera.Priority = 1;
        StartCoroutine(FadeCombatBackground(true));
    }

    public void BeginCombat()
    {
        if (GameState == GameState.SELECTION || GameState == GameState.FIGHTING) return;


        AudioManager.Instance.StartCombatMusic();
        GameState = GameState.SELECTION;
        
        new UIContextChangedEvent(new UIContext.Combat()).Invoke();
    }

    public void ActivateDynamicCamera()
    {
        baseCamera.Priority = 0;
        dynamicCamera.Priority = 1;
    }

    public void ActivateBaseCamera()
    {
        baseCamera.Priority = 1;
        dynamicCamera.Priority = 0;
    }

    private void PerformGameStart()
    {
        
    }

    // Deprecated functions, please just open a new SpriteFadeHandler instead of borrowing the combat fade screen handler.
    public void SetDarkScreen()
    {
        CombatFadeScreenHandler.Instance.SetDarkScreen();
    }

    public IEnumerator FadeInLightScreen(float duration)
    {
        yield return StartCoroutine(CombatFadeScreenHandler.Instance.FadeInLightScreen(duration));
    }

    public IEnumerator FadeInDarkScreen(float duration)
    {
        yield return StartCoroutine(CombatFadeScreenHandler.Instance.FadeInDarkScreen(duration));
    }

    private void PerformOutOfCombat()
    {
        Deactivate(handContainer);

        foreach (EntityClass entity in GrabAllEntities())
        {
            entity.DeEmphasize();
            entity.OutOfCombat();
            entity.UnTargetable();
        }
        
        new UIContextChangedEvent(new UIContext.Dialogue()).Invoke();
    }

    private void PerformInCombat()
    {

        foreach (EntityClass entity in GrabAllEntities())
        {
            entity.InCombat();
            entity.Targetable();
        }
    }

    public void SetEnemiesPassive(List<EnemyClass> passiveEnemies)
    {
        passiveEnemies.ForEach(enemy =>
        {
            if (enemy.Team == EntityTeam.EnemyTeam)
                enemyTeam.Remove(enemy);
            else if (enemy.Team == EntityTeam.NeutralTeam)
                neutralTeam.Remove(enemy);
        });

        foreach (EnemyClass enemy in passiveEnemies)
        {
            enemy.OutOfCombat();
            enemy.UnTargetable();
        }
    }


    public void SetEnemiesHostile(List<EnemyClass> hostileEnemies)
    {
        hostileEnemies.ForEach(enemy =>
        {
            if (enemy.Team == EntityTeam.EnemyTeam)
                enemyTeam.Add(enemy);
            else if (enemy.Team == EntityTeam.NeutralTeam)
                neutralTeam.Add(enemy);
        });

        foreach (EnemyClass enemy in hostileEnemies)
        {
            enemy.InCombat();
            enemy.Targetable();
        }
    }

    //set (@param darkenScene) true to fade **combat background** in, false to fade out
    private IEnumerator FadeCombatBackground(bool darkenScene)
    {
        float duration = 1f;
        if (darkenScene) 
            yield return StartCoroutine(CombatFadeScreenHandler.Instance.FadeToAlpha(0.8f, duration));
        else
            yield return StartCoroutine(CombatFadeScreenHandler.Instance.FadeInLightScreen(duration));
    }

    private void CrosshairAllEnemies()
    {
        foreach (var enemy in enemyTeam)
        {
            enemy.CrossHair();
        }
    }

    private void UncrosshairAllEnemies()
    {
        foreach (var enemy in enemyTeam)
        {
            enemy.UnCrossHair();
        }
    }

    private void HandleCrosshairEnemies(ActionClass.CardState previousState, ActionClass.CardState nextState)
    {
        if (nextState == ActionClass.CardState.HOVER)
        {
            CrosshairAllEnemies();
        }
        else if (nextState == ActionClass.CardState.NORMAL)
        {
            UncrosshairAllEnemies();
        }
    }

    public bool CanHighlight()
    {
        return (!PauseMenuV2.IsPaused) && GameState == GameState.SELECTION;
    }
    
    private void SetDoubleSpeed(DoubleSpeedChangedEvent e) {
        IsDoubleSpeedEnabled = e.enabled;
        
        if (GameState == GameState.SELECTION || GameState == GameState.FIGHTING) {
            Time.timeScale = e.enabled ? 2f : 1f;
        }
    }
    

    public GameState GameState
    {
        get => gameState;
        set
        {
            var oldState = gameState;
            OnGameStateChanging?.Invoke(value);
            gameState = value;
            if (value == GameState.SELECTION || value == GameState.FIGHTING) {
                Time.timeScale = IsDoubleSpeedEnabled ? 2f : 1f;
            } else {
                Time.timeScale = 1f;
            }
            switch (value)
            {
                case GameState.SELECTION:
                    PerformSelection(); //Gamestate no longer enters selection automatically and requires a scene object to manually start combat. 
                    break;
                case GameState.FIGHTING:
                    PerformFighting();
                    break;
                case GameState.GAME_WIN:
                    Time.timeScale = 1f;
                    PerformWin();
                    break;
                case GameState.GAME_LOSE:
                    PerformLose();
                    break;
                case GameState.GAME_START:
                    PerformGameStart(); //Careful, if you set the game state within these methods you can get strange behaviour
                    break;
                case GameState.OUT_OF_COMBAT:
                    PerformOutOfCombat();
                    break;
                case GameState.AFTER_COMBAT:
                    PerformOutOfCombat();
                    break;
                default:
                    break;
            }
            new GameStateChanged(oldState, value).Invoke();
            OnGameStateChanged?.Invoke(value);
        }
    }

    public List<EntityClass> GetPlayers()
    {
        return new List<EntityClass>(playerTeam);
    }

    public List<EntityClass> GetEnemies() 
    { 
        return new List<EntityClass>(enemyTeam);
    }

    public List<EntityClass> GetNeutral()
    {
        return new List<EntityClass>(neutralTeam);
    }
    
    private ClasslessCards? GetDefaultAction(DefaultCard q) => cardDatabase.GetDefaultAction(q.player);

}

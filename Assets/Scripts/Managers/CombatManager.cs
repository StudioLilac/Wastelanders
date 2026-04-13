using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Context;
using Systems.Persistence;
using WeaponDeckSerialization;
using UI_Toolkit;
using System;

#nullable enable
public record GetGameState() : IQuery<GameState?>;
public record DefaultCard(PlayerClass Player) : IQuery<ClasslessCards?>;
public record GetDeck(PlayerDatabase.PlayerName PlayerName) : IQuery<List<InstantiableActionClassInfo>?>;
public record CanHighlight() : IQuery<bool?>;
public record GetTeammates(EntityTeam Team) : IQuery<List<EntityClass>?>;
public record GetOpponents(EntityTeam Team) : IQuery<List<EntityClass>?>;
public record GameStateChanged(GameState OldState, GameState NewState): IEvent;
#nullable disable

public record PlayersWin() : IEvent;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private GameState gameState;

    private List<EntityClass> playerTeam = new();
    private List<EntityClass> enemyTeam = new();
    private List<EntityClass> neutralTeam = new();

    [SerializeField] private PlayerDatabase playerDatabase;
    [SerializeField] private CardDatabase cardDatabase;
    private List<InstantiableActionClassInfo> GetDeck(PlayerDatabase.PlayerName playerName) => cardDatabase.GetPrefabInfoForDeck(playerDatabase.GetDeckByPlayerName(playerName));

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
        this.Answer<CanHighlight, bool?>(_ => CanHighlight());
        this.Answer<GetTeammates, List<EntityClass>?>(query => HandleGetTeammates(query.Team));
        this.Answer<GetOpponents, List<EntityClass>?>(query => HandleGetOpponents(query.Team));
        this.Answer<DefaultCard, ClasslessCards?>(q => cardDatabase.GetDefaultAction(q.Player));
        this.Answer<GetDeck, List<InstantiableActionClassInfo>?>(q => GetDeck(q.PlayerName));
        this.Subscribe<AddEntityToTeam>(HandleAddEntityToTeam);
        this.Subscribe<RemoveEntityFromTeam>(HandleRemoveEntityFromTeam);
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
        ActionClass.CardStateChange += HandleCrosshairEnemies;
    }

    private void OnDestroy()
    {
        ActionClass.CardStateChange -= HandleCrosshairEnemies;
        ClearEvents();
    }



    //Allows players to start selection again, resets enemies attacks and position
    private void PerformSelection()
    {
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

    private void HandleAddEntityToTeam(AddEntityToTeam evt)
    {
        Action<EntityClass> action = evt.Team switch
        {
            EntityTeam.PlayerTeam => AddPlayer,
            EntityTeam.EnemyTeam => AddEnemy,
            EntityTeam.NeutralTeam => AddNeutral,
            _ => throw new ArgumentOutOfRangeException()
        };
        action(evt.Entity);
    }

    private void HandleRemoveEntityFromTeam(RemoveEntityFromTeam evt)
    {
        Action<EntityClass> action = evt.Team switch
        {
            EntityTeam.PlayerTeam => RemovePlayer,
            EntityTeam.EnemyTeam => RemoveEnemy,
            EntityTeam.NeutralTeam => RemoveNeutral,
            _ => throw new ArgumentOutOfRangeException()
        };
        action(evt.Entity);
    }

    private void AddPlayer(EntityClass player)
    {
        playerTeam.Add(player);
    }

    private void AddEnemy(EntityClass enemy)
    {
        enemyTeam.Add(enemy);
    }

    private void AddNeutral(EntityClass neutral)
    {
        neutralTeam.Add(neutral);
    }

    //Purpose: Call this when a player is removed or killed
    private void RemovePlayer(EntityClass player)
    {
        playerTeam.Remove(player);

        if (playerTeam.Count == 0)
        {
            EnemiesWinEvent?.Invoke();
        }
    }

    //Purpose: Call this when an enemy is removed or killed
    private void RemoveEnemy(EntityClass enemy)
    {
        enemyTeam.Remove(enemy);

        if (enemyTeam.Count == 0)
        {
            PlayersWinEvent?.Invoke();
            new PlayersWin().Invoke();
        }
    }

    private void RemoveNeutral(EntityClass neutral)
    {
        neutralTeam.Remove(neutral);
    }

    private List<EntityClass>? HandleGetTeammates(EntityTeam team)
    {
        return team switch
        {
            EntityTeam.PlayerTeam => new List<EntityClass>(playerTeam),
            EntityTeam.EnemyTeam => new List<EntityClass>(enemyTeam),
            EntityTeam.NeutralTeam => new List<EntityClass>(neutralTeam),
            _ => throw new ArgumentOutOfRangeException("Team possibly not initialized, this is my team: " + team)
        };
    }

    private List<EntityClass>? HandleGetOpponents(EntityTeam team)
    {
        return team switch
        {
            EntityTeam.PlayerTeam => new List<EntityClass>(enemyTeam.Concat(neutralTeam)),
            EntityTeam.EnemyTeam => new List<EntityClass>(playerTeam.Concat(neutralTeam)),
            EntityTeam.NeutralTeam => new(),
            _ => throw new ArgumentOutOfRangeException("Team possibly not initialized, this is my team: " + team)
        };
    }

    private void PerformLose()
    {
        StartCoroutine(FadeCombatBackground(false));
        PerformOutOfCombat();
        //Save game after loss too.
        SaveLoadSystem.Instance.SaveGame();
    }

    private void PerformWin()
    {
        StartCoroutine(FadeCombatBackground(false));
        PerformOutOfCombat();
        // Save game after each win 
        SaveLoadSystem.Instance.SaveGame();
    }

    private void PerformFighting()
    {
        SoundID.CB_roll_dice.Play();
        StartCoroutine(FadeCombatBackground(true));
    }

    public void BeginCombat()
    {
        if (GameState == GameState.SELECTION || GameState == GameState.FIGHTING) return;


        AudioManager.Instance.StartCombatMusic();
        GameState = GameState.SELECTION;
        
        new UIContextChangedEvent(new UIContext.Combat()).Invoke();
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

    private bool CanHighlight()
    {
        return (!PauseMenuV2.IsPaused) && GameState == GameState.SELECTION;
    }

    public GameState GameState
    {
        get => gameState;
        set
        {
            var oldState = gameState;
            OnGameStateChanging?.Invoke(value);
            gameState = value;
            switch (value)
            {
                case GameState.SELECTION:
                    PerformSelection(); //Gamestate no longer enters selection automatically and requires a scene object to manually start combat. 
                    break;
                case GameState.FIGHTING:
                    PerformFighting();
                    break;
                case GameState.GAME_WIN:
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
}

using LevelSelectInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using DialogueScripts;
using Steamworks;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static BattleIntroEnum;
using UI_Toolkit;

public class TutorialIntroduction : DialogueClasses
{
    [SerializeField] private Jackie jackie;
    [SerializeField] private Transform jackieDefaultTransform;
    [SerializeField] private EnemyIves ives;
    [SerializeField] private Transform ivesDefaultTransform;
    [SerializeField] private GameObject trainingDummyPrefab;
    [SerializeField] private Transform dummy1StartingPos;
    [SerializeField] private List<Transform> groupDummySpawnPos;
    [SerializeField] private Transform jackieEndPosition;
    [SerializeField] private Transform ivesPassiveBattlePosition;
    [SerializeField] private BattleBeginButton battleBeginButton;


    [SerializeField] private Sprite laidBackSprite;
    [SerializeField] private Sprite puzzledSoldierSprite;
    [SerializeField] private Image cityBgImage;
    [SerializeField] private Image laidBackImageUI;
    [SerializeField] private Image puzzeledImageUI;
    [SerializeField] private UIFadeHandler backgroundScrim;

    [SerializeField] private List<GameObject> ivesTutorialDeck;
    [SerializeField] private List<GameObject> jackieTutorialDeck;

    [SerializeField] private DialogueEntryWrapper openingDialogue;
    [SerializeField] private DialogueWrapper jackieMonologue;
    [SerializeField] private DialogueEntryWrapper jackieSoloDialogue;
    [SerializeField] private DialogueWrapper soldierGreeting;
    [SerializeField] private DialogueWrapper jackieTalksWithSolider;
    [SerializeField] private DialogueEntryWrapper jackieTalksWithIves;
    [SerializeField] private DialogueWrapper ivesChatsWithJackie;

    //SingleDummyTutorial
    [SerializeField] private DialogueWrapper youCanPlayCardsTutorial;
    [SerializeField] private DialogueWrapper cardFieldsTutorial;
    [SerializeField] private DialogueWrapper queueUpActionsTutorial;
    [SerializeField] private DialogueWrapper duplicateSpeedTutorial;
    [SerializeField] private DialogueWrapper rollingDiceTutorial;
    //Plays after first Dummy killed
    [SerializeField] private DialogueWrapper buffTutorial;
    //After Ives Starts fighting
    [SerializeField] private DialogueWrapper readingOpponentTutorial;
    [SerializeField] private DialogueWrapper clashingCardsTutorial;
    [SerializeField] private DialogueWrapper defensiveCardsTutorial;
    [SerializeField] private DialogueWrapper cardAbilitiesTutorial;
    [SerializeField] private DialogueWrapper clashingOutcomeTutorial;
    [SerializeField] private DialogueWrapper clashingStrategyTutorial;
    [SerializeField] private DialogueWrapper cardsExhaustedTutorial;

    //After Ives is defeated
    [SerializeField] private DialogueWrapper ivesIsDefeated;
    [SerializeField] private DialogueWrapper jackieBeatTheDummies;
    [SerializeField] private DialogueWrapper endingTutorialDialogue;
    [SerializeField] private DialogueEntryWrapper endOfTutorialV2;

    [SerializeField] private DialogueWrapper gameLoseDialogue;

    [SerializeField] private bool jumpToCombat;

    private List<GameObject> trainingDummies = new();

    private float dummiesLeft;



    private const float BRIEF_PAUSE = 0.2f; // For use after an animation to make it visually seem smoother
    private const float MEDIUM_PAUSE = 1f; //For use after a text box comes down and we want to add some weight to the text.

    protected override void GameStateChange(GameState gameState)
    {
        if (gameState == GameState.GAME_START)
        {
            StartCoroutine(ExecuteGameStart());
        }
    }

    public void OnDestroy()
    {
        CombatManager.ClearEvents();
        DialogueBox.ClearDialogueEvents();
        ActionClass.CardHighlightedEvent -= OnPlayerFirstHighlightCard;
        EntityClass.OnEntityDeath -= FirstDummyDies;
        DisplayableClass.OnShowCard -= ExplainDefense;
    }

    private IEnumerator ExecuteGameStart()
    {
        CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
        CombatManager.Instance.SetDarkScreen();
        yield return new WaitForEndOfFrame();
        ives.OutOfCombat();
        jackie.OutOfCombat();
        jackie.SetReturnPosition(jackieDefaultTransform.position);
        if (!jumpToCombat && !GameStateManager.Instance.JumpToCombat)
        {
            yield return StartCoroutine(FadeImage(cityBgImage, 1f, true));

            { 
                yield return StartCoroutine(DialogueBoxV2.Instance.Play(openingDialogue));
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(FadeImage(cityBgImage, 1f, false));
                cityBgImage.gameObject.SetActive(false);
            }

            yield return StartCoroutine(CombatManager.Instance.FadeInLightScreen(1.2f));

           
            yield return StartCoroutine(jackie.MoveToPosition(jackieDefaultTransform.position, 0, 1.2f)); //Jackie Runs into the scene and talks 
            yield return new WaitForSeconds(MEDIUM_PAUSE);
            StartCoroutine(backgroundScrim.FadeToAlpha(0.7f, 1.0f));

            yield return StartCoroutine(DialogueBoxV2.Instance.Play(jackieSoloDialogue));
            yield return new WaitForSeconds(MEDIUM_PAUSE);

            ives.SetReturnPosition(ivesDefaultTransform.position);
            yield return StartCoroutine(ives.MoveToPosition(ivesDefaultTransform.position, 0, 1f)); //Ives comes into the scene
            yield return new WaitForSeconds(0.2f);

            jackie.FaceRight(); //Jackie turns to face the person approaching her

            yield return StartCoroutine(DialogueBoxV2.Instance.Play(jackieTalksWithIves));
            StartCoroutine(backgroundScrim.FadeToAlpha(0f, 1.0f));
            var jackieMove = StartCoroutine(jackie.MoveToPosition(dummy1StartingPos.position, 1.4f, 0.8f));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ives.MoveToPosition(dummy1StartingPos.position, 1.2f, 0.8f)); //Ives goes to place a dummy down
            yield return jackieMove;

            trainingDummies.Add(Instantiate(trainingDummyPrefab, dummy1StartingPos)); //Ives summons Dummy
        } else
        {
            GameStateManager.Instance.JumpToCombat = false;
            //Set up the scene for a combat Jump in.
            ives.SetReturnPosition(ivesDefaultTransform.position);
            StartCoroutine(CombatManager.Instance.FadeInLightScreen(2f));
            yield return StartCoroutine(ives.MoveToPosition(dummy1StartingPos.position, 1.2f, 0.8f)); //Ives goes to place a dummy down
            yield return new WaitForSeconds(BRIEF_PAUSE);
            trainingDummies.Add(Instantiate(trainingDummyPrefab, dummy1StartingPos));
            
        }

        yield return new WaitForSeconds(BRIEF_PAUSE);
        new BattleIntroEvent(Get<TutorialIntro>()).Invoke();
        yield return new WaitForSeconds(1f);

        jackie.InjectDeck(jackieTutorialDeck);
        jackie.InCombat(); //Workaround for now, ill have to remove this once i manually start instantiating 
        ives.SetReturnPosition(ivesPassiveBattlePosition.position);
        StartCoroutine(ives.ResetPosition()); //Prevent Players from attacking Ives LOL
        CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass> { ives });

        DialogueManager.Instance.MoveBoxToTop();
        CombatManager.Instance.BeginCombat();
        
        BeginCombatTutorial();
        yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.GAME_WIN);

        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        DialogueManager.Instance.MoveBoxToTop();
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(buffTutorial.Dialogue));
        
        //Ives retrieves the dead training dummy
        foreach (GameObject trainingDummy in trainingDummies)
        {
            trainingDummy.GetComponent<SpriteRenderer>().sortingOrder -= 1;
            yield return StartCoroutine(ives.MoveToPosition(trainingDummy.transform.position, 0.8f, 0.8f));
            yield return new WaitForSeconds(0.3f);
            Destroy(trainingDummy);
            yield return new WaitForSeconds(0.3f);
        }
        trainingDummies.Clear();

        foreach (Transform pos in groupDummySpawnPos)
        {
            yield return StartCoroutine(ives.MoveToPosition(pos.position, 1.2f, 0.8f));
            yield return new WaitForSeconds(0.2f);
            trainingDummies.Add(Instantiate(trainingDummyPrefab, pos));
            yield return new WaitForSeconds(0.8f);
        }

        StartCoroutine(ives.ResetPosition());

        yield return StartCoroutine(SecondWave());

        var jackieIvesChatter =  StartCoroutine(DialogueBoxV2.Instance.Play(endOfTutorialV2));
        StartCoroutine(backgroundScrim.FadeToAlpha(0.7f, 1.0f));

        yield return new WaitForSeconds(1.0f);
        
        //Ives retrieves the dead training dummy
        foreach (GameObject trainingDummy in trainingDummies)
        {
            trainingDummy.GetComponent<SpriteRenderer>().sortingOrder -= 1;
            yield return StartCoroutine(ives.MoveToPosition(trainingDummy.transform.position, 0.8f, 0.8f));
            yield return new WaitForSeconds(0.3f);
            Destroy(trainingDummy);
            yield return new WaitForSeconds(0.3f);
        }

        yield return jackieIvesChatter;

        AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
        StartCoroutine(CombatManager.Instance.FadeInDarkScreen(3f));
        yield return StartCoroutine(jackie.MoveToPosition(jackieEndPosition.position, 0, 4f));

        GameStateManager.Instance.UpdateLevelProgress(StageInformation.DECK_SELECTION_TUTORIAL);
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.SelectionScreen>().SceneName);
        yield break;
    }

    //Player first sees that they can play cards
    private void BeginCombatTutorial()
    {
        EntityClass.OnEntityDeath += FirstDummyDies; //Setup Listener to set state to Game Win
        battleBeginButton.SelectionSprite.enabled = false;
        HUDV2.Instance.SetDeckInfoVisibility(false);
        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        ActionClass.CardHighlightedEvent += OnPlayerFirstHighlightCard;
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(youCanPlayCardsTutorial.Dialogue));   
    }

    //Once hovering over a card, we talk about speed and power
    private void OnPlayerFirstHighlightCard(ActionClass card)
    {
        ActionClass.CardHighlightedEvent -= OnPlayerFirstHighlightCard;
        StartCoroutine(PlayerFirstHighlightCard(card));
    }

    private IEnumerator PlayerFirstHighlightCard(ActionClass card)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        HighlightManager.Instance.PlayerManuallyInsertedAction += OnPlayerFirstInsertCard;
        StartCoroutine(StartDialogueWithNextEvent(cardFieldsTutorial.Dialogue, () => { }));
    }


    //Once a player targets an enemy, we talk about the queue
    private void OnPlayerFirstInsertCard(ActionClass card)
    {
        HighlightManager.Instance.PlayerManuallyInsertedAction -= OnPlayerFirstInsertCard;
        battleBeginButton.SelectionSprite.enabled = true;
        battleBeginButton.CanStartCombat = false;
        StartCoroutine(PlayerFirstInsertCard(card));
    }

    private IEnumerator PlayerFirstInsertCard(ActionClass card)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        DialogueManager.Instance.MoveBoxToBottom();
        StartCoroutine(StartDialogueWithNextEvent(queueUpActionsTutorial.Dialogue, () => {
            battleBeginButton.CanStartCombat = true;
            CardComparator.Instance.playersAreRollingDiceEvent += OnPlayerFightsDummy;
        }));
    }

    private IEnumerator OnPlayerFightsDummy()
    {
        CardComparator.Instance.playersAreRollingDiceEvent -= OnPlayerFightsDummy;
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(rollingDiceTutorial.Dialogue));
    }

    private void FirstDummyDies(EntityClass entity)
    {
        if (entity is TrainingDummy)
        {
            EntityClass.OnEntityDeath -= FirstDummyDies;
            CombatManager.Instance.GameState = GameState.GAME_WIN;
        }
    }

    // --------------------------------- Reworked Second phase -----------------------------

    private IEnumerator SecondWave()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        CombatManager.Instance.GameState = GameState.SELECTION;
        EntityClass.OnEntityDeath += OnDummyDies;
        dummiesLeft = groupDummySpawnPos.Count;
        HUDV2.Instance.SetDeckInfoVisibility(true);
        DialogueManager.Instance.MoveBoxToBottom();
        StartCoroutine(DialogueManager.Instance.StartDialogue(cardsExhaustedTutorial.Dialogue));

        yield return new WaitUntil(() => dummiesLeft == 0);
        CombatManager.Instance.GameState = GameState.GAME_WIN;
        new OnFinishSparring().Invoke();
        yield return new WaitForSeconds(1f);
    }

    private void OnDummyDies(EntityClass trainingDummy)
    {
        trainingDummy.OutOfCombat();
        trainingDummy.UnTargetable();
        dummiesLeft -= 1;
    }


    //-----------------------------------Ives Fight----------------------------------------

    private void BeginCombatIvesFight()
    {
        CombatManager.PlayersWinEvent += IvesDies; //Setup Listener to set state to Game Win
        CombatManager.EnemiesWinEvent += EnemiesWin;
        CombatManager.OnGameStateChanged += ExplainAbilities;
        StartCoroutine(StartDialogueWithNextEvent(readingOpponentTutorial.Dialogue, () => { HighlightManager.Instance.PlayerManuallyInsertedAction += OnPlayerPlayClashingCard; }));
    }

    private void OnPlayerPlayClashingCard(ActionClass actionClass)
    {
        HighlightManager.Instance.PlayerManuallyInsertedAction -= OnPlayerPlayClashingCard;
        StartCoroutine(StartDialogueWithNextEvent(clashingCardsTutorial.Dialogue, () => { CardComparator.Instance.playersAreRollingDiceEvent += OnPlayerClashingWithIves; }));
    }

    private IEnumerator OnPlayerClashingWithIves()
    {
        DialogueManager.Instance.MoveBoxToBottom();
        CardComparator.Instance.playersAreRollingDiceEvent -= OnPlayerClashingWithIves;
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(clashingOutcomeTutorial.Dialogue));
    }

    private void ExplainAbilities(GameState gameState)
    {
        if (gameState == GameState.SELECTION)
        {
            CombatManager.OnGameStateChanged -= ExplainAbilities;
            StartCoroutine(StartDialogueWithNextEvent(cardAbilitiesTutorial.Dialogue, () => { DisplayableClass.OnShowCard += ExplainDefense; CombatManager.OnGameStateChanged += GiveClashAdvice; }));
        }

    }

    private void ExplainDefense(ActionClass actionClass)
    {
        DisplayableClass.OnShowCard -= ExplainDefense;
        if (actionClass is Brace) StartCoroutine(StartDialogueWithNextEvent(defensiveCardsTutorial.Dialogue, () => { }));
    }

    private void GiveClashAdvice(GameState gameState)
    {
        if (gameState == GameState.SELECTION)
        {
            CombatManager.OnGameStateChanged -= GiveClashAdvice;
            StartCoroutine(StartDialogueWithNextEvent(clashingStrategyTutorial.Dialogue, () => { }));
        }
    }
    private void IvesDies()
    {
        CombatManager.PlayersWinEvent -= IvesDies; //Setup Listener to set state to Game Win
        CombatManager.EnemiesWinEvent -= EnemiesWin;
        CombatManager.Instance.GameState = GameState.GAME_WIN;
    }

    private void EnemiesWin()
    {
        GameLose();
        CombatManager.PlayersWinEvent -= IvesDies;
        CombatManager.EnemiesWinEvent -= EnemiesWin;
        CombatManager.Instance.GameState = GameState.GAME_LOSE;
    }

    private void GameLose()
    {
        GameOver.Instance.FadeInWithDialogue(gameLoseDialogue);
    }
    //------------------------------------------------------Helpers---------------------------------------------------------------------------------

    IEnumerator FadeImage(Image image, float duration, bool fadeIn)
    {
        if (fadeIn)
        {
            // Fade in
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            while (image.color.a < 1.0f)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a + (Time.deltaTime / duration));
                yield return null;
            }
        }
        else
        {
            // Fade out
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            while (image.color.a > 0.0f)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - (Time.deltaTime / duration));
                yield return null;
            }
        }
    }

    //Helper to wait until dialogue is done, then start @param dialogue, then run a callback like setting up a new event. 
    private IEnumerator StartDialogueWithNextEvent(List<DialogueText> dialogue, Action callbackToRun)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(dialogue));
        callbackToRun();
    }

}

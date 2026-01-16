using Cinemachine;
using LevelSelectInformation;
using SceneBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using DialogueScripts;
using UnityEngine;
using UnityEngine.UI;
using static BattleIntroEnum;

public class FrogSlimeFightDialogue : DialogueClasses
{
    [SerializeField] private bool instakill;
    [SerializeField] private bool jumpToCombat;

    [SerializeField] private Jackie jackie;
    [SerializeField] private Transform jackieDefaultTransform;
    [SerializeField] private Transform jackieWander1;
    [SerializeField] private Transform jackieWander2;
    [SerializeField] private Transform jackieWander3;
    [SerializeField] private Transform firstSlimeCrawlPos;
    [SerializeField] private Transform secondSlimeCrawlPos;
    [SerializeField] private Transform thirdSlimeCrawlPos;

    [SerializeField] private Transform jackieFirstFightPosition;


    [SerializeField] private Transform outOfScreen;
    [SerializeField] private Transform treeHidingPositionJackie;
    [SerializeField] private Transform jackieFiresShotTransform;
    [SerializeField] private Transform secondFightBaseCameraPosition;

    [SerializeField] private WasteFrog frog;
    [SerializeField] private Transform frogInitialWalkIn;
    [SerializeField] private Transform frogConfrontPosition;
    [SerializeField] private Transform frogFightPosition;
    [SerializeField] private Transform jackieFightPosition;

    [SerializeField] private WasteFrog frog2;
    [SerializeField] private Transform frog2Battle;
    [SerializeField] private Transform frog2WalkIn;

    [SerializeField] private SlimeStack firstTutorialSlime;
    [SerializeField] private SlimeStack slime;
    [SerializeField] private Transform slimeBattle;
    [SerializeField] private Transform slimeWalkIn;


    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain mainBrain;
    [SerializeField] private GameObject scoutBeetlePrefab;
    [SerializeField] CinemachineVirtualCamera closeUpCamera;
    [SerializeField] CinemachineVirtualCamera startingCamera;
    [SerializeField] CinemachineVirtualCamera baseCamera;
    [SerializeField] CinemachineVirtualCamera dynamicCamera;
    [SerializeField] Image marshBG;
    [SerializeField] ActorProfile ivesProfile;
    [SerializeField] private SpriteRenderer treeOverlay;
    [SerializeField] private List<SpriteFadeHandler> closeTrees;
    [SerializeField] private Sprite frogDeathSprite;
    [SerializeField] private Sprite jackieHoldingCrystalSprite;

    [SerializeField] private DialogueEntryInUnityEditor[] startingNarration;
    [SerializeField] private DialogueEntryInUnityEditor[] testBegins;
    [SerializeField] private List<DialogueText> jackieStrategyPlan;

    [SerializeField] private DialogueEntryWrapper explainEnemyAttacks;
    [SerializeField] private DialogueEntryWrapper explainClashing;
    [SerializeField] private DialogueEntryWrapper duringClashing;
    [SerializeField] private DialogueEntryWrapper afterClashing;


    [SerializeField] private DialogueEntryWrapper afterFirstCombat;

    // After the frog enters the scene
    [SerializeField] private List<DialogueText> andNowWeWait;
    [SerializeField] private List<DialogueText> jackiePreMissedShot;
    [SerializeField] private List<DialogueText> jackiePostMissedShot;
    [SerializeField] private List<DialogueText> jackiePreCombat;
    //Combat Dialogue
    [SerializeField] private List<DialogueText> startOfCombatDialogue;
    // After the frog is defeated
    [SerializeField] private List<DialogueText> afterCombatDialogue;
    [SerializeField] private List<DialogueText> crystalExtraction;
    [SerializeField] private List<DialogueText> beetleEntrance;

    //Game Lose Dialogue
    [SerializeField] private List<DialogueText> gameLoseDialogue;

    private bool playDeadFrog = false;
    private WasteFrog lastKilledFrog;
    private DefaultSceneBuilder sceneBuilder;

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
        EntityClass.OnEntityDeath -= EnsureFrogDeath;
        DialogueBox.DialogueBoxEvent -= OnDialogueBoxEvent;
        HighlightManager.Instance.PlayerManuallyInsertedAction -= OnPlayerPlayClashingCard;
    }
    private void SetUpCombatStatus()
    {
        sceneBuilder = DefaultSceneBuilder.Construct();
        sceneBuilder.PlayersPosition = jackieFightPosition;

        frog.SetReturnPosition(frogFightPosition.position);
        frog2.SetReturnPosition(frog2Battle.position);
        slime.SetReturnPosition(slimeBattle.position);

        jackie.OutOfCombat(); frog.OutOfCombat(); frog2.OutOfCombat(); slime.OutOfCombat();
        frog.UnTargetable(); frog2.UnTargetable(); slime.UnTargetable();
    }

    private IEnumerator ExecuteGameStart()
    {
        CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
        CombatManager.Instance.SetDarkScreen();
        UIFadeScreenManager.Instance.SetDarkScreen();

        yield return new WaitForEndOfFrame();


        SetUpCombatStatus();
        if (!GameStateManager.Instance.JumpToCombat && !jumpToCombat)
        {
            startingCamera.Priority = 2;
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1.0f));
            //Narrate the scene
            yield return StartCoroutine(DialogueBoxV2.Instance.Play(startingNarration.Into()));
            yield return new WaitForSeconds(BRIEF_PAUSE);

            //Ives Talks to the examinees
            yield return new WaitForSeconds(BRIEF_PAUSE);
            yield return StartCoroutine(DialogueBoxV2.Instance.Play(testBegins.Into()));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FadeImage(marshBG, 1f, false));
            marshBG.gameObject.SetActive(false);

            //Jackie walks in the scene
            yield return StartCoroutine(CombatManager.Instance.FadeInLightScreen(1.5f));
            var oldStyle = mainBrain.m_DefaultBlend;
            var longBlend = new CinemachineBlendDefinition(oldStyle.m_Style, 2.0f);
            mainBrain.m_DefaultBlend = longBlend;
            var jackieSprite = jackie.GetComponent<SpriteRenderer>();
            var slimeSprite = firstTutorialSlime.GetComponent<SpriteRenderer>();
            var jackieSpriteLayerName = jackieSprite.sortingLayerName;
            var jackieSpriteSortingOrder = jackieSprite.sortingOrder;
            jackieSprite.sortingLayerName = treeOverlay.sortingLayerName;
            jackieSprite.sortingOrder = treeOverlay.sortingOrder - 1;
            slimeSprite.sortingLayerName = treeOverlay.sortingLayerName;
            slimeSprite.sortingOrder = treeOverlay.sortingOrder - 1;

            yield return StartCoroutine(jackie.MoveToPosition(jackieDefaultTransform.position, 0f, 1.2f));
            yield return new WaitForSeconds(BRIEF_PAUSE);

            //Jackie Wanders around
            {
                Coroutine jackieWander = StartCoroutine(HaveJackieWander());
                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(jackieStrategyPlan));
                yield return jackieWander;
            }

            jackieSprite.sortingLayerName = jackieSpriteLayerName;
            jackieSprite.sortingOrder = jackieSpriteSortingOrder;
            slimeSprite.sortingLayerName = jackieSpriteLayerName;
            slimeSprite.sortingOrder = jackieSpriteSortingOrder;

            {

                new BattleIntroEvent(Get<TutorialIntro>()).Invoke();
                firstTutorialSlime.SetReturnPosition(thirdSlimeCrawlPos.position);
                jackie.SetReturnPosition(jackieFirstFightPosition.position);
                StartCoroutine(jackie.ResetPosition());
                CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass>() { frog, frog2, slime });
                yield return StartCoroutine(firstTutorialSlime.ResetPosition());
                jackie.InCombat(); firstTutorialSlime.InCombat(); firstTutorialSlime.Targetable();

                CombatManager.PlayersWinEvent += PlayersWin;
                CombatManager.EnemiesWinEvent += EnemiesWin;
                CombatManager.Instance.BeginCombat();


                startingCamera.Priority = 0;
                StartCoroutine(BeginClashTutorial());
                yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.GAME_WIN);
                CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            }

            jackieSprite.sortingLayerName = treeOverlay.sortingLayerName;
            jackieSprite.sortingOrder = treeOverlay.sortingOrder - 1;
            slimeSprite.sortingLayerName = treeOverlay.sortingLayerName;
            slimeSprite.sortingOrder = treeOverlay.sortingOrder - 1;

            {
                yield return new WaitForSeconds(2f);
                yield return StartCoroutine(DialogueBoxV2.Instance.Play(afterFirstCombat));
                yield return new WaitForSeconds(0.5f);
            }

            {
                startingCamera.Priority = 0;
                closeUpCamera.Priority = 2;
                mainBrain.m_DefaultBlend =
                    new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 3.0f);
                yield return StartCoroutine(
                    jackie.MoveToPosition(treeHidingPositionJackie.position - new Vector3(1.0f, 0.0f, 0.0f), 0f, 2f));
                jackie.animator.enabled = false; // Allows her to be in her peaking animation. 
                yield return new WaitForSeconds(1f);
            }


            //Jackie Hides behind tree
            {
                jackie.gameObject.transform.position = treeHidingPositionJackie.position;
                jackie.gameObject.transform.rotation = treeHidingPositionJackie.rotation;
                yield return new WaitForSeconds(BRIEF_PAUSE);


                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(andNowWeWait));

                // Frog walks in
                StartCoroutine(frog.MoveToPosition(frogInitialWalkIn.position, 0f, 2f));
                yield return new WaitForSeconds(1.8f);
                yield return StartCoroutine(MoveObjectInRotationDirection(jackie.gameObject, 0.25f, 0.3f));
                yield return new WaitForSeconds(BRIEF_PAUSE);
                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(jackiePreMissedShot));

                //Jackie Misses and frog runs away with jackie chasing it

                yield return new WaitForSeconds(1f);
                jackie.gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                jackie.gameObject.transform.position = treeHidingPositionJackie.position + new Vector3(1f, 0, 0);
                jackie.animator.enabled = true;
                jackie.AttackAnimation(PistolCards.PISTOL_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(PistolCards.PISTOL_SOUND_FX_NAME);

                yield return StartCoroutine(MakeFrogJump(frog, 1f));
                yield return StartCoroutine(frog.MoveToPosition(frogConfrontPosition.position, 0f, 1.2f,
                    outOfScreen.position));
                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(jackiePostMissedShot));

                closeUpCamera.Priority = 0;
                StartCoroutine(jackie.MoveToPosition(frogConfrontPosition.position, 2f, 2f));
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(frog2.MoveToPosition(frog2WalkIn.position, 0f, 2f));
                yield return StartCoroutine(slime.MoveToPosition(slimeWalkIn.position, 0f, 2f));

                frog.FaceLeft();

                yield return new WaitForSeconds(MEDIUM_PAUSE);
            }

            mainBrain.m_DefaultBlend = oldStyle;
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(jackiePreCombat));
        }
        else
        {
            yield return new WaitForSeconds(1f);
            marshBG.gameObject.SetActive(false);
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1.0f));
            GameStateManager.Instance.JumpToCombat = false;
        }





















        
        closeTrees.ForEach(tree => StartCoroutine(tree.FadeInLightScreen(0.8f)));

        // start frog fight
        new BattleIntroEvent(Get<ClashIntro>()).Invoke();
        CombatManager.Instance.SetEnemiesHostile(new List<EnemyClass>() { frog, frog2, slime });
        baseCamera.transform.position = secondFightBaseCameraPosition.position;
        StartCoroutine(jackie.ResetPosition());
        StartCoroutine(frog2.ResetPosition());
        StartCoroutine(slime.ResetPosition());
        frog.SetReturnPosition(frogFightPosition.position);
        yield return StartCoroutine(frog.ResetPosition());
        jackie.InCombat();
        frog.Targetable(); frog.InCombat(); frog2.Targetable(); frog2.InCombat(); slime.Targetable(); slime.InCombat();
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());

        CombatManager.PlayersWinEvent += PlayersWin;
        CombatManager.EnemiesWinEvent += EnemiesWin;
        EntityClass.OnEntityDeath += EnsureFrogDeath;

        CombatManager.Instance.BeginCombat();
        
        //Starting Combat
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(startOfCombatDialogue));
        if (instakill)
        {
            jackie.AddStacks(Accuracy.buffName, 900);
            jackie.AddStacks(Resonate.buffName, 900);
        }
        yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.GAME_WIN);
        CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
        ReviveJackie();
        yield return new WaitForSeconds(MEDIUM_PAUSE);

        //After Combat
        AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(afterCombatDialogue));
        yield return new WaitForSeconds(BRIEF_PAUSE);
        CombatManager.Instance.ActivateDynamicCamera();
        yield return StartCoroutine(jackie.MoveToPosition(lastKilledFrog.transform.position, 1.5f, 1.7f));
        yield return new WaitForSeconds(MEDIUM_PAUSE);
        closeUpCamera.transform.position = CombatManager.Instance.dynamicCamera.transform.position;
        closeUpCamera.m_Lens.OrthographicSize = CombatManager.Instance.dynamicCamera.m_Lens.OrthographicSize;

        //Jackei picks up the crystal
        jackie.animator.enabled = false;
        jackie.transform.rotation = Quaternion.Euler(0, 0, -25);
        yield return new WaitForSeconds(MEDIUM_PAUSE);
        jackie.GetComponent<SpriteRenderer>().sprite = jackieHoldingCrystalSprite;
        yield return new WaitForSeconds(MEDIUM_PAUSE);
        jackie.transform.rotation = Quaternion.identity;
        yield return new WaitForSeconds(BRIEF_PAUSE);
        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(crystalExtraction));

        //Jackie moves off screen
        closeUpCamera.Priority = 2;
        jackie.animator.enabled = true;
        jackie.Emphasize();
        yield return new WaitForSeconds(MEDIUM_PAUSE);
        yield return StartCoroutine(jackie.MoveToPosition(jackie.transform.position + new Vector3(12f, -1f, 0), 0f, 1.5f));
        yield return new WaitForSeconds(MEDIUM_PAUSE);

        yield return StartCoroutine(DialogueManager.Instance.StartDialogue(beetleEntrance));
        yield return new WaitForSeconds(BRIEF_PAUSE);
        //Beetle is spawned in and follows Jackie
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.6f, mainCamera.nearClipPlane));
        GameObject scoutBeetleObj = Instantiate(scoutBeetlePrefab, bottomLeft + new Vector3(-0.1f, 0, 0), Quaternion.identity);
        ScoutBeetle scoutBeetle = scoutBeetleObj.GetComponent<ScoutBeetle>();
        scoutBeetle.OutOfCombat();
        scoutBeetle.UnTargetable();
        yield return new WaitForSeconds(2f);
        StartCoroutine(scoutBeetle.MoveToPosition(jackie.transform.position, 0f, 2.5f));

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(CombatManager.Instance.FadeInDarkScreen(1.5f));

        GameStateManager.Instance.UpdateLevelProgress(StageInformation.BEETLE_STAGE);
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.BeetleFight>().SceneName);
    }
    // In case players decide to bring spawnable enemies and they are the last ones alive.
    private void ReviveJackie()
    {
        if (jackie.IsDead)
        {
            jackie.gameObject.SetActive(true);
            jackie.OutOfCombat();
            sceneBuilder.PlayersPosition = jackieDefaultTransform;
            StartCoroutine(jackie.ResetPosition());
        }
    }
    private int numberOfBroadcasts = 0;
    private void OnDialogueBoxEvent()
    {
        ++numberOfBroadcasts;
    }
    IEnumerator HaveJackieWander()
    {
        DialogueBox.DialogueBoxEvent += OnDialogueBoxEvent;
        //Jackie wanders up
        yield return new WaitUntil(() => numberOfBroadcasts >= 1);
        yield return StartCoroutine(jackie.MoveToPosition(jackieWander1.position, 0f, 1.2f));
        yield return new WaitForSeconds(BRIEF_PAUSE);

        // Slime stack starts following her

        //jackie Shakes her head
        yield return new WaitUntil(() => numberOfBroadcasts >= 2);
        jackie.FaceLeft();
        yield return new WaitForSeconds(0.3f);
        jackie.FaceRight();
        yield return new WaitForSeconds(0.3f);
        jackie.FaceLeft();
        yield return new WaitForSeconds(0.3f);
        jackie.FaceRight();
        yield return new WaitForSeconds(0.3f);
        jackie.FaceLeft();
        var firstWalk = StartCoroutine(firstTutorialSlime.MoveToPosition(firstSlimeCrawlPos.position, 0, 3f));

        //Jackie wanders down
        yield return new WaitUntil(() => numberOfBroadcasts >= 3);
        StopCoroutine(firstWalk);
        var secondtWalk = StartCoroutine(firstTutorialSlime.MoveToPosition(secondSlimeCrawlPos.position, 0, 3f));
        yield return StartCoroutine(jackie.MoveToPosition(jackieWander2.position, 0f, 1f));
        yield return new WaitForSeconds(BRIEF_PAUSE);

        //Jackie wanders to the middle
        yield return new WaitUntil(() => numberOfBroadcasts >= 4);
        StopCoroutine(secondtWalk);
        var thirdWalk = StartCoroutine(firstTutorialSlime.MoveToPosition(thirdSlimeCrawlPos.position, 0, 3f));
        yield return StartCoroutine(jackie.MoveToPosition(jackieWander3.position, 0f, 0.8f));
        yield return new WaitForSeconds(BRIEF_PAUSE);

        //jackie turns left
        yield return new WaitUntil(() => numberOfBroadcasts >= 5);
        StopCoroutine(thirdWalk);
        jackie.FaceRight();
        DialogueBox.DialogueBoxEvent -= OnDialogueBoxEvent;
    }


    IEnumerator BeginClashTutorial()
    {
        yield return StartDialogueWithNextEvent(explainEnemyAttacks, () => { HighlightManager.Instance.PlayerManuallyInsertedAction += OnPlayerPlayClashingCard; });
        yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.FIGHTING);
        //Combat will start and clash phase will happen in between

        //During NEXT Selection phase this dialogue occurs
        yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.SELECTION);
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(afterClashing));
    }
    private void OnPlayerPlayClashingCard(ActionClass actionClass)
    {
        HighlightManager.Instance.PlayerManuallyInsertedAction -= OnPlayerPlayClashingCard;
        StartCoroutine(StartDialogueWithNextEvent(explainClashing, () => { CardComparator.Instance.playersAreRollingDiceEvent += OnPlayerClashingWithSlime; }));
    }

    private IEnumerator OnPlayerClashingWithSlime()
    {
        DialogueManager.Instance.MoveBoxToBottom();
        CardComparator.Instance.playersAreRollingDiceEvent -= OnPlayerClashingWithSlime;
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(duringClashing));
    }


    private IEnumerator StartDialogueWithNextEvent(DialogueEntryWrapper dialogue, Action callbackToRun)
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue());
        yield return StartCoroutine(DialogueBoxV2.Instance.Play(dialogue));
        callbackToRun();
    }



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
    IEnumerator MoveObjectInRotationDirection(GameObject obj, float distance, float duration)
    {
        Vector3 startPosition = obj.transform.position;
        Vector3 endPosition = obj.transform.position + obj.transform.up * distance + obj.transform.right * distance;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the object ends up at the exact end position
        obj.transform.position = endPosition;
    }

    IEnumerator MakeFrogJump(EntityClass origin, float jumpHeight)
    {
        yield return new WaitForSeconds(0.10f);
        if (origin.HasAnimationParameter("IsStaggered"))
        {
            origin.animator.SetBool("IsStaggered", true);
        }
        yield return new WaitForSeconds(0.16f);
        Vector3 originalPosition = origin.myTransform.position;
        origin.myTransform.position = originalPosition + new Vector3(0, jumpHeight, 0);
        yield return new WaitForSeconds(0.28f);
        origin.myTransform.position = originalPosition;
        yield return new WaitForSeconds(0.20f);
        if (origin.HasAnimationParameter("IsStaggered"))
        {
            origin.animator.SetBool("IsStaggered", false);
        }
    }

    private void EnsureFrogDeath(EntityClass entity)
    {
        if (entity is WasteFrog wasteFrog) 
        {
            if (playDeadFrog)
            {
                //Function that overrides death animation to die in the scene
                IEnumerator DieInScene()
                {
                    BattleQueue.BattleQueueInstance.RemoveAllInstancesOfEntity(wasteFrog);
                    wasteFrog.RemoveEntityFromCombat();
                    wasteFrog.animator.enabled = false;
                    wasteFrog.GetComponent<SpriteRenderer>().sprite = frogDeathSprite;
                    wasteFrog.OutOfCombat();
                    wasteFrog.UnTargetable();
                    wasteFrog.combatInfo.gameObject.SetActive(false);
                    wasteFrog.transform.rotation = Quaternion.Euler(0, 0, 75);
                    wasteFrog.DestroyDeck();
                    yield break;
                }

                wasteFrog.DeathHandler = DieInScene;
                lastKilledFrog = wasteFrog;
                EntityClass.OnEntityDeath -= EnsureFrogDeath;
            }
            playDeadFrog = true; //Only change death animation of the second frog
        }
    }

    private void PlayersWin()
    {
        CombatManager.EnemiesWinEvent -= EnemiesWin;
        CombatManager.PlayersWinEvent -= PlayersWin;
        CombatManager.Instance.GameState = GameState.GAME_WIN;
    }

    private void EnemiesWin()
    {
        CombatManager.EnemiesWinEvent -= EnemiesWin;
        CombatManager.PlayersWinEvent -= PlayersWin;
        GameLose();
        CombatManager.Instance.GameState = GameState.GAME_LOSE;
    }
    private void GameLose()
    {
        GameOver.Instance.FadeInWithDialogue(new DialogueWrapper(gameLoseDialogue));
    }
}

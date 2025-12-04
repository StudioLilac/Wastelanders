using LevelSelectInformation;
using SceneBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Systems.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static BattleIntroEnum;

//@author: Andrew
public class PreQueenFight : DialogueClasses
{
    [SerializeField] private Jackie jackie;
    [SerializeField] private Ives ives;
    [SerializeField] private Transform playerCombatTransform;


    //Plan One Parent
    [SerializeField] private GameObject crystal1;
    [SerializeField] private GameObject crystal2;
    [SerializeField] private GameObject crystal3;
    [SerializeField] private GameObject plan1Parent;
    private List<EntityClass> entitiesInPlanOne = new();
    [SerializeField] private Beetle scout1;
    [SerializeField] private Beetle worker1;
    [SerializeField] private Beetle worker2;
    [SerializeField] private Jackie planOneJackie;
    [SerializeField] private Transform jackieShotPosition;
    [SerializeField] private Ives planOneIves;

    //Plan two Parent
    [SerializeField] private Beetle beetlePile1;
    [SerializeField] private Beetle beetlePile2;
    [SerializeField] private Beetle beetlePile3;
    [SerializeField] private Beetle drone1;
    [SerializeField] private Beetle drone2;
    [SerializeField] private Jackie planTwoJackie;
    [SerializeField] private Ives planTwoIves;
    [SerializeField] private GameObject plan2Parent;
    private List<EntityClass> entitiesInPlanTwo = new();

    //Plan 3 Parent
    [SerializeField] private GameObject plan3Parent;
    private List<EntityClass> entitiesInPlanThree = new();
    [SerializeField] private Crystals bugCrystal1;
    [SerializeField] private Crystals bugCrystal2;
    [SerializeField] private Crystals bugCrystal3;


    [SerializeField] private List<Beetle> campBeetles;
    [SerializeField] private List<Crystals> crystals;
    [SerializeField] private Crystals middleBigCrystal;
    [SerializeField] private List<Crystals> bigCrystals;
    [SerializeField] private SpriteRenderer treeOverlay;

    [SerializeField] private QueenBeetle theQueen;
    [SerializeField] private Transform queenTransform;
    [SerializeField] private List<Beetle> queenGuardBeetles;
    [SerializeField] private List<Transform> queenGuardBeetleTransforms;


    [SerializeField] private bool jumpToCombat;
    [SerializeField] private SpriteFadeHandler spriteFadeHandler;
    private DefaultSceneBuilder defaultSceneBuilder;




    [SerializeField] private DialogueWrapper initialPlanByJackie;
    [SerializeField] private DialogueWrapper planOneDialogue;
    [SerializeField] private DialogueWrapper planTwoDialogue;
    [SerializeField] private DialogueWrapper planThreeDialogue;
    [SerializeField] private DialogueWrapper AfterBeetleFightDialogue;
    [SerializeField] private DialogueWrapper CrystalHitDialogue;
    [SerializeField] private DialogueWrapper PreQueenFightDialogue;
    [SerializeField] private DialogueWrapper LastBitDialogue;
    [SerializeField] private DialogueWrapper PostFight;
    [SerializeField] private DialogueWrapper BeetleGainedBuff;
    [SerializeField] private DialogueWrapper gameLoseDialogue;

    private const float BRIEF_PAUSE = 0.2f; // For use after an animation to make it visually seem smoother
    private const float MEDIUM_PAUSE = 1f; //For use after a text box comes down and we want to add some weight to the text.
    
    protected override void GameStateChange(GameState gameState)
    {
        if (gameState == GameState.GAME_START)
        {
            StartCoroutine(ExecuteGameStart());
        }
    }

    private void OnDestroy()
    {
        DialogueBox.ClearDialogueEvents();
        CombatManager.ClearEvents();
        Beetle.OnGainBuffs -= ExplainBeetleBuff;
    }

    private IEnumerator ExecuteGameStart()
    {
        CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
        CombatManager.Instance.SetDarkScreen();

        yield return new WaitForSeconds(0.5f);
        ives.OutOfCombat();
        jackie.OutOfCombat(); //Workaround for now, ill have to remove this once i manually start instantiating players
        defaultSceneBuilder = DefaultSceneBuilder.Construct();
        defaultSceneBuilder.PlayersPosition = playerCombatTransform;

        CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass>(bigCrystals));

        foreach (Crystals c in crystals)
        {   
            c.OutOfCombat();
        }
        foreach (Beetle b in campBeetles)
        {
            b.OutOfCombat();
            if (b.GetType() != typeof(WorkerBeetle))
            {
                b.FaceLeft();
            }
        }
        foreach (Transform entity in plan1Parent.transform)
        {
            EntityClass entityItem = entity.GetComponent<EntityClass>();
           
            entitiesInPlanOne.Add(entityItem);
            entityItem.Emphasize();
            entityItem.OutOfCombat();
            SpriteRenderer spriteRenderer = entity.GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            color.a = 0f; // Set alpha to 0
            spriteRenderer.color = color;


        }

        foreach (Transform entity in plan2Parent.transform)
        {
            EntityClass entityItem = entity.GetComponent<EntityClass>();
            entitiesInPlanTwo.Add(entityItem);
            entityItem.Emphasize();
            entityItem.OutOfCombat();
            SpriteRenderer spriteRenderer = entity.GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            color.a = 0f; // Set alpha to 0
            spriteRenderer.color = color;
        }
        foreach (Transform entity in plan3Parent.transform)
        {
            EntityClass entityItem = entity.GetComponent<EntityClass>();
            entitiesInPlanThree.Add(entityItem);
            entityItem.Emphasize();
            entityItem.OutOfCombat();
            SpriteRenderer spriteRenderer = entity.GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            color.a = 0f; // Set alpha to 0
            spriteRenderer.color = color;
        }



        
        if (!GameStateManager.Instance.JumpToCombat)
        {   
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(initialPlanByJackie.Dialogue));

            //Activate First Plan
            {
                int broadcasts = 0;
                void DialogueBoxListener()
                {
                    ++broadcasts;
                }
                DialogueBox.DialogueBoxEvent += DialogueBoxListener;

                List<Coroutine> coroutines = new List<Coroutine>();

                foreach (EntityClass entity in entitiesInPlanOne)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 0f, 1f, 0.8f));
                    coroutines.Add(coroutine);

                }

                Coroutine dialogueCoroutine = StartCoroutine(DialogueManager.Instance.StartDialogue(planOneDialogue.Dialogue));
                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();


                yield return new WaitUntil(() => broadcasts >= 1);
                yield return StartCoroutine(scout1.MoveToPosition(crystal2.transform.position, 1f, 0.8f));
                yield return new WaitForSeconds(0.2f);


                yield return new WaitUntil(() => broadcasts >= 2);
                Coroutine workerPos = StartCoroutine(worker1.MoveToPosition(crystal1.transform.position, 0.8f, 1.2f));
                yield return StartCoroutine(worker2.MoveToPosition(crystal2.transform.position, 1.3f, 1.2f));
                yield return workerPos;
                yield return new WaitForSeconds(0.2f);

                yield return new WaitUntil(() => broadcasts >= 3);
                Coroutine jackieRun = StartCoroutine(planOneJackie.MoveToPosition(worker1.transform.position, 1.5f, 0.8f));
                yield return StartCoroutine(planOneIves.MoveToPosition(worker2.transform.position, 1.5f, 0.8f));
                yield return jackieRun;

                DialogueBox.DialogueBoxEvent -= DialogueBoxListener;


                yield return new WaitForSeconds(0.2f);


                planOneJackie.AttackAnimation(StaffCards.STAFF_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(StaffCards.STAFF_SOUND_FX_NAME);
                Coroutine runBeetle = StartCoroutine(worker1.StaggerEntities(planOneJackie, worker2, 0.3f));
                yield return new WaitForSeconds(0.2f);
                planOneIves.AttackAnimation(FistCards.FIST_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(FistCards.FIST_SOUND_FX_NAME);
                yield return StartCoroutine(worker2.StaggerEntities(planOneIves, worker1, 0.3f));
                yield return runBeetle;
                DieInScene(worker1);
                DieInScene(worker2);

                Coroutine beetleTriesToRun = StartCoroutine(scout1.MoveToPosition(plan2Parent.transform.position + new Vector3(-2, 0, 0), 0, 1.5f));
                yield return new WaitForSeconds(0.8f);
                yield return StartCoroutine(planOneJackie.MoveToPosition(jackieShotPosition.position, 0f, 0.5f));
                StopCoroutine(beetleTriesToRun);
                planOneJackie.AttackAnimation(PistolCards.PISTOL_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(PistolCards.PISTOL_SOUND_FX_NAME);
                yield return StartCoroutine(scout1.StaggerEntities(planOneJackie, scout1, 0.3f));
                DieInScene(scout1);
                entitiesInPlanOne.Remove(scout1);

                yield return dialogueCoroutine;
                yield return new WaitForSeconds(0.2f);

                planOneIves.animator.enabled = false;
                //Clean up props used this scene
                foreach (EntityClass entity in entitiesInPlanOne)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 1f, 0f, 0.8f));
                    coroutines.Add(coroutine);

                }
                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();

                foreach (EntityClass entity in entitiesInPlanOne)
                {
                    DieInScene(entity);
                    Destroy(entity.gameObject);
                }
                entitiesInPlanOne.Clear();
            }

            //Second Plan
            {
                yield return new WaitForSeconds(1f);

                beetlePile1.animator.enabled = false;
                beetlePile2.animator.enabled = false;
                beetlePile3.animator.enabled = false;

                int broadcasts = 0;
                void DialogueBoxListener()
                {
                    ++broadcasts;
                }
                DialogueBox.DialogueBoxEvent += DialogueBoxListener;

                List<Coroutine> coroutines = new List<Coroutine>();

                foreach (EntityClass entity in entitiesInPlanTwo)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 0f, 1f, 0.8f));
                    coroutines.Add(coroutine);

                }

                Coroutine dialogueCoroutine = StartCoroutine(DialogueManager.Instance.StartDialogue(planTwoDialogue.Dialogue));
                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();

                //Scout carries over one scene
                entitiesInPlanTwo.Add(scout1);


                yield return new WaitUntil(() => broadcasts >= 1);
                yield return StartCoroutine(drone1.MoveToPosition(beetlePile1.transform.position, 1f, 0.8f));
                yield return StartCoroutine(drone2.MoveToPosition(scout1.transform.position, 1f, 0.8f));
                yield return new WaitForSeconds(0.2f);


                yield return new WaitUntil(() => broadcasts >= 2);
                Coroutine jackieRun = StartCoroutine(planTwoIves.MoveToPosition(drone1.transform.position, 1.5f, 0.8f));
                yield return StartCoroutine(planTwoJackie.MoveToPosition(drone2.transform.position, 1.5f, 0.8f));
                yield return jackieRun;

                DialogueBox.DialogueBoxEvent -= DialogueBoxListener;


                yield return new WaitForSeconds(0.2f);


                planTwoJackie.AttackAnimation(StaffCards.STAFF_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(StaffCards.STAFF_SOUND_FX_NAME);
                Coroutine runBeetle = StartCoroutine(drone2.StaggerEntities(planTwoJackie, drone2, 0.3f));
                yield return new WaitForSeconds(0.2f);
                planTwoIves.AttackAnimation(AxeCards.AXE_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(AxeCards.AXE_SOUND_FX_NAME);
                yield return StartCoroutine(drone1.StaggerEntities(planTwoIves, drone1, 0.3f));
                yield return runBeetle;
                DieInScene(drone1);
                DieInScene(drone2);


                yield return new WaitForSeconds(0.8f);

                planTwoIves.animator.enabled = false;

                //Clean up props used this scene
                foreach (EntityClass entity in entitiesInPlanTwo)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 1f, 0f, 0.8f));
                    coroutines.Add(coroutine);
                }
                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();

                foreach (EntityClass entity in entitiesInPlanTwo)
                {
                    DieInScene(entity);
                    Destroy(entity.gameObject);
                }
            }


            //Third Scene 3 
            {
                yield return new WaitForSeconds(0.5f);
                List<Coroutine> coroutines = new List<Coroutine>();

                jackie.Emphasize();
                ives.Emphasize();

                foreach (EntityClass entity in entitiesInPlanThree)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 0f, 1f, 0.8f));
                    coroutines.Add(coroutine);
                }

                foreach (Crystals entity in crystals)
                {
                    entity.Emphasize();
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 0f, 1f, 0.8f));
                    coroutines.Add(coroutine);
                }

                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();


                DialogueManager.Instance.MoveBoxToTop();

                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(planThreeDialogue.Dialogue));
                yield return StartCoroutine(CombatManager.Instance.FadeInLightScreen(0.5f));

                var jackieSprite = jackie.GetComponent<SpriteRenderer>();
                var oldLayer = treeOverlay.sortingLayerName;
                var oldOrder = treeOverlay.sortingOrder;
                treeOverlay.sortingLayerName = jackieSprite.sortingLayerName;
                treeOverlay.sortingOrder = jackieSprite.sortingOrder + 1;

                StartCoroutine(ives.ResetPosition());
                yield return StartCoroutine(jackie.ResetPosition()); //Jackie Runs into the scene
                yield return new WaitForSeconds(MEDIUM_PAUSE);
                Coroutine jackieClash = StartCoroutine(NoCombatClash(jackie, campBeetles[3], false, StaffCards.STAFF_SOUND_FX_NAME));

                yield return new WaitForSeconds(BRIEF_PAUSE);
                yield return StartCoroutine(NoCombatClash(ives, campBeetles[0], false, FistCards.FIST_SOUND_FX_NAME));

                yield return new WaitForSeconds(BRIEF_PAUSE);
                StartCoroutine(NoCombatClash(ives, campBeetles[2], false, FistCards.FIST_SOUND_FX_NAME));

                yield return new WaitForSeconds(BRIEF_PAUSE);
                yield return StartCoroutine(NoCombatClash(jackie, campBeetles[1], false,StaffCards.STAFF_SOUND_FX_NAME));

                yield return new WaitForSeconds(MEDIUM_PAUSE);
                treeOverlay.sortingLayerName = oldLayer;
                treeOverlay.sortingOrder = oldOrder;

                //Clean up props used this scene
                foreach (EntityClass entity in entitiesInPlanThree)
                {
                    Coroutine coroutine = StartCoroutine(FadeSprite(entity.gameObject.GetComponent<SpriteRenderer>(), 1f, 0f, 0.8f));
                    coroutines.Add(coroutine);
                }
                // Wait for all coroutines to finish
                foreach (var cor in coroutines)
                {
                    yield return cor;
                }
                coroutines.Clear();

                foreach (EntityClass entity in entitiesInPlanThree)
                {
                    DieInScene(entity);
                    Destroy(entity.gameObject);
                }
            }

            {
                //At this point, 7 items of crystals are all here, 3 left, 3 right, 1 big
                foreach (Crystals crystal in crystals)
                {
                    crystal.DeEmphasize();
                }
                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(AfterBeetleFightDialogue.Dialogue));
                yield return StartCoroutine(jackie.MoveToPosition(middleBigCrystal.transform.position, 2f, 0.5f));

                jackie.AttackAnimation(StaffCards.STAFF_ANIMATION_NAME);
                AudioManager.Instance?.PlaySFX(StaffCards.STAFF_SOUND_FX_NAME);
                jackie.AddStacks(Resonate.buffName, 1);


                DialogueManager.Instance.MoveBoxToBottom();
                yield return new WaitForSeconds(BRIEF_PAUSE);

                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(CrystalHitDialogue.Dialogue));
            }
        }
        else
        {
            GameStateManager.Instance.JumpToCombat = false;
            jackie.AddStacks(Resonate.buffName, 1);
            CleanUpScene1();
            CleanUpScene2();
            CleanUpScene3();

            yield return StartCoroutine(CombatManager.Instance.FadeInLightScreen(0.5f));
        }
        {
            //Bugs spawn from the crystals
            queenGuardBeetles[0].transform.position = bugCrystal1.transform.position;
            queenGuardBeetles[1].transform.position = bugCrystal2.transform.position;
            queenGuardBeetles[2].transform.position = bugCrystal3.transform.position;
            crystals.Remove(bugCrystal1);
            crystals.Remove(bugCrystal2);
            crystals.Remove(bugCrystal3);
            DieInScene(bugCrystal1);
            DieInScene(bugCrystal2);
            DieInScene(bugCrystal3);
            Destroy(bugCrystal1.gameObject);
            Destroy(bugCrystal2.gameObject);
            Destroy(bugCrystal3.gameObject);

            queenGuardBeetles[0].OutOfCombat();
            queenGuardBeetles[1].OutOfCombat();
            queenGuardBeetles[2].OutOfCombat();

            //Big Crystals and side crystals are now removed. Only crystals in the actual combat remain
            for (int i = 0; i < bigCrystals.Count; i++)
            {
                crystals.Remove(bigCrystals[i]);
            }
            //Shake here TODO
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(PreQueenFightDialogue.Dialogue));
        }

        {
            defaultSceneBuilder.PlayersPosition = playerCombatTransform;
            StartCoroutine(jackie.ResetPosition());
            yield return StartCoroutine(ives.ResetPosition());

            jackie.FaceRight();
            ives.FaceRight();
            theQueen.OutOfCombat();
            theQueen.Emphasize();
            for (int i = 0; i < queenGuardBeetles.Count; i++)
            {
                queenGuardBeetles[i].OutOfCombat();
                queenGuardBeetles[i].transform.position = new Vector3(
                    queenGuardBeetles[i].transform.position.x,
                    queenGuardBeetles[i].transform.position.y, -1);
                StartCoroutine(queenGuardBeetles[i].MoveToPosition(queenGuardBeetleTransforms[i].position, 0f, 1.5f));
            }
            yield return StartCoroutine(theQueen.MoveToPosition(queenTransform.position, 0f, 1.5f));
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(LastBitDialogue.Dialogue));
            
            jackie.DeEmphasize();
            ives.DeEmphasize();
            theQueen.DeEmphasize();

            ives.InCombat();
            jackie.InCombat();
            theQueen.InCombat();
            theQueen.SetReturnPosition(theQueen.transform.position);
            theQueen.IntializeChildBeetles(queenGuardBeetles);
            for (int i = 0; i < queenGuardBeetles.Count; i++)
            {
                queenGuardBeetles[i].InCombat();
                queenGuardBeetles[i].SetReturnPosition(queenGuardBeetles[i].transform.position);
            }
            foreach (Crystals crystal in crystals)
            {
                crystal.InCombat();
            }

            CombatManager.Instance.BeginCombat();
            new BattleIntroEvent(Get<ClashIntro>()).Invoke();
            BeginQueenCombat();
            yield return new WaitUntil(() => CombatManager.Instance.GameState == GameState.GAME_WIN);
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);

            GameStateManager.Instance.FirstTimeFinished = GameStateManager.Instance.CurrentLevelProgress < StageInformation.PRINCESS_FROG_FIGHT.LevelID;
            GameStateManager.Instance.UpdateLevelProgress(StageInformation.PRINCESS_FROG_FIGHT);

            yield return new WaitForSeconds(1.5f);
            DialogueManager.Instance.MoveBoxToBottom();

            yield return StartCoroutine(CombatManager.Instance.FadeInDarkScreen(1.5f));

            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(PostFight.Dialogue));

            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.PostQueenFight>().SceneName);
        }
        
    }
    
    public IEnumerator FadeTMP(TextMeshProUGUI text, float duration)
    {
        Color c = text.color;
        c.a = 0;
        text.color = c;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Clamp01(time / duration);
            text.color = c;
            yield return null;
        }
    }

    void BeginQueenCombat()
    {
        CombatManager.PlayersWinEvent += PlayersWin;
        CombatManager.EnemiesWinEvent += EnemiesWin;
        Beetle.OnGainBuffs += ExplainBeetleBuff;
    }

    private void ExplainBeetleBuff(string buffType, int stacks, Beetle beetle)
    {
        StartCoroutine(ExplainQueen(buffType, stacks, beetle));
    }

    private IEnumerator ExplainQueen(string buffType, int stacks, Beetle beetle)
    {
        if (buffType == Resonate.buffName && stacks > 0)
        {
            yield return new WaitUntil(() => CombatManager.Instance.GameState != GameState.FIGHTING);
            Beetle.OnGainBuffs -= ExplainBeetleBuff;
            StartCoroutine(DialogueManager.Instance.StartDialogue(BeetleGainedBuff.Dialogue));
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
        GameOver.Instance.FadeInWithDialogue(gameLoseDialogue);
    }
    //------helpers------

    // "clashes" both entities, without rolling dice. bool parameter decides who gets staggered
    // this is stolen from card comparator (code duplication!) if there is a better way of doing this pls lmk
    private IEnumerator NoCombatClash(EntityClass e1, EntityClass e2, bool e1GetsHit, string soundEffectName = "")
    {
        EntityClass origin = e1;
        EntityClass target = e2;
        float originRatio = 0.5f;
        float targetRatio = 1f - originRatio;
        Vector3 centeredDistance = (origin.myTransform.position * originRatio + targetRatio * target.myTransform.position);
        float bufferedRadius = 0.25f;
        float duration = 0.6f;

        float xBuffer = CardComparator.X_BUFFER;

        StartCoroutine(origin?.MoveToPosition(HorizontalProjector(centeredDistance, origin.myTransform.position, xBuffer), bufferedRadius, duration, centeredDistance));
        yield return StartCoroutine(target?.MoveToPosition(HorizontalProjector(centeredDistance, target.myTransform.position, xBuffer), bufferedRadius, duration, centeredDistance));
        if (!e1GetsHit)
        {
            e1.AttackAnimation("IsMelee");
            if (soundEffectName != "") AudioManager.Instance?.PlaySFX(soundEffectName);
            yield return StartCoroutine(e2.StaggerEntities(e1, e2, 0.3f));
            e2.RemoveEntityFromCombat();
            yield return StartCoroutine(e2.Die());
        }
    }

    private Vector3 HorizontalProjector(Vector3 centeredDistance, Vector3 currentPosition, float xBuffer)
    {
        Vector3 vectorToCenter = (centeredDistance - currentPosition);

        return vectorToCenter.x > 0 ?
            currentPosition + vectorToCenter - new Vector3(xBuffer, 0f, 0f) :
            currentPosition + vectorToCenter + new Vector3(xBuffer, 0f, 0f);
    }

    //Helpers
    //-----------------------------------------
    //Fade Background that gives you more control over the level of fade
    private IEnumerator FadeSprite(SpriteRenderer sprite, float startAlpha, float endAlpha, float duration)
    {   float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
            yield return null;
        }

    }

    void DieInScene(EntityClass entityClass)
    {
        BattleQueue.BattleQueueInstance.RemoveAllInstancesOfEntity(entityClass);
        if (entityClass is EnemyClass enemy)
        {
            enemy.RemoveEntityFromCombat();
            enemy.DestroyDeck();
        } else if (entityClass is PlayerClass player)
        {
            CombatManager.Instance.RemovePlayer(player);
            player.DestroyDeck();
        }
        entityClass.animator.enabled = false;
        entityClass.GetComponent<SpriteRenderer>().sortingOrder -= 1;
        entityClass.OutOfCombat();
        entityClass.UnTargetable();
        entityClass.combatInfo.gameObject.SetActive(false);
        entityClass.transform.rotation = Quaternion.Euler(0, 0, 75);
    }

    void CleanUpScene2()
    {
        foreach (EntityClass entity in entitiesInPlanTwo)
        {
            DieInScene(entity);
            Destroy(entity.gameObject);
        }
    }

    void CleanUpScene1()
    {
        foreach (EntityClass entity in entitiesInPlanOne)
        {
            DieInScene(entity);
            Destroy(entity.gameObject);
        }
    }

    void CleanUpScene3()
    {
        foreach (EntityClass entity in entitiesInPlanThree)
        {
            DieInScene(entity);
            Destroy(entity.gameObject);
        }
    }
}

using Cards.EnemyCards.FrogCards;
using DialogueScripts;
using Entities;
using LevelSelectInformation;
using SceneBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static BattleIntroEnum;

public class Epilogue_3 : MonoBehaviour
{
    public bool instaKill;
    // BGs
    [SerializeField] private SpriteFadeHandler black;
    [SerializeField] private SpriteFadeHandler purpleFlash;
    [SerializeField] private SpriteFadeHandler car;
    [SerializeField] private UIFadeHandler vignette;
    [SerializeField] private UIFadeHandler afterFightBg;
    [SerializeField] private UIFadeHandler injectionBg;
    [SerializeField] private Sprite injection2;
    [SerializeField] private GameObject tundraBackground;

    [SerializeField] private SpriteRenderer cam;
    [SerializeField] private Beetle beetle;
    [SerializeField] private GameObject jackie;
    [SerializeField] private WasteFrog frog;
    [SerializeField] private GameObject ives;
    [SerializeField] private SlimeStack slime;

    [SerializeField] private Transform offFieldPosition;
    [SerializeField] private GameObject tundra1Bg;
    [SerializeField] private GameObject tundra2Bg;

    public Beetle beetleBrown;
    public Beetle beetleBlue;
    public Beetle beetleGreen;
    public SlimeStack ivesSlime;
    public WasteFrog jackieFrog;
    public PrincessFrog princessFrog;
    public Jackie jackieFighter;
    public Ives ivesFighter;
    public ActionClass jackieAction;
    public ActionClass beetleAction;
    public List<EnemyClass> enemyWorkers;

    public Beetle draggingBeetle;
    public WasteFrog spittingFrog;
    public Crystals spatOutCrystal;
    public Crystals draggedCrystal;

    public List<Crystals> crystals;
    public Crystals toEat;
    public Crystals toEat2;


    public PrincessFrog princessFrogJackie;


    public Transform princessResetPosition;
    public Transform beetleBlueReturnPos;
    public Transform beetleGreenReturnPos;
    public Transform beetleBrownReturnPos;
    public Transform slimeReturnPosition;
    public Transform jackieRunPosition;
    public Transform playerReturnPosition;
    public Transform offscreenWorkerPos;
    public Transform toPlaceCrystal;
    public Transform crystalParent;
    public Transform toDragCrystal;

    public AudioClip tundraAudio;


    // Dialogue
    [SerializeField] private DialogueEntryWrapper Driving;

    [SerializeField] private DialogueEntryWrapper WesternMarsh;
    [SerializeField] private DialogueEntryWrapper WesternMarsh2;
    [SerializeField] private DialogueEntryWrapper WesternMarsh3;
    [SerializeField] private DialogueEntryWrapper HeadingOut;

    [SerializeField] private DialogueEntryWrapper Tundra;

    [SerializeField] private DialogueEntryWrapper BattlePreMoveForward;
    [SerializeField] private DialogueEntryWrapper BattleHalt;
    [SerializeField] private DialogueEntryWrapper BattleStandstill;
    [SerializeField] private DialogueEntryWrapper BattleCome;
    [SerializeField] private DialogueEntryWrapper BattleAttack;

    [SerializeField] private DialogueEntryWrapper BattleJackieHumanForm;
    [SerializeField] private DialogueEntryWrapper BattleIvesStaggeredForm;
    [SerializeField] private DialogueEntryWrapper BattleHelpOthers;
    [SerializeField] private DialogueEntryWrapper GameLoseDialogue;
    [SerializeField] private DialogueEntryWrapper FocusAttackHint;
    [SerializeField] private DialogueEntryWrapper RedirectAttackHint;

    [SerializeField] private DialogueEntryWrapper PostBattleMedical;
    [SerializeField] private DialogueEntryWrapper PostBattleCheckup;
    [SerializeField] private DialogueEntryWrapper PostBattleInjection;
    [SerializeField] private DialogueEntryWrapper PostBattleInjection2;
    [SerializeField] private DialogueEntryWrapper PostBattleBlackvein;
    [SerializeField] private DialogueEntryWrapper PostBattleJackieTransform;
    
    [SerializeField] private DialogueEntryWrapper PrincessDeckUnlocked;

    private List<Beetle> EnemyBeetles => new List<Beetle> { beetleBlue, beetleBrown, beetleGreen };

#nullable enable
    void Start() {
        this.Subscribe<CustomEvent>(CustomEventHandler);
        this.Subscribe<TeamWinEvent>(OnTeamWin);
        StartCoroutine(StartScene());
    }

    void OnTeamWin(TeamWinEvent ev)
    {
        if (ev.Team == EntityTeam.PlayerTeam)
        {
            new SetGameState(GameState.GAME_WIN).Invoke();
        } else
        {
            new SetGameState(GameState.GAME_LOSE).Invoke();
            GameOver.Instance.FadeInWithDialogue(GameLoseDialogue);
        }
    }

    void CustomEventHandler(CustomEvent ev)
    {
        if (ev.EventName == "fade")
        {
            StartCoroutine(black.FadeInLightScreen(1f)); // Strike Team, gather up before you all get settled!
        }
        else if (ev.EventName == "cam_flash") // Like this!
        {
            void Callback()
            {
                cam.gameObject.SetActive(false);
                beetle.gameObject.SetActive(true);
            }
            SoundID.VN_purple_pulse.Play();
            StartCoroutine(PurpleFlash(0.5f, Callback));
        }
        else if (ev.EventName == "klack") // Klackackakkc
        {
            beetle.AttackAnimation(Pincer.PINCER_ANIMATION_NAME);
        } else if (ev.EventName == "injection_half") // Has the serum always been pink?
        {
            injectionBg.Image.sprite = injection2;
        }
    }

    void Setup()
    {
        frog.DestroyDeck(); slime.DestroyDeck(); beetle.DestroyDeck(); ivesSlime.DestroyDeck(); jackieFrog.DestroyDeck(); 
        frog.OutOfCombat(); slime.OutOfCombat(); beetle.OutOfCombat(); ivesSlime.OutOfCombat(); jackieFrog.OutOfCombat();
        beetleBlue.OutOfCombat(); beetleGreen.OutOfCombat(); beetleBrown.OutOfCombat(); princessFrog.OutOfCombat();
        crystals.ForEach(it => it.OutOfCombat()); enemyWorkers.ForEach(it => it.OutOfCombat());

        beetleBlue.SetReturnPosition(beetleBlueReturnPos.position); beetleGreen.SetReturnPosition(beetleGreenReturnPos.position);
        beetleBrown.SetReturnPosition(beetleBrownReturnPos.position); princessFrog.SetReturnPosition(princessResetPosition.position);

        DefaultSceneBuilder.Construct().PlayersPosition = playerReturnPosition;
    }

    private IEnumerator StartScene()
    {
        Setup();
        new SetGameState(GameState.OUT_OF_COMBAT).Invoke();
        black.SetDarkScreen();
        if (!GameStateManager.Instance.JumpToCombat)
        {
            {
                tundraBackground.SetActive(false);
                yield return black.FadeInLightScreen(2f);

                yield return DialogueBoxV2.Instance.Play(Driving);
                AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
                yield return black.FadeInDarkScreen(1f);
            }
            {
                tundra1Bg.SetActive(true);
                tundra2Bg.SetActive(false);
                tundraBackground.SetActive(true);
                StartCoroutine(vignette.FadeToAlpha(155f / 255f, 0f));
                yield return new WaitForSeconds(1f);
                AudioManager.Instance.FadeInBackgroundTrack(1f, tundraAudio, true);
                yield return DialogueBoxV2.Instance.Play(WesternMarsh);
                yield return new WaitForSeconds(1f);
                yield return DialogueBoxV2.Instance.Play(WesternMarsh2);
                yield return DialogueBoxV2.Instance.Play(WesternMarsh3);
                yield return new WaitForSeconds(1f);
                void JackieCallback()
                {
                    jackie.SetActive(false);
                    frog.gameObject.SetActive(true);
                }
                void IvesCallback()
                {
                    ives.SetActive(false);
                    slime.gameObject.SetActive(true);
                }

                SoundID.VN_finger_snap.Play();
                SoundID.VN_purple_pulse.Play();
                yield return PurpleFlash(0.5f, JackieCallback);
                SoundID.VN_finger_snap.Play();
                SoundID.VN_purple_pulse.Play();
                yield return PurpleFlash(0.5f, IvesCallback);

                yield return new WaitForSeconds(0.5f);
                yield return DialogueBoxV2.Instance.Play(HeadingOut);
                yield return new WaitForSeconds(0.5f);

                StartCoroutine(slime.MoveToPosition(offFieldPosition.position, 0f, 2f));
                StartCoroutine(beetle.MoveToPosition(offFieldPosition.position, 0f, 3f));
                StartCoroutine(frog.MoveToPosition(offFieldPosition.position, 0f, 2.5f));
                yield return new WaitForSeconds(1.0f);
                yield return black.FadeInDarkScreen(2f);
            }

            tundra1Bg.SetActive(false);
            tundra2Bg.SetActive(true);
            yield return DialogueBoxV2.Instance.Play(Tundra);
            yield return black.FadeInLightScreen(1f);

            IEnumerator SpittingFrog()
            {
                yield return new WaitForSeconds(0.3f);
                spittingFrog.AttackAnimation(FrogAttacks.FROG_ATTACK_NAME);
                yield return new WaitForSeconds(0.3f);
                spatOutCrystal.gameObject.SetActive(false);
                yield return new WaitForSeconds(1.0f);
                yield return StartCoroutine(spittingFrog.MoveToPosition(toPlaceCrystal.transform.position + new Vector3(1f, 0, 0), 0f, 2.5f));
                spittingFrog.FaceLeft();
                yield return new WaitForSeconds(1.0f);
                spittingFrog.AttackAnimation(FrogAttacks.FROG_ATTACK_NAME);
                yield return new WaitForSeconds(0.3f);
                spatOutCrystal.transform.position = toPlaceCrystal.position;
                spatOutCrystal.gameObject.SetActive(true);
            }

            IEnumerator PrincessFrogEating()
            {
                princessFrog.AttackAnimation(FrogAttacks.PRINCESS_FROG_ATTACK_NAME);
                yield return new WaitForSeconds(0.3f);
                SoundID.CB_excavate.Play();
                toEat.TakeDamage(princessFrog, toEat.Health);
                StartCoroutine(toEat.Die());
                yield return new WaitForSeconds(1.0f);
                yield return StartCoroutine(princessFrog.MoveToPosition(toEat2.transform.position, 1f, 1f));
                yield return new WaitForSeconds(1.0f);
                princessFrog.AttackAnimation(FrogAttacks.PRINCESS_FROG_ATTACK_NAME);
                yield return new WaitForSeconds(0.3f);
                SoundID.CB_excavate.Play();
                toEat2.TakeDamage(princessFrog, toEat2.Health);
                StartCoroutine(toEat2.Die());
                CardComparator.Instance.ClearEvents();
            }

            StartCoroutine(SpittingFrog());
            StartCoroutine(PrincessFrogEating());
            StartCoroutine(draggingBeetle.MoveToPosition(draggingBeetle.transform.position + new Vector3(3, 0, 0), 0, 7f, draggingBeetle.transform.position + new Vector3(-5, 0, 0)));
            StartCoroutine(ivesSlime.MoveToPosition(ivesSlime.transform.position + new Vector3(5f, 0f, 0f), 0f, 2.5f));
            StartCoroutine(beetleGreen.MoveToPosition(beetleGreen.transform.position + new Vector3(5f, 0f, 0f), 0f, 3f));
            StartCoroutine(jackieFrog.MoveToPosition(jackieFrog.transform.position + new Vector3(5f, 0f, 0f), 0f, 2f));
            StartCoroutine(beetleBlue.MoveToPosition(beetleBlue.transform.position + new Vector3(5f, 0f, 0f), 0f, 2.5f));
            StartCoroutine(beetleBrown.MoveToPosition(beetleBrown.transform.position + new Vector3(5f, 0f, 0f), 0f, 2.5f));
            yield return StartCoroutine(beetleGreen.MoveToPosition(beetleGreen.transform.position + new Vector3(5f, 0f, 0f), 0f, 2.5f));

            yield return new WaitForSeconds(1.0f);
            yield return DialogueBoxV2.Instance.Play(BattlePreMoveForward);

            princessFrog.FaceLeft();
            StartCoroutine(PurpleFlash(0.5f));
            yield return DialogueBoxV2.Instance.Play(BattleHalt); // ???: Halt


            yield return DialogueBoxV2.Instance.Play(BattleStandstill); // Dammit! Something's controlling ...

            yield return StartCoroutine(princessFrog.ResetPosition());
            StartCoroutine(PurpleFlash(0.5f));
            yield return DialogueBoxV2.Instance.Play(BattleCome); // ???: Come


            draggedCrystal.transform.SetParent(crystalParent, true);
            foreach (var worker in enemyWorkers)
            {
                StartCoroutine(worker.MoveToPosition(offscreenWorkerPos.position, 0f, 3f));
            }
            StartCoroutine(beetleBlue.MoveToPosition(beetleBlueReturnPos.position, 0f, 3f));
            StartCoroutine(beetleBrown.MoveToPosition(beetleBrownReturnPos.position, 0f, 3f));
            StartCoroutine(beetleGreen.MoveToPosition(beetleGreenReturnPos.position, 0f, 3f));
            var coroutine = StartCoroutine(jackieFrog.MoveToPosition(slimeReturnPosition.position, 0f, 5f));

            ivesFighter.transform.position = ivesSlime.gameObject.transform.position;
            ivesSlime.gameObject.SetActive(false);
            ivesFighter.gameObject.SetActive(true);
            ivesFighter.OutOfCombat();

            yield return DialogueBoxV2.Instance.Play(BattleJackieHumanForm); // Jackie: Ives!

            new ActivateDynamicCameraEvent().Invoke();
            (new GetDynamicCamera().Query())!.m_Lens.OrthographicSize = 3.5f;
            void ResetCamera(GameStateChanged ev)
            {
                if (ev.NewState == GameState.FIGHTING)
                {
                    this.UnSubscribe<GameStateChanged>(ResetCamera);
                    (new GetDynamicCamera().Query())!.m_Lens.OrthographicSize = 3f;
                }
            }
            this.Subscribe<GameStateChanged>(ResetCamera);

            yield return StartCoroutine(ivesFighter.MoveToPosition(jackieRunPosition.position, 0f, 1.3f));
            StopCoroutine(coroutine);

            jackieAction.Target = jackieFrog;
            jackieAction.Origin = ivesFighter;
            jackieAction.Speed = 5;
            yield return StartCoroutine(CardComparator.Instance.OneSidedAttack(new BattleQueue.ActionWrapper(jackieAction)));


            jackieFighter.transform.position = jackieFrog.gameObject.transform.position;
            jackieFrog.gameObject.SetActive(false);
            jackieFighter.gameObject.SetActive(true);
            jackieFighter.SetStaggered(true);
            jackieFighter.OutOfCombat();
            enemyWorkers.ForEach(it => it.DestroyDeck());
            enemyWorkers.ForEach(it => it.gameObject.SetActive(false));
            yield return new WaitForSeconds(1f);
            ivesFighter.FaceRight();
            StartCoroutine(PurpleFlash(0.5f));
            yield return DialogueBoxV2.Instance.Play(BattleAttack);

            var beetleCoroutine = StartCoroutine(beetleBlue.MoveToPosition(ivesFighter.transform.position, 2f, 3f));
            jackieAction.Target = beetleBrown;
            jackieAction.Speed = 3;
            yield return StartCoroutine(CardComparator.Instance.OneSidedAttack(new BattleQueue.ActionWrapper(jackieAction), autoRoll: true));
            yield return new WaitForSeconds(0.5f);
            StopCoroutine(beetleCoroutine);
            jackieAction.Target = beetleBlue;
            jackieAction.Speed = 3;
            var greenCoroutine = StartCoroutine(beetleGreen.MoveToPosition(beetleGreen.transform.position + new Vector3(2, 0, 0), 0f, 3f));
            yield return StartCoroutine(CardComparator.Instance.OneSidedAttack(new BattleQueue.ActionWrapper(jackieAction), autoRoll: true));
            yield return new WaitForSeconds(0.5f);

            StopCoroutine(greenCoroutine);
            beetleAction.Target = ivesFighter;
            beetleAction.Origin = beetleGreen;
            beetleAction.Speed = 4;
            yield return StartCoroutine(CardComparator.Instance.OneSidedAttack(new BattleQueue.ActionWrapper(beetleAction), autoRoll: true));
            BattleQueue.BattleQueueInstance.ClearBattleQueue();
            yield return new WaitForSeconds(1f);
            ivesFighter.DeEmphasize();

            yield return DialogueBoxV2.Instance.Play(BattleIvesStaggeredForm);
            ivesFighter.SetStaggered(false);
            jackieFighter.SetStaggered(false);
            yield return new WaitForSeconds(0.5f);
            yield return DialogueBoxV2.Instance.Play(BattleHelpOthers);
            yield return new WaitForSeconds(0.5f);
        } else
        {
            tundra1Bg.SetActive(false);
            tundra2Bg.SetActive(true);
            beetleBlue.transform.position = beetleBlueReturnPos.position;
            beetleGreen.transform.position = beetleGreenReturnPos.position;
            beetleBrown.transform.position = beetleBrownReturnPos.position;
            princessFrog.transform.position = princessResetPosition.position;
            toEat.gameObject.SetActive(false);
            toEat2.gameObject.SetActive(false);
            ivesSlime.gameObject.SetActive(false);
            jackieFrog.gameObject.SetActive(false);
            ivesFighter.gameObject.SetActive(true);
            jackieFighter.gameObject.SetActive(true);
            StartCoroutine(ivesFighter.ResetPosition());
            StartCoroutine(jackieFighter.ResetPosition());
            spatOutCrystal.transform.position = toPlaceCrystal.position;
            draggedCrystal.transform.SetParent(crystalParent, true);
            draggedCrystal.transform.position = toDragCrystal.position;
            princessFrog.AddStacks(Resonate.buffName, 6);
            GameStateManager.Instance.JumpToCombat = false;
            StartCoroutine(black.FadeInLightScreen(3f));
            yield return new WaitForSeconds(0.1f);
        }


        void CombatPrep()
        {
            enemyWorkers.ForEach(it => it.DestroyDeck());
            enemyWorkers.ForEach(it => it.gameObject.SetActive(false));
            List<EnemyClass> enemies = new() { beetleBlue, beetleGreen, beetleBrown, };
            enemies.ForEach(it => it.DeathHandler = it.PassOut);
            enemies.ForEach(it => AdjustEnemyClass(it));
            princessFrog.OwnedMinions = enemies;
            this.Subscribe<OnBuffsUpdatedEvent>(OnBuffEvent);
            this.Subscribe<CardUsed<GobbleCard>>(OnGobbleUsed);
            if (instaKill)
            {
                jackieFighter.AddStacks(Accuracy.buffName, 900);
                jackieFighter.AddStacks(Resonate.buffName, 900);
                ivesFighter.AddStacks(Accuracy.buffName, 900);
                ivesFighter.AddStacks(Resonate.buffName, 900);
            }
        }
        CombatPrep();
        CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass>() { frog, slime, beetle, ivesSlime, jackieFrog, toEat, toEat2 }.Concat(enemyWorkers).ToList());
        CombatManager.Instance.BeginCombat();
        new BattleIntroEvent(Get<ClashIntro>()).Invoke();

        yield return new WaitUntil(() => new GetGameState().Query() == GameState.GAME_WIN);
        GameStateManager.Instance.UpdateLevelProgress(StageInformation.Get<StageInformation.IvesFinale>());
        new SetGameState(GameState.OUT_OF_COMBAT).Invoke();
        AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);

        yield return new WaitForSeconds(3f);
        if (ivesFighter.IsDead)
        {
            ivesFighter.gameObject.SetActive(true);
            ivesFighter.OutOfCombat();
        }

        if (jackieFighter.IsDead)
        {
            jackieFighter.gameObject.SetActive(true);
            jackieFighter.OutOfCombat();
        }

        StartCoroutine(ivesFighter.ResetPosition());
        StartCoroutine(jackieFighter.ResetPosition());
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(1f);
        yield return black.FadeInDarkScreen(2f);
        yield return StartCoroutine(afterFightBg.FadeInDarkScreen(1f));
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.FadeInBackgroundTrack(2f, tundraAudio, true);
        yield return DialogueBoxV2.Instance.Play(PostBattleMedical);

        EnemyBeetles.ForEach(it =>
        {
            it.gameObject.SetActive(true);
            it.Revive();
            it.OutOfCombat();
            StartCoroutine(it.MoveToPosition(ivesFighter.transform.position, 3f, 1f));
        });
        ivesFighter.SetStaggered(true);

        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleCheckup); 
        yield return injectionBg.FadeInDarkScreen(1f);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleInjection);

        StartCoroutine(injectionBg.FadeInLightScreen(1));
        yield return DialogueBoxV2.Instance.Play(PostBattleInjection2);


        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleBlackvein);
        black.SetLightScreen();
        yield return afterFightBg.FadeInLightScreen(1f);

        void TurnIntoPrincessFrog()
        {
            jackieFighter.gameObject.SetActive(false);
            princessFrogJackie.gameObject.SetActive(true);
            princessFrogJackie.OutOfCombat();
            princessFrogJackie.transform.position = jackieFighter.transform.position;
        }


        yield return new WaitForSeconds(2f);
        yield return PurpleFlash(0.5f, TurnIntoPrincessFrog);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleJackieTransform);
        yield return new WaitForSeconds(1f);

        StartCoroutine(princessFrogJackie.MoveToPosition(princessFrogJackie.transform.position + new Vector3(-7f, 0, 0), 0f, 3f));
        
        yield return StartCoroutine(beetleBrown.MoveToPosition(ivesFighter.transform.position, 0.8f, 1f));
        ivesFighter.transform.SetParent(beetleBrown.transform, true);
        StartCoroutine(beetleBrown.MoveToPosition(ivesFighter.transform.position + new Vector3(-7f, 0f, 0), 0.5f, 2f));
        StartCoroutine(beetleBlue.MoveToPosition(beetleBlue.transform.position + new Vector3(-7f, 0, 0), 0f, 2.5f));
        yield return StartCoroutine(beetleGreen.MoveToPosition(beetleGreen.transform.position + new Vector3(-7f, 0, 0), 0f, 3f));
        yield return black.FadeInDarkScreen(2f);
        yield return DialogueBoxV2.Instance.Play(PrincessDeckUnlocked);
        yield return new WaitForSeconds(1f);


        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
    }

    private void OnBuffEvent(OnBuffsUpdatedEvent ev)
    {
        if (ev.WhoAmI.GetBuffStacks(Resonate.buffName) >= 3 && EnemyBeetles.Contains(ev.WhoAmI))
        {
            this.UnSubscribe<OnBuffsUpdatedEvent>(OnBuffEvent);
            IEnumerator ScheduleDialogue()
            {
                yield return new WaitUntil(() => new GetGameState().Query() != GameState.FIGHTING);
                if (DialogueBoxV2.Instance.IsActive) // If active, wait a cycle to avoid cutting off current dialogue
                {
                    yield return new WaitUntil(() => new GetGameState().Query() != GameState.SELECTION);
                    yield return new WaitUntil(() => new GetGameState().Query() != GameState.FIGHTING);
                }
                StartCoroutine(DialogueBoxV2.Instance.Play(FocusAttackHint));
            }

            StartCoroutine(ScheduleDialogue());
        }
    }

    private void OnGobbleUsed(CardUsed<GobbleCard> ev)
    {
        this.UnSubscribe<CardUsed<GobbleCard>>(OnGobbleUsed);
        StartCoroutine(DialogueBoxV2.Instance.Play(RedirectAttackHint));
    }

    private IEnumerator PurpleFlash(float delay, Action? callback = default) {
        yield return purpleFlash.FadeInDarkScreen(delay);
        callback?.Invoke();
        yield return purpleFlash.FadeInLightScreen(delay);
    }

    private void AdjustEnemyClass(EnemyClass enemyClass)
    {
        enemyClass.TargetingWeights = delegate (EntityClass entity)
        {
            return entity.Team switch
            {
                EntityTeam.PlayerTeam => 40,
                EntityTeam.NeutralTeam => 20,
                _ => 0
            };
        };
    }

    private IEnumerator NoCombatClash(EntityClass e1, EntityClass e2, bool e1GetsHit, SoundID soundEffectName = SoundID.None)
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
            e1.AttackAnimation(FistCards.FIST_ANIMATION_NAME);
            if (soundEffectName != SoundID.None) soundEffectName.Play();
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
}
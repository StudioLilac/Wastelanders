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

public class Epilogue_3 : MonoBehaviour {
    // BGs
    [SerializeField] private SpriteFadeHandler black;
    [SerializeField] private SpriteFadeHandler purpleFlash;
    [SerializeField] private SpriteFadeHandler car;
    [SerializeField] private UIFadeHandler vignette;
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
    public List<EnemyClass> enemyWorkers;

    public Beetle draggingBeetle;
    public WasteFrog spittingFrog;
    public Crystals spatOutCrystal;
    public Crystals draggedCrystal;

    public List<Crystals> crystals;
    public Crystals toEat;
    public Crystals toEat2;


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
            StartCoroutine(ivesSlime.MoveToPosition(ivesSlime.transform.position + new Vector3(5f, 0f, 0f), 0f, 2f));
            StartCoroutine(beetleGreen.MoveToPosition(beetleGreen.transform.position + new Vector3(5f, 0f, 0f), 0f, 3f));
            StartCoroutine(jackieFrog.MoveToPosition(jackieFrog.transform.position + new Vector3(5f, 0f, 0f), 0f, 2.5f));
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
            var coroutine = StartCoroutine(ivesSlime.MoveToPosition(slimeReturnPosition.position, 0f, 5f));

            jackieFighter.transform.position = jackieFrog.gameObject.transform.position;
            jackieFrog.gameObject.SetActive(false);
            jackieFighter.gameObject.SetActive(true);
            jackieFighter.OutOfCombat();

            yield return DialogueBoxV2.Instance.Play(BattleJackieHumanForm); // Jackie: Ives!

            new ActivateDynamicCameraEvent().Invoke();
            yield return StartCoroutine(jackieFighter.MoveToPosition(jackieRunPosition.position, 0f, 1.3f));
            StopCoroutine(coroutine);

            jackieAction.Target = ivesSlime;
            jackieAction.Origin = jackieFighter;
            jackieAction.Speed = 5;
            yield return StartCoroutine(CardComparator.Instance.OneSidedAttack(new BattleQueue.ActionWrapper(jackieAction)));


            ivesFighter.transform.position = ivesSlime.gameObject.transform.position;
            ivesSlime.gameObject.SetActive(false);
            ivesFighter.gameObject.SetActive(true);
            ivesFighter.SetStaggered(true);
            ivesFighter.OutOfCombat();
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(BattleIvesStaggeredForm);
            ivesFighter.SetStaggered(false);
            ivesFighter.Heal(5);
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
            
        }
        CombatPrep();
        CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass>() { frog, slime, beetle, ivesSlime, jackieFrog, toEat, toEat2 }.Concat(enemyWorkers).ToList());
        CombatManager.Instance.BeginCombat();
        new BattleIntroEvent(Get<ClashIntro>()).Invoke();

        yield return new WaitUntil(() => new GetGameState().Query() == GameState.GAME_WIN);
        new SetGameState(GameState.OUT_OF_COMBAT).Invoke();
        yield return new WaitForSeconds(1f);
        if (!ivesFighter.IsDead) ivesFighter.SetStaggered(true);
        yield return new WaitForSeconds(1f);
        yield return black.FadeInDarkScreen(2f);


        // TODO: [Fade into the same background, with the beetles surrounding the princess frog.] 
        // TODO: COMBAT TIME
        // TODO: [Fight scene ends. Fade into fight background again.]
        // TODO: [On the left of the screen, Jackie battle idle, Ives staggered animation.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleMedical); // Jackie: Ives! Are you alright?
        // TODO: [Jackie and Ives battle sprites move to the left. The strike team beetles move to the right.
        //  Purple screen flash into Jackie and Ives dialogue sprites standing together on the left,
        //  and the NPCs on the right.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleCheckup); // Thank you Jackie... for freeing us.
        // TODO: [Display injection frame 1]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleInjection);
        // TODO: [Play injection animation to halfway]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleInjection2);
        // TODO: [Fade into Ives’ in-game sprite. Resonance stacks start to accumulate on her.
        //  Fade back to dialogue sprites.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PostBattleBlackvein);
        // TODO: [Purple flash. Jackie turns into the princess frog.]
        yield return new WaitForSeconds(1f);
        yield return PurpleFlash(0.5f);
        yield return DialogueBoxV2.Instance.Play(PostBattleJackieTransform);
        // TODO: [The rest of the NPC’s re-shift and carry Ives in-game sprite offscreen.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(PrincessDeckUnlocked);

        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
    }

    private void OnBuffEvent(OnBuffsUpdatedEvent ev)
    {
        if (ev.WhoAmI.GetBuffStacks(Resonate.buffName) >= 3 && new List<EnemyClass>() { beetleBlue, beetleBrown, beetleGreen }.Contains(ev.WhoAmI))
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
}
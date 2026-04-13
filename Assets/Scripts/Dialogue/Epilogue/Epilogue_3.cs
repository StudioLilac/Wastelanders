using System;
using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.Serialization;

public class Epilogue_3 : MonoBehaviour {
    // BGs
    [SerializeField] private SpriteFadeHandler black;
    [SerializeField] private SpriteFadeHandler purpleFlash;
    [SerializeField] private SpriteFadeHandler car;
    [SerializeField] private UIFadeHandler vignette;
    [SerializeField] private GameObject tundraBackground;
    [SerializeField] private SpriteFadeHandler battleBackground;

    [SerializeField] private SpriteRenderer cam;
    [SerializeField] private Beetle beetle;

    // Dialogue
    [SerializeField] private DialogueEntryWrapper Driving;

    [SerializeField] private DialogueEntryWrapper WesternMarsh;
    [SerializeField] private DialogueEntryWrapper WesternMarsh2;
    [SerializeField] private DialogueEntryWrapper WesternMarsh3;

    [SerializeField] private DialogueEntryWrapper Tundra;

    [SerializeField] private DialogueEntryWrapper BattlePreMoveForward;
    [SerializeField] private DialogueEntryWrapper BattleHalt;
    [SerializeField] private DialogueEntryWrapper BattleStandstill;
    [SerializeField] private DialogueEntryWrapper BattleCome;
    [SerializeField] private DialogueEntryWrapper BattleJackieHumanForm;
    [SerializeField] private DialogueEntryWrapper BattleIvesStaggeredForm;
    [SerializeField] private DialogueEntryWrapper BattleHelpOthers;

    [SerializeField] private DialogueEntryWrapper PostBattleMedical;
    [SerializeField] private DialogueEntryWrapper PostBattleCheckup;
    [SerializeField] private DialogueEntryWrapper PostBattleInjection;
    [SerializeField] private DialogueEntryWrapper PostBattleInjection2;
    [SerializeField] private DialogueEntryWrapper PostBattleBlackvein;
    [SerializeField] private DialogueEntryWrapper PostBattleJackieTransform;
    
    [SerializeField] private DialogueEntryWrapper PrincessDeckUnlocked;

    
    void Start() {
        this.Subscribe<CustomEvent>(CustomEventHandler);
        StartCoroutine(StartScene());
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
                beetle.OutOfCombat();
            }
            StartCoroutine(PurpleFlash(0.5f, Callback));
        }
        else if (ev.EventName == "klack") // Klackackakkc
        {
            beetle.AttackAnimation(Pincer.PINCER_ANIMATION_NAME);
        }
    }

    private IEnumerator StartScene()
    {
        tundraBackground.SetActive(false);
        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(0f);
        yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);

        yield return DialogueBoxV2.Instance.Play(Driving);
        AudioManager.Instance.FadeOutCurrentBackgroundTrack(2f);
        yield return black.FadeInDarkScreen(1f);
        yield return car.FadeInLightScreen(0f);
        tundraBackground.SetActive(true);
        StartCoroutine(vignette.FadeToAlpha(225f/255f, 0f));
        yield return new WaitForSeconds(1f);

        yield return DialogueBoxV2.Instance.Play(WesternMarsh);
        yield return DialogueBoxV2.Instance.Play(WesternMarsh2);
        // TODO: Audio: Shock and murmurs ripple through the faces of the experienced warriors.
        yield return DialogueBoxV2.Instance.Play(WesternMarsh3);

        // [multiple purples flashes with Waste creatures popping up]
        // TODO: "Waste creatures popping up"
        yield return new WaitForSeconds(2f);
        yield return PurpleFlash(0.5f);
        yield return PurpleFlash(0.5f);
        yield return PurpleFlash(0.5f);

        yield return black.FadeInDarkScreen(2f);

        ////// New scene after this






        yield return black.FadeInLightScreen(2f);
        yield return DialogueBoxV2.Instance.Play(Tundra);

        yield return black.FadeInDarkScreen(2f);
        yield return battleBackground.FadeInDarkScreen(0f);
        yield return black.FadeInLightScreen(2f);

        /*
         TODO:
            [There are crystals and beetles scattered around the place working on mining the crystals.
            Some crystals are being broken actively.]
            [The strike team (Ives = slime, Jackie = frog, everyone else = beetles)
            move their way past crystals then past Waste creatures working on excavating crystals.
            A beetle digs up a crystal, a frog swallows it and brings it somewhere.
            Maybe with a set interval that is strangely organized and robotic.]
         */
        yield return DialogueBoxV2.Instance.Play(BattlePreMoveForward);

        // TODO: [The group moves forward. Three quick purple screen flashes. (flashes below)]
        yield return PurpleFlash(0.5f);
        yield return PurpleFlash(0.5f);
        yield return PurpleFlash(0.5f);
        yield return DialogueBoxV2.Instance.Play(BattleHalt); // ???: Halt
        // TODO: [The strike team immediately comes to a standstill.]
        yield return DialogueBoxV2.Instance.Play(BattleStandstill); // Dammit! Something's controlling ...
        yield return PurpleFlash(0.5f);
        yield return PurpleFlash(0.5f);
        yield return DialogueBoxV2.Instance.Play(BattleCome); // ???: Come
        // TODO:
        //  [The rest of the party besides Jackie begin moving to the right following the new blast.
        //  Jackie changes back to her human form.]
        yield return DialogueBoxV2.Instance.Play(BattleJackieHumanForm); // Jackie: Ives!
        // TODO: [Jackie moves up to Ives and knocks her out of her slime form. She changes into her staggered form.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(BattleIvesStaggeredForm); // Jackie: Ives! Are you alright!?
        // TODO: [The rest of the beetles on the screen all move offscreen at the same time.]
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(BattleHelpOthers);
        // TODO: [Jackie and Ives dash offscreen. Fade to black, 2 seconds.]
        yield return black.FadeInDarkScreen(2f);
        yield return black.FadeInLightScreen(2f);
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

    private IEnumerator PurpleFlash(float delay, Action callback = null) {
        yield return purpleFlash.FadeInDarkScreen(delay);
        callback?.Invoke();
        yield return purpleFlash.FadeInLightScreen(delay);
    }
}
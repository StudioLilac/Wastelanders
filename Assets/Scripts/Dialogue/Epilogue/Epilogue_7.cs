using Cinemachine;
using DialogueScripts;
using LevelSelectInformation;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dialogue.Epilogue
{
    public class Epilogue_7 : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] defaultBackground;
        [SerializeField] private SpriteRenderer[] deadCreatureBackground;
        [SerializeField] private SpriteFadeHandler blackFadeHandler;

        [SerializeField] private SpriteRenderer blackingOutSr1;
        [SerializeField] private SpriteRenderer blackingOutSr2;
        [SerializeField] private SpriteRenderer[] blackScreen; // Used for a black screen, keeping dialogue visible
        [SerializeField] private SpriteRenderer[] wakeUpBackground;
        [SerializeField] private SpriteRenderer[] reversedCaveBackground;
        [SerializeField] private CinemachineVirtualCamera dynamicCamera;
        [SerializeField] private DialogueEntryInUnityEditor[] bonePopping;
        [SerializeField] private DialogueEntryInUnityEditor[] preKadeFadeDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postKadeFadeDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] kadeAndOthersDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] everyoneDialogue;

        [SerializeField] private AudioClip suspenseDrone;

#nullable enable

        private void Awake()
        {
            this.Answer<GetActiveCamera, CinemachineVirtualCamera?>(evt => dynamicCamera);
        }

        private IEnumerator Start()
        {
            ControllableAudioChannel tracker = AudioManager.Instance.CreateChannel(SoundID.VN_ep7_tracker_loop, AudioCategory.Music);
            ControllableAudioChannel drips = AudioManager.Instance.CreateChannel(SoundID.VN_ep7_water_drips, AudioCategory.Music, level: 0f);
            ControllableAudioChannel eerie = AudioManager.Instance.CreateChannel(suspenseDrone, AudioCategory.Music, level: 0.7f);

            tracker.Play();
            drips.Play();

            blackFadeHandler.SetDarkScreen();
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.PreFight(tracker));

            tracker.RestoreTempo(4f);
            StartCoroutine(drips.FadeTo(1f, 2f));
            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return StartCoroutine(blackFadeHandler.FadeInLightScreen(1.5f));
            
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.OutOfTheCave);
            StartCoroutine(tracker.FadeTo(0.1f, 2f));
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.CreatureFight(eerie));
            new ShakeScreen(Intensity: 0.8f).Invoke();

            // [Fade into Dead Creature background with Jackie and creature]
            yield return FadeOutSpriteRenderers(2, defaultBackground);
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.DeadCreature);

            // "the beeping of the device begins to grow louder"
            StartCoroutine(tracker.FadeTo(0.8f, 3f));
            blackingOutSr1.gameObject.SetActive(true);
            StartCoroutine(FadeInSpriteRenderer(1f, blackingOutSr1));
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.BlackingOut1);
            blackingOutSr2.gameObject.SetActive(true);
            yield return FadeInSpriteRenderer(1f, blackingOutSr2);
            yield return DialogueBoxV2.Instance.Play(Epilogue_7_DAC.BlackingOut2);
            StartCoroutine(eerie.FadeTo(0f, 1f));
            StartCoroutine(drips.FadeTo(0f, 2f));
            StartCoroutine(blackFadeHandler.FadeInDarkScreen(1.5f));
            tracker.Dispose();
            eerie.Dispose();
            drips.Dispose();
            yield return tracker.FadeTo(0f, 2f);
            yield return FadeOutSpriteRenderers(2, deadCreatureBackground); // Fade into black screen without hiding dialogue
            blackingOutSr1.gameObject.SetActive(false);
            blackingOutSr2.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f); // Small delay between black screen and next dialogue
            AudioManager.Instance.StartBackgroundTrack();
            SoundID.VN_footsteps.Play();
            yield return DialogueBoxV2.Instance.Play(bonePopping.Into());
            yield return FadeOutSpriteRenderers(2, blackScreen);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(preKadeFadeDialogue.Into());
            yield return DialogueBoxV2.Instance.Play(postKadeFadeDialogue.Into());

            yield return StartCoroutine(blackFadeHandler.FadeInDarkScreen(1f));
            StartCoroutine(FadeOutSpriteRenderers(0f, wakeUpBackground));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(blackFadeHandler.FadeInLightScreen(1f));
            yield return DialogueBoxV2.Instance.Play(kadeAndOthersDialogue.Into());


            yield return StartCoroutine(blackFadeHandler.FadeInDarkScreen(1f));
            yield return FadeOutSpriteRenderers(0.1f, reversedCaveBackground);
            SoundID.VN_footsteps.Play();
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(blackFadeHandler.FadeInLightScreen(1f));
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(everyoneDialogue.Into());
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }



        private IEnumerator FadeInSpriteRenderer(float time, SpriteRenderer sr)
        {
            float curTime = 0;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
            while (curTime < time)
            {
                curTime += Time.deltaTime;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, curTime / time);
                yield return null;
            }
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }

        private IEnumerator FadeOutSpriteRenderers(float time, SpriteRenderer[] spriteRenderers)
        {
            float curTime = 0;
            while (curTime < time)
            {
                curTime += Time.deltaTime;
                foreach (SpriteRenderer sr in spriteRenderers)
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1 - curTime / time);
                yield return null;
            }
            foreach (SpriteRenderer sr in spriteRenderers) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        }
    }

    // Cave creature-fight dialogue authored in code. Split into the beats the controller
    // sequences around (background swaps, screen fades, the blip/drip audio crossfade).
    // Jackie's internal thoughts share her speaker; only narration is italicised.
    public static class Epilogue_7_DAC
    {
        // Jackie navigating the tunnels, up to the moment the blips fade under the water drips.
        public static DialogueAsCode PreFight(ControllableAudioChannel tracker) => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "Yeesh, these tunnels are impossible to navigate.", DialogueSprite.JackRetort)
            .Narrate("<i>The darkness returns the words back to her.")
            .Do(new CallbackEvent(() => tracker.SlowTempo(0.7f, 4f)))
            .Narrate("<i>Jackie continues forward regardless, using the faint glow of the tracker to illuminate her steps.</i>", SoundID.VN_footsteps)
            .Narrate("<i>As she does, the tracker beeps begin to slow.</i>")
            .Line(DialogueCharacter.Jackie, "Guess I took a wrong turn, should have taken a left.", DialogueSprite.JackieNeutralSoft)
            .Narrate("<i>Jackie backtracks and takes the left tunnel. Walking ahead, she enters a wide cavern with debris piled high around her.</i>", SoundID.VN_footsteps);

        public static DialogueAsCode OutOfTheCave => new DialogueAsCode()
        .Narrate("<i>A pale light cracks through the mossy walls.</i>")
        .Narrate("<i>She stops to gather her bearings again, making sure she takes the correct path among the many that lay in front of her.</i>");

        // The fight proper, from the creature stirring in the rubble to its collapse.
        public static DialogueAsCode CreatureFight(ControllableAudioChannel eerie) => new DialogueAsCode()
            .Narrate("<i>The clearing is quiet. Aside from her blips, only the drips of water break the silence.</i>")
            .Do(new CallbackEvent(() => eerie.Play()))
            .Narrate("<i>Then one of the stones lets out a wet, guttural snarl. The rocks around her begin to shift.</i>", SoundID.VN_ep7_dragon_hiss)
            .Line(DialogueCharacter.Jackie, "Crap!", DialogueSprite.JackieSurprisedOpen)
            .Narrate("<i>A massive, lizard-like creature with a scarred eye pounces from the rubble.</i>")
            .Line(DialogueCharacter.Jackie, "Brace!", DialogueSprite.JackieSurprisedClosed)
            .Narrate("<i>Metal clashes against bone.</i>", SoundID.CB_clash_tie)
            .Narrate("<i>Jackie is thrown, slamming hard against the stone wall. Her ribs flaring as the wind is knocked out of her.</i>", SoundID.VN_ep7_jackie_hit_wall)
            .Line(DialogueCharacter.Jackie, "Ugh!", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jackie, "Too heavy. Can't... can’t take it head on.", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jackie, "...Okay. Calm. What did Ives say before? 'Don’t fight its strength...'", DialogueSprite.JackieNeutralSoft)
            .Narrate("<i>The creature closes the distance. Claws primed to reap.</i>", SoundID.VN_ep7_creature_growl)
            .Narrate("<i>Jackie dives under the swing and past its foreleg.</i>")
            .Narrate("<i>But she’s not quite fast enough. A claw catches and tears through her calf.</i>", SoundID.VN_ep7_pant_rip)
            .Line(DialogueCharacter.Jackie, "ACK! My leg...", DialogueSprite.JackieSurprisedOpen)
            .Narrate("<i>She tumbles through the gravel, kicking up dust and trailing red.</i>", SoundID.VN_ep7_gravel_drag)
            .Narrate("<i>Behind her, the beast wheels around, its tail knocking boulders on the wall loose.</i>", SoundID.VN_ep7_jackie_hit_wall)
            .Line(DialogueCharacter.Jackie, "’Don’t fight its strength... Fight its shape.’", DialogueSprite.JackieNeutralSoft)
            .Line(DialogueCharacter.Jackie, "Right. But how, with a busted leg?", DialogueSprite.JackieFocused) // expression is an added choice; the script leaves it unkeyed
            .Narrate("<i>She scrambles back, and steadies herself on stone.</i>", SoundID.VN_ep7_gravel_drag)
            .Narrate("<i>The beast stares down Jackie with its one good eye. Judging the damage and distance.</i>")
            .Narrate("<i>Jackie props herself with the staff. Hot blood pounding in her ears. The glove beeping monotonously in rhythm.</i>")
            .Line(DialogueCharacter.Jackie, "I got it. I just need to time this.", DialogueSprite.JackieFocused)
            .Narrate("<i>Seeing its prey wobble upright, the creature lunges for the kill.</i>")
            .Line(DialogueCharacter.Jackie, "NOW!", DialogueSprite.JackieSurprisedOpen)
            .Narrate("<i>Jackie morphs into the form of the smallest beetle she’s fought.</i>", SoundID.VN_finger_snap)
            .Narrate("<i>The world balloons around her as she clings to the gravel. The creature’s momentum carrying it past her.</i>")
            .Narrate("<i>The entire cave shudders as maw meets stone.</i>", SoundID.VN_ep7_dragon_hit_wall)
            .Narrate("<i>Jackie unshifts, and in a breath, springs up on her good leg. Driving all of her weight behind her staff.</i>")
            .Narrate("<i>The steel tip finds the base of the skull, wedging itself arms-length deep.</i>", SoundID.CB_excavate)
            .Narrate("<i>The beast convulses, then collapses.</i>", SoundID.VN_ep7_dragon_death);

        // Standing over the corpse, wounded, as the beeping starts to swell back up.
        public static DialogueAsCode DeadCreature => new DialogueAsCode()
            .Narrate("<i>Jackie leans on her staff, her head pounding and body trembling. She lets out a wet cough.</i>")
            .Line(DialogueCharacter.Jackie, "That’s... not good.... I should be getting back ...", DialogueSprite.JackieTired)
            .Narrate("<i>She takes a step and her leg buckles.</i>")
            .Line(DialogueCharacter.Jackie, "I’ll have to make sure Cam gets this damn tracker to work while I’m shifted.", DialogueSprite.JackieTired);

        public static DialogueAsCode BlackingOut1 => new DialogueAsCode()
            .Narrate("<i>The dark edges of the cavern begin to close in as her vision tunnels. As the beeping of the device begins to grow louder.</i>")
            .Line(DialogueCharacter.Jackie, "When... I get... back...", DialogueSprite.JackieTired);

        public static DialogueAsCode BlackingOut2 => new DialogueAsCode()
            .Narrate("<i>The cold stone floor rushes up to meet her.</i>")
            .Exit(2f, DialogueCharacter.Jackie);
    }
}
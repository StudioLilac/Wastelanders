using DialogueScripts;
using LevelSelectInformation;
using System.Collections;
using UnityEngine;

public class Epilogue_9 : MonoBehaviour
{
    [SerializeField] private UIFadeHandler blackBg;
    [SerializeField] private UIFadeHandler purpleFlash;
    [SerializeField] private GameObject caveBg;
    [SerializeField] private GameObject tunnelBg;
    [SerializeField] private GameObject tundraBG;

    [SerializeField] private DialogueEntryWrapper Preamble;
    [SerializeField] private DialogueEntryWrapper JayRadio;
    [SerializeField] private DialogueEntryWrapper PostRadio;
    [SerializeField] private DialogueEntryWrapper PreAscent;
    [SerializeField] private DialogueEntryWrapper PostAscent;
    [SerializeField] private DialogueEntryWrapper PostWallFlash;
    [SerializeField] private DialogueEntryWrapper Perimeter;
    [SerializeField] private DialogueEntryWrapper TundraWeiss;
    [SerializeField] private DialogueEntryWrapper PostRipple;
    [SerializeField] private AudioClip evilBackgroundTrack;
    [SerializeField] private AudioClip windBlowing;

    void Start()
    {
        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene()
    {
        yield return blackBg.FadeInDarkScreen(0f);
        yield return DialogueBoxV2.Instance.Play(Preamble);
        yield return blackBg.FadeInLightScreen(2f);

        yield return DialogueBoxV2.Instance.Play(JayRadio);
        AudioManager.Instance.PlaySFX(SoundID.VN_radio_static);
        yield return new WaitForSeconds(3f);

        yield return DialogueBoxV2.Instance.Play(PostRadio);
        yield return DialogueBoxV2.Instance.Play(Epilogue9Dialogue.RockyJackieDiscussion);
        yield return new WaitForSeconds(1f);
        yield return blackBg.FadeInDarkScreen(1f);
        yield return new WaitForSeconds(1f);
        tunnelBg.SetActive(true); caveBg.SetActive(false);
        SoundID.VN_footsteps.Play();

        ControllableAudioChannel eerie = AudioManager.Instance.CreateChannel(evilBackgroundTrack, AudioCategory.Music, level: 0.7f);
        eerie.Play();
        ControllableAudioChannel tracker = AudioManager.Instance.CreateChannel(SoundID.VN_ep7_tracker_loop, AudioCategory.Music);
        tracker.Play();
        yield return new WaitForSeconds(0.5f);
        SoundID.VN_footsteps.Play();
        yield return new WaitForSeconds(1f);
        tracker.SlowTempo(0.7f, 3f);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(Epilogue9Dialogue.PreAscent1);
        SoundID.VN_footsteps.Play();
        yield return new WaitForSeconds(0.7f);
        SoundID.VN_footsteps.Play();
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(Epilogue9Dialogue.PreAscent2);
        SoundID.VN_footsteps.Play();
        yield return new WaitForSeconds(0.3f);
        SoundID.VN_footsteps.Play();
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(Epilogue9Dialogue.PreAscent3);

        tracker.RestoreTempo(4f);
        yield return blackBg.FadeInLightScreen(1f);
        StartCoroutine(tracker.FadeTo(0.3f, 3f));

        yield return DialogueBoxV2.Instance.Play(PostAscent);
        SoundID.VN_purple_pulse.Play();
        yield return purpleFlash.FadeInDarkScreen(0.2f);
        yield return purpleFlash.FadeInLightScreen(3f);
        yield return DialogueBoxV2.Instance.Play(PostWallFlash);

        StartCoroutine(tracker.FadeTo(0f, 1.5f));
        StartCoroutine(eerie.FadeTo(0f, 3.5f));
        AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
        yield return blackBg.FadeInDarkScreen(1.5f);
        yield return new WaitForSeconds(1.5f);
        ControllableAudioChannel wind = AudioManager.Instance.CreateChannel(windBlowing, AudioCategory.Music, level: 0.75f);
        wind.Play();
        tunnelBg.SetActive(false); tundraBG.SetActive(true);
        yield return blackBg.FadeInLightScreen(1.5f);
        yield return DialogueBoxV2.Instance.Play(Perimeter);
        yield return DialogueBoxV2.Instance.Play(TundraWeiss);
        yield return new WaitForSeconds(0.5f);
        SoundID.VN_purple_pulse.Play();
        yield return purpleFlash.FadeInDarkScreen(0.5f);
        yield return purpleFlash.FadeInLightScreen(0.5f);
        yield return new WaitForSeconds(0.5f);
        yield return Epilogue9Dialogue.Blip.Play();

        StartCoroutine(wind.FadeTo(0f, 1.5f));
        yield return blackBg.FadeInDarkScreen(1.5f);
        yield return new WaitForSeconds(0.5f);

        new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
    }

    public static class Epilogue9Dialogue
    {
        public static DialogueAsCode RockyJackieDiscussion => new DialogueAsCode()
            .Enter(DialogueCharacter.Jackie, CharacterActions.SetOffscreenLeft, DialogueSprite.JackieFocused, fadeDuration: 0)
            .Move(DialogueCharacter.Jackie, CharacterActions.SetLeft)
            .Line(DialogueCharacter.Jackie, "Rocky. I'm going.")
            .Move(DialogueCharacter.Rocky, CharacterActions.SetRight)
            .Line(DialogueCharacter.Rocky, "Don't be ridiculous. You can hardly walk.", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Jackie, "I can move just fine. Kade even says getting some movement will help with recovery.", DialogueSprite.JackieWry)
            .Line(DialogueCharacter.Jackie, "Besides I’ve got experience you don't.")
            .Line(DialogueCharacter.Jackie, "This tracker is tuned to the Princess Frog’s frequency. I can track it through these tunnels.")
            .Line(DialogueCharacter.Rocky, "Then give it to Jay. He knows the tunnels better than any of us.", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Jackie, "No. That frequency changes constantly, you need someone who knows how to keep it tuned or you’ll lose it.", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie, "I’ve been tracking this thing for weeks, I know its patterns.")
            .Line(DialogueCharacter.Rocky, "And I said <i>no</i>. We are not having this argument again.")
            .Line(DialogueCharacter.Jackie, "Why? Why won't you let me help?", DialogueSprite.JackieRetort)
            .Line(DialogueCharacter.Jackie, "You’ll have a much smoother time finding your people if I come along.")
            .Line(DialogueCharacter.Rocky, "You want to know why?", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Rocky, "Because you’re exactly like <i>her</i>.")
            .Line(DialogueCharacter.Jackie, "Her?", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie, "Rocky! I'm not like my Ma—", DialogueSprite.JackieRetort)
            .Line(DialogueCharacter.Rocky, "Listen, I’ve worked with your mother for a long time.", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Rocky, "The one thing you get used to hearing are assurances. That with her around, things would turn out alright.")
            .Line(DialogueCharacter.Rocky, "But this came from someone who would spend nights at the strategy table, hedging positions, preparing backup plans.")
            .Line(DialogueCharacter.Rocky, "Even she knew that no matter how strong she was, she could never truly guarantee that everyone would make it home.")
            .Line(DialogueCharacter.Rocky, "And so with those assurances she made, she always had something to prove.")
            .Line(DialogueCharacter.Rocky, "To prove that, perhaps <i>she</i>— could be the one thing in the world you <i>could</i> count on.")
            .Line(DialogueCharacter.Rocky, "...It was noble, but the world doesn’t care.")
            .Line(DialogueCharacter.Rocky, "When the situation got too uncertain for her, she’d order us to stay back so she could deal with it alone.")
            .Line(DialogueCharacter.Rocky, "We could only let her step in. Up until the day she didn’t step back out.")
            .Line(DialogueCharacter.Rocky, "And I am not about to be the one who lets Ailin’s daughter step into the dark to die.")
            .Line(DialogueCharacter.Jackie, "...", DialogueSprite.JackieStern)
            .Line(DialogueCharacter.Jackie, "...You think I don’t know her?")
            .Line(DialogueCharacter.Jackie, "How she'd sugarcoat her missions by calling them ‘business trips’?", DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Jackie, "How she'd promise to be back soon when there was no way of knowing?")
            .Line(DialogueCharacter.Jackie, "How if she really truly wanted to be someone who could be counted on, m-maybe she should have just stayed the fuck home?", DialogueSprite.JackieRetort) //Pained expression 
            .Line(DialogueCharacter.Rocky, "...", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Jackie, "...")
            .Line(DialogueCharacter.Jackie, "I’m not my Ma, Rocky.", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie, "I’m not gonna stand here and say to your face that everything will be fine just because I’m coming along.")
            .Line(DialogueCharacter.Jackie, "I don’t know if we’ll find the frog. I don’t know if your scouts are still alive.")
            .Line(DialogueCharacter.Jackie, "I won’t even say that I’ll be safe.")
            .Line(DialogueCharacter.Jackie, "The only thing I can say is that if you bring me along, you won't be left in the dark.", DialogueSprite.JackieStern)
            .Line(DialogueCharacter.Jackie, "I’ll call out every fork. Good or bad I’ll say it, and we’ll decide where to go based on the readings.")
            .Line(DialogueCharacter.Jackie, "So if you would rather order the only person who knows anything to stay back...")
            .Line(DialogueCharacter.Jackie, "Because you’re uncertain of what I might do.")
            .Line(DialogueCharacter.Jackie, "Won’t that make you like her instead?", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Rocky, "...", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Jackie, "I’d know. It’s why I’ve got this limp.")
            .Line(DialogueCharacter.Jackie, "Kade didn’t ask when she saved my life.", DialogueSprite.JackieNeutralSoft)
            .Line(DialogueCharacter.Jackie, "But I’m asking.")
            .Line(DialogueCharacter.Rocky, "...", DialogueSprite.RockySerious)
            .Enter(DialogueCharacter.Jay, CharacterActions.SetOffscreenRight, DialogueSprite.JaySerious, fadeDuration: 0)
            .Move(DialogueCharacter.Rocky, CharacterActions.SetMiddle)
            .Move(DialogueCharacter.Jay, CharacterActions.SetRight)
            .Line(DialogueCharacter.Jay, "She's right, Cap. Delta knows the tunnels just as well as I do.", DialogueSprite.JaySerious)
            .Line(DialogueCharacter.Jay, "Whatever happened up there, they sure didn’t get lost.")
            .Line(DialogueCharacter.Jay, "We might be a little over our heads on this one.")
            .Line(DialogueCharacter.Rocky, "...Fine.", DialogueSprite.RockySerious)
            .Line(DialogueCharacter.Rocky, "But you stay in the center formation and navigate with Kade. Jay, you're on point with me.")
            .Line(DialogueCharacter.Rocky, "If I say retreat, we retreat. No heroics. Capiche?")
            .Line(DialogueCharacter.Jackie, "Capiche.", DialogueSprite.JackieSmile)
            .Exit(DialogueCharacter.Jackie, DialogueCharacter.Jay, DialogueCharacter.Rocky);
        public static DialogueAsCode PreAscent1 => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "Signal’s changed, it’ll thin out if we continue forward.")
            .Line(DialogueCharacter.Rocky, "We’ll double back.");
        public static DialogueAsCode PreAscent2 => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "Signal is strongest on the left.")
            .Line(DialogueCharacter.Rocky, "We’ll go left. ");
        public static DialogueAsCode PreAscent3 => new DialogueAsCode()
            .Narrate("<i>The Wastelanders make quick pace through the tunnels. Jackie yells the readings, Rocky makes the call in front.</i>")
            .Narrate("<i>Despite taking detours and backtracking, they never hit a dead end.</i>");

        public static DialogueAsCode Blip => new DialogueAsCode()
            .Line(DialogueCharacter.Ives, "<i>If she’s using the tracker, she’ll see this blip.</i>")
            .Line(DialogueCharacter.Ives, "<i>You’re favourite game kid. Ready or not kid, here I come. </i>");
    }

}

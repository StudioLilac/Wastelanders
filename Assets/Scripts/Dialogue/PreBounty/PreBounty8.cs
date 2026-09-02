using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.PreBounty
{
    public class PreBounty8 : MonoBehaviour
    {
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue0;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue1;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue2;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue3;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue4;
        [SerializeField] private DialogueEntryInUnityEditor[] postDialogue4;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue5;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue6;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue7;
        [SerializeField] private DialogueEntryInUnityEditor[] dialogue8;
        [SerializeField] private GameObject background1, background2, background3;
        [SerializeField] private Image caveFlickerLayer, splitScreenLayer, ailinRevealLayer;
        [SerializeField] private UIFadeHandler scrim;

        private IEnumerator Start()
        {
            scrim.SetDarkScreen();
            yield return scrim.FadeInLightScreen(1.5f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.Opening);
            SoundID.VN_footsteps.Play();
            yield return new WaitForSeconds(1f);
            yield return scrim.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(1f);
            background1.SetActive(false); background2.SetActive(true);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.Recovery);
            SoundID.VN_footsteps.Play();
            yield return scrim.FadeInLightScreen(1f);
            yield return new WaitForSeconds(1f);

            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.AfterRecovery);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.Camp);
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.JaysAnswer);
            yield return scrim.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(2f);
            background2.SetActive(false); background3.SetActive(true);
            yield return scrim.FadeInLightScreen(1f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.LabScan);
            yield return FadeSplit(0, 1);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.PhoneCallStart);
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.PhoneCallEnd);
            yield return new WaitForSeconds(.5f); AudioManager.Instance.PlaySFX(SoundID.VN_video_call_hangup);
            yield return FadeSplit(1, 0);
            yield return new WaitForSeconds(2f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.AilinScan);
            for (float t = 0; t < 4; t += Time.deltaTime) { ailinRevealLayer.color = new Color(1, 1, 1, t / 4); yield return null; }
            ailinRevealLayer.color = Color.white;
            yield return scrim.FadeInDarkScreen(2f);
        }
        private IEnumerator FadeSplit(float from, float to) { for (float t = 0; t < .5f; t += Time.deltaTime) { splitScreenLayer.color = new Color(1, 1, 1, Mathf.Lerp(from, to, t / .5f)); yield return null; } splitScreenLayer.color = new Color(1, 1, 1, to); }
        private void Update() => caveFlickerLayer.color = new Color(1, 1, 1, .5f + .5f * Mathf.Sin(Time.time));
    }

    public static class PreBounty8Dialogue
    {
        // Rocky, Kade, Jay, Ari, and NITES do not have keyed portraits yet, so their current expressions are retained.
        public static DialogueAsCode Opening => new DialogueAsCode()
            .Narrate("<i>Rocky moves Jackie onto the stretcher, and Kade tends to the dressing on her leg.</i>")
            .Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft, DialogueSprite.RockySerious).Enter(DialogueCharacter.Kade, CharacterActions.SetOffscreenRight, DialogueSprite.KadeSoft, fadeDuration: 0f)
            .Line(DialogueCharacter.Rocky, "You’re Jackie...")
            .Move(DialogueCharacter.Kade, CharacterActions.SetRight).Line(DialogueCharacter.Kade, "You know her?")
            .Line(DialogueCharacter.Rocky, "She’s Ailin’s daughter.").Line(DialogueCharacter.Rocky, "Didn’t think I’d see her again, here of all places...")
            .Line(DialogueCharacter.Jackie, "You... you knew my Ma?", DialogueSprite.JackieSurprisedOpen).Line(DialogueCharacter.Rocky, "Knew her? We were her team.")
            .Line(DialogueCharacter.Jackie, "What? Then... why are you here? Everyone... everyone thinks you’re dead!", DialogueSprite.JackieSurprisedOpen)
            .Narrate("<i>Kade and Rocky share a long, heavy look.</i>").Line(DialogueCharacter.Rocky, "...It’s a long, hard story, Jackie. Let’s get you to camp first.")
            .Line(DialogueCharacter.Jackie, "Is she... is she here with you?", DialogueSprite.JackieSurprisedClosed).Line(DialogueCharacter.Rocky, "...No. She’s not here.")
            .Line(DialogueCharacter.Jackie, "What? Why? What happened?", DialogueSprite.JackieSurprisedOpen).Line(DialogueCharacter.Rocky, "We were betrayed, and we... lost her.")
            .Line(DialogueCharacter.Kade, "Tsk. Wording please.").Line(DialogueCharacter.Kade, "We were separated. We don’t know where she is or whether she’s dead or alive.")
            .Line(DialogueCharacter.Rocky, "That was supposed to be better?").InterruptedLine(DialogueCharacter.Jackie, "Betrayed? By who? Tell me! I have to—", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Rocky, "No. Absolutely not. Look at you. You can't even walk and are already looking for another fight.")
            .Line(DialogueCharacter.Rocky, "Please. Don’t make this any harder for us and yourself. Not until you get better.").Line(DialogueCharacter.Jackie, "...Okay.")
            .Exit(DialogueCharacter.Rocky, DialogueCharacter.Kade);
        public static DialogueAsCode Recovery => new DialogueAsCode()
            .Line(DialogueCharacter.Rocky, "...How’s Ives? Is she doing alright?").Line(DialogueCharacter.Jackie, "...She’s... she’s fine.")
            .Line(DialogueCharacter.Rocky, "Good. Good to hear. She tended to draw the short straw, you know?").Line(DialogueCharacter.Rocky, "Our disappearance... It must have been hard on her. We were all she had.")
            .Line(DialogueCharacter.Jackie, "So why didn’t you come back?", DialogueSprite.JackieSurprisedOpen).Line(DialogueCharacter.Rocky, "We couldn’t. You’ll see why we’re out here soon enough.").Line(DialogueCharacter.Rocky, "Until then, just rest.");
        public static DialogueAsCode AfterRecovery => new DialogueAsCode().Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft).Enter(DialogueCharacter.Jay, CharacterActions.SetOffscreenRight, DialogueSprite.JaySorry, fadeDuration: 0f)
            .Line(DialogueCharacter.Rocky, "Welcome to camp. Get comfortable here.")
            .Move(DialogueCharacter.Jay, CharacterActions.SetRight)
            .Line(DialogueCharacter.Jay, "Hey, a new face! Who’s—").Line(DialogueCharacter.Jay, "Wait. No way! Is that, Little Jackie?")
            .Line(DialogueCharacter.Jackie, "Do I... know you?", DialogueSprite.JackieTired).Line(DialogueCharacter.Jay, "It’s me! Uncle Jay! Wow. You’ve grown so much! I haven’t seen you since you were, well, little!")
            .Line(DialogueCharacter.Rocky, "Heh. Jay’s got a perfect memory. Doesn’t forget a face. Or anything, for that matter.").Line(DialogueCharacter.Rocky, "I’ll leave you with him; feel free to ask him any questions. He’ll know the details best.")
            .Move(DialogueCharacter.Rocky, CharacterActions.SetOffscreenLeft)
            .Line(DialogueCharacter.Rocky, "Hey Ari! Get our guest a flask of water.").Line(DialogueCharacter.Nites, "On it, Cap!")
            .Narrate("<i>Ari hands Jackie a flask of water and leaves. As he turns, Jackie gets a good look at his crystalline scars and milky-white eye.</i>")
            .Line(DialogueCharacter.Jackie, "Is... is he okay?", DialogueSprite.JackieTired)
            .Move(DialogueCharacter.Jay, CharacterActions.SetMiddle)
            .Line(DialogueCharacter.Jay, "Oh, Ari? Yeah, he’s doing better now! Things haven’t been easy for any of us, but we make do!");
        public static DialogueAsCode Camp => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "...You’re all NITES, right?").Line(DialogueCharacter.Jackie, "How did you all end up here? And why haven't you come back?")
            .Line(DialogueCharacter.Jay, "Well, that’s a question and a half. Frankly, several choices that all brought us here.").Line(DialogueCharacter.Jay, "Let me think about where I should start.")
            .Narrate("<i>Jay’s gaze leaves Jackie and falls unfocused onto the shadowed cave walls.</i>");
        public static DialogueAsCode JaysAnswer => new DialogueAsCode()
            .Line(DialogueCharacter.Jay, "It's a Tuesday morning in my office, 10:24 a.m. on the wall clock over the door before Ari comes in.")
            .Line(DialogueCharacter.Jay, "His older sister was killed in a transport accident in the Tundra. Bad planning, rough route, and crucially, no backup support.")
            .Line(DialogueCharacter.Jay, "He tells me, ‘I’ll make sure no one gets left behind again.’ And so I put him on the team.")
            .Line(DialogueCharacter.Jay, "Then comes the day where I make the wrong call. We see flares on a faraway terrace, and I elect to take a detour.")
            .Line(DialogueCharacter.Jay, "A creature smashes into the crystals, releasing fine pink dust. Ari’s skin flash-crystalizes, and the man on his back too.")
            .Line(DialogueCharacter.Jay, "They turned into monsters. Only a handful of scouts and I made it out.")
            .Line(DialogueCharacter.Jay, "The reason we’re stuck out here is that Ari and the others get regular crystal implants to reverse the effects of the Waste.")
            .Line(DialogueCharacter.Jackie, "How did you move on? From knowing that people close to you got hurt because of a call you made?", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jay, "Some people move on by ‘letting go’ or ‘forgetting.’ But I can’t. The memories will be there should I open them.")
            .Line(DialogueCharacter.Jay, "When Ailin’s team found us, Kade started her research. I was terrified to face Ari when he woke up.")
            .Line(DialogueCharacter.Jay, "But he ran over and hugged me. He kept saying, ‘You’re alive! You’re safe!’")
            .Line(DialogueCharacter.Jay, "He told me, ‘You didn’t leave me behind. You came back for me. To save me.’")
            .Line(DialogueCharacter.Jay, "You don’t get to make that decision for them, Jackie. If you’re concerned, talk to them. Don’t protect them from their own decisions.")
            .Line(DialogueCharacter.Jackie, "But I’m afraid that what they’re doing won’t make them happy!", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jay, "Maybe not. But Ari had the largest smile I’ve ever seen. Trust me, I keep track.")
            .Narrate("<i>Rocky and Kade walk back over, their expressions grim.</i>")
            .Line(DialogueCharacter.Rocky, "Kade ran her sensors. There’s a massive Amplitude concentration at the peak of the mountain. The ‘frog’ story is real.")
            .Line(DialogueCharacter.Jay, "I’ll organize another search party.").Line(DialogueCharacter.Jackie, "Let me come. I can guide you.").Line(DialogueCharacter.Rocky, "No. Not a chance. You’re resting. Trust Jay’s team.")
            .Line(DialogueCharacter.Jackie, "Then be careful. The frog can control the Waste Creatures.").Line(DialogueCharacter.Jay, "Noted, I’ll pass that on, scout!");
        public static DialogueAsCode LabScan => new DialogueAsCode().Enter(DialogueCharacter.Cam, CharacterActions.SetLeft, DialogueSprite.CamTalk)
            .Line(DialogueCharacter.System, "Designation N-0174. Tone five, plus twenty-two cents. Amplitude Concentration: 65%. Tuning candidacy: viable.")
            .Line(DialogueCharacter.Cam, "Hah yes! These scouts form clusters around exactly where tones five and six should be.").Line(DialogueCharacter.System, "Scan error. No frequencies within supplied range.")
            .Line(DialogueCharacter.Cam, "I guess I’ll have to run a full search then.").Line(DialogueCharacter.System, "Unbounded search initiated. Current progress 2%.")
            .Line(DialogueCharacter.Cam, "I should reach Jackie in the meantime. I’ll try Ives.", DialogueSprite.CamPout, SoundID.VN_radio_static);
        public static DialogueAsCode PhoneCallStart => new DialogueAsCode().Enter(DialogueCharacter.Cam, CharacterActions.SetLeft, DialogueSprite.CamTalk).Enter(DialogueCharacter.Ives, CharacterActions.SetRight, DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives, "Hey, kiddo. How are things?", sfx: SoundID.VN_video_call_pickup).Line(DialogueCharacter.Cam, "Have you heard from Jackie recently by chance?").Line(DialogueCharacter.Ives, "No. Is everything good?").Line(DialogueCharacter.Cam, "She hasn’t checked in for three days now?").Line(DialogueCharacter.Ives, "I’ll ask around. Hold for a bit.");
        public static DialogueAsCode PhoneCallEnd => new DialogueAsCode().Line(DialogueCharacter.Ives, "Shoot. Nobody’s seen her. She’s been AWOL from roll call for about 72 hours.").Line(DialogueCharacter.Cam, "Before she left, she gave me coordinates. Well... I made her.").Line(DialogueCharacter.Ives, "Smart thinking. We’ll arrive at the Tundra camp by the end of the day. I’ll draft a plan.").Line(DialogueCharacter.Cam, "Good luck. And Ives, don’t push yourself too hard.").Line(DialogueCharacter.Ives, "Yeah, thanks kiddo. Don’t worry, I’ll steer myself around those crystals.").Line(DialogueCharacter.Cam, "Here’s hoping for good fortune to all of us.");
        public static DialogueAsCode AilinScan => new DialogueAsCode().Enter(DialogueCharacter.Cam, CharacterActions.SetLeft, DialogueSprite.CamTalk).Line(DialogueCharacter.System, "Unbounded search complete.").Line(DialogueCharacter.System, "Designation C-0010. Tone one, zero cents. Amplitude Concentration: 291%. Search Candidacy: Excellent.").Line(DialogueCharacter.System, "Infection status: Uninfected. Amplitude Tolerance: 4.8σ. Crystal concentration: 0.0u/L.").Narrate("<i>Ailin’s body is revealed inside the tank.</i>").Line(DialogueCharacter.Cam, "W-What...!? With that amount of amplitude, how are you still intact?", DialogueSprite.CamConfused).Line(DialogueCharacter.Cam, "Uninfected, high tolerance, and tone one. You’re perfect.").Line(DialogueCharacter.Cam, "C-0010. Let’s get you prepped.");
    }
}

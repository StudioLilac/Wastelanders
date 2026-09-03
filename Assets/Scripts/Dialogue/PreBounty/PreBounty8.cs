using DialogueScripts;
using LevelSelectInformation;
using System.Collections;
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
        [SerializeField] private GameObject background1, background2, background3, camLabBackground, splitCallBackground;
        [SerializeField] private Image caveFlickerLayer, ailinRevealLayer;
        [SerializeField] private UIFadeHandler scrim;
        [SerializeField] private AudioClip suspenseDrone;

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
            
            yield return scrim.FadeInDarkScreen(0.5f);
            background2.SetActive(false); background3.SetActive(true);
            yield return scrim.FadeToAlpha(0.8f, 0.5f);

            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.JaysAnswer);
            yield return scrim.FadeToAlpha(0.5f, 1f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.JaysAnswerTheDay);

            yield return scrim.FadeInDarkScreen(0.5f);
            background2.SetActive(true); background3.SetActive(false);
            StartCoroutine(scrim.FadeInLightScreen(0.5f));
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.FlashBackDone);
            yield return scrim.FadeInDarkScreen(1f);


            AudioManager.Instance.FadeOutCurrentBackgroundTrack(1f);
            yield return DialogueBoxV2.Instance.Play(new DialogueAsCode().Exit(DialogueCharacter.Jay, DialogueCharacter.Rocky));
            yield return new WaitForSeconds(2f);
            background2.SetActive(false); camLabBackground.SetActive(true);
            yield return scrim.FadeInLightScreen(1f);
            ControllableAudioChannel eerie = AudioManager.Instance.CreateChannel(suspenseDrone, AudioCategory.Music, level: 0f);
            eerie.Play();
            eerie.FadeTo(1f, 1f);

            SoundID.VN_footsteps.Play();
            yield return new WaitForSeconds(0.5f);
            SoundID.VN_system_beep.Play();
            yield return new WaitForSeconds(0.5f);
            yield return DialogueBoxV2.Instance.Play(new DialogueAsCode().Line(DialogueCharacter.System, "<i>Designation N-0174. Tone five, plus twenty-two cents. Amplitude Concentration: 65%. Tuning candidacy: viable.</i>"));

            SoundID.VN_footsteps.Play();
            yield return new WaitForSeconds(0.5f);
            SoundID.VN_system_beep.Play();
            yield return new WaitForSeconds(0.5f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.LabScan);

            SoundID.VN_footsteps.Play();
            yield return new WaitForSeconds(0.5f);
            SoundID.VN_system_beep.Play();
            yield return new WaitForSeconds(0.5f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.LabScanFailed);
            yield return new WaitForSeconds(0.5f);
            SoundID.VN_system_beep.Play();
            yield return new WaitForSeconds(0.5f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.LabScanFailedAgain);
            yield return scrim.FadeInDarkScreen(0.5f);
            yield return new WaitForSeconds(1.5f);
            yield return scrim.FadeInLightScreen(0.5f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.AllScanFailed);


            yield return scrim.FadeInDarkScreen(0.5f);

            StartCoroutine(eerie.FadeTo(0.7f, 2f));
            camLabBackground.SetActive(false); splitCallBackground.SetActive(true);
            yield return new DialogueAsCode()
                .Enter(DialogueCharacter.Cam, CharacterActions.SetRight, DialogueSprite.CamSmile, fadeDuration: 0)
                .Enter(DialogueCharacter.Ives, CharacterActions.SetLeft, DialogueSprite.IvesSmile, fadeDuration: 0).Play();
            yield return new WaitForSeconds(1.5f);
            yield return scrim.FadeInLightScreen(0.5f);

            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.PhoneCallStart);
            yield return scrim.FadeInDarkScreen(0.5f);
            SoundID.VN_radio_static.Play();
            yield return new WaitForSeconds(2f);
            yield return scrim.FadeInLightScreen(0.5f);


            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.PhoneCallEnd);

            yield return scrim.FadeInDarkScreen(0.5f);
            yield return new WaitForSeconds(1.5f);
            camLabBackground.SetActive(true); splitCallBackground.SetActive(false);
            SoundID.VN_video_call_hangup.Play();
            yield return new DialogueAsCode()
                .Enter(DialogueCharacter.Cam, CharacterActions.SetMiddle, DialogueSprite.CamNeutral, fadeDuration: 0)
                .Enter(DialogueCharacter.Ives, CharacterActions.SetOffscreenRight, DialogueSprite.IvesSmile, fadeDuration: 0).Play();
            yield return scrim.FadeInLightScreen(0.5f);

            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(PreBounty8Dialogue.AilinScan);
            StartCoroutine(eerie.FadeTo(1.5f, 2f));
            for (float t = 0; t < 3; t += Time.deltaTime) { ailinRevealLayer.color = new Color(1, 1, 1, t / 3); yield return null; }
            ailinRevealLayer.color = Color.white;
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(eerie.FadeTo(0f, 2.5f));
            yield return scrim.FadeInDarkScreen(1.5f);
            yield return new WaitForSeconds(1f);


            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }
        private void Update() => caveFlickerLayer.color = new Color(1, 1, 1, .5f + .5f * Mathf.Sin(Time.time));
    }

    public static class PreBounty8Dialogue
    {
        // Rocky, Kade, Jay, Ari, and NITES do not have keyed portraits yet, so their current expressions are retained.
        public static DialogueAsCode Opening => new DialogueAsCode()
            .Narrate("<i>Rocky moves Jackie onto the stretcher, and Kade tends to the dressing on her leg.</i>")
            .Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft, DialogueSprite.RockySerious).Enter(DialogueCharacter.Kade, CharacterActions.SetOffscreenRight, DialogueSprite.KadeSoft, fadeDuration: 0f)
            .Line(DialogueCharacter.Rocky, "You’re Jackie...")
            .Move(DialogueCharacter.Kade, CharacterActions.SetRight)
            .Line(DialogueCharacter.Kade, "You know her?")
            .Line(DialogueCharacter.Rocky, "She’s Ailin’s daughter.")
            .Line(DialogueCharacter.Rocky, "Didn’t think I’d see her again, here of all places...")
            .Line(DialogueCharacter.Jackie, "You... you knew my Ma?", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Rocky, "Knew her? We were her team.")
            .Line(DialogueCharacter.Jackie, "What? Then... why are you here? Everyone... everyone thinks you’re dead!", DialogueSprite.JackieSurprisedOpen)
            .Narrate("<i>Kade and Rocky share a long, heavy look.</i>")
            .Line(DialogueCharacter.Rocky, "...It’s a long, hard story, Jackie. Let’s get you to camp first.")
            .Line(DialogueCharacter.Jackie, "Is she... is she here with you?", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Rocky, "...No. She’s not here.")
            .Line(DialogueCharacter.Jackie, "What? Why? What happened?", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Rocky, "We were betrayed, and we... lost her.")
            .Line(DialogueCharacter.Kade, "Tsk. Wording please.", DialogueSprite.KadeSerious)
            .Line(DialogueCharacter.Kade, "We were separated. We don’t know where she is or whether she’s dead or alive.")
            .Line(DialogueCharacter.Rocky, "That was supposed to be better?")
            .InterruptedLine(DialogueCharacter.Jackie, "Betrayed? By who? Tell me! I have to—", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Rocky, "No. Absolutely not. Look at you. You can't even walk and are already looking for another fight.")
            .Line(DialogueCharacter.Rocky, "Please. Don’t make this any harder for us and yourself. Not until you get better.")
            .Line(DialogueCharacter.Jackie, "...Okay.")
            .Exit(DialogueCharacter.Rocky, DialogueCharacter.Kade);

        public static DialogueAsCode Recovery => new DialogueAsCode()
            .Line(DialogueCharacter.Rocky, "...How’s Ives? Is she doing alright?").Line(DialogueCharacter.Jackie, "...She’s... she’s fine.")
            .Line(DialogueCharacter.Rocky, "Good. Good to hear. She tended to draw the short straw, you know?")
            .Line(DialogueCharacter.Rocky, "Our disappearance... It must have been hard on her. We were all she had.")
            .Line(DialogueCharacter.Jackie, "So why didn’t you come back?", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Rocky, "We couldn’t. You’ll see why we’re out here soon enough.").Line(DialogueCharacter.Rocky, "Until then, just rest.");
        public static DialogueAsCode AfterRecovery => new DialogueAsCode().Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft).Enter(DialogueCharacter.Jay, CharacterActions.SetOffscreenRight, DialogueSprite.JaySorry, fadeDuration: 0f)
            .Line(DialogueCharacter.Rocky, "Welcome to camp. Get comfortable here.")
            .Move(DialogueCharacter.Jay, CharacterActions.SetRight)
            .Line(DialogueCharacter.Jay, "Hey, a new face! Who’s—").Line(DialogueCharacter.Jay, "Wait. No way! Is that, Little Jackie?")
            .Line(DialogueCharacter.Jackie, "Do I... know you?", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jay, "It’s me! Uncle Jay! Wow. You’ve grown so much! I haven’t seen you since you were, well, little!")
            .Line(DialogueCharacter.Rocky, "Heh. Jay’s got a perfect memory. Doesn’t forget a face. Or anything, for that matter.")
            .Line(DialogueCharacter.Rocky, "I’ll leave you with him, feel free to ask him any questions. He’ll know the details best.")
            .Move(DialogueCharacter.Rocky, CharacterActions.SetOffscreenLeft).Line(DialogueCharacter.Rocky, "Hey Ari! Get our guest a flask of water.")
            .Line(DialogueCharacter.Ari, "On it, Cap!")
            .Narrate("<i>Ari hands Jackie a flask of water and leaves.</i>")
            .Narrate("<i>As he turns, Jackie gets a good look at his crystalline scars and milky-white eye.</i>")
            .Line(DialogueCharacter.Jackie, "Is... is he okay?", DialogueSprite.JackieTired)
            .Move(DialogueCharacter.Jay, CharacterActions.SetMiddle)
            .Line(DialogueCharacter.Jay, "Oh, Ari? Yeah, he’s doing better now! Things haven’t been easy for any of us, but we make do!");
        public static DialogueAsCode Camp => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "...You’re all NITES, right?")
            .Line(DialogueCharacter.Jackie, "How did you all end up here? And why haven't you come back?")
            .Line(DialogueCharacter.Jay, "Well, that’s a question and a half. Frankly, several choices that all brought us here.")
            .Line(DialogueCharacter.Jay, "Let me think about where I should start.")
            .Exit(DialogueCharacter.Jay)
            .Narrate("<i>Jay’s gaze leaves Jackie and falls unfocused onto the shadowed cave walls.</i>");
        public static DialogueAsCode JaysAnswer => new DialogueAsCode()
            .Line(DialogueCharacter.Jay, "It's a Tuesday morning in my office, 10:24 a.m. on the wall clock over the door before Ari comes in.")
            .Line(DialogueCharacter.Jay, "I place down my coffee mug in my right hand as Ari hands me his application for the team.")
            .Line(DialogueCharacter.Jay, "In my other hand is a report on a Tundra transport accident. Lack of manpower, poor route planning, and crucially, no backup.")
            .Line(DialogueCharacter.Jay, "It killed Ari’s older sister.")
            .Line(DialogueCharacter.Jay, "I look at him, cuffs hastily rolled back, and a tie slightly too long.")
            .Line(DialogueCharacter.Jay, "He’s a small guy, so I ask what makes him fit to join the team.")
            .Line(DialogueCharacter.Jay, "He says, \"Six minutes a kilometer. With twenty kilos on my back.\"")
            .Line(DialogueCharacter.Jay, "Then he tells me, \"I’ll make sure no one gets left behind again.\"")
            .Line(DialogueCharacter.Jay, "And so I put him on the team, among the many others with their own strengths.");
        public static DialogueAsCode JaysAnswerTheDay => new DialogueAsCode()
           .Line(DialogueCharacter.Jay, "And then comes the day where I make the wrong call.")
            .Line(DialogueCharacter.Jay, "It’s a late sunny afternoon in the mountain ranges. We’re 7 hours out and heading back.")
            .Line(DialogueCharacter.Jay, "We’re all in rough shape after a couple altercations. Two thirds my men are on the Serums.")
            .Line(DialogueCharacter.Jay, "That’s when we see the flares, on a faraway terrace. One of the other scouting teams in the distance.")
            .Line(DialogueCharacter.Jay, "Protocol says to mark it, return, and launch a rescue party with fresh hands.")
            .Line(DialogueCharacter.Jay, "But we're far out, and I don’t know the severity of the situation. So I elect to take a detour.")
            .Line(DialogueCharacter.Jay, "My men follow. I don’t need a map so I’m up in front.")
            .Line(DialogueCharacter.Jay, "At the top of the terrace, there are these waist-tall crystalline growths that spike up from the ground.")
            .Line(DialogueCharacter.Jay, "The ground, as far as the eye can see, is covered in these arm-deep, forearm-wide holes drilled in the ground at sporadic intervals.")
            .Line(DialogueCharacter.Jay, "I was here two days ago. There was nothing. We hurry our pace.")
            .Line(DialogueCharacter.Jay, "We come upon the other team, there are eight down, and six taking care of them. ")
            .Line(DialogueCharacter.Jay, "Ari is the first to get one of the eight on his back. And we follow suit.")
            .Line(DialogueCharacter.Jay, "On the path back, the ground begins to move, and these hardy Waste critters break from the surface. ")
            .Line(DialogueCharacter.Jay, "It’s nothing exceptional to what we’d dealt with before.")
            .Line(DialogueCharacter.Jay, "As we’re winding down the fight, a creature desperately lunges at Ari, who evades it with the man on his back.")
            .Line(DialogueCharacter.Jay, "The creature smashes head first into the crystals, releasing fine pink dust in the air. And it happens instantly.")
            .Line(DialogueCharacter.Jay, "Ari’s skin flash-crystalizes. His veins erupting violet as flesh is expelled from his bones, hardening in the next instant into a craterous shell.")
            .Line(DialogueCharacter.Jay, "The man on his back too. Their screams joined a cacophony, who was who I could no longer tell. ")
            .Line(DialogueCharacter.Jay, "They turned into monsters. Lashed out against anything that moved. More crystals shattered.")
            .Line(DialogueCharacter.Jay, "Only a handful of scouts and I made it out.")
            .Line(DialogueCharacter.Jay, "...")
            ;

    public static DialogueAsCode FlashBackDone => new DialogueAsCode()
            .Enter(DialogueCharacter.Jay, CharacterActions.SetMiddle, DialogueSprite.JaySerious, fadeDuration: 0f)
            .Narrate("<i>Jay pauses, his eyes coming back into focus onto the water flask in front of him.</i>")
            .Line(DialogueCharacter.Jay, "But yeah, I try not to dwell on that much anymore. After all, we’re doing much better now.")
            .Line(DialogueCharacter.Jay, "The reason why we’re stuck out here is that Ari and the others get regular crystal implants to reverse the effects of the Waste. ")
            .Line(DialogueCharacter.Jay, "Kade calls them 'deposition grafts': \"Seeds that force Waste to turn into a stable crystal lattice.\"")
            .Line(DialogueCharacter.Jay, "\"Just need to swap ‘em out once in a while.\"")
            .Line(DialogueCharacter.Jay, "As far as I can tell, those Crystals are made of hardened Waste, which is easier to deal with.")
            .Line(DialogueCharacter.Jay, "Thanks to her, we can all live day by day despite what’s in them.")
            .Narrate("<i>Jackie rummages through a pocket and pulls out a vial of Serum.</i>")
            .Narrate("<i>She holds it against the fire. It’s clear. </i>")
            .Line(DialogueCharacter.Jackie, "...Can I ask a question?", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jay, "Shoot.")
            .Line(DialogueCharacter.Jackie, "How did you move on? From knowing that people close to you got hurt because of a call you made?")
            .Line(DialogueCharacter.Jay, "Oh. I hear you.")
            .Line(DialogueCharacter.Jay, "Some people move on by “letting go” or “forgetting.” But, well, I can’t.")
            .Line(DialogueCharacter.Jay, "For the first couple of years after, I was a complete mess. ")
            .Line(DialogueCharacter.Jay, "I couldn't sleep, couldn't leave these caves or the memories would flood back. So I stopped leaving.")
            .Line(DialogueCharacter.Jay, "When Ailin’s team found us, she urged me to go back home while she would search for the rest of my team.")
            .Line(DialogueCharacter.Jay, "I might have taken up her offer, if it were not for Kade starting her research on the operating table.")
            .Line(DialogueCharacter.Jay, "She needed someone who could tell her who was lying on that table, and everything about them.")
            .Line(DialogueCharacter.Jay, "“Scar on the nose. That’s Marcie. B+ blood, 110/75 mmHg, woodwind timbre.”")
            .Line(DialogueCharacter.Jay, "I... can’t tell whether it was out of a sense of duty, or atonement. But I ended up being there for all of it.")
            .Line(DialogueCharacter.Jay, "Except one. The day Kade stabilized Ari.")
            .Line(DialogueCharacter.Jay, "I was terrified. I mean, truly, deeply afraid.", DialogueSprite.JaySorry)
            .Line(DialogueCharacter.Jackie, "...Of what?")
            .Line(DialogueCharacter.Jay, "Of him. Of how he’d feel and what he'd say once he woke up and saw me.")
            .Line(DialogueCharacter.Jay, "I was his leader and I made the call that left him a monster for years.")
            .Line(DialogueCharacter.Jay, "All I left with were a few scratches from the terrain. It was entirely luck that I didn’t get any Waste in me that day.")
            .Line(DialogueCharacter.Jay, "I wasn’t ready for him to hate me.")
            .Line(DialogueCharacter.Jay, "So I excused myself, assigned myself the long recon route that day.")
            .Line(DialogueCharacter.Jay, "But, there he was when I got back, standing by the camp entrance. Kade must have told him which tunnel I’d take back. ")
            .Line(DialogueCharacter.Jay, "He ran over. Threw his arms around me so tight I thought my ribs would crack.")
            .Line(DialogueCharacter.Jackie, "He wasn’t angry?", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Jay, "No. He was ecstatic. He kept saying, “You’re alive! You’re safe!”")
            .Line(DialogueCharacter.Jay, "I didn’t get it. I asked him, “Ari, how can you... after what I did to you?”")
            .Line(DialogueCharacter.Jay, "And I’ll never forget his answer. Not that I could, even if I wanted to, but you get the point.")
            .Line(DialogueCharacter.Jay, "He looked me right in my guilty, stupid face and said, “Jay, I joined the team to stop people from being left behind.”")
            .Line(DialogueCharacter.Jay, "“You didn’t leave me behind. You came back for me. To save me.”")
            .Line(DialogueCharacter.Jay, "I realized, all this time I thought I had every frame on what happened that day.")
            .Line(DialogueCharacter.Jay, "But I didn’t. Because I didn’t have theirs.")
            .Line(DialogueCharacter.Jay, "And so I began asking. Some woke up. Others... never got the chance to give me their answer.")
            .Line(DialogueCharacter.Jay, "But whatever reasons they had to follow me, it was theirs.")
            .Line(DialogueCharacter.Jay, "And it wasn’t up to me to decide whether they were good enough.")
            .Line(DialogueCharacter.Jackie, "But... what if I don’t want people to risk getting hurt for me?")
            .Line(DialogueCharacter.Jay, "Too bad!")
            .Line(DialogueCharacter.Jackie, "...")
            .Line(DialogueCharacter.Jay, "No, really. It’s too bad, haha. You don’t get to make that decision for them, Jackie.")
            .Line(DialogueCharacter.Jay, "Trust me, you’ll be all sorts of bent out of shape if you try. \r\n")
            .Line(DialogueCharacter.Jay, "If you’re concerned, you talk to them. Get their perspective on the matter. \r\n")
            .Line(DialogueCharacter.Jay, "But you don’t go around “protecting” them from their own decisions. That’s just not what equals do.")
            .Line(DialogueCharacter.Jackie, "...But that’s so messed up. I feel...", DialogueSprite.JackieTired)
            .Line(DialogueCharacter.Jay, "Powerless? Yeah. Welcome to caring about people.")
            .Line(DialogueCharacter.Jay, "Sometimes caring means letting them decide what makes them happy.")
            .Line(DialogueCharacter.Jackie, "But what if what they’re doing won’t make them happy!")
            .Line(DialogueCharacter.Jay, "Maybe not. But the man who greeted by the camp entrance sure had the largest smile I’ve ever seen.")
            .Line(DialogueCharacter.Jay, "Trust me, I keep track.")
            .Narrate("<i>Jay points to Ari, sharpening a spear by the campfire while tending to a fresh pot of boiling water.</i>")
            .Line(DialogueCharacter.Jackie, "...")
            .Move(DialogueCharacter.Rocky, CharacterActions.SetLeft).Move(DialogueCharacter.Jay, CharacterActions.SetRight)
            .Narrate("<i>Rocky and Kade walk back over, their expressions grim.</i>")
            .Line(DialogueCharacter.Rocky, "Hey Jay. Kade ran her sensors. There’s a massive Amplitude concentration at the peak of the mountain.")
            .Line(DialogueCharacter.Rocky, " That 'frog' is real. Have you seen anything in that sector?\r\n")
            .Line(DialogueCharacter.Jay, "What? I sent Bravo up there not a week ago. They did a full routine sweep of the routes.", DialogueSprite.JaySerious)
            .Line(DialogueCharacter.Jay, "...I'll organize another search party.")
            .Line(DialogueCharacter.Jackie, "Let me come. I can guide you.").
            Line(DialogueCharacter.Rocky, "No. Not a chance. You’re resting. Trust Jay’s team.")
            .Line(DialogueCharacter.Jackie, "But!...")
            .Line(DialogueCharacter.Jackie, "...")
            .Line(DialogueCharacter.Jackie, "...Alright.")
            .Line(DialogueCharacter.Jackie, "Then be careful. The frog can control the Waste Creatures.")
            .Line(DialogueCharacter.Jackie, "Anything you fight might be more coordinated than you’d think.")
            .Line(DialogueCharacter.Jay, "Noted, I’ll pass that on, scout!");
        public static DialogueAsCode LabScan => new DialogueAsCode()
            .Enter(DialogueCharacter.Cam, CharacterActions.SetOffscreenLeft, DialogueSprite.CamSmile, fadeDuration: 0)
            .Line(DialogueCharacter.System, "<i>Designation N-0268. Tone six, minus thirty-four cents. Amplitude Concentration: 71%. Tuning candidacy: viable.</i>")
            .Move(DialogueCharacter.Cam, CharacterActions.SetLeft)
            .Line(DialogueCharacter.Cam, "Hah yes! These scouts form clusters around exactly where tones five and six should be.")
            .Line(DialogueCharacter.Cam, "With two and three from the clinical records, that leaves four and seven unaccounted for in the scale.")
            .Line(DialogueCharacter.Cam, "Here’s hoping these last few tanks will give me something to work with.")
            .Move(DialogueCharacter.Cam, CharacterActions.SetRight);

        public static DialogueAsCode LabScanFailed => new DialogueAsCode()
            .Line(DialogueCharacter.System, "<i>Scan error. No frequencies within supplied range.</i>")
            .Line(DialogueCharacter.Cam, "What? But this band should cover between four and seven.", DialogueSprite.CamTalk);

        public static DialogueAsCode LabScanFailedAgain => new DialogueAsCode()
            .Line(DialogueCharacter.System, "<i>Scan error. No frequencies within supplied range.</i>")
            .Line(DialogueCharacter.Cam, "Huh, weird. I guess I’ll come back to this one.")
            .Move(DialogueCharacter.Cam, CharacterActions.SetOffscreenRight);

        public static DialogueAsCode AllScanFailed => new DialogueAsCode()
            .Enter(DialogueCharacter.Cam, CharacterActions.SetMiddle, DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "That’s every tank then. No fours, and no sevens.")
            .Line(DialogueCharacter.Cam, "Guess that means I’ll have to look for those frequencies myself.")
            .Line(DialogueCharacter.Cam, "System, scan me. You think I could do that?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.System, "<i>Unregistered subject. Tone one, zero cents. Amplitude Concentration: 101% Search candidacy: Unviable. Amplitude Insufficiency.</i>", sfx: SoundID.VN_system_beep)
            .Line(DialogueCharacter.Cam, "Yeah, I can read.", DialogueSprite.CamPout)
            .Line(DialogueCharacter.Cam, "...Log me anyway.", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "...")
            .Line(DialogueCharacter.Cam, "Though that tank earlier... \"No frequencies within supplied range.\"")
            .Line(DialogueCharacter.Cam, "That’s not the same as no data.")
            .Line(DialogueCharacter.Cam, "I guess I’ll have to run a full search then.", sfx: SoundID.VN_footsteps)
            .Line(DialogueCharacter.System, "<i>Unbounded search initiated. Current progress 2%.</i>")
            .Line(DialogueCharacter.Cam, "Oh that’ll take a while. In the meantime, I wonder what Jackie's up to.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "It’s weird she hasn’t sent her coordinates in a while.")
            .Narrate("<i>Cam taps his console, initiating a call.</i>")
            .Line(DialogueCharacter.Cam, "Hmm, I can’t reach her. I’ll try Ives.", sfx: SoundID.VN_radio_static);
        public static DialogueAsCode PhoneCallStart => new DialogueAsCode()
            .Line(DialogueCharacter.Ives, "Hey, kiddo. How are things?", sfx: SoundID.VN_video_call_pickup)
            .Line(DialogueCharacter.Cam, "Good! I’ve been working on a new theory. If my search goes right, I might be able to help you!", DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Ives, "Heh appreciate the effort, but you don’t usually call me for science. What’s up?")
            .Line(DialogueCharacter.Cam, "Have you heard from Jackie recently by chance?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Ives, "No. Is everything good?", DialogueSprite.IvesQuestioning)
            .Line(DialogueCharacter.Cam, "Yeah it’s just, I told her to keep me updated on something she was doing.")
            .Line(DialogueCharacter.Cam, "But she hasn’t checked in for... three days now?")
            .Line(DialogueCharacter.Ives, "...I’ll ask around. Hold for a bit.")
            .Move(DialogueCharacter.Ives, CharacterActions.SetOffscreenLeft);
        public static DialogueAsCode PhoneCallEnd => new DialogueAsCode()
            .Move(DialogueCharacter.Ives, CharacterActions.SetLeft)
            .Line(DialogueCharacter.Ives, "Shoot. Nobody’s seen her.")
            .Line(DialogueCharacter.Ives, "She’s been AWOL from roll call for about 72 hours. And they can’t launch a search without a location.")
            .Line(DialogueCharacter.Cam, "That's fine. Before she left, she gave me coordinates. Well, I made her.")
            .Line(DialogueCharacter.Ives, "Smart thinking. Send them when you get the chance.", DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives, "We’ll be arriving at the Tundra camp by the end of the day. I’ll draft a plan.", DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Cam, "Good luck. And Ives... stay clear of any crystals alright? And watch your symptoms with Dr. Weise?")
            .Line(DialogueCharacter.Ives, "Yeah, thanks kiddo. Hear you loud and clear, I’ll do my best to steer myself around those crystals.", DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Cam, "Sounds great. I’ll see ya.", DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Ives, "See ya.")
            ;
        public static DialogueAsCode AilinScan => new DialogueAsCode()
            .Line(DialogueCharacter.Cam, "Here’s hoping for good fortune to all of us.")
            .Line(DialogueCharacter.System, "<i>Unbounded search complete.</i>", sfx: SoundID.VN_system_beep)
            .Line(DialogueCharacter.System, "<i>Designation C-0010. Tone one, zero cents. Amplitude Concentration: 291%. Search Candidacy: Excellent.</i>")
            .Line(DialogueCharacter.System, "<i>Infection status: Uninfected. Amplitude Tolerance: 4.8σ. Crystal concentration: 0.0u/L.</i>")
            .Line(DialogueCharacter.Cam, "W-What...!? With that amount of amplitude, how are you still intact?", DialogueSprite.CamConfused)
            .Line(DialogueCharacter.Cam, "Uninfected, high tolerance, and tone one. You’re perfect.")
            .InterruptedLine(DialogueCharacter.Cam, "Thank goodness... Now I won’t have to search using myself. Afterall, Ives doesn’t have time to waste—", DialogueSprite.CamSmile, duration: 1f)
            .Line(DialogueCharacter.Cam, "On second thought, I should have rephrased that...", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "Anyway, I’ll have to thank you properly when you wake up. You’re doing me a huge favour.", DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Cam, "C-0010. Let’s get you prepped.")
            .Exit(DialogueCharacter.Cam);
    }
}

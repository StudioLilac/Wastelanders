using DialogueScripts;
using LevelSelectInformation;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue
{
    public class Epilogue_6 : MonoBehaviour
    {
        public Image trainingBackground;
        public UIFadeHandler scrim;

        private IEnumerator Start()
        {
            scrim.SetDarkScreen();
            yield return StartCoroutine(scrim.FadeToAlpha(0.8f, 1.5f));
            yield return DialogueBoxV2.Instance.Play(Epilogue_6_Dialogue.JackieIvesOpener);
            yield return StartCoroutine(scrim.FadeToAlpha(0.5f, 1.5f));
            yield return DialogueBoxV2.Instance.Play(Epilogue_6_Dialogue.JackieIvesOpenerTraining);
            yield return StartCoroutine(scrim.FadeInDarkScreen(1.5f));

            trainingBackground.gameObject.SetActive(false);
            SoundID.VN_radio_static.Play();
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(scrim.FadeInLightScreen(1.5f));

            yield return DialogueBoxV2.Instance.Play(Epilogue_6_Dialogue.Dialogue);
            yield return StartCoroutine(scrim.FadeInDarkScreen(1.5f));

            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }
    }

    public static class Epilogue_6_Dialogue
    {

        public static DialogueAsCode JackieIvesOpener => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "Aunt Ives? When will Ma be back?")
            .Line(DialogueCharacter.Ives, "Dunno kid. She’ll be back when she feels like it.")
            .Line(DialogueCharacter.Jackie, "But she promised she’d be back soon...")
            .Line(DialogueCharacter.Ives, "You’ll have to trust that she’ll keep it. ")
            .Line(DialogueCharacter.Jackie, "Okay...  Then can we do something?")
            .Line(DialogueCharacter.Ives, "Sure kid, what’s up?")
            .Line(DialogueCharacter.Jackie, "I wanna train my sneaking some more before Ma comes back.")
            .Line(DialogueCharacter.Jackie, "I’ll hide somewhere and you come find me. But if I sneak up on you I win!")
            .Line(DialogueCharacter.Ives, "Sorry kid, hiding and seeking ain’t really my thing. That’s your Ma's. You’ll have to wait till she comes back.")
            .Line(DialogueCharacter.Jackie, "Aww, but I failed so badly the last time. How will I grow stronger then?")
            .Line(DialogueCharacter.Ives, "How about this, she taught you how to use a staff right? Why don’t we spar for a bit? ")
            .Line(DialogueCharacter.Jackie, "Oh, that sounds fun! Sure!")
            ;


        public static DialogueAsCode JackieIvesOpenerTraining => new DialogueAsCode()
            .Narrate("<i>Jackie picks up her staff, Ives puts her fists up in a stance, and the spar begins.</i>")
            .Narrate("<i>Jackie pushes forward on the offensive swinging with a flurry of blows. </i>")
            .Narrate("<i>Ives paces backwards, absorbing each blow. When she finds an opening, she strikes. Just enough to knock Jackie off balance.</i>")
            .Line(DialogueCharacter.Jackie, "Oof.")
            .Line(DialogueCharacter.Ives, "Not bad kid, but here’s a tip. Right now, you’re faster than you are stronger, so use that speed to your advantage.")
            .Line(DialogueCharacter.Ives, "Don’t try to match someone stronger than you head on, get a good feel for how they move first, then punish the openings.")
            .Line(DialogueCharacter.Ives, "For you, I’d say...")
            .Line(DialogueCharacter.Ives, "Don’t fight their strength, fight their shape.\r\n")
            .Line(DialogueCharacter.Jackie, "Hmm. ")
            .Narrate("<i>Jackie shuts her eyes, and scrunches up her face. </i>")
            .Line(DialogueCharacter.Ives, "What’s wrong? You alright?")
            .Line(DialogueCharacter.Jackie, "Just thinking.")
            .Line(DialogueCharacter.Ives, "Oh.")
            .Narrate("<i>A wry smile slips out of Ives’ mouth.</i>")
            .Line(DialogueCharacter.Jackie, "Okay got it! One more time.")
            .Line(DialogueCharacter.Ives, "Hah. Sure.")
            .Narrate("<i>They spar again, this time Jackie keeps her distance, letting Ives make the first move.</i>")
            .Narrate("<i>She’s pushed back, but manages to land a clean hit on Ives on her way to the ground.</i>")
            .Line(DialogueCharacter.Jackie, "Ack.")
            .Line(DialogueCharacter.Ives, "Hey good hit. You’re a quick learner.")
            .Narrate("<i>Ives offers a hand to Jackie. Jackie hesitates.</i>")
            .Line(DialogueCharacter.Jackie, "...You’ll help me get strong?")
            .Line(DialogueCharacter.Ives, "I’ll be here. Any time. ")
            .Line(DialogueCharacter.Jackie, "Then let’s go again!")
            .Narrate("<i>Jackie takes the hand, and Ives pulls her up.</i>")
            ;

        public static DialogueAsCode Dialogue => new DialogueAsCode()
            .Enter(DialogueCharacter.Cam, CharacterActions.SetLeft, DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "Hey Jackie, connection alright?")

            .Enter(DialogueCharacter.Jackie, CharacterActions.SetRight, DialogueSprite.JackieSmile)
            .Line(DialogueCharacter.Jackie, "Hey Cam! I hear ya.")
            .Line(DialogueCharacter.Cam, "Good, how's it going out there?", DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Jackie,
                "Mostly dull. We're on lockdown, so no \"official\" mining business...")
            .Line(DialogueCharacter.Jackie,
                "But I've been getting in some extra scouting at night!",
                DialogueSprite.JackieSmile)
            .Line(DialogueCharacter.Cam, "Jackie, are you insane!? By yourself?", DialogueSprite.CamConfused)
            .Line(DialogueCharacter.Jackie,
                "Relax. I'm taking the form of that Frog. Your glove works like a charm.",
                DialogueSprite.JackieWry)
            .Line(DialogueCharacter.Jackie,
                "Besides, something's not right about it. It's too... organized.",
                DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie,
                "It's been hoarding crystals, dragging them deep into the mountains.") 
            .Line(DialogueCharacter.Jackie, "I was planning on getting a closer look.")
            .Line(DialogueCharacter.Cam, "Jackie, you can't just... I swear, what part of \"danger\" don't you get?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "I get \"danger\" just fine. What I'm doing is worth it. ", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie, "Besides, the resupply is arriving soon anyway.")
            .Line(DialogueCharacter.Jackie, "I heard they’re sending us a fresh batch of serums.", DialogueSprite.JackieSmile)
            .Line(DialogueCharacter.Jackie, "Once they arrive, we'll be able to go out officially again.")
            .Line(DialogueCharacter.Jackie, "I won't have to sneak around for much longer.")
            .Line(DialogueCharacter.Cam, "Yeah, about that…")
            .Line(DialogueCharacter.Cam, "Make sure to check the serum against a light every time before you use it. If there's any colour in it, don't.")
            .Line(DialogueCharacter.Cam, "And even if it’s clear and you use it. Make sure you come back immediately and get checked up, okay?")
            .Line(DialogueCharacter.Jackie, "Yeah, yeah. Whatever, mom.", DialogueSprite.JackieSmile)
            .Line(DialogueCharacter.Cam, "Hey, I'm serious! The mountains are a signal dead zone.", DialogueSprite.CamPout)
            .Line(DialogueCharacter.Cam, "If something happens...")
            .Line(DialogueCharacter.Jackie, "Then it happens. Look. I didn’t pick up the phone to be lectured. What do you want?" , DialogueSprite.JackieRetort)
            .Line(DialogueCharacter.Cam, "... I wanted to talk about Ives.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "Oh yeah, I heard! She'll be back.", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Jackie, "She's getting better right?")
            .Line(DialogueCharacter.Cam, "...", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Jackie, "...Cam?", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Cam,
                "I'm sorry Jackie. We couldn't purge the Waste in time.",
                DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Cam, "Her condition...")
            .Line(DialogueCharacter.Cam, "...It's permanent.")
            .Line(DialogueCharacter.Jackie, "...Permanent?", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Cam,
                "She'll be on suppressants, so she'll... she'll look fine.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam,
                "But we're worried that by returning to the field, the exertion will make her feel worse.")
            .Line(DialogueCharacter.Jackie, "...", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Cam, "...Jackie? You still there?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "...Yeah. Sorry. That... I just... Shit.", DialogueSprite.JackieAstonished)
            .Narrate("<i>Jackie looks to the ground, running a hand through her hair.</i>")
            .Line(DialogueCharacter.Jackie,
                "If only I'd been faster. If I'd noticed that damn injection...", //Pained
                DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Cam, "Jackie, no. It wasn't your fault.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "We're all glad you noticed at all...")
            .Line(DialogueCharacter.Jackie, "Cam...", DialogueSprite.JackieSurprisedClosed) // closed eyes /tired
            .Line(DialogueCharacter.Jackie,
                "When I stood there seeing the doctors take Ives away... I had a thought.")
            .Line(DialogueCharacter.Jackie, "It's happening again. First Ma... and now Ives.", DialogueSprite.JackieSurprisedOpen)
            .Line(DialogueCharacter.Jackie, "Everyone who's been there for my sake has paid for it.")
            .Line(DialogueCharacter.Jackie, "And I let it happen.")
            .Line(DialogueCharacter.Cam, "Jackie, the Waste is at fault. Not you.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "But I'm the only part of this I can do anything about.", DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Jackie, "Blaming the Waste won't bring Ma back... and it can't fix Ives.")
            .Line(DialogueCharacter.Jackie, "I'm putting a stop to this. No one else is following me.")
            .Line(DialogueCharacter.Jackie, "So it's not you next.") // Pained
            .InterruptedLine(DialogueCharacter.Cam, "Jackie, you can't just—", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Jackie, "Finding Ma is my responsibility now, you hear me?", DialogueSprite.JackieRetort)
            .Line(DialogueCharacter.Jackie, "I'm the one who has to prove that I'm strong enough to protect myself now.")
            .Line(DialogueCharacter.Jackie, "That I don't need anyone else... and I won't vanish like her.") // Pained
            .Line(DialogueCharacter.Cam, "...")
            .Line(DialogueCharacter.Cam, "You know, I've also been running that same day in my head.",
                DialogueSprite.CamNeutral) // /expr
            .Line(DialogueCharacter.Cam, "A day where I caught the flaw during testing.")
            .Line(DialogueCharacter.Cam, "Because if its my fault then maybe there was something I could've done.")
            .Line(DialogueCharacter.Cam, "I've been looking every night, and I've found nothing.")
            .Line(DialogueCharacter.Cam, "Just myself alone in a lab.", DialogueSprite.CamSerious) 
            .Line(DialogueCharacter.Cam, "So I've made a choice. With... Dr. Weise, I'm going to help Ives with everything I’ve got.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "While Ives is choosing to do the same for you.")
            .Line(DialogueCharacter.Cam, "It takes a lot to make those choices.")
            .Line(DialogueCharacter.Cam, "So you don't get to just unmake them for us!", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Jackie, "But what is that choice costing her?", DialogueSprite.JackieSerious) // /expr
            .Line(DialogueCharacter.Jackie, "How can I let her pay that price for me!?", DialogueSprite.JackieRetort)
            .Line(DialogueCharacter.Cam,
                "There's no \"letting\" Jackie. She's not asking.",
                DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "Only she knows exactly what it costs her.")
            .Line(DialogueCharacter.Cam, "And yet she's still choosing to be out here for you.")
            .Line(DialogueCharacter.Cam,
                "So why can't you then trust us to decide for ourselves?",
                DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Jackie, "Because that trust... means nothing.", DialogueSprite.JackieSerious) // /pained
            .Line(DialogueCharacter.Jackie, "My Ma was the strongest person alive. She promised she'd be back.")
            .Line(DialogueCharacter.Jackie, "And... And she's gone.") // Pained 
            .Line(DialogueCharacter.Jackie, "Ives promised we'd figure this out together.")
            .Line(DialogueCharacter.Jackie, "And she's sick.")
            .Line(DialogueCharacter.Jackie, "Now you're asking me to trust you?")
            .Line(DialogueCharacter.Jackie, "Nobody's word out here can be kept for certain, Cam.")
            .Line(DialogueCharacter.Jackie, "So tell me. How can I trust yours?")
            .Line(DialogueCharacter.Cam, "...", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Jackie,
                "... Besides, there's no \"together,\" right now Cam.",
                DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Jackie, "You're at the base, stuck in a lab. Ives is... days away.")
            .Line(DialogueCharacter.Jackie, "Command has us all on lockdown.")
            .Line(DialogueCharacter.Jackie, "I just can't wait around for others anymore.") // Pained
            .Line(DialogueCharacter.Jackie, "I'm the only one out here who can do anything.")
            .Line(DialogueCharacter.Jackie, "And if I lose that Princess Frog? We lose our only lead.")
            .Line(DialogueCharacter.Cam, "...You—", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Cam, "Dammit.")
            .Line(DialogueCharacter.Cam, "*sigh* Fine....", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "I can't stop you, can I?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "...") // Jackie Sad. 
            .Line(DialogueCharacter.Cam, "Well if you're set on going in, you're not going in there blind.")
            .Line(DialogueCharacter.Cam,
                "I'll be working with Dr. Weise on a contingency for Ives. And it's all centered on that Frog.")
            .Line(DialogueCharacter.Cam, "I'm sending a data packet to your glove right now.")
            .Line(DialogueCharacter.Cam,
                "It should give you an edge by allowing you to track the frequency tones it emits when you get closer.")
            .Line(DialogueCharacter.Cam, "And send me your coordinates each time you go.")
            .Line(DialogueCharacter.Jackie, "...", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Cam,
                "Just like you said before. Don't promise, just do it.",
                DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "We do this together, even when we're apart. Got it?", DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Jackie, "...Fine. I'll send it.", DialogueSprite.JackieFocused) // Closed mouth Regular. 
            .Exit(DialogueCharacter.Cam, DialogueCharacter.Jackie);
    }
}

using DialogueScripts;
using LevelSelectInformation;
using System.Collections;
using UnityEngine;

namespace Dialogue.Epilogue
{
    public class Epilogue_6 : MonoBehaviour
    {
        private IEnumerator Start()
        {
            UIFadeScreenManager.Instance.SetDarkScreen();
            SoundID.VN_radio_static.Play(); // *Radio static noise /sfx
            yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f); // [Fade into the Tundra Lab split screen BG]
            yield return new WaitForSeconds(1f);
            yield return DialogueBoxV2.Instance.Play(Epilogue_6_Dialogue.Dialogue);
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f); // [fade to black]

            new BountyInformationEvent(BountyInformation.Get<BountyInformation.PrincessFrogBounty>()).Invoke();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.ContractSelect>().SceneName);
        }
    }

    public static class Epilogue_6_Dialogue
    {
        public static DialogueAsCode Dialogue => new DialogueAsCode()
            .Enter(DialogueCharacter.Cam, CharacterActions.SetLeft, DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "Hey Jackie, connection alright?")

            .Enter(DialogueCharacter.Jackie, CharacterActions.SetRight, DialogueSprite.JackieWry)
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
                DialogueSprite.JackieSmile)
            .Line(DialogueCharacter.Jackie,
                "Besides, something's not right about it. It's too... organized.",
                DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie,
                "It's been hoarding crystals, dragging them deep into the mountains.") 
            .Line(DialogueCharacter.Jackie, "I was planning on getting a closer look.")
            .Line(DialogueCharacter.Cam,
                "...Jackie, you can't just... I swear, what part of \"danger\" don't you get?",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "Just once, could you wait for backup?")
            .Line(DialogueCharacter.Jackie, "Whatever, mom.", DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Cam,
                "Hey, I'm serious! The mountains are a signal dead zone.",
                DialogueSprite.CamPout)
            .Line(DialogueCharacter.Cam, "If something happens...")
            .Line(DialogueCharacter.Jackie,
                "Then it happens. Look. The resupply is arriving soon anyway.",
                DialogueSprite.JackRetort)
            .Line(DialogueCharacter.Jackie, "I heard they're sending us new serums.")
            .Line(DialogueCharacter.Jackie, "Once they arrive, we'll be able to go out officially again.", DialogueSprite.JackieFocused)
            .Line(DialogueCharacter.Jackie, "I won't have to sneak around for much longer.")
            .Line(DialogueCharacter.Cam, "Yeah, about that...", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "... I wanted to talk about Ives.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "Yeah, I heard! She'll be back.", DialogueSprite.JackieSmile)
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
            .Line(DialogueCharacter.Jackie, "Everyone who's done anything for my sake has paid for it.")
            .Line(DialogueCharacter.Jackie, "And I let it happen.")
            .Line(DialogueCharacter.Cam, "Jackie, the Waste is at fault. Not you.", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "But I'm the only part of this I can do anything about.", DialogueSprite.JackieSerious)
            .Line(DialogueCharacter.Jackie, "Blaming the Waste won't bring Ma back... and it can't fix Ives.")
            .Line(DialogueCharacter.Jackie, "It's time to put a stop this. To stop anyone else from following me.")
            .Line(DialogueCharacter.Jackie, "So it's not you next.") // Pained
            .InterruptedLine(DialogueCharacter.Cam, "Jackie, you can't just—", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Jackie, "Finding Ma is my responsibility now, you hear me?", DialogueSprite.JackRetort)
            .Line(DialogueCharacter.Jackie, "I'm the one who has to prove to her that I'm strong enough to protect myself.")
            .Line(DialogueCharacter.Jackie, "That I don't need anyone else... and I won't vanish like her.") // Pained
            .Line(DialogueCharacter.Cam, "...")
            .Line(DialogueCharacter.Cam, "You know, I've also been rerunning that same day in my head.",
                DialogueSprite.CamNeutral) // /expr
            .Line(DialogueCharacter.Cam, "A version of that day where I caught the flaw during testing.")
            .Line(DialogueCharacter.Cam, "Because if it was my fault then maybe there was something I could've done.")
            .Line(DialogueCharacter.Cam, "I've been looking every night, and I've found nothing.")
            .Line(DialogueCharacter.Cam,
                "All that blame and the only thing I found is myself alone in my lab.",
                DialogueSprite.CamSerious) 
            .Line(DialogueCharacter.Cam,
                "So I've made a choice. To give what I've actually got to helping Ives with... Dr. Weise.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam, "While Ives is choosing to do the same for you.")
            .Line(DialogueCharacter.Cam, "It takes a lot to make those choices.")
            .Line(DialogueCharacter.Cam, "So you don't get to just unmake them for us!", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Jackie, "But what is that choice costing her?", DialogueSprite.JackieSerious) // /expr
            .Line(DialogueCharacter.Jackie, "How can I let her pay that price for me!?", DialogueSprite.JackRetort)
            .Line(DialogueCharacter.Cam,
                "There's no \"letting\" Jackie. Because she's not asking.",
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
            .Line(DialogueCharacter.Jackie, "There's no promise out here that anyone can keep for certain, Cam.")
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
            .Line(DialogueCharacter.Cam, "...", DialogueSprite.CamSerious)
            .Line(DialogueCharacter.Cam, "Dammit.")
            .Line(DialogueCharacter.Cam, "*sigh* Fine....", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Cam, "I can't stop you, can I?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "...") // Jackie Sad. 
            .Line(DialogueCharacter.Cam, "Well if you're set on going in, you're not going in there blind.")
            .Line(DialogueCharacter.Cam,
                "I'll be working with Dr. Weise on a contingency for Ives. And it's all centered on that Frog.")
            .Line(DialogueCharacter.Cam, "I'm sending a data packet to your glove right now.")
            .Line(DialogueCharacter.Cam,
                "It should give you an edge by allowing you to track its frequency signature when you get closer.")
            .Line(DialogueCharacter.Cam, "And send me your coordinates each time you go.")
            .Line(DialogueCharacter.Jackie, "...", DialogueSprite.JackieSurprisedClosed)
            .Line(DialogueCharacter.Cam,
                "Just like you said before. Don't promise, just do it.",
                DialogueSprite.CamSmile)
            .Line(DialogueCharacter.Cam, "We do this together, even when we're apart. Got it?", DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Jackie, "...Fine. I'll send it.") // Closed mouth Regular. 
            .Exit(DialogueCharacter.Cam, DialogueCharacter.Jackie);
    }
}

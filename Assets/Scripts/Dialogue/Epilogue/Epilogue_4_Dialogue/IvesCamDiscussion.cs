using DialogueScripts;

namespace Epilogue4
{
    public static class IvesCamDiscussion
    {
        public static DialogueAsCode PartA => new DialogueAsCode()
            .Line(DialogueCharacter.Cam, "Ives... I'm so sorry...", DialogueSprite.CamTalk)
            .Enter(DialogueCharacter.Ives, CharacterActions.SetRight, DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "Don't you start pitying me, kid. We both know this had nothing to do with you.",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Cam,
                "But it was my invention that failed to protect us... failed you.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Ives,
                "Hey, I knew the risks. All of us did.",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "Far as I see it, your invention got the NITES further than anyone else ever has. One step closer to Ailin. One step closer to the truth.")
            .Line(DialogueCharacter.Cam,
                "I should've known about the reaction, about the Frog. If only I knew more...",
                DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Ives,
                "Hah, I'm sure if you had known, we wouldn't have been able to step out that morning.",
                DialogueSprite.IvesLaugh)
            .Line(DialogueCharacter.Cam, "...", DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Ives,
                "Listen, I get it. That terrible feeling of 'what could' if you had just known...",
                DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives,
                "But you didn't. No one did. Hell, if there was someone who knew, I'd like to have a 'word' with them.",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "So no one, especially not you, can blame you for not knowing.")
            .Line(DialogueCharacter.Cam,
                "But it's my job as a scientist to know! I thought it was safe. I said it was safe.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Cam,
                "My whole life has been spent figuring out the Waste to help people...")
            .Line(DialogueCharacter.Cam,
                "And for what... to end up hurting those closest to me?")
            .Line(DialogueCharacter.Ives, "...", DialogueSprite.IvesNeutral);

        public static DialogueAsCode PartB => new DialogueAsCode()
            .Line(DialogueCharacter.Ives,
                "Cam. I need you to understand something.",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives, "I know you feel like shit right now.")
            .Line(DialogueCharacter.Ives,
                "You're wondering how anyone can trust a piece of trash like you ever again.")
            .Line(DialogueCharacter.Ives, "How you can even begin to trust yourself.")
            .Line(DialogueCharacter.Ives,
                "Been there, kid. Drove that road to the end and back.",
                DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives,
                "But you know where that suffocating pressure comes from?",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "From an ambitious promise that you've made to yourself.")
            .Line(DialogueCharacter.Ives,
                "That you must become a perfect scientist, and realize your full potential.")
            .Line(DialogueCharacter.Ives,
                "Because you figure that only the best version of yourself can navigate this uncertain world for you.")
            .Line(DialogueCharacter.Ives,
                "So you tail their bumper, turn for turn.")
            .Line(DialogueCharacter.Ives, "Figure if you can just match 'em, the doubt of whether you'll finally reach the destination will finally quiet down.")
            .Line(DialogueCharacter.Ives,
                "But that's a tough drive kid, cuz they take those turns better than you do.")
            .Line(DialogueCharacter.Ives,
                "And when you pop a tire chasing that perfect driver in front, they'll just keep cruising. Won't even spare a glance in the rear view mirror.")
            .Line(DialogueCharacter.Ives,
                "While you're there stuck on the shoulder, jack under the car, asking ourselves, \"Can I ever catch up?\"")
            .Line(DialogueCharacter.Ives,
                "Maybe you will. But that ideal self can't help you change this tire, kid.")
            .Line(DialogueCharacter.Ives, "Cuz they never blew one!", DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives,
                "They always had the perfect map, and the perfect roads. That's their secret.")
            .Line(DialogueCharacter.Ives,
                "That blowout you had? That wasn't a fluke on your road. That's just the road. It simply wasn't as paved as you thought it'd be.")
            .Line(DialogueCharacter.Ives,
                "And look, I won't lie to you. A busted tire ain't like dust on a windshield.", DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives, "You can't just wipe it off and pretend it didn't happen.")
            .Line(DialogueCharacter.Ives, "It'll take time to fix, and the patch is going to be there a while.")
            .Line(DialogueCharacter.Ives, "...")
            .Line(DialogueCharacter.Ives,
                "Heh, I might be driving on a couple patches myself too, kid.",
                DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives,
                "But, now you have an opportunity to prove to yourself something better.",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "That even with a busted tire, you don't abandon your wheel.")
            .Line(DialogueCharacter.Ives, "You patch up, learn to let a little air out of the tires to better grip the rough, and steer yourself a new way forward.")
            .Line(DialogueCharacter.Ives,
                "Because now there's one less thing about this road that you don't know, and one more thing that you do.")
            .Line(DialogueCharacter.Ives,
                "And that's what real trust is. It's not waiting for the day your perfect self gets to take the wheel.", DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Ives,
                "It's knowing that the grease-stained version of you, who gets the final say over where to go next, is getting better at driving.")
            .Line(DialogueCharacter.Ives, "Got it?")

            .Narrate("<i>Cam stays silent for a while, and eventually says.</i>")
            .Line(DialogueCharacter.Cam,
                "Do you think you've found where you want to go?",
                DialogueSprite.CamNeutral)
            .Line(DialogueCharacter.Ives, "...", DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "I dunno. But, I trust I'll adjust if I find out that I didn't.")
            .Line(DialogueCharacter.Cam,
                "...But what if you can't? What if it's too late?")
            .Line(DialogueCharacter.Ives,
                "Then I only could have known if I had driven it to the end.")
            .Line(DialogueCharacter.Ives,
                "Where I end up doesn't bother me much anymore. I know I'm moving in the direction that matters to me most.")
            .Line(DialogueCharacter.Ives,
                "You have to learn to trust that instinct kiddo. Cuz when the map fails, that's the only thing that is entirely yours.")
            .Line(DialogueCharacter.Cam, "I see...")
            .Line(DialogueCharacter.Ives,
                "So, what are you going to do now?",
                DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Cam,
                "I'm going to get some answers.",
                DialogueSprite.CamTalk)
            .Line(DialogueCharacter.Ives,
                "Damn straight you are. C'mere.",
                DialogueSprite.IvesSmile)
            .Line(DialogueCharacter.Cam, "Thank you, Ives.", DialogueSprite.CamSmile);
    }
}

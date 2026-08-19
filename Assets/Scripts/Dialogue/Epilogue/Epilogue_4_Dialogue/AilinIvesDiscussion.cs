using DialogueScripts;
using Dialogue.Epilogue;

namespace Epilogue4
{
    public static class AilinIvesDiscussion
    {
        public static DialogueAsCode PartA => new DialogueAsCode()
            .Line(DialogueCharacter.Ailin,
                "Ives. You have a moment? It's about the Jay situation.")
            .Enter(DialogueCharacter.Ives, CharacterActions.SetMiddle, DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives,
                "Yeah, I heard already. Headquarters placed your daughter under my watch. Cruel way to tell a gal she ain't up to stuff huh?")
            .Line(DialogueCharacter.Ailin,
                "No Ives. I asked for you specifically.")
             .Do(new CustomEvent { EventName = Epilogue_4.WEAK })
            .Line(DialogueCharacter.Ives,
                "Yeah. I figured, that's where it hurts particularly.")

            .Line(DialogueCharacter.Ailin, "...")
            .Do(new CustomEvent { EventName = Epilogue_4.WORTHLESS })
            .Line(DialogueCharacter.Ives,
                "You know, we've been working together for quite some time now. But I can't help but feel that I don't belong here.")
            .Line(DialogueCharacter.Ives,
                "A scrap rat like me? Working for the government? Tsk, what were you thinking when you recruited me.")
            .Do(new CustomEvent { EventName = Epilogue_4.PATHETIC })
            .Line(DialogueCharacter.Ives,
                "And here I am playing babysitter behind the walls and people that I'm supposed to protect. Tell me, what does that make me?")
            .Line(DialogueCharacter.Ailin,
                "A friend I trust more than anything.")
            .Do(new CustomEvent { EventName = Epilogue_4.RESOLVE })
            .Line(DialogueCharacter.Ives, "Please, don't.")
            .Line(DialogueCharacter.Ailin, "I mean it.")
            .Line(DialogueCharacter.Ailin, "It's why I wanted to catch you today.")
            .Line(DialogueCharacter.Ailin,
                "We'll be going in blind into the Tundra. Though I'm confident the team can manage, I promised Jackie I'd be back soon.")
            .Line(DialogueCharacter.Ailin,
                "So if there are any delays out there... I need someone strong and capable to look after her.")
            .Line(DialogueCharacter.Ives, "You're her mother, why don't you just stay?")
            .Line(DialogueCharacter.Ailin,
                "...Because I'm the one who sent Jay in. So I'm the one who has to get him back.")
            .Line(DialogueCharacter.Ailin,
                "There's no world where I sit here letting someone else take care of that.")
            .Line(DialogueCharacter.Ives, "Then you must know how I feel.")
            .Line(DialogueCharacter.Ailin, "...")
            .Line(DialogueCharacter.Ailin,
                "Shit Ives... you always know where to strike huh.")
            .Line(DialogueCharacter.Ives, "...")
            .Line(DialogueCharacter.Ives,
                "I'll watch your kid.")
            .Line(DialogueCharacter.Ives, "But I swear, you better come back or I'll bring you back myself.")
            .Line(DialogueCharacter.Ives, "I won't play babysitter forever.")
            .Line(DialogueCharacter.Ailin, "...Wouldn't dream of it.")
            .Line(DialogueCharacter.Ives, "...", DialogueSprite.IvesQuestioning)
            .Line(DialogueCharacter.Ives, "Ailin, are you s...")
            .Line(DialogueCharacter.Ailin, "Mm?")
            .Line(DialogueCharacter.Ives, "...", DialogueSprite.IvesNeutral)
            .Line(DialogueCharacter.Ives, "...Nothing. Go bring Jay home for me.")
            .Exit(DialogueCharacter.Ives);
    }
}

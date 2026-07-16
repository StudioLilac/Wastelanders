#nullable enable
using DialogueScripts;


/// Epilogue 3 opening "Driving" scene.
public class Epilogue3Driving
{
    public DialogueAsCode PartA => new DialogueAsCode()
        .Enter(DialogueCharacter.Cam, CharacterActions.SetOffscreenLeft, DialogueSprite.CamTalk)
        .Enter(DialogueCharacter.Jackie, CharacterActions.SetLeft, DialogueSprite.JackieMouthOpen)
        .Enter(DialogueCharacter.Ives, CharacterActions.SetRight, DialogueSprite.IvesNeutral)

        .Line(DialogueCharacter.Jackie,
            "Ow my ass, what’s with all these holes!? It’s like the road has been chewed up and spat out.",
            DialogueSprite.JackieMouthOpen)

        .Enter(DialogueCharacter.Cam, CharacterActions.SetMiddle, DialogueSprite.CamTalk)
        .Line(DialogueCharacter.Cam,
            "I don’t understand, the map says this is supposed to be a smooth straight road. Ideal conditions for a convoy.")
        .Do(new CustomEvent { EventName = Epilogue_3.STOP_ENGINE })
        .Narrate("<i>Suddenly the tire bursts.</i>", SoundID.VN_tire_burst)
        .Line(DialogueCharacter.Ives, "There goes the rear tire.")
        .Line(DialogueCharacter.Jackie,
            "Just great. Ives, tell me we can still drive on that. We can't stop here in the middle of... whatever this is.", DialogueSprite.JackieSerious)
        .Line(DialogueCharacter.Ives, "Grab the jack and the spare, kid.", sfx: SoundID.VN_door_shut)
        .Line(DialogueCharacter.Jackie,
            "Cam, if your map can't even get a detail like this right, how can we trust it to get to where we wanna go?",
            DialogueSprite.JackieMouthOpen)
        .Line(DialogueCharacter.Cam,
            "I really thought the map was right...")
        .Line(DialogueCharacter.Cam,
            "Ives, don't the vehicles have automated systems, shouldn’t we just put it on auto-pilot?")
        .Line(DialogueCharacter.Ives,
            "Nah, the rest of the convoy can. But we’re up in front so they can have it that easy.")
        .Line(DialogueCharacter.Ives,
            "Our job is to get a feel for the route and relay it back.")
        .Line(DialogueCharacter.Ives,
            "You put it on auto-pilot, and the machine assumes the road is paved too.")
        .Line(DialogueCharacter.Ives,
            "I need to feel the wheel kick in my hands to be able to steer around the worst of these holes.")
        .Line(DialogueCharacter.Ives,
            "Besides, a flat ain't the end of the world. It just tells me what kinda ground we're dealing with here.", sfx: SoundID.VN_jack_raising)
        .Line(DialogueCharacter.Ives,
            "Jackie, radio the convoy. Tell 'em to let some air out of their tires. Softer rubber grips the rough better.")
        .Line(DialogueCharacter.Jackie,
            "Copy that. I'll keep watch while you work.", DialogueSprite.JackieMouthOpen)
        .Line(DialogueCharacter.Jackie,
            "...How long do you think this'll take?")
        .Line(DialogueCharacter.Ives, "Long as it takes. Why?")
        .Line(DialogueCharacter.Jackie, "The light's already starting to go out here.")
        .Line(DialogueCharacter.Jackie, "I don’t like the idea of crawling through this jumble in the dark.")
        .Line(DialogueCharacter.Cam,
            "Don’t worry! I'm sure Ives will have us back on the road quickly. I'll run the numbers right now to get an ETA.",
            DialogueSprite.CamSmile)
        .InterruptedLine(DialogueCharacter.Cam, "We'll be there soon, I promise!")
        .Line(DialogueCharacter.Jackie, "...Don't promise. Just get it done.", DialogueSprite.JackieSerious)
        .Narrate("<i>After a short while, the grease covered Ives emerges from under the vehicle.</i>", sfx: SoundID.VN_drill)
        .Line(DialogueCharacter.Ives, "Let's get back to it.", DialogueSprite.IvesNeutral, sfx: SoundID.VN_door_shut)
        .Exit(DialogueCharacter.Jackie, DialogueCharacter.Cam, DialogueCharacter.Ives)
        .Narrate("<i>The engine revs back to life, as the vanguard continues on their way.</i>", SoundID.VN_engine_rev);

    /// Closing narration, played over the black screen after the fade.
    public DialogueAsCode PartB => new DialogueAsCode()
        .Narrate("<i>Eventually, the convoy comes to a halt at camp. A lingering sense of unease permeating through the tang of gasoline as the soldiers fan out to pitch camp.</i>");
}

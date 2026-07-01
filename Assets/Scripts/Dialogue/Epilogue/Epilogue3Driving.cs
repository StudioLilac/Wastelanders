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
            "Just great. Ives, tell me we can still drive on that. We can't stop here in the middle of... whatever this is.")
        .Line(DialogueCharacter.Ives, "Grab the jack and the spare, kid.", sfx: SoundID.VN_door_shut)
        .Line(DialogueCharacter.Jackie,
            "Cam, if your map can't even get a detail like this right, how can we trust it to get to where we wanna go?",
            DialogueSprite.JackieMouthOpen)
        .Line(DialogueCharacter.Cam,
            "I really thought the map was right...")
        .Line(DialogueCharacter.Cam,
            "Ives, don't the vehicles have automated systems, shouldn’t we just put it on auto-pilot?")
        .Line(DialogueCharacter.Ives,
            "Nah, maybe the rest of the convoy can. But we’re up in front precisely so they can have that choice.")
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
            "Copy that. I'll watch the perimeter while you work.")
        .Line(DialogueCharacter.Jackie,
            "Just... don't tell me it'll be a quick fix if it's wont. I want to know exactly how long we're delayed.")
        .Line(DialogueCharacter.Cam,
            "Let some air out, right. I'll... I'll recalibrate our heading based on our new speed over this debris.",
            DialogueSprite.CamNeutral)
        .Line(DialogueCharacter.Cam,
            "I promise I'll get us an accurate ETA.")
        .Line(DialogueCharacter.Jackie,
            "Don't promise. Just do it.", DialogueSprite.JackieSerious)
        .Narrate("<i>In a few short moments, the grease covered Ives emerges from under the vehicle.</i>", sfx: SoundID.VN_drill)
        .Line(DialogueCharacter.Ives, "Let's get back to it.", DialogueSprite.IvesNeutral, sfx: SoundID.VN_door_shut)
        .Exit(DialogueCharacter.Jackie, DialogueCharacter.Cam, DialogueCharacter.Ives)
        .Narrate("<i>The engine revs back to life, as the vanguard continues on their way.</i>", SoundID.VN_engine_rev);

    /// <summary>Closing narration, played over the black screen after the fade.</summary>
    public DialogueAsCode PartB => new DialogueAsCode()
        .Narrate("<i>Eventually, the convoy comes to a halt at camp. A lingering sense of unease permeating through the tang of gasoline as the soldiers fan out to pitch camp.</i>");
}

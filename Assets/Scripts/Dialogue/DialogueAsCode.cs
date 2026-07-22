using System;
using System.Collections.Generic;
using DialogueScripts;

#nullable enable
/// Builder for authoring dialogue in code instead of hand-wiring
public class DialogueAsCode
{
    private readonly ActorDatabase actors;
    private readonly List<DialogueEntry> entries = new();

    public DialogueAsCode() : this(RequireActorDatabase()) { }

    public DialogueAsCode(ActorDatabase actors)
    {
        this.actors = actors;
    }

    /// A spoken line: with the given expression, optionally playing a sound effect.
    public DialogueAsCode Line(DialogueCharacter speaker, string text, DialogueSprite expr = DialogueSprite.NoChange,  SoundID sfx = SoundID.None)
    {
        var actor = Resolve(speaker);
        entries.Add(new DialogueEntry(
            content: text,
            speaker: actor,
            sfxId: sfx,
            picture: null,
            events: (expr == DialogueSprite.NoChange)
                ? new()
                : new()
                {
                    new ExpressionChange { actor = actor, expression = expr }
                }
        ));
        return this;
    }

    public DialogueAsCode InterruptedLine(DialogueCharacter speaker, string text, DialogueSprite expr = DialogueSprite.NoChange, SoundID sfx = SoundID.None)
    {
        var actor = Resolve(speaker);
        var events = new List<DialogueEvents>() { new AutoAdvanceAfter { Time = 0.5f } };
        if (expr != DialogueSprite.NoChange) events.Add(new ExpressionChange { actor = actor, expression = expr });
        entries.Add(new DialogueEntry(
            content: text,
            speaker: actor,
            sfxId: sfx,
            picture: null,
            events: events
        ));
        return this;
    }

    /// An italicised narration / atmosphere beat with no staged speaker,
    /// optionally carrying a sound effect.
    public DialogueAsCode Narrate(string text, SoundID sfx = SoundID.None)
    {
        entries.Add(new DialogueEntry(
            content: text,
            speaker: actors.Narration,
            sfxId: sfx,
            picture: null,
            events: new List<DialogueEvents>()));
        return this;
    }

    /// Brings an actor on-stage at position with a starting
    /// expression and fades them in. 
    public DialogueAsCode Enter(DialogueCharacter character, CharacterActions position, DialogueSprite expr, float fadeDuration = 1f)
    {
        var actor = Resolve(character);
        return Do(
            new ExpressionChange { actor = actor, expression = expr },
            new ActorAction { actor = actor, action = position, duration = 0f },
            new ActorAction { actor = actor, action = CharacterActions.FadeOut, duration = 0 },
            new ActorAction { actor = actor, action = CharacterActions.FadeIn, duration = fadeDuration });
    }

    /// Fades the given characters off-stage. Emitted as a pure-event beat.
    public DialogueAsCode Exit(float fadeDuration, params DialogueCharacter[] leaving)
    {
        var events = new List<DialogueEvents>();
        foreach (var character in leaving)
        {
            events.Add(new ActorAction { actor = Resolve(character), action = CharacterActions.FadeOut, duration = fadeDuration });
        }
        return Do(events.ToArray());
    }

    public DialogueAsCode Exit(params DialogueCharacter[] leaving) => Exit(1f, leaving);

    /// Emit raw dialogue events as a pure-event beat (no text).
    public DialogueAsCode Do(params DialogueEvents[] events)
    {
        entries.Add(new DialogueEntry(
            content: string.Empty,
            speaker: actors.Event,
            sfxId: SoundID.None,
            picture: null,
            events: new List<DialogueEvents>(events)));
        return this;
    }

    public DialogueEntry[] Build() => entries.ToArray();

    public static implicit operator DialogueEntry[](DialogueAsCode d) => d.Build();

    private ActorProfile Resolve(DialogueCharacter character) => character switch
    {
        DialogueCharacter.Jackie => actors.Jackie,
        DialogueCharacter.Ives => actors.Ives,
        DialogueCharacter.Cam => actors.Cam,
        DialogueCharacter.Weise => actors.Weise,
        DialogueCharacter.Ailin => actors.Ailin,
        DialogueCharacter.Narration => actors.Narration,
        DialogueCharacter.Broadcast => actors.Broadcast,
        DialogueCharacter.Loudspeaker => actors.Loudspeaker,
        DialogueCharacter.Tutorial => actors.Tutorial,
        DialogueCharacter.Event => actors.Event,
        DialogueCharacter.Unknown => actors.Unkown,
        _ => throw new ArgumentOutOfRangeException(nameof(character), character, "Unmapped DialogueCharacter")
    };

    private static ActorDatabase RequireActorDatabase() =>
        new GetActorDatabase().Query()
        ?? throw new InvalidOperationException(
            "DialogueAsCode could not resolve an ActorDatabase. A DialogueBoxV2 must be present " +
            "in the scene to answer GetActorDatabase before dialogue is built.");
}

public enum DialogueCharacter
{
    Jackie,
    Ives,
    Cam,
    Weise,
    Ailin,
    Narration,
    Broadcast,
    Loudspeaker,
    Tutorial,
    Event,
    Unknown,
}

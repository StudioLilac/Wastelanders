using System;
using System.Collections.Generic;

namespace Cinematics
{
#nullable enable
    public enum RevealMode
    {
        /// Characters appear one at a time at the beat's rate. The default.
        Type,

        /// The whole line appears at once and fades up. For long narration you
        /// want read rather than heard.
        Fade,

        /// The whole line appears instantly. For shouts and hard cuts.
        Cut
    }

    public record ExitMode()
    {
        /// Alpha ramps to zero over the presenter's fade duration.
        public record Fade() : ExitMode;

        /// Instant. Pairs with an interrupted line or a hard cut to the next beat.
        public record Cut(float InterruptedMs) : ExitMode;
    }

    /// <summary>
    /// One caption in a boxless cinematic. Deliberately not a DialogueEntry:
    /// there is no actor on stage, no expression, no portrait and no box, so
    /// reusing that vocabulary would mean carrying six fields that are always null.
    /// The shared layer between the two systems is <see cref="PerformanceMarkup"/>,
    /// which both can run over their content strings.
    /// </summary>
    public sealed class CinematicBeat
    {
        public DialogueCharacter Speaker;

        /// Authored text, still containing markup. Parsed by the presenter.
        public string? Raw;

        /// Silence before the caption appears, in milliseconds.
        public float LeadMs;

        public RevealMode Reveal;

        /// Characters per second. Zero means use the speaker's current base rate.
        public float Rate;

        /// Milliseconds to hold after the last character. Negative means derive
        /// from word count.
        public float HoldMs = -1f;

        public ExitMode? Exit;

        /// Fired when the caption starts revealing, after the lead. Use inline
        /// {!name} instead when the cue should land on a specific word.
        public string? Cue;

        /// Set by the builder: true the first time a speaker appears, so the
        /// presenter can prefix a name once and let colour carry it afterwards.
        public bool ShowSpeakerLabel;

        /// Authoring index, surfaced on the presenter for jump-to-beat iteration.
        public int Index;
    }

    /// <summary>
    /// Fluent builder for boxless cinematic scripts.
    ///
    /// Base rates are stateful and apply to every subsequent beat by that speaker
    /// until changed, so an arc like Ives decaying from 20 cps to 12 across the
    /// scene costs four <see cref="SetRate"/> calls rather than thirty per-beat
    /// overrides.
    /// </summary>
    public sealed class CinematicScript
    {
        private readonly List<CinematicBeat> beats = new List<CinematicBeat>();
        private readonly Dictionary<DialogueCharacter, float> baseRates =
            new Dictionary<DialogueCharacter, float>();
        private readonly HashSet<DialogueCharacter> seen = new HashSet<DialogueCharacter>();

        /// Fallback when a speaker has no base rate set.
        public float DefaultRate { get; set; } = 32f;

        /// <summary>
        /// Sets the reveal rate for this speaker's subsequent beats. Call it more
        /// than once to shape an arc.
        /// </summary>
        public CinematicScript SetRate(DialogueCharacter speaker, float charsPerSecond)
        {
            baseRates[speaker] = charsPerSecond;
            return this;
        }

        public CinematicScript Line(
            DialogueCharacter speaker,
            string text,
            float lead = 0f,
            RevealMode reveal = RevealMode.Type,
            float rate = 0f,
            float hold = -1f,
            ExitMode? exit = null,
            string? cue = null)
        {
            bool first = seen.Add(speaker) && speaker != DialogueCharacter.Narration;

            beats.Add(new CinematicBeat
            {
                Speaker = speaker,
                Raw = text,
                LeadMs = lead,
                Reveal = reveal,
                Rate = rate > 0f ? rate : ResolveRate(speaker),
                HoldMs = hold,
                Exit = exit ?? new ExitMode.Fade(),
                Cue = cue,
                ShowSpeakerLabel = first,
                Index = beats.Count
            });

            return this;
        }

        public CinematicScript Narrate(
            string text,
            float lead = 0f,
            RevealMode reveal = RevealMode.Type,
            float rate = 0f,
            float hold = -1f,
            ExitMode? exit = null,
            string? cue = null) =>
            Line(DialogueCharacter.Narration, text, lead, reveal, rate, hold, exit, cue);

        /// <summary>
        /// A beat that fires a cue and shows nothing. Use for camera and audio
        /// moves that do not belong to any caption.
        /// </summary>
        public CinematicScript Beat(string cue, float lead = 0f, float hold = 0f) =>
            Line(DialogueCharacter.Event, string.Empty, lead, RevealMode.Cut, 0f, hold, new ExitMode.Cut(0f), cue);

        private float ResolveRate(DialogueCharacter speaker) =>
            baseRates.TryGetValue(speaker, out float r) ? r : DefaultRate;

        public CinematicBeat[] Build() => beats.ToArray();

        public static implicit operator CinematicBeat[](CinematicScript s) => s.Build();
    }
}

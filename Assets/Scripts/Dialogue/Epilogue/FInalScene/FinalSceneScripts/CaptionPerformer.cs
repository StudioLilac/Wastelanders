using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Renders <see cref="CinematicBeat"/>s as boxless captions by walking TMP's
    /// maxVisibleCharacters. Owns timing, not content: everything about how a line
    /// performs comes from the beat and its markup.
    /// </summary>
    public class CaptionPerformer : MonoBehaviour
    {
        [Serializable]
        public struct SpeakerStyle
        {
            public DialogueCharacter speaker;
            public Color color;
            public string label;
        }

        [Header("Target")]
        [SerializeField] private TextMeshProUGUI captionText;

        [Header("Styling")]
        [SerializeField]
        private SpeakerStyle[] speakerStyles =
        {
            new SpeakerStyle { speaker = DialogueCharacter.Narration, color = new Color(1f, 1f, 1f), label = "" },
            new SpeakerStyle { speaker = DialogueCharacter.Jackie, color = new Color(0f, 0.8f, 1f), label = "Jackie" },
            new SpeakerStyle { speaker = DialogueCharacter.Ives, color = new Color(1f, 0.3f, 0.1f), label = "Ives" },
            new SpeakerStyle { speaker = DialogueCharacter.Cam, color = new Color(1f, 0.8f, 0f), label = "Cam" },
            new SpeakerStyle { speaker = DialogueCharacter.Jay, color = new Color(0.7f, 0f, 0.9f), label = "Jay" }
        };

        [SerializeField] private Color fallbackColor = Color.magenta;

        [Header("Timing")]
        [SerializeField] private PunctuationProfile punctuation = new PunctuationProfile();

        [Tooltip("Derived hold = clamp(base + perWord * words, min, max), in milliseconds.")]
        [SerializeField] private float holdBaseMs = 500f;
        [SerializeField] private float holdPerWordMs = 70f;
        [SerializeField] private float holdMinMs = 500f;
        [SerializeField] private float holdMaxMs = 3000f;
        [SerializeField] private float interruptedMs = 200f;

        [SerializeField] private float fadeInSeconds = 0.35f;
        [SerializeField] private float fadeOutSeconds = 0.25f;

        [Header("Iteration")]
        [Tooltip("Skip straight to this beat index on play. Leave at 0 to run the scene from the top.")]
        [SerializeField] private int startAtBeat = 0;

        [SerializeField] private bool logBeatIndices = false;

        /// <summary>
        /// Fired for every cue, whether beat-level or inline. Cues dispatch at the
        /// moment the caption starts revealing, not after a fade completes.
        /// </summary>
        public event Action<string> OnCue;

        /// <summary>
        /// Fired as each beat begins, for logging and for jumping around during tuning.
        /// </summary>
        public event Action<CinematicBeat> OnBeatStarted;

        /// <summary>
        /// Time source, in seconds since the last frame. Defaults to unscaled delta.
        /// Swap this for an FMOD-derived delta when you wire the music sync so the
        /// performance drifts with the track rather than with the frame clock.
        /// </summary>
        public Func<float> DeltaTime { get; set; } = () => Time.unscaledDeltaTime;

        private Dictionary<DialogueCharacter, SpeakerStyle> styleLookup;
        private float fastForwardScale = 20f;
        private bool FastForward => Input.GetKey(KeyCode.RightArrow);

        private void Awake()
        {
            styleLookup = new Dictionary<DialogueCharacter, SpeakerStyle>();
            foreach (SpeakerStyle style in speakerStyles) styleLookup[style.speaker] = style;

            if (captionText != null)
            {
                captionText.alpha = 0f;
                captionText.text = string.Empty;
            }
        }

        /// <summary>
        /// The performer's own timing values, handed to CinematicSchedule so the
        /// offline computation cannot drift from what actually plays.
        /// </summary>
        public CinematicSchedule.Settings TimingSettings => new CinematicSchedule.Settings
        {
            Punctuation = punctuation,
            HoldBaseMs = holdBaseMs,
            HoldPerWordMs = holdPerWordMs,
            HoldMinMs = holdMinMs,
            HoldMaxMs = holdMaxMs,
            FadeInSeconds = fadeInSeconds,
            FadeOutSeconds = fadeOutSeconds,
            DefaultRate = 32f,
            InterruptedMs = interruptedMs,
        };

        public CinematicSchedule.Entry[] BuildSchedule(IReadOnlyList<CinematicBeat> beats) =>
            CinematicSchedule.Compute(beats, TimingSettings);

        public IEnumerator Play(IReadOnlyList<CinematicBeat> beats)
        {
            for (int i = Mathf.Max(0, startAtBeat); i < beats.Count; i++)
            {
                yield return PlayBeat(beats[i]);
            }

            captionText.alpha = 0f;
        }

        /// <summary>
        /// Plays anchored to a clock rather than by accumulating waits.
        ///
        /// Each beat holds until the clock reaches its scheduled start, so frame
        /// error and beat overruns cannot accumulate across the scene: a beat that
        /// runs long steals from the gap before the next one instead of pushing
        /// everything after it out of time with the music.
        ///
        /// Pass a start offset to begin partway in. Seek the clock to the matching
        /// position first, or the first beat will wait for the clock to catch up.
        /// </summary>
        public IEnumerator PlayFrom(
            IReadOnlyList<CinematicBeat> beats,
            Func<float> clock,
            float startSeconds = 0f)
        {
            CinematicSchedule.Entry[] schedule = BuildSchedule(beats);
            int first = startSeconds <= 0f
                ? Mathf.Max(0, startAtBeat)
                : CinematicSchedule.IndexAt(schedule, startSeconds);

            if (logBeatIndices)
                Debug.Log($"[Caption] starting at beat {first} " +
                          $"({CinematicSchedule.Timecode(schedule[first].Start)}), " +
                          $"total {CinematicSchedule.Timecode(CinematicSchedule.TotalSeconds(schedule))}");

            for (int i = first; i < beats.Count; i++)
            {
                float target = schedule[i].Start;

                while (clock() < target) yield return null;

                float late = clock() - target;
                if (late > 0.35f && logBeatIndices)
                    Debug.LogWarning($"[Caption] beat {i} started {late:F2}s late. " +
                                     "The previous beat overran its scheduled length.");

                yield return PlayBeat(beats[i]);
            }

            captionText.alpha = 0f;
        }

        public IEnumerator PlayBeat(CinematicBeat beat)
        {
            OnBeatStarted?.Invoke(beat);
            if (logBeatIndices) Debug.Log($"[Caption] beat {beat.Index}: {beat.Speaker} \"{beat.Raw}\"");

            if (beat.LeadMs > 0f) yield return Wait(beat.LeadMs);

            PerformedLine line = PerformanceMarkup.Parse(beat.Raw, punctuation);

            // Pure-event beats show nothing.
            if (line.VisibleLength == 0 && string.IsNullOrEmpty(beat.Raw))
            {
                DispatchCue(beat.Cue);
                foreach (TimedAction action in line.Actions)
                    if (action.Kind == ActionKind.Cue) DispatchCue(action.CueName);

                if (beat.HoldMs > 0f) yield return Wait(beat.HoldMs);
                yield break;
            }

            int prefixLength = ApplyTextAndStyle(beat, line);

            DispatchCue(beat.Cue);

            switch (beat.Reveal)
            {
                case RevealMode.Type:
                    yield return TypeReveal(beat, line, prefixLength);
                    break;

                case RevealMode.Fade:
                    ShowAll(prefixLength, line);
                    FlushNonPauseActions(line);
                    yield return FadeAlpha(1f, fadeInSeconds);
                    break;

                case RevealMode.Cut:
                    ShowAll(prefixLength, line);
                    FlushNonPauseActions(line);
                    captionText.alpha = 1f;
                    break;
            }

            yield return HandleHold(line, beat);
            yield return HandleExitMode(line, beat);
        }

        private IEnumerator HandleHold(PerformedLine line, CinematicBeat beat)
        {
            float hold = beat.HoldMs >= 0f
                ? beat.HoldMs
                : (line.EndsInterrupted ? 0f : DeriveHold(line.WordCount));

            if (hold > 0f) yield return Wait(hold);
        }

        private IEnumerator HandleExitMode(PerformedLine line, CinematicBeat beat)
        {
            ExitMode exit = line.EndsInterrupted && beat.HoldMs < 0f ? new ExitMode.Cut(interruptedMs) : beat.Exit;

            if (exit is ExitMode.Cut cut)
            {
                yield return Wait(cut.InterruptedMs);
                captionText.alpha = 0f;
            }
            else
            {
                yield return FadeAlpha(0f, fadeOutSeconds);
            }
        }

        /// <returns>Visible length of the speaker label prefix, which is never typed.</returns>
        private int ApplyTextAndStyle(CinematicBeat beat, PerformedLine line)
        {
            SpeakerStyle style = styleLookup.TryGetValue(beat.Speaker, out SpeakerStyle s)
                ? s
                : new SpeakerStyle { color = fallbackColor, label = beat.Speaker.ToString() };

            captionText.color = style.color;

            string prefix = beat.ShowSpeakerLabel && !string.IsNullOrEmpty(style.label)
                ? style.label + ": "
                : string.Empty;

            captionText.text = prefix + line.Text;
            captionText.maxVisibleCharacters = 0;
            captionText.ForceMeshUpdate();

            return prefix.Length;
        }

        private void ShowAll(int prefixLength, PerformedLine line) =>
            captionText.maxVisibleCharacters = prefixLength + line.VisibleLength;

        /// <summary>
        /// Fade and Cut have no reveal timeline, so their cues fire at once and
        /// their inline pauses are ignored. Rate changes are meaningless there too.
        /// </summary>
        private void FlushNonPauseActions(PerformedLine line)
        {
            foreach (TimedAction action in line.Actions)
                if (action.Kind == ActionKind.Cue) DispatchCue(action.CueName);
        }

        private IEnumerator TypeReveal(CinematicBeat beat, PerformedLine line, int prefixLength)
        {
            // The label is part of the caption but is not performed: it snaps in
            // with the first character.
            captionText.maxVisibleCharacters = prefixLength;
            captionText.alpha = 1f;

            float rate = beat.Rate > 0f ? beat.Rate : 32f;
            float budget = 0f;
            int revealed = 0;
            int actionIndex = 0;
            IReadOnlyList<TimedAction> actions = line.Actions;

            while (true)
            {
                // Actions land before the character at their index is revealed,
                // which includes index == VisibleLength for trailing cues.
                while (actionIndex < actions.Count && actions[actionIndex].VisibleIndex <= revealed)
                {
                    TimedAction action = actions[actionIndex++];
                    switch (action.Kind)
                    {
                        case ActionKind.Cue:
                            DispatchCue(action.CueName);
                            break;
                        case ActionKind.Rate:
                            rate = action.Value;
                            break;
                        case ActionKind.Pause:
                            if (action.Value > 0f) yield return Wait(action.Value);
                            break;
                    }
                }

                if (revealed >= line.VisibleLength) break;

                // Accumulate a character budget so the reveal is frame-rate
                // independent and high rates can emit several characters per frame.
                while (budget < 1f)
                {
                    budget += DeltaTime() * rate * (FastForward ? fastForwardScale : 1f);
                    yield return null;
                }

                budget -= 1f;
                revealed++;
                captionText.maxVisibleCharacters = prefixLength + revealed;
            }

            captionText.maxVisibleCharacters = prefixLength + line.VisibleLength;
        }

        private float DeriveHold(int wordCount) =>
            Mathf.Clamp(holdBaseMs + holdPerWordMs * wordCount, holdMinMs, holdMaxMs);

        private void DispatchCue(string cue)
        {
            if (string.IsNullOrEmpty(cue)) return;
            OnCue?.Invoke(cue);
        }

        private IEnumerator Wait(float milliseconds)
        {
            float remaining = milliseconds / 1000f;
            while (remaining > 0f)
            {
                remaining -= DeltaTime() * (FastForward ? fastForwardScale : 1f);
                yield return null;
            }
        }

        private IEnumerator FadeAlpha(float target, float duration)
        {
            if (duration <= 0f)
            {
                captionText.alpha = target;
                yield break;
            }

            float start = captionText.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += DeltaTime() * (FastForward ? fastForwardScale : 1f);
                captionText.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            captionText.alpha = target;
        }
    }
}

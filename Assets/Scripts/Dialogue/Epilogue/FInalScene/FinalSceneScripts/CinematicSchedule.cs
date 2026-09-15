using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Cinematics
{
    /// <summary>
    /// Works out when every beat starts, without playing the scene.
    ///
    /// This mirrors CaptionPerformer's timing exactly: same reveal loop, same
    /// punctuation pauses, same hold derivation. If the two ever disagree, the
    /// performer is the authority and this is the bug.
    /// </summary>
    public static class CinematicSchedule
    {
        /// <summary>
        /// Everything CaptionPerformer uses to decide how long a beat takes.
        /// Passed in rather than duplicated, so there is one source of truth.
        /// </summary>
        public struct Settings
        {
            public PunctuationProfile Punctuation;
            public float HoldBaseMs;
            public float HoldPerWordMs;
            public float HoldMinMs;
            public float HoldMaxMs;
            public float FadeInSeconds;
            public float FadeOutSeconds;
            public float DefaultRate;
            public float InterruptedMs;
        }

        public struct Entry
        {
            public int Index;
            public DialogueCharacter Speaker;
            public string Text;

            /// Seconds from the start of the caption sequence.
            public float Start;

            public float Duration;
            public float End => Start + Duration;

            /// Seconds spent on lead, reveal, hold and exit respectively.
            public float Lead, Reveal, Hold, Exit;
        }

        public static Entry[] Compute(IReadOnlyList<CinematicBeat> beats, Settings settings)
        {
            var entries = new Entry[beats.Count];
            float cursor = 0f;

            for (int i = 0; i < beats.Count; i++)
            {
                CinematicBeat beat = beats[i];
                PerformedLine line = PerformanceMarkup.Parse(beat.Raw, settings.Punctuation);

                float lead = beat.LeadMs / 1000f;

                float reveal;
                switch (beat.Reveal)
                {
                    case RevealMode.Type: reveal = RevealSeconds(beat, line, settings); break;
                    case RevealMode.Fade: reveal = settings.FadeInSeconds; break;
                    default: reveal = 0f; break;
                }

                float hold = beat.HoldMs >= 0f
                    ? beat.HoldMs / 1000f
                    : (line.EndsInterrupted
                        ? 0f
                        : Clamp(settings.HoldBaseMs + settings.HoldPerWordMs * line.WordCount,
                                settings.HoldMinMs, settings.HoldMaxMs) / 1000f);

                float exit = true switch
                {
                    var _ when line.EndsInterrupted && beat.HoldMs < 0f => settings.InterruptedMs / 1000f,
                    var _ when beat.Exit is ExitMode.Cut cut => cut.InterruptedMs / 1000f,
                    _ => settings.FadeOutSeconds,
                };
                    
                entries[i] = new Entry
                {
                    Index = i,
                    Speaker = beat.Speaker,
                    Text = line.Text,
                    Start = cursor,
                    Lead = lead,
                    Reveal = reveal,
                    Hold = hold,
                    Exit = exit,
                    Duration = lead + reveal + hold + exit
                };

                cursor += entries[i].Duration;
            }

            return entries;
        }

        /// <summary>
        /// Mirrors CaptionPerformer.TypeReveal: one character at a time at the
        /// current rate, with pauses and rate changes consumed at their indices.
        /// </summary>
        private static float RevealSeconds(CinematicBeat beat, PerformedLine line, Settings settings)
        {
            float rate = beat.Rate > 0f ? beat.Rate : settings.DefaultRate;
            float time = 0f;
            int revealed = 0;
            int actionIndex = 0;
            IReadOnlyList<TimedAction> actions = line.Actions;

            while (true)
            {
                while (actionIndex < actions.Count && actions[actionIndex].VisibleIndex <= revealed)
                {
                    TimedAction action = actions[actionIndex++];
                    if (action.Kind == ActionKind.Pause) time += action.Value / 1000f;
                    else if (action.Kind == ActionKind.Rate) rate = action.Value;
                }

                if (revealed >= line.VisibleLength) break;

                time += 1f / Math.Max(rate, 0.01f);
                revealed++;
            }

            return time;
        }

        private static float Clamp(float v, float min, float max) =>
            v < min ? min : (v > max ? max : v);

        public static float TotalSeconds(Entry[] entries) =>
            entries.Length == 0 ? 0f : entries[entries.Length - 1].End;

        /// <summary>Index of the beat active at the given time, or the last one.</summary>
        public static int IndexAt(Entry[] entries, float seconds)
        {
            for (int i = 0; i < entries.Length; i++)
                if (seconds < entries[i].End) return i;
            return Math.Max(0, entries.Length - 1);
        }

        /// <summary>
        /// Tab separated start, end, label. This is Audacity's label track format,
        /// which imports onto a waveform in one step and is the fastest way to see
        /// the script against the music without playing the scene.
        /// </summary>
        public static string ToLabelTrack(Entry[] entries, float offsetSeconds = 0f)
        {
            var sb = new StringBuilder();
            foreach (Entry e in entries)
            {
                string label = e.Speaker == DialogueCharacter.Narration
                    ? e.Text
                    : $"{e.Speaker}: {e.Text}";

                sb.Append((e.Start + offsetSeconds).ToString("F3", CultureInfo.InvariantCulture)).Append('\t')
                  .Append((e.End + offsetSeconds).ToString("F3", CultureInfo.InvariantCulture)).Append('\t')
                  .Append(Sanitise(label)).Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>Generic CSV with the timing breakdown, for spreadsheets or Premiere.</summary>
        public static string ToCsv(Entry[] entries, float offsetSeconds = 0f)
        {
            var sb = new StringBuilder();
            sb.Append("index,start,end,timecode,duration,lead,reveal,hold,exit,speaker,text\n");

            foreach (Entry e in entries)
            {
                float start = e.Start + offsetSeconds;
                sb.Append(e.Index).Append(',')
                  .Append(F(start)).Append(',')
                  .Append(F(e.End + offsetSeconds)).Append(',')
                  .Append(Timecode(start)).Append(',')
                  .Append(F(e.Duration)).Append(',')
                  .Append(F(e.Lead)).Append(',')
                  .Append(F(e.Reveal)).Append(',')
                  .Append(F(e.Hold)).Append(',')
                  .Append(F(e.Exit)).Append(',')
                  .Append(e.Speaker).Append(',')
                  .Append('"').Append(e.Text.Replace("\"", "\"\"")).Append('"')
                  .Append('\n');
            }

            return sb.ToString();
        }

        private static string F(float v) => v.ToString("F3", CultureInfo.InvariantCulture);

        public static string Timecode(float seconds)
        {
            int total = (int)seconds;
            int ms = (int)((seconds - total) * 1000f);
            return $"{total / 60:00}:{total % 60:00}.{ms:000}";
        }

        private static string Sanitise(string s) =>
            s.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
    }
}

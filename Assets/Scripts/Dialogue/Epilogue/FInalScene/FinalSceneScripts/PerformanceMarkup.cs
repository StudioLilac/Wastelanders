using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Cinematics
{
    /// <summary>
    /// What happens at a given point during a line's reveal.
    /// </summary>
    public enum ActionKind
    {
        /// Stop revealing for <see cref="TimedAction.Value"/> milliseconds.
        Pause,

        /// Change the reveal rate to <see cref="TimedAction.Value"/> characters per second.
        Rate,

        /// Fire <see cref="TimedAction.CueName"/> on the presenter.
        Cue
    }

    /// <summary>
    /// An action bound to a position in a line's *visible* character stream.
    /// Fires after <c>VisibleIndex - 1</c> has been revealed and before
    /// <c>VisibleIndex</c> is revealed. An action at index 0 fires before the
    /// first character appears; an action at index <c>VisibleLength</c> fires
    /// after the last character appears but before the hold begins.
    /// </summary>
    public readonly struct TimedAction
    {
        public readonly int VisibleIndex;
        public readonly ActionKind Kind;

        /// Milliseconds for <see cref="ActionKind.Pause"/>, chars/sec for <see cref="ActionKind.Rate"/>.
        public readonly float Value;

        /// Cue name for <see cref="ActionKind.Cue"/>, otherwise null.
        public readonly string CueName;

        /// True when the author wrote the tag by hand; false when it was derived
        /// from punctuation. Explicit actions win ties against derived ones.
        public readonly bool IsExplicit;

        private TimedAction(int visibleIndex, ActionKind kind, float value, string cueName, bool isExplicit)
        {
            VisibleIndex = visibleIndex;
            Kind = kind;
            Value = value;
            CueName = cueName;
            IsExplicit = isExplicit;
        }

        public static TimedAction MakePause(int index, float milliseconds, bool isExplicit) =>
            new TimedAction(index, ActionKind.Pause, milliseconds, null, isExplicit);

        public static TimedAction MakeRate(int index, float charsPerSecond) =>
            new TimedAction(index, ActionKind.Rate, charsPerSecond, null, true);

        public static TimedAction MakeCue(int index, string cueName) =>
            new TimedAction(index, ActionKind.Cue, 0f, cueName, true);

        public override string ToString() => Kind switch
        {
            ActionKind.Pause => $"@{VisibleIndex} pause {Value}ms{(IsExplicit ? "" : " (derived)")}",
            ActionKind.Rate => $"@{VisibleIndex} rate {Value}cps",
            ActionKind.Cue => $"@{VisibleIndex} cue '{CueName}'",
            _ => $"@{VisibleIndex} ?"
        };
    }

    /// <summary>
    /// Tunable pause lengths derived from punctuation. Kept as a plain
    /// [Serializable] class (System, not UnityEngine) so it can be exposed in the
    /// inspector for live tuning while the parser itself stays engine-free.
    /// </summary>
    [Serializable]
    public class PunctuationProfile
    {
        public bool enabled = true;

        /// Pause after a comma.
        public float commaMs = 350f;

        /// Pause after a single sentence terminator that is not the end of the line.
        public float sentenceMs = 700f;

        /// Pause after an ellipsis (two or more dots, or the single '…' glyph).
        public float ellipsisMs = 1000f;

        public static readonly PunctuationProfile Default = new PunctuationProfile();

        public static readonly PunctuationProfile Off = new PunctuationProfile { enabled = false };
    }

    /// <summary>
    /// The result of parsing one authored line.
    /// </summary>
    public sealed class PerformedLine
    {
        /// Markup stripped. TMP rich text tags are preserved verbatim.
        public string Text { get; }

        /// Number of characters TMP will count as visible, which matches
        /// <c>TMP_TextInfo.characterCount</c> for <see cref="Text"/>.
        public int VisibleLength { get; }

        /// Whitespace-delimited word count of the visible text, for hold derivation.
        public int WordCount { get; }

        /// True when the line ends on an en or em dash, i.e. the speaker was cut off.
        /// Presenters should force an instant exit and a zero hold unless overridden.
        public bool EndsInterrupted { get; }

        /// Ascending by <see cref="TimedAction.VisibleIndex"/>. Stable within an index.
        public IReadOnlyList<TimedAction> Actions { get; }

        internal PerformedLine(string text, int visibleLength, int wordCount, bool endsInterrupted,
            IReadOnlyList<TimedAction> actions)
        {
            Text = text;
            VisibleLength = visibleLength;
            WordCount = wordCount;
            EndsInterrupted = endsInterrupted;
            Actions = actions;
        }

        public override string ToString() =>
            $"\"{Text}\" [{VisibleLength} chars, {Actions.Count} actions{(EndsInterrupted ? ", interrupted" : "")}]";
    }

    /// <summary>
    /// Parses inline performance markup out of an authored line.
    ///
    /// Three tags, all in braces so they never collide with TMP's angle-bracket
    /// rich text:
    ///
    ///   {600}          pause 600 milliseconds at this point
    ///   {r24}          reveal at 24 characters per second from this point on
    ///   {!ivescrashes} fire the cue named "ivescrashes" at this point
    ///
    /// Anything that looks like a tag but does not parse is passed through as
    /// literal text, so a stray brace never eats a character.
    ///
    /// After stripping, pauses are derived from punctuation using
    /// <see cref="PunctuationProfile"/>. A hand-written {..} pause at the same
    /// index suppresses the derived one, so authoring an override is always
    /// possible and never requires disabling the whole system.
    ///
    /// This class intentionally has no UnityEngine dependency. Run it under
    /// dotnet test for fast iteration on the markup semantics.
    /// </summary>
    public static class PerformanceMarkup
    {
        public static PerformedLine Parse(string raw) => Parse(raw, PunctuationProfile.Default);

        public static PerformedLine Parse(string raw, PunctuationProfile punctuation)
        {
            if (string.IsNullOrEmpty(raw))
                return new PerformedLine(string.Empty, 0, 0, false, Array.Empty<TimedAction>());

            var builder = new StringBuilder(raw.Length);
            var visible = new List<char>(raw.Length);
            var authored = new List<TimedAction>();

            int i = 0;
            while (i < raw.Length)
            {
                char c = raw[i];

                // TMP rich text: copy verbatim, contributes no visible characters.
                if (c == '<')
                {
                    int close = raw.IndexOf('>', i + 1);
                    if (close >= 0)
                    {
                        builder.Append(raw, i, close - i + 1);
                        i = close + 1;
                        continue;
                    }
                    // Unclosed '<' falls through and is treated as a literal.
                }

                // Performance markup.
                if (c == '{')
                {
                    int close = raw.IndexOf('}', i + 1);
                    if (close > i + 1 &&
                        TryParseTag(raw.Substring(i + 1, close - i - 1), visible.Count, out TimedAction action))
                    {
                        authored.Add(action);
                        i = close + 1;
                        continue;
                    }
                    // Unrecognised tag falls through and is treated as a literal.
                }

                builder.Append(c);
                visible.Add(c);
                i++;
            }

            var derived = new List<TimedAction>();
            bool interrupted = DerivePunctuation(visible, punctuation, derived);

            IReadOnlyList<TimedAction> actions = Merge(authored, derived);

            return new PerformedLine(
                builder.ToString(),
                visible.Count,
                CountWords(visible),
                interrupted,
                actions);
        }

        /// <summary>
        /// Strips markup without deriving anything. Useful for logging, search,
        /// and localisation extraction.
        /// </summary>
        public static string StripMarkup(string raw) => Parse(raw, PunctuationProfile.Off).Text;

        private static bool TryParseTag(string body, int visibleIndex, out TimedAction action)
        {
            action = default;
            if (body.Length == 0) return false;

            // {!cueName}
            if (body[0] == '!')
            {
                if (body.Length < 2) return false;
                action = TimedAction.MakeCue(visibleIndex, body.Substring(1).Trim());
                return true;
            }

            // {r24} or {r24.5}
            if (body[0] == 'r' || body[0] == 'R')
            {
                if (float.TryParse(body.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture,
                        out float cps) && cps > 0f)
                {
                    action = TimedAction.MakeRate(visibleIndex, cps);
                    return true;
                }
                return false;
            }

            // {600}
            if (float.TryParse(body, NumberStyles.Float, CultureInfo.InvariantCulture, out float ms) && ms >= 0f)
            {
                action = TimedAction.MakePause(visibleIndex, ms, true);
                return true;
            }

            return false;
        }

        private static bool IsTerminator(char c) => c == '.' || c == '!' || c == '?' || c == '\u2026';

        private static bool IsDash(char c) => c == '\u2013' || c == '\u2014';

        /// <returns>True when the line ends on a dash.</returns>
        private static bool DerivePunctuation(List<char> visible, PunctuationProfile profile,
            List<TimedAction> output)
        {
            int last = visible.Count - 1;
            while (last >= 0 && char.IsWhiteSpace(visible[last])) last--;
            if (last < 0) return false;

            bool interrupted = IsDash(visible[last]);

            if (profile == null || !profile.enabled) return interrupted;

            int i = 0;
            while (i <= last)
            {
                char c = visible[i];

                if (c == ',')
                {
                    // A pause at last + 1 would duplicate the hold, so only interior commas count.
                    if (i < last) output.Add(TimedAction.MakePause(i + 1, profile.commaMs, false));
                    i++;
                    continue;
                }

                if (IsTerminator(c))
                {
                    // Collapse runs so "..." and "?!" produce one pause, not three.
                    int j = i;
                    while (j <= last && IsTerminator(visible[j])) j++;

                    bool isEllipsis = (j - i) >= 2 || c == '\u2026';
                    float ms = isEllipsis ? profile.ellipsisMs : profile.sentenceMs;

                    // Terminal punctuation gets no pause; that is what the hold is for.
                    if (j <= last) output.Add(TimedAction.MakePause(j, ms, false));

                    i = j;
                    continue;
                }

                i++;
            }

            return interrupted;
        }

        /// <summary>
        /// Both inputs are already ascending by index. Authored actions sort ahead
        /// of derived ones at the same index, and a derived pause is dropped when an
        /// authored pause occupies the same index.
        /// </summary>
        private static TimedAction[] Merge(List<TimedAction> authored, List<TimedAction> derived)
        {
            if (derived.Count == 0) return authored.ToArray();

            var result = new List<TimedAction>(authored.Count + derived.Count);
            int a = 0, d = 0;

            while (a < authored.Count || d < derived.Count)
            {
                if (d >= derived.Count)
                {
                    result.Add(authored[a++]);
                    continue;
                }

                if (a >= authored.Count || derived[d].VisibleIndex < authored[a].VisibleIndex)
                {
                    TimedAction candidate = derived[d++];
                    if (!HasAuthoredPauseAt(authored, candidate.VisibleIndex)) result.Add(candidate);
                    continue;
                }

                result.Add(authored[a++]);
            }

            return result.ToArray();
        }

        private static bool HasAuthoredPauseAt(List<TimedAction> authored, int index)
        {
            for (int i = 0; i < authored.Count; i++)
            {
                if (authored[i].VisibleIndex == index && authored[i].Kind == ActionKind.Pause) return true;
                if (authored[i].VisibleIndex > index) return false;
            }
            return false;
        }

        private static int CountWords(List<char> visible)
        {
            int words = 0;
            bool inWord = false;
            for (int i = 0; i < visible.Count; i++)
            {
                if (char.IsWhiteSpace(visible[i]))
                {
                    inWord = false;
                }
                else if (!inWord)
                {
                    inWord = true;
                    words++;
                }
            }
            return words;
        }
    }
}

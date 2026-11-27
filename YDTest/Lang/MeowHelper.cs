using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NIM_Plugin.Language;

namespace YDTest.Lang
{
    public class MeowHelper : I_Language
    {
        public string Code { get; } = "meow";
        public string LanguageName { get; } = "Cat";
        public double PassScore { get; set; } = 0.5;

        public float Version { get; set; } = 0.1f;

        private static readonly HashSet<string> StopWords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "meow", "mew", "nya", "purr", "mrrp", "prr", "meowmeow",
            "miau", "miao", "nyan", "nyaa", "mrow", "mraow", "chirp"
        };

        // Core meow vocabulary
        private static readonly Regex MeowKeywordRegex = new Regex(
            @"\b(meow|mew|nya|purr|mrrp|prr|miau|miao|nyan|nyaa|mrow|mraow)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Stretched meows: meooow / nyaaa / prrrr
        private static readonly Regex MeowStretchRegex = new Regex(
            @"\b(me+o+w+|ny+a+|pr+r+|mr+r+p+|mi+a+u+|chi+r+p+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Tail tilde and heart symbols
        private static readonly Regex MeowDecorationRegex = new Regex(
            @"[~♥♡❤🐾🐱]",
            RegexOptions.Compiled);

        private static readonly Regex LetterRegex = new Regex(
            @"[\p{L}]",
            RegexOptions.Compiled);

        public HashSet<string> GetStopWords()
        {
            return StopWords;
        }

        public bool IsProbablyMatch(string Input)
        {
            return GetProbability(Input) >= PassScore;
        }

        /// <summary>
        /// Returns how cat-like the text is, from 0 to 1. PassScore or higher counts as Cat.
        /// </summary>
        /// <param name="Input">Text to inspect</param>
        public double GetProbability(string Input)
        {
            const double MaxFailScore = 0.49;
            const double KeywordBonus = 0.1;
            const double StretchBonus = 0.15;
            const double DecorationBonus = 0.05;

            if (string.IsNullOrWhiteSpace(Input))
                return MaxFailScore;

            int LetterCount = LetterRegex.Matches(Input).Count;

            if (LetterCount == 0)
                return MaxFailScore;

            int KeywordHits = MeowKeywordRegex.Matches(Input).Count;
            int StretchHits = MeowStretchRegex.Matches(Input).Count;

            if (KeywordHits == 0 && StretchHits == 0)
                return MaxFailScore;

            // Keyword density
            double Density = (double)(KeywordHits + StretchHits) / Math.Max(1, LetterCount / 3);
            double Score = Math.Min(1.0, Density);

            // One keyword is enough to pass
            if (KeywordHits >= 1)
                Score = Math.Max(Score, PassScore) + KeywordBonus * KeywordHits;

            // Stretched meows add extra confidence: meooow~
            if (StretchHits >= 1)
                Score += StretchBonus * StretchHits;

            // Tail tilde and hearts are a cat's signature
            if (MeowDecorationRegex.IsMatch(Input))
                Score += DecorationBonus;

            return Math.Min(1.0, Score);
        }
    }
}

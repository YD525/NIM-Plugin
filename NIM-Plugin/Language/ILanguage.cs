using System.Collections.Generic;

namespace NIM_Plugin.Language
{
    public interface I_Language
    {
        float Version { get; set; }
        double PassScore { get; set; }

        /// <summary>
        /// ISO 639-1 (or BCP-47) language code, e.g. "en"
        /// </summary>
        string Code { get; }

        string LanguageName { get; }

        HashSet<string> GetStopWords();
        bool IsProbablyMatch(string Input);
        double GetProbability(string Input);
    }
}

using System.Collections.Generic;

namespace NIM_Plugin.Language
{
    public interface I_Language
    {
        float Version { get; set; }
        double PassScore { get; set; } 
        int GetLanguage();
        HashSet<string> GetStopWords();
        bool IsProbablyMatch(string Input);
        double GetProbability(string Input);
    }
}

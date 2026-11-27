using System.Collections.Generic;

namespace NIM_Plugin.Language
{
    public interface I_Language
    {
        HashSet<string> GetStopWords();
        bool IsProbablyMatch(string Input);
        double GetProbability(string Input);
    }
}

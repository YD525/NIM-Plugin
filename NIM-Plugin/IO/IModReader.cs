using System.Collections.Generic;

namespace NIM_Plugin.IO
{
    public class Mod_Content
    {
        public string ModName = "";
        /// <summary>
        /// string UniqueKey,string Data
        /// </summary>
        public Dictionary<string, string> Rows = new Dictionary<string, string>();
    }
    public interface I_ModReader
    {
        /// <summary>
        /// File types to take over.
        /// </summary>
        string TakenOverFileExtension { get; set; }

        Mod_Content Read(byte[] Data);

        byte[] Save(Mod_Content Content);
    }
}

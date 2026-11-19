using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NIM_Plugin.Data
{
    public class Mod_Data
    {
        public string Key = "";
        public string Data = "";

        public int GroupID = 0;

        public Mod_Data(string Key, string Data,int GroupID = 0)
        { 
            this.Key = Key;
            this.Data = Data;
            this.GroupID = GroupID;
        }
    }
    public class Mod_Content
    {
        public string ModName = "";

        private List<Mod_Data> _Rows = new List<Mod_Data>() { };
        public ReadOnlyCollection<Mod_Data> Rows
        {
            get { return _Rows.AsReadOnly(); }
        }

        public HashSet<string> UniqueKeys = new HashSet<string>();

        public void Clear()
        {
            _Rows.Clear();
        }
        public void Add(string Key,string Value,int GroupID = 0)
        {
            if (UniqueKeys.Contains(Key))
            {
                throw new Exception("The primary key must be malicious.");
            }

            UniqueKeys.Add(Key);

            _Rows.Add(new Mod_Data(Key,Value,GroupID));
        }
    }
    public interface I_ModReader
    {
        /// <summary>
        /// File types to take over.
        /// </summary>
        string TakenOverFileExtension { get; set; }

        /// <summary>
        /// Receives the raw file bytes and performs pure in-memory binary parsing without file I/O.
        /// Called before <see cref="CanRead"/> so the plugin can determine whether it is capable of handling the data.
        /// </summary>
        /// <param name="Data">Raw byte array of the target file.</param>
        /// <param name="FileUniqueKey"></param>
        /// <returns>Parsed mod content.</returns>
        Mod_Content Read(int FileUniqueKey,byte[] Data);

        /// <summary>
        /// Called after <see cref="Read"/> to report whether the plugin can parse the data.
        /// Return <c>true</c> if capable; if <c>false</c>, the engine passes the file to the next plugin.
        /// </summary>
        /// <returns><c>true</c> if the data can be parsed; otherwise <c>false</c>.</returns>
        bool CanRead();

        /// <summary>
        /// Serializes mod content back into raw binary bytes in memory without performing file I/O.
        /// </summary>
        /// <param name="Content">Mod content to serialize.</param>
        /// <returns>Serialized raw byte array.</returns>
        byte[] Save(Mod_Content Content);
    }
}

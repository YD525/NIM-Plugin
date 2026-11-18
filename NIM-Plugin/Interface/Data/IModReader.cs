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

        Mod_Content Read(byte[] Data);

        byte[] Save(Mod_Content Content);
    }
}

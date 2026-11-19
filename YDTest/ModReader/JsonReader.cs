using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NIM_Plugin.Data;
using NIM_Plugin.Json;

namespace YDTest.ModReader
{
    public class TestJson
    {
        public int ID = 0;
        public string Name = "";
        public string ValueA = "";
        public string ValueB = "";
        public bool XX = false;
    }

    //Please read the methods I’ve written carefully; they are very important.
    public class JsonReader : I_ModReader
    {
        public static JsonReader Instance = new JsonReader();
        public string TakenOverFileExtension { get; set; } = ".json";

        private List<TestJson> _Rows = new List<TestJson>();

        private bool _CanRead = false;

        public int FileUniqueKey = 0;
        public Mod_Content Read(int FileUniqueKey, byte[] Data)
        {
            _Rows.Clear();
            _CanRead = false;
            this.FileUniqueKey = FileUniqueKey;

            string Json = Encoding.UTF8.GetString(Data);

            if (JsonPayload.IsValidDocument(Json))
            {
                try
                {
                    _Rows = JsonConvert.DeserializeObject<List<TestJson>>(Json);
                    _CanRead = true;
                }
                catch { }
            }

            Mod_Content GenContent = new Mod_Content();

            for (int i = 0; i < _Rows.Count; i++)
            {
                GenContent.Add(FileUniqueKey + "_" + i.ToString() + "A", _Rows[i].ValueA, 0);
                GenContent.Add(FileUniqueKey + "_" + i.ToString() + "B", _Rows[i].ValueB, 1);
            }

            return GenContent;
        }
        public bool CanRead()
        {
            return _CanRead;
        }
        public byte[] Save(Mod_Content Content)
        {
            for (int i = 0; i < Content.Rows.Count; i++)
            { 
                var Row = Content.Rows[i];

                if (Row.Key.Contains("_"))
                { 
                    string GetKey = Row.Key = Row.Key.Substring(Row.Key.IndexOf("_") + "_".Length);

                    if (GetKey.EndsWith("A") || GetKey.EndsWith("B"))
                    {
                        int GetRealOffset = int.Parse(GetKey.Substring(0, GetKey.Length - 1));

                        if (GetKey.EndsWith("A"))
                        {
                            _Rows[GetRealOffset].ValueA = Row.Data;
                        }
                        if (GetKey.EndsWith("B"))
                        {
                            _Rows[GetRealOffset].ValueB = Row.Data;
                        }
                    }
                }
            }

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_Rows));
        }
    }
}

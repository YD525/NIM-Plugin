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
    public class FileDataBinding
    {
        public List<TestJson> Data = null;
        public Mod_File Reader = null;

        public FileDataBinding(List<TestJson> Data, Mod_File Reader)
        {
            this.Data = Data;
            this.Reader = Reader;
        }
    }

    //Please read the methods I’ve written carefully; they are very important.
    public class JsonReader : I_ModReader
    {
        public static JsonReader Instance = new JsonReader();
        public string TakenOverFileExtension { get; set; } = ".json";
        public List<string> TakenOverDirectoryNames { get; set; } = new List<string>() { "DB" };
        public bool CanTakeOverEntireDirectory { get; set; } = true;

        private bool _CanRead = false;

        private Dictionary<int, FileDataBinding> _Files = new Dictionary<int, FileDataBinding>();
        public Mod_Content Read(List<Mod_File> ModFiles)
        {
            _CanRead = false;

            _Files.Clear();

            Mod_Content GenContent = new Mod_Content();

            for (int i = 0; i < ModFiles.Count; i++)
            {
                var Reader = ModFiles[i];

                var FileUniqueKey = Reader.GetFileUniqueKey();

                string Json = Encoding.UTF8.GetString(Reader.Read());

                if (!_Files.ContainsKey(FileUniqueKey))
                {
                    _Files.Add(FileUniqueKey,new FileDataBinding(new List<TestJson>(),Reader));
                }

                if (JsonPayload.IsValidDocument(Json))
                {
                    try
                    {
                        _Files[FileUniqueKey].Data = JsonConvert.DeserializeObject<List<TestJson>>(Json);
                    }
                    catch { }
                }

                if (_Files[FileUniqueKey].Data != null)
                {
                    _CanRead = true;

                    for (int ir = 0; ir < _Files[FileUniqueKey].Data.Count; ir++)
                    {
                        GenContent.Add(FileUniqueKey + "_" + ir.ToString() + "_" + "A", _Files[FileUniqueKey].Data[ir].ValueA, 0);
                        GenContent.Add(FileUniqueKey + "_" + ir.ToString() + "_" + "B", _Files[FileUniqueKey].Data[ir].ValueB, 1);
                    }
                }
            }

            return GenContent;
        }
        public bool CanRead()
        {
            return _CanRead;
        }
        public int Save(Mod_Content Content)
        {
            int SaveCount = 0;

            HashSet<int> Modified = new HashSet<int>();

            for (int i = 0; i < Content.Rows.Count; i++)
            {
                var Row = Content.Rows[i];

                if (Row.Key.Contains("_"))
                {
                    var Params = Row.Key.Split('_');

                    if (Params.Length > 2)
                    {
                        try
                        {
                            int FileUniqueKey = int.Parse(Params[0]);

                            if ((Params[2].Equals("A") || Params[2].Equals("B")) && _Files.ContainsKey(FileUniqueKey))
                            {
                                int GetRealOffset = int.Parse(Params[1]);

                                if (Params[2].Equals("A"))
                                {
                                    _Files[FileUniqueKey].Data[GetRealOffset].ValueA = Row.Data;
                                }
                                if (Params[2].Equals("B"))
                                {
                                    _Files[FileUniqueKey].Data[GetRealOffset].ValueB = Row.Data;
                                }

                                Modified.Add(FileUniqueKey);
                            }
                        }
                        catch { }
                    }
                }
            }

            foreach (var GetKey in Modified)
            {
                var File = _Files[GetKey];

                if (File.Reader.Save(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(File.Data))))
                {
                    SaveCount++;
                }
            }

            return SaveCount;
        }
    }
}

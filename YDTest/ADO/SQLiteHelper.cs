using NIM_Plugin.ADO;
using NIM_Plugin.Core;

namespace YDTest.ADO
{
    public class SQLiteHelper : I_Database
    {
        public static SQLiteHelper Instance = new SQLiteHelper();

        private DB_ExecuteQuery _ExecuteQuery;
        private DB_ExecuteNonQuery _ExecuteNonQuery;
        private DB_ExecuteScalar _ExecuteScalar;

        private bool CanUse = false;
        public bool Bind(DB_ExecuteQuery ExecuteQuery, DB_ExecuteNonQuery ExecuteNonQuery, DB_ExecuteScalar ExecuteScalar)
        {
            this._ExecuteQuery = ExecuteQuery;
            this._ExecuteNonQuery = ExecuteNonQuery;
            this._ExecuteScalar = ExecuteScalar;

            if (this._ExecuteQuery != null && this._ExecuteNonQuery != null && this._ExecuteScalar != null)
            {
                return true;
            }

            return false;
        }
        public void OnBound()
        {
            CanUse = true;

            Init();
        }

        public void Init()
        {
            Test();
        }
        public void Test()
        {
            if (CanUse)
            {
                bool BoundSuccessfully = false;

                try
                {
                    //Test Call SQL Engine
                    if (int.Parse(_ExecuteScalar.Invoke("SELECT 1").ToString()) == 1)
                    {
                        BoundSuccessfully = true;
                    }
                }
                catch { }

                if(BoundSuccessfully)
                if (PluginApi.ShowPrompt("SQL", "SQL methods can be called.", "OK") == "OK")
                {
                    //Do some things
                }
            }
        }
    }
}

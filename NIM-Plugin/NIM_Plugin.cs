using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIM_Plugin
{
    public class NIMPlugin
    {
        public static Dictionary<string, bool> Plugins = null;

        public static string Version = "1.0.0";

        public static string PluginsPath = @"\CorePlugins\";

        private static string CurrentPath = "";
        public static void Init(string StartupPath,Dictionary<string, bool> Plugins)
        {
            CurrentPath = StartupPath;
            NIMPlugin.Plugins = Plugins;
        }
      
        public static string GetFullPath(string Path)
        {
            string GetShellPath = CurrentPath;

            if (Path.StartsWith(@"\"))
            {
                Path = Path.Substring(1);
            }

            return GetShellPath + Path;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NIM_Plugin.DLL;

namespace NIM_Plugin.Core
{
    public class PluginHub
    {
        /// <summary>
        /// The whitelist only records the signatures of official mods.
        /// </summary>
        public static readonly IReadOnlyList<string> Verified_SHA256_List = new List<string>() { "d8df0d3dea2d6e39aaa55881b5b32a9d4b6b05e547acca9a7e56360072a98538" }.AsReadOnly();

        public static PluginHub Instance = new PluginHub();
        public sealed class PluginEntry
        {
            public I_Plugin Instance { get; internal set; }
            public string FilePath { get; internal set; }
            public Type PluginType { get; internal set; }
            public string Name { get; internal set; }
            public string Author { get; internal set; }
            public string Description { get; internal set; }
            public string Version { get; internal set; }

            public string SHA256_Sign = "";
            public Exception LastError { get; internal set; }
        }

        private readonly List<PluginEntry> _Plugins = new List<PluginEntry>();
        private object _Lock = new object();

        public void Init()
        {
            ScanPlugins();
        }
        public void ScanPlugins()
        {
            lock (_Lock)
            {
                _Plugins.Clear();
            }

            string PluginDir = NIMPlugin.GetFullPath(NIMPlugin.PluginsPath);

            // Directory missing: skip everything, and never prune (would wipe the user's switches).
            if (!Directory.Exists(PluginDir)) return;

            HashSet<string> Seen = new HashSet<string>();
            bool ScanOk = true;

            string[] DllFiles;
            try
            {
                DllFiles = Directory.GetFiles(PluginDir, "*.dll", SearchOption.TopDirectoryOnly);
            }
            catch (Exception)
            {
                return;
            }

            foreach (string Dll in DllFiles)
            {
                string Sign;
                try
                {
                    Sign = SHA256_Validator.ComputeFileSHA256(Dll);
                }
                catch (Exception)
                {
                    // Cannot identify this DLL (file locked, etc.), so the scan is incomplete: do not prune.
                    ScanOk = false;
                    continue;
                }

                bool Verified = Verified_SHA256_List.Contains(Sign);

                if (!Verified)
                {
                    try
                    {
                        //Plugins are not allowed to perform file read/write operations or directory query operations. If you need to persist data, please implement the I_Database interface to store the data you need. I_Database provides three separate database operation methods that cover all scenarios. Please create a new table to store your data.
                        if (!FileSafetyScanner.Scan(Dll).IsSafe)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        ScanOk = false;
                        continue;
                    }
                }

                // A DLL that exists on disk counts as "seen" even if it fails to load below,
                // so a temporarily broken DLL does not lose the user's switch.
                Seen.Add(Sign);

                try
                {
                    Assembly Asm = Assembly.LoadFrom(Dll);

                    Type[] Types;
                    try { Types = Asm.GetTypes(); }
                    catch (ReflectionTypeLoadException Ex) { Types = Ex.Types; }

                    foreach (Type T in Types)
                    {
                        if (T == null || T.IsAbstract || T.IsInterface) continue;
                        if (!typeof(I_Plugin).IsAssignableFrom(T)) continue;

                        PluginNameAttribute NameAttr = T.GetCustomAttribute<PluginNameAttribute>();
                        PluginAuthorAttribute AuthorAttr = T.GetCustomAttribute<PluginAuthorAttribute>();
                        PluginDescriptionAttribute DescriptionAttr = T.GetCustomAttribute<PluginDescriptionAttribute>();
                        PluginVersionAttribute VersionAttr = T.GetCustomAttribute<PluginVersionAttribute>();
                        PluginEnabledAttribute EnabledAttr = T.GetCustomAttribute<PluginEnabledAttribute>();

                        if (NameAttr == null || AuthorAttr == null || DescriptionAttr == null || VersionAttr == null || EnabledAttr == null)
                        {
                            // [Violating plugin] Simply skip any plugins that don't behave as expected.
                            continue;
                        }

                        if (EnabledAttr.Enabled && !Verified)
                        {
                            // [Violating plugin] Claims to be official but is not in the whitelist.
                            continue;
                        }

                        var Enabled = EnabledAttr.Enabled && Verified;

                        PluginEntry Entry = new PluginEntry
                        {
                            Instance = null, // Do not create the instance early, otherwise Enabled is meaningless.
                            FilePath = Dll,
                            PluginType = T,
                            SHA256_Sign = Sign,
                            Name = NameAttr.Name,
                            Author = AuthorAttr.Author,
                            Description = DescriptionAttr.Description,
                            Version = VersionAttr.Version.ToString("F1", CultureInfo.InvariantCulture),
                        };

                        if (!NIMPlugin.Plugins.ContainsKey(Sign))
                        {
                            NIMPlugin.Plugins.Add(Sign, Enabled);
                        }

                        lock (_Lock)
                        {
                            _Plugins.Add(Entry);
                        }
                    }
                }
                catch (Exception)
                {
                    // Load failed: skip this DLL. Its hash is already in Seen, so the user's switch is kept.
                }
            }

            if (ScanOk)
            {
                PruneInvalidKeys(Seen);
            }
        }

        /// <summary>
        /// Remove config entries whose DLL no longer exists in the plugin directory.
        /// </summary>
        private static void PruneInvalidKeys(HashSet<string> Seen)
        {
            List<string> Stale = NIMPlugin.Plugins.Keys
                .Where(K => !Seen.Contains(K))
                .ToList(); // ToList first: cannot remove while enumerating

            foreach (string Key in Stale)
            {
                NIMPlugin.Plugins.Remove(Key);
            }
        }

        public (bool Executed, Dictionary<string, object[]> Results) Dispatch(int Type, string MethodName, object[] ParamsRef = null)
        {
            bool Executed = false;
            Dictionary<string, object[]> Results = new Dictionary<string, object[]>();

            if (Type < 0 || Type > 1)
            {
                return (false, Results);
            }

            List<PluginEntry> Snapshot;

            lock (_Lock) 
            { 
                Snapshot = _Plugins.ToList(); 
            }

            foreach (PluginEntry Entry in Snapshot)
            {
                if (string.IsNullOrEmpty(Entry.SHA256_Sign) || !NIMPlugin.Plugins.ContainsKey(Entry.SHA256_Sign))
                {
                    continue;
                }

                if (!NIMPlugin.Plugins[Entry.SHA256_Sign])
                {
                    continue;
                }

                object[] Args = ParamsRef;

                try
                {
                    if (Entry.Instance == null)
                    {
                        Entry.Instance = (I_Plugin)Activator.CreateInstance(Entry.PluginType);
                    }

                    if (Type == 0)
                    {
                        Entry.Instance.Init(MethodName, ref Args);
                    }
                    else if (Type == 1)
                    {
                        Entry.Instance.HandleEvent(MethodName, ref Args);
                    }

                    Executed = true;

                    if (Args != null)
                    {
                        Results[Entry.FilePath] = Args;
                    }
                }
                catch (Exception Ex)
                {
                    Entry.LastError = Ex;
                    Executed = true;
                }
            }

            return (Executed, Results);
        }

        public bool IsTrustedPlugin(string Key)
        {
            if (Verified_SHA256_List.Contains(Key))
            {
                return true;
            }

            return false;
        }

        public List<string[]> GetPlugins()
        {
            List<string[]> SetPlugins = new List<string[]>();

            List<PluginEntry> Snapshot;

            lock (_Lock)
            {
                Snapshot = _Plugins.ToList();
            }

            foreach (var Get in Snapshot)
            {
                if (NIMPlugin.Plugins.ContainsKey(Get.SHA256_Sign))
                {
                    string[] Params = new string[7];
                    Params[0] = Get.FilePath;
                    Params[1] = Get.Name;
                    Params[2] = Get.Author;
                    Params[3] = Get.Description;
                    Params[4] = Get.Version;
                    Params[5] = Get.SHA256_Sign;
                    Params[6] = NIMPlugin.Plugins[Get.SHA256_Sign].ToString();
                    SetPlugins.Add(Params);
                }
            }

            return SetPlugins;
        }

        /// <summary>
        /// Enables a plugin.
        /// </summary>
        /// <param name="Key">The SHA256 hash of the plugin DLL.</param>
        /// <returns>True if the plugin exists and was enabled; otherwise, false.</returns>
        public bool EnablePlugin(string Key)
        {
            if (NIMPlugin.Plugins.ContainsKey(Key))
            {
                NIMPlugin.Plugins[Key] = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Disables a plugin.
        /// </summary>
        /// <param name="Key">The SHA256 hash of the plugin DLL.</param>
        /// <returns>True if the plugin exists and was disabled; otherwise, false.</returns>
        public bool DisablePlugin(string Key)
        {
            if (NIMPlugin.Plugins.ContainsKey(Key))
            {
                NIMPlugin.Plugins[Key] = false;
                return true;
            }

            return false;
        }
    }
}

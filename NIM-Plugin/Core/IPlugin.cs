using System;

namespace NIM_Plugin.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginNameAttribute : Attribute
    {
        public string Name { get; }

        public PluginNameAttribute(string Name)
        {
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginAuthorAttribute : Attribute
    {
        public string Author { get; }

        public PluginAuthorAttribute(string Author)
        {
            this.Author = Author;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public PluginDescriptionAttribute(string Description)
        {
            this.Description = Description;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginVersionAttribute : Attribute
    {
        public float Version { get; }

        public PluginVersionAttribute(float Version)
        {
            this.Version = Version;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginEnabledAttribute : Attribute
    {
        public bool Enabled { get; }

        public PluginEnabledAttribute(bool Enabled)
        {
            this.Enabled = Enabled;
        }
    }

    public interface I_Plugin
    {
        void Init(Engine_Method Method, ref object[] ParamsRef);

        void HandleEvent(Engine_Method Method, ref object[] ParamsRef);
    }
}

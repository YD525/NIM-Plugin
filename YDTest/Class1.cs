using NIM_Plugin;
using NIM_Plugin.Core;
using YDTest.ADO;
using YDTest.ModReader;

namespace YDTest
{
    [PluginName("YDTest")] //Enter your plugin name here.
    [PluginAuthor("YD525")] //Remember to use a different author name—definitely don’t use the one I’ve written here.
    [PluginDescription("Just to tell users how to build plugins.")] //Write your description here
    [PluginVersion(1.0f)]// Two-part format: Major.Revision
    [PluginEnabled(false)]// Official plugins: set to true to run the plugin automatically after the main program starts. Third-party plugins: keep false.
    public class Class1:I_Plugin
    {
        /// <summary>
        /// Called by the engine when the plugin is loaded.
        /// </summary>
        /// <param name="MethodName">Name of the feature currently being loaded.</param>
        /// <param name="ParamsRef">Params is a pointer,Used to pass or return data.</param>
        public void Init(Engine_Method Method, ref object[] ParamsRef)
        {
            //This approach is fully implemented via the following parameters:
            //LoadNodes
            //Allows you to customize the translation platforms you wish to use.
            //InitDatabase
            //Enables you to control the data within the engine's core database.
            //LoadReaders
            //Allows you to customize file parsers—for instance, to handle file formats that are not natively supported by NIM.
            switch (Method)
            {
                case Engine_Method.LoadNodes:
                    { 
                    }
                break;
                case Engine_Method.InitDatabase:
                    {
                        ParamsRef = new object[1];
                        ParamsRef[0] = SQLiteHelper.Instance;
                    }
                break;
                case Engine_Method.LoadReaders:
                    {
                        ParamsRef = new object[1];
                        ParamsRef[0] = JsonReader.Instance;
                    }
                break;
            }
        }

        /// <summary>
        /// Called by the engine when an event occurs, so the plugin can handle it.
        /// </summary>
        /// <param name="MethodName">Name of the event method raised by the engine.</param>
        /// <param name="ParamsRef">Params is a pointer,Used to pass or return data.</param>
        public void HandleEvent(Engine_Method Method, ref object[] ParamsRef)
        {
            //My initial concept was for this method to take charge of the engine’s entire translation workflow, thereby facilitating interim adjustments by plugins and allowing for indirect control over the engine’s output.
            //This method hasn't been implemented yet, so I'll leave it as a placeholder for now. I’m still weighing the scheduling order for certain parts of the core engine—it’s a rather complex matter.
        }
    }
}

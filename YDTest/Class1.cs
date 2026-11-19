using System.Collections.Generic;
using System;
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

        public Class1()
        {
            //A plugin instance is created.
        }

        //Interfaces currently supported for construction.

        //I_Plugin is the entry point of the plug-in.

        //I_AI_Node,I_Local_AI_Node,I_Node If you wish to add a new platform, please implement one of the three interfaces, depending on the type of translation platform: use `I_Node` for traditional translation platforms, `I_Local_AI_Node` for local AI translation platforms, and `I_AI_Node` for online AI translation platforms. Note that you should pass the `Type` to the engine, rather than an instance.

        //I_Database If you wish to implement read/write operations for the NIM database, you must implement the necessary logic and pass the instance to the engine.

        //I_ModReader If you want NIM to support reading and writing a specific file type, implement this interface and pass the instance to the engine. Implementing a constructor and IDisposable is optional, so customize it as you see fit.

        /// <summary>
        /// This method represents the initialization of events for any engine module; it may be called multiple times, as different modules can trigger the initialization process.
        /// </summary>
        /// <param name="Method">The method type after current initialization.</param>
        /// <param name="ParamsRef">Params is a pointer,Used to pass or return data.</param>
        public void Init(Engine_Method Method, ref object[] ParamsRef)
        {
            //This approach is fully implemented via the following parameters:

            //LoadNodes
            //This event indicates that the engine is loading nodes, allowing you to pass in your new nodes.

            //InitDatabase
            //This event indicates that the engine has successfully connected to the database, allowing you to access the engine's database read/write methods. You can then make use of the delegate you passed in when `OnBound()` is triggered.

            //LoadReaders
            //The engine is currently building the mod parser. You may submit your own parser; whether or not it is selected depends on the loading order of the respective plugins.

            switch (Method)
            {
                case Engine_Method.LoadNodes:
                    {
                        //ParamsRef = new object[3];

                        //ParamsRef[0] = new List<Type>()
                        //{
                        //    //Need to implement the I_Local_AI_Node interface
                        //    typeof(LocalAI)
                        //};

                        //ParamsRef[1] = new List<Type>()
                        //{
                        //    //Need to implement the I_AI_Node interface
                        //    typeof(ClaudeApi),
                        //    typeof(ChatGptApi),
                        //    typeof(GeminiApi),
                        //    typeof(NanoGptApi),
                        //    typeof(DeepSeekApi)
                        //};

                        //ParamsRef[2] = new List<Type>()
                        //{
                        //    //Need to implement I_Node interface
                        //    typeof(DeepLApi)
                        //};
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

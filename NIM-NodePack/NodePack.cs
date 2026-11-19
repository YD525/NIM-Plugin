using System;
using System.Collections.Generic;
using NIM_NodePack.Nodes;
using NIM_Plugin;
using NIM_Plugin.Core;

namespace NIM_NodePack
{
    [PluginName("Node Pack")] //Enter your plugin name here.
    [PluginAuthor("NIM Engine")] //Remember to use a different author name—definitely don’t use the one I’ve written here.
    [PluginDescription("Node extension pack that registers multiple translation platform nodes so the engine can use them.")] //Write your description here
    [PluginVersion(1.0f)]// Two-part format: Major.Revision
    [PluginEnabled(true)]// Official plugins: set to true to run the plugin automatically after the main program starts. Third-party plugins: keep false.
    public class NodePack: I_Plugin
    {
        /// <summary>
        /// This method represents the initialization of events for any engine module; it may be called multiple times, as different modules can trigger the initialization process.
        /// </summary>
        /// <param name="Method">The method type after current initialization.</param>
        /// <param name="ParamsRef">Params is a pointer,Used to pass or return data.</param>
        public void Init(Engine_Method Method, ref object[] ParamsRef)
        {
            if (Method == Engine_Method.LoadNodes)
            {
                ParamsRef = new object[3];

                ParamsRef[0] = new List<Type>()
                {
                    //Need to implement the I_Local_AI_Node interface
                    typeof(LocalAI)
                };

                ParamsRef[1] = new List<Type>()
                {
                    //Need to implement the I_AI_Node interface
                    typeof(ClaudeApi),
                    typeof(ChatGptApi),
                    typeof(GeminiApi),
                    typeof(NanoGptApi),
                    typeof(DeepSeekApi)
                }; 

                ParamsRef[2] = new List<Type>()
                {
                    //Need to implement I_Node interface
                    typeof(DeepLApi)
                };
            }
        }

        /// <summary>
        /// Called by the engine when an event occurs, so the plugin can handle it.
        /// </summary>
        /// <param name="MethodName">Name of the event method raised by the engine.</param>
        /// <param name="ParamsRef">Params is a pointer,Used to pass or return data.</param>
        public void HandleEvent(Engine_Method Method, ref object[] ParamsRef)
        {
            //This method hasn't been implemented yet, so I'll leave it as a placeholder for now. I’m still weighing the scheduling order for certain parts of the core engine—it’s a rather complex matter.
        }
    }
}

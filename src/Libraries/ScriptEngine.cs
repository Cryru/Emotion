using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Used to run LUA scripts.
    /// Uses MoonSharp - Public Repository: https://github.com/xanathar/moonsharp
    /// </summary>
    public static class ScriptEngine
    {
        #region "Declarations"
        /// <summary>
        /// The MoonSharp lua engine.
        /// </summary>
        private static Script Interpreter = new Script();
        /// <summary>
        /// List of exposed functions.
        /// </summary>
        private static List<string> exposedFunctions = new List<string>();
        /// <summary>
        /// Whether to append "return" to all ExecuteLine calls.
        /// </summary>
        public static bool returnAll = false;
        #endregion

        /// <summary>
        /// Setups the scripting engine.
        /// </summary>
        public static void SetupScripting()
        {
            //Add default functions.
            ExposeFunction("getListeners", (Func<string>) getListeners);
            ExposeFunction("getObjects", (Func<string>) getObjects);
            ExposeFunction("autoReturn", (Func<bool, string>) autoReturn);
            ExposeFunction("getLog", (Func<string>) getLog);
            ExposeFunction("help", (Func<string>) help);
            UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
        }

        /// <summary>
        /// Executes the provided line of script.
        /// </summary>
        /// <param name="Script">The script to execute as a string.</param>
        public static DynValue ExecuteScript(string Script)
        {
            try
            {
                //Run the script and append a return if specified.
                return Interpreter.DoString(returnAll ? "return " + Script : Script);
            }
            catch (Exception e)
            {
                return DynValue.NewString("<color=#f44b42>" + e.Message + "</>\nFunctions you can use:\n" + help());
            }
        }

        /// <summary>
        /// Exposes a function to be used by the lia script interpreter.
        /// </summary>
        /// <param name="Name">The name the function will be called by in the script.</param>
        /// <param name="Function">The func object to register.</param>
        public static void ExposeFunction(string Name, object Function)
        {
            string functionType = Function.GetType().ToString().Replace("System.Func`", "");
            for (int i = 1; i < 10; i++)
            {
                functionType = functionType.Replace(i.ToString(), "");
            }
            functionType = functionType.Replace("System.", "");
            functionType = functionType.Replace("[", "(");
            functionType = functionType.Replace("]", ")");
            functionType = functionType.Replace(",", ", ");

            exposedFunctions.Add("<color=#f2a841>" + Name + "</><color=#6bdd52>" + functionType + "</>");

            Interpreter.Globals[Name] = Function;
        }

        /// <summary>
        /// Returns a colored script message.
        /// </summary>
        /// <param name="msg">The message to color.</param>
        public static string ScriptMessage(string msg)
        {
            return "<color=#42f4cb>" + msg + "</>";
        }

        #region "Default Script Functions"
        /// <summary>
        /// Returns all currently attached listeners.
        /// </summary>
        private static string getListeners()
        {
            return string.Join("\n", Events.ESystem.ListenerQueue.Select(x => "<color=#f2a841>" + x.Type + 
            (x.TargetedSender != null ? "</> wants <color=#6bdd52>" + x.TargetedSender + "</>" : "</>")));
        }
        /// <summary>
        /// Sets the "returnAll" variable.
        /// </summary>
        /// <param name="setting">The variable to set it to.</param>
        private static string autoReturn(bool setting)
        {
            returnAll = setting;
            if (setting) return ScriptMessage("All scripts will now automatically return values."); else
                return ScriptMessage("Scripts will no longer automatically return values.");
        }
        /// <summary>
        /// Returns all objects attached to the scene.
        /// </summary>
        private static string getObjects()
        {
            return string.Join("\n", Context.Core.Scene.AttachedObjects.Select(x => "<color=#f2a841>" + x.Key + "</> - <color=#6bdd52>" + x.Value.ComponentCount + "</> components"));
        }
        /// <summary>
        /// Prints the system log.
        /// </summary>
        private static string getLog()
        {
            return string.Join("\n", Debugging.Logger.Log);
        }
        /// <summary>
        /// Prints all exposed functions.
        /// </summary>
        private static string help()
        {
            return string.Join("\n", exposedFunctions);
        }
        #endregion
    }
}

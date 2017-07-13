using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Microsoft.Xna.Framework;
using SoulEngine.Modules;

namespace SoulEngine.Modules
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Used to run javascript.
    /// Uses Jint - Public Repository: https://github.com/sebastienros/jint
    /// </summary>
    public class ScriptEngine : IModule
    {
        #region "Declarations"
        /// <summary>
        /// The jint engine.
        /// </summary>
        public static Engine Interpreter = new Engine();
        /// <summary>
        /// List of exposed data and documentation. Used to generate the help menu.
        /// </summary>
        public List<string> helpDocumentation;
        #endregion

        /// <summary>
        /// Setups the scripting engine.
        /// </summary>
        public bool Initialize()
        {
            // Define help documentation.
            helpDocumentation = new List<string>();

            //Add default functions.
            ExposeFunction("help", (Func<string>)help, "Returns help on all exposed functions.");
            ExposeFunction("info", (Func<string>)Info.getInfo, "Returns info on the engine.");

            return true;
        }

        /// <summary>
        /// Executes the provided line of script.
        /// </summary>
        /// <param name="Script">The script to execute as a string.</param>
        public JsValue ExecuteScript(string Script)
        {
            //Check if scripting is enabled.
            if (!Settings.Scripting) return "";

            try
            {
                //Run the script and append a return if specified.
                return Interpreter.Execute(Script).GetCompletionValue();
            }
            catch (Exception e)
            {
                Context.Core.Module<ErrorManager>().RaiseError(e.Message, 50);
                return JsValue.FromObject(Interpreter, "<color=#f44b42>" + e.Message + "</>\nFunctions you can use:\n" + help());
            }
        }

        /// <summary>
        /// Exposes a function to be used by the javascript interpreter.
        /// </summary>
        /// <param name="Name">The name the function will be called by in the script.</param>
        /// <param name="Function">The func object to register.</param>
        /// <param name="Documentation">Documentation on what the function does.</param>
        public void ExposeFunction(string Name, object Function, string Documentation = "")
        {
            //Check if scripting is enabled.
            if (!Settings.Scripting) return;

            string functionType = Function.GetType().ToString();
            string returnType = "";

            for (int i = 1; i < 10; i++)
            {
                functionType = functionType.Replace(i.ToString(), "");
            }
            functionType = functionType.Replace("System.", "");
            functionType = functionType.Replace("[", "(");
            functionType = functionType.Replace("]", ")");
            functionType = functionType.Replace(",", ", ");


            //if the functions returns a value.
            if (functionType.Contains("Func`"))
            {
                functionType = functionType.Replace("Func`", "");
                string[] args = functionType.Split(',');
                returnType = args[args.Length - 1].Replace(")", "").Replace("(", "");
                functionType = string.Join(",", args.SubArray(0, args.Length - 1)) + (args.Length > 1 ? ")" : "");
            }

            //if the function doesn't return a value.
            if (functionType.Contains("Action`"))
            {
                functionType = functionType.Replace("Action`", "");
                returnType = "Nothing";
            }
            if (functionType.Contains("Action"))
            {
                functionType = functionType.Replace("Action", "");
                returnType = "Nothing";
            }

            helpDocumentation.Add("<color=#f2a841>" + Name + "</><color=#6bdd52>" + functionType + "</> => " + returnType + (Documentation != "" ? " | " + Documentation : ""));

            Interpreter.SetValue(Name, Function);
        }

        /// <summary>
        /// Returns a colored script message.
        /// </summary>
        /// <param name="msg">The message to color.</param>
        private string ScriptMessage(string msg)
        {
            return "<color=#42f4cb>" + msg + "</>";
        }

        #region "Default Script Functions"
        

        /// <summary>
        /// Prints all exposed functions.
        /// </summary>
        private string help()
        {
            return string.Join("\n", helpDocumentation);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Microsoft.Xna.Framework;

namespace SoulEngine.Scripting
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Used to run javascript.
    /// Uses Jint - Public Repository: https://github.com/sebastienros/jint
    /// </summary>
    public static class ScriptEngine
    {
        #region "Declarations"
        /// <summary>
        /// The jint engine.
        /// </summary>
        public static Engine Interpreter = new Engine();
        /// <summary>
        /// List of exposed functions.
        /// </summary>
        private static List<string> exposedFunctions;
        #endregion

        /// <summary>
        /// Setups the scripting engine.
        /// </summary>
        public static void SetupScripting()
        {
            //Check if scripting is enabled.
            if (!Settings.Scripting) return;

            //Define an internal list of exposed functions.
            exposedFunctions = new List<string>();

            //Add default functions.
            ExposeFunction("getObjects", (Func<string>)getObjects);
            ExposeFunction("object", (Action<string>) selectObject);
            ExposeFunction("object", (Action<int>)selectObject);
            ExposeFunction("getLog", (Func<string>)getLog);
            ExposeFunction("help", (Func<string>)help);
            ExposeFunction("info", (Func<string>)Info.getInfo);
            ExposeFunction("line", (Action<int, int, int, int>) line);
            ExposeFunction("rect", (Action<int, int, int, int>) rect);

            Interpreter.SetValue("Settings", new Settings());
            Interpreter.SetValue("Lines", Lines);
            Interpreter.SetValue("Rects", Rects);
        }

        /// <summary>
        /// Executes the provided line of script.
        /// </summary>
        /// <param name="Script">The script to execute as a string.</param>
        public static JsValue ExecuteScript(string Script)
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
                if (!Debugging.DebugScene.consoleOpened) Debugging.Logger.Add(e.Message);
                return JsValue.FromObject(Interpreter, "<color=#f44b42>" + e.Message + "</>\nFunctions you can use:\n" + help());
            }
        }

        /// <summary>
        /// Exposes a function to be used by the javascript interpreter.
        /// </summary>
        /// <param name="Name">The name the function will be called by in the script.</param>
        /// <param name="Function">The func object to register.</param>
        public static void ExposeFunction(string Name, object Function)
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

            exposedFunctions.Add("<color=#f2a841>" + Name + "</><color=#6bdd52>" + functionType + "</> => " + returnType);

            Interpreter.SetValue(Name, Function);
        }

        /// <summary>
        /// Returns a colored script message.
        /// </summary>
        /// <param name="msg">The message to color.</param>
        private static string ScriptMessage(string msg)
        {
            return "<color=#42f4cb>" + msg + "</>";
        }

        #region "Default Script Functions"
        /// <summary>
        /// Returns all objects attached to the scene.
        /// </summary>
        private static string getObjects()
        {
            return string.Join("\n", Context.Core.Scene.AttachedObjects.Select(x => "<color=#f2a841>" + x.Key + "</> - <color=#6bdd52>" + x.Value.ComponentCount + "</> components")) + 
                (Context.Core.Scene.AttachedClusters.Count > 0 ? 
                "\n" + string.Join("\n", Context.Core.Scene.AttachedClusters.Select(x => "<color=#f2a841>" + x.Key + "</> - <color=#6bdd52>" + x.Value.Count + "</> components")) :
                "");
        }
        /// <summary>
        /// Draws a border around the object with the provided name.
        /// </summary>
        /// <param name="objectName">The name of the object to draw a border around.</param>
        private static void selectObject(string objectName)
        {
            Debugging.DebugScene.selectedObject = objectName;
        }
        /// <summary>
        /// Draws a border around the object with the provided index.
        /// </summary>
        /// <param name="objectName">The name of the object to draw a border around.</param>
        private static void selectObject(int objectIndex)
        {
            int index = 0;
            string name = null;
            foreach (var item in Context.Core.Scene.AttachedObjects)
            {
                name = item.Key;
                index++;
                if (index == objectIndex) break;
            }
            Debugging.DebugScene.selectedObject = name;
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
        /// <summary>
        /// Draws a line on the screen.
        /// </summary>
        private static List<List<Vector2>> Lines = new List<List<Vector2>>();
        private static void line(int x, int y, int x2, int y2)
        {
            
            List<Vector2> temp = new List<Vector2>();
            temp.Add(new Vector2(x, y));
            temp.Add(new Vector2(x2, y2));
            Lines.Add(temp);
            
        }
        private static List<Rectangle> Rects = new List<Rectangle>();
        private static void rect(int x, int y, int width, int height)
        {
            Rects.Add(new Rectangle(x, y, width, height));
        }
        #endregion

        public static void Draw()
        {
            Context.ink.Start(Enums.DrawChannel.Screen);
            for (int i = 0; i < Lines.Count; i++)
            {
                Context.ink.DrawLine(Lines[i][0], Lines[i][1], 1, Color.Red);
            }
            for (int i = 0; i < Rects.Count; i++)
            {
                Context.ink.DrawRectangle(Rects[i], 1, Color.Red);
            }
            Context.ink.End();
        }
    }
}

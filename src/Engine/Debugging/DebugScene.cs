using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Events;
using SoulEngine.Objects.Components;
using Microsoft.Xna.Framework;

namespace SoulEngine.Debugging
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A scene-like object that manages debugging components.
    /// </summary>
    public static class DebugScene
    {
        private static GameObject stats;
        public static string debugText;

        private static GameObject console;
        private static string consoleInput = "";
        private static string previousInput = "";
        private static string consoleOutput = "<color=#b642f4>" + Info.Name + " " + Info.Version + "</> {" + Info.GUID + "}";
        private static string consoleBlinker = "|";
        private static Ticker consoleBlinkTicker;

        //Console
        private static bool consoleOpened = false;

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public static void Setup()
        {
            stats = GameObject.GenericTextObject;

            stats.Component<ActiveText>().AutoHeight = true;
            stats.Component<ActiveText>().AutoWidth = true;

            console = GameObject.GenericTextObject;
            console.AddComponent(new ActiveTexture());

            console.Component<ActiveTexture>().Texture = AssetManager.BlankTexture;
            console.Component<ActiveTexture>().Tint = Color.Black;
            console.Component<ActiveTexture>().Opacity = 0.5f;

            console.Component<Transform>().Width = Settings.Width;
            console.Component<Transform>().Height = Settings.Height / 2;
            console.Component<Transform>().Y = Settings.Height - console.Component<Transform>().Height;

            UpdateConsoleText();
            consoleBlinkTicker = new Ticker(500, -1, true);

            ESystem.Add(new Listen(EType.GAME_TICKSTART, Update));
            ESystem.Add(new Listen(EType.GAME_FRAMESTART, Compose));
            ESystem.Add(new Listen(EType.GAME_FRAMEEND, DrawHook));

            ESystem.Add(new Listen(EType.KEY_PRESSED, ToggleConsole, Microsoft.Xna.Framework.Input.Keys.OemTilde));
            ESystem.Add(new Listen(EType.KEY_PRESSED, ExecuteConsole, Microsoft.Xna.Framework.Input.Keys.Enter));
            ESystem.Add(new Listen(EType.KEY_PRESSED, ConsolePreviousInput, Microsoft.Xna.Framework.Input.Keys.Up));
            ESystem.Add(new Listen(EType.INPUT_TEXT, ConsoleInput));
            ESystem.Add(new Listen(EType.TICKER_TICK, ConsoleBlinkToggle, consoleBlinkTicker));
        }
        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        private static void Update()
        {
            stats.Component<Transform>().Width = stats.Component<ActiveText>().Width;
            stats.Component<Transform>().Height = stats.Component<ActiveText>().Height;

            stats.Component<ActiveText>().Text = Context.Core.Scene.ToString().Replace("SoulEngine.", "") + "\n" +
                "<border=#000000><color=#e2a712>FPS: " + FPS + "</></>";

            stats.Update();
            if (consoleOpened) console.Update();
        }
        /// <summary>
        /// Composes component textures on linked objects.
        /// </summary>
        private static void Compose()
        {
            //Don't log events caused by the debugger.
            Logger.Enabled = false;
            stats.Compose();
            if (consoleOpened) console.Compose();
            Logger.Enabled = true;
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        private static void DrawHook(Event e)
        {
            FPSCounterUpdate((GameTime)e.Data);

            Context.ink.Start(Enums.DrawChannel.Screen);
            stats.Draw();
            if (consoleOpened) console.Draw();
            Context.ink.End();
        }
        #endregion

        #region "FPS Counter"
        /// <summary>
        /// The frames rendered in the current second.
        /// </summary>
        private static int curFrames = 0;
        /// <summary>
        /// The frames rendered in the last second.
        /// </summary>
        public static int FPS = 0;
        /// <summary>
        /// The current second number.
        /// </summary>
        private static int curSec = 0;
        private static void FPSCounterUpdate(GameTime gameTime)
        {
            //Check if the current second has passed.
            if (curSec != gameTime.TotalGameTime.Seconds)
            {
                curSec = gameTime.TotalGameTime.Seconds; //Assign the current second to a variable.
                FPS = curFrames; //Set the current second's frames to the last second's frames as a second has passed.
                curFrames = 0;
            }
            curFrames += 1;
        }
        #endregion

        #region "Console"
        /// <summary>
        /// Toggles the console on or off.
        /// </summary>
        private static void ToggleConsole()
        {
            consoleOpened = !consoleOpened;
        }
        /// <summary>
        /// Accepts text input to display to the console.
        /// </summary>
        /// <param name="e"></param>
        private static void ConsoleInput(Event e)
        {
            if (!consoleOpened || (string) e.Data == "`") return;

            if ((string)e.Data != "\b") consoleInput += e.Data;
            else
            {
                if(consoleInput.Length != 0) consoleInput = consoleInput.Substring(0, consoleInput.Length - 1);
            }

            UpdateConsoleText();
        }
        /// <summary>
        /// Executes the console input through the LUA interpreter.
        /// </summary>
        private static void ExecuteConsole()
        {
            if (!consoleOpened) return;

            //Send input to the script engine to process.
            consoleOutput = ScriptEngine.ExecuteScript(consoleInput).ToPrintString();
            previousInput = consoleInput;
            consoleInput = "";

            UpdateConsoleText();
        }
        /// <summary>
        /// Updates the text that the console displays.
        /// </summary>
        private static void UpdateConsoleText()
        {
            console.Component<ActiveText>().Text = consoleOutput + "\n" + "> " + consoleInput + consoleBlinker;
        }

        /// <summary>
        /// Selection blinker.
        /// </summary>
        private static void ConsoleBlinkToggle()
        {
            if (consoleBlinker == "|") consoleBlinker = ""; else consoleBlinker = "|";
            UpdateConsoleText();
        }

        /// <summary>
        /// Inserts the previous console input in the input field.
        /// </summary>
        private static void ConsolePreviousInput()
        {
            if (!consoleOpened) return;

            consoleInput = previousInput;
        }
        #endregion
    }
}

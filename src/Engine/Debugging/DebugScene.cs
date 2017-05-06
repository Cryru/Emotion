using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Events;
using SoulEngine.Objects.Components;
using Microsoft.Xna.Framework;
using SoulEngine.Scripting;

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
        private static string consoleOutput = Info.getInfo() + "\n" + 
            "Start typing and press Enter to execute code. To scroll use the mouse wheel." + "\n" +
            "Use the arrows to navigate and the Up arrow to repeat the previous command.";
        private static string consoleBlinker = "|";
        private static Ticker consoleBlinkTicker;
        private static int blinkerLocation = 0;

        //Console
        public static bool consoleOpened = false;

        public static string selectedObject = "";

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public static void Setup()
        {
            stats = GameObject.GenericTextObject;

            stats.AddComponent(new ActiveTexture(Enums.TextureMode.Stretch, AssetManager.BlankTexture));
            stats.Component<ActiveTexture>().Opacity = 0.5f;
            stats.Component<ActiveTexture>().Tint = Color.Black;

            stats.Component<ActiveText>().AutoHeight = true;
            stats.Component<ActiveText>().AutoWidth = true;
            stats.Component<ActiveText>().Padding = new Vector2(Functions.ManualRatio(3, 540), Functions.ManualRatio(3, 540));

            console = GameObject.GenericTextObject;
            console.AddComponent(new ActiveTexture(Enums.TextureMode.Stretch, AssetManager.BlankTexture));
            console.Component<ActiveTexture>().Tint = Color.Black;
            console.Component<ActiveTexture>().Opacity = 0.5f;

            console.Component<ActiveText>().Style = Enums.TextStyle.Justified;

            console.Width = Settings.Width;
            console.Height = Settings.Height / 2;
            console.Y = Settings.Height - console.Height;
            console.Layer = Enums.ObjectLayer.UI;
            console.AddComponent(new MouseInput());

            consoleBlinkTicker = new Ticker(300, -1, true);

            Context.Core.OnUpdate += Update;
            Context.Core.OnCompose += Compose;
            Context.Core.OnDrawEnd += DrawHook;

            Input.OnKeyDown += KeysManager;
            console.Component<MouseInput>().OnMouseScroll += ScrollManager;
            Input.OnTextInput += ConsoleInput;
            consoleBlinkTicker.OnTick += ConsoleBlinkToggle;

            Logger.Add("Debugging enabled!");
        }

        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        private static void Update(object sender, SoulUpdateEventArgs e)
        {
            stats.Component<ActiveText>().Text = Context.Core.Scene.ToString().Replace("SoulEngine.", "") + "\n" +
                "<border=#000000><color=#e2a712>FPS: " + FPS + "</></>" + (debugText == null ? "" : "\n" + debugText);

            stats.Width = stats.Component<ActiveText>().Width + Functions.ManualRatio(6, 540);
            stats.Height = stats.Component<ActiveText>().Height + Functions.ManualRatio(6, 540);
            stats.Update();

            //Update display status.
            console.Drawing = consoleOpened;
            console.Update();
        }
        /// <summary>
        /// Composes component textures on linked objects.
        /// </summary>
        private static void Compose(object sender, SoulUpdateEventArgs e)
        {
            //Don't log events caused by the debugger.
            Logger.Enabled = false;
            stats.Compose();
            console.Compose();
            Logger.Enabled = true;
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        private static void DrawHook(object sender, SoulUpdateEventArgs e)
        {
            FPSCounterUpdate(e.updateTime);

            Context.ink.Start(Enums.DrawChannel.Screen);
            stats.Draw();
            console.Draw();
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
            console.Component<ActiveText>().ScrollBottom();
        }
        /// <summary>
        /// Accepts text input to display to the console.
        /// </summary>
        private static void ConsoleInput(object sender, TextInputEventArgs e)
        {
            string inputChar = e.Character.ToString();

            if (!consoleOpened || inputChar == "`" || inputChar == "\r") return;

            if (inputChar != "\b")
            {
                consoleInput = consoleInput.Substring(0, blinkerLocation) + inputChar + consoleInput.Substring(blinkerLocation);
                blinkerLocation++;
            }
            else
            {
                if (consoleInput.Length != 0 && blinkerLocation > 0)
                {
                    consoleInput = consoleInput.Substring(0, blinkerLocation - 1) + consoleInput.Substring(blinkerLocation);
                    blinkerLocation--;
                }
            }

            console.Component<ActiveText>().ScrollBottom();
            UpdateConsoleText();
        }
        /// <summary>
        /// Executes the console input through the JS interpreter.
        /// </summary>
        private static void ExecuteConsole()
        {
            if (!consoleOpened) return;

            //Send input to the script engine to process.
            consoleOutput = ScriptEngine.ExecuteScript(consoleInput).ToString();
            previousInput = consoleInput;
            consoleInput = "";

            UpdateConsoleText();

            console.Component<ActiveText>().ScrollBottom();
        }
        /// <summary>
        /// Updates the text that the console displays.
        /// </summary>
        private static void UpdateConsoleText()
        {
            if (blinkerLocation > consoleInput.Length) blinkerLocation = Math.Max(0, consoleInput.Length);

            console.Component<ActiveText>().Text = consoleOutput + "\n" + "> " + consoleInput.Substring(0, blinkerLocation) + consoleBlinker + 
                consoleInput.Substring(blinkerLocation);
        }

        /// <summary>
        /// Selection blinker.
        /// </summary>
        private static void ConsoleBlinkToggle(object sender, EventArgs e)
        {
            if (consoleBlinker == "|") consoleBlinker = " "; else consoleBlinker = "|";
            UpdateConsoleText();
        }

        /// <summary>
        /// Inserts the previous console input in the input field.
        /// </summary>
        private static void ConsolePreviousInput()
        {
            if (!consoleOpened) return;

            consoleInput = previousInput;
            blinkerLocation = previousInput.Length;

            UpdateConsoleText();
        }

        private static void MoveBlinkerRight()
        {
            blinkerLocation += 1;
            consoleBlinker = " ";
            UpdateConsoleText();
        }

        private static void MoveBlinkerLeft()
        {
            if(blinkerLocation != 0) blinkerLocation -= 1;
            consoleBlinker = " ";
            UpdateConsoleText();
        }


        private static void ScrollUp()
        {
            if (console.Component<ActiveText>().TextHeight <= console.Height) return;
            if (console.Component<ActiveText>().Scroll.Y == -15) return;

            console.Component<ActiveText>().ScrollLineUp();
            UpdateConsoleText();
        }

        private static void ScrollDown()
        {
            if (console.Component<ActiveText>().Scroll.Y <= console.Height - console.Component<ActiveText>().TextHeight) return;
            console.Component<ActiveText>().ScrollLineDown();
            UpdateConsoleText();
        }
        #endregion

        #region "Event Wrappers"
        private static void KeysManager(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Microsoft.Xna.Framework.Input.Keys.OemTilde:
                    ToggleConsole();
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Enter:
                    ExecuteConsole();
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Up:
                    ConsolePreviousInput();
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Left:
                    MoveBlinkerLeft();
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Right:
                    MoveBlinkerRight();
                    break;

            }
        }
        private static void ScrollManager(object sender, MouseScrollEventArgs e)
        {
            if(e.ScrollAmount < 0)
            {
                ScrollDown();
            }
            else
            {
                ScrollUp();
            }
        }
        #endregion
    }
}

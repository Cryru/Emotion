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
using SoulEngine.Modules;

namespace SoulEngine.Modules
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A scene-like object that manages debugging components.
    /// </summary>
    public class DebugModule : IModuleUpdatable, IModuleDrawable, IModuleComposable
    {
        private GameObject stats;
        public string debugText;

        private GameObject console;
        private string consoleInput = "";
        private string previousInput = "";
        private string consoleOutput = Info.getInfo() + "\n" +
            "Start typing and press Enter to execute code. To scroll use the mouse wheel." + "\n" +
            "Use the arrows to navigate and the Up arrow to repeat the previous command.";
        private string consoleBlinker = "|";
        private Ticker consoleBlinkTicker;
        private int blinkerLocation = 0;

        //Console
        public bool consoleOpened = false;

        public string selectedObject = "";

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public bool Initialize()
        {
            // Check if scripting is enabled, since it is required for the debugging module.
            if (!Context.Core.isModuleLoaded<ScriptEngine>()) return false;

            // The stats in the top left corner.
            stats = GameObject.GenericDrawObject;
            stats.Component<ActiveTexture>().ActualTexture = AssetManager.BlankTexture;
            stats.Component<ActiveTexture>().Opacity = 0.5f;
            stats.Component<ActiveTexture>().Tint = Color.Black;
            stats.AddComponent(new ActiveText());
            stats.Component<ActiveText>().AutoHeight = true;
            stats.Component<ActiveText>().AutoWidth = true;
            stats.Component<ActiveText>().Padding = new Vector2(Functions.ManualRatio(3, 540), Functions.ManualRatio(3, 540));
            
            // The console.
            console = GameObject.GenericDrawObject;
            console.Component<ActiveTexture>().ActualTexture = AssetManager.BlankTexture;
            console.Component<ActiveTexture>().Tint = Color.Black;
            console.Component<ActiveTexture>().Opacity = 0.5f;
            console.AddComponent(new ActiveText());
            console.Width = Settings.Width;
            console.Height = Settings.Height / 2;
            console.Y = Settings.Height - console.Height;
            console.Layer = Enums.ObjectLayer.UI;
            console.AddComponent(new MouseInput());

            // Ticker for the console blink.
            consoleBlinkTicker = new Ticker(300, -1, true);

            Input.OnKeyDown += KeysManager;
            console.Component<MouseInput>().OnMouseScroll += ScrollManager;
            Input.OnTextInput += ConsoleInput;
            consoleBlinkTicker.OnTick += ConsoleBlinkToggle;

            // Refresh the console text.
            UpdateConsoleText();

            return true;
        }

        float delayer;
        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        public void Update()
        {
            if(Context.Core.Scene != null)
            {
                stats.Component<ActiveText>().Text = (Context.Core.__sceneSetupAllowed ? "Loading: " : "") + Context.Core.Scene.ToString().Replace("SoulEngine.", "") + "\n" +
               "<border=#000000><color=#e2a712>FPS: " + FPS + "</></>" + (debugText == null ? "" : "\n" + debugText);
            }
           
            stats.Width = stats.Component<ActiveText>().Width + Functions.ManualRatio(6, 540);
            stats.Height = stats.Component<ActiveText>().Height + Functions.ManualRatio(6, 540);
            stats.Update();

            //Update display status.
            console.Drawing = consoleOpened;
            console.Update();

            //Update buttons on a delay.
            delayer += Context.Core.frameTime;
            if (delayer > 100)
            {
                delayer -= 100;

                if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    MoveBlinkerLeft();
                }

                if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    MoveBlinkerRight();
                }
            }
        }
        /// <summary>
        /// Composes component textures on linked objects.
        /// </summary>
        public void Compose()
        {
            stats.Compose();
            console.Compose();
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        public void Draw()
        {
            FPSCounterUpdate();

            Context.ink.Start(Enums.DrawMatrix.Screen);
            stats.Draw();
            console.Draw();
            Context.ink.End();
        }
        #endregion

        #region "FPS Counter"
        /// <summary>
        /// The frames rendered in the last second.
        /// </summary>
        public int FPS = 0;
        /// <summary>
        /// The time.
        /// </summary>
        private float secondCounter = 0;
        private void FPSCounterUpdate()
        {
            secondCounter += Context.Core.frameTime;

            //Check if one second has passed.
            if (secondCounter > 1000)
            {
                secondCounter -= 1000;
                FPS = (int)(1000f / Context.Core.frameTime);
            }
        }
        #endregion

        #region "Console"
        /// <summary>
        /// Toggles the console on or off.
        /// </summary>
        private void ToggleConsole()
        {
            consoleOpened = !consoleOpened;
            console.Component<ActiveText>().ScrollBottom();
        }
        /// <summary>
        /// Accepts text input to display to the console.
        /// </summary>
        private void ConsoleInput(object sender, TextInputEventArgs e)
        {
            string inputChar = e.Character.ToString();

            if (!consoleOpened || inputChar == "`" || inputChar == "\r") return;

            if (inputChar == "<") inputChar = "\\<";
            if (inputChar == ">") inputChar = "\\>";

            if (inputChar != "\b")
            {
                consoleInput = consoleInput.Substring(0, blinkerLocation) + inputChar + consoleInput.Substring(blinkerLocation);
                blinkerLocation += inputChar.Length;
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
        private void ExecuteConsole()
        {
            if (!consoleOpened) return;

            //Send input to the script engine to process.
            consoleOutput = Context.Core.Module<ScriptEngine>().ExecuteScript(consoleInput).ToString();
            previousInput = consoleInput;
            consoleInput = "";

            UpdateConsoleText();

            console.Component<ActiveText>().ScrollBottom();
        }
        /// <summary>
        /// Updates the text that the console displays.
        /// </summary>
        private void UpdateConsoleText()
        {
            if (blinkerLocation > consoleInput.Length) blinkerLocation = Math.Max(0, consoleInput.Length);

            console.Component<ActiveText>().Text = consoleOutput + "\n" + "> " + consoleInput.Substring(0, blinkerLocation) + consoleBlinker +
                consoleInput.Substring(blinkerLocation);
        }

        /// <summary>
        /// Selection blinker.
        /// </summary>
        private void ConsoleBlinkToggle(object sender, EventArgs e)
        {
            if (consoleBlinker == "|") consoleBlinker = " "; else consoleBlinker = "|";
            UpdateConsoleText();
        }

        /// <summary>
        /// Inserts the previous console input in the input field.
        /// </summary>
        private void ConsolePreviousInput()
        {
            if (!consoleOpened) return;

            consoleInput = previousInput;
            blinkerLocation = previousInput.Length;

            UpdateConsoleText();
        }

        private void MoveBlinkerRight()
        {
            blinkerLocation += 1;
            consoleBlinker = "|";
            UpdateConsoleText();
        }

        private void MoveBlinkerLeft()
        {
            if (blinkerLocation != 0) blinkerLocation -= 1;
            consoleBlinker = "|";
            UpdateConsoleText();
        }


        private void ScrollUp()
        {
            if (console.Component<ActiveText>().TextHeight <= console.Height) return;
            if (console.Component<ActiveText>().Scroll.Y == 4) return;

            console.Component<ActiveText>().ScrollLineUp();
            UpdateConsoleText();
        }

        private void ScrollDown()
        {
            if (console.Component<ActiveText>().Scroll.Y <= console.Height - console.Component<ActiveText>().TextHeight) return;
            console.Component<ActiveText>().ScrollLineDown();
            UpdateConsoleText();
        }
        #endregion

        #region "Event Wrappers"
        private void KeysManager(object sender, KeyEventArgs e)
        {
            switch (e.Key)
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
                    delayer = -300;
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Right:
                    MoveBlinkerRight();
                    delayer = -300;
                    break;
            }
        }
        private void ScrollManager(object sender, MouseScrollEventArgs e)
        {
            if (e.ScrollAmount > 0)
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

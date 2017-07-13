using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Enums;
using SoulEngine.Events;
using SoulEngine.Debugging;
using SoulEngine.Scripting;
using System.Threading;
using SoulEngine.Modules;
using System.Diagnostics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The game instance for use by the engine.
    /// </summary>
    public class Core : Game
    {
        #region Variables
        /// <summary>
        /// The time in milliseconds it took for the last frame to render.
        /// </summary>
        public float frameTime = 0;

        public bool Paused
        {
            get
            {
                return (Context.Core.isModuleLoaded<DebugModule>() && Context.Core.Module<DebugModule>().consoleOpened) ||
                    (IsActive == false && Settings.PauseOnFocusLoss);
            }
        }
        #endregion

        #region Systems
        /// <summary>
        /// Loaded modules.
        /// </summary>
        private List<IModule> Modules = new List<IModule> {
            Settings.Debug ? new Logger() : null,
            new ErrorManager(),
            new AssetManager(),
            new WindowManager(),
            new TimingManager(),
            new SceneManager(),
            new InputModule(),
            new SoundEngine(),
            Settings.Networking ? new Networking() : null,
            Settings.Scripting ? new ScriptEngine() : null,
            Settings.Debug ? new DebugModule() : null
        };
        private List<string> ModulesIndex = new List<string>();
        #endregion

        #region Allow Flags
        /// <summary>
        /// Used to check whether composing is done properly.
        /// </summary>
        public bool __composeAllowed = false;
        #endregion

        #region "Initialization"
        /// <summary>
        /// The initializer.
        /// </summary>
        public Core()
        {
            //Setup the graphics device.
            Context.GraphicsManager = new GraphicsDeviceManager(this);

            //Setup the Content root folder for the master scene. The root for this folder is the exe.
            Content.RootDirectory = "Content";

            //Reroute window events to the core and input classes.
            Window.TextInput += Input_TextInput;
        }

        /// <summary>
        /// Setups the spritebatch and starts the start sequence.
        /// </summary>
        protected override void LoadContent()
        {
            //Apply settings.
            IsMouseVisible = Settings.RenderMouse;
            IsFixedTimeStep = Settings.FPS > 0 ? true : false; //Check whether to cap FPS based on the fps target.
            TargetElapsedTime = Settings.FPS > 0 ? TimeSpan.FromMilliseconds(Math.Floor(1000f / Settings.FPS)) : TimeSpan.FromSeconds(1);
            Context.GraphicsManager.SynchronizeWithVerticalRetrace = Settings.vSync;
            Window.Title = Settings.WName;

            //Apply hardcoded settings.
            Window.AllowAltF4 = true;

            //Setup the brush for drawing.
            Context.ink = new SpriteBatch(GraphicsDevice);

            // Load modules.
            LoadModules();

            // Measure boot time.
            Starter.bootPerformance.Stop();
            if (Context.Core.isModuleLoaded<Logger>())
                Context.Core.Module<Logger>().Add("Engine loading completed in: " + Starter.bootPerformance.ElapsedMilliseconds + "ms");
        }
        #endregion

        #region Module System
        /// <summary>
        /// Loads all modules defined in the core list.
        /// </summary>
        private void LoadModules()
        {
            // Clear the index list.
            ModulesIndex.Clear();

            for (int i = 0; i < Modules.Count; i++)
            {

                // Check if null, in which case we skip loading, but still add to the index to maintain positions.
                if (Modules[i] == null)
                {
                    ModulesIndex.Add("");
                    continue;
                }

                // Measure how fast each module boots.
                Stopwatch moduleBootMeasure = new Stopwatch();
                moduleBootMeasure.Start();

                // Initialize module.
                if (Modules[i].Initialize())
                {
                    moduleBootMeasure.Stop();

                    // If logger is loaded, log boot time.
                    if (isModuleLoaded<Logger>())
                        Context.Core.Module<Logger>().Add("Module " + Modules[i].GetType().ToString() + " loaded in " + moduleBootMeasure.ElapsedMilliseconds + " ms.");

                    // Index the module for quicker searching in the future.
                    ModulesIndex.Add(Modules[i].GetType().ToString());
                }
                else
                {
                    // If the error manager is loaded, log an error.
                    if (isModuleLoaded<ErrorManager>())
                        Context.Core.Module<ErrorManager>().RaiseError("Module " + Modules[i].GetType().ToString() + "failed to load.", 100);

                    // IPut in an empty string to maintain other modules' order.
                    ModulesIndex.Add("Non-loaded module: " + Modules[i].GetType().ToString());
                }
            }
        }

        /// <summary>
        /// Returns the loaded module by type. Returns a null reference if the module isn't loaded.
        /// </summary>
        /// <typeparam name="T">The type of module.</typeparam>
        /// <returns>The module object.</returns>
        public T Module<T>()
        {
            return (T)Convert.ChangeType(Modules[IdModule<T>()], typeof(T));
        }

        /// <summary>
        /// Returns the id of the module within the loaded modules list.
        /// </summary>
        /// <typeparam name="T">The type of the module.</typeparam>
        /// <returns>The id of the module, -1 if not attached.</returns>
        private int IdModule<T>()
        {
            int id = ModulesIndex.IndexOf(typeof(T).ToString());

            if (id == -1 && isModuleLoaded<ErrorManager>())
            {
                Module<ErrorManager>().RaiseError("Expected module " + typeof(T).ToString() + " to be loaded.", 101);
            }

            return id;
        }

        /// <summary>
        /// Returns whether the selected module is loaded.
        /// </summary>
        /// <typeparam name="T">The type of the module.</typeparam>
        /// <returns>True if loaded, false if not.</returns>
        public bool isModuleLoaded<T>()
        {
            return ModulesIndex.IndexOf(typeof(T).ToString()) != -1;
        }
        #endregion

        #region "Loops"
        /// <summary>
        /// Is executed every tick.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Run core modules that require updating.
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i] is IModuleUpdatable)
                {
                    ((IModuleUpdatable)Modules[i]).Update();
                }
            }
        }

        /// <summary>
        /// Is executed every frame.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //If the game is not focused, don't update.
            if (IsActive == false && Settings.PauseOnFocusLoss) return;

            //Record frametime.
            frameTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Compose textures, we do this first due to a bug which prevents us to switch render targets in between renders.
            __composeAllowed = true;
            // Run core modules that require composing.
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i] is IModuleComposable)
                {
                    ((IModuleComposable)Modules[i]).Compose();
                }
            }
            __composeAllowed = false;

            // Clear the screen, then the drawing area.
            Context.Graphics.Clear(Color.Black);
            Context.ink.Start(DrawMatrix.Screen);
            Context.ink.Draw(AssetManager.BlankTexture, new Rectangle(0, 0, Settings.Width, Settings.Height), Settings.FillColor);
            Context.ink.End();

            // Run core modules that require drawing.
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i] is IModuleDrawable)
                {
                    ((IModuleDrawable)Modules[i]).Draw();
                }
            }
        }
        #endregion

        #region "Event Rerouting"
        /// <summary>
        /// Is triggered when text input is detected to the game.
        /// This returns the character as input, for instance a backspace is '/b' and so on.
        /// </summary>
        private void Input_TextInput(object sender, TextInputEventArgs e)
        {
            InputModule.triggerTextInput(sender, e);
        }
        #endregion
    }
}


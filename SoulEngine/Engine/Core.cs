// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Reflection;
using Raya.Graphics;
using Raya.System;
using Soul.Engine.Enums;
using Soul.Engine.Internal;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine
{
    public static class Core
    {
        #region Properties

        /// <summary>
        /// Whether the game is paused.
        /// </summary>
        public static bool Paused;

        #endregion

        #region Information

        /// <summary>
        /// The time between frames. Used for accurate time keeping.
        /// </summary>
        public static int FrameTime
        {
            get { return _frameTime; }
        }

        #endregion

        #region Internals

        /// <summary>
        /// Internal frame time variable.
        /// </summary>
        private static int _frameTime;

        /// <summary>
        /// Whether the engine has been started.
        /// </summary>
        private static bool Started
        {
            get { return NativeContext != null; }
        }

        /// <summary>
        /// The scene to display when loading another.
        /// </summary>
        internal static Scene LoadingScene;

        #endregion

        #region Raya API

        /// <summary>
        /// The Raya context.
        /// </summary>
        public static Context NativeContext;

        #endregion

        public static void Start(Scene startScene, string sceneName = "startScene", Scene loadingScene = null)
        {
            // Assign the loading scene.
            LoadingScene = loadingScene;

            // Create a Raya context.
            NativeContext = new Context();

            // Start boot timer.
            Clock bootTime = new Clock();

            // Setup logger.
            Logger.Enabled = true;
            Logger.LogLimit = 2;
            Logger.Stamp = "==========\n" + "SoulEngine 2018 Log" + "\n==========";

            // Connect to the SoulLib error manager.
            ErrorManager.ErrorCallback += (error) => { Error.Raise(999, error, Severity.Critical); };

            // Create the window.
            NativeContext.CreateWindow();

            // Initiate modules.
            Input.Update(); // Input first.
            ScriptEngine.Update(); // Scripting afterward as debugging depends on it.
            Debugger.Update(); // Debugging then so we have it up early.
            PhysicsModule.Update(); // Physics module is next as it's a user module.
            AssetLoader.Setup(); // The asset loader needs to be ready for the scene manager.
            SceneManager.Update(); // SceneManager is last because it's the primary user module and somewhat depends on Physics.

            // Send debugging boot messages.
            Debugger.DebugMessage(DebugMessageSource.Boot,
                "SoulEngine 2018 " + Assembly.GetExecutingAssembly().GetName().Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, "Using: ");
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- Raya " + Raya.Meta.Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- SoulLib " + Soul.Meta.Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- SoulPhysics " + Physics.Meta.Version);

            // Apply settings.
            Settings.ApplySettings();

            // Hook logger to the closing events.
            NativeContext.Closed += (sender, args) => Logger.ForceDump();
            NativeContext.LostFocus += (sender, args) =>
            {
                if (Settings.PauseOnFocusLoss) Paused = true;
            };
            NativeContext.GainedFocus += (sender, args) =>
            {
                if (Settings.PauseOnFocusLoss) Paused = false;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.ForceDump();

            // Boot ready.
            Debugger.DebugMessage(DebugMessageSource.Boot,
                "Boot took " + bootTime.ElapsedTime.AsMilliseconds() + " ms");
            bootTime.Dispose();

            // Load the starting scene, if any.
            if (startScene != null) SceneManager.LoadScene(sceneName, startScene, true);

            // Define the timing clock.
            Clock timingClock = new Clock();

            // Start main loop.
            while (NativeContext.Running)
            {
                // Get the time since the last frame time timer restart, which is the time it took for the last frame to render.
                _frameTime = timingClock.ElapsedTime.AsMilliseconds();

                // Check if the timer is 0, this happens when the game is paused.
                if (_frameTime == 0 && Settings.FPSCap > 0)
                {
                    // In this case we correct it to the cap.
                    _frameTime = 1000 / Settings.FPSCap;
                }

                // Restart the frame time timer, this is done first and outside of the pause loop to ensure timing is consistent always.
                timingClock.Restart();

                // Update modules.
                Input.Update(); // Input first, as we want accurate input data for the frame, including the debugger.
                Debugger.Update(); // Debugging second as the console can cause focus loss, and we want it to run as a priority. 

                // Run Raya events, this is outside of the pause as it covers window events.
                NativeContext.Tick();

                // If the game is paused don't run any game related modules and processes.
                if (Paused || (Debugger.ManualMode && !Debugger.AdvanceFrame)) continue;

                // Run game related modules.
                ScriptEngine.Update(); // Scripting first to update any script data.
                PhysicsModule.Update(); // Physics to update bodies.

                // Start drawing.
                NativeContext.StartDraw();

                // Update the screen manager.
                SceneManager.Update(); // The scene manager is last inside a draw cycle as data should already be updated and is up for being drawn.

                // Finish drawing frame.
                NativeContext.EndDraw();
            }
        }

        #region Drawing

        /// <summary>
        /// Draws a drawable Raya object.
        /// </summary>
        /// <param name="drawable"></param>
        public static void Draw(Drawable drawable)
        {
            if (NativeContext == null)
            {
                Error.Raise(0, "SoulEngine's Core hasn't been started.", Severity.Critical);
                return;
            }


            NativeContext.Draw(drawable);
        }

        #endregion
    }
}
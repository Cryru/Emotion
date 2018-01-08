// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Systems;
using OpenTK;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Soul.Engine
{
    public static class Core
    {
        #region Declarations

        /// <summary>
        /// Whether the engine is running.
        /// </summary>
        public static bool Running { get; private set; }

        /// <summary>
        /// Whether the engine is paused. This is different from the game being paused. In this state nothing is rendered.
        /// </summary>
        public static bool Paused;

        /// <summary>
        /// The Breath window.
        /// </summary>
        internal static Window BreathWin;

        #endregion

        public static void Setup(Scene startingScene)
        {
            // Set running to true.
            Running = true;

            // Load the error handling module first.
            ErrorHandling.Setup();

            // Load the scripting engine.
            Scripting.Setup();
#if DEBUG
            // Load the debugging module.
            Debugging.Setup();
#endif

            // Apply pre-settings.
            Settings.ApplyPreSettings();

            // Create a Breath window.
            BreathWin = new Window(() =>
            {

                // Setup the scene manager and load the starting scene.
                SceneManager.Setup();
                SceneManager.LoadScene("start", startingScene, true);
            }, Update, Draw);

            // Apply first run settings.
            BreathWin.WindowBorder = WindowBorder.Fixed;

            // Apply runtime settings.
            //Settings.ApplySettings();

            // Hook up events.
            BreathWin.FocusedChanged += (e, args) => { Paused = !BreathWin.Focused; };

            // Start the loop.
            BreathWin.Start((double) Settings.TPS, (double) Settings.FPS);
        }

        #region Loops

        private static void Update()
        {
#if DEBUG
            // Update the debugger.
            Debugging.Update();
#endif
            // Check if paused.
            if (Paused) return;

            // Update the scripting engine.
            Scripting.Update();

            // Update the scene manager. This updates the scene loading, all loaded systems and components, and by proxy runs all the game logic.
            SceneManager.Update();

            Window.Current.Title = "SE2018 DEMO " + (1000 / BreathWin.FrameTime);
        }

        private static void Draw()
        {
            // Draw the current scene. This propagates to the draw hook inside the scene.
           SceneManager.Draw();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Stops engine execution.
        /// </summary>
        public static void Stop(bool error = false)
        {
            Running = false;
            BreathWin.Close();
            BreathWin.Exit();
            BreathWin.Dispose();
            Environment.Exit(error ? 1 : 0);
        }

        #endregion
    }
}
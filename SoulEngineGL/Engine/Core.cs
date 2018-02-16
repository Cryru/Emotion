// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;
using Soul.Engine.Diagnostics;
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
        /// The MonoGame context running.
        /// </summary>
        public static Context Context;

        #endregion

        /// <summary>
        /// Setups SoulEngine.
        /// </summary>
        /// <param name="startingScene">The scene to start with.</param>
        public static void Setup(Scene startingScene)
        {
            // Create a context.
            Context = new Context(() =>
            {
                // Load the scripting module.
                Scripting.Setup();

#if DEBUG
                // Load the debugging module.
                Debugging.Setup();
#endif

                // Apply settings.
                Settings.ApplySettings();

                // Setup modules.
                AssetLoader.Setup();
                WindowManager.Setup();
                SceneManager.Setup();

                // Set running to true.
                Running = true;

                // Load the provided first scene.
                SceneManager.LoadScene("0", startingScene, true);

            }, Update, Draw);

            // Run the context.
            Context.Run();

            // Shutdown.
            Context.Dispose();
            Environment.Exit(0);

            //            // Load the input module.
            //            Input.Setup();


            //            // Create a Breath window.
            //            BreathWin = new Window(null, Update, Draw, new Vector2(Settings.Width, Settings.Height));

            //            // Setup the scene manager and load the starting scene.
            //            SceneManager.Setup();
            //            SceneManager.LoadScene("start", startingScene, true);

            //            // Apply runtime settings.
            //            Settings.ApplySettings();

            //            // Hook up events.
            //            BreathWin.FocusedChanged += (e, args) => { Paused = !BreathWin.Focused; };

            //            // Start the loop.
            //            BreathWin.Start(Settings.TPS, Settings.FPS);
        }

        public static void Update()
        {
            if(!Running) return;

#if DEBUG
            // Update the debugger.
            Debugging.Update();
#endif
            //            // Check if paused.
            //            if (Paused) return;

            //            // Update input.
            //            Input.Update();

            //            // Update the scripting engine.
            //            Scripting.Update();

            // Update the scene manager. This updates the scene loading, and the current scene.
            SceneManager.Update();

            //            // End the input update.
            //            Input.UpdateEnd();
        }

        private static void Draw()
        {
            if(!Running) return;

            // Clear the screen.
            Context.GraphicsDevice.Clear(Color.Black);

            // Draw the current scene. This propagates to the draw hook inside the scene.
            SceneManager.Draw();
        }

        /// <summary>
        /// Stops engine execution.
        /// </summary>
        public static void Stop()
        {
            if(!Running) return;

            Running = false;
            Context.Exit();            
        }
    }
}
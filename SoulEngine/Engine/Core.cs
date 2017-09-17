// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Reflection;
using Raya.Graphics;
using Raya.System;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine
{
    public static class Core
    {
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

        #endregion

        #region Raya API

        /// <summary>
        /// The Raya context.
        /// </summary>
        public static Context NativeContext;

        #endregion

        public static void Start(Scene startScene, string sceneName = "startScene")
        {
            // Create a Raya context.
            NativeContext = new Context();

            // Start boot timer.
            Clock bootTime = new Clock();

            // Setup logger.
            Logger.Enabled = true;
            Logger.LogLimit = 2;
            Logger.Stamp = "==========\n" + "SoulEngine 2018 Log" + "\n==========";

            // Send debugging boot messages.
            Debugger.DebugMessage(DebugMessageSource.Boot,
                "Starting SoulEngine 2018 " + Assembly.GetExecutingAssembly().GetName().Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, "Using: ");
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- Raya " + Raya.System.Meta.Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- SoulLib " + Info.SoulVersion);
            Debugger.DebugMessage(DebugMessageSource.Boot, " |- SoulPhysics " + Physics.Meta.Version);

            // Create the window.
            NativeContext.CreateWindow();

            // Hook logger to the closing events.
            NativeContext.Closed += (sender, args) => Logger.ForceDump();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.ForceDump();

            // Initiate modules.
            Input.Start();
            ScriptEngine.Start();
            Debugger.Start();
            SceneManager.Start();
            PhysicsModule.Start();

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

                // Restart the frame time timer.
                timingClock.Restart();

                // Tick events and update Raya.
                NativeContext.Tick();

                // Start drawing.
                NativeContext.StartDraw();

                // Update modules.
                Input.Update();
                ScriptEngine.Update();
                Debugger.Update();
                SceneManager.Update();
                PhysicsModule.Update();

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
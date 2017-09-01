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
            get { return _nativeContext != null; }
        }

        #endregion

        #region Raya API

        /// <summary>
        /// The context within the actor system.
        /// </summary>
        private static Context _nativeContext;

        #endregion

        public static void Start(Scene startScene)
        {
            // Create a Raya context.
            _nativeContext = new Context();

            // Start boot timer.
            Clock bootTime = new Clock();

            // Setup logger.
            // Start Soul logging.
            Logger.Enabled = true;
            Logger.LogLimit = 2;
            Logger.Stamp = "==========\n" + "SoulEngine 2018 Log" + "\n==========";

            // Send debugging boot messages.
            Debugger.DebugMessage(DebugMessageSource.Boot,
                "Starting SoulEngine " + Assembly.GetExecutingAssembly().GetName().Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, "Using: ");
            Debugger.DebugMessage(DebugMessageSource.Boot, "Raya " + Meta.Version);
            Debugger.DebugMessage(DebugMessageSource.Boot, "SoulLib " + Info.SoulVersion);

            // Create the window.
            _nativeContext.CreateWindow();

            // Hook logger to the closing events.
            _nativeContext.Closed += (sender, args) => Logger.ForceDump();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.ForceDump();

            // Initiate modules.
            Debugger.Start();
            SceneManager.Start();
            ScriptEngine.Start();

            // Boot ready.
            Debugger.DebugMessage(DebugMessageSource.Boot,
                "Boot took " + bootTime.ElapsedTime.AsMilliseconds() + " ms");
            bootTime.Dispose();

            // Load the starting scene, if any.
            if (startScene != null) SceneManager.LoadScene("startScene", startScene, true);

            // Define the timing clock.
            Clock timingClock = new Clock();

            // Start main loop.
            while (_nativeContext.Running)
            {
                // Get the time since the last frame time timer restart, which is the time it took for the last frame to render.
                _frameTime = timingClock.ElapsedTime.AsMilliseconds();

                // Restart the frame time timer.
                timingClock.Restart();

                // Tick events and update Raya.
                _nativeContext.Tick();

                // Start drawing.
                _nativeContext.StartDraw();

                // Update modules.
                Debugger.Update();
                SceneManager.Update();
                ScriptEngine.Update();

                // Finish drawing frame.
                _nativeContext.EndDraw();
            }
        }

        #region Drawing

        /// <summary>
        /// Draws a drawable Raya object.
        /// </summary>
        /// <param name="drawable"></param>
        public static void Draw(Drawable drawable)
        {
            if (!Started) Error.Raise(0, "SoulEngine's Core hasn't been started.", Severity.Critical);

            _nativeContext.Draw(drawable);
        }

        #endregion
    }
}
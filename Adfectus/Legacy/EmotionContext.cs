#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Adfectus.Common;
using Adfectus.Common.Configuration;
using Adfectus.Common.Hosting;
using Adfectus.Common.Threading;
using Adfectus.Graphics;
using Adfectus.Implementation.GLFW;
using Adfectus.Input;
using Adfectus.IO;
using Adfectus.Logging;
using Adfectus.Scenography;
using Adfectus.Sound;

#endregion

namespace Emotion.Engine
{
    public static class Context
    {
        #region Modules

        public static LoggingProvider Log { get => Adfectus.Common.Engine.Log; }
        public static AssetLoader AssetLoader { get => Adfectus.Common.Engine.AssetLoader; }
        public static Renderer Renderer { get => Adfectus.Common.Engine.Renderer; }
        public static IHost Host { get => Adfectus.Common.Engine.Host; }
        public static GraphicsManager GraphicsManager { get => Adfectus.Common.Engine.GraphicsManager; }
        public static IInputManager InputManager { get => Adfectus.Common.Engine.InputManager; }
        public static SceneManager SceneManager { get => Adfectus.Common.Engine.SceneManager; }
        public static ScriptingEngine ScriptingEngine { get => Adfectus.Common.Engine.ScriptingEngine; }
        public static SoundManager SoundManager { get => Adfectus.Common.Engine.SoundManager; }

        #endregion

        #region Config

        public static Flags Flags { get => Adfectus.Common.Engine.Flags; }

        #endregion

        #region Trackers

        public static bool IsSetup { get => Adfectus.Common.Engine.IsSetup; }
        public static bool IsRunning { get => Adfectus.Common.Engine.IsRunning; }
        public static bool IsUnfocused
        {
            get => Adfectus.Common.Engine.IsUnfocused;
        }
        public static float FrameTime { get => Adfectus.Common.Engine.FrameTime; }
        public static float RawFrameTime { get => (float) Adfectus.Common.Engine.RawFrameTime; }
        public static float TotalTime { get => Adfectus.Common.Engine.TotalTime; }

        #endregion

        public static void Setup(Action<object> config = null)
        {
            config?.Invoke(new object());
            Adfectus.Common.Engine.Setup();
        }

        public static void Run()
        {
            Adfectus.Common.Engine.Run();
        }

        public static void Quit()
        {
            Adfectus.Common.Engine.Quit();
        }
    }
}
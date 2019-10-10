#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Config;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Platform.Implementation
{
    public abstract class PlatformBase : IDisposable
    {
        /// <summary>
        /// Whether the platform is setup.
        /// </summary>
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Whether the platform's window is open, and considered active.
        /// </summary>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// The platform's window.
        /// </summary>
        public Window Window { get; protected set; }

        /// <summary>
        /// The platform's audio context. If any.
        /// </summary>
        public AudioContext Audio { get; protected set; }

        /// <summary>
        /// List of connected monitors.
        /// </summary>
        public List<Monitor> Monitors = new List<Monitor>();

        /// <summary>
        /// Called when a key is pressed, let go, or a held event is triggered.
        /// </summary>
        public EmotionEvent<Key, KeyStatus> OnKey = new EmotionEvent<Key, KeyStatus>();

        /// <summary>
        /// Called when a mouse key is pressed or let go.
        /// </summary>
        public EmotionEvent<MouseKey, KeyStatus> OnMouseKey = new EmotionEvent<MouseKey, KeyStatus>();

        /// <summary>
        /// Called when the mouse scrolls.
        /// </summary>
        public EmotionEvent<float> OnMouseScroll = new EmotionEvent<float>();

        /// <summary>
        /// Called when text input is detected. Most of the time this is identical to OnKey, but without the state.
        /// </summary>
        public EmotionEvent<char> OnTextInput = new EmotionEvent<char>();

        /// <summary>
        /// Returns the current mouse position. Is preprocessed by the Renderer to scale to the window if possible.
        /// </summary>
        public Vector2 MousePosition { get; protected set; } = Vector2.Zero;

        internal PlatformConfig Config;

        /// <summary>
        /// The sizes to switch between in debug mode by using ctrl + F1-F9
        /// </summary>
        private Vector2[] _windowSizes =
        {
            new Vector2(640, 360), // Lowest 16:9 good integer scaling potential
            new Vector2(960, 540), // Low 16:9
            new Vector2(1280, 720), // 16:9 720p HD
            new Vector2(1920, 1080), // 16:9 1080p FullHD

            new Vector2(640, 400), // Lowest 16:10
            new Vector2(768, 480), // Low 16:10
            new Vector2(1280, 800), // 16:10 HD
            new Vector2(1920, 1200), // 16:10 FullHD

            new Vector2(800, 600) // 4:3
        };

        #region Internal

        /// <summary>
        /// Setup the native platform and creates a window.
        /// </summary>
        /// <param name="conf">Optional configuration for the platform.</param>
        public void Setup(PlatformConfig conf = null)
        {
            // Check if default config, which should always be valid.
            // If not using the default config, check if it is valid.
            if (conf == null)
                Config = new PlatformConfig();
            else if (!conf.IsValidContext())
                return;
            else
                Config = conf;

            SetupPlatform();
            Window = CreateWindow();
            if (Window == null) return;

            // Bind this window and its context.
            // "There /can/ be only one."
            Window.Context.MakeCurrent();
            Gl.BindAPI(Window.Context.GetProcAddress);
            Gl.QueryContextVersion();

            Window.DisplayMode = Config.DisplayMode;

            // Show and focus.
            Window.WindowState = WindowState.Normal;

            // Attach default key behavior.
            OnKey.AddListener(DefaultButtonBehavior);

            IsSetup = true;
            IsOpen = true;
        }

        #endregion

        #region Implementation API

        protected abstract void SetupPlatform();
        protected abstract Window CreateWindow();

        /// <summary>
        /// Display an error message natively.
        /// Usually this means a popup will show up.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public abstract void DisplayMessageBox(string message);

        /// <summary>
        /// Update the platform. If this returns false then the platform/its window was closed.
        /// If the window is unfocused this blocks until a platform message is received.
        /// </summary>
        /// <returns>Whether the platform is alive.</returns>
        public abstract bool Update();

        #endregion

        #region Input API

        /// <summary>
        /// Returns whether the specified key is down.
        /// </summary>
        /// <param name="key">To key to check.</param>
        public abstract bool GetKeyDown(Key key);

        /// <summary>
        /// Returns whether the specified mouse key is down.
        /// </summary>
        /// <param name="key">To mouse key to check.</param>
        public abstract bool GetMouseKeyDown(MouseKey key);

        #endregion

        private bool DefaultButtonBehavior(Key key, KeyStatus state)
        {
            if (Engine.Configuration.DebugMode)
            {
                bool ctrl = GetKeyDown(Key.LeftControl) || GetKeyDown(Key.RightControl);
                if (key >= Key.F1 && key <= Key.F9 && state == KeyStatus.Down && ctrl && Window != null)
                {
                    Vector2 chosenSize = _windowSizes[key - Key.F1];
                    Window.Size = chosenSize;
                    Engine.Log.Info($"Set window size to {chosenSize}", MessageSource.Platform);
                }
            }

            bool alt = GetKeyDown(Key.LeftAlt) || GetKeyDown(Key.RightAlt);

            if (key == Key.Enter && state == KeyStatus.Down && alt && Window != null)
            {
                Window.DisplayMode = Window.DisplayMode == DisplayMode.Fullscreen ? DisplayMode.Windowed : DisplayMode.Fullscreen;
            }

            return true;
        }

        internal void UpdateMonitor(Monitor monitor, bool connected, bool first)
        {
            if (connected)
            {
                Engine.Log.Info($"Connected monitor {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                if (first)
                {
                    Monitors.Insert(0, monitor);

                    // Re-initiate fullscreen mode.
                    if (Window == null || Window.DisplayMode != DisplayMode.Fullscreen) return;
                    Window.DisplayMode = DisplayMode.Windowed;
                    Window.DisplayMode = DisplayMode.Fullscreen;
                }
                else
                {
                    Monitors.Add(monitor);
                }
            }
            else
            {
                Engine.Log.Info($"Disconnected monitor {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                // Exit fullscreen mode as it may have been fullscreen on this monitor.
                // This will cause a recenter on the primary monitor.
                if (Window != null && Window.DisplayMode == DisplayMode.Fullscreen)
                    Window.DisplayMode = DisplayMode.Windowed;

                Monitors.Remove(monitor);
            }
        }

        /// <summary>
        /// Dispose of the platform.
        /// </summary>
        public virtual void Dispose()
        {
            IsOpen = false;
            Window.Dispose();
        }
    }
}
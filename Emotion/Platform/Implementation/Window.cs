#region Using

using System;
using System.Numerics;
using Emotion.Utility;

#endregion

namespace Emotion.Platform.Implementation
{
    /// <summary>
    /// Whatever the representation of a window on the native platform is.
    /// </summary>
    public abstract class Window : IDisposable
    {
        /// <summary>
        /// The graphics context of the window.
        /// </summary>
        public GraphicsContext Context { get; protected set; }

        /// <summary>
        /// The state of the window on the screen.
        /// Whether it is maximized, minimized etc.
        /// Is set to "Normal" when the window is created.
        /// </summary>
        public abstract WindowState WindowState { get; set; }

        /// <summary>
        /// The window's display mode. Windowed, fullscreen, borderless fullscreen.
        /// Is originally set by the config.
        /// </summary>
        public abstract DisplayMode DisplayMode { get; set; }

        /// <summary>
        /// The position of the window on the screen.
        /// </summary>
        public abstract Vector2 Position { get; set; }

        /// <summary>
        /// The size of the window in pixels.
        /// </summary>
        public abstract Vector2 Size { get; set; }

        /// <summary>
        /// Whether the window is focused.
        /// </summary>
        public bool Focused
        {
            get => _focus;
            set
            {
                if (_focus == value) return;
                _focus = value;
                OnFocusChanged.Invoke(_focus);
            }
        }

        /// <summary>
        /// The platform which created this window.
        /// </summary>
        protected PlatformBase _platform;

        /// <summary>
        /// This event is called when the window's size changes.
        /// The input parameter is the new size.
        /// </summary>
        public EmotionEvent<Vector2> OnResize { get; protected set; } = new EmotionEvent<Vector2>();

        /// <summary>
        /// This event is called when the window's focus changes
        /// On some platforms the window cannot be unfocused - and this will never be called.
        /// The input parameter is the new focus state.
        /// </summary>
        public EmotionEvent<bool> OnFocusChanged { get; protected set; } = new EmotionEvent<bool>();

        private bool _focus = true;

        protected Window(PlatformBase platform)
        {
            _platform = platform;
        }

        #region Cleanup

        public virtual void Dispose()
        {
            // no-op
        }

        #endregion
    }
}
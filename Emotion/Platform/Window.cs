#region Using

using System;
using System.Numerics;
using Emotion.Utility;

#endregion

namespace Emotion.Platform
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
        /// The size of the window in pixels. The size must be an even number on both axes.
        /// </summary>
        public Vector2 Size
        {
            get => GetSize();
            set
            {
                if (DisplayMode != DisplayMode.Windowed) return;

                Vector2 val = value.Floor();
                if (val.X % 2 != 0) val.X++;
                if (val.Y % 2 != 0) val.Y++;
                SetSize(val);
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

        protected Window(PlatformBase platform)
        {
            _platform = platform;
        }

        internal abstract void UpdateDisplayMode();
        protected abstract Vector2 GetSize();
        protected abstract void SetSize(Vector2 size);

        #region Cleanup

        public virtual void Dispose()
        {
            // no-op
        }

        #endregion
    }
}
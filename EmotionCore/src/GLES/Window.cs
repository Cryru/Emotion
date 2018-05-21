// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using OpenTK;
using OpenTK.Graphics;

#endregion

namespace Emotion.GLES
{
    public sealed class Window : GameWindow
    {
        /// <summary>
        /// The context this window belongs to.
        /// </summary>
        internal Context EmotionContext;

        /// <summary>
        /// Create a new window.
        /// </summary>
        /// <param name="context">The Emotion context window belongs to.</param>
        internal Window(Context context) : base(960, 540, GraphicsMode.Default, "", GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 0,
#if DEBUG
            GraphicsContextFlags.Debug)
#else
            GraphicsContextFlags.ForwardCompatible)
#endif
        {
            EmotionContext = context;

            // Apply settings to properties.
            Title = EmotionContext.Settings.WindowTitle;
            Width = EmotionContext.Settings.WindowWidth;
            Height = EmotionContext.Settings.WindowHeight;
            Icon = EmotionContext.Settings.WindowIcon;

            // Setup window location.
            X = DisplayDevice.Default.Width / 2 - Width / 2;
            Y = DisplayDevice.Default.Height / 2 - Height / 2;
        }

        protected override void OnResize(EventArgs e)
        {
            EmotionContext.Renderer.SetViewport();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            EmotionContext.LoopUpdate((float) (e.Time * 1000));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            EmotionContext.LoopDraw();
        }

        /// <summary>
        /// Destroys the window, closing it and freeing resources.
        /// </summary>
        internal void Destroy()
        {
            Close();
        }
    }
}
// Emotion - https://github.com/Cryru/Emotion

#if GLES

#region Using

using System;
using OpenTK;

#endregion

namespace Emotion.Platform.GLES
{
    public sealed class GLWindow : GameWindow
    {
        #region Properties

        public IntPtr Pointer { get; set; }

        #endregion

        #region Declarations

        /// <summary>
        /// The context this object belongs to.
        /// </summary>
        internal GLContext EmotionContext;

        #endregion

        /// <summary>
        /// Create a new window.
        /// </summary>
        /// <param name="context">The context which will spawn the window.</param>
        internal GLWindow(GLContext context)
        {
            EmotionContext = context;

            // Apply settings to properties.
            Title = context.Settings.WindowTitle;
            Width = context.Settings.WindowWidth;
            Height = context.Settings.WindowHeight;
            //// Create the window within SDL.
            //Pointer = ErrorHandler.CheckError(SDL.SDL_CreateWindow(
            //    context.Settings.WindowTitle,
            //    SDL.SDL_WINDOWPOS_CENTERED,
            //    SDL.SDL_WINDOWPOS_CENTERED,
            //    context.Settings.WindowWidth,
            //    context.Settings.WindowHeight,
            //    SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS | SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS
            //));

            //// Get the window's surface.
            //Surface = ErrorHandler.CheckError(SDL.SDL_GetWindowSurface(Pointer));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            EmotionContext.LoopUpdate((float) (e.Time / 1000));
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
            //SDL.SDL_DestroyWindow(Pointer);
        }
    }
}

#endif
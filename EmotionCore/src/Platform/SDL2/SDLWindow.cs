// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Platform.SDL2.Base;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2
{
    public sealed class SDLWindow : INativeObject
    {
        #region Properties

        /// <summary>
        /// The window's title.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                SDL.SDL_SetWindowTitle(Pointer, value);
            }
        }

        private string _title;

        /// <summary>
        /// The size of the window.
        /// </summary>
        public Vector2 Size
        {
            get
            {
                SDL.SDL_GetWindowSize(Pointer, out int w, out int h);

                return new Vector2(w, h);
            }
            set => SDL.SDL_SetWindowSize(Pointer, (int) value.X, (int) value.Y);
        }

        #endregion

        #region Objects

        /// <summary>
        /// The SDL surface which the window displays.
        /// </summary>
        internal IntPtr Surface;

        /// <inheritdoc />
        public IntPtr Pointer { get; set; }

        /// <summary>
        /// The context this object belongs to.
        /// </summary>
        internal SDLContext EmotionContext;

        #endregion

        /// <summary>
        /// Create a new window.
        /// </summary>
        /// <param name="context">The context which will spawn the window.</param>
        internal SDLWindow(SDLContext context)
        {
            // Apply settings to properties.
            EmotionContext = context;
            _title = context.Settings.WindowTitle;

            // Create the window within SDL.
            Pointer = ErrorHandler.CheckError(SDL.SDL_CreateWindow(
                context.Settings.WindowTitle,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                context.Settings.WindowWidth,
                context.Settings.WindowHeight,
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS | SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS
            ));

            // Get the window's surface.
            Surface = ErrorHandler.CheckError(SDL.SDL_GetWindowSurface(Pointer));
        }

        /// <summary>
        /// Destroys the window, closing it and freeing resources.
        /// </summary>
        internal void Destroy()
        {
            SDL.SDL_DestroyWindow(Pointer);
        }
    }
}

#endif
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using Emotion.Modules;
using SDL2;

#endregion

namespace Emotion.Objects
{
    public class Renderer
    {
        internal Context Context;
        internal IntPtr Pointer;
        internal IntPtr GLContext;
        internal Camera Camera;

        internal Renderer(Context context)
        {
            Context = context;

            // Create a renderer.
            Pointer = ExternalErrorHandler.CheckError(SDL.SDL_CreateRenderer(Context.Window.Pointer, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));

            // Create an OpenGL Context.
            GLContext = ExternalErrorHandler.CheckError(SDL.SDL_GL_CreateContext(Context.Window.Pointer));
        }

        #region Primary Functions

        /// <summary>
        /// Clears everything drawn on the screen.
        /// </summary>
        public void Clear()
        {
            SDL.SDL_RenderClear(Pointer);
        }

        /// <summary>
        /// Displays everything drawn.
        /// </summary>
        public void Present()
        {
            SDL.SDL_RenderPresent(Pointer);
        }

        #endregion

        #region Drawing Functions

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        public void DrawTexture(Texture texture, Rectangle location, Rectangle source)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = location.X, y = location.Y, h = location.Height, w = location.Width};
            SDL.SDL_Rect src = new SDL.SDL_Rect {x = source.X, y = source.Y, h = source.Height, w = source.Width};

            // Add camera.
            if (Camera != null)
            {
                des.x -= Camera.Bounds.X;
                des.y -= Camera.Bounds.Y;
            }

            ExternalErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, texture.Pointer, ref src, ref des), true);
        }

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        public void DrawTexture(Texture texture, Rectangle location)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = location.X, y = location.Y, h = location.Height, w = location.Width};

            // Add camera.
            if (Camera != null)
            {
                des.x -= Camera.Bounds.X;
                des.y -= Camera.Bounds.Y;
            }

            ExternalErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, texture.Pointer, IntPtr.Zero, ref des), true);
        }

        /// <summary>
        /// Draws a rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        public void DrawRectangle(Rectangle rect)
        {
            SDL.SDL_Rect re = new SDL.SDL_Rect {x = rect.X, y = rect.Y, h = rect.Height, w = rect.Width};

            SDL.SDL_RenderDrawRect(Pointer, ref re);
        }

        #endregion

        #region Camera

        /// <summary>
        /// Sets the renderer's camera.
        /// </summary>
        /// <param name="camera">The camera to bind to the renderer.</param>
        public void SetCamera(Camera camera)
        {
            Camera = camera;
        }

        #endregion
    }
}
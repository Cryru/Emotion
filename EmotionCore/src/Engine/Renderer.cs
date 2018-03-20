// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using Emotion.Assets;
using Emotion.Objects.Game;
using Emotion.Systems;
using SDL2;

#endregion

namespace Emotion.Engine
{
    public class Renderer
    {
        #region Properties

        /// <summary>
        /// The resolution to render at.
        /// </summary>
        public Point RenderSize { get; private set; }

        #endregion

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

            // Set the render size.
            ExternalErrorHandler.CheckError(SDL.SDL_RenderSetLogicalSize(Pointer, context.InitialSettings.RenderWidth, context.InitialSettings.RenderHeight));
            RenderSize = new Point(context.InitialSettings.RenderWidth, context.InitialSettings.RenderHeight);
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

        public void SetScale(float scale)
        {
            SDL.SDL_RenderSetScale(Pointer, scale, scale);
        }

        #endregion

        #region Drawing Functions

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="camera">Whether to draw through the current camera, or on the screen.</param>
        public void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = location.X, y = location.Y, h = location.Height, w = location.Width};
            SDL.SDL_Rect src = new SDL.SDL_Rect {x = source.X, y = source.Y, h = source.Height, w = source.Width};

            // Add camera.
            if (Camera != null && camera)
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
        /// <param name="camera">Whether to draw through the current camera, or on the screen.</param>
        public void DrawTexture(Texture texture, Rectangle location, bool camera = true)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = location.X, y = location.Y, h = location.Height, w = location.Width};

            // Add camera.
            if (Camera != null && camera)
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
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">Whether to draw through the current camera, or on the screen.</param>
        public void DrawRectangle(Rectangle rect, Color color, bool camera = true)
        {
            // Set color.
            SDL.SDL_SetRenderDrawColor(Pointer, color.R, color.G, color.B, color.A);

            // Transform rect to sdl rect.
            SDL.SDL_Rect re = new SDL.SDL_Rect {x = rect.X, y = rect.Y, h = rect.Height, w = rect.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                re.x -= Camera.Bounds.X;
                re.y -= Camera.Bounds.Y;
            }

            SDL.SDL_RenderDrawRect(Pointer, ref re);

            // Return original black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
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
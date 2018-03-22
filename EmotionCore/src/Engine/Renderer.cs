// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Engine.Assets;
using Emotion.Objects.Game;
using Emotion.Primitives;
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
        public Vector2 RenderSize { get; private set; }

        #endregion

        internal Context Context;
        internal IntPtr Pointer;
        internal IntPtr GLContext;
        internal Camera Camera;

        internal Renderer(Context context)
        {
            Context = context;

            // Create a renderer.
            Pointer = SDLErrorHandler.CheckError(SDL.SDL_CreateRenderer(Context.Window.Pointer, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));

            // Create an OpenGL Context.
            GLContext = SDLErrorHandler.CheckError(SDL.SDL_GL_CreateContext(Context.Window.Pointer));

            // Set the render size.
            SDLErrorHandler.CheckError(SDL.SDL_RenderSetLogicalSize(Pointer, context.InitialSettings.RenderWidth, context.InitialSettings.RenderHeight));
            RenderSize = new Vector2(context.InitialSettings.RenderWidth, context.InitialSettings.RenderHeight);
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
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = (int) location.X, y = (int) location.Y, h = (int) location.Height, w = (int) location.Width};
            SDL.SDL_Rect src = new SDL.SDL_Rect {x = (int) source.X, y = (int) source.Y, h =(int)  source.Height, w = (int) source.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                des.x -= (int) Camera.Bounds.X;
                des.y -= (int) Camera.Bounds.Y;
            }

            SDLErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, texture.Pointer, ref src, ref des), true);
        }

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="camera">Whether to draw through the current camera, or on the screen.</param>
        public void DrawTexture(Texture texture, Rectangle location, bool camera = true)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = (int) location.X, y = (int) location.Y, h = (int) location.Height, w =(int) location.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                des.x -= (int) Camera.Bounds.X;
                des.y -= (int) Camera.Bounds.Y;
            }

            SDLErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, texture.Pointer, IntPtr.Zero, ref des), true);
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
            SDL.SDL_Rect re = new SDL.SDL_Rect {x = (int) rect.X, y = (int) rect.Y, h = (int) rect.Height, w = (int) rect.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                re.x -= (int) Camera.Bounds.X;
                re.y -= (int) Camera.Bounds.Y;
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

#endif
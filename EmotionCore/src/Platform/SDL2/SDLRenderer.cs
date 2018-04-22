// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.Base.Objects;
using Emotion.Platform.SDL2.Assets;
using Emotion.Platform.SDL2.Base;
using Emotion.Platform.SDL2.Objects;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2
{
    /// <inheritdoc cref="Renderer" />
    public sealed class SDLRenderer : Renderer, INativeObject
    {
        #region Properties

        public override Vector2 RenderSize { get; protected set; }

        public override CameraBase Camera { get; set; }

        /// <inheritdoc />
        public IntPtr Pointer { get; set; }

        internal IntPtr GLContext;

        #endregion

        internal SDLRenderer(SDLContext context)
        {
            // Create a renderer.
            Pointer = ErrorHandler.CheckError(SDL.SDL_CreateRenderer(context.Window.Pointer, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));

            // Create an OpenGL Context.
            GLContext = ErrorHandler.CheckError(SDL.SDL_GL_CreateContext(context.Window.Pointer));

            // Set the render size.
            ErrorHandler.CheckError(SDL.SDL_RenderSetLogicalSize(Pointer, context.Settings.RenderWidth, context.Settings.RenderHeight));

            RenderSize = new Vector2(context.Settings.RenderWidth, context.Settings.RenderHeight);

            // Set certain SDL properties.
            ErrorHandler.CheckError(SDL.SDL_SetRenderDrawBlendMode(Pointer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND));
        }

        internal void Destroy()
        {
            SDL.SDL_DestroyRenderer(Pointer);
        }

        #region Primary Functions

        public override void Clear(Color color)
        {
            // Set color.
            SDL.SDL_SetRenderDrawColor(Pointer, color.R, color.G, color.B, color.A);

            SDL.SDL_RenderClear(Pointer);

            // Return default black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        public override void Present()
        {
            SDL.SDL_RenderPresent(Pointer);
        }

        public void SetScale(float scale)
        {
            SDL.SDL_RenderSetScale(Pointer, scale, scale);
        }

        #endregion

        #region Texture Functions

        public override void DrawTexture(Texture texture, Rectangle location, Rectangle source, float opacity, bool camera = true)
        {
            SDLTexture sdlTexture = (SDLTexture) texture;

            // Set the opacity to the requested one.
            sdlTexture.SetAlpha((byte) (opacity * 255));

            // Draw the texture.
            DrawTexture(texture, location, source, camera);

            // Revert opacity to maximum.
            sdlTexture.SetAlpha(255);
        }

        public override void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = (int) location.X, y = (int) location.Y, h = (int) location.Height, w = (int) location.Width};
            SDL.SDL_Rect src = new SDL.SDL_Rect {x = (int) source.X, y = (int) source.Y, h = (int) source.Height, w = (int) source.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                des.x -= (int) Camera.Bounds.X;
                des.y -= (int) Camera.Bounds.Y;
            }

            ErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, ((SDLTexture) texture).Pointer, ref src, ref des), true);
        }

        public override void DrawTexture(Texture texture, Rectangle location, bool camera = true)
        {
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = (int) location.X, y = (int) location.Y, h = (int) location.Height, w = (int) location.Width};

            // Add camera.
            if (Camera != null && camera)
            {
                des.x -= (int) Camera.Bounds.X;
                des.y -= (int) Camera.Bounds.Y;
            }

            ErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, ((SDLTexture) texture).Pointer, IntPtr.Zero, ref des), true);
        }

        #endregion

        #region Primitive Drawing

        public override void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true)
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

            // Return default black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        public override void DrawRectangle(Rectangle rect, Color color, bool camera = true)
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

            SDL.SDL_RenderFillRect(Pointer, ref re);

            // Return original black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        public override void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true)
        {
            // Set color.
            SDL.SDL_SetRenderDrawColor(Pointer, color.R, color.G, color.B, color.A);

            // Add camera.
            if (Camera != null && camera)
            {
                start.X -= (int) Camera.Bounds.X;
                end.X -= (int) Camera.Bounds.X;
                start.Y -= (int) Camera.Bounds.Y;
                end.Y -= (int) Camera.Bounds.Y;
            }

            // Draw the line.
            SDL.SDL_RenderDrawLine(Pointer, (int) start.X, (int) start.Y, (int) end.X, (int) end.Y);

            // Return original black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        #endregion

        #region Text Drawing

        public override TextDrawingSession StartTextSession(Font font, int fontSize, int width, int height)
        {
            // Get a pointer to the font at the specified size.
            SDLTextDrawingSession session = new SDLTextDrawingSession
            {
                Font = ((SDLFont) font).GetSize(fontSize),
                Size = new Vector2(width, height),
                Renderer = this
            };
            session.FontAscent = SDLTtf.TTF_FontAscent(session.Font);
            session.Reset();

            return session;
        }

        public override void DrawText(Font font, int size, string text, Color color, Vector2 location, bool camera = true)
        {
            // Convert color to platform color.
            SDL.SDL_Color platformColor = new SDL.SDL_Color
            {
                r = color.R,
                g = color.G,
                b = color.B
            };

            // Add camera.
            if (Camera != null && camera)
            {
                location.X -= (int) Camera.Bounds.X;
                location.Y -= (int) Camera.Bounds.Y;
            }

            // Get a pointer to the font at the specified size.
            IntPtr fontPointer = ((SDLFont) font).GetSize(size);

            // Render the text to a surface, then copy it to a texture, and free the surface.
            IntPtr messageSurface = SDLTtf.TTF_RenderText_Solid(fontPointer, text, platformColor);
            IntPtr messageTexture = SDL.SDL_CreateTextureFromSurface(Pointer, messageSurface);
            SDL.SDL_FreeSurface(messageSurface);

            // Get the size of the text.
            Vector2 calcSize = font.MeasureString(text, size);

            // Determine the destination rectangle of the text.
            SDL.SDL_Rect des = new SDL.SDL_Rect {x = (int) location.X, y = (int) location.Y, h = (int) calcSize.Y, w = (int) calcSize.X};

            // Render the text texture.
            ErrorHandler.CheckError(SDL.SDL_RenderCopy(Pointer, messageTexture, IntPtr.Zero, ref des), true);

            // Cleanup.
            SDL.SDL_DestroyTexture(messageTexture);
        }

        public override void DrawText(Font font, int size, string[] text, Color color, Vector2 location, bool camera = true)
        {
            int lineSpacing = font.LineSpacing(size);

            // Render each line.
            for (int i = 0; i < text.Length; i++)
            {
                location.Y += lineSpacing * i;

                DrawText(font, size, text[i], color, location, camera);
            }
        }

        #endregion

        #region RenderTarget Functions

        /// <summary>
        /// Starts rendering on the specified render target, or null to render to the window.
        /// </summary>
        /// <param name="target">The render target to render to, or null to render to the window.</param>
        public void SetRenderTarget(RenderTarget target)
        {
            SDL.SDL_SetRenderTarget(Pointer, target?.Pointer ?? IntPtr.Zero);
        }

        #endregion
    }
}

#endif
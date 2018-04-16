// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.SDL2.Assets;
using Emotion.Platform.SDL2.Base;
using Emotion.Platform.SDL2.Objects;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2
{
    /// <inheritdoc cref="IRenderer" />
    public sealed class SDLRenderer : NativeObject, IRenderer
    {
        #region Properties

        /// <summary>
        /// The resolution to render at.
        /// </summary>
        public Vector2 RenderSize { get; private set; }

        #endregion

        #region Declarations

        internal IntPtr GLContext;
        internal CameraBase Camera;

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
        }

        internal void Destroy()
        {
            SDL.SDL_DestroyRenderer(Pointer);
        }

        #region Primary Functions

        public void Clear(Color color)
        {
            // Set color.
            SDL.SDL_SetRenderDrawColor(Pointer, color.R, color.G, color.B, color.A);

            SDL.SDL_RenderClear(Pointer);

            // Return default black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        public void Present()
        {
            SDL.SDL_RenderPresent(Pointer);
        }

        public void SetScale(float scale)
        {
            SDL.SDL_RenderSetScale(Pointer, scale, scale);
        }

        #endregion

        #region Texture Functions

        public void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true)
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

        public void DrawTexture(Texture texture, Rectangle location, bool camera = true)
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

        public void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true)
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

            SDL.SDL_RenderFillRect(Pointer, ref re);

            // Return original black.
            SDL.SDL_SetRenderDrawColor(Pointer, 0, 0, 0, 255);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true)
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

        /// <summary>
        /// Begin a text rendering session.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="width">The width of the final resulting texture.</param>
        /// <param name="height">The height of the final resulting texture.</param>
        /// <returns>A text drawing session.</returns>
        public TextDrawingSession TextSessionStart(SDLFont font, int size, int width, int height)
        {
            // Get a pointer to the font at the specified size.
            TextDrawingSession session = new TextDrawingSession
            {
                Font = font.GetSize(size),
                Surface = SDL.SDL_CreateRGBSurface(0, width, height, 32, 0xff000000, 0x00ff0000, 0x0000ff00, 0x000000ff),
                Cache = new SDLTexture(width, height)
            };
            session.FontAscent = SDLTtf.TTF_FontAscent(session.Font);

            return session;
        }

        /// <summary>
        /// Add a glyph to a text rendering session. The glyph is automatically positioned to be after the previous one.
        /// </summary>
        /// <param name="session">The text rendering session to add a glyph to.</param>
        /// <param name="glyphChar">The glyph to add.</param>
        /// <param name="color">The glyph color.</param>
        /// <param name="xOffset">The x offset to render the glyph at.</param>
        /// <param name="yOffset">The y offset to render the glyph at.</param>
        public void TextSessionAddGlyph(TextDrawingSession session, char glyphChar, Color color, int xOffset = 0, int yOffset = 0)
        {
            if (session == null || session.Finalized) throw new Exception("Cannot add a glyph to a non-existent or finalized state.");

            // Convert color to platform color.
            SDL.SDL_Color platformColor = new SDL.SDL_Color
            {
                r = color.R,
                g = color.G,
                b = color.B
            };

            IntPtr glyph = ErrorHandler.CheckError(SDLTtf.TTF_RenderGlyph_Solid(session.Font, glyphChar, platformColor));

            // Get glyph data.
            ErrorHandler.CheckError(SDLTtf.TTF_GlyphMetrics(session.Font, glyphChar, out int minX, out int _, out int _, out int _, out int advance));

            SDL.SDL_Rect des = new SDL.SDL_Rect {x = xOffset + session.XOffset + minX, y = yOffset + session.YOffset, w = 0, h = 0};
            session.XOffset += advance;
            ErrorHandler.CheckError(SDL.SDL_BlitSurface(glyph, IntPtr.Zero, session.Surface, ref des));
            SDL.SDL_FreeSurface(glyph);
        }

        /// <summary>
        /// Add a new line to the text rendering session.
        /// </summary>
        /// <param name="session">The text rendering session to add a new line to.</param>
        public void TextSessionNewLine(TextDrawingSession session)
        {
            if (session == null || session.Finalized) throw new Exception("Cannot add a glyph to a non-existent or finalized state.");

            session.XOffset = 0;
            session.YOffset += SDLTtf.TTF_FontLineSkip(session.Font);
        }

        /// <summary>
        /// End a text drawing session, and return the result as a texture. Any old results are automatically freed.
        /// </summary>
        /// <param name="session">The text drawing session to return the result of.</param>
        /// <param name="finalize">
        /// Whether to permanently end the text drawing session. If false then the session can be reused,
        /// more glyphs can be added and a result can be received again.
        /// </param>
        /// <returns>A texture result of the text drawing session.</returns>
        public SDLTexture TextSessionEnd(TextDrawingSession session, bool finalize)
        {
            if (session == null || session.Finalized) throw new Exception("Cannot add a glyph to a non-existent or finalized state.");

            // Get the resulting texture from the session.
            SDLTexture resultTexture = session.Cache;

            // Check if an old texture is loaded into the session.
            if (resultTexture.Pointer != IntPtr.Zero) resultTexture.Destroy();

            // Set it up.
            resultTexture.Pointer = ErrorHandler.CheckError(SDL.SDL_CreateTextureFromSurface(Pointer, session.Surface));

            // Check if finalizing session.
            if (finalize)
            {
                SDL.SDL_FreeSurface(session.Surface);
                session.Cache = null;
                session.Font = IntPtr.Zero;
                session.Surface = IntPtr.Zero;
                session.Finalized = true;
            }

            // Return it.
            return resultTexture;
        }

        public void DrawText(Font font, string text, Color color, Vector2 location, int size, bool camera = true)
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

        public void DrawText(Font font, string[] text, Color color, Vector2 location, int size, bool camera = true)
        {
            int lineSpacing = font.LineSpacing(size);

            // Render each line.
            for (int i = 0; i < text.Length; i++)
            {
                location.Y += lineSpacing * i;

                DrawText(font, text[i], color, location, size, camera);
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

        #region Camera

        public void SetCamera(CameraBase camera)
        {
            Camera = camera;
        }

        public CameraBase GetCamera()
        {
            return Camera;
        }

        #endregion
    }
}

#endif
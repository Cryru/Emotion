// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.GLES.Text;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK;
using OpenTK.Graphics.ES30;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.GLES
{
    /// <summary>
    /// Handles rendering.
    /// </summary>
    public class Renderer : ContextObject
    {
        #region Properties

        /// <summary>
        /// The resolution to render at.
        /// </summary>
        public Vector2 RenderSize { get; protected set; }

        /// <summary>
        /// The center point of the screen.
        /// </summary>
        public Vector2 ScreenCenter
        {
            get => new Rectangle(0, 0, RenderSize.X, RenderSize.Y).Center;
        }

        /// <summary>
        /// The renderer's camera.
        /// </summary>
        public CameraBase Camera { get; set; }

        #endregion

        #region Drawing State

        /// <summary>
        /// The drawing area vertex.
        /// </summary>
        private VBO _drawArea;

        /// <summary>
        /// The running GL program.
        /// </summary>
        private GlProgram _currentProgram;

        /// <summary>
        /// The VBO used to store all render vertices.
        /// </summary>
        private VBO _allVBO;

        /// <summary>
        /// The VBO used to store texture UV.
        /// </summary>
        private VBO _allTextureVBO;

        /// <summary>
        /// A blank 1x1 white texture.
        /// </summary>
        private Texture _blankTexture;

        #endregion

        internal Renderer(Context context) : base(context)
        {
            RenderSize = new Vector2(Context.Settings.RenderWidth, Context.Settings.RenderHeight);

            // Get the default shaders.
            string defaultVertex = Helpers.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultVertex.glsl");
            string defaultFrag = Helpers.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultFrag.glsl");
            Shader vert = new Shader(ShaderType.VertexShader, defaultVertex);
            Shader frag = new Shader(ShaderType.FragmentShader, defaultFrag);

            // Create a program and link the shaders to it.
            _currentProgram = new GlProgram();
            _currentProgram.AttachShader(vert);
            _currentProgram.AttachShader(frag);
            _currentProgram.Link();
            _currentProgram.Use();

            // Add default uniforms.
            _currentProgram.AddUniformVariable("color");
            _currentProgram.AddUniformVariable("projectionMatrix");
            _currentProgram.AddUniformVariable("textureMatrix");

            _currentProgram.SetUniformData("color", Color.White);
            _currentProgram.SetUniformData("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, 0, 1));
            _currentProgram.SetUniformData("textureMatrix", Matrix4.Identity);

            // Cleanup shaders.
            vert.Destroy();
            frag.Destroy();

            // Create an empty VAO. Newer OpenGL requires this.
            VAO mainVao = new VAO();
            mainVao.Use();

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            // Setup the draw area cleaner.
            _drawArea = new VBO();
            _drawArea.Upload(RectangleToVertices(new Rectangle(0, 0, RenderSize.X, RenderSize.Y)));

            // Setup additional drawing parameters.
            _allVBO = new VBO();
            _allTextureVBO = new VBO();

            _blankTexture = new Texture();
            _blankTexture.Process(new byte[]
            {
                66, 77, 58, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0, 40, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 196, 14, 0, 0, 196, 14, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255,
                255, 255
            });
            _blankTexture.Use();

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Check for any GL errors.
            Helpers.CheckError("additional renderer setup");
        }

        #region Helpers

        private OpenTK.Vector2[] RectangleToVertices(Rectangle rect)
        {
            return new[]
            {
                new OpenTK.Vector2(rect.X, rect.Y),
                new OpenTK.Vector2(rect.X + rect.Width, rect.Y),
                new OpenTK.Vector2(rect.X + rect.Width, rect.Y + rect.Height),
                new OpenTK.Vector2(rect.X, rect.Y + rect.Height)
            };
        }

        #endregion

        #region Internal API

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        internal void Clear()
        {
            ClearTarget();
            DrawVBO(_drawArea, Context.Settings.ClearColor);
        }

        /// <summary>
        /// Swaps window buffers, displaying everything rendered to the window.
        /// </summary>
        internal void Present()
        {
            Context.Window.SwapBuffers();
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        internal void Destroy()
        {
            _currentProgram.Destroy();
            _drawArea.Destroy();
        }

        /// <summary>
        /// Draws a VBO.
        /// </summary>
        /// <param name="vertVBO">The VBO to draw.</param>
        /// <param name="color">The color to draw it in.</param>
        /// <param name="textureVBO">The vertices of the texture, if any.</param>
        /// <param name="primitiveType">The drawing technique.</param>
        internal void DrawVBO(VBO vertVBO, Color color, VBO textureVBO = null, PrimitiveType primitiveType = PrimitiveType.TriangleFan)
        {
            // Set the color.
            _currentProgram.SetUniformData("color", color);

            // Bind the VBO.
            vertVBO.Use();

            // Setup vertex as an attribute.
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, OpenTK.Vector2.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);

            if (textureVBO != null)
            {
                textureVBO.Use();

                // Setup UV as an attribute.
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, OpenTK.Vector2.SizeInBytes, 0);
                GL.EnableVertexAttribArray(1);
            }

            // Draw.
            GL.DrawArrays(primitiveType, 0, vertVBO.UploadedLength);
        }

        /// <summary>
        /// Recalculates and resets the window's pillar and letter boxing.
        /// </summary>
        internal void SetViewport()
        {
            // Calculate borderbox / pillarbox.
            float targetAspectRatio = RenderSize.X / RenderSize.Y;

            float width = Context.Window.ClientSize.Width;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > Context.Window.ClientSize.Height)
            {
                height = Context.Window.ClientSize.Height;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            int vpX = (int) (Context.Window.ClientSize.Width / 2 - width / 2);
            int vpY = (int) (Context.Window.ClientSize.Height / 2 - height / 2);

            // Set viewport.
            GL.Viewport(vpX, vpY, (int) width, (int) height);
            GL.Scissor(vpX, vpY, (int) width, (int) height);
        }

        #endregion

        #region API

        /// <summary>
        /// Draw on the specified render target.
        /// </summary>
        /// <param name="target">The render target to draw on.</param>
        public void DrawOn(RenderTarget target)
        {
            // Bind the framebuffer.
            target.UseBuffer();

            // Set the projection matrix to the buffer size.
            _currentProgram.SetUniformData("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, target.Width, target.Height, 0, 0, 1));

            // Modify the viewport.
            GL.Viewport(0, 0, target.Width, target.Height);

            // Change blend function.
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstAlpha);
        }

        /// <summary>
        /// Draw on the screen.
        /// </summary>
        public void DrawOnScreen()
        {
            // Unbind the frame buffer.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Restore projection matrix.
            _currentProgram.SetUniformData("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, 0, 1));

            // Restore viewport.
            SetViewport();

            // Restore blend function.
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Clear the current render target.
        /// </summary>
        public void ClearTarget()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        #endregion

        #region Primitive Drawing

        /// <summary>
        /// Draws a rectangle outline on the current render target, which by default is the window.
        /// </summary>
        /// <param name="rect">The rectangle to outline.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true)
        {
            _currentProgram.SetUniformData("color", color);

            if (camera && Camera != null)
            {
                rect.X -= Camera.Bounds.X;
                rect.Y -= Camera.Bounds.Y;
            }

            _allVBO.Upload(RectangleToVertices(rect));
            DrawVBO(_allVBO, color, null, PrimitiveType.LineLoop);
        }

        /// <summary>
        /// Draws a filled rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawRectangle(Rectangle rect, Color color, bool camera = true)
        {
            _currentProgram.SetUniformData("color", color);

            if (camera && Camera != null)
            {
                rect.X -= Camera.Bounds.X;
                rect.Y -= Camera.Bounds.Y;
            }

            _allVBO.Upload(RectangleToVertices(rect));
            DrawVBO(_allVBO, color);
        }

        /// <summary>
        /// Draws a line on the current render target, which by default is the window.
        /// </summary>
        /// <param name="start">The line's starting point.</param>
        /// <param name="end">The line's ending point.</param>
        /// <param name="color">The line's color.</param>
        /// <param name="camera">
        /// Whether the line's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true)
        {
            _currentProgram.SetUniformData("color", color);

            if (camera && Camera != null)
            {
                start.X -= Camera.Bounds.X;
                start.Y -= Camera.Bounds.Y;
                end.X -= Camera.Bounds.X;
                end.Y -= Camera.Bounds.Y;
            }

            OpenTK.Vector2[] vertices =
            {
                new OpenTK.Vector2(start.X, start.Y),
                new OpenTK.Vector2(end.X, end.Y)
            };

            _allVBO.Upload(vertices);
            DrawVBO(_allVBO, color, null, PrimitiveType.Lines);
        }

        #endregion

        #region Texture Drawing

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawTexture(ITexture texture, Rectangle location, bool camera = true)
        {
            DrawTexture(texture, location, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, camera);
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawTexture(ITexture texture, Rectangle location, Rectangle source, bool camera = true)
        {
            DrawTexture(texture, location, source, Color.White, camera);
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="color">Tints the texture in the provided color. Opacity can be controlled through the color alpha.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawTexture(ITexture texture, Rectangle location, Color color, bool camera = true)
        {
            DrawTexture(texture, location, new Rectangle(0, 0, texture.Width, texture.Height), color, camera);
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="color">Tints the texture in the provided color. Opacity can be controlled through the color alpha.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public void DrawTexture(ITexture texture, Rectangle location, Rectangle source, Color color, bool camera = true)
        {
            // Set as first texture.
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture.
            texture.Use();

            // Upload the texture UI to the VBO. If a source is not specify take the whole texture.
            _allTextureVBO.Upload(RectangleToVertices(source));

            // Set the texture matrix.
            _currentProgram.SetUniformData("textureMatrix", texture.TextureMatrix);

            if (camera && Camera != null)
            {
                location.X -= Camera.Bounds.X;
                location.Y -= Camera.Bounds.Y;
            }

            // Setup the VBO.
            _allVBO.Upload(RectangleToVertices(location));
            DrawVBO(_allVBO, color, _allTextureVBO);

            // Unset the texture.
            _blankTexture.Use();

            Helpers.CheckError("drawing texture");
        }

        #endregion

        #region Text Drawing

        /// <summary>
        /// Begin a text rendering session.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="fontSize">The font size to use.</param>
        /// <param name="width">The width of the final resulting texture.</param>
        /// <param name="height">The height of the final resulting texture.</param>
        /// <returns>A text drawing session.</returns>
        public TextDrawingSession StartTextSession(Font font, uint fontSize, int width, int height)
        {
            TextDrawingSession session = new TextDrawingSession
            {
                Font = font,
                FontSize = fontSize,
                Size = new Vector2(width, height),
                Renderer = this
            };
            session.Reset();

            return session;
        }

        /// <summary>
        /// Draws text on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        public void DrawText(Font font, uint size, string text, Color color, Vector2 location)
        {
            DrawText(font, size, text.Split('\n'), color, location);
        }

        /// <summary>
        /// Draws a text array with line spacing, on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="text">The text array to render, each item will be on a separate line.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        public void DrawText(Font font, uint size, string[] text, Color color, Vector2 location)
        {
            // Create a dummy session.
            TextDrawingSession session = new TextDrawingSession
            {
                Font = font,
                FontSize = size,
                Renderer = this,
                UseCache = false,
                MasterOffset = location
            };
            session.Reset();

            // Add glyphs.
            foreach (string line in text)
            {
                foreach (char c in line)
                {
                    session.AddGlyph(c, color);
                }

                session.NewLine();
            }

            // Cleanup.
            session.Destroy();
        }

        #endregion
    }
}
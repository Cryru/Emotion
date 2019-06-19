#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Game.Camera;
using Adfectus.Graphics.Text;
using Adfectus.Input;
using Adfectus.Logging;
using Adfectus.Primitives;
using Adfectus.Utility;

#endregion

namespace Adfectus.Graphics
{
    /// <summary>
    /// The object which takes care of rendering and managing GL.
    /// </summary>
    public sealed class Renderer
    {
        #region Render State

        /// <summary>
        /// The scale of the host resolution to the actual render resolution.
        /// </summary>
        public Vector2 HostScale { get; private set; }

        /// <summary>
        /// The host resolutions margins to the actual render resolution.
        /// The margins are the black bars in letterbox/pillarbox mode.
        /// </summary>
        public Vector2 HostMargins { get; private set; }

        /// <summary>
        /// The near clipping plane when in 2d mode.
        /// </summary>
        public float Near2D { get; set; } = -100;

        /// <summary>
        /// The far clipping plane when in 2d mode.
        /// </summary>
        public float Far2D { get; set; } = 100;

        /// <summary>
        /// The renderer's camera.
        /// </summary>
        public CameraBase Camera
        {
            get => _camera;
            set
            {
                _camera = value;
                _camera.Update();
                UpdateCameraMatrix();
            }
        }

        /// <summary>
        /// Whether the view matrix is enabled.
        /// </summary>
        public bool ViewMatrixEnabled
        {
            get => _viewMatrixEnabled;
            set
            {
                Submit();

                _viewMatrixEnabled = value;
                SyncViewMatrix();
            }
        }

        private bool _viewMatrixEnabled = true;

        /// <summary>
        /// The current render target.
        /// </summary>
        public RenderTarget CurrentTarget
        {
            get => _targetStack.Peek();
        }

        /// <summary>
        /// The base render target - the one on the very bottom.
        /// </summary>
        public RenderTarget BaseTarget { get; private set; }

        /// <summary>
        /// The current model matrix.
        /// </summary>
        public Matrix4x4 ModelMatrix
        {
            get => _modelMatrix.CurrentMatrix;
        }

        /// <summary>
        /// Private camera tracker.
        /// </summary>
        private CameraBase _camera;

        /// <summary>
        /// The main drawing buffer.
        /// </summary>
        private StreamBuffer _mainBuffer;

        private StreamBuffer _verticesStreamBuffer;

        /// <summary>
        /// The model matrix stack.
        /// </summary>
        private TransformationStack _modelMatrix;

        /// <summary>
        /// A stack of render targets. The one on top is the one rendering is done to.
        /// </summary>
        private Stack<RenderTarget> _targetStack;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new renderer. Is called by the Engine when initializing modules.
        /// </summary>
        internal Renderer(EngineBuilder builder)
        {
            // Create default render target.
            _targetStack = new Stack<RenderTarget>();
            RenderTarget defaultTarget = Engine.GraphicsManager.CreateRenderTarget(builder.RenderSize, false, TextureInternalFormat.Rgb, TexturePixelFormat.Rgb, true);
            _targetStack.Push(defaultTarget);
            BaseTarget = defaultTarget;

            // Create objects.
            Camera = new CameraBase(new Vector3(0, 0, 0), CurrentTarget.Size);
            _modelMatrix = new TransformationStack();

            // Setup main map buffer.
            _mainBuffer = Engine.GraphicsManager.CreateQuadStreamBuffer(Engine.Flags.RenderFlags.MaxRenderable);
            _verticesStreamBuffer = Engine.GraphicsManager.CreateStreamBuffer(1, Engine.Flags.RenderFlags.MaxRenderable, 0, 1, true);
        }

        /// <summary>
        /// Destroy the renderer.
        /// </summary>
        internal void Destroy()
        {
            _mainBuffer.Delete();
        }

        #endregion

        #region Loop API

        /// <summary>
        /// Clear the screen.
        /// </summary>
        internal void Clear()
        {
            // Restore states.
            Engine.GraphicsManager.ResetState();
            Engine.GraphicsManager.ClearScreen();
            Engine.GraphicsManager.DefaultGLState();
            EnsureRenderTarget();
            if (_targetStack.Count == 1) Engine.GraphicsManager.ClearScreen();

            // Update the current camera.
            Camera.Update();

            // Sync the current shader.
            SystemSyncCurrentShader();
            Engine.GraphicsManager.CheckError("clear update");
        }

        /// <summary>
        /// Flush the main mapping buffer.
        /// </summary>
        internal void End()
        {
            // Submit remaining rendered.
            Submit();

            // Flush the target to the host.
            Engine.GraphicsManager.BindRenderTarget(null);
            Engine.GraphicsManager.StateClip(false);
            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("modelMatrix", Matrix4x4.Identity);
            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("viewMatrix", Matrix4x4.Identity);
            Engine.Renderer.Render(Vector3.Zero, CurrentTarget.Size, Color.White, CurrentTarget.Texture);
            Engine.Renderer.Submit();

            Engine.GraphicsManager.CheckError("flushing current target");
        }

        #endregion

        #region Scale APIs

        /// <summary>
        /// The host has changed size and the rendering must adapt.
        /// </summary>
        /// <param name="newSizeX">The new width of the host. If not provided will be taken from Host.Size.</param>
        /// <param name="newSizeY">The new height of the host. If not provided will be taken from Host.Size.</param>
        public void HostResized(int newSizeX = 0, int newSizeY = 0)
        {
            Vector2 size;
            if (newSizeX == 0 || newSizeY == 0)
                size = Engine.Host.Size;
            else
                size = new Vector2(newSizeX, newSizeY);

            // Check if size is 0.
            if (size.X == 0 || size.Y == 0) Engine.Log.Warning("Host resized to a size of 0.", MessageSource.Engine);

            // Calculate borderbox / pillarbox.
            float targetAspectRatio = BaseTarget.Size.X / BaseTarget.Size.Y;

            float width = size.X;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > size.Y)
            {
                height = size.Y;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            if (Engine.Flags.RenderFlags.IntegerScale)
            {
                float xIntScale = (float) Math.Floor(width / BaseTarget.Size.X);
                float yIntScale = (float) Math.Floor(height / BaseTarget.Size.Y);
                width = BaseTarget.Size.X * xIntScale;
                height = BaseTarget.Size.Y * yIntScale;
            }

            int vpX = (int) (size.X / 2 - width / 2);
            int vpY = (int) (size.Y / 2 - height / 2);

            // Set the host scale attributes.
            HostScale = new Vector2(width, height);
            HostMargins = new Vector2(vpX, vpY);

            // Set viewport.
            Engine.GraphicsManager?.SetViewport(vpX, vpY, (int) width, (int) height);
        }

        /// <summary>
        /// Scale the render size based on the host's aspect ratio.
        /// </summary>
        /// <param name="size">The set render size.</param>
        /// <returns>The scaled size to the aspect ratio of the host.</returns>
        public static Vector2 ScaleRenderSize(Vector2 size)
        {
            string aspectRatio = Helpers.GetAspectRatio(size.X, size.Y);
            Vector2 screenSize = Engine.Host.GetScreenSize();
            string screenAspectRatio = Helpers.GetAspectRatio(screenSize.X, screenSize.Y);

            if (aspectRatio == screenAspectRatio) return size;

            string[] aspectParams = screenAspectRatio.Split(':');
            float.TryParse(aspectParams[0], out float screenWidth);
            float.TryParse(aspectParams[1], out float screenHeight);

            float majorValue = Math.Max(size.X, size.Y);
            bool majorIsWidth = majorValue == size.X;

            float w;
            float h;

            // Calculate the appropriate aspect's render size.
            if (majorIsWidth)
            {
                w = size.X;
                h = screenHeight * size.X / screenWidth;
            }
            else
            {
                w = screenWidth * size.Y / screenHeight;
                h = size.Y;
            }

            return new Vector2(w, h);
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Render a quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void Render(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            _mainBuffer.MapNextQuad(position, size, color, texture, textureArea);
        }

        /// <summary>
        /// Renders a string to the screen.
        /// </summary>
        /// <param name="font">The font to render using.</param>
        /// <param name="textSize">The size to render in.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position to render to.</param>
        /// <param name="color">The color to render in.</param>
        [Obsolete("Use the Atlas overload instead.")]
        public void RenderString(Font font, uint textSize, string text, Vector3 position, Color color)
        {
            Atlas atlas = font.GetFontAtlas(textSize);
            RenderString(atlas, text, position, color);
        }

        /// <summary>
        /// Renders a string to the screen.
        /// </summary>
        /// <param name="atlas">The font atlas to use. Can be accessed from a Font asset.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position to render to.</param>
        /// <param name="color">The color to render in.</param>
        public void RenderString(Atlas atlas, string text, Vector3 position, Color color)
        {
            // Queue letters.
            Rectangle[] uvs = new Rectangle[text.Length];

            float penX = position.X;
            float penY = position.Y;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    penX = position.X;
                    penY += atlas.LineSpacing;
                    continue;
                }

                if (i > 0) penX += atlas.GetKerning(text[i - 1], text[i]);

                // Check if glyph is out of bounds, in which case set it to char 0, otherwise load the char.
                Glyph g = atlas.Glyphs.Length <= text[i] ? atlas.Glyphs[0] : atlas.Glyphs[text[i]];

                Vector3 renderPos = new Vector3(g.MinX + penX, penY + g.YBearing, position.Z);
                uvs[i] = new Rectangle(g.X, g.Y, g.Width, g.Height);
                Render(renderPos, uvs[i].Size, color, atlas.Texture, uvs[i]);
                penX += g.Advance;
            }
        }

        /// <summary>
        /// Render a line.
        /// </summary>
        /// <param name="pointOne">The first point.</param>
        /// <param name="pointTwo">The second point.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1)
        {
            _mainBuffer.MapNextLine(pointOne, pointTwo, color, thickness);
        }

        /// <summary>
        /// Render a rectangle outline.
        /// </summary>
        /// <param name="position">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderOutline(Vector3 position, Vector2 size, Color color, float thickness = 1)
        {
            RenderLine(position, new Vector3(position.X + size.X, position.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X + size.X, position.Y, position.Z), new Vector3(position.X + size.X, position.Y + size.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X + size.X, position.Y + size.Y, position.Z), new Vector3(position.X, position.Y + size.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X, position.Y + size.Y, position.Z), position, color, thickness);
        }

        /// <summary>
        /// Render a circle outline.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        public void RenderCircleOutline(Vector3 position, float radius, Color color, bool useCenter = false)
        {
            // Add the circle's model matrix.
            PushToModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float fX = 0;
            float fY = 0;
            float pX = 0;
            float pY = 0;

            // Generate points.
            for (uint i = 0; i < Engine.Flags.RenderFlags.CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / Engine.Flags.RenderFlags.CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                if (i == 0)
                {
                    RenderLine(new Vector3(radius + x, radius + y, 0), new Vector3(radius + x, radius + y, 0), color);
                    fX = x;
                    fY = y;
                }
                else if (i == Engine.Flags.RenderFlags.CircleDetail - 1)
                {
                    RenderLine(new Vector3(radius + pX, radius + pY, 0), new Vector3(radius + x, radius + y, 0), color);
                    RenderLine(new Vector3(radius + x, radius + y, 0), new Vector3(radius + fX, radius + fY, 0), color);
                }
                else
                {
                    RenderLine(new Vector3(radius + pX, radius + pY, 0), new Vector3(radius + x, radius + y, 0), color);
                }

                pX = x;
                pY = y;
            }

            // Remove the model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Render a circle.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        public void RenderCircle(Vector3 position, float radius, Color color, bool useCenter = false)
        {
            // Add the circle's model matrix.
            PushToModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float pX = 0;
            float pY = 0;
            float fX = 0;
            float fY = 0;

            // Generate points.
            for (uint i = 0; i < Engine.Flags.RenderFlags.CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / Engine.Flags.RenderFlags.CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                _mainBuffer.MapNextVertex(new Vector3(radius + pX, radius + pY, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius + x, radius + y, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);

                if (i == 0)
                {
                    fX = x;
                    fY = y;
                }
                else if (i == Engine.Flags.RenderFlags.CircleDetail - 1)
                {
                    _mainBuffer.MapNextVertex(new Vector3(radius + pX, radius + pY, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius + fX, radius + fY, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                }

                pX = x;
                pY = y;
            }

            // Remove the model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Draw an array of vertices and colors. The number of colors must be equal to the vertices or 1.
        /// </summary>
        /// <param name="vertices">
        /// The array of vertices.
        /// </param>
        /// <param name="colors">
        /// An array of colors corresponding to the vertices. If only one color is provided it will be applied
        /// to all vertices.
        /// </param>
        public void RenderVertices(Vector3[] vertices, params Color[] colors)
        {
            if (vertices.Length != colors.Length && colors.Length != 1)
            {
                Engine.Log.Warning("Tried to render vertices but the number of vertices and colors don't match, and the colors are not one.", MessageSource.Renderer);
                return;
            }

            _verticesStreamBuffer.Reset();

            for (int i = 0; i < vertices.Length; i++)
            {
                _verticesStreamBuffer.MapNextVertex(vertices[i], colors.Length == 1 ? colors[0] : colors[i]);
            }

            Render(_verticesStreamBuffer);
        }

        /// <summary>
        /// Shortcut for Render(position, texture.Size, Color.White, texture).
        /// </summary>
        /// <param name="position">The position to render to.</param>
        /// <param name="texture">The texture to use.</param>
        /// <param name="textureArea">The area of the texture to render.</param>
        public void RenderTexture(Vector3 position, Texture texture, Rectangle? textureArea = null)
        {
            Render(position, texture.Size, Color.White, texture, textureArea);
        }

        /// <summary>
        /// Shortcut for Render(position, size, Color.White, texture).
        /// </summary>
        /// <param name="position">The position to render to.</param>
        /// <param name="size">The size of the render.</param>
        /// <param name="texture">The texture to use.</param>
        /// <param name="textureArea">The area of the texture to render.</param>
        public void RenderTexture(Vector3 position, Vector2 size, Texture texture, Rectangle? textureArea = null)
        {
            Render(position, size, Color.White, texture, textureArea);
        }


        /// <summary>
        /// Shortcut for Render(position, texture.Size, color, texture).
        /// </summary>
        /// <param name="position">The position to render to.</param>
        /// <param name="color">The texture's tint.</param>
        /// <param name="texture">The texture to use.</param>
        /// <param name="textureArea">The area of the texture to render.</param>
        public void RenderTexture(Vector3 position, Color color, Texture texture, Rectangle? textureArea = null)
        {
            Render(position, texture.Size, color, texture, textureArea);
        }

        /// <summary>
        /// Submit all render commands so far.
        /// </summary>
        public void Submit()
        {
            GLThread.ForceGLThread();

            // Check if anything was mapped at all.
            if (_mainBuffer == null || !_mainBuffer.Mapping || !_mainBuffer.AnythingMapped) return;

            _mainBuffer.Render();
            _mainBuffer.Reset();
        }

        #endregion

        #region State Modification

        /// <summary>
        /// Sets the current shader to the one specified. If null sets the shader to the default one.
        /// </summary>
        /// <param name="shader">The shader to set.</param>
        public void SetShader(ShaderProgram shader = null)
        {
            // If shade will be modified, submit.
            if (shader != Engine.GraphicsManager.CurrentShader) Submit();

            // Bind the new shader.
            Engine.GraphicsManager.BindShaderProgram(shader);

            // Sync shader uniforms.
            SystemSyncCurrentShader();
        }

        /// <summary>
        /// Push a matrix on top of the model matrix stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void PushToModelMatrix(Matrix4x4 matrix, bool multiply = true)
        {
            // Flush the draw buffer.
            Submit();

            // Push into stack and update shader.
            _modelMatrix.Push(matrix, multiply);
            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix);
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            // Flush the draw buffer.
            Submit();

            // Pop out of stack and update shader.
            _modelMatrix.Pop();
            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix);
        }

        #endregion

        #region Render Target

        /// <summary>
        /// Push a render target on top of the target stack, meaning it will be used for subsequent drawing.
        /// </summary>
        /// <param name="target">The target to push.</param>
        public void PushRenderTarget(RenderTarget target)
        {
            Submit();
            _targetStack.Push(target);
            EnsureRenderTarget();
        }

        /// <summary>
        /// Pop off a render target off of the top of the target stack, meaning the one before it will be used for subsequent
        /// drawing.
        /// </summary>
        public void PopRenderTarget()
        {
            if (_targetStack.Count == 1)
            {
                Engine.Log.Error("Cannot pop off the default render target.", MessageSource.Game);
                return;
            }

            Submit();
            _targetStack.Pop();
            EnsureRenderTarget();
        }

        /// <summary>
        /// Clear the currently bound render target.
        /// </summary>
        public void ClearRenderTarget()
        {
            Engine.GraphicsManager.ClearScreen();
        }

        /// <summary>
        /// Resyncs the current render target with the one on top of the target stack.
        /// You probably shouldn't be calling this yourself.
        /// </summary>
        public void EnsureRenderTarget()
        {
            RenderTarget current = _targetStack.Peek();
            Engine.GraphicsManager.BindRenderTarget(current);
            SystemSyncCurrentShader();
        }

        #endregion

        #region Renderable

        /// <summary>
        /// Render a renderable object.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        public void Render(IRenderable renderable)
        {
            // Render the draw buffer.
            Submit();

            // Render the renderable.
            renderable.Render();
        }

        /// <summary>
        /// Render a renderable object with a model matrix.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="modelMatrix">The renderable's model matrix.</param>
        /// <param name="multiplyMatrix">Whether to multiply the new matrix by the previous matrix.</param>
        public void Render(IRenderable renderable, Matrix4x4 modelMatrix, bool multiplyMatrix = true)
        {
            // Push the model matrix.
            PushToModelMatrix(modelMatrix, multiplyMatrix);

            // Render the renderable.
            renderable.Render();

            // Pop model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="multiplyMatrix">Whether to multiply the new matrix by the previous matrix.</param>
        public void Render(ITransformRenderable renderable, bool multiplyMatrix = true)
        {
            // Push the model matrix.
            PushToModelMatrix(renderable.ModelMatrix, multiplyMatrix);

            // Render the renderable.
            renderable.Render();

            // Pop model matrix.
            PopModelMatrix();
        }

        #endregion

        #region System Functions

        /// <summary>
        /// Synchronize the view matrix with the current shader.
        /// </summary>
        public void SyncViewMatrix()
        {
            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("viewMatrix", ViewMatrixEnabled ? Camera.ViewMatrix : Matrix4x4.Identity);
            Engine.GraphicsManager.CheckError("syncing shader");
        }

        /// <summary>
        /// Update the camera's matrix.
        /// System function.
        /// </summary>
        public void UpdateCameraMatrix()
        {
            // If not on the GL thread, camera will be updated on the next frame.
            if (!GLThread.IsGLThread()) return;

            GLThread.ExecuteGLThread(() =>
            {
                // Flush the draw buffer.
                Submit();
                if (ViewMatrixEnabled) SyncViewMatrix();
            });
        }

        /// <summary>
        /// Synchronize the current shader's uniform properties with the actual ones.
        /// System function.
        /// </summary>
        /// <param name="full">Whether to perform a full synchronization. Some properties are not expected to change often.</param>
        public void SystemSyncCurrentShader(bool full = true)
        {
            if (full)
                Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("projectionMatrix",
                    Matrix4x4.CreateOrthographicOffCenter(0, CurrentTarget.Size.X, CurrentTarget.Size.Y, 0, Near2D, Far2D));

            Engine.GraphicsManager.CurrentShader.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix);
            SyncViewMatrix();

            Engine.GraphicsManager.CurrentShader.SetUniformFloat("iTime", Engine.TotalTime / 1000f);
            Engine.GraphicsManager.CurrentShader.SetUniformVector3("iResolution", new Vector3(CurrentTarget.Size.X, CurrentTarget.Size.Y, 0));
            Engine.GraphicsManager.CurrentShader.SetUniformVector4("iMouse",
                new Vector4(Engine.InputManager.GetMousePosition(), Engine.InputManager.IsMouseKeyDown(MouseKey.Left) ? 1 : 0, Engine.InputManager.IsMouseKeyDown(MouseKey.Right) ? 1 : 0));

            Engine.GraphicsManager.CheckError("Syncing shader");
        }

        #endregion
    }
}
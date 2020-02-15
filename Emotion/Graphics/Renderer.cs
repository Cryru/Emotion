#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// The renderer module doesn't actually perform rendering (that is done by the RenderComposer commands)
    /// but instead manages states and the rendering lifecycle.
    /// </summary>
    public sealed class Renderer
    {
        #region Settings

        /// <summary>
        /// How detailed drawn circles should be. Updates instantly. Default is 30.
        /// </summary>
        public int CircleDetail { get; set; } = 30;

        /// <summary>
        /// Whether v-sync is enabled. On some platforms this is forced on - check using the ForcedVSync flag. On by default.
        /// </summary>
        public bool VSync
        {
            get => ForcedVSync || _vSync;
            set
            {
                _vSync = value;
                GLThread.ExecuteGLThreadAsync(ApplySettings);
            }
        }

        private bool _vSync = true;

        /// <summary>
        /// The positive cut off of the camera.
        /// </summary>
        public float FarZ = 100;

        /// <summary>
        /// The negative cut off of the camera.
        /// </summary>
        public float NearZ = -100;

        #endregion

        #region Flags

        /// <summary>
        /// Whether v-sync is forced by the platform or driver.
        /// </summary>
        public bool ForcedVSync { get; internal set; }

        /// <summary>
        /// Whether the software renderer is being used.
        /// </summary>
        public bool SoftwareRenderer { get; private set; }

        /// <summary>
        /// Whether the renderer is running in compatibility mode, falling back to older features.
        /// </summary>
        public bool CompatibilityMode { get; private set; }

        /// <summary>
        /// Whether direct state access is supported.
        /// https://www.khronos.org/opengl/wiki/Direct_State_Access
        /// </summary>
        public bool Dsa { get; private set; }

        /// <summary>
        /// The maximum textures that can be mapped in one StreamBuffer and/or shader. If more than the allowed textures are mapped
        /// an exception is raised.
        /// </summary>
        public int TextureArrayLimit { get; private set; } = 16;

        /// <summary>
        /// The maximum number of indices in the default Ibo buffers.
        /// </summary>
        public uint MaxIndices { get; internal set; } = ushort.MaxValue;

        #endregion

        #region Objects

        /// <summary>
        /// A representation of the screen's frame buffer.
        /// </summary>
        public FrameBuffer ScreenBuffer { get; private set; }

        /// <summary>
        /// The frame buffer rendering is done to, before it is flushed to the screen buffer.
        /// </summary>
        public FrameBuffer DrawBuffer { get; private set; }

        /// <summary>
        /// The camera active in the scene.
        /// </summary>
        public CameraBase Camera;

        /// <summary>
        /// The stack of frame buffers.
        /// The screen buffer is not a part of this stack.
        /// </summary>
        private Stack<FrameBuffer> _bufferStack = new Stack<FrameBuffer>();

        /// <summary>
        /// The model matrix stack.
        /// </summary>
        private TransformationStack _matrixStack = new TransformationStack();

        /// <summary>
        /// The main composer used for "immediate mode" like drawing.
        /// </summary>
        private RenderComposer _composer;

        private RenderState _blitState;

        #endregion

        #region State

        /// <summary>
        /// The current drawing state. Don't modify directly!
        /// </summary>
        public RenderState CurrentState { get; private set; } = new RenderState();

        private bool _viewMatrix = true;

        /// <summary>
        /// The current frame buffer.
        /// </summary>
        public FrameBuffer CurrentTarget
        {
            get => _bufferStack.Count == 0 ? null : _bufferStack.Peek();
        }

        /// <summary>
        /// The current model matrix.
        /// </summary>
        public Matrix4x4 ModelMatrix
        {
            get => _matrixStack.CurrentMatrix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ratio between the current DrawBuffer resolution and RenderSize.
        /// Cannot fall below 1 as the RenderSize is the minimum size.
        /// If not in FullScale mode then this is always one.
        /// </summary>
        public float Scale { get; private set; } = 1;

        /// <summary>
        /// The closest integer scale to the current scale - used to make sure pixel art looks consistent.
        /// Cannot fall below 1 as the RenderSize is the minimum size.
        /// </summary>
        public int IntScale { get; private set; } = 1;

        #endregion

        internal void Setup()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

            Engine.Log.Info($"Created OpenGL Context {Gl.CurrentVersion}", MessageSource.Renderer);
            Engine.Log.Info($" Renderer: {Gl.CurrentRenderer}", MessageSource.Renderer);
            Engine.Log.Info($" Vendor: {Gl.CurrentVendor}", MessageSource.Renderer);
            Engine.Log.Info($" Shader: {Gl.CurrentShadingVersion}", MessageSource.Renderer);

            // Set flags.
            SoftwareRenderer = Gl.CurrentRenderer.Contains("llvmpipe");
            CompatibilityMode = SoftwareRenderer || Engine.Configuration.RendererCompatMode;
            Dsa = !CompatibilityMode && Gl.CurrentVersion.Major >= 4 && Gl.CurrentVersion.Minor >= 5;
            TextureArrayLimit = SoftwareRenderer ? 4 : Gl.CurrentLimits.MaxTextureImageUnits;

            Engine.Log.Info($" Flags: {(CompatibilityMode ? "Compat, " : "")}{(Dsa ? "Dsa, " : "")}Textures[{TextureArrayLimit}]", MessageSource.Renderer);

            // Create default indices.
            IndexBuffer.CreateDefaultIndexBuffers();

            // Set start state.
            Gl.ClearColor(0, 0, 0, 0);

            ShaderProgram defaultProgram = ShaderFactory.CreateDefaultShader();
            if (defaultProgram == null)
            {
                Engine.SubmitError(new Exception("Couldn't create default shaders."));
                return;
            }

            ShaderProgram.EnsureBound(ShaderFactory.DefaultProgram.Pointer);

            // Create a representation of the screen buffer, and the buffer which will be drawn to.
            ScreenBuffer = new FrameBuffer(0, Engine.Host.Window.Size);
            CreateDrawbuffer(Engine.Configuration.RenderSize);

            // Create the blit state command for copying the draw buffer to the screen buffer.
            _blitState = RenderState.Default.Clone();
            _blitState.AlphaBlending = false;
            _blitState.DepthTest = false;
            _blitState.ViewMatrix = false;

            // Decide on scaling mode.
            if (Engine.Configuration.ScaleBlackBars)
            {
                Engine.Host.Window.OnResize.AddListener(HostResizedBlackBars);
                HostResizedBlackBars(Engine.Host.Window.Size);
            }
            else
            {
                Engine.Host.Window.OnResize.AddListener(HostResized);
                HostResized(Engine.Host.Window.Size);
            }

            // Put in a default camera.
            Camera = new PixelArtCamera(Vector3.Zero);

            // Initialize the default composer.
            _composer = new RenderComposer();
        }

        #region Event Handles and Sizing

        /// <summary>
        /// Apply rendering settings.
        /// </summary>
        public void ApplySettings()
        {
            Engine.Host.Window.Context.SwapInterval = _vSync ? 1 : 0;
        }

        private void CreateDrawbuffer(Vector2 size)
        {
            if (!Engine.Configuration.UseIntermediaryBuffer)
            {
                DrawBuffer = new FrameBuffer(0, size);
                _bufferStack.Clear();
                _bufferStack.Push(DrawBuffer);
                return;
            }

            if (DrawBuffer == null)
            {
                DrawBuffer = new FrameBuffer(new Texture(size), true);
            }
            else if (DrawBuffer.Size == size)
            {
                return;
            }
            else if (DrawBuffer.Texture.Size.X >= size.X && DrawBuffer.Texture.Size.Y >= size.Y)
            {
                DrawBuffer.Size = size;
            }
            else
            {
                DrawBuffer?.Dispose();
                DrawBuffer = new FrameBuffer(new Texture(size), true);
            }

            _bufferStack.Clear();
            _bufferStack.Push(DrawBuffer);
        }

        /// <summary>
        /// Recreate the drawbuffer when the host is resized.
        /// </summary>
        internal bool HostResized(Vector2 size)
        {
            // Recalculate scale.
            Vector2 baseRes = Engine.Configuration.RenderSize;
            Vector2 ratio = size / baseRes;
            Scale = MathF.Min(ratio.X, ratio.Y);
            IntScale = (int) MathF.Floor((size.X + size.Y) / (baseRes.X + baseRes.Y));

            // Set viewport.
            Gl.Viewport(0, 0, (int) size.X, (int) size.Y);
            ScreenBuffer.Viewport = new Rectangle(0, 0, size);
            ScreenBuffer.Size = size;

            if (Engine.Configuration.IntScaleDrawBuffer)
            {
                Scale -= IntScale - 1;
                size /= IntScale;
                size.X = (int) size.X;
                size.Y = (int) size.Y;
                IntScale = 1;
            }

            Engine.Log.Info($"Resized host - scale is {Scale} and int scale is {IntScale}", MessageSource.Renderer);

            // Recreate draw buffer.
            CreateDrawbuffer(size);
            Camera?.RecreateMatrix();

            ApplySettings();

            return true;
        }

        /// <summary>
        /// Recalculate the draw buffer when the host is resized, using black bars.
        /// </summary>
        internal bool HostResizedBlackBars(Vector2 size)
        {
            // Calculate borderbox / pillarbox.
            float targetAspectRatio = DrawBuffer.Size.X / DrawBuffer.Size.Y;
            float width = size.X;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > size.Y)
            {
                height = size.Y;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            if (Engine.Configuration.IntScaleDrawBuffer)
            {
                var xIntScale = (float) Math.Floor(width / DrawBuffer.Size.X);
                var yIntScale = (float) Math.Floor(height / DrawBuffer.Size.Y);
                width = DrawBuffer.Size.X * xIntScale;
                height = DrawBuffer.Size.Y * yIntScale;
            }

            var vpX = (int) (size.X / 2 - width / 2);
            var vpY = (int) (size.Y / 2 - height / 2);

            // Set viewport.
            ScreenBuffer.Viewport = new Rectangle(vpX, vpY, width, height);
            ScreenBuffer.Size = size;

            ApplySettings();

            return true;
        }

        /// <summary>
        /// Scale the mouse position according to the draw buffer margins, if not in FullScale mode.
        /// </summary>
        public Vector2 ScaleMousePosition(Vector2 pos)
        {
            // Get the difference in scale.
            float scaleX = ScreenBuffer.Viewport.Size.X / DrawBuffer.Size.X;
            float scaleY = ScreenBuffer.Viewport.Size.Y / DrawBuffer.Size.Y;

            // Calculate letterbox/pillarbox margins.
            float marginX = ScreenBuffer.Size.X / 2 - ScreenBuffer.Viewport.Size.X / 2;
            float marginY = ScreenBuffer.Size.Y / 2 - ScreenBuffer.Viewport.Size.Y / 2;

            return new Vector2((pos.X - marginX) / scaleX, (pos.Y - marginY) / scaleY);
        }

        #endregion

        /// <summary>
        /// Called at the start of the frame.
        /// Prepares the renderer for rendering.
        /// </summary>
        public RenderComposer StartFrame()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

            // Reset cached bound state, because on some drivers SwapBuffers unbinds all objects.
            FrameBuffer.Bound = 0;
            VertexBuffer.Bound = 0;
            IndexBuffer.Bound = 0;
            VertexArrayObject.Bound = 0;
            ShaderProgram.Bound = 0;
            for (var i = 0; i < Texture.Bound.Length; i++)
            {
                Texture.Bound[i] = 0;
            }

            // Reset to the default state.
            _composer.SetState(RenderState.Default, true);

            // Clear the screen.
            ScreenBuffer.Bind();
            Clear();

            if (Engine.Configuration.UseIntermediaryBuffer)
            {
                // Clear the draw buffer.
                // No need to call EnsureRenderTarget as the DrawBuffer should be the only one in the stack here.
                DrawBuffer.Bind();
                Clear();
            }

            // Check if a render target was forgotten.
            if (_bufferStack.Count > 1)
                Debug.Assert(false);

            return _composer;
        }

        /// <summary>
        /// Called at the end of the frame.
        /// Flushes everything and performs the actual rendering.
        /// </summary>
        public void EndFrame()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

            if (Engine.Configuration.UseIntermediaryBuffer)
            {
                // Push a blit from the draw buffer to the screen buffer.
                _composer.SetState(_blitState);
                _composer.RenderTo(ScreenBuffer);
                _composer.RenderSprite(Vector3.Zero, ScreenBuffer.Size, Color.White, DrawBuffer.Texture);
                _composer.RenderTo(null);
            }

            _composer.InvalidateStateBatches();
        }

        public void Update()
        {
            // Update the camera
            Camera.Update();
        }

        #region State Control

        /// <summary>
        /// Set whether to use view matrix. Automatically sync to the current shader.
        /// </summary>
        /// <param name="matrix">Whether to use the view matrix.</param>
        public void SetViewMatrix(bool matrix)
        {
            _viewMatrix = matrix;
            CurrentState.ViewMatrix = matrix;
            SyncViewMatrix();
        }

        /// <summary>
        /// Set whether to use depth testing.
        /// </summary>
        /// <param name="depth">Whether to use depth testing.</param>
        public void SetDepth(bool depth)
        {
            if (depth)
            {
                Gl.Enable(EnableCap.DepthTest);
                Gl.DepthFunc(DepthFunction.Lequal);
            }
            else
            {
                Gl.Disable(EnableCap.DepthTest);
            }

            CurrentState.DepthTest = depth;
        }

        /// <summary>
        /// Whether to use stencil testing.
        /// </summary>
        /// <param name="stencil">Whether to use stencil testing.</param>
        /// <param name="clear">If enabling stencil testing the stencil buffer is cleared.</param>
        public void SetStencil(bool stencil, bool clear = true)
        {
            // Set the stencil test to it's default state - don't write to it.
            void StencilStateDefault()
            {
                Gl.StencilMask(0x00);
                Gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
                Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            }

            if (stencil)
            {
                Gl.Enable(EnableCap.StencilTest);

                if (!clear) return;
                ClearStencil();
                StencilStateDefault();
            }
            else
            {
                Gl.Disable(EnableCap.StencilTest);
                StencilStateDefault(); // Some drivers don't understand that off means off
            }

            CurrentState.StencilTest = stencil;
        }

        /// <summary>
        /// Whether to use (Src,1-Src) alpha blending.
        /// </summary>
        /// <param name="blend">Whether to use (Src,1-Src) alpha blending.</param>
        public void SetBlending(bool blend)
        {
            if (blend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }

            CurrentState.AlphaBlending = blend;
        }

        /// <summary>
        /// Set the clip box to use, if any.
        /// </summary>
        /// <param name="clip">The clip box to use.</param>
        public void SetClip(Rectangle? clip)
        {
            if (clip == null)
            {
                Gl.Disable(EnableCap.ScissorTest);
            }
            else
            {
                Gl.Enable(EnableCap.ScissorTest);
                Rectangle c = clip.Value;
                Gl.Scissor((int) c.X, (int) (CurrentTarget.Size.Y - c.Height - c.Y), (int) c.Width, (int) c.Height);
            }

            CurrentState.ClipRect = clip;
        }

        #endregion

        #region API

        /// <summary>
        /// Clear whatever is on the currently bound frame buffer.
        /// This is affected by the scissor if any.
        /// </summary>
        public void Clear()
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        /// <summary>
        /// Clear only the depth buffer of the currently bound frame buffer.
        /// </summary>
        public void ClearDepth()
        {
            Gl.Clear(ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Clear only the stencil buffer of the currently bound frame buffer.
        /// </summary>
        public void ClearStencil()
        {
            Gl.StencilMask(0xFF);
            Gl.Clear(ClearBufferMask.StencilBufferBit);
        }

        #endregion

        #region Framebuffer, Shader, and Model Matrix Syncronization and State

        /// <summary>
        /// Synchronizes uniform properties with the currently bound shader.
        /// </summary>
        public void SyncShader()
        {
            ShaderProgram currentShader = CurrentState.Shader;
            if (CurrentState.Shader == null) return;

            currentShader.SetUniformMatrix4("projectionMatrix", Matrix4x4.CreateOrthographicOffCenter(0, CurrentTarget.Size.X, CurrentTarget.Size.Y, 0, NearZ, FarZ));

            SyncModelMatrix();
            SyncViewMatrix();

            currentShader.SetUniformFloat("iTime", Engine.TotalTime / 1000f);
            currentShader.SetUniformVector3("iResolution", new Vector3(CurrentTarget.Size.X, CurrentTarget.Size.Y, 0));
            currentShader.SetUniformVector4("iMouse",
                new Vector4(Engine.Host.MousePosition, Engine.Host.IsMouseKeyDown(MouseKey.Left) ? 1 : 0, Engine.Host.IsMouseKeyDown(MouseKey.Right) ? 1 : 0));
        }

        /// <summary>
        /// Synchronizes the model matrix to the current shader.
        /// Moved to a separate function so that it can be updated without updating all uniforms.
        /// </summary>
        private void SyncModelMatrix()
        {
            CurrentState.Shader.SetUniformMatrix4("modelMatrix", ModelMatrix);
        }

        /// <summary>
        /// Synchronizes the view matrix to the current shader.
        /// Moved to a separate function so that it can be updated without updating all uniforms.
        /// </summary>
        private void SyncViewMatrix()
        {
            Matrix4x4 viewMat = _viewMatrix ? Camera.ViewMatrix : Matrix4x4.Identity;
            CurrentState.Shader.SetUniformMatrix4("viewMatrix", viewMat);
        }

        /// <summary>
        /// Ensures the current render target is current, and bound.
        /// </summary>
        public void EnsureRenderTarget()
        {
            // Happens on initialization.
            if (CurrentTarget == null) return;

            CurrentTarget.Bind();
            SyncShader();
        }

        /// <summary>
        /// Push a framebuffer on top of the target stack, meaning it will be used for subsequent drawing.
        /// </summary>
        /// <param name="target">The target to push.</param>
        internal void PushFramebuffer(FrameBuffer target)
        {
            _bufferStack.Push(target);
            EnsureRenderTarget();
        }

        /// <summary>
        /// Pop off a render target off of the top of the target stack, meaning the one before it will be used for subsequent
        /// drawing.
        /// </summary>
        internal void PopFramebuffer(bool rebindPrevious = true)
        {
            if (_bufferStack.Count == 1)
            {
                Engine.Log.Error("Cannot pop off the Drawbuffer.", MessageSource.Renderer);
                return;
            }

            _bufferStack.Pop();
            if(rebindPrevious) EnsureRenderTarget();
        }

        /// <summary>
        /// Push a matrix on top of the model matrix stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        internal void PushModelMatrix(Matrix4x4 matrix, bool multiply = true)
        {
            _matrixStack.Push(matrix, multiply);
            SyncModelMatrix();
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        internal void PopModelMatrix()
        {
            _matrixStack.Pop();
            SyncModelMatrix();
        }

        #endregion
    }
}
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Host;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics
{
    public sealed unsafe class Renderer
    {
        #region Properties

        /// <summary>
        /// Whether the renderer is running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// The default shader program. Is linked by default at start.
        /// </summary>
        public ShaderProgram DefaultShaderProgram { get; private set; }

        /// <summary>
        /// The time which has passed since start. Used for tracking time in shaders and such.
        /// </summary>
        public float Time { get; private set; }

        public TransformationStack TransformationStack { get; private set; }

        #endregion

        #region Render State

        #endregion

        #region Limits

        public static readonly int MaxRenderable = 6000;
        public static readonly int MaxRenderableSize = VertexData.SizeInBytes * 4;
        public static readonly int MaxBufferSize = MaxRenderable * MaxRenderableSize;
        public static readonly int MaxIndicesSize = MaxRenderable * 6;

        #endregion

        #region Attribute Locations

        public static readonly int VertexLocation = 0;
        public static readonly int ColorLocation = 1;

        #endregion

        #region Other

        /// <summary>
        /// The host of this renderer. This could be a window or an app, or anything which creates a GLES context.
        /// </summary>
        private IHost _host;

        #endregion

        #region Initialization

        public Renderer(Settings settings)
        {
            // todo: remove
            ThreadManager.BindThread();

            // Create host.
            if (CurrentPlatform.OS == PlatformID.Win32NT || CurrentPlatform.OS == PlatformID.Unix || CurrentPlatform.OS == PlatformID.MacOSX)
                _host = new Window(settings);
            else
                throw new Exception("Unsupported platform.");

            // todo: move to context
            _host.SetHooks(Update, Draw);

            SetupRenderer();
        }

        internal void SetupRenderer()
        {
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            TransformationStack = new TransformationStack();

            // Create a default program, and use it.
            DefaultShaderProgram = new ShaderProgram(null, null);
            DefaultShaderProgram.Use();

            _mapBuffer = new MapBuffer(MaxBufferSize);
            _dataPointer = _mapBuffer.Start();

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Run the host // todo: move to context
        /// </summary>
        public void Run()
        {
            if (Running) throw new Exception("Renderer is already running.");
            Running = true;

            // Start the host. Should be blocking.
            _host.Run();

            Running = false;
        }

        internal void Destroy()
        {
            _mapBuffer.Destroy();
        }

        #endregion

        private MapBuffer _mapBuffer;
        private VertexData* _dataPointer;
        private int _indicesCount;

        #region Management API - Called by Engine.

        public Action<float> update;
        public Action<float> draw;

        internal void Update(float deltaTime)
        {
            update?.Invoke(deltaTime);
        }

        internal void Draw(float frameTime)
        {
            float frmTime = frameTime;

            // Add to time.
            Time += frmTime;

            // Start drawing, clear the screen.
            Start();

            // Execute context draw.
            draw?.Invoke(frmTime);

            // Flush to host.
            End();

            Helpers.CheckError("renderer draw loop end");
            Thread.Sleep(1);
        }

        internal void Start()
        {
            // Clear.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        internal void End()
        {
            _host.SwapBuffers();

            Helpers.CheckError("end");
        }

        #endregion

        /// <summary>
        /// Sets the current shader program to the one provided, or default if none provided.
        /// </summary>
        /// <param name="program">The shader program to set to, or the default one if null.</param>
        public void UseShaderProgram(ShaderProgram program = null)
        {
            if (program == null)
                DefaultShaderProgram.Use();
            else
                program.Use();

            // Sync time.
            ShaderProgram.Current.SetUniformFloat("time", Time);
        }

        public void Render(Renderable2D renderable)
        {
            // Convert the color to an int.
            uint c = ((uint)renderable.Color.A << 24) | ((uint)renderable.Color.B << 16) | ((uint)renderable.Color.G << 8) | renderable.Color.R;

            // Add the model matrix to the stack.
            TransformationStack.Push(renderable.ModelMatrix);

            // Set four vertices.
            _dataPointer->Vertex = Vector3.TransformPosition(Vector3.Zero, TransformationStack.CurrentMatrix);
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(renderable.Size.X, 0, 0), TransformationStack.CurrentMatrix);
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(renderable.Size.X, renderable.Size.Y, 0), TransformationStack.CurrentMatrix);
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(0, renderable.Size.Y, 0), TransformationStack.CurrentMatrix);
            _dataPointer->Color = c;
            _dataPointer++;

            // Remove the model matrix from the stack.
            TransformationStack.Pop();

            // Increment indices count.
            _indicesCount += 6;

            Helpers.CheckError("render");
        }

        public void Flush()
        {
            // Draw indices.
            _mapBuffer.Draw(_indicesCount);

            // Reset count.
            _indicesCount = 0;

            // Start mapping the buffer.
            _dataPointer = _mapBuffer.Start();
        }
    }
}
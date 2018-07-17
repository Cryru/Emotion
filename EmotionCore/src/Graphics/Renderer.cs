// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Host;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.GLES.Buffer;

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

        #endregion

        #region Render State

        private Queue<Renderable2D> _renderQueue = new Queue<Renderable2D>();

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

        /// <summary>
        /// Run the renderer and its host.
        /// </summary>
        public void Run()
        {
            if (Running) throw new Exception("Renderer is already running.");
            Running = true;

            // Start the host. Should be blocking.
            _host.Run();

            Running = false;
        }

        internal void SetupRenderer()
        {
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            // Create a default program, and use it.
            DefaultShaderProgram = new ShaderProgram(null, null);
            DefaultShaderProgram.Use();

            _vao = new VertexArray();
            _vbo = new Buffer(MaxBufferSize, 3, BufferUsageHint.DynamicDraw);
            _vao.AttachBuffer(_vbo, VertexLocation, VertexData.SizeInBytes,  (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"), 3);
            _vao.AttachBuffer(_vbo, ColorLocation, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"), 4);
            Helpers.CheckError("loading vbo into vao");

            ushort[] indices = new ushort[MaxIndicesSize];
            ushort offset = 0;
            for (int i = 0; i < MaxIndicesSize; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }

            _ibo = new IndexBuffer(indices);

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            //GL.ClearColor(1f, 0, 0, 1f);
        }

        internal void Destroy()
        {
            _vbo?.Destroy();
            _ibo?.Destroy();
            _vao?.Destroy();
        }

        #endregion

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
            Thread.Sleep(1);
        }

        internal void Start()
        {
            // Clear.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _vbo.Bind();
            _dataPointer = (VertexData*) GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, VertexData.SizeInBytes, BufferAccessMask.MapWriteBit);
            Helpers.CheckError("start");
        }

        internal void End()
        {
            _host.SwapBuffers();

            Helpers.CheckError("end");
        }

        #endregion

        private int _indicesCount;
        private Buffer _vbo;
        private IndexBuffer _ibo;
        private VertexArray _vao;
        private VertexData* _dataPointer;

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
            Vector4 color = new Vector4(renderable.Color.R / 255f, renderable.Color.G / 255f, renderable.Color.B / 255f, renderable.Color.A / 255f);
            //uint c = (uint)color.W << 24 | (uint) renderable.Color.B << 16 | (uint)  renderable.Color.G << 8 | (uint) renderable.Color.R;

            _dataPointer->Vertex = renderable.Position;
            _dataPointer->Color = color;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(renderable.Position.X + renderable.Size.X, renderable.Position.Y, renderable.Position.Z);
            _dataPointer->Color = color;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(renderable.Position.X + renderable.Size.X, renderable.Position.Y + renderable.Size.Y, renderable.Position.Z);
            _dataPointer->Color = color;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(renderable.Position.X, renderable.Position.Y + renderable.Size.Y, renderable.Position.Z);
            _dataPointer->Color = color;
            _dataPointer++;

            _indicesCount += 6;

            Helpers.CheckError("render");
        }

        public void Flush()
        {
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            _vbo.Unbind();

            _vao.Bind();
            _ibo.Bind();

            GL.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
            Helpers.CheckError("draw");

            _ibo.Unbind();
            _vao.Unbind();
            _indicesCount = 0;

            Helpers.CheckError("flush");
        }
    }
}
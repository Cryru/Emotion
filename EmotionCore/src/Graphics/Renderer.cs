// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.Batching;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics
{
    public sealed class Renderer : ContextObject
    {
        /// <summary>
        /// The maximum number of renderable object that can fit in one buffer. This limit is determined by the IBO data type being
        /// ushort.
        /// </summary>
        public static readonly int MaxRenderable = ushort.MaxValue;

        #region Render State

        /// <summary>
        /// The transformation matrix stack. Everything to be rendered is multiplied by this.
        /// </summary>
        public TransformationStack TransformationStack { get; private set; }

        /// <summary>
        /// The buffer used for drawing objects.
        /// </summary>
        private QuadMapBuffer _mainBuffer;

        /// <summary>
        /// The buffer used for drawing outlines of objects.
        /// </summary>
        private LineMapBuffer _outlineBuffer;

        /// <summary>
        /// A list of objects to be rendered at the end of the current frame.
        /// </summary>
        private List<Renderable> _renderableQueue = new List<Renderable>();

        #endregion

        #region Initialization

        internal Renderer(Context context) : base(context)
        {
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            // Setup render state.
            TransformationStack = new TransformationStack();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram(null, null);
            defaultProgram.Bind();
            SyncShader(defaultProgram);

            // Setup main map buffer.
            _mainBuffer = new QuadMapBuffer(MaxRenderable);
            _outlineBuffer = new LineMapBuffer(MaxRenderable / 2);
            _mainBuffer.Start();
            _outlineBuffer.Start();

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            // Setup additional GL arguments.
            GL.Enable(EnableCap.Blend);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        internal void Destroy()
        {
            _mainBuffer.Delete();
            _outlineBuffer.Delete();
        }

        #endregion

        #region System API

        /// <summary>
        /// Clear the screen.
        /// </summary>
        internal void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Helpers.CheckError("clear");
        }

        /// <summary>
        /// Flush the system buffers and restart their mapping.
        /// </summary>
        internal void End()
        {
            RenderFlush();
            RenderOutlineFlush();

            // Check if any renderables are queued to be rendered.
            if (_renderableQueue.Count <= 0) return;
            IOrderedEnumerable<Renderable> ordered = _renderableQueue.OrderBy(x => x.Z);
            foreach (Renderable r in ordered)
            {
                r.Draw(this);
            }

            _renderableQueue.Clear();
        }

        public void SyncShader(ShaderProgram shader)
        {
            shader.SetUniformFloat("time", Context.Time);
            shader.SetUniformMatrix4("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, -100, 100));
        }

        #endregion

        #region Drawing API

        public void Render(Rectangle bounds, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _mainBuffer.Add(new Vector3(bounds.X, bounds.Y, 0), bounds.Size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void Render(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _mainBuffer.Add(location, size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void RenderOutline(Rectangle bounds, Color color, Matrix4? vertMatrix = null)
        {
            _outlineBuffer.Add(new Vector3(bounds.X, bounds.Y, 0), bounds.Size, color, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void RenderOutline(Vector3 location, Vector2 size, Color color, Matrix4? vertMatrix = null)
        {
            _outlineBuffer.Add(location, size, color, vertMatrix * TransformationStack.CurrentMatrix);
        }

        /// <summary>
        /// Flushes the main rendering buffer.
        /// </summary>
        public void RenderFlush()
        {
            if (!_mainBuffer.Mapping || !_mainBuffer.AnythingMapped) return;
            if (_mainBuffer.Mapping) _mainBuffer.FinishMapping();
            _mainBuffer.Draw();
            _mainBuffer.Start();
        }

        /// <summary>
        /// Flushes the outline rendering buffer.
        /// </summary>
        public void RenderOutlineFlush()
        {
            if (!_outlineBuffer.Mapping || !_outlineBuffer.AnythingMapped) return;
            if (_mainBuffer.Mapping) _outlineBuffer.FinishMapping();
            _outlineBuffer.Draw();
            _outlineBuffer.Start();
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        public void Render(Renderable renderable)
        {
            _renderableQueue.Add(renderable);
        }

        /// <summary>
        /// Renders a string to the screen.
        /// </summary>
        /// <param name="font">The font to render using.</param>
        /// <param name="textSize">The size to render in.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position to render to.</param>
        /// <param name="color">The color to render in.</param>
        public void RenderString(Font font, uint textSize, string text, Vector3 position, Color color)
        {
            Rectangle[] uvs = new Rectangle[text.Length];
            Atlas atlas = font.GetFontAtlas(textSize);

            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0) position.X += atlas.GetKerning(text[i - 1], text[i]);

                Glyph g = atlas.Glyphs[text[i]];

                Vector3 renderPos = new Vector3(position.X + g.MinX, position.Y + g.YBearing, 0);
                uvs[i] = new Rectangle(g.X, g.Y, g.Width, g.Height);
                Render(renderPos, uvs[i].Size, color, atlas.Texture, uvs[i]);
                position.X += g.Advance;
            }
        }

        #endregion
    }
}
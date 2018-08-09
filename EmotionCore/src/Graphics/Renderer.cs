// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using SharpFont;
using Glyph = Emotion.Graphics.Text.Glyph;

#endregion

namespace Emotion.Graphics
{
    public sealed class Renderer : ContextObject
    {
        #region Render State

        /// <summary>
        /// The transformation matrix stack. Everything to be rendered is multiplied by this.
        /// </summary>
        public TransformationStack TransformationStack { get; private set; }

        /// <summary>
        /// The buffer used for drawing objects.
        /// </summary>
        private MapBuffer _mainBuffer;

        /// <summary>
        /// The buffer used for drawing outlines of objects.
        /// </summary>
        private MapBuffer _outlineBuffer;

        /// <summary>
        /// The maximum number of renderable object that can fit in one buffer. This limit is determined by the IBO data type being
        /// ushort.
        /// </summary>
        public static readonly int MaxRenderable = ushort.MaxValue;

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

            // Setup main map buffer.
            _mainBuffer = new MapBuffer(MaxRenderable, this);
            _outlineBuffer = new MapBuffer(MaxRenderable / 2, this);
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
        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Helpers.CheckError("clear");
        }

        /// <summary>
        /// Flush the system buffers and restart their mapping.
        /// </summary>
        public void End()
        {
            RenderFlush();
            RenderOutlineFlush();
        }

        #endregion

        #region Drawing API

        public void RenderFlush()
        {
            if (!_mainBuffer.Mapping || !_mainBuffer.AnythingMapped) return;
            _mainBuffer.Flush();
            _mainBuffer.Start();
        }

        public void RenderOutlineFlush()
        {
            if (!_outlineBuffer.Mapping || !_outlineBuffer.AnythingMapped) return;
            _outlineBuffer.Flush((int) PrimitiveType.LineLoop);
            _outlineBuffer.Start();
        }

        public void Render(Renderable2D renderable)
        {
            // Add the model matrix to the stack.
            TransformationStack.Push(renderable.ModelMatrix);

            // Pass the renderable.
            _mainBuffer.Add(Vector3.Zero, renderable.Size, renderable.Color, renderable.Texture, renderable.TextureArea, TransformationStack.CurrentMatrix);

            // Remove the model matrix from the stack.
            TransformationStack.Pop();
        }

        public void Render(Label renderable)
        {
            RenderString(renderable.Font, renderable.TextSize, renderable.Text, Vector3.Zero, renderable.Color);
        }

        public void RenderString(Font font, int textSize, string text, Vector3 position, Color color)
        {
            Rectangle[] uvs = new Rectangle[text.Length];
            Atlas atlas = font.GetFontAtlas(textSize);

            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0)
                {
                    FTVector26Dot6 kerning = atlas.Face.GetKerning(text[i - 1], text[i], KerningMode.Unfitted);
                    position.X += (float) kerning.X.ToDouble();
                }

                Glyph g = atlas.Glyphs[text[i]];

                Vector3 renderPos = new Vector3(position.X + g.MinX, position.Y + g.YBearing, 0);
                uvs[i] = new Rectangle(g.X, g.Y, g.Width, g.Height);
                Render(renderPos, uvs[i].Size, color, atlas.Texture, uvs[i]);
                position.X += g.Advance;
            }
        }

        public void Render(Rectangle bounds, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _mainBuffer.Add(new Vector3(bounds.X, bounds.Y, 0), bounds.Size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void Render(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _mainBuffer.Add(location, size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void RenderOutline(Renderable2D renderable)
        {
            // Add the model matrix to the stack.
            TransformationStack.Push(renderable.ModelMatrix);

            // Pass the renderable.
            _outlineBuffer.Add(Vector3.Zero, renderable.Size, renderable.Color, renderable.Texture, renderable.TextureArea, TransformationStack.CurrentMatrix);

            // Remove the model matrix from the stack.
            TransformationStack.Pop();
        }

        public void RenderOutline(Rectangle bounds, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _outlineBuffer.Add(new Vector3(bounds.X, bounds.Y, 0), bounds.Size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        public void RenderOutline(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _outlineBuffer.Add(location, size, color, texture, textureArea, vertMatrix * TransformationStack.CurrentMatrix);
        }

        #endregion
    }
}
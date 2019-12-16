#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Text;
using Emotion.Graphics.Command;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Text;

#endregion

namespace Emotion.Graphics
{
    public sealed class RenderComposer
    {
        private const int RENDER_COMMAND_INITIAL_SIZE = 128;

        /// <summary>
        /// The quad batch currently active.
        /// </summary>
        public QuadBatch ActiveQuadBatch;

        /// <summary>
        /// Whether the composer has processed all of its commands.
        /// </summary>
        public bool Processed;

        /// <summary>
        /// Current list of pushed render commands.
        /// </summary>
        internal List<RenderCommand> RenderCommands = new List<RenderCommand>(RENDER_COMMAND_INITIAL_SIZE);

        /// <summary>
        /// Handles recycling of different recyclable command types.
        /// </summary>
        private Dictionary<Type, CommandRecycler> _commandCache = new Dictionary<Type, CommandRecycler>();

        #region Objects

        /// <summary>
        /// The common vertex buffer of this composer. Used if the command doesn't care about creating its own.
        /// Do not use outside of command execution.
        /// </summary>
        public VertexBuffer VertexBuffer;

        /// <summary>
        /// The common vao of this composer. Used if the command doesn't care about creating its own.
        /// Is bound to the common VertexBuffer.
        /// Do not use outside of command execution.
        /// </summary>
        public VertexArrayObject CommonVao;

        #endregion

        /// <summary>
        /// Process all render commands in the composer, preparing them for rendering.
        /// </summary>
        public void Process()
        {
            Debug.Assert(GLThread.IsGLThread());

            // Create graphics objects if needed.
            if (VertexBuffer == null)
            {
                VertexBuffer = new VertexBuffer((uint) (Engine.Renderer.MaxIndices * VertexData.SizeInBytes));
                CommonVao = new VertexArrayObject<VertexData>(VertexBuffer);
            }

            if (Processed) return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var c = 0; c < RenderCommands.Count; c++)
            {
                RenderCommands[c].Process(this);
            }

            Processed = true;
        }

        /// <summary>
        /// Execute all render commands in the composer.
        /// </summary>
        public void Execute()
        {
            Debug.Assert(GLThread.IsGLThread());
            Debug.Assert(Processed);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var c = 0; c < RenderCommands.Count; c++)
            {
                RenderCommands[c].Execute(this);
            }
        }

        /// <summary>
        /// Reset the render composer.
        /// </summary>
        public void Reset()
        {
            // Reset trackers.
            ActiveQuadBatch = null;
            RenderCommands.Clear();

            foreach (KeyValuePair<Type, CommandRecycler> recycler in _commandCache)
            {
                recycler.Value.Reset();
            }
        }

        /// <summary>
        /// Get a recycled render command of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of command to return.</typeparam>
        /// <returns>A recycled, ready to use instance of the specified command type.</returns>
        public T GetRenderCommand<T>() where T : RecyclableCommand, new()
        {
            Type type = typeof(T);
            if (_commandCache.ContainsKey(type)) return (T) _commandCache[type].GetObject();

            var newRecycler = new CommandRecycler<T>();
            _commandCache.Add(type, newRecycler);
            return (T) newRecycler.GetObject();
        }

        /// <summary>
        /// Push a render command to the composer.
        /// </summary>
        /// <param name="command">The command to push.</param>
        /// <param name="dontBatch">Whether to not batch if batchable.</param>
        public void PushCommand(RenderCommand command, bool dontBatch = false)
        {
            if (command == null) return;

            Processed = false;

            // Check if a batchable command.
            switch (command)
            {
                case RenderSpriteCommand batchable when !dontBatch:
                    // Push to the current batch. Create new batch if there is no active one, or it is full.
                    QuadBatch batch = ActiveQuadBatch;
                    if (batch == null || batch.Full)
                    {
                        batch = GetRenderCommand<QuadBatch>();
                        PushCommand(batch);
                        ActiveQuadBatch = batch;
                    }

                    batch.PushSprite(batchable);
                    return;
            }

            RenderCommands.Add(command);

            // Command post processing.
            switch (command)
            {
                // Changing the state invalidates the batches.
                // Drawing vertices too, as they will be drawn by a different VBO and IBO.
                case RenderVerticesCommand _:
                case ChangeStateCommand _:
                case FramebufferModificationCommand _:
                case ModelMatrixModificationCommand _:
                // We don't know what the sub composer will do, so invalidate batches.
                case SubComposerCommand _:
                // If pushing a batch and it wasn't caught by the !dontBatch case
                case QuadBatch _:
                    InvalidateStateBatches();
                    break;
            }
        }

        #region RenderSprite Overloads

        private static Matrix4x4 _flipMatX = Matrix4x4.CreateScale(-1, 1, 1);
        private static Matrix4x4 _flipMatY = Matrix4x4.CreateScale(1, -1, 1);

        /// <summary>
        /// Render a (textured) quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        /// <param name="flipX">Whether to flip the texture on the x axis.</param>
        /// <param name="flipY">Whether to flip the texture on the y axis.</param>
        public void RenderSprite(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            var command = GetRenderCommand<RenderSpriteCommand>();
            command.Position = position;
            command.Size = size;
            command.Color = color.ToUint();
            command.Texture = texture;
            command.TextureModifier = null;
            if (flipX)
            {
                if (texture != null && textureArea != null)
                {
                    Rectangle r = (Rectangle) textureArea;
                    r.X = texture.Size.X - r.Width - r.X;
                    textureArea = r;
                }
                command.TextureModifier = _flipMatX;
            }
            if (flipY)
            {
                if (texture != null && textureArea != null)
                {
                    Rectangle r = (Rectangle) textureArea;
                    r.Y = texture.Size.Y - r.Height - r.Y;
                    textureArea = r;
                }
                command.TextureModifier = _flipMatY;
            }
            command.UV = textureArea;
            PushCommand(command);
        }

        /// <summary>
        /// Render a (textured) quad to the screen using its transform.
        /// </summary>
        /// <param name="transform">The quad's transform.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void RenderSprite(Transform transform, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(transform.Position, transform.Size, color, texture, textureArea);
        }

        /// <summary>
        /// Render a (textured) quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void RenderSprite(Vector3 position, Vector2 size, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position, size, Color.White, texture, textureArea);
        }

        #endregion

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(new Vector3(pointOne, 0), new Vector3(pointTwo, 0), color, thickness);
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            var command = GetRenderCommand<RenderLineCommand>();
            command.PointOne = pointOne;
            command.PointTwo = pointTwo;
            command.Color = color.ToUint();
            command.Thickness = thickness;
            PushCommand(command);
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
        /// Render arbitrary vertices. Clockwise order is expected.
        /// </summary>
        /// <param name="vertices">The vertex to render.</param>
        /// <param name="colors">The color (or colors) of the vertex/vertices.</param>
        public void RenderVertices(List<Vector3> vertices, params Color[] colors)
        {
            RenderVertices(vertices.ToArray(), colors);
        }

        /// <summary>
        /// Render arbitrary vertices. Clockwise order is expected.
        /// </summary>
        /// <param name="vertices">The vertex to render.</param>
        /// <param name="colors">The color (or colors) of the vertex/vertices.</param>
        public void RenderVertices(Vector3[] vertices, params Color[] colors)
        {
            var command = GetRenderCommand<RenderVerticesCommand>();

            for (var i = 0; i < vertices.Length; i++)
            {
                uint c;
                if (i >= colors.Length)
                    c = colors.Length == 0 ? Color.White.ToUint() : colors[0].ToUint();
                else
                    c = colors[i].ToUint();

                command.AddVertex(new VertexData {Vertex = vertices[i], Color = c, Tid = -1});
            }

            PushCommand(command);
        }

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        /// <param name="position">The top left position of where to start drawing the string.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text itself.</param>
        /// <param name="atlas">The font atlas to use.</param>
        public void RenderString(Vector3 position, Color color, string text, DrawableFontAtlas atlas)
        {
            if (atlas?.Atlas?.Glyphs == null) return;

            var layout = new TextLayouter(atlas.Atlas);
            foreach (char c in text)
            {
                Vector2 gPos = layout.AddLetter(c, out AtlasGlyph g);
                if (g == null) continue;
                var uv = new Rectangle(g.Location, g.Size);
                RenderSprite(new Vector3(position.X + gPos.X, position.Y + gPos.Y, position.Z), uv.Size, color, atlas.Texture, uv);
            }
        }

        /// <summary>
        /// Render a composer.
        /// </summary>
        /// <param name="composer">The composer to render.</param>
        public void AddSubComposer(RenderComposer composer)
        {
            if (composer == this) return;

            var command = GetRenderCommand<SubComposerCommand>();
            command.Composer = composer;
            PushCommand(command);
        }

        /// <summary>
        /// Render a transform renderable. The rendering code is inside the object itself.
        /// This just makes sure its model matrix is pushed and invalidates the current batch.
        /// </summary>
        /// <param name="renderable">The renderable to enter.</param>
        public void Render(TransformRenderable renderable)
        {
            PushModelMatrix(renderable.ModelMatrix);
            renderable.Render(this);
            PopModelMatrix();
        }

        #region State Changes

        /// <summary>
        /// Enable or disable stencil testing.
        /// When enabling the stencil buffer is cleared.
        /// </summary>
        /// <param name="stencil">Whether to enable or disable stencil testing.</param>
        public void SetStencilTest(bool stencil)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                StencilTest = stencil
            };
            PushCommand(stateChange);
        }

        /// <summary>
        /// Set whether to use the view matrix.
        /// </summary>
        /// <param name="viewMatrix">Whether to use the view matrix.</param>
        public void SetUseViewMatrix(bool viewMatrix)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                ViewMatrix = viewMatrix
            };
            PushCommand(stateChange);
        }

        /// <summary>
        /// Set whether to use alpha blending.
        /// </summary>
        /// <param name="alphaBlend">Whether to use alpha blending.</param>
        public void SetAlphaBlend(bool alphaBlend)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                AlphaBlending = alphaBlend
            };
            PushCommand(stateChange);
        }

        /// <summary>
        /// Set whether to use depth testing.
        /// </summary>
        /// <param name="depth">Whether to use depth testing.</param>
        public void SetDepthTest(bool depth)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                DepthTest = depth
            };
            PushCommand(stateChange);
        }

        /// <summary>
        /// Set the current shader.
        /// </summary>
        /// <param name="shader">The shader to set as current.</param>
        /// <param name="onSet">A function to call once the shader is bound. You can upload uniforms and such in here.</param>
        public void SetShader(ShaderProgram shader, Action onSet)
        {
            SetShader(shader, (s) => onSet?.Invoke());
        }

        /// <summary>
        /// Set the current shader.
        /// </summary>
        /// <param name="shader">The shader to set as current.</param>
        /// <param name="onSet">A function to call once the shader is bound. You can upload uniforms and such in here.</param>
        public void SetShader(ShaderProgram shader = null, Action<ShaderProgram> onSet = null)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                Shader = shader ?? ShaderFactory.DefaultProgram
            };
            stateChange.ShaderOnSet = onSet;
            PushCommand(stateChange);
        }

        /// <summary>
        /// Whether, and where to clip.
        /// </summary>
        /// <param name="rect">The rectangle to clip outside of.</param>
        public void SetClipRect(Rectangle? rect)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = new RenderState
            {
                ClipRect = rect
            };
            PushCommand(stateChange);
        }

        /// <summary>
        /// Set a new state.
        /// </summary>
        /// <param name="newState">The state to set.</param>
        public void SetState(RenderState newState)
        {
            var stateChange = GetRenderCommand<ChangeStateCommand>();
            stateChange.State = newState;
            PushCommand(stateChange);
        }

        /// <summary>
        /// Invalidates current render batches.
        /// This should be done when the state changes in some way because calls afterwards will differ from those before and
        /// cannot be batched.
        /// </summary>
        public void InvalidateStateBatches()
        {
            ActiveQuadBatch = null;
        }


        /// <summary>
        /// Render to a frame buffer.
        /// </summary>
        /// <param name="buffer">The buffer to render to. If set to null will revert to the previous buffer.</param>
        public void RenderTo(FrameBuffer buffer)
        {
            var command = GetRenderCommand<FramebufferModificationCommand>();
            command.Buffer = buffer;
            PushCommand(command);
        }

        /// <summary>
        /// Clears the frame buffer currently being rendered to.
        /// </summary>
        public void ClearFrameBuffer()
        {
            var command = GetRenderCommand<ExecCodeCommand>();
            command.Func = Engine.Renderer.Clear;
            PushCommand(command);
        }

        /// <summary>
        /// Push a matrix on top of the model matrix stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void PushModelMatrix(Matrix4x4 matrix, bool multiply = true)
        {
            var command = GetRenderCommand<ModelMatrixModificationCommand>();
            command.Matrix = matrix;
            command.Multiply = multiply;
            PushCommand(command);
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            var command = GetRenderCommand<ModelMatrixModificationCommand>();
            command.Matrix = null;
            PushCommand(command);
        }

        #endregion

        public void Dispose()
        {
            VertexBuffer.Dispose();
            CommonVao.Dispose();

            foreach (KeyValuePair<Type, CommandRecycler> recycler in _commandCache)
            {
                recycler.Value.Dispose();
            }
        }
    }
}
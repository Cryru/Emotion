#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Command;
using Emotion.Graphics.Command.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Primitives;
using Emotion.Standard.Utility;
using OpenGL;
using VertexDataBatch = Emotion.Graphics.Command.Batches.SpriteBatchBase<Emotion.Graphics.Data.VertexData>;

#endregion

namespace Emotion.Graphics
{
    public sealed partial class RenderComposer
    {
        private const int RENDER_COMMAND_INITIAL_SIZE = 16;

        /// <summary>
        /// The quad batch currently active.
        /// </summary>
        public VertexDataBatch ActiveQuadBatch;

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

        #region Common Objects

        /// <summary>
        /// The common vertex buffer of this composer. Used if the command doesn't care about creating its own.
        /// Do not use outside of command execution.
        /// </summary>
        public VertexBuffer VertexBuffer;

        /// <summary>
        /// A common VertexData VAO. As that is the most commonly used structure.
        /// VaoCache[typeof(VertexData)] will also return this object.
        /// </summary>
        public VertexArrayObject CommonVao;

        /// <summary>
        /// Cached VAOs per structure type. These are all bound to the common VBO.
        /// </summary>
        public Dictionary<Type, VertexArrayObject> VaoCache = new Dictionary<Type, VertexArrayObject>();

        /// <summary>
        /// Shared memory pool.
        /// Page size is 2 mb - which is close to the data the maximum number of sprites that can be drawn in one batch using the
        /// VertexData struct.
        /// </summary>
        public NativeMemoryPool MemoryPool = new NativeMemoryPool(1000 * 1000 * 2);

        #endregion

        /// <summary>
        /// The factory for the currently active batch.
        /// </summary>
        private CommandRecycler _spriteBatchFactory;

        /// <summary>
        /// Whether to create GL objects if missing.
        /// </summary>
        private bool _createGl;

        /// <summary>
        /// Create a new RenderComposer to manage render commands.
        /// </summary>
        /// <param name="ownGraphicsMemory">
        /// Whether the composer will own graphics memory.
        /// If set to false the CommonVao, VertexBuffer, and VaoCache will be uninitialized.
        /// Beware that some commands require them.
        /// </param>
        public RenderComposer(bool ownGraphicsMemory = true)
        {
            _createGl = ownGraphicsMemory;
            Reset();
        }

        /// <summary>
        /// Process all render commands in the composer, preparing them for rendering.
        /// </summary>
        public void Process()
        {
            Debug.Assert(GLThread.IsGLThread());

            // Create graphics objects if needed.
            if (_createGl && VertexBuffer == null)
            {
                VertexBuffer = new VertexBuffer((uint) (Engine.Renderer.MaxIndices * VertexData.SizeInBytes));
                CommonVao = new VertexArrayObject<VertexData>(VertexBuffer);
                VaoCache.Add(typeof(VertexData), CommonVao);
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
            // Reset batch state.
            RestoreSpriteBatchType();

            // Clear old commands.
            RenderCommands.Clear();

            // Reset the memory pool.
            MemoryPool.Reset();

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
            CommandRecycler<T> recycler = GetRenderCommandRecycler<T>();
            return (T) recycler.GetObject();
        }

        /// <summary>
        /// Gets the recycling factory of the specified render command.
        /// </summary>
        /// <typeparam name="T">The command whose recycler to return.</typeparam>
        /// <returns>The recycling factory of the specified render command..</returns>
        public CommandRecycler<T> GetRenderCommandRecycler<T>() where T : RecyclableCommand, new()
        {
            Type type = typeof(T);
            if (_commandCache.ContainsKey(type)) return (CommandRecycler<T>) _commandCache[type];

            var newRecycler = new CommandRecycler<T>();
            _commandCache.Add(type, newRecycler);
            return newRecycler;
        }

        #region Batching

        /// <summary>
        /// Returns the current batch, or creates a new one if none.
        /// </summary>
        /// <returns></returns>
        public VertexDataBatch GetBatch()
        {
            if (ActiveQuadBatch != null && !ActiveQuadBatch.Full) return ActiveQuadBatch;

            // Create new batch if there is no active one, or it is full.
            var batch = (VertexDataBatch) _spriteBatchFactory.GetObject();

            // If using shared memory, link to the composer.
            if (batch is ISharedMemorySpriteBatch sharedMemoryBatch) sharedMemoryBatch.SetOwner(this);

            PushCommand(batch);
            ActiveQuadBatch = batch;

            return ActiveQuadBatch;
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
        /// Set the type of the sprite batch. This will invalidate the current batch and create a batch of this type.
        /// Subsequent batches until the end of the frame will be of this type.
        /// </summary>
        /// <typeparam name="T">The type of batch.</typeparam>
        public void SetSpriteBatchType<T>() where T : VertexDataBatch, new()
        {
            InvalidateStateBatches();
            _spriteBatchFactory = GetRenderCommandRecycler<T>();
        }

        /// <summary>
        /// Restore to the default batch type.
        /// </summary>
        public void RestoreSpriteBatchType()
        {
            SetSpriteBatchType<VertexDataShaderMemorySpriteBatch>();
        }

        #endregion

        /// <summary>
        /// Push a render command to the composer.
        /// </summary>
        /// <param name="command">The command to push.</param>
        /// <param name="_">Legacy</param>
        public void PushCommand(RenderCommand command, bool _ = false)
        {
            if (command == null) return;

            Processed = false;

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
                case ExecCodeCommand _:
                case StencilStateCommand _:
                // We don't know what the sub composer will do, so invalidate batches.
                case SubComposerCommand _:
                // If pushing a batch.
                case SpriteBatchBase _:
                    InvalidateStateBatches();
                    break;
            }
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
        /// Render a circle outline.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        /// <param name="circleDetail">How detailed the circle should be.</param>
        public void RenderCircleOutline(Vector3 position, float radius, Color color, bool useCenter = false, int circleDetail = 30)
        {
            // Add the circle's model matrix.
            PushModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float fX = 0;
            float fY = 0;
            float pX = 0;
            float pY = 0;

            // Generate points.
            for (uint i = 0; i < circleDetail; i++)
            {
                var angle = (float) (i * 2 * Math.PI / circleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                if (i == 0)
                {
                    RenderLine(new Vector3(radius + x, radius + y, 0), new Vector3(radius + x, radius + y, 0), color);
                    fX = x;
                    fY = y;
                }
                else if (i == circleDetail - 1)
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
        /// <param name="circleDetail">How detailed the circle should be.</param>
        public void RenderCircle(Vector3 position, float radius, Color color, bool useCenter = false, int circleDetail = 30)
        {
            // Add the circle's model matrix.
            PushModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float pX = 0;
            float pY = 0;
            float fX = 0;
            float fY = 0;

            var vertices = new List<Vector3>();

            // Generate points.
            for (uint i = 0; i < circleDetail; i++)
            {
                var angle = (float) (i * 2 * Math.PI / circleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                vertices.Add(new Vector3(radius + pX, radius + pY, 0));
                vertices.Add(new Vector3(radius + x, radius + y, 0));
                vertices.Add(new Vector3(radius, radius, 0));
                vertices.Add(new Vector3(radius, radius, 0));

                if (i == 0)
                {
                    fX = x;
                    fY = y;
                }
                else if (i == circleDetail - 1)
                {
                    vertices.Add(new Vector3(radius + pX, radius + pY, 0));
                    vertices.Add(new Vector3(radius + fX, radius + fY, 0));
                    vertices.Add(new Vector3(radius, radius, 0));
                    vertices.Add(new Vector3(radius, radius, 0));
                }

                pX = x;
                pY = y;
            }

            RenderVertices(vertices, color);

            // Remove the model matrix.
            PopModelMatrix();
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

        #region Stencil States

        /// <summary>
        /// Enables writing to the stencil buffer.
        /// Anything drawn after this call will have the specified value within the stencil buffer.
        /// By default the 0xFF value is written.
        /// </summary>
        public void StencilStartDraw(int value = 0xFF)
        {
            var stencilStateChange = GetRenderCommand<StencilStateCommand>();
            stencilStateChange.Func = StencilFunction.Always;
            stencilStateChange.Mask = 0xFF;
            stencilStateChange.Threshold = value;
            PushCommand(stencilStateChange);
        }

        /// <summary>
        /// Stops writing to the stencil buffer.
        /// Drawing after this call will not affect the stencil buffer.
        /// This is the default stencil state.
        /// </summary>
        public void StencilStopDraw()
        {
            var stencilStateChange = GetRenderCommand<StencilStateCommand>();
            stencilStateChange.Func = StencilFunction.Always;
            stencilStateChange.Mask = 0x00;
            stencilStateChange.Threshold = 0xFF;
            PushCommand(stencilStateChange);
        }

        /// <summary>
        /// Rendering after this call will not draw where the stencil buffer is smaller than the threshold value.
        /// This is 0xFF by default - matching the default of StencilStartDraw
        /// </summary>
        public void StencilCutOutFrom(int threshold = 0xFF)
        {
            var stencilStateChange = GetRenderCommand<StencilStateCommand>();
            stencilStateChange.Func = StencilFunction.Greater;
            stencilStateChange.Mask = 0x00;
            stencilStateChange.Threshold = threshold;
            PushCommand(stencilStateChange);
        }

        /// <summary>
        /// Opposite of StencilCutOutFrom. Only where the value is set will there be drawn.
        /// </summary>
        public void StencilFillIn(int threshold = 0xFF)
        {
            var stencilStateChange = GetRenderCommand<StencilStateCommand>();
            stencilStateChange.Func = StencilFunction.Greater;
            stencilStateChange.Mask = 0xFF;
            stencilStateChange.Threshold = threshold;
            PushCommand(stencilStateChange);
        }

        public void StencilMask(int filter = 0xFF)
        {
            var stencilStateChange = GetRenderCommand<StencilStateCommand>();
            stencilStateChange.Func = StencilFunction.Less;
            stencilStateChange.Mask = 0x00;
            stencilStateChange.Threshold = filter;
            PushCommand(stencilStateChange);
        }

        #endregion

        /// <summary>
        /// Toggle whether to render to the color buffer.
        /// </summary>
        /// <param name="renderColor">Whether to render to the color buffer.</param>
        public void ToggleRenderColor(bool renderColor)
        {
            var codeFunc = GetRenderCommand<ExecCodeCommand>();
            codeFunc.Func = () =>
            {
                if (renderColor)
                    Gl.ColorMask(true, true, true, true);
                else
                    Gl.ColorMask(false, false, false, false);
            };
            PushCommand(codeFunc);
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
            SetShader(shader, s => onSet?.Invoke());
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
        /// Render to a frame buffer, but clear all its attachments first.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to render to. If set to null will revert to the previous buffer, in which case the
        /// buffer will not be cleared.
        /// </param>
        public void RenderToAndClear(FrameBuffer buffer)
        {
            var command = GetRenderCommand<FramebufferModificationCommand>();
            command.Buffer = buffer;
            PushCommand(command);

            if (buffer != null) ClearFrameBuffer();
        }

        /// <summary>
        /// Works like RenderTo(null) but doesn't rebind the previous target. Used for swapping between targets.
        /// </summary>
        public void RenderTargetPop()
        {
            var command = GetRenderCommand<FramebufferModificationCommand>();
            command.Buffer = null;
            command.RebindPrevious = false;
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
        /// Clears the depth buffer of the currently bound frame buffer.
        /// </summary>
        public void ClearDepth()
        {
            var command = GetRenderCommand<ExecCodeCommand>();
            command.Func = Engine.Renderer.ClearDepth;
            PushCommand(command);
        }

        /// <summary>
        /// Clears the stencil buffer of the currently bound frame buffer.
        /// This will also reset the stencil state to "StencilStopDraw"
        /// </summary>
        public void ClearStencil()
        {
            var command = GetRenderCommand<ExecCodeCommand>();
            command.Func = Engine.Renderer.ClearStencil;
            PushCommand(command);
            StencilStopDraw();
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

            foreach (KeyValuePair<Type, VertexArrayObject> vao in VaoCache)
            {
                vao.Value.Dispose();
            }

            MemoryPool.Dispose();

            foreach (KeyValuePair<Type, CommandRecycler> recycler in _commandCache)
            {
                recycler.Value.Dispose();
            }
        }
    }
}
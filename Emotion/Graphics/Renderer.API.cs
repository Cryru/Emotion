#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Primitives;
using OpenGL;
using VertexDataBatch = Emotion.Graphics.Batches.SpriteBatchBase<Emotion.Graphics.Data.VertexData>;

#endregion

namespace Emotion.Graphics
{
    public sealed partial class RenderComposer
    {
        #region Batching

        /// <summary>
        /// Returns the current batch, or creates a new one if none.
        /// </summary>
        public VertexDataBatch GetBatch()
        {
            if (ActiveQuadBatch.Full) InvalidateStateBatches();

            return ActiveQuadBatch;
        }

        /// <summary>
        /// Invalidates the current batch - flushing it to the current buffer.
        /// This should be done when the state changes in some way because calls afterwards will differ from those before and
        /// cannot be batched.
        /// </summary>
        public void InvalidateStateBatches()
        {
            if (ActiveQuadBatch == null || ActiveQuadBatch.BatchedSprites == 0) return;
            PerfProfiler.FrameEventStart($"RenderBatch {ActiveQuadBatch.BatchedSprites} Sprites {ActiveQuadBatch.TextureSlotUtilization} Textures");
            ActiveQuadBatch.Render(this);
            ActiveQuadBatch.Recycle();
            PerfProfiler.FrameEventEnd($"RenderBatch {ActiveQuadBatch.BatchedSprites} Sprites {ActiveQuadBatch.TextureSlotUtilization} Textures");
        }

        /// <summary>
        /// Set a custom sprite batch as the current batch.
        /// This will flush the old batch.
        /// </summary>
        /// <param name="batch">The batch to set as current.</param>
        public void SetSpriteBatch(VertexDataBatch batch)
        {
            InvalidateStateBatches();

            // If using shared memory, link to the composer.
            if (batch is ISharedMemorySpriteBatch sharedMemoryBatch) sharedMemoryBatch.SetMemory(FencedBufferSource);

            ActiveQuadBatch = batch;
        }

        /// <summary>
        /// The default sprite batch factory method.
        /// </summary>
        public void SetDefaultSpriteBatch()
        {
            InvalidateStateBatches();

            var defaultBatch = new VertexDataSharedMemorySpriteBatch();
            defaultBatch.SetMemory(FencedBufferSource);
            ActiveQuadBatch = defaultBatch;
        }

        #endregion

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
            InvalidateStateBatches();

            int neededLength = vertices.Length * VertexData.SizeInBytes;
            if (neededLength > VertexBuffer.Size) VertexBuffer.Upload(IntPtr.Zero, (uint) neededLength);

            Span<VertexData> vertMapper = VertexBuffer.CreateMapper<VertexData>(0, vertices.Length * VertexData.SizeInBytes);
            for (var v = 0; v < vertices.Length; v++)
            {
                vertMapper[v].Vertex = vertices[v];
                vertMapper[v].Color = v >= colors.Length ? colors.Length == 0 ? Color.White.ToUint() : colors[0].ToUint() : colors[v].ToUint();
                vertMapper[v].Tid = -1;
            }

            VertexBuffer.FinishMapping();
            VertexArrayObject.EnsureBound(CommonVao);
            IndexBuffer.EnsureBound(IndexBuffer.SequentialIbo.Pointer);
            Gl.DrawElements(PrimitiveType.TriangleFan, vertices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
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

        /// <summary>
        /// Render a renderable object.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        public void Render(IRenderable renderable)
        {
            renderable.Render(this);
        }

        #region State Changes

        /// <summary>
        /// Enable or disable stencil testing.
        /// When enabling the stencil buffer is cleared.
        /// </summary>
        /// <param name="stencil">Whether to enable or disable stencil testing.</param>
        public void SetStencilTest(bool stencil)
        {
            InvalidateStateBatches();

            // Set the stencil test to it's default state - don't write to it.

            if (stencil)
            {
                Gl.Enable(EnableCap.StencilTest);

                // Clear after enabling.
                ClearStencil();
                StencilStateDefault();
            }
            else
            {
                Gl.Disable(EnableCap.StencilTest);
                StencilStateDefault(); // Some drivers don't understand that off means off
            }

            Engine.Renderer.CurrentState.StencilTest = stencil;
        }

        private static void StencilStateDefault()
        {
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
            Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        }

        #region Stencil States

        /// <summary>
        /// Enables writing to the stencil buffer.
        /// Anything drawn after this call will have the specified value within the stencil buffer.
        /// By default the 0xFF value is written.
        /// </summary>
        public void StencilStartDraw(int value = 0xFF)
        {
            InvalidateStateBatches();
            Gl.StencilMask(0xFF);
            Gl.StencilFunc(StencilFunction.Always, value, 0xFF);
        }

        /// <summary>
        /// Stops writing to the stencil buffer.
        /// Drawing after this call will not affect the stencil buffer.
        /// This is the default stencil state.
        /// </summary>
        public void StencilStopDraw()
        {
            InvalidateStateBatches();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
        }

        /// <summary>
        /// Rendering after this call will not draw where the stencil buffer is smaller than the threshold value.
        /// This is 0xFF by default - matching the default of StencilStartDraw
        /// </summary>
        public void StencilCutOutFrom(int threshold = 0xFF)
        {
            InvalidateStateBatches();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Greater, threshold, 0xFF);
        }

        /// <summary>
        /// Opposite of StencilCutOutFrom. Only where the value is set will there be drawn.
        /// </summary>
        public void StencilFillIn(int threshold = 0xFF)
        {
            InvalidateStateBatches();
            Gl.StencilMask(0xFF);
            Gl.StencilFunc(StencilFunction.Greater, threshold, 0xFF);
        }

        public void StencilMask(int filter = 0xFF)
        {
            InvalidateStateBatches();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Less, filter, 0xFF);
        }

        #endregion

        /// <summary>
        /// Toggle whether to render to the color buffer.
        /// </summary>
        /// <param name="renderColor">Whether to render to the color buffer.</param>
        public void ToggleRenderColor(bool renderColor)
        {
            InvalidateStateBatches();
            if (renderColor)
                Gl.ColorMask(true, true, true, true);
            else
                Gl.ColorMask(false, false, false, false);
        }

        /// <summary>
        /// Set whether to use the view matrix.
        /// </summary>
        /// <param name="viewMatrix">Whether to use the view matrix.</param>
        public void SetUseViewMatrix(bool viewMatrix)
        {
            InvalidateStateBatches();
            CurrentState.ViewMatrix = viewMatrix;
            SyncViewMatrix();
        }

        /// <summary>
        /// Set whether to use (Src,1-Src,1,1-Src) alpha blending.
        /// This causes transparent objects to blend their colors.
        /// </summary>
        /// <param name="alphaBlend">Whether to use alpha blending.</param>
        public void SetAlphaBlend(bool alphaBlend)
        {
            InvalidateStateBatches();

            if (alphaBlend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }

            Engine.Renderer.CurrentState.AlphaBlending = alphaBlend;
        }

        /// <summary>
        /// Set whether to use depth testing.
        /// </summary>
        /// <param name="depth">Whether to use depth testing.</param>
        public void SetDepthTest(bool depth)
        {
            InvalidateStateBatches();

            if (depth)
            {
                Gl.Enable(EnableCap.DepthTest);
                Gl.DepthFunc(DepthFunction.Lequal);
            }
            else
            {
                Gl.Disable(EnableCap.DepthTest);
            }

            Engine.Renderer.CurrentState.DepthTest = depth;
        }

        /// <summary>
        /// Set the current shader.
        /// </summary>
        /// <param name="shader">The shader to set as current.</param>
        public ShaderProgram SetShader(ShaderProgram shader = null)
        {
            InvalidateStateBatches();
            shader ??= ShaderFactory.DefaultProgram;
            ShaderProgram.EnsureBound(shader.Pointer);
            CurrentState.Shader = shader;
            SyncShader();

            return shader;
        }

        /// <summary>
        /// Whether, and where to clip.
        /// </summary>
        /// <param name="clip">The rectangle to clip outside of.</param>
        public void SetClipRect(Rectangle? clip)
        {
            InvalidateStateBatches();

            if (clip == null)
            {
                Gl.Disable(EnableCap.ScissorTest);
            }
            else
            {
                Gl.Enable(EnableCap.ScissorTest);
                Rectangle c = clip.Value;
                Gl.Scissor((int) c.X,
                    (int) (Engine.Renderer.CurrentTarget.Viewport.Height - c.Height - c.Y),
                    (int) c.Width,
                    (int) c.Height);
            }

            Engine.Renderer.CurrentState.ClipRect = clip;
        }

        /// <summary>
        /// Set a new state.
        /// </summary>
        /// <param name="newState">The state to set.</param>
        /// <param name="force">Whether to set it regardless of the previous state.</param>
        public void SetState(RenderState newState, bool force = false)
        {
            InvalidateStateBatches();

            // Check which state changes should apply, by checking which were set and which differ from the current.
            if (newState.Shader != null && (force || newState.Shader != Engine.Renderer.CurrentState.Shader))
            {
                ShaderProgram.EnsureBound(newState.Shader.Pointer);
                Engine.Renderer.CurrentState.Shader = newState.Shader;
                Engine.Renderer.SyncShader();
            }

            if (newState.DepthTest != null && (force || newState.DepthTest != Engine.Renderer.CurrentState.DepthTest)) SetDepthTest((bool) newState.DepthTest);
            if (newState.StencilTest != null && (force || newState.StencilTest != Engine.Renderer.CurrentState.StencilTest)) SetStencilTest((bool) newState.StencilTest);
            if (newState.AlphaBlending != null && (force || newState.AlphaBlending != Engine.Renderer.CurrentState.AlphaBlending)) SetAlphaBlend((bool) newState.AlphaBlending);
            if (newState.ViewMatrix != null && (force || newState.ViewMatrix != Engine.Renderer.CurrentState.ViewMatrix)) SetUseViewMatrix((bool) newState.ViewMatrix);
            if (force || newState.ClipRect != Engine.Renderer.CurrentState.ClipRect) SetClipRect(newState.ClipRect);
        }

        /// <summary>
        /// Render to a frame buffer.
        /// </summary>
        /// <param name="buffer">The buffer to render to. If set to null will revert to the previous buffer.</param>
        public void RenderTo(FrameBuffer buffer)
        {
            InvalidateStateBatches();
            if (buffer != null)
                Engine.Renderer.PushFramebuffer(buffer);
            else
                Engine.Renderer.PopFramebuffer();
        }

        /// <summary>
        /// Works like RenderTo(null) but doesn't rebind the previous target. Used for swapping between targets.
        /// </summary>
        public void RenderTargetPop()
        {
            InvalidateStateBatches();
            Engine.Renderer.PopFramebuffer();
        }

        /// <summary>
        /// Clear whatever is on the currently bound frame buffer.
        /// This is affected by the scissor if any.
        /// </summary>
        public void ClearFrameBuffer()
        {
            InvalidateStateBatches();
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        /// <summary>
        /// Clears the depth buffer of the currently bound frame buffer.
        /// </summary>
        public void ClearDepth()
        {
            InvalidateStateBatches();
            Gl.Clear(ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Clears the stencil buffer of the currently bound frame buffer.
        /// This will also reset the stencil state to "StencilStopDraw"
        /// </summary>
        public void ClearStencil()
        {
            InvalidateStateBatches();
            StencilStartDraw();
            Gl.Clear(ClearBufferMask.StencilBufferBit);
            StencilStopDraw();
        }

        /// <summary>
        /// Push a matrix on top of the model matrix stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void PushModelMatrix(Matrix4x4 matrix, bool multiply = true)
        {
            InvalidateStateBatches();
            _matrixStack.Push(matrix, multiply);
            SyncModelMatrix();
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            InvalidateStateBatches();
            _matrixStack.Pop();
            SyncModelMatrix();
        }

        #endregion
    }
}
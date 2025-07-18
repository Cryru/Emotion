﻿#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
    public sealed partial class RenderComposer
    {
        /// <summary>
        /// Invalidates the current batch - flushing it to the current buffer.
        /// This should be done when the state changes in some way because calls afterwards will differ from those before and
        /// cannot be batched.
        /// </summary>
        public void FlushRenderStream()
        {
            if (RenderStream == null || !RenderStream.AnythingMapped) return;
            RenderStream.FlushRender();
        }

        /// <summary>
        /// Render arbitrary vertices. Clockwise order is expected.
        /// </summary>
        /// <param name="vertices">The vertex to render.</param>
        /// <param name="colors">The color (or colors) of the vertex/vertices.</param>
        public void RenderVertices(List<Vector2> vertices, params Color[] colors)
        {
            RenderVertices(vertices.ToArray(), colors);
        }

        /// <summary>
        /// Render arbitrary vertices. Clockwise order is expected.
        /// </summary>
        /// <param name="verts">The vertex to render.</param>
        /// <param name="colors">The color (or colors) of the vertex/vertices.</param>
        public void RenderVertices(Vector2[] verts, params Color[] colors)
        {
            var vertCount = (uint) verts.Length;
            Span<VertexData> vertices = RenderStream.GetStreamMemory(vertCount, BatchMode.TriangleFan);
            for (var i = 0; i < verts.Length; i++)
            {
                vertices[i].Vertex = verts[i].ToVec3();
                vertices[i].Color = i >= colors.Length ? colors.Length == 0 ? Color.WhiteUint : colors[0].ToUint() : colors[i].ToUint();
                vertices[i].UV = Vector2.Zero;
            }
        }

        public void RenderVertices(Span<Vector3> verts, params Color[] colors)
        {
            var vertCount = (uint)verts.Length;
            Span<VertexData> vertices = RenderStream.GetStreamMemory(vertCount, BatchMode.TriangleFan);
            for (var i = 0; i < verts.Length; i++)
            {
                vertices[i].Vertex = verts[i];
                vertices[i].Color = i >= colors.Length ? colors.Length == 0 ? Color.WhiteUint : colors[0].ToUint() : colors[i].ToUint();
                vertices[i].UV = Vector2.Zero;
            }
        }

        public void RenderQuad(Span<Vector3> verts, Color color)
        {
            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
            VertexData.WriteDefaultQuadUV(vertices);

            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Vertex = verts[i];
                vertices[i].Color = color.ToUint();
            }
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
            PerfProfiler.FrameEventStart("StateChange: Stencil");
            FlushRenderStream();

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

            RenderState currentState = CurrentState;
            currentState.StencilTest = stencil;
            CurrentState = currentState;
            PerfProfiler.FrameEventEnd("StateChange: Stencil");
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
        /// You might want to disable drawing to color buffer while drawing on the stencil buffer.
        /// </summary>
        public void StencilStartDraw(int value = 0xFF)
        {
            FlushRenderStream();
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
            FlushRenderStream();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
        }

        /// <summary>
        /// Rendering after this call will not draw where the stencil buffer is smaller than the threshold value.
        /// This is 0xFF by default - matching the default of StencilStartDraw
        /// </summary>
        public void StencilCutOutFrom(int threshold = 0xFF)
        {
            FlushRenderStream();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Greater, threshold, 0xFF);
        }

        /// <summary>
        /// Opposite of StencilCutOutFrom. Only where the value is set will there be drawn.
        /// </summary>
        public void StencilFillIn(int threshold = 0xFF)
        {
            FlushRenderStream();
            Gl.StencilMask(0xFF);
            Gl.StencilFunc(StencilFunction.Lequal, threshold, 0xFF);
        }

        public void StencilMask(int filter = 0xFF)
        {
            FlushRenderStream();
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Less, filter, 0xFF);
        }

        public void StencilWindingStart()
        {
            FlushRenderStream();
            Gl.StencilMask(0xFF);
            // Each draw inverts the value in the stencil.
            Gl.StencilFunc(StencilFunction.Always, 0, 1);
            Gl.StencilOp(StencilOp.Invert, StencilOp.Invert, StencilOp.Invert);
        }

        public void StencilWindingEnd()
        {
            FlushRenderStream();
            Gl.StencilMask(0xFF);
            // Enable drawing only where the value is 1.
            Gl.StencilFunc(StencilFunction.Equal, 1, 1);
            Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        }

        #endregion

        /// <summary>
        /// Toggle whether to render to the color buffer.
        /// </summary>
        /// <param name="renderColor">Whether to render to the color buffer.</param>
        public void ToggleRenderColor(bool renderColor)
        {
            FlushRenderStream();
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
            FlushRenderStream();
            RenderState currentState = CurrentState;
            currentState.ViewMatrix = viewMatrix;
            CurrentState = currentState;
            SyncViewMatrix();
        }

        /// <summary>
        /// Set the preferred behavior of the projection matrix.
        /// </summary>
        /// <param name="behavior">The behavior of which projection matrix to apply.</param>
        public void SetProjectionBehavior(ProjectionBehavior behavior)
        {
            FlushRenderStream();
            RenderState currentState = CurrentState;
            currentState.ProjectionBehavior = behavior;
            CurrentState = currentState;
            SyncViewMatrix();
        }

        /// <summary>
        /// Set whether to use alpha blending.
        /// This causes transparent objects to blend their colors when drawn on top of each other.
        /// </summary>
        /// <param name="alphaBlend">Whether to use alpha blending.</param>
        public void SetAlphaBlend(bool alphaBlend)
        {
            PerfProfiler.FrameEventStart("StateChange: AlphaBlend");
            FlushRenderStream();

            if (alphaBlend)
            {
                Gl.Enable(EnableCap.Blend);
                Gl.BlendFuncSeparate(CurrentState.SFactorRgb, CurrentState.DFactorRgb, CurrentState.SFactorA, CurrentState.DFactorA);
            }
            else
            {
                Gl.Disable(EnableCap.Blend);
            }

            RenderState currentState = CurrentState;
            currentState.AlphaBlending = alphaBlend;
            CurrentState = currentState;
            PerfProfiler.FrameEventEnd("StateChange: AlphaBlend");
        }

        /// <summary>
        /// Set the type of alpha blending to use.
        /// The default is (Src,1-Src,1,1-Src)
        /// </summary>
        public void SetAlphaBlendType(BlendingFactor sFactorRgb, BlendingFactor dFactorRgb, BlendingFactor sFactorAlpha, BlendingFactor dFactorAlpha)
        {
            FlushRenderStream();
            RenderState currentState = CurrentState;
            currentState.SFactorRgb = sFactorRgb;
            currentState.DFactorRgb = dFactorRgb;
            currentState.SFactorA = sFactorAlpha;
            currentState.DFactorA = dFactorAlpha;
            CurrentState = currentState;
            if (currentState.AlphaBlending == true) SetAlphaBlend(true);
        }

        /// <summary>
        /// Set the alpha blend type to the default one of (Src,1-Src,1,1-Src)
        /// </summary>
        public void SetDefaultAlphaBlendType()
        {
            var defaultState = RenderState.Default;
            SetAlphaBlendType(defaultState.SFactorRgb, defaultState.DFactorRgb, defaultState.SFactorA, defaultState.DFactorA);
        }

        /// <summary>
        /// Set whether to use depth testing.
        /// </summary>
        /// <param name="depth">Whether to use depth testing.</param>
        public void SetDepthTest(bool depth)
        {
            PerfProfiler.FrameEventStart("StateChange: DepthTest");
            FlushRenderStream();

            if (depth)
            {
                Gl.Enable(EnableCap.DepthTest);
                Gl.DepthFunc(DepthFunction.Lequal);
            }
            else
            {
                Gl.Disable(EnableCap.DepthTest);
            }

            RenderState currentState = CurrentState;
            currentState.DepthTest = depth;
            CurrentState = currentState;
            PerfProfiler.FrameEventEnd("StateChange: DepthTest");
        }

        /// <summary>
        /// Set the current shader.
        /// </summary>
        /// <param name="shader">The shader to set as current.</param>
        public ShaderProgram SetShader(ShaderProgram shader = null)
        {
            FlushRenderStream();
            shader ??= ShaderFactory.DefaultProgram;
            ShaderProgram.EnsureBound(shader.Pointer);

            RenderState currentState = CurrentState;
            currentState.Shader = shader;
            CurrentState = currentState;
            SyncShader();

            return shader;
        }

        /// <summary>
        /// Whether, and where to clip.
        /// </summary>
        /// <param name="clip">The rectangle to clip outside of.</param>
        public void SetClipRect(Rectangle? clip)
        {
            PerfProfiler.FrameEventStart("StateChange: Clip");
            FlushRenderStream();

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

            RenderState currentState = CurrentState;
            currentState.ClipRect = clip;
            CurrentState = currentState;
            PerfProfiler.FrameEventEnd("StateChange: Clip");
        }

        public void SetFaceCulling(bool enabled, bool backFace)
        {
            if (CurrentState.FaceCulling == enabled)
            {
                if (!enabled) return;
                if (CurrentState.FaceCullingBackFace == backFace) return;
            }

            FlushRenderStream();

            if (enabled)
            {
                Gl.Enable(EnableCap.CullFace);
                Gl.FrontFace(FrontFaceDirection.Ccw);
                Gl.CullFace(backFace ? CullFaceMode.Back : CullFaceMode.Front);
            }
            else
            {
                Gl.Disable(EnableCap.CullFace);
            }

            RenderState currentState = CurrentState;
            currentState.FaceCulling = enabled;
            currentState.FaceCullingBackFace = backFace;
            CurrentState = currentState;
        }

        /// <summary>
        /// Set a new state.
        /// </summary>
        /// <param name="inputState">The state to set.</param>
        /// <param name="force">Whether to set it regardless of the previous state.</param>
        public void SetState(RenderState? inputState, bool force = false)
        {
            if (inputState == null) return;
            RenderState newState = inputState.Value;

            FlushRenderStream();
            RenderState currentState = Engine.Renderer.CurrentState;

            // Check which state changes should apply, by checking which were set and which differ from the current.
            PerfProfiler.FrameEventStart("Depth/Stencil/Blend Set");

            if (force || newState.DepthTest != currentState.DepthTest)
                SetDepthTest(newState.DepthTest);

            if (force || newState.StencilTest != currentState.StencilTest)
                SetStencilTest(newState.StencilTest);

            if ((newState.SFactorRgb != currentState.SFactorRgb) ||
                (newState.DFactorRgb != currentState.DFactorRgb) ||
                (newState.SFactorA != currentState.SFactorA) ||
                (newState.DFactorA != currentState.DFactorA))
                SetAlphaBlendType(newState.SFactorRgb, newState.DFactorRgb, newState.SFactorA, newState.DFactorA);

            if (force || newState.AlphaBlending != currentState.AlphaBlending)
                SetAlphaBlend(newState.AlphaBlending);

            PerfProfiler.FrameEventEnd("Depth/Stencil/Blend Set");

            PerfProfiler.FrameEventStart("ShaderSet");

            if (newState.ShaderName != null)
                newState.Shader = RenderState.ResolveFromName(newState.ShaderName);

            if (force || newState.Shader != currentState.Shader)
                SetShader(newState.Shader);

            PerfProfiler.FrameEventEnd("ShaderSet");

            PerfProfiler.FrameEventStart("View/Clip Set");

            if (force || newState.ViewMatrix != currentState.ViewMatrix)
                SetUseViewMatrix(newState.ViewMatrix);

            if (force || newState.ProjectionBehavior != currentState.ProjectionBehavior)
                SetProjectionBehavior(newState.ProjectionBehavior);

            if (force || newState.ClipRect != currentState.ClipRect)
                SetClipRect(newState.ClipRect);

            if (force || newState.FaceCulling != currentState.FaceCulling || newState.FaceCullingBackFace != currentState.FaceCullingBackFace)
                SetFaceCulling(newState.FaceCulling, newState.FaceCullingBackFace);

            PerfProfiler.FrameEventEnd("View/Clip Set");
        }

        /// <summary>
        /// Push a framebuffer to render to on top of the stack. If null then pop the top buffer, reverting to the previous one.
        /// Any subsequent draw calls will draw to the buffer on top of the stack.
        /// </summary>
        public void RenderTo(FrameBuffer buffer)
        {
            FlushRenderStream();
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
            FlushRenderStream();
            Engine.Renderer.PopFramebuffer();
        }

        /// <summary>
        /// Clear whatever is on the currently bound frame buffer.
        /// This is affected by the scissor if any.
        /// </summary>
        public void ClearFrameBuffer()
        {
            FlushRenderStream();
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        /// <summary>
        /// Clears the depth buffer of the currently bound frame buffer.
        /// </summary>
        public void ClearDepth()
        {
            FlushRenderStream();
            Gl.Clear(ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Clears the stencil buffer of the currently bound frame buffer.
        /// This will also reset the stencil state to "StencilStopDraw"
        /// </summary>
        public void ClearStencil()
        {
            FlushRenderStream();
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
            FlushRenderStream();
            _matrixStack.Push(matrix, multiply);
            SyncModelMatrix();
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            FlushRenderStream();
            _matrixStack.Pop();
            SyncModelMatrix();
        }

        #endregion
    }
}
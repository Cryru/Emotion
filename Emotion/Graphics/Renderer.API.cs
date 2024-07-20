#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
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
            Assert(vertices != null);

            for (var i = 0; i < verts.Length; i++)
            {
                vertices[i].Vertex = verts[i].ToVec3();
                vertices[i].Color = i >= colors.Length ? colors.Length == 0 ? Color.WhiteUint : colors[0].ToUint() : colors[i].ToUint();
                vertices[i].UV = Vector2.Zero;
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

        private void StencilStateDefault()
        {
            _StencilMask(0x00);
            _StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
            _StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
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
            _StencilMask(0xFF);
            _StencilFunc(StencilFunction.Always, value, 0xFF);
        }

        /// <summary>
        /// Stops writing to the stencil buffer.
        /// Drawing after this call will not affect the stencil buffer.
        /// This is the default stencil state.
        /// </summary>
        public void StencilStopDraw()
        {
            FlushRenderStream();
            _StencilMask(0x00);
            _StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
        }

        /// <summary>
        /// Rendering after this call will not draw where the stencil buffer is smaller than the threshold value.
        /// This is 0xFF by default - matching the default of StencilStartDraw
        /// </summary>
        public void StencilCutOutFrom(int threshold = 0xFF)
        {
            FlushRenderStream();
            _StencilMask(0x00);
            _StencilFunc(StencilFunction.Greater, threshold, 0xFF);
        }

        /// <summary>
        /// Opposite of StencilCutOutFrom. Only where the value is set will there be drawn.
        /// </summary>
        public void StencilFillIn(int threshold = 0xFF)
        {
            FlushRenderStream();
            _StencilMask(0xFF);
            _StencilFunc(StencilFunction.Lequal, threshold, 0xFF);
        }

        public void StencilMask(int filter = 0xFF)
        {
            FlushRenderStream();
            _StencilMask(0x00);
            _StencilFunc(StencilFunction.Less, filter, 0xFF);
        }

        public void StencilWindingStart()
        {
            FlushRenderStream();
            _StencilMask(0xFF);
            // Each draw inverts the value in the stencil.
            _StencilFunc(StencilFunction.Always, 0, 1);
            _StencilOp(StencilOp.Invert, StencilOp.Invert, StencilOp.Invert);
        }

        public void StencilWindingEnd()
        {
            FlushRenderStream();
            _StencilMask(0xFF);
            // Enable drawing only where the value is 1.
            _StencilFunc(StencilFunction.Equal, 1, 1);
            _StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        }

        private StencilFunction? _stencilFunc;
        private int _stencilFuncRef;
        private uint _stencilFuncMask;

        // Internal state function to reduce redundant state changes.
        private void _StencilFunc(StencilFunction func, int rf, uint mask)
        {
            if (func == _stencilFunc && rf == _stencilFuncRef && _stencilFuncMask == mask) return;

            _stencilFunc = func;
            _stencilFuncRef = rf;
            _stencilFuncMask = mask;
            Gl.StencilFunc(func, rf, mask);
        }

        private StencilOp? _stencilOp1;
        private StencilOp? _stencilOp2;
        private StencilOp? _stencilOp3;

        private void _StencilOp(StencilOp stencilOp1, StencilOp stencilOp2, StencilOp stencilOp3)
        {
            if (_stencilOp1 == stencilOp1 && _stencilOp2 == stencilOp2 && _stencilOp3 == stencilOp3) return;

            _stencilOp1 = stencilOp1;
            _stencilOp2 = stencilOp2;
            _stencilOp3 = stencilOp3;
            Gl.StencilOp(stencilOp1, stencilOp2, stencilOp3);
        }

        private uint _stencilMask;

        private void _StencilMask(uint mask)
        {
            if (_stencilMask == mask) return;

            _stencilMask = mask;
            Gl.StencilMask(mask);
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
            //SyncViewMatrix();
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
            //SyncViewMatrix();
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
                _BlendFuncSeparate(CurrentState.SFactorRgb!.Value, CurrentState.DFactorRgb!.Value, CurrentState.SFactorA!.Value, CurrentState.DFactorA!.Value);
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
            SetAlphaBlendType(defaultState.SFactorRgb!.Value, defaultState.DFactorRgb!.Value, defaultState.SFactorA!.Value, defaultState.DFactorA!.Value);
        }

        private BlendingFactor? _SRgb;
        private BlendingFactor? _DRgb;
        private BlendingFactor? _SA;
        private BlendingFactor? _DA;

        // Internal state function to reduce redundant state changes.
        private void _BlendFuncSeparate(BlendingFactor sRgb, BlendingFactor dRgb, BlendingFactor sA, BlendingFactor dA)
        {
            if (sRgb == _SRgb && dRgb == _DRgb && _SA == sA && _DA == dA) return;

            _SRgb = sRgb;
            _DRgb = dRgb;
            _SA = sA;
            _DA = dA;
            Gl.BlendFuncSeparate(sRgb, dRgb, sA, dA);
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
                _SetDepthFunc(DepthFunction.Lequal);
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

        private DepthFunction? _depthFuncCache;

        // Internal state function to reduce redundant state changes.
        private void _SetDepthFunc(DepthFunction depthFunc)
        {
            if (depthFunc == _depthFuncCache) return;

            _depthFuncCache = depthFunc;
            Gl.DepthFunc(depthFunc);
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
            shader.ClearUniformCache();

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
            if (newState.DepthTest != null && (force || newState.DepthTest != currentState.DepthTest)) SetDepthTest((bool) newState.DepthTest);
            if (newState.StencilTest != null && (force || newState.StencilTest != currentState.StencilTest)) SetStencilTest((bool) newState.StencilTest);
            if ((newState.SFactorRgb != null && newState.SFactorRgb != currentState.SFactorRgb) ||
                (newState.DFactorRgb != null && newState.DFactorRgb != currentState.DFactorRgb) ||
                (newState.SFactorA != null && newState.SFactorA != currentState.SFactorA) ||
                (newState.DFactorA != null && newState.DFactorA != currentState.DFactorA))
                SetAlphaBlendType(
                    newState.SFactorRgb ?? currentState.SFactorRgb!.Value,
                    newState.DFactorRgb ?? currentState.DFactorRgb!.Value,
                    newState.SFactorA ?? currentState.SFactorA!.Value,
                    newState.DFactorA ?? currentState.DFactorA!.Value);
            if (newState.AlphaBlending != null && (force || newState.AlphaBlending != currentState.AlphaBlending)) SetAlphaBlend((bool) newState.AlphaBlending);
            PerfProfiler.FrameEventEnd("Depth/Stencil/Blend Set");

            PerfProfiler.FrameEventStart("ShaderSet");
            if (newState.Shader != null && (force || newState.Shader != currentState.Shader))
                SetShader(newState.Shader);
            PerfProfiler.FrameEventEnd("ShaderSet");

            PerfProfiler.FrameEventStart("View/Clip Set");
            if (force || (newState.ViewMatrix != null && newState.ViewMatrix != currentState.ViewMatrix))
                SetUseViewMatrix((bool) newState.ViewMatrix);
            if (force || (newState.ProjectionBehavior != null && newState.ProjectionBehavior != currentState.ProjectionBehavior))
                SetProjectionBehavior((ProjectionBehavior) newState.ProjectionBehavior);
            if (force || newState.ClipRect != currentState.ClipRect) SetClipRect(newState.ClipRect);
            if (
                    (newState.FaceCulling != null || newState.FaceCullingBackFace != null) &&
                    (force || newState.FaceCulling != currentState.FaceCulling || newState.FaceCullingBackFace != currentState.FaceCullingBackFace)
                )
                SetFaceCulling(
                    newState.FaceCulling ?? currentState.FaceCulling.Value, 
                    newState.FaceCullingBackFace ?? currentState.FaceCullingBackFace.Value
                );
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
            SetShaderDirty();
            //SyncModelMatrix();
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            FlushRenderStream();
            _matrixStack.Pop();
            SetShaderDirty();
            //SyncModelMatrix();
        }

        #endregion
    }
}
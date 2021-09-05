#region Using

using Emotion.Graphics.Shading;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
    public class RenderState
    {
        public bool? AlphaBlending;
        public BlendingFactor? SFactorRgb;
        public BlendingFactor? DFactorRgb;
        public BlendingFactor? SFactorA;
        public BlendingFactor? DFactorA;

        public bool? DepthTest;
        public bool? StencilTest;
        public Rectangle? ClipRect;
        public bool? ViewMatrix;

        public ShaderProgram Shader;

        /// <summary>
        /// A default state.
        /// </summary>
        public static RenderState Default = new RenderState
        {
            DepthTest = true,
            StencilTest = false,
            ClipRect = null,
            AlphaBlending = true,
            SFactorRgb = BlendingFactor.SrcAlpha,
            DFactorRgb = BlendingFactor.OneMinusSrcAlpha,
            SFactorA = BlendingFactor.One,
            DFactorA = BlendingFactor.OneMinusSrcAlpha,
            Shader = ShaderFactory.DefaultProgram,
            ViewMatrix = true
        };

        /// <summary>
        /// Clone the state.
        /// </summary>
        public RenderState Clone()
        {
            return new RenderState
            {
                DepthTest = DepthTest,
                StencilTest = StencilTest,
                ClipRect = ClipRect,
                AlphaBlending = AlphaBlending,
                SFactorRgb = SFactorRgb,
                DFactorRgb = DFactorRgb,
                SFactorA = SFactorA,
                DFactorA = DFactorA,
                Shader = Shader,
                ViewMatrix = ViewMatrix
            };
        }
    }
}
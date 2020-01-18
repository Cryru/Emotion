#region Using

using Emotion.Graphics.Shading;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class RenderState
    {
        public bool? AlphaBlending;
        public bool? DepthTest;
        public bool? StencilTest;
        public Rectangle? ClipRect;
        public bool? ViewMatrix;

        public ShaderProgram Shader;

        /// <summary>
        /// A default state.
        /// </summary>
        public static RenderState Default()
        {
            return new RenderState
            {
                DepthTest = true,
                StencilTest = false,
                ClipRect = null,
                AlphaBlending = true,
                Shader = ShaderFactory.DefaultProgram,
                ViewMatrix = true
            };
        }

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
                Shader = Shader,
                ViewMatrix = ViewMatrix
            };
        }
    }
}
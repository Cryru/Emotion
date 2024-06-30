#region Using

using Emotion.Graphics.Shading;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// The kind of projection matrix to use.
    /// </summary>
    public enum ProjectionBehavior
    {
        /// <summary>
        /// When the view matrix is enabled, the camera's projection is used,
        /// otherwise the default 2d projection is used.
        /// </summary>
        AutoCamera,

        /// <summary>
        /// Always use the default 2d projection.
        /// </summary>
        AlwaysDefault2D,

        /// <summary>
        /// Always use the camera projection, even if disabled.
        /// </summary>
        AlwaysCameraProjection
    }

    public struct RenderState
    {
        public bool? AlphaBlending;
        public BlendingFactor? SFactorRgb;
        public BlendingFactor? DFactorRgb;
        public BlendingFactor? SFactorA;
        public BlendingFactor? DFactorA;

        public bool? DepthTest;
        public bool? StencilTest;
        public Rectangle? ClipRect;
        public bool? FaceCulling;
        public bool? FaceCullingBackFace;
        public bool? ViewMatrix;
        public ProjectionBehavior? ProjectionBehavior;

        public ShaderProgram Shader;

        /// <summary>
        /// A default state.
        /// </summary>
        public static RenderState Default;

        public static void CreateDefault()
        {
            Default = new RenderState
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
                ViewMatrix = true,
                ProjectionBehavior = Graphics.ProjectionBehavior.AutoCamera
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
                SFactorRgb = SFactorRgb,
                DFactorRgb = DFactorRgb,
                SFactorA = SFactorA,
                DFactorA = DFactorA,
                Shader = Shader,
                ViewMatrix = ViewMatrix,
                ProjectionBehavior = ProjectionBehavior
            };
        }
    }
}
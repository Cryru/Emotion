#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using Emotion.Standard.Reflector;
using OpenGL;

#endregion

namespace Emotion.Graphics;

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

[ReflectorGenerateStructPerMemberHelpers]
public partial struct RenderState
{
    public static RenderState Default = new RenderState();

    public bool AlphaBlending = true;

    public BlendingFactor SFactorRgb = BlendingFactor.SrcAlpha;
    public BlendingFactor DFactorRgb = BlendingFactor.OneMinusSrcAlpha;
    public BlendingFactor SFactorA = BlendingFactor.One;
    public BlendingFactor DFactorA = BlendingFactor.OneMinusSrcAlpha;

    public bool DepthTest = true;
    public bool StencilTest = false;
    public Rectangle? ClipRect = null;
    public bool FaceCulling = false;
    public bool FaceCullingBackFace = false;
    public bool ViewMatrix = true;
    public ProjectionBehavior ProjectionBehavior = Graphics.ProjectionBehavior.AutoCamera;

    public ShaderReference Shader = ShaderReference.Invalid;

    public RenderState()
    {

    }
}
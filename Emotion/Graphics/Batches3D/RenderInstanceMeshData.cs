#nullable enable

#region Using

using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Graphics.Batches3D;

// Per mesh
public struct RenderInstanceMeshData
{
    public int ObjectRegistrationId; // -> RenderInstanceObjectData
    public Matrix4x4[]? BoneData;
    public int NextMesh;
}

// Per mesh (transparent mode)
public struct RenderInstanceMeshDataTransparent
{
    public Mesh Mesh;
    public ShaderProgram Shader;
    public bool UploadMetaStateToShader;

    public int ObjectRegistrationId; // -> RenderInstanceObjectData
    public Matrix4x4[]? BoneData;
}
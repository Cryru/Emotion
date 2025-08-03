#nullable enable

#region Using

using Emotion.Graphics.Shading;
using Emotion.Standard.Memory;

#endregion

namespace Emotion.Graphics.Batches3D;

/// <summary>
/// Contains all instances batches that use the same pipeline state
/// </summary>
public struct MeshRenderPipelineStateGroup
{
    public bool UploadMetaStateToShader;
    public ShaderProgram Shader;

    public StructArenaLinkedList<MeshRenderMeshBatch> MeshRenderBatchList;
}
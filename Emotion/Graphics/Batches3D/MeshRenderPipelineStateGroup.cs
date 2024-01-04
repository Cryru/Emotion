#nullable enable

#region Using

using Emotion.Graphics.Shading;

#endregion

namespace Emotion.Graphics.Batches3D
{
    /// <summary>
    /// Contains all instances batches in a specific pass that use the same pipeline state
    /// </summary>
    public struct MeshRenderPipelineStateGroup
    {
        public bool UploadMetaStateToShader;
        public ShaderProgram Shader;

        public int MeshRenderBatchLL_Start; // MeshRenderMeshBatch
        public int MeshRenderBatchLL_End;
    }
}
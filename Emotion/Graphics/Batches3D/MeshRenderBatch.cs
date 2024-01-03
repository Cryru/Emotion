#nullable enable

#region Using

using Emotion;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Graphics.Batches3D
{
    // Collects all instances of mesh + shader combinations
    public struct MeshRenderBatch
    {
        public RenderPass RenderPass;
        public Mesh Mesh;
        public int MeshDataLL_Start;
        public int MeshDataLL_End;

        public bool UploadMetaStateToShader;
        public ShaderProgram Shader;
    }
}
#nullable enable

#region Using

using Emotion;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Graphics.Batches3D
{
    // Collects all instances of mesh in a specific pass
    public struct MeshRenderMeshBatch
    {
        public Mesh Mesh;
        public int NextRenderBatch;

        public int MeshInstanceLL_Start; // RenderInstanceMeshData
        public int MeshInstanceLL_End;
    }
}
#nullable enable

#region Using


#endregion

namespace Emotion.Graphics.Batches3D
{
    // Per mesh
    public struct RenderInstanceMeshData
    {
        public int ObjectRegistrationId; // -> RenderInstanceObjectData
        public Matrix4x4[]? BoneData;
        public int NextMesh;
    }
}
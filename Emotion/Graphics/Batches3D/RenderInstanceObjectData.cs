#nullable enable

#region Using

using Emotion.Game.ThreeDee;

#endregion

namespace Emotion.Graphics.Batches3D
{
    // Per entity
    public struct RenderInstanceObjectData
    {
        public bool BackfaceCulling;
        public MeshEntityMetaState MetaState;
        public Matrix4x4 ModelMatrix;
        public float DistanceToCamera;
    }
}
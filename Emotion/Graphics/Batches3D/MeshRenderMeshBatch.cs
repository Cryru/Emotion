#nullable enable

#region Using

using Emotion.Game.World.ThreeDee;
using Emotion.Standard.Memory;

#endregion

namespace Emotion.Graphics.Batches3D;

// Collects all instances of mesh in a specific pass
public struct MeshRenderMeshBatch : IStructArenaLinkedListItem
{
    public int NextItem { get; set; }

    public Mesh Mesh;
    public StructArenaLinkedList<RenderInstanceMeshData> MeshInstanceList;
}
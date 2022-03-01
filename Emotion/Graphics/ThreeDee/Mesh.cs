using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;

#nullable enable

namespace Emotion.Graphics.ThreeDee
{
    /// <summary>
    /// 3D geometry and material that makes up a 3D object.
    /// </summary>
    public class Mesh
    {
        public string Name = null!;
        public MeshMaterial Material = null!;

        /// <summary>
        /// One of these must be present, but not both.
        /// </summary>
        public VertexData[]? Vertices;
        public VertexDataWithBones[]? VerticesWithBones;

        public ushort[] Indices = null!;
        public MeshBone[]? Bones = null;
    }
}
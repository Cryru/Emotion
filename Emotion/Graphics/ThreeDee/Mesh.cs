using Emotion.Graphics.Data;

namespace Emotion.Graphics.ThreeDee
{
    /// <summary>
    /// 3D geometry and material that makes up a 3D object.
    /// </summary>
    public class Mesh
    {
        public string Name;

        public VertexData[] Vertices;
        public ushort[] Indices;

        public MeshMaterial Material;
    }
}
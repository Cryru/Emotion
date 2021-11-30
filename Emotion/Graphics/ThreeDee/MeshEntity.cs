namespace Emotion.Graphics.ThreeDee
{
    /// <summary>
    /// A collection of meshes which make up one visual object.
    /// Not all of the meshes are always visible.
    /// </summary>
    public class MeshEntity
    {
        public string Name { get; set; }
        public Mesh[] Meshes { get; set; }
    }
}
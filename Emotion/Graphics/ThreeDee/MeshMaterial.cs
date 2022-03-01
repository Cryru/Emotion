#region Using

using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Graphics.ThreeDee
{
    /// <summary>
    /// Settings description of how to render a 3D object.
    /// </summary>
    public class MeshMaterial
    {
        public string Name = null!;
        public Color DiffuseColor = Color.White;
        public string? DiffuseTextureName = null;
        public Texture? DiffuseTexture;

        public static MeshMaterial DefaultMaterial = new MeshMaterial();
    }
}
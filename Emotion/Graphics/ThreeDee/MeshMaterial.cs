#region Using

using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.ThreeDee
{
    /// <summary>
    /// Settings description of how to render a 3D object.
    /// </summary>
    public class MeshMaterial
    {
        public string Name;
        public Texture DiffuseTexture;
        public Color DiffuseColor = Color.White;

        public static MeshMaterial DefaultMaterial = new MeshMaterial();
    }
}
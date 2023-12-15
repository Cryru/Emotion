#region Using

using Emotion.Editor;
using Emotion.Graphics.Objects;
using Emotion.IO;

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
        public Color DiffuseColor = Color.White; // todo: Currently unused

        [AssetFileName<TextureAsset>] public string? DiffuseTextureName = null;
        public Texture? DiffuseTexture;

        public bool BackFaceCulling = true;

        public static MeshMaterial DefaultMaterial = new MeshMaterial
        {
            Name = "Default"
        };
    }
}
#region Using

using System.Numerics;
using Emotion.IO;
using Emotion.Standard.Image;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Effects
{
    /// <summary>
    /// Used to load the texture and get a palette map.
    /// </summary>
    public class PaletteBaseTexture : TextureAsset
    {
        public byte[] PaletteMap;

        protected override void UploadTexture(Vector2 size, byte[] bgraPixels, bool flipped)
        {
            PaletteMap = ImageUtil.GeneratePaletteMap(bgraPixels, out _);
            base.UploadTexture(size, bgraPixels, flipped);
        }
    }
}
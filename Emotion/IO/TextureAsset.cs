﻿#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Image.BMP;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Represents an image. Supports all formats supported by Emotion.Standard.Image
    /// </summary>
    public class TextureAsset : Asset
    {
        /// <summary>
        /// The asset's uploaded graphics texture.
        /// </summary>
        public Texture Texture { get; set; }

        protected override void CreateInternal(byte[] data)
        {
            byte[] pixels = null;
            var width = 0;
            var height = 0;
            var flipped = false;

            // Check if PNG.
            if (PngFormat.IsPng(data))
            {
                pixels = PngFormat.Decode(data, out PngFileHeader header);
                width = header.Width;
                height = header.Height;
                flipped = true;
            }
            // Check if BMP.
            else if (BmpFormat.IsBmp(data))
            {
                pixels = BmpFormat.Decode(data, out BmpFileHeader header);
                width = header.Width;
                height = header.Height;
            }

            if (pixels == null || width == 0 || height == 0)
            {
                Engine.Log.Warning($"Couldn't load texture - {Name}.", MessageSource.AssetLoader);
                return;
            }

            UploadTexture(new Vector2(width, height), pixels, flipped);
        }

        protected virtual void UploadTexture(Vector2 size, byte[] bgraPixels, bool flipped)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Texture = new Texture(size, bgraPixels);
                if (flipped) Texture.TextureMatrix = Matrix4x4.CreateScale(1, -1, 1);
            });
        }

        protected override void DisposeInternal()
        {
            Texture.Dispose();
            Texture = null;
        }
    }
}
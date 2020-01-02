using System.IO;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Image;
using Emotion.Standard.Image.BMP;
using Emotion.Standard.Image.PNG;
using Emotion.Test;
using Emotion.Utility;
using Tests.Results;

namespace Tests.Classes
{
    [Test("StandardImage")]
    public class StandardImageTests
    {
        [Test]
        public void DecodeBmp16()
        {
            string bmp16 = Path.Join("Assets", "Images", "16colorbmp.bmp");
            byte[] bytes = File.ReadAllBytes(bmp16);
            Assert.True(bytes != null);
            Assert.True(BmpFormat.IsBmp(bytes));

            byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
            Assert.True(decodedPixelData != null);

            Runner.ExecuteAsLoop(_ =>
            {
                var r = new Texture(
                    new Vector2(fileHeader.Width, fileHeader.Height),
                    decodedPixelData
                );

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, r);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.Bmp16ColorDecode);
                r.Dispose();
            }).WaitOne();
        }

        [Test]
        public void DecodeBmp24()
        {
            string bmp24 = Path.Join("Assets", "Images", "24bitbmp.bmp");
            byte[] bytes = File.ReadAllBytes(bmp24);
            Assert.True(bytes != null);
            Assert.True(BmpFormat.IsBmp(bytes));

            byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
            Assert.True(decodedPixelData != null);

            Runner.ExecuteAsLoop(_ =>
            {
                var r = new Texture(
                    new Vector2(fileHeader.Width, fileHeader.Height),
                    decodedPixelData
                );

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, r);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.Bmp24BitDecode);
                r.Dispose();
            }).WaitOne();
        }

        [Test]
        public void DecodeBmp256()
        {
            string bmp256 = Path.Join("Assets", "Images", "256colorbmp.bmp");
            byte[] bytes = File.ReadAllBytes(bmp256);
            Assert.True(bytes != null);
            Assert.True(BmpFormat.IsBmp(bytes));

            byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
            Assert.True(decodedPixelData != null);

            Runner.ExecuteAsLoop(_ =>
            {
                var r = new Texture(
                    new Vector2(fileHeader.Width, fileHeader.Height),
                    decodedPixelData
                );

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, r);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.Bmp256ColorDecode);
                r.Dispose();
            }).WaitOne();
        }

        [Test]
        public void DecodePng()
        {
            string png = Path.Join("Assets", "Images", "standardPng.png");
            byte[] bytes = File.ReadAllBytes(png);
            Assert.True(bytes != null);
            Assert.True(PngFormat.IsPng(bytes));

            byte[] decodedPixelData = PngFormat.Decode(bytes, out PngFileHeader fileHeader);
            Assert.True(decodedPixelData != null);

            // PNG images are flipped.
            ImageUtil.FlipImageY(decodedPixelData, fileHeader.Height);

            Runner.ExecuteAsLoop(_ =>
            {
                var r = new Texture(
                    new Vector2(fileHeader.Width, fileHeader.Height),
                    decodedPixelData
                );

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, r);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.PngDecode);
                r.Dispose();
            }).WaitOne();
        }

        [Test]
        public void DecodePngInterlaced()
        {
            string png = Path.Join("Assets", "Images", "spritesheetAnimation.png");
            byte[] bytes = File.ReadAllBytes(png);
            Assert.True(bytes != null);
            Assert.True(PngFormat.IsPng(bytes));

            byte[] decodedPixelData = PngFormat.Decode(bytes, out PngFileHeader fileHeader);
            Assert.True(decodedPixelData != null);
            Assert.True(fileHeader.InterlaceMethod == 1);

            // PNG images are flipped.
            ImageUtil.FlipImageY(decodedPixelData, fileHeader.Height);

            Runner.ExecuteAsLoop(_ =>
            {
                var r = new Texture(
                    new Vector2(fileHeader.Width, fileHeader.Height),
                    decodedPixelData
                );

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, r);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.PngDecodeInterlaced);
                r.Dispose();
            }).WaitOne();
        }
    }
}
#region Using

using System.Collections;
using System.IO;
using System.Numerics;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Image.BMP;
using Emotion.Standard.Image.PNG;
using Emotion.Testing;
using OpenGL;

#endregion

namespace Tests.EngineTests;

public class StandardImageTests : ProxyRenderTestingScene
{
    [Test]
    public IEnumerator DecodeBmp16()
    {
        string bmp16 = Path.Join("Assets", "Images", "16colorbmp.bmp");
        byte[] bytes = File.ReadAllBytes(bmp16);
        Assert.True(BmpFormat.IsBmp(bytes));

        byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
        Assert.True(decodedPixelData != null);

        Texture texture = null;

        ToRender = (composer) =>
        {
            texture = new Texture(
                new Vector2(fileHeader.Width, fileHeader.Height),
                decodedPixelData,
                PixelFormat.Bgra
            )
            { FlipY = true };

            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, texture);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot();

        texture.Dispose();
    }

    [Test]
    public IEnumerator DecodeBmp24()
    {
        string bmp24 = Path.Join("Assets", "Images", "24bitbmp.bmp");
        byte[] bytes = File.ReadAllBytes(bmp24);
        Assert.True(BmpFormat.IsBmp(bytes));

        byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
        Assert.True(decodedPixelData != null);

        Texture texture = null;

        ToRender = (composer) =>
        {
            texture = new Texture(
                new Vector2(fileHeader.Width, fileHeader.Height),
                decodedPixelData,
                PixelFormat.Bgra
            )
            { FlipY = true };

            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, texture);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot();

        texture.Dispose();
    }

    [Test]
    public IEnumerator DecodeBmp256()
    {
        string bmp256 = Path.Join("Assets", "Images", "256colorbmp.bmp");
        byte[] bytes = File.ReadAllBytes(bmp256);
        Assert.True(BmpFormat.IsBmp(bytes));

        byte[] decodedPixelData = BmpFormat.Decode(bytes, out BmpFileHeader fileHeader);
        Assert.True(decodedPixelData != null);

        Texture texture = null;

        ToRender = (composer) =>
        {
            texture = new Texture(
                new Vector2(fileHeader.Width, fileHeader.Height),
                decodedPixelData,
                PixelFormat.Bgra
            )
            { FlipY = true };

            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, new Vector2(fileHeader.Width, fileHeader.Height), Color.White, texture);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot();

        texture.Dispose();
    }

    [Test]
    public IEnumerator DecodePng()
    {
        string png = Path.Join("Assets", "Images", "standardPng.png");
        byte[] bytes = File.ReadAllBytes(png);
        Assert.True(PngFormat.IsPng(bytes));

        byte[] decodedPixelData = PngFormat.Decode(bytes, out PngFileHeader fileHeader);
        Assert.True(decodedPixelData != null);

        Texture texture = null;

        ToRender = (composer) =>
        {
            texture = new Texture(
                fileHeader.Size,
                decodedPixelData,
                fileHeader.PixelFormat);

            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, fileHeader.Size, Color.White, texture);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot();

        texture.Dispose();
    }

    [Test]
    public IEnumerator DecodePngInterlaced()
    {
        string png = Path.Join("Assets", "Images", "spritesheetAnimation.png");
        byte[] bytes = File.ReadAllBytes(png);
        Assert.True(PngFormat.IsPng(bytes));

        byte[] decodedPixelData = PngFormat.Decode(bytes, out PngFileHeader fileHeader);
        Assert.True(decodedPixelData != null);
        Assert.True(fileHeader.InterlaceMethod == 1);

        Texture texture = null;

        ToRender = (composer) =>
        {
            texture = new Texture(
                fileHeader.Size,
                decodedPixelData,
                fileHeader.PixelFormat);

            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, fileHeader.Size, Color.White, texture);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot();

        texture.Dispose();
    }
}
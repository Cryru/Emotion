#region Using

using System;
using System.Collections;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Testing;
using OpenGL;

#endregion

namespace Tests.EngineTests;

public class BasicTest : ProxyRenderTestingScene
{
    [Test]
    public IEnumerator EngineSetup()
    {
        Assert.True(Engine.Host != null);
        Assert.True(Engine.Renderer != null);
        Assert.True(Engine.Status == EngineStatus.Running);

        yield break;
    }

    [Test]
    public IEnumerator ClearColorAndBasicRender()
    {
        ToRender = (composer) =>
        {

        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(BasicTest), nameof(ClearColorAndBasicRender));
    }

    /// <summary>
    /// Ensure data buffer mapping is correct.
    /// </summary>
    [Test]
    public IEnumerator DataBufferTest()
    {
        ToRender = (composer) =>
        {
            var buffer = new DataBuffer(BufferTarget.ArrayBuffer);
            buffer.Upload(nint.Zero, 12);
            Span<float> mapper = buffer.CreateMapper<float>();
            mapper[0] = 50;
            mapper[2] = 0.1f;
            float[] data = mapper.ToArray();
            buffer.FinishMapping();

            Assert.True(data.Length == 3);
            Assert.True(data[0] == 50);
            Assert.True(data[1] == 0);
            Assert.True(data[2] == 0.1f);

            // Re-read memory.
            mapper = buffer.CreateMapper<float>();
            data = mapper.ToArray();

            Assert.True(data.Length == 3);
            Assert.True(data[0] == 50);
            Assert.True(data[1] == 0);
            Assert.True(data[2] == 0.1f);

            buffer.Dispose();

            Assert.True(buffer.Pointer == 0);
        };

        yield return new TestWaiterRunLoops(1);
    }
}
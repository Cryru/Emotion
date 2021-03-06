﻿#region Using

using System;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Test;
using OpenGL;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    [Test]
    public class BasicTest
    {
        [Test]
        public void EngineSetup()
        {
            Assert.True(Engine.Host != null);
            Assert.True(Engine.Renderer != null);
            Assert.True(Engine.Status == EngineStatus.Running);
        }

        [Test]
        public void ClearColorAndBasicRender()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                Engine.Renderer.StartFrame();
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.ClearColorBasicRender);
            }).WaitOne();
        }

        /// <summary>
        /// Ensure data buffer mapping is correct.
        /// </summary>
        [Test]
        public void DataBufferTest()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                var buffer = new DataBuffer(BufferTarget.ArrayBuffer);
                buffer.Upload(IntPtr.Zero, 12);
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
            }).WaitOne();
        }
    }
}
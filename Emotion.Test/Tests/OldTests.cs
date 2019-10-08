namespace Emotion.Test.Tests
{
    public static class OldTests
    {
        // ------------------------/------------------------/------------------------/
        // This test was used to determine which way of mapping DataBuffers is fastest.
        // Mapping using a span is about 5-10 times faster.
        // ------------------------/------------------------/------------------------/
        //#pragma warning disable 649
        //        private struct TestStruct
        //        {
        //            public float X;
        //            public float Y;
        //            public float Z;

        //            public uint PackedColor;
        //        }
        //#pragma warning restore 649

        //        [Test]
        //        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        //        public unsafe void DataBufferPerformance()
        //        {
        //            const int iterations = 10000;
        //            const int writeAmount = 500;

        //            Runner.ExecuteAsLoop(_ =>
        //            {
        //                int memorySize = (sizeof(TestStruct) * writeAmount);
        //                DataBuffer buffer = new DataBuffer(OpenGL.BufferTarget.ArrayBuffer);
        //                buffer.Upload(IntPtr.Zero, (uint)memorySize);

        //                MemoryStream str = new MemoryStream(memorySize);

        //                long averageTickNormalMap = Runner.PerformanceTest(iterations, () =>
        //                {
        //                    buffer.SetMapPosition(0);
        //                    for (int j = 0; j < writeAmount; j++)
        //                    {
        //                        buffer.Map(new TestStruct() { X = j });
        //                    }
        //                });

        //                buffer.ReadMemory(str);
        //                byte[] dataNormalMap = str.ToArray();

        //                long averageTickMapperMap = Runner.PerformanceTest(iterations, () =>
        //                {
        //                    Span<TestStruct> mapper = buffer.CreateMapper<TestStruct>(0);

        //                    for (int j = 0; j < writeAmount; j++)
        //                    {
        //                        mapper[j] = new TestStruct() { X = j };
        //                    }
        //                });

        //                buffer.ReadMemory(str);
        //                byte[] dataMapperMap = str.ToArray();

        //                Assert.True(dataNormalMap.Length == dataMapperMap.Length);
        //                Assert.True(averageTickNormalMap > averageTickMapperMap);
        //                for (int i = 0; i < dataNormalMap.Length; i++)
        //                {
        //                    Assert.True(dataNormalMap[i] == dataMapperMap[i]);
        //                }

        //                buffer.FinishMapping();
        //                buffer.Dispose();
        //                Assert.True(buffer.Pointer == 0);
        //            }).WaitOne();
        //        }
    }
}
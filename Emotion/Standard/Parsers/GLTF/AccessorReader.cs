#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Standard.GLTF;

public static partial class GLTFFormat
{
    public struct AccessorReader<T> where T : unmanaged
    {
        public ReadOnlyMemory<byte> Data;

        public Func<ReadOnlyMemory<byte>, bool, float> ReaderFunc;

        public int ComponentCount;

        public int ComponentSize;

        public int Stride;

        public int Count;

        public bool Normalized;

        public static AccessorReader<T> Empty = new AccessorReader<T>()
        {
            Data = ReadOnlyMemory<byte>.Empty,
            Count = 0
        };

        public unsafe T ReadElement(int index)
        {
            if (index >= Count) return default;

            ReadOnlyMemory<byte> elementData = Data.Slice(Stride * index, ComponentSize * ComponentCount);

            Span<float> floatDataSpan = stackalloc float[ComponentCount];
            for (int i = 0; i < ComponentCount; i++)
            {
                float value = ReaderFunc(elementData.Slice(i * ComponentSize), Normalized);
                floatDataSpan[i] = value;
            }

            var returnValueSpan = MemoryMarshal.Cast<float, T>(floatDataSpan);
            return returnValueSpan[0];
        }
    }
}

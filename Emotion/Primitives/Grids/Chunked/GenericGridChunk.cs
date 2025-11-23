#nullable enable

using Emotion.Core.Systems.IO;
using System.Runtime.InteropServices;

namespace Emotion.Primitives.Grids.Chunked;

public class GenericGridChunk<T> : IGridChunk<T> where T : unmanaged, IEquatable<T>
{
    protected T[] _data = Array.Empty<T>();

    public bool IsEmpty()
    {
        T defValue = default;
        for (int i = 0; i < _data.Length; i++)
        {
            T value = _data[i];
            if (!defValue.Equals(value)) return false;
        }
        return true;
    }

    public int GetNonEmptyCount()
    {
        T defValue = default;
        int nonEmpty = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            T value = _data[i];
            if (!defValue.Equals(value)) nonEmpty++;
        }
        return nonEmpty;
    }

    public T[] GetRawData()
    {
        return _data;
    }

    public void SetRawData(T[] data)
    {
        _data = data;
    }

    public unsafe bool _Save(string fileName)
    {
        T[] data = _data;
        Span<T> span = new Span<T>(data);
        Span<byte> spanAsBytes = MemoryMarshal.Cast<T, byte>(span);
        return Engine.AssetLoader.Save($"{fileName}.bin", spanAsBytes);
    }

    public IEnumerator _LoadRoutine(string fileName, Asset asset)
    {
        OtherAsset binData = Engine.AssetLoader.Get<OtherAsset>(fileName, asset, false, true, true);
        yield return binData;

        // todo: copying this is a bit cringe...
        ReadOnlySpan<byte> span = binData.Content.Span;
        ReadOnlySpan<T> spanAsT = MemoryMarshal.Cast<byte, T>(span);
        _data = spanAsT.ToArray();
    }
}

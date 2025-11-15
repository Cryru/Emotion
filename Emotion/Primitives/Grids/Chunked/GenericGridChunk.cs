#nullable enable

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

    public unsafe void _Save(string fileName)
    {
        T[] data = _data;
        int dataLength = sizeof(T) * _data.Length;
        byte[] dataTemp = new byte[dataLength];
        fixed (T* pData = &data[0])
        {
            Marshal.Copy((nint)pData, dataTemp, 0, dataLength);
        }
        //MemoryMarshal.AsBytes<T>(_data);
        Engine.AssetLoader.SaveDevMode(dataTemp, $"{fileName}.bin", false);
    }
}

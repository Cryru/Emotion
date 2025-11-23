#nullable enable

using Emotion.Core.Systems.IO;

namespace Emotion.Primitives.Grids.Chunked;

public interface IGridChunk<T> where T : struct
{
    public bool IsEmpty();

    public int GetNonEmptyCount();

    public T[] GetRawData();

    public void SetRawData(T[] data);

    public bool _Save(string fileName);

    public IEnumerator _LoadRoutine(string fileName, Asset asset);
}
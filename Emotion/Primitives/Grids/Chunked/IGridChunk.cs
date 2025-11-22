#nullable enable

namespace Emotion.Primitives.Grids.Chunked;

public interface IGridChunk<T> where T : struct
{
    public bool IsEmpty();

    public int GetNonEmptyCount();

    public T[] GetRawData();

    public void SetRawData(T[] data);

    public void _Save(string fileName);

    public IEnumerator _LoadRoutine(string fileName);
}
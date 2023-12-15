namespace Emotion.Graphics.Batches;

public ref struct StreamData<T> // This is the struct returned to API users that request stream memory.
{
    public Span<T> VerticesData;
    public Span<ushort> IndicesData;

    /// <summary>
    /// Index offset for batching draw calls. Needs to be added to all indices.
    /// todo: make the batch do this rather than the user.
    /// </summary>
    public ushort StructIndex;
}
#nullable enable

namespace Emotion.Primitives.Grids;

public interface IMutableGrid<T>
{
    public void Resize(Vector2 newSize, Func<int, int, T>? initNewDataFunc = null);

    public void Offset(Vector2 offset, bool wrapAround, Action<int, int, T>? onOffset = null);

    public bool Compact(T compactValue, out Vector2 compactOffset);
}

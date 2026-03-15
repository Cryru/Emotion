#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Standard.Extensions;

public static class ListExtensions
{
    public static T? SafelyGet<T>(this IList<T> list, int idx)
    {
        if (idx < 0 || idx >= list.Count) return default;
        return list[idx];
    }

    public static int GetIndexOf<T>(this IReadOnlyList<T> list, T item)
        where T : class
    {
        int index = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (item == list[i])
                index = i;
        }
        return index;
    }

    public static ReadOnlySpan<T> AsSpan<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list);
    }
}
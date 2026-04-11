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

    public static void SetAll<T>(this IList<T> list, T val)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = val;
        }
    }

    public static T GetMax<T>(this IList<T> list)
        where T : INumber<T>
    {
        T maxNum = T.Zero;
        for (int i = 0; i < list.Count; i++)
        {
            maxNum = T.Max(maxNum, list[i]);
        }
        return maxNum;
    }

    public static T GetSum<T>(this IList<T> list, int count = -1)
        where T : INumber<T>
    {
        if (count == -1) count = list.Count;

        T sum = T.Zero;
        for (int i = 0; i < count; i++)
        {
            sum = sum + list[i];
        }
        return sum;
    }

    public static void Reset<T>(this IList<T?> list, int count)
    {
        list.Clear();
        for (int i = 0; i < count; i++)
        {
            list.Add(default);
        }
    }

    public static void RemoveAtSwapBack<T>(this IList<T?> list, int idx)
    {
        list[idx] = list[^1];
        list.RemoveAt(list.Count - 1);
    }
}
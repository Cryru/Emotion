#nullable enable

using System.Text;

namespace System;

public static class MiscExtensions
{
    public static T? SafelyGet<T>(this IList<T> list, int idx)
    {
        if (idx < 0 || idx >= list.Count) return default;
        return list[idx];
    }
}
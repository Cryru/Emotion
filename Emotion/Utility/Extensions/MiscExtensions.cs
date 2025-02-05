#nullable enable

using System.Text;

namespace System;

public static class MiscExtensions
{
    /// <summary>
    /// Returns a friendly display name for the type.
    /// Handles generic arguments.
    /// </summary>
    public static string GetFriendlyName(this Type type)
    {
        if (!type.IsGenericType) return type.Name;

        Type[] genericArguments = type.GetGenericArguments();
        var genericArgumentsTogether = new StringBuilder();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            if (i != 0) genericArgumentsTogether.Append(", ");
            genericArgumentsTogether.Append(genericArguments[i].Name);
        }

        int genericStart = type.Name.IndexOf("`", StringComparison.Ordinal);
        return $"{type.Name[..genericStart]}<{genericArgumentsTogether}>";
    }
}
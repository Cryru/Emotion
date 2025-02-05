#nullable enable

namespace System;

public static class EnumExtensions
{
    /// <summary>
    /// Get all enum flags.
    /// </summary>
    public static IEnumerable<T> GetEnumFlags<T>(this Enum flags) where T : Enum
    {
        foreach (Enum value in Enum.GetValues(flags.GetType()))
        {
            if (flags.HasFlag(value)) yield return (T)value;
        }
    }

    public unsafe static bool EnumHasFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
    {
        switch (sizeof(TEnum))
        {
            case 1:
                return (*(byte*)&lhs & *(byte*)&rhs) > 0;
            case 2:
                return (*(ushort*)&lhs & *(ushort*)&rhs) > 0;
            case 4:
                return (*(uint*)&lhs & *(uint*)&rhs) > 0;
            case 8:
                return (*(ulong*)&lhs & *(ulong*)&rhs) > 0;
            default:
                return false;
        }
    }

    public unsafe static TEnum EnumRemoveFlag<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
    {
        switch (sizeof(TEnum))
        {
            case 1:
                {
                    TEnum retVal = lhs;
                    byte* retValPtr = (byte*)&retVal;
                    *retValPtr = (byte)(*(byte*)&lhs & ~*(byte*)&rhs);
                    return retVal;
                }
            case 2:
                {
                    TEnum retVal = lhs;
                    ushort* retValPtr = (ushort*)&retVal;
                    *retValPtr = (ushort)(*(ushort*)&lhs & ~*(ushort*)&rhs);
                    return retVal;
                }
            case 4:
                {
                    TEnum retVal = lhs;
                    uint* retValPtr = (uint*)&retVal;
                    *retValPtr = (uint)(*(uint*)&lhs & ~*(uint*)&rhs);
                    return retVal;
                }
            case 8:
                {
                    TEnum retVal = lhs;
                    ulong* retValPtr = (ulong*)&retVal;
                    *retValPtr = (ulong)(*(ulong*)&lhs & ~*(ulong*)&rhs);
                    return retVal;
                }
        }

        return lhs;
    }
}
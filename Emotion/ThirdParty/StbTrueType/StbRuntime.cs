#region Using

using Emotion.Standard.Memory;
using System.Runtime.InteropServices;

#endregion

namespace StbTrueTypeSharp;

internal static unsafe class StbRuntime
{
    public static void* malloc(int size)
    {
        var ptr = Marshal.AllocHGlobal(size);
        byte* bytePtr = (byte*)ptr.ToPointer();

        for (int i = 0; i < size; i++)
        {
            bytePtr[i] = 0;
        }

        return bytePtr;
    }

    public static void free(void* a)
    {
        if (a == null)
            return;

        var ptr = new nint(a);
        Marshal.FreeHGlobal(ptr);
    }

    public static void memcpy(void* to, void* from, ulong size)
    {
        NativeMemory.Copy(from, to, (nuint)size);
    }

    public static void memclear(void* ptr, int size)
    {
        NativeMemory.Clear(ptr, (nuint) size);
    }

    public static void memclear(void* ptr, uint size)
    {
        NativeMemory.Clear(ptr, size);
    }

    public static double pow(double a, double b)
    {
        return Math.Pow(a, b);
    }

    public static float ceil(float a)
    {
        return MathF.Ceiling(a);
    }

    public static float floor(float a)
    {
        return MathF.Floor(a);
    }

    public static float cos(float value)
    {
        return MathF.Cos(value);
    }

    public static float acos(float value)
    {
        return MathF.Acos(value);
    }

    public static float sin(float value)
    {
        return MathF.Sin(value);
    }

    public static float sqrt(float val)
    {
        return MathF.Sqrt(val);
    }

    public static float fmod(float x, float y)
    {
        return x % y;
    }

    public static ulong strlen(sbyte* str)
    {
        var ptr = str;

        while (*ptr != '\0')
            ptr++;

        return (ulong)ptr - (ulong)str - 1;
    }
}
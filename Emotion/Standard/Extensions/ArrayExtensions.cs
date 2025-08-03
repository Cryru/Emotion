#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Standard.Extensions;

public static class ArrayExtensions
{
    /// <summary>
    /// Creates a new array with the element added.
    /// This is worse than using a list and it will copy the array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AddToArray<T>(this T[] array, T element, bool front = false)
    {
        var newArray = new T[array.Length + 1];
        Array.Copy(array, 0, newArray, front ? 1 : 0, array.Length);
        if (front)
            newArray[0] = element;
        else
            newArray[^1] = element;
        return newArray;
    }

    /// <summary>
    /// Creates a new array with the item at the specified index removed.
    /// This is worse than using a list and it will copy the array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] RemoveFromArray<T>(this T[] array, int elementIdx)
    {
        if (array.Length - 1 < elementIdx) return array;
        if (array.Length == 1 && elementIdx == 0) return Array.Empty<T>();

        var newArray = new T[array.Length - 1];

        int firstCopyLength = elementIdx;
        if (elementIdx != 0)
            Array.Copy(array, 0, newArray, 0, elementIdx);

        int secondCopyLength = array.Length - elementIdx - 1;
        if (secondCopyLength != 0)
            Array.Copy(array, elementIdx, newArray, Math.Max(0, elementIdx - 1), secondCopyLength);

        return newArray;
    }

    /// <inheritdoc cref="RemoveFromArray{T}(T[], int)" />
    public static Array RemoveFromArray(this Array array, int elementIdx)
    {
        if (array.Length - 1 < elementIdx) return array;
        if (array.Length == 1 && elementIdx == 0) return null;

        var newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length - 1);

        int firstCopyLength = elementIdx;
        if (elementIdx != 0)
            Array.Copy(array, 0, newArray, 0, elementIdx);

        int secondCopyLength = array.Length - elementIdx;
        if (secondCopyLength != 0)
            Array.Copy(array, elementIdx, newArray, Math.Max(0, elementIdx - 1), secondCopyLength);

        return newArray;
    }

    /// <summary>
    /// Join two arrays to create a new array contains the items of both.
    /// </summary>
    public static T[] JoinArrays<T>(this T[] arrayOne, T[] arrayTwo)
    {
        var newArray = new T[arrayOne.Length + arrayTwo.Length];
        Array.Copy(arrayOne, 0, newArray, 0, arrayOne.Length);
        Array.Copy(arrayTwo, 0, newArray, arrayOne.Length, arrayTwo.Length);
        return newArray;
    }

    /// <summary>
    /// Returns the index of an element within an array, or -1 if not found.
    /// This will loop the whole array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this T[] array, T element)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(element)) return i;
        }

        return -1;
    }

    /// <inheritdoc cref="IndexOf{T}(T[], T)" />
    public static int IndexOf(this Array array, object element)
    {
        for (var i = 0; i < array.Length; i++)
        {
            object item = array.GetValue(i);
            if (ReferenceEquals(item, element)) return i;
        }

        return -1;
    }

    /// <summary>
    /// Swap two items in an array by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="idx"></param>
    /// <param name="withIdx"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ArraySwap<T>(this T[] array, int idx, int withIdx)
    {
        T temp = array[idx];
        array[idx] = array[withIdx];
        array[withIdx] = temp;
    }
}
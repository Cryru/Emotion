#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Standard.BinaryStruct
{
    /// <summary>
    /// Converts a data struct to binary and back.
    /// </summary>
    /// If you want your struct to contain a string you need to use a fixed char array.
    /// <example>
    /// Example of a struct with a string:
    /// <code>
    /// public unsafe struct StructWithString
    /// {
    ///     public fixed char MyName[5]
    ///     public string MyNameStr
    ///     {
    ///         get
    ///         {
    ///             fixed (char* ptr = MyName)
    ///             {
    ///                 return new string(ptr);
    ///             }
    ///         }
    ///         set
    ///         {
    ///             for (var i = 0; i &lt; value.Length; i++)
    ///             {
    ///                 MyName[i] = value[i];
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class BinaryStructFormat
    {
        public static unsafe byte[] To<T>(T obj) where T : unmanaged
        {
            int byteSize = sizeof(T);
            var data = new byte[byteSize];
            var objPtr = (IntPtr) (&obj);
            Marshal.Copy(objPtr, data, 0, byteSize);
            return data;
        }

        public static unsafe T From<T>(byte[] bin) where T : unmanaged
        {
            int byteSize = bin.Length;
            var newObj = new T();
            var objPtr = (IntPtr) (&newObj);
            Marshal.Copy(bin, 0, objPtr, byteSize);
            return newObj;
        }
    }
}
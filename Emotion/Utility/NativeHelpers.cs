#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion.Utility
{
    public static class NativeHelpers
    {
        /*
                         C++ Type              |     C# Type     |  Size
           BOOL                                | bool            | 1 byte
           BYTE                                | byte            | 1 byte
           CHAR                                | byte            | 1 byte
           DECIMAL                             | Decimal         | 16 bytes
           DOUBLE                              | double          | 8 bytes
           DWORD                               | uint, UInt32    | 4 bytes
           FLOAT                               | float, single   | 4 bytes
           INT, signed int int                 | Int32           | 4 bytes
           INT16, signed short int short,      | Int16           | 2 bytes
           INT32, signed int int,              | Int32           | 4 bytes
           INT64 long                          | Int64           | 8 bytes
           LONG int                            | Int32           | 4 bytes
           LONG32, signed int int              | Int32           | 4 bytes
           LONG64 long                         | Int64           | 8 bytes
           LONGLONG long                       | Int64           | 8 bytes
           SHORT, signed short int short,      | Int16 (short)   | 2 bytes
           USHORT                              | UInt16 (ushort) | 2 bytes
           UCHAR, unsigned char                | byte            | 1 byte
           UINT, unsigned int uint,            | UInt32          | 4 bytes
           UINT16, WORD ushort,                | UInt16          | 2 bytes
           UINT32, unsigned int uint           | UInt32          | 4 bytes
           UINT64 ulong                        | UInt64          | 8 bytes
           ULONG, unsigned long uint           | UInt32          | 4 bytes
           ULONG32 uint                        | UInt32          | 4 bytes
           ULONG64 ulong                       | UInt64          | 8 bytes
           ULONGLONG ulong                     | UInt64          | 8 bytes
           WORD                                | ushort          | 2 bytes
           void*, pointers                     | IntPtr          | x86=4 bytes, x64=8 bytes
         */


        /// <summary>
        /// A reference to IntPtr.Zero
        /// </summary>
        public static IntPtr Nullptr = IntPtr.Zero;

        /// <summary>
        /// Make a C ulong (C# uint) from two ushorts.
        /// </summary>
        /// <returns></returns>
        public static uint MakeULong(ushort high, ushort low)
        {
            return ((uint) low & 0xFFFF) | (((uint) high & 0xFFFF) << 16);
        }

        /// <summary>
        /// Make a word from two bytes.
        /// </summary>
        /// <returns></returns>
        public static ushort MakeWord(byte high, byte low)
        {
            return (ushort) (((uint) low & 0xFF) | (((uint) high & 0xFF) << 8));
        }

        // ByteAccess functions taken from https://www.codeproject.com/Messages/1296018/Re-What-is-the-equivalent-of-LOBYTE-and-HIBYTE-in.aspx

        /// <summary>
        /// Access the lower word in a double word.
        /// </summary>
        public static ushort LoWord(uint nValue)
        {
            return (ushort) (nValue & 0xFFFF);
        }

        /// <summary>
        /// Access the high word in a double word.
        /// </summary>
        public static ushort HiWord(uint nValue)
        {
            return (ushort) (nValue >> 16);
        }

        /// <summary>
        /// Access the high word in a double word.
        /// </summary>
        public static ushort HiWord(ulong nValue)
        {
            return (ushort) (nValue >> 16);
        }

        /// <summary>
        /// Access the lower byte in a word.
        /// </summary>
        public static byte LoByte(ushort nValue)
        {
            return (byte) (nValue & 0xFF);
        }

        /// <summary>
        /// Access the high byte in a word.
        /// </summary>
        public static byte HiByte(ushort nValue)
        {
            return (byte) (nValue >> 8);
        }

        /// <summary>
        /// Copy the contents of one pointer to another.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemCopy(IntPtr src, IntPtr dst, int size)
        {
            new Span<byte>((byte*) src, size).CopyTo(new Span<byte>((byte*) dst, size));
        }

        /// <summary>
        /// Create a string from a null terminated char array.
        /// </summary>
        /// <param name="chArr">The char array to make a string from.</param>
        /// <returns>The <see cref="string" /> contained.</returns>
        public static string StringFromNullTerminated(char[] chArr)
        {
            int i;
            for (i = 0; i < chArr.Length; i++)
            {
                if (chArr[i] == '\0') break;
            }

            return new string(chArr, 0, i);
        }

        /// <summary>
        /// Create a string from a pointer to a null terminated UTF8 string. Copies memory.
        /// </summary>
        /// <param name="ptr">The address of the first character of the unmanaged string.</param>
        /// <returns>The <see cref="string" /> contained in <paramref name="ptr" />.</returns>
        public static string StringFromPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            var len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
                ++len;

            var buffer = new byte[len];
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Create a null terminated UTF8 string in unmanaged memory. Allocates memory.
        /// </summary>
        /// <param name="text">The string to copy.</param>
        /// <returns>A <see cref="IntPtr" /> to memory with the string copied.</returns>
        public static IntPtr StringToPtr(string text)
        {
            int len = Encoding.UTF8.GetByteCount(text);
            var buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            IntPtr ptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ptr, buffer.Length);
            return ptr;
        }

        /// <summary>
        /// Get a function from a library into a delegate.
        /// </summary>
        public static TDelegate GetFunctionByPtr<TDelegate>(IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? default : Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
        }
    }
}
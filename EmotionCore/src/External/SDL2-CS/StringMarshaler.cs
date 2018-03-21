// Emotion - https://github.com/Cryru/Emotion

#if SDL2

/* SDL2# - C# Wrapper for SDL2
 *
 * Copyright (c) 2013-2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 * Source code is edited for use in project Emotion which can be found at http://www.github.com/Cryru/Emotion
 * Vlad "Cryru" Abadzhiev <TheCryru@gmail.com>
 */

#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

// ReSharper disable once CheckNamespace
namespace SDL2
{
    internal unsafe class StringMarshaler : ICustomMarshaler
    {
        public const string LeaveAllocated = "LeaveAllocated";

        private static ICustomMarshaler
            _leaveAllocatedInstance = new StringMarshaler(true),
            _defaultInstance = new StringMarshaler(false);

        public static ICustomMarshaler GetInstance(string cookie)
        {
            switch (cookie)
            {
                case "LeaveAllocated":
                    return _leaveAllocatedInstance;
                default:
                    return _defaultInstance;
            }
        }

        private bool _leaveAllocated;

        public StringMarshaler(bool leaveAllocated)
        {
            _leaveAllocated = leaveAllocated;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;
            byte* ptr = (byte*)pNativeData;
            while (*ptr != 0) ptr++;
            byte[] bytes = new byte[ptr - (byte*)pNativeData];
            Marshal.Copy(pNativeData, bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            if (managedObj == null)
                return IntPtr.Zero;
            if (!(managedObj is string str)) throw new ArgumentException("ManagedObj must be a string.", "managedObj");
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            IntPtr mem = SDL.SDL_malloc((IntPtr)(bytes.Length + 1));
            Marshal.Copy(bytes, 0, mem, bytes.Length);

            // Cast the pointer to a byte array pointer.
            byte* memBytePointer = (byte*) mem;
            if (memBytePointer == null) throw new Exception("Failed to cast pointer to a byte array pointer.");

            // Set the last byte to zero.
            memBytePointer[bytes.Length] = 0;

            // Return the pointer.
            return mem;
        }

        public void CleanUpManagedData(object managedObj)
        {
            
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (!_leaveAllocated) SDL.SDL_free(pNativeData);
        }

        public int GetNativeDataSize()
        {
            return -1;
        }
    }
}


#endif
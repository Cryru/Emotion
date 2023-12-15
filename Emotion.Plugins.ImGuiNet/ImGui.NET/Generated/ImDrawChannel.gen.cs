#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImDrawChannel
    {
        public ImVector _CmdBuffer;
        public ImVector _IdxBuffer;
    }

    public unsafe struct ImDrawChannelPtr
    {
        public ImDrawChannel* NativePtr { get; }

        public ImDrawChannelPtr(ImDrawChannel* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImDrawChannelPtr(IntPtr nativePtr)
        {
            NativePtr = (ImDrawChannel*) nativePtr;
        }

        public static implicit operator ImDrawChannelPtr(ImDrawChannel* nativePtr)
        {
            return new ImDrawChannelPtr(nativePtr);
        }

        public static implicit operator ImDrawChannel*(ImDrawChannelPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImDrawChannelPtr(IntPtr nativePtr)
        {
            return new ImDrawChannelPtr(nativePtr);
        }

        public ImPtrVector<ImDrawCmdPtr> _CmdBuffer
        {
            get => new ImPtrVector<ImDrawCmdPtr>(NativePtr->_CmdBuffer, Unsafe.SizeOf<ImDrawCmd>());
        }

        public ImVector<ushort> _IdxBuffer
        {
            get => new ImVector<ushort>(NativePtr->_IdxBuffer);
        }
    }
}
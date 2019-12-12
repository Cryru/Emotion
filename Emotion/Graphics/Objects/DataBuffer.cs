#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// Represents an OpenGL buffer.
    /// </summary>
    public class DataBuffer : IDisposable
    {
        /// <summary>
        /// The bound buffers of each type.
        /// </summary>
        public static Dictionary<BufferTarget, uint> Bound = new Dictionary<BufferTarget, uint>();

        static DataBuffer()
        {
            // Create bound dictionary with all types.
            var possibleTypes = (BufferTarget[]) Enum.GetValues(typeof(BufferTarget));
            foreach (BufferTarget type in possibleTypes)
            {
                Bound[type] = 0;
            }
        }

        /// <summary>
        /// The OpenGL pointer to this DataBuffer.
        /// </summary>
        public uint Pointer { get; set; }

        /// <summary>
        /// The type of data buffer.
        /// </summary>
        public BufferTarget Type { get; set; }

        /// <summary>
        /// The size of the buffer, in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Create a new data buffer.
        /// </summary>
        /// <param name="type">The type of buffer to create.</param>
        /// <param name="byteSize">The size of the buffer in bytes. This is optional and can be set later with an upload.</param>
        /// <param name="dataUsage">The usage for the data. Only matters if a size is specified.</param>
        public DataBuffer(BufferTarget type, uint byteSize = 0, BufferUsage dataUsage = BufferUsage.StreamDraw)
        {
            Debug.Assert(GLThread.IsGLThread());

            Type = type;
            Pointer = Gl.GenBuffer();
            EnsureBound(Pointer, Type);

            if (byteSize != 0) Upload(IntPtr.Zero, byteSize, dataUsage);
        }

        /// <summary>
        /// Upload data to the buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data itself.</param>
        /// <param name="usage">What the buffer will be used for.</param>
        public void Upload<T>(T[] data, BufferUsage usage = BufferUsage.StreamDraw)
        {
            // Finish mapping - if it was.
            FinishMapping();

            int byteSize = Marshal.SizeOf(data[0]);
            Size = (uint) (data.Length * byteSize);

            if (Engine.Renderer.Dsa)
            {
                Gl.NamedBufferData(Pointer, Size, data, usage);
            }
            else
            {
                EnsureBound(Pointer, Type);
                Gl.BufferData(Type, Size, data, usage);
            }
        }

        /// <summary>
        /// Upload data to the buffer.
        /// </summary>
        /// <param name="data">A pointer to the data.</param>
        /// <param name="byteSize">The data's size in bytes.</param>
        /// <param name="usage">What the buffer will be used for.</param>
        public void Upload(IntPtr data, uint byteSize, BufferUsage usage = BufferUsage.StreamDraw)
        {
            // Finish mapping - if it was.
            FinishMapping();

            Size = byteSize;

            if (Engine.Renderer.Dsa)
            {
                Gl.NamedBufferData(Pointer, Size, data, usage);
            }
            else
            {
                EnsureBound(Pointer, Type);
                Gl.BufferData(Type, Size, data, usage);
            }
        }

        /// <summary>
        /// Upload partial data to the buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data to upload.</param>
        /// <param name="offset">The offset to upload from.</param>
        public void UploadPartial<T>(T[] data, uint offset = 0)
        {
            // Finish mapping - if it was.
            FinishMapping();

            int byteSize = Marshal.SizeOf(data[0]);
            var offsetPtr = (IntPtr) offset;
            var partSize = (uint) (data.Length * byteSize);

            if (offset > Size || offset + partSize > Size)
            {
                Engine.Log.Warning("Tried to map buffer out of range.", MessageSource.GL);
                Debug.Assert(false);
                return;
            }

            EnsureBound(Pointer, Type);
            Gl.BufferSubData(Type, offsetPtr, partSize, data);
        }

        /// <summary>
        /// Upload partial data to the buffer.
        /// </summary>
        /// <param name="data">The data to upload.</param>
        /// <param name="size">The data's size in bytes.</param>
        /// <param name="offset">The offset to upload from.</param>
        public void UploadPartial(IntPtr data, uint size, uint offset = 0)
        {
            var offsetPtr = (IntPtr) offset;

            EnsureBound(Pointer, Type);
            Gl.BufferSubData(Type, offsetPtr, size, data);
        }

        #region Mapping

        private bool _mapping;
        private unsafe byte* _mappingPtr;

        /// <summary>
        /// Start mapping the memory of the buffer.
        /// </summary>
        public unsafe void StartMapping()
        {
            if (_mapping)
            {
                Engine.Log.Warning("Tried to start mapping a buffer which is already mapping.", MessageSource.GL);
                return;
            }

            if (Size == 0)
            {
                Engine.Log.Warning("Tried to start mapping a buffer with no memory.", MessageSource.GL);
                return;
            }

            if (Engine.Renderer.Dsa)
            {
                _mappingPtr = (byte*) Gl.MapNamedBufferRange(Pointer, IntPtr.Zero, Size, BufferAccessMask.MapWriteBit);
            }
            else
            {
                EnsureBound(Pointer, Type);
                _mappingPtr = (byte*) Gl.MapBufferRange(Type, IntPtr.Zero, Size, BufferAccessMask.MapWriteBit);
            }

            if ((IntPtr) _mappingPtr == IntPtr.Zero)
            {
                Engine.Log.Warning("Couldn't start mapping buffer. Expect a crash.", MessageSource.GL);
                Debug.Assert(false);
            }

            _mapping = true;
        }

        /// <summary>
        /// Get a portion of the buffer's memory as a <see cref="System.Span{T}" />, allowing you to map data
        /// of that type. When finished mapping you should invoke <see cref="FinishMapping" /> to flush the data to the GPU.
        /// </summary>
        /// <typeparam name="T">The type of mapper to create.</typeparam>
        /// <param name="offset">Offset for the mapping region from the beginning of the buffer - in bytes.</param>
        /// <param name="length">Length for the mapping region. Set to -1 to map to the end of the buffer - in bytes.</param>
        /// <returns>A mapper used to map data in the buffer.</returns>
        public unsafe Span<T> CreateMapper<T>(int offset = 0, int length = -1)
        {
            EnsureBound(Pointer, Type);

            if (length == -1) length = (int) (Size - offset);
            if (offset + length > Size)
            {
                Debug.Assert(false);
                return null;
            }

            if (!_mapping) StartMapping();
            if (!_mapping) return null;

            var mapper = new Span<T>(&_mappingPtr[offset], length / Marshal.SizeOf<T>());
            return mapper;
        }

        /// <summary>
        /// Get a portion of the buffer's memory as pointer to it, allowing you to map data. 
        /// When finished mapping you should invoke <see cref="FinishMapping" /> to flush the data to the GPU.
        /// </summary>
        /// <typeparam name="T">The type of mapper to create.</typeparam>
        /// <param name="offset">Offset for the mapping region from the beginning of the buffer - in bytes.</param>
        /// <param name="length">Length for the mapping region. Set to -1 to map to the end of the buffer - in bytes.</param>
        /// <returns>A mapper used to map data in the buffer.</returns>
        public unsafe byte* CreateUnsafeMapper<T>(int offset = 0)
        {
            EnsureBound(Pointer, Type);

            if (!_mapping) StartMapping();
            if (!_mapping) return null;

            return &_mappingPtr[offset];
        }

        /// <summary>
        /// Finish mapping the buffer, flushing data to the GPU.
        /// If the buffer wasn't mapping to begin with - nothing happens.
        /// </summary>
        public unsafe void FinishMapping()
        {
            if (!_mapping) return;

            _mappingPtr = (byte*) IntPtr.Zero;

            if (Engine.Renderer.Dsa)
            {
                Gl.UnmapNamedBuffer(Pointer);
            }
            else
            {
                EnsureBound(Pointer, Type);
                Gl.UnmapBuffer(Type);
            }

            _mapping = false;
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            uint ptr = Pointer;
            Pointer = 0;

            if (Engine.Host == null) return;
            if (Bound[Type] == ptr) Bound[Type] = 0;

            GLThread.ExecuteGLThreadAsync(() => { Gl.DeleteBuffers(ptr); });
        }

        #endregion

        /// <summary>
        /// Ensures the provided pointer is the currently bound data buffer of the provided type.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        /// <param name="type">The type of data buffer to ensure is bound.</param>
        public static void EnsureBound(uint pointer, BufferTarget type)
        {
            // Check if it is already bound.
            if (Bound[type] == pointer)
            {
                // If in debug mode, verify this with OpenGL.
                if (!Engine.Configuration.DebugMode) return;

                bool foundBindingName = Enum.TryParse($"{type}Binding", true, out GetPName bindingName);
                if (!foundBindingName) Engine.Log.Warning($"Couldn't find binding name for data buffer of type {type}", MessageSource.GL);
                Gl.GetInteger(bindingName, out uint actualBound);
                if (actualBound != pointer)
                    Engine.Log.Error($"Assumed bound data buffer of type {type} was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            Gl.BindBuffer(type, pointer);
            Bound[type] = pointer;
        }
    }
}
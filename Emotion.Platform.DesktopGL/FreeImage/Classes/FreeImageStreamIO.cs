#region Using

using System;
using System.IO;

#endregion

namespace FreeImageAPI.IO
{
    /// <summary>
    /// Internal class wrapping stream io functions.
    /// </summary>
    /// <remarks>
    /// FreeImage can read files from a disk or a network drive but also allows the user to
    /// implement their own loading or saving functions to load them directly from an ftp or web
    /// server for example.
    /// <para />
    /// In .NET streams are a common way to handle data. The <b>FreeImageStreamIO</b> class handles
    /// the loading and saving from and to streams. It implements the funtions FreeImage needs
    /// to load data from an an arbitrary source.
    /// <para />
    /// The class is for internal use only.
    /// </remarks>
    internal static class FreeImageStreamIO
    {
        /// <summary>
        /// <see cref="FreeImageAPI.IO.FreeImageIO" /> structure that can be used to read from streams via
        /// <see cref="FreeImageAPI.FreeImage.LoadFromHandle(FREE_IMAGE_FORMAT, ref FreeImageIO, fi_handle, FREE_IMAGE_LOAD_FLAGS)" />
        /// .
        /// </summary>
        public static readonly FreeImageIO io;

        /// <summary>
        /// Initializes a new instances which can be used to
        /// create a FreeImage compatible <see cref="FreeImageAPI.IO.FreeImageIO" /> structure.
        /// </summary>
        static FreeImageStreamIO()
        {
            io.readProc = streamRead;
            io.writeProc = streamWrite;
            io.seekProc = streamSeek;
            io.tellProc = streamTell;
        }

        /// <summary>
        /// Reads the requested data from the stream and writes it to the given address.
        /// </summary>
        private static unsafe uint streamRead(IntPtr buffer, uint size, uint count, fi_handle handle)
        {
            Stream stream = handle.GetObject() as Stream;
            if (stream == null || !stream.CanRead) return 0;

            uint readCount = 0;
            byte* ptr = (byte*) buffer;
            byte[] bufferTemp = new byte[size];
            int read;
            while (readCount < count)
            {
                read = stream.Read(bufferTemp, 0, (int) size);
                if (read != (int) size)
                {
                    stream.Seek(-read, SeekOrigin.Current);
                    break;
                }

                for (int i = 0; i < read; i++, ptr++)
                {
                    *ptr = bufferTemp[i];
                }

                readCount++;
            }

            return readCount;
        }

        /// <summary>
        /// Reads the given data and writes it into the stream.
        /// </summary>
        private static unsafe uint streamWrite(IntPtr buffer, uint size, uint count, fi_handle handle)
        {
            Stream stream = handle.GetObject() as Stream;
            if (stream == null || !stream.CanWrite) return 0;

            uint writeCount = 0;
            byte[] bufferTemp = new byte[size];
            byte* ptr = (byte*) buffer;
            while (writeCount < count)
            {
                for (int i = 0; i < size; i++, ptr++)
                {
                    bufferTemp[i] = *ptr;
                }

                try
                {
                    stream.Write(bufferTemp, 0, bufferTemp.Length);
                }
                catch
                {
                    return writeCount;
                }

                writeCount++;
            }

            return writeCount;
        }

        /// <summary>
        /// Moves the streams position.
        /// </summary>
        private static int streamSeek(fi_handle handle, int offset, SeekOrigin origin)
        {
            Stream stream = handle.GetObject() as Stream;
            if (stream == null) return 1;

            stream.Seek(offset, origin);
            return 0;
        }

        /// <summary>
        /// Returns the streams current position
        /// </summary>
        private static int streamTell(fi_handle handle)
        {
            Stream stream = handle.GetObject() as Stream;
            if (stream == null) return -1;

            return (int) stream.Position;
        }
    }
}
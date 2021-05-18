#region Using

using System;
using System.IO;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;
using Emotion.Utility.Zlib;

#endregion

namespace Emotion.Standard.Image.PNG
{
    /// <summary>
    /// Stores header information about a chunk.
    /// </summary>
    internal sealed class PngChunk
    {
        /// <summary>
        /// Whether the chunk is valid.
        /// </summary>
        public bool Valid;

        /// <summary>
        /// A chunk type as string with 4 chars.
        /// </summary>
        public string Type;

        /// <summary>
        /// The chunk's data bytes appropriate to the chunk type, if any.
        /// This field can be of zero length.
        /// </summary>
        public ByteReader ChunkReader;

        /// <summary>
        /// A CRC (Cyclic Redundancy Check) calculated on the preceding bytes in the chunk,
        /// including the chunk type code and chunk data fields, but not including the length field.
        /// The CRC is always present, even for chunks containing no data
        /// </summary>
        public uint Crc;

        public PngChunk(ByteReader stream)
        {
            // Read chunk length.
            var lengthBuffer = new byte[4];
            int numBytes = stream.Read(lengthBuffer, 0, 4);
            if (numBytes >= 1 && numBytes <= 3)
            {
                Engine.Log.Warning($"Chunk length {numBytes} is not valid!", MessageSource.ImagePng);
                return;
            }

            Array.Reverse(lengthBuffer);
            var length = BitConverter.ToInt32(lengthBuffer, 0);

            // Invalid chunk or after end chunk.
            if (numBytes == 0) return;

            // Read the chunk type.
            var typeBuffer = new byte[4];
            int typeBufferNumBytes = stream.Read(typeBuffer, 0, 4);
            if (typeBufferNumBytes >= 1 && typeBufferNumBytes <= 3) throw new Exception("ImagePng: Chunk type header is not valid!");
            var chars = new char[4];
            chars[0] = (char) typeBuffer[0];
            chars[1] = (char) typeBuffer[1];
            chars[2] = (char) typeBuffer[2];
            chars[3] = (char) typeBuffer[3];
            Type = new string(chars);

            ChunkReader = stream.Branch(0, false, length);
            stream.Seek(length, SeekOrigin.Current);

            // Read compressed chunk.
            var crcBuffer = new byte[4];
            int numBytesCompression = stream.Read(crcBuffer, 0, 4);
            if (numBytesCompression >= 1 && numBytesCompression <= 3) throw new Exception("ImagePng: Compressed data header is not valid!");
            Array.Reverse(crcBuffer);
            Crc = BitConverter.ToUInt32(crcBuffer, 0);

#if DEBUG
            var crc = new Crc32();
            crc.Update(typeBuffer);
            crc.Update(ChunkReader.Data.Span);

            // PNGs saved with Gimp spam the log with warnings.
            // https://gitlab.gnome.org/GNOME/gimp/-/issues/2111
            //if (crc.Value != Crc)
            //    Engine.Log.Warning($"CRC Error. PNG Image chunk {Type} is corrupt!", "ImagePng");
#endif
            Valid = true;
        }
    }
}
﻿#region Using

using System;
using System.IO;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.Utility.Zlib;

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
        /// An unsigned integer giving the number of bytes in the chunk's
        /// data field. The length counts only the data field, not itself,
        /// the chunk type code, or the CRC. Zero is a valid length
        /// </summary>
        public int Length;

        /// <summary>
        /// A chunk type as string with 4 chars.
        /// </summary>
        public string Type;

        /// <summary>
        /// The data bytes appropriate to the chunk type, if any.
        /// This field can be of zero length.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// A CRC (Cyclic Redundancy Check) calculated on the preceding bytes in the chunk,
        /// including the chunk type code and chunk data fields, but not including the length field.
        /// The CRC is always present, even for chunks containing no data
        /// </summary>
        public uint Crc;

        public PngChunk(MemoryStream stream)
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

            Length = BitConverter.ToInt32(lengthBuffer, 0);

            // Check if the length is valid.
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

            Data = new byte[Length];
            stream.Read(Data, 0, Length);

            // Read compressed chunk.
            var crcBuffer = new byte[4];

            int numBytesCompression = stream.Read(crcBuffer, 0, 4);
            if (numBytesCompression >= 1 && numBytesCompression <= 3) throw new Exception("ImagePng: Compressed data header is not valid!");

            Array.Reverse(crcBuffer);

            Crc = BitConverter.ToUInt32(crcBuffer, 0);
            var crc = new Crc32();
            crc.Update(typeBuffer);
            crc.Update(Data);

            if (crc.Value != Crc)
                Engine.Log.Warning($"CRC Error. PNG Image chunk {Type} is corrupt!", "ImagePng");

            Valid = true;
        }
    }
}
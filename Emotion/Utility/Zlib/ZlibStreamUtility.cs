#region Using

using System.IO;
using System.IO.Compression;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Utility.Zlib
{
    /// <summary>
    /// Provides methods for decompressing streams by using the Zlib Deflate algorithm.
    /// </summary>
    internal sealed class ZlibStreamUtility
    {
        public static byte[] Compress(byte[] dataToCompress, int compressionLevel)
        {
            // Write the zlib header : http://tools.ietf.org/html/rfc1950
            // CMF(Compression Method and flags)
            // This byte is divided into a 4 - bit compression method and a 
            // 4-bit information field depending on the compression method.
            // bits 0 to 3  CM Compression method
            // bits 4 to 7  CINFO Compression info
            //
            //   0   1
            // +---+---+
            // |CMF|FLG|
            // +---+---+
            var cmf = 0x78;
            var flg = 218;

            // http://stackoverflow.com/a/2331025/277304
            if (compressionLevel >= 5 && compressionLevel <= 6)
                flg = 156;
            else if (compressionLevel >= 3 && compressionLevel <= 4)
                flg = 94;

            else if (compressionLevel <= 2) flg = 1;

            // Just in case
            flg -= (cmf * 256 + flg) % 31;

            if (flg < 0) flg += 31;

            using var str = new MemoryStream();
            str.WriteByte((byte) cmf);
            str.WriteByte((byte) flg);

            // Initialize the deflate Stream.
            var level = CompressionLevel.Optimal;

            if (compressionLevel >= 1 && compressionLevel <= 5)
                level = CompressionLevel.Fastest;

            else if (compressionLevel == 0) level = CompressionLevel.NoCompression;

            // Initialize the deflate stream and copy to it.
            using var deflateStream = new DeflateStream(str, level);
            deflateStream.Write(dataToCompress, 0, dataToCompress.Length);
            deflateStream.Close();

            return str.ToArray();
        }

        public static byte[] Decompress(Stream stream)
        {
            // Read the zlib header : http://tools.ietf.org/html/rfc1950
            // CMF(Compression Method and flags)
            // This byte is divided into a 4 - bit compression method and a 
            // 4-bit information field depending on the compression method.
            // bits 0 to 3  CM Compression method
            // bits 4 to 7  CINFO Compression info
            //
            //   0   1
            // +---+---+
            // |CMF|FLG|
            // +---+---+
            int cmf = stream.ReadByte();
            int flag = stream.ReadByte();
            if (cmf == -1 || flag == -1) return null;

            if ((cmf & 0x0f) != 8)
            {
                Engine.Log.Warning($"Bad compression method for ZLIB header: cmf={cmf}", MessageSource.Other);
                return null;
            }

            if ((flag & 32) != 0)
                // The dictionary is 4 bytes long
                stream.Position += 4;

            // Decompress using the .Net deflate stream
            using var str = new MemoryStream();
            using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
            deflateStream.CopyTo(str);
            deflateStream.Flush();

            return str.ToArray();
        }
    }
}
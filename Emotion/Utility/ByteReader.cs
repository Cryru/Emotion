#region Using

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Custom implementation of the BinaryReader which supports branching, and has a smaller memory footprint.
    /// </summary>
    public class ByteReader : ByteReaderBase
    {
        // This class implements a hiding int as a Position and
        // a different implementation for some functions.
        // The stream class expects things to work a certain way, which
        // isn't always convenient.

        public new int Position
        {
            get => (int) base.Position;
            set => base.Position = value;
        }

        public ByteReader(ReadOnlyMemory<byte> data) : base(data)
        {
            Data = data;
        }

        public ByteReader()
        {
        }

        /// <summary>
        /// Read a byte.
        /// </summary>
        /// <returns>The read byte.</returns>
        public new byte ReadByte()
        {
            byte b = Data.Span[Position];
            Position++;
            return b;
        }
    }

    /// <summary>
    /// This class exists because "Stream" has some questionable decisions about its interface, and
    /// the ByteReader hides those.
    /// </summary>
    public abstract class ByteReaderBase : Stream, IDisposable
    {
        public long TotalPosition
        {
            get => _branchPosition + Position;
        }

        public override long Position { get; set; }
        public ReadOnlyMemory<byte> Data { get; protected set; }

        private long _branchPosition;

        protected ByteReaderBase(ReadOnlyMemory<byte> data)
        {
            Data = data;
        }

        protected ByteReaderBase()
        {
        }

        /// <summary>
        /// Branch the reader into another one.
        /// </summary>
        /// <param name="offset">The offset of the reader.</param>
        /// <param name="fromStart">Whether the offset is from the start, or from the current position.</param>
        /// <param name="length">The length of the data. If set to -1 (as is the default) then data to the end is taken.</param>
        /// <returns></returns>
        public ByteReader Branch(int offset, bool fromStart, int length = -1)
        {
            var b = new ByteReader {_branchPosition = (fromStart ? _branchPosition : TotalPosition) + offset};
            long startPos = fromStart ? offset : Position + offset;

            Debug.Assert(startPos <= Data.Length);

            if (length == -1)
            {
                b.Data = Data.Slice((int) startPos);
            }
            else
            {
                Debug.Assert(startPos + length <= Data.Length);
                b.Data = Data.Slice((int) startPos, length);
            }

            return b;
        }

        /// <summary>
        /// Read a number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read bytes.</returns>
        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            ReadOnlySpan<byte> s = Data.Span.Slice((int) Position, count);
            Position += count;
            return s;
        }

        /// <summary>
        /// Read a char.
        /// </summary>
        /// <returns>The read char.</returns>
        public char ReadChar()
        {
            var c = (char) Data.Span[(int) Position];
            Position++;
            return c;
        }

        /// <summary>
        /// Read a number of characters.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The read characters.</returns>
        public char[] ReadChars(int count)
        {
            var chars = new char[count];
            for (var i = 0; i < count; i++)
            {
                chars[i] = (char) Data.Span[(int) Position];
                Position++;
            }

            return chars;
        }

        /// <summary>
        /// Read an int (little endian).
        /// </summary>
        /// <returns>The read int.</returns>
        public int ReadInt()
        {
            ReadOnlySpan<byte> p = ReadBytes(4);
            var n = BitConverter.ToInt32(p);
            return !BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Read a short (little endian).
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            ReadOnlySpan<byte> p = ReadBytes(2);
            var n = BitConverter.ToInt16(p);
            return !BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Redirect to ReadInt()
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            return ReadInt();
        }

        /// <summary>
        /// Redirect to ReadShort().
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            return ReadShort();
        }

        /// <summary>
        /// Redirect to ReadByte().
        /// </summary>
        /// <returns></returns>
        public byte ReadInt8()
        {
            return (byte) ReadByte();
        }

        /// <summary>
        /// Read a byte.
        /// </summary>
        /// <returns>The read byte.</returns>
        public override int ReadByte()
        {
            byte b = Data.Span[(int) Position];
            Position++;
            return b;
        }

        /// <summary>
        /// Read a signed byte.
        /// </summary>
        /// <returns>The read signed byte.</returns>
        public sbyte ReadSByte()
        {
            var b = (sbyte) Data.Span[(int) Position];
            Position++;
            return b;
        }

        /// <summary>
        /// Reads a C ULong (uint) as big endian.
        /// </summary>
        public uint ReadULongBE()
        {
            ReadOnlySpan<byte> p = ReadBytes(4);
            var n = BitConverter.ToUInt32(p);
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Reads a ushort as big endian.
        /// </summary>
        public ushort ReadUShortBE()
        {
            ReadOnlySpan<byte> p = ReadBytes(2);
            var n = BitConverter.ToUInt16(p);
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Reads a 24 bit uint as big endian. Is returned as a Uint32.
        /// </summary>
        public uint ReadUInt24BE()
        {
            ReadOnlySpan<byte> p = ReadBytes(3);
            if (BitConverter.IsLittleEndian) return (uint) (p[2] | (p[1] << 8) | (p[0] << 16));
            return (uint) (p[0] | (p[1] << 8) | (p[2] << 16));
        }

        /// <summary>
        /// Reads a uint as big endian.
        /// </summary>
        public uint ReadUIntBE()
        {
            ReadOnlySpan<byte> p = ReadBytes(4);
            var n = BitConverter.ToUInt32(p);
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Reads a int32 as big endian.
        /// </summary>
        public int ReadInt32BE()
        {
            ReadOnlySpan<byte> p = ReadBytes(4);
            var n = BitConverter.ToInt32(p);
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Reads a short as big endian.
        /// </summary>
        public short ReadShortBE()
        {
            ReadOnlySpan<byte> p = ReadBytes(2);
            var n = BitConverter.ToInt16(p);
            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
        }

        /// <summary>
        /// Reads a float as big endian.
        /// </summary>
        public float ReadFloatBE()
        {
            short decimalData = ReadShortBE();
            ushort fraction = ReadUShortBE();
            return decimalData + fraction / 65535f;
        }

        #region Specific

        /// <summary>
        /// Reads two ushort as an OpenType version.
        /// </summary>
        public float ReadOpenTypeVersionBE(uint minorBase = 0x1000)
        {
            ushort major = ReadUShortBE();
            ushort minor = ReadUShortBE();

            // How to interpret the minor version is very vague in the spec. 0x5000 is 5, 0x1000 is 1
            // Default returns the correct number if minor = 0xN000 where N is 0-9
            // Set minorBase to 1 for tables that use minor = N where N is 0-9
            return major + minor / minorBase / 10f;
        }

        /// <summary>
        /// Reads a C ULong (uint) as big endian apple mac timestamp.
        /// </summary>
        public uint ReadULongBETimestamp()
        {
            // "LONGDATETIME" is a 64-bit integer.
            // However, unix timestamps traditionally use 32 bits, so we only take the last 32 bits.
            ReadULongBE();
            uint v = ReadULongBE();

            // Subtract seconds between 01/01/1904 and 01/01/1970
            // to convert Apple Mac timestamp to Standard Unix timestamp
            v -= 2082844800;

            return v;
        }

        /// <summary>
        /// Read an unsigned int of a variable size.
        /// 1 to 4 bytes are supported.
        /// </summary>
        /// <param name="n">The number of bytes in the int.</param>
        /// <returns>The read int.</returns>
        public uint ReadVariableUIntBE(int n)
        {
            return n switch
            {
                0 => 0,
                1 => (byte) ReadByte(),
                2 => ReadUShortBE(),
                3 => ReadUInt24BE(),
                4 => ReadUIntBE(),
                _ => ReadUIntBE()
            };
        }

        #endregion

        #region Stream Interface

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = true;
        public override bool CanWrite { get; } = false;

        public override long Length
        {
            get => Data.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            // noop
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position + count > Length) count = (int) (Length - Position);
            ReadOnlySpan<byte> copyData = Data.Span.Slice((int) Position, count);
            copyData.CopyTo(new Span<byte>(buffer).Slice(offset));
            Position += count;
            return count;
        }

        #endregion

        public new void Dispose()
        {
            Data = null;
            base.Dispose();
        }
    }
}
#region Using

using System;
using System.IO;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Stream wrapper around ReadOnlyMemory
    /// </summary>
    public class ReadOnlyMemoryStream : Stream
    {
        private ReadOnlyMemory<byte> _memory;

        public ReadOnlyMemoryStream(ReadOnlyMemory<byte> memory)
        {
            _memory = memory;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = (int) Position;
            int length = _memory.Length;
            if (pos + count > length) count = (int) (length - Position);
            if (count == 0) return 0;

            var destSpan = new Span<byte>(buffer, offset, count);
            ReadOnlySpan<byte> srcSpan = _memory.Span;
            srcSpan.Slice(pos, count).CopyTo(destSpan);

            Position += count;
            return count;
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

        public override void Flush()
        {
            // no-op
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get => true;
        }

        public override bool CanSeek
        {
            get => true;
        }

        public override bool CanWrite
        {
            get => false;
        }

        public override long Length
        {
            get => _memory.Length;
        }

        public override long Position { get; set; }
    }
}
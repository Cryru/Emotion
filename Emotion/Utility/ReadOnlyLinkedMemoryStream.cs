#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Asymmetric unrolled linked list in the form of a stream.
    /// </summary>
    public class ReadOnlyLinkedMemoryStream : Stream
    {
        private List<ReadOnlyMemory<byte>> _memoryChunks = new List<ReadOnlyMemory<byte>>();

        public void AddMemory(ReadOnlyMemory<byte> memory)
        {
            _memoryChunks.Add(memory);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long length = Length;
            if (Position + count > length) count = (int) (length - Position);
            if (count == 0) return 0;

            int writeOffset = offset;
            Span<byte> destSpan = buffer;
            var totalWritten = 0;

            while (totalWritten < count)
            {
                ReadOnlyMemory<byte> chunkAtPosition = GetBufferAtTotalPos(Position, out int bufferOffset);
                Debug.Assert(bufferOffset != -1);
                int bytesToWrite = Math.Min(chunkAtPosition.Length - bufferOffset, count - totalWritten);

                ReadOnlySpan<byte> src = chunkAtPosition.Span.Slice(bufferOffset, bytesToWrite);
                Span<byte> dst = destSpan.Slice(writeOffset);
                src.CopyTo(dst);

                writeOffset += bytesToWrite;
                totalWritten += bytesToWrite;
                Position += bytesToWrite;
            }

            return count;
        }

        private ReadOnlyMemory<byte> GetBufferAtTotalPos(long pos, out int bufferOffset)
        {
            var offset = 0;
            for (var i = 0; i < _memoryChunks.Count; i++)
            {
                ReadOnlyMemory<byte> currentChunk = _memoryChunks[i];
                if (pos >= offset && pos < offset + currentChunk.Length)
                {
                    bufferOffset = (int) (pos - offset);
                    return currentChunk;
                }

                offset += currentChunk.Length;
            }

            bufferOffset = -1;
            return null;
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
            get
            {
                var length = 0;
                for (var i = 0; i < _memoryChunks.Count; i++)
                {
                    length += _memoryChunks[i].Length;
                }

                return length;
            }
        }

        public override long Position { get; set; }
    }
}
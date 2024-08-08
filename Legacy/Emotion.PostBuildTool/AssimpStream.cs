#region Using

using System.IO;
using Assimp;

#endregion

namespace Emotion.PostBuildTool
{
	public class AssimpStream : IOStream
	{
		private Stream _stream;

		public AssimpStream(Stream managedStream, string fileName, FileIOMode fileMode) : base(fileName, fileMode)
		{
			_stream = managedStream;
		}

		public override long Write(byte[] dataToWrite, long count)
		{
			var lengthLeft = _stream.Length - _stream.Position;
			if (count > lengthLeft) count = lengthLeft;

			_stream.Write(dataToWrite, 0, (int) count);
			return count;
		}

		public override long Read(byte[] dataRead, long count)
		{
			long lengthLeft = _stream.Length - _stream.Position;
			if (count > lengthLeft) count = lengthLeft;

			return _stream.Read(dataRead, 0, (int) count);
		}

		public override ReturnCode Seek(long offset, Origin seekOrigin)
		{
			SeekOrigin conv;
			if (seekOrigin == Origin.Set)
				conv = SeekOrigin.Begin;
			else if (seekOrigin == Origin.Current)
				conv = SeekOrigin.Current;
			else
				conv = SeekOrigin.End;

			_stream.Seek(offset, conv);
			return ReturnCode.Success;
		}

		public override long GetPosition()
		{
			return _stream.Position;
		}

		public override long GetFileSize()
		{
			return _stream.Length;
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override bool IsValid
		{
			get => true;
		}
	}
}
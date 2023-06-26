#nullable enable

#region Using

using System.IO;
using System.Runtime.InteropServices;
using Emotion.Utility;
using Silk.NET.Assimp;
using File = Silk.NET.Assimp.File;

#endregion

namespace Emotion.IO.MeshAssetTypes.Assimp;

public sealed class AssimpStream : ReadOnlyLinkedMemoryStream
{
	public string Name { get; }
	public IntPtr Memory { get; private set; }

	public unsafe AssimpStream(string name)
	{
		Name = name;
		Memory = UnmanagedMemoryAllocator.MemAlloc(sizeof(File));

		var file = new File
		{
			FileSizeProc = PfnFileTellProc.From(GetFileSize),
			ReadProc = PfnFileReadProc.From(ReadFileData),
			SeekProc = PfnFileSeek.From(FileSeek),
			TellProc = PfnFileTellProc.From(GetFilePointer),
			FlushProc = PfnFileFlushProc.From(FileFlushProc)
		};
		Marshal.StructureToPtr(file, Memory, false);
	}

	private unsafe nuint GetFileSize(File* _)
	{
		return (nuint) (int) Length;
	}

	private unsafe nuint ReadFileData(File* _, byte* arg1, nuint arg2, nuint arg3)
	{
		var dest = new Span<byte>(arg1, (int) (arg2 * arg3));
		return (nuint) Read(dest);
	}

	private unsafe Return FileSeek(File* arg0, nuint arg1, Origin arg2)
	{
		SeekOrigin conv = arg2 switch
		{
			Origin.Set => SeekOrigin.Begin,
			Origin.Cur => SeekOrigin.Current,
			_ => SeekOrigin.End
		};
		Seek((int) arg1, conv);
		return Return.Success;
	}

	private unsafe nuint GetFilePointer(File* _)
	{
		return (nuint) (int) Position;
	}

	private unsafe void FileFlushProc(File* _)
	{
		Flush();
	}

	public override void Close()
	{
		base.Close();

		UnmanagedMemoryAllocator.Free(Memory);
		Memory = IntPtr.Zero;
	}
}
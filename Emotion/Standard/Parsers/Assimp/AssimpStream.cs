#nullable enable

#region Using

using System.IO;
using System.Runtime.InteropServices;
using File = Silk.NET.Assimp.File;

#endregion

namespace Emotion.Standard.Parsers.Assimp;

public sealed class AssimpStream : ReadOnlyLinkedMemoryStream
{
    public string Name { get; }

    public nint NativeStructMemory { get; private set; }

    public unsafe AssimpStream(string name)
    {
        Name = name;
        NativeStructMemory = UnmanagedMemoryAllocator.MemAlloc(sizeof(File));

        var file = new File
        {
            FileSizeProc = PfnFileTellProc.From(GetFileSize),
            ReadProc = PfnFileReadProc.From(ReadFileData),
            SeekProc = PfnFileSeek.From(FileSeek),
            TellProc = PfnFileTellProc.From(GetFilePointer),
            FlushProc = PfnFileFlushProc.From(FileFlushProc)
        };
        Marshal.StructureToPtr(file, NativeStructMemory, true);
    }

    private unsafe nuint GetFileSize(File* _)
    {
        return (nuint)(int)Length;
    }

    private unsafe nuint ReadFileData(File* _, byte* data, nuint elementSize, nuint elementCount)
    {
        var bytesLeft = (int)(Length - Position);
        int elementsCanRead = bytesLeft / (int)elementSize;
        elementsCanRead = Math.Min(elementsCanRead, (int)elementCount);
        var dest = new Span<byte>(data, (int)elementSize * elementsCanRead);
        int bytesRead = Read(dest);
        Assert(bytesRead == dest.Length);
        return (nuint)elementsCanRead;
    }

    private unsafe Return FileSeek(File* arg0, nuint arg1, Origin arg2)
    {
        SeekOrigin conv = arg2 switch
        {
            Origin.Set => SeekOrigin.Begin,
            Origin.Cur => SeekOrigin.Current,
            _ => SeekOrigin.End
        };
        Seek((int)arg1, conv);
        return Return.Success;
    }

    private unsafe nuint GetFilePointer(File* _)
    {
        return (nuint)(int)Position;
    }

    private unsafe void FileFlushProc(File* _)
    {
        Flush();
    }

    public override void Close()
    {
        base.Close();

        UnmanagedMemoryAllocator.Free(NativeStructMemory);
        NativeStructMemory = nint.Zero;
    }
}
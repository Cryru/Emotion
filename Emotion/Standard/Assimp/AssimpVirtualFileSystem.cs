#nullable enable

using Emotion.IO;
using Emotion.Utility;
using Silk.NET.Assimp;

namespace Emotion.Standard.Assimp;

public unsafe struct AssimpVirtualFileSystem
{
    public FileIO NativeFileIO;

    private List<AssimpStream> _loadedFiles = new List<AssimpStream>();

    public AssimpVirtualFileSystem()
    {
        NativeFileIO = new FileIO()
        {
            OpenProc = PfnFileOpenProc.From(OpenFileCallback),
            CloseProc = PfnFileCloseProc.From(CloseFileCallback)
        };
    }

    public void AddFile(string name, ReadOnlyMemory<byte> data)
    {
        var thisStream = new AssimpStream(name);
        thisStream.AddMemory(data);
        _loadedFiles.Add(thisStream);
    }

    private unsafe File* OpenFileCallback(FileIO* arg0, byte* arg1, byte* arg2)
    {
        string readMode = NativeHelpers.StringFromPtr((IntPtr)arg2);
        if (readMode != "rb")
        {
            Engine.Log.Error("Only read-binary file mode is supported.", "Assimp");
            return null;
        }

        string fileName = NativeHelpers.StringFromPtr((IntPtr)arg1);
        for (var i = 0; i < _loadedFiles.Count; i++)
        {
            AssimpStream alreadyOpenFile = _loadedFiles[i];
            if (alreadyOpenFile.Name == fileName) return (File*)alreadyOpenFile.NativeStructMemory;
        }

        //string assetPath = AssetLoader.GetDirectoryName(Name);
        //if (!fileName.StartsWith(assetPath))
        //    fileName = AssetLoader.GetNonRelativePath(assetPath, fileName);
        var byteAsset = Engine.AssetLoader.Get<OtherAsset>(fileName);
        if (byteAsset == null) return null;

        var assimpStream = new AssimpStream(fileName);
        assimpStream.AddMemory(byteAsset.Content);
        _loadedFiles.Add(assimpStream);
        return (File*)assimpStream.NativeStructMemory;
    }

    private unsafe void CloseFileCallback(FileIO* arg0, File* arg1)
    {
        // When Assimp wants to close a file we'll keep it loaded
        // but reset its position instead as it can be requested again and
        // we want to reduce IO. At the end of the asset parsing all loaded
        // files will be properly unloaded.
        var filePtr = (IntPtr)arg1;
        for (var i = 0; i < _loadedFiles.Count; i++)
        {
            AssimpStream file = _loadedFiles[i];
            if (file.NativeStructMemory == filePtr)
            {
                file.Position = 0;
                return;
            }
        }
    }

    public void Dispose()
    {
        // Clear virtual file system
        for (var i = 0; i < _loadedFiles.Count; i++)
        {
            AssimpStream file = _loadedFiles[i];
            file.Dispose();
        }

        _loadedFiles.Clear();
    }
}

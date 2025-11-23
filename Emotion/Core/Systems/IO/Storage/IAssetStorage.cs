#nullable enable

using System.IO;

namespace Emotion.Core.Systems.IO.Storage;

public ref struct AssetStorageOperation
{
    public bool IsValid { get => Stream != null; }

    public static AssetStorageOperation Invalid { get => new(); }

    public ReadOnlySpan<char> VirtualPath;
    public Stream Stream;

    public AssetStorageOperation(ReadOnlySpan<char> virtualPath, Stream stream)
    {
        VirtualPath = virtualPath;
        Stream = stream;
    }

    // File storage specifics
    public string ActualFilePath = string.Empty;
    public string TempFile = string.Empty;
}

public interface IAssetStorage
{
    public string MountPoint { get; }

    public AssetStorageOperation StartSave(ReadOnlySpan<char> virtualPath);

    public bool EndSave(AssetStorageOperation op);
}

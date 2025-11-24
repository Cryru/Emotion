#nullable enable

using System.IO;

namespace Emotion.Core.Systems.IO.Storage;

public class FileStorage : IAssetStorage
{
    public string MountPoint { get => _mountPoint; }

    private AssetLoader _loader;
    private string _actualFolder;
    private string _mountPoint;

    public FileStorage(AssetLoader loader, string actualPath, string mountPoint)
    {
        _loader = loader;
        _actualFolder = actualPath;
        _mountPoint = mountPoint;
    }

    public AssetStorageOperation StartSave(ReadOnlySpan<char> virtualPath)
    {
        // String allocations here are a bit cringe, but the file API works with strings only anyway so....
        var virtualPathNotMounted = virtualPath.Slice(_mountPoint.Length);
        string actualPath;
        if (Path.DirectorySeparatorChar != '/')
            actualPath = Path.Join(_actualFolder, virtualPathNotMounted.ToString().ToLowerInvariant().Replace('/', Path.DirectorySeparatorChar));
        else
            actualPath = Path.Join(_actualFolder, virtualPathNotMounted.ToString().ToLowerInvariant());

        string tempFileName = actualPath + ".writeTemp";
        try
        {
            string? directoryName = Path.GetDirectoryName(actualPath);
            if (directoryName != null)
                Directory.CreateDirectory(directoryName);

            FileStream stream = File.OpenWrite(tempFileName);

            return new AssetStorageOperation(virtualPath, stream)
            {
                VirtualPath = virtualPath,
                ActualFilePath = actualPath,
                TempFile = tempFileName,
            };
        }
        catch (Exception)
        {
            Engine.Log.Error($"Failed to start writing {actualPath} for asset {virtualPath}", MessageSource.AssetLoader);
        }
        return AssetStorageOperation.Invalid;
    }

    public bool EndSave(AssetStorageOperation op)
    {
        try
        {
            op.Stream.Dispose();

            string filePath = op.ActualFilePath.ToString();
            if (File.Exists(filePath)) // Back-up the old one
                File.Move(filePath, $"{filePath}.backup", true);

            string tempFile = op.TempFile;
            File.Move(tempFile, filePath, true);
        }
        catch (Exception)
        {
            Engine.Log.Error($"Failed writing {op.ActualFilePath} for asset {op.VirtualPath}", MessageSource.AssetLoader);
            return false;
        }

        // Mount if it doesn't exist.
        if (!_loader.Exists(op.VirtualPath))
            _loader.MountFile(op.ActualFilePath, op.VirtualPath);

        Engine.Log.Info($"Wrote file {op.ActualFilePath} for asset {op.VirtualPath}", MessageSource.AssetLoader);

        return true;
    }
}

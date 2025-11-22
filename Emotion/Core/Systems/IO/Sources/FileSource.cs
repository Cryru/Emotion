#nullable enable

using System.IO;
using System.Threading.Tasks;

namespace Emotion.Core.Systems.IO.Sources;

public class FileSource : IAssetSource<string>
{
    public static FileReadRoutineResult GetAssetContent(string filePath)
    {
        // Not found in manifest
        if (!File.Exists(filePath))
        {
            var result = new FileReadRoutineResult();
            result.SetData(ReadOnlyMemory<byte>.Empty);
            Engine.Log.Error($"File doesn't exist - {filePath}.", MessageSource.AssetLoader);
            return result;
        }

        // Read asynchronously.
        try
        {
            var result = new FileReadRoutineResult();
            Task<byte[]> task = File.ReadAllBytesAsync(filePath);
            result.SetAsyncTask(task);
            return result;
        }
        catch (Exception)
        {
            return FileReadRoutineResult.GenericErrored;
        }
    }
}
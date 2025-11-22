#nullable enable

using System.IO;
using System.Reflection;

namespace Emotion.Core.Systems.IO.Sources;

public class EmbeddedSource : IAssetSource<EmbeddedSource.LoadData>
{
    public record struct LoadData(Assembly Assembly, string EmbeddedPath);

    public static FileReadRoutineResult GetAssetContent(LoadData loadData)
    {
        string path = loadData.EmbeddedPath;

        // Read the asset from the embedded file.
        using Stream? stream = loadData.Assembly.GetManifestResourceStream(path);

        // Not found.
        if (stream == null)
        {
            Engine.Log.Error($"Couldn't read asset {path}", MessageSource.AssetLoader);
            return FileReadRoutineResult.GenericErrored;
        }

        // Read from stream.
        var data = new byte[stream.Length];
        stream.ReadExactly(data, 0, (int)stream.Length);

        var result = new FileReadRoutineResult();
        result.SetData(data);
        return result;
    }
}
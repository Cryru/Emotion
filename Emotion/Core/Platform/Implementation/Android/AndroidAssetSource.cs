#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using System.IO;
using Activity = Android.App.Activity;

#endregion

namespace Emotion.Core.Platform.Implementation.Android;

public class AndroidAssetSource : AssetSource
{
    private Activity _activity;
    private Dictionary<string, string> _enginePathToFilePath = new();
    private string[] manifest;

    public AndroidAssetSource(Activity activity)
    {
        _activity = activity;

        List<string> assets = PopulateAssetsDown("");
        manifest = assets.ToArray() ?? Array.Empty<string>();

        for (var i = 0; i < manifest.Length; i++)
        {
            string str = manifest[i];
            _enginePathToFilePath.Add(AssetLoader.NameToEngineName(str), str);
        }
    }

    private List<string> PopulateAssetsDown(string path)
    {
        var folderAssets = new List<string>();

        var thisFolder = _activity.Assets?.List(path);
        if (thisFolder == null) return folderAssets;

        for (int i = 0; i < thisFolder.Length; i++)
        {
            string folderOrFile = thisFolder[i];

            string innerPath = path == "" ? folderOrFile : path + "/" + folderOrFile;
            List<string> insideThis = PopulateAssetsDown(innerPath);
            if (insideThis.Count > 0)
            {
                folderAssets.AddRange(insideThis);
            }
            else
            {
                folderAssets.Add(innerPath);
            }
        }

        return folderAssets;
    }

    public override ReadOnlyMemory<byte> GetAsset(string enginePath)
    {
        if (_activity.Assets == null) return null;
        if (!_enginePathToFilePath.ContainsKey(enginePath)) return null;

        string filePath = _enginePathToFilePath[enginePath];
        Stream stream = _activity.Assets.Open(filePath);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public override string[] GetManifest()
    {
        return manifest;
    }

    public override FileReadRoutineResult GetAssetRoutine(string enginePath)
    {
        ReadOnlyMemory<byte> resultBytes = GetAsset(enginePath);
        var result = new FileReadRoutineResult();
        result.SetData(resultBytes);
        return result;
    }
}
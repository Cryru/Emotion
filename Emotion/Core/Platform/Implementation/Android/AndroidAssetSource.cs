#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.IO.Sources;
using System.IO;
using Activity = Android.App.Activity;

#endregion

namespace Emotion.Core.Platform.Implementation.Android;

public class AndroidAssetSource : IAssetSource<string>
{
    private static Activity? _activity;

    public static void MountAssets(Activity activity)
    {
        MountAssetsDown(activity, string.Empty);
        _activity = activity;
    }

    private static bool MountAssetsDown(Activity activity, string path)
    {
        string[]? thisFolder = activity.Assets?.List(path);
        if (thisFolder == null) return false;

        for (int i = 0; i < thisFolder.Length; i++)
        {
            string folderOrFile = thisFolder[i];

            string innerPath = path == "" ? folderOrFile : path + "/" + folderOrFile;
            if (!MountAssetsDown(activity, innerPath))
            {
                Engine.AssetLoader.MountCustomSourceAsset<AndroidAssetSource, string>(innerPath, innerPath);
            }
        }

        return true;
    }

    public static FileReadRoutineResult GetAssetContent(string loadData)
    {
        if (_activity == null) return FileReadRoutineResult.GenericErrored;

        Stream stream = _activity.Assets.Open(loadData);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var resultBytes = memoryStream.ToArray();

        var result = new FileReadRoutineResult();
        result.SetData(resultBytes);
        return result;
    }
}
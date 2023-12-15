#region Using

using Emotion.IO;

#endregion

namespace Emotion.Droid
{
    public class AndroidAssetSource : AssetSource
    {
        private Activity _activity;
        private Dictionary<string, string> _enginePathToFilePath = new();
        private string[] manifest;

        public AndroidAssetSource(Activity activity)
        {
            _activity = activity;
            manifest = _activity.Assets?.List("") ?? Array.Empty<string>();

            for (var i = 0; i < manifest.Length; i++)
            {
                string str = manifest[i];
                _enginePathToFilePath.Add(AssetLoader.NameToEngineName(str), str);
            }
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
    }
}
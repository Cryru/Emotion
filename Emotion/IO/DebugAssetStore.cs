#region Using

using System.IO;
using Emotion.Platform.Implementation.CommonDesktop;

#endregion

namespace Emotion.IO
{
    public class DebugAssetStore : FileAssetStore
    {
        public DebugAssetStore() : base("../../../Assets")
        {
        }

        public override void SaveAsset(byte[] data, string name, bool backup)
        {
            string oldFolder = Folder;
            // Save to project folder.
            base.SaveAsset(data, Path.Join(Folder, name), false);
            // Save to exe folder.
            Folder = "Assets";
            base.SaveAsset(data, Path.Join(Folder, name), false);
            Folder = oldFolder;
        }
    }
}
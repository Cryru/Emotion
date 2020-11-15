#region Using

using System;
using System.Diagnostics;
using System.IO;
using Emotion.Common;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.Win32;

#endregion

namespace Emotion.IO
{
    public class DebugAssetStore : FileAssetStore
    {
        public static string AssetDevPath = Path.Join("..", "..", "..", "Assets");

        // Make sure a mass of explorer windows don't open when saving a lot of assets, such as in an exporter tool.
        private static float _lastExplorerOpenedTime;

        public DebugAssetStore() : base(AssetDevPath)
        {
        }

        public override void SaveAsset(byte[] data, string name, bool backup)
        {
            // Save to project folder.
            base.SaveAsset(data, Path.Join(_folderFs, name), false);
            // Save to exe folder.
            base.SaveAsset(data, Path.Join(".", "Assets", name), false);

            if (Engine.Host is Win32Platform && Engine.TotalTime - _lastExplorerOpenedTime > 20_000)
            {
                _lastExplorerOpenedTime = Engine.TotalTime;
                Process.Start("explorer.exe", $"{Path.GetDirectoryName(Path.Join(_folderFs, name))}");
            }
        }

        public override ReadOnlyMemory<byte> GetAsset(string enginePath)
        {
            enginePath = AssetLoader.JoinPath(AssetLoader.NameToEngineName(Folder), enginePath);
            return base.GetAsset(enginePath);
        }

        public override string ToString()
        {
            return "Debug Asset Source";
        }
    }
}
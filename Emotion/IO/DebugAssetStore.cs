﻿#region Using

using System.IO;
using Emotion.Platform.Implementation.CommonDesktop;

#endregion

namespace Emotion.IO
{
    public class DebugAssetStore : FileAssetStore
    {
        public static string ProjectDevPath = Path.Join("..", "..", "..");
        public static string AssetDevPath = Path.Join(ProjectDevPath, "Assets");

        public DebugAssetStore() : base(AssetDevPath)
        {
        }

        public override void SaveAsset(byte[] data, string name, bool backup)
        {
            // Save to project folder.
            base.SaveAsset(data, Path.Join(_folderFs, name), false);
            // Save to exe folder.
            base.SaveAsset(data, Path.Join(".", "Assets", name), false);

            // This will cause any new assets to be added to the manifest.
            // This is fine since sources are only held as a reference per asset
            // in the current (not very good) system.
            if (!Engine.AssetLoader.Exists(name))
            {
                Engine.AssetLoader.AddSource(new FileAssetSource("Assets"), false);
                Assert(Engine.AssetLoader.Exists(name));
            }
        }

        public override ReadOnlyMemory<byte> GetAsset(string enginePath)
        {
            enginePath = AssetLoader.JoinPath(AssetLoader.NameToEngineName(Folder), enginePath);
            return base.GetAsset(enginePath);
        }

        public static void DeleteFile(string path)
        {
            string oldFileSystemPath = Path.Join(AssetDevPath, path);
            if (File.Exists(oldFileSystemPath)) File.Delete(oldFileSystemPath);
            string assetPath = Path.Join("Assets", path);
            if (File.Exists(assetPath)) File.Delete(assetPath);
        }

        public override string ToString()
        {
            return "Debug Asset Source";
        }
    }
}
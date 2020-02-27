﻿#region Using

using System.IO;
using Emotion.IO;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    /// <inheritdoc cref="FileAssetSource" />
    public class FileAssetStore : FileAssetSource, IAssetStore
    {
        /// <inheritdoc />
        public FileAssetStore(string folder) : base(folder)
        {
        }

        /// <inheritdoc />
        public void SaveAsset(byte[] data, string name, bool backup)
        {
            string filePath = EnginePathToFilePath(name);

            if (!File.Exists(filePath))
            {
                // If new - add to the internal manifest.
                InternalManifest.TryAdd(name, filePath);
            }
            else if(backup)
            {
                // Backup old - if any.
                File.Copy(filePath, filePath + ".backup", true);
            }

            // Create missing directories.
            string directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName))
                Directory.CreateDirectory(directoryName);
            else
                filePath = Path.Join($"{Folder}", filePath);

            FileStream stream = File.Open(filePath, FileMode.Create);
            stream.Write(data, 0, data.Length);
            stream.Dispose();
        }

        /// <summary>
        /// Convert an engine path to a file path.
        /// </summary>
        /// <param name="enginePath">The engine path to convert.</param>
        /// <returns>The file path corresponding to the specified engine path.</returns>
        protected static string EnginePathToFilePath(string enginePath)
        {
            return enginePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <inheritdoc />
        protected override string FilePathToEnginePath(string filePath)
        {
            return AssetLoader.NameToEngineName(filePath);
        }

        public override string ToString()
        {
            return $".Net System.IO Store @ ./{Folder}";
        }
    }
}
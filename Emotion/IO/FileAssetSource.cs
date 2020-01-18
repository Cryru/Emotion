#region Using

using System;
using System.IO;
using System.Linq;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    /// <inheritdoc />
    public class FileAssetSource : AssetSource
    {
        /// <summary>
        /// The folder assets will be read from.
        /// </summary>
        public string Folder { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// The folder relative to the executable where file assets will be loaded from.
        /// </summary>
        /// <param name="folder">The folder to load assets from.</param>
        public FileAssetSource(string folder)
        {
            Folder = folder;

            // Check if folder exists.
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

            // Populate internal manifest.
            string[] files = Directory.GetFiles(Folder, "*", SearchOption.AllDirectories);
            files.AsParallel().ForAll(x => InternalManifest.TryAdd(FilePathToEnginePath(x), x));
        }

        /// <inheritdoc />
        public override byte[] GetAsset(string enginePath)
        {
            // Convert to file path.
            bool found = InternalManifest.TryGetValue(enginePath, out string filePath);

            // Check if found.
            if (!found) return new byte[0];

            // Check if exists.
            if (File.Exists(filePath)) return File.ReadAllBytes(filePath);

            Engine.Log.Error($"Couldn't read asset {enginePath} with file path {filePath}.", MessageSource.AssetLoader);
            return new byte[0];
        }

        /// <inheritdoc />
        public override DateTime GetAssetModified(string enginePath)
        {
            // Convert to file path.
            bool found = InternalManifest.TryGetValue(enginePath, out string filePath);

            // Check if found.
            if (!found) return base.GetAssetModified(enginePath);

            // The API actually allows for a file to be modified before it was created. lol
            DateTime lastWrite = File.GetLastWriteTimeUtc(filePath);
            DateTime creationTime = File.GetCreationTimeUtc(filePath);
            int later = DateTime.Compare(lastWrite, creationTime);

            return later > 0 ? lastWrite : creationTime;
        }

        /// <summary>
        /// Convert a file path of any type to an engine path.
        /// </summary>
        /// <param name="filePath">The file path to convert.</param>
        /// <returns>The engine path corresponding to the specified file path.</returns>
        private string FilePathToEnginePath(string filePath)
        {
            return AssetLoader.NameToEngineName(filePath.Replace(Folder + Path.DirectorySeparatorChar, ""));
        }
    }
}
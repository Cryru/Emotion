#region Using

using System;
using System.IO;
using System.Linq;
using Adfectus.Common;
using Adfectus.IO;
using Adfectus.Logging;

#endregion

namespace Emotion.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public sealed class FileAssetSource : AssetSource
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
            files.AsParallel().ForAll(x => InternalManifest.TryAdd(FilePathToCrossPlatform(x), x));
        }

        /// <inheritdoc />
        public override byte[] GetAsset(string enginePath)
        {
            // Convert to file path.
            bool found = InternalManifest.TryGetValue(enginePath, out string filePath);

            // Check if found.
            if (!found)
            {
                Engine.Log.Error($"Couldn't find asset {enginePath}.", MessageSource.AssetLoader);
                return new byte[0];
            }

            // Check if exists.
            if (File.Exists(filePath)) return File.ReadAllBytes(filePath);

            Engine.Log.Error($"Couldn't read asset {enginePath} with file path {filePath}.", MessageSource.AssetLoader);
            return new byte[0];
        }

        /// <summary>
        /// Convert a file path of any type to an engine path.
        /// </summary>
        /// <param name="filePath">The file path to convert.</param>
        /// <returns>The engine path corresponding to the specified file path.</returns>
        public string FilePathToCrossPlatform(string filePath)
        {
            return filePath.Replace('/', '$').Replace('\\', '$').Replace('$', '/').Replace(Folder + "/", "");
        }
    }
}
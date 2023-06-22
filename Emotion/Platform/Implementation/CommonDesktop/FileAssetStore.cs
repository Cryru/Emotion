#region Using

using System.IO;
using Emotion.IO;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    /// <inheritdoc cref="FileAssetSource" />
    public class FileAssetStore : FileAssetSource, IAssetStore
    {
        /// <inheritdoc />
        public FileAssetStore(string folder) : base(folder, true)
        {
        }

        /// <inheritdoc />
        public virtual void SaveAsset(byte[] data, string name, bool backup)
        {
            string filePath = EnginePathToFilePath(name);
            filePath = filePath.Replace(_folderFs.ToLower(), _folderFs);

            if (!File.Exists(filePath))
            {
                // If new - add to the internal manifest.
                InternalManifest.TryAdd(FilePathToEnginePath(filePath, FolderInPath), filePath);
            }
            else if (backup)
            {
                // Backup old - if any.
                File.Copy(filePath, filePath + ".backup", true);
            }

            // Create missing directories.
            string directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName))
                Directory.CreateDirectory(directoryName);

            // Save to another file, and rename to the target file to ensure
            // no corruption occurs when saving.
            string tempFile = filePath + ".temp";
            FileStream stream = File.Open(tempFile, FileMode.Create);
            stream.Write(data, 0, data.Length);
            stream.Dispose();

            File.Copy(tempFile, filePath, true);
            File.Delete(tempFile);
        }

        /// <summary>
        /// Convert an engine path to a file path.
        /// </summary>
        /// <param name="enginePath">The engine path to convert.</param>
        /// <returns>The file path corresponding to the specified engine path.</returns>
        protected static string EnginePathToFilePath(string enginePath)
        {
            return "." + Path.DirectorySeparatorChar + enginePath.Replace('/', Path.DirectorySeparatorChar);
        }

        ///// <inheritdoc />
        //protected override string FilePathToEnginePath(string filePath, bool includeFolder = false)
        //{
        //    return AssetLoader.NameToEngineName(filePath);
        //}

        public override string ToString()
        {
            return $".Net System.IO Store @ {_folderFs}";
        }
    }
}
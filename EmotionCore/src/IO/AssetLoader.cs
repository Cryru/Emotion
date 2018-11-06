// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Emotion.Debug;
using Soul;

#endregion

namespace Emotion.IO
{
    public class AssetLoader
    {
        #region Properties

        /// <summary>
        /// The root directory in which assets are located.
        /// </summary>
        public string RootDirectory = "Assets";

        #endregion

        #region Objects

        /// <summary>
        /// Loaded assets.
        /// </summary>
        private Dictionary<string, Asset> _loadedAssets;

        #endregion

        internal AssetLoader()
        {
            _loadedAssets = new Dictionary<string, Asset>();
        }

        /// <summary>
        /// Returns an asset, if not loaded loads it.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="path">A file path to the asset with the RootDirectory as the parent. Will be converted to an engine path.</param>
        /// <returns>A loaded asset.</returns>
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public T Get<T>(string path) where T : Asset
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            lock (_loadedAssets)
            {
                // Check if the asset is already loaded, in which case return it.
                if (_loadedAssets.ContainsKey(enginePath)) return (T) _loadedAssets[enginePath];
            }

            // Check whether the file exists.
            if (!Exists(enginePath)) throw new Exception("Could not find asset " + enginePath);

            // Read the file.
            byte[] fileContents = ReadFile(enginePath);

            // Create an instance of the asset and add it.
            DebugMessageWrap("Creating", enginePath, typeof(T), MessageType.Info);
            T temp = (T) Activator.CreateInstance(typeof(T));
            temp.Name = enginePath;
            temp.Create(fileContents);
            lock (_loadedAssets)
            {
                _loadedAssets.Add(enginePath, temp);
            }

            DebugMessageWrap("Created", enginePath, typeof(T), MessageType.Trace);

            // Return.
            return temp;
        }

        /// <summary>
        /// Destroy an asset, freeing memory.
        /// </summary>
        /// <param name="path">A path to the asset. Will be converted to an engine path.</param>
        public void Destroy(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);
            Asset asset;

            lock (_loadedAssets)
            {
                // Check if loaded.
                if (!_loadedAssets.ContainsKey(enginePath)) return;

                // Log that destruction will commence.
                DebugMessageWrap("Destroying", enginePath, _loadedAssets[enginePath].GetType(), MessageType.Info);

                // Assign to destroy the asset outside of the lock.
                asset = _loadedAssets[enginePath];

                // Remove from the list.
                _loadedAssets.Remove(enginePath);
            }

            asset.Destroy();
            DebugMessageWrap("Destroyed", enginePath, null, MessageType.Trace);
        }

        #region Helpers

        /// <summary>
        /// Returns whether the specified file exists.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        public bool Exists(string path)
        {
            return File.Exists(AssetPathToAssetCrossPlatform(path));
        }

        /// <summary>
        /// Reads a file and returns its contents as a byte array.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The contents of the file as a byte array.</returns>
        private byte[] ReadFile(string path)
        {
            string parsedPath = AssetPathToAssetCrossPlatform(path);

            if (!File.Exists(parsedPath)) throw new Exception("The file " + parsedPath + " could not be found.");

            // Load the bytes of the file.
            return File.ReadAllBytes(parsedPath);
        }

        /// <summary>
        /// Converts the provided path to an engine universal format,
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        public static string PathToEnginePath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', '/');
        }

        /// <summary>
        /// Converts the provided path to the current platform's path signature.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        private string AssetPathToAssetCrossPlatform(string path)
        {
            return RootDirectory + Path.DirectorySeparatorChar + Utilities.CrossPlatformPath(path);
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Post a formatted asset loader debug message.
        /// </summary>
        /// <param name="operation">The operation being performed.</param>
        /// <param name="path">An engine path to the file.</param>
        /// <param name="type">The type of asset.</param>
        /// <param name="messageType">The type of message to log.</param>
        private void DebugMessageWrap(string operation, string path, Type type, MessageType messageType)
        {
            Debugger.Log(messageType, MessageSource.AssetLoader, operation + " asset [" + path + "]" + (type != null ? " of type " + type : ""));
        }

        #endregion
    }
}
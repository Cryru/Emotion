// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Emotion.Debug;
using Emotion.Engine;

#endregion

namespace Emotion.IO
{
    public class AssetLoader : ContextObject
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

        /// <summary>
        /// Assets loaded outside the GL thread. Awaiting upload.
        /// </summary>
        private List<Asset> _assetUploadQueue;

        /// <summary>
        /// Assets being unloaded outside the GL thread. Awaiting destruction.
        /// </summary>
        private List<Asset> _assetUnloadQueue;

        #endregion

        internal AssetLoader(Context context) : base(context)
        {
            _loadedAssets = new Dictionary<string, Asset>();
            _assetUploadQueue = new List<Asset>();
            _assetUnloadQueue = new List<Asset>();
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

            // Check if the asset is already loaded, in which case return it.
            if (_loadedAssets.ContainsKey(enginePath))
            {
                // Get the asset.
                Asset asset = _loadedAssets[enginePath];

                // Check state.
                switch (_loadedAssets[enginePath].State)
                {
                    // Check if needs to be uploaded.
                    case AssetState.Processing:
                        // Check if on the GL thread and we can upload it.
                        if (Thread.CurrentThread.ManagedThreadId == 1)
                        {
                            DebugMessageWrap("Uploading", enginePath, typeof(T));

                            // Perform uploading.
                            asset.ProcessNative();
                            return (T) asset;
                        }
                        else
                        {
                            DebugMessageWrap("Returning non-uploaded", enginePath, typeof(T), true);

                            // Add the asset to the upload queue.
                            _assetUploadQueue.Add(asset);

                            return (T) asset;
                        }
                    // Check if processed and uploaded and ready for usage.
                    case AssetState.Processed:
                        return (T) asset;
                    // If it was destroyed, remove it and reload it.
                    case AssetState.Destroyed:
                        _loadedAssets.Remove(enginePath);
                        return Get<T>(enginePath);
                    default:
                        return null;
                }
            }

            DebugMessageWrap("Processing", enginePath, typeof(T));

            // Check whether the file exists.
            if (!Exists(enginePath)) throw new Exception("Could not find asset " + enginePath);

            // Read the file.
            byte[] fileContents = ReadFile(enginePath);

            // Create an instance of the asset.
            T temp = (T) Activator.CreateInstance(typeof(T));
            temp.AssetName = enginePath;
            _loadedAssets[enginePath] = temp;

            // Process.
            temp.Process(fileContents);

            // Check if needs to be processed native.
            if (temp.State == AssetState.Processing) return Get<T>(enginePath);

            // Check if ready.
            if (temp.State == AssetState.Processed) return temp;

            return null;
        }

        /// <summary>
        /// Free an asset unloading and destroying it.
        /// </summary>
        /// <param name="path">A path to the asset. Will be converted to an engine path.</param>
        public void Free(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if loaded.
            if (!_loadedAssets.ContainsKey(enginePath)) return;

            // Destroy it.
            _loadedAssets[enginePath].Destroy();
            
            // Check if fully destroyed.
            if (_loadedAssets[enginePath].State == AssetState.Destroying)
            {
                // Check if on the GL thread.
                if (Thread.CurrentThread.ManagedThreadId == 1)
                {
                    _loadedAssets[enginePath].DestroyNative();
                }
                else
                {
                    _assetUnloadQueue.Add(_loadedAssets[enginePath]);
                }
            }

            _loadedAssets.Remove(enginePath);
        }

        /// <summary>
        /// Processes assets awaiting upload.
        /// </summary>
        internal void Update()
        {
            // Check for any assets left to upload.
            for (int i = _assetUploadQueue.Count - 1; i >= 0; i--)
            {
                // Check if loaded.
                if (_assetUploadQueue[i].State == AssetState.Processed) continue;

                DebugMessageWrap("Uploading from queue", _assetUploadQueue[i].AssetName, _assetUploadQueue[i].GetType(), true);

                // Upload the asset.
                _assetUploadQueue[i].ProcessNative();
                // Add it to the loaded assets.
                _assetUploadQueue.RemoveAt(i);
            }

            // Check for any assets left to upload.
            for (int i = _assetUnloadQueue.Count - 1; i >= 0; i--)
            {
                // Check if destroyed.
                if (_assetUnloadQueue[i].State == AssetState.Destroyed) continue;

                DebugMessageWrap("Destroying form queue", _assetUploadQueue[i].AssetName, _assetUploadQueue[i].GetType(), true);

                // Upload the asset.
                _assetUnloadQueue[i].DestroyNative();
                // Add it to the loaded assets.
                _assetUnloadQueue.RemoveAt(i);
            }
        }

        #region Helpers

        /// <summary>
        /// Returns whether the specified file exists.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        public bool Exists(string path)
        {
            return File.Exists(PathToCrossPlatform(path));
        }

        /// <summary>
        /// Reads a file and returns its contents as a byte array.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The contents of the file as a byte array.</returns>
        private byte[] ReadFile(string path)
        {
            string parsedPath = PathToCrossPlatform(path);

            if (!File.Exists(parsedPath)) throw new Exception("The file " + parsedPath + " could not be found.");

            // Load the bytes of the file.
            return File.ReadAllBytes(parsedPath);
        }

        /// <summary>
        /// Converts the provided path to an engine universal format,
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        private static string PathToEnginePath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', '/');
        }

        /// <summary>
        /// Converts the provided path to the current platform's path signature.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        private string PathToCrossPlatform(string path)
        {
            return RootDirectory + Path.DirectorySeparatorChar + path.Replace('/', '$').Replace('\\', '$').Replace('$', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Post a formatted asset loader debug message.
        /// </summary>
        /// <param name="operation">The operation being performed.</param>
        /// <param name="path">An engine path to the file.</param>
        /// <param name="type">The type of asset.</param>
        /// <param name="warning">Whether the message is a warning.</param>
        private void DebugMessageWrap(string operation, string path, Type type, bool warning = false)
        {
            Debugger.Log(warning ? MessageType.Warning : MessageType.Trace, MessageSource.AssetLoader, operation + " asset [" + path + "] of type " + type);
        }

        #endregion
    }
}
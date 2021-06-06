#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.IO.AssetPack
{
    /// <summary>
    /// Load a package of assets created by the Emotion.PostBuildTool
    /// </summary>
    public abstract class PackedAssetSource : AssetSource
    {
        /// <summary>
        /// Whether the source is ready. It should only be attached to the AssetLoader when ready.
        /// </summary>
        public bool Ready
        {
            get => LoadingTask != null && LoadingTask.IsCompleted;
        }

        /// <summary>
        /// The task where all blobs packages are loaded into memory.
        /// </summary>
        public Task LoadingTask;

        /// <summary>
        /// How far along are we to loading all blob packages. 0-1
        /// </summary>
        public float Progress;

        private string _blobDirectory;
        private AssetBlobManifest _manifest;
        private Dictionary<string, (int blobIdx, BlobFile blobFile)> _fileManifest;
        private string[] _manifestNames;
        private byte[][] _blobContents;

        protected PackedAssetSource(string directory)
        {
            _blobDirectory = directory;
            _manifestNames = Array.Empty<string>();
        }

        public void StartLoad()
        {
            LoadingTask = Load();
        }

        protected async Task Load()
        {
            try
            {
                string manifestPath = Path.Join(_blobDirectory, "manifest.xml");
                byte[] manifestBytes = await GetFileContent(manifestPath);
                if (manifestBytes == null)
                {
                    Engine.Log.Error($"Couldn't retrieve packed asset source manifest {manifestPath}", MessageSource.PackedAssetSource);
                    return;
                }

                _manifest = XMLFormat.From<AssetBlobManifest>(manifestBytes);

                // Build file manifest.
                _fileManifest = new Dictionary<string, (int blobIdx, BlobFile blobFile)>();
                int numberOfBlobs = _manifest.Blobs.Length;
                for (var i = 0; i < numberOfBlobs; i++)
                {
                    AssetBlob currentBlob = _manifest.Blobs[i];
                    foreach ((string fileName, BlobFile file) in currentBlob.BlobMeta)
                    {
                        _fileManifest.Add(AssetLoader.NameToEngineName(fileName), (currentBlob.Index, file));
                    }
                }

                // Start loading individual blobs.
                _blobContents = new byte[numberOfBlobs][];
                var blobLoadingTasks = new Task[numberOfBlobs];
                float progressPerBlob = 1.0f / numberOfBlobs;
                for (var i = 0; i < numberOfBlobs; i++)
                {
                    blobLoadingTasks[i] = LoadBlob(i).ContinueWith(_ => { Progress += progressPerBlob; });
                }

                await Task.WhenAll(blobLoadingTasks);

                // Convert manifest.
                Progress = 1f;
                _manifestNames = _fileManifest.Keys.ToArray();
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Error in loading packed asset source - {ex}.", MessageSource.PackedAssetSource);
            }
        }

        protected async Task LoadBlob(int blobIdx)
        {
            try
            {
                string blobName = Path.Join(_blobDirectory, $"{_manifest.BlobNamePrefix}{blobIdx}.bin");
                byte[] content = await GetFileContent(blobName);

                if (content == null)
                {
                    Engine.Log.Error($"Couldn't load asset blob {blobName}. Removing its assets from the manifest.", MessageSource.PackedAssetSource);
                    var removeKeys = new List<string>();
                    foreach ((string fileName, (int blobIdx, BlobFile blobFile) value) in _fileManifest)
                    {
                        if (value.blobIdx == blobIdx) removeKeys.Add(fileName);
                    }

                    for (var i = 0; i < removeKeys.Count; i++)
                    {
                        _fileManifest.Remove(removeKeys[i]);
                    }

                    _blobContents[blobIdx] = null;
                }
                else
                {
                    _blobContents[blobIdx] = content;
                    Engine.Log.Trace($"Loaded asset blob {blobName} of size {Helpers.FormatByteAmountAsString(content.Length)}!", MessageSource.PackedAssetSource);
                }
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Error in loading asset blob {blobIdx} - {ex}.", MessageSource.PackedAssetSource);
            }
        }

        public override string[] GetManifest()
        {
            return _manifestNames;
        }

        public override ReadOnlyMemory<byte> GetAsset(string enginePath)
        {
            // Try to get the index of the blob which contains this asset from the manifest.
            if (_fileManifest == null) return null;
            if (!_fileManifest.TryGetValue(enginePath, out (int blobIdx, BlobFile blobFile) assetLocation)) return null;

            // Try to get the blob which contains the asset.
            if (_blobContents.Length - 1 < assetLocation.blobIdx || _blobContents[assetLocation.blobIdx] == null) return null;
            byte[] blob = _blobContents[assetLocation.blobIdx];

            // Read the asset from the blob offset.
            return new ReadOnlyMemory<byte>(blob).Slice(assetLocation.blobFile.Offset, assetLocation.blobFile.Length);
        }

        public override string ToString()
        {
            return $"PackedAssetSource @ {_blobDirectory}";
        }

        /// <summary>
        /// Implementation dependent file retrieve. Used for the manifest and the blobs.
        /// </summary>
        protected abstract Task<byte[]> GetFileContent(string fileName);
    }
}
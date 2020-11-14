#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.XML;

#endregion

namespace Emotion.IO.AssetPack
{
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
                byte[] manifestBytes = await GetFileContent(Path.Join(_blobDirectory, "manifest.xml"));
                if (manifestBytes == null) return;

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

                _blobContents = new byte[numberOfBlobs][];
                var blobLoadingTasks = new Task[numberOfBlobs];
                float progressPerBlob = 1.0f / numberOfBlobs;
                for (var i = 0; i < _blobContents.Length; i++)
                {
                    blobLoadingTasks[i] = LoadBlob(i).ContinueWith(_ => { Progress += progressPerBlob; });
                }

                await Task.WhenAll(blobLoadingTasks);

                Progress = 1f;
                _manifestNames = _fileManifest.Keys.ToArray();
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Error in loading packed asset source - {ex}.", "PackedAssetSource");
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
                    Engine.Log.Error($"Couldn't load asset blob {blobName}. Removing its assets from the manifest.", "PackedAssetSource");
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
                    Engine.Log.Trace($"Loaded asset blob {blobName} of size {content.Length / 1024f / 1024f:0.00}MB!", "PackedAssetSource");
                }
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Error in loading asset blob {blobIdx} - {ex}.", "PackedAssetSource");
            }
        }

        public override string[] GetManifest()
        {
            return _manifestNames;
        }

        public override byte[] GetAsset(string enginePath)
        {
            if (_fileManifest == null) return null;
            if (!_fileManifest.TryGetValue(enginePath, out (int blobIdx, BlobFile blobFile) assetLocation)) return null;
            // Try to find the blob.
            if (_blobContents.Length - 1 < assetLocation.blobIdx || _blobContents[assetLocation.blobIdx] == null) return null;
            byte[] blob = _blobContents[assetLocation.blobIdx];

            // Copy the file contents to a new array.
            var file = new byte[assetLocation.blobFile.Length];
            Array.Copy(blob, assetLocation.blobFile.Offset, file, 0, file.Length);
            return file;
        }

        public override string ToString()
        {
            return $"PackedAssetSource @ {_blobDirectory}";
        }

        protected abstract Task<byte[]> GetFileContent(string fileName);
    }
}
#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Emotion.Common;
using Emotion.IO;
using Emotion.IO.AssetPack;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.PostBuildTool
{
    public static class EmotionWebPostPublish
    {
        public static string InputFolder = Path.Join(".", "Assets");
        public static string OutputDirectory = Path.Join(".", "wwwroot", "AssetBlobs");
        public static int MaxBlobSize = 52428800; // 50mb
        public static string BlobNamePrefix = "AssetBlob";

        public static void Main(string[] args)
        {
            Engine.LightSetup(new Configurator
            {
                DebugMode = true
            });

            if (!Directory.Exists(InputFolder))
            {
                Engine.Log.Warning($"Didn't find input folder - {InputFolder}.", "Script");
                return;
            }

            // Get files.
            string[] files = Directory.GetFiles(InputFolder, "*", SearchOption.AllDirectories);
            Engine.Log.Info($"Found {files.Length} files.", "Script");
            if (files.Length == 0) return;

            // Ensure output exists and is empty.
            if (Directory.Exists(OutputDirectory)) Directory.Delete(OutputDirectory, true);
            Directory.CreateDirectory(OutputDirectory);

            // Generate blobs.
            var blobs = new List<AssetBlob>();
            AssetBlob currentBlob = null;
            FileStream blobWriteStream = null;
            var blobFileSize = 0;
            for (var i = 0; i < files.Length; i++)
            {
                if (currentBlob == null)
                {
                    currentBlob = GenerateAssetBlob();
                    blobs.Add(currentBlob);
                    blobWriteStream = File.Create(Path.Join(OutputDirectory, currentBlob.Name));
                    blobFileSize = 0;
                }

                string fileName = files[i];
                string internalName = fileName.Replace(InputFolder + Path.DirectorySeparatorChar, "");
                byte[] bytes = File.ReadAllBytes(fileName);

                currentBlob.BlobMeta.Add(internalName, new BlobFile(blobFileSize, bytes.Length));
                blobWriteStream.Write(bytes);
                blobFileSize += bytes.Length;
                Engine.Log.Info($"   Writing file {fileName} [{internalName}] of size {Helpers.FormatByteAmountAsString(bytes.Length)}", "Script");

                bool lastFile = i == files.Length - 1;
                int nextFileSize = lastFile ? 0 : (int) new FileInfo(files[i + 1]).Length;
                if (blobFileSize + nextFileSize >= MaxBlobSize || lastFile)
                {
                    Engine.Log.Info($"  Finished blob {currentBlob.Name} with size {Helpers.FormatByteAmountAsString(blobFileSize)}", "Script");
                    blobWriteStream.Close();
                    blobWriteStream.Dispose();
                    currentBlob = null;
                }
            }

            Engine.Log.Info($"Building complete. {blobs.Count} blobs created!", "Script");

            // Generate manifest.
            var manifest = new AssetBlobManifest
            {
                Blobs = blobs.ToArray()
            };
            string xml = XMLFormat.To(manifest);
            File.WriteAllText(Path.Join(OutputDirectory, "manifest.xml"), xml);
            Engine.Log.Info("Manifest written.", "Script");

            // Verification step.
            Engine.Log.Info("Verifying assets...", "Script");
            var filePackedSource = new FilePackedAssetSource(OutputDirectory);
            filePackedSource.StartLoad();
            filePackedSource.LoadingTask.Wait();

            for (var i = 0; i < manifest.Blobs.Length; i++)
            {
                AssetBlob blob = manifest.Blobs[i];
                foreach ((string blobPath, BlobFile _) in blob.BlobMeta)
                {
                    string realName = Path.Join(InputFolder, blobPath);
                    Engine.Log.Info($" Checking file {realName}...", "Script");

                    byte[] bytesFileSystem = File.ReadAllBytes(realName);
                    ReadOnlySpan<byte> bytesBlob = filePackedSource.GetAsset(AssetLoader.NameToEngineName(blobPath)).Span;

                    Debug.Assert(bytesFileSystem.Length == bytesBlob.Length);
                    for (var j = 0; j < bytesFileSystem.Length; j++)
                    {
                        byte byteFile = bytesFileSystem[j];
                        byte byteBlob = bytesBlob[j];
                        Debug.Assert(byteFile == byteBlob);
                    }
                }
            }

            Engine.Log.Info("Done!", "Script");
        }

        private static int _nextBlobIdx;
        private static object _nextBlobLock = new();

        public static AssetBlob GenerateAssetBlob()
        {
            var blobInst = new AssetBlob();
            lock (_nextBlobLock)
            {
                int idx = _nextBlobIdx;
                var name = $"{BlobNamePrefix}{idx}.bin";
                blobInst.Name = name;
                blobInst.Index = idx;
                _nextBlobIdx++;
                Engine.Log.Info($"  Starting blob {name}.", "Script");
            }

            return blobInst;
        }
    }
}
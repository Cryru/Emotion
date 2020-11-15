#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Emotion.Common;
using Emotion.IO;
using Emotion.IO.AssetPack;
using Emotion.Standard.XML;

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
            var blobs = new ConcurrentBag<AssetBlob>();
            AssetBlob b = GenerateAssetBlob();
            blobs.Add(b);
            FileStream str = File.Create(Path.Join(OutputDirectory, b.Name));
            var blobFileSize = 0;

            for (var j = 0; j < files.Length; j++)
            {
                string fileName = files[j];
                string internalName = fileName.Replace(InputFolder + Path.DirectorySeparatorChar, "");

                byte[] bytes = File.ReadAllBytes(fileName);
                if (blobFileSize + bytes.Length >= MaxBlobSize)
                {
                    Engine.Log.Info($"  Finished blob {b.Name} with size {blobFileSize / 1024 / 1024}MB.", "Script");
                    str.Flush();
                    str.Dispose();
                    b = GenerateAssetBlob();
                    blobs.Add(b);
                    str = File.Create(Path.Join(OutputDirectory, b.Name));
                    blobFileSize = 0;
                }

                b.BlobMeta.Add(internalName, new BlobFile(blobFileSize, bytes.Length));
                str.Write(bytes);
                blobFileSize += bytes.Length;
                Engine.Log.Info($"   File {fileName} [{internalName}] of size {bytes.Length / 1024}KB added to blob {b.Name}.", "Script");
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

            // Verification step
            Engine.Log.Info("Verifying assets...", "Script");
            var filePackedSource = new FilePackedAssetSource(OutputDirectory);
            filePackedSource.StartLoad();
            filePackedSource.LoadingTask.Wait();

            for (var i = 0; i < manifest.Blobs.Length; i++)
            {
                AssetBlob currentBlob = manifest.Blobs[i];
                foreach (KeyValuePair<string, BlobFile> file in currentBlob.BlobMeta)
                {
                    string realName = Path.Join(InputFolder, file.Key);
                    Engine.Log.Info($" Checking file {realName}...", "Script");

                    byte[] bytesFileSystem = File.ReadAllBytes(realName);
                    ReadOnlySpan<byte> bytesBlob = filePackedSource.GetAsset(AssetLoader.NameToEngineName(file.Key)).Span;

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
        private static object _nextBlobLock = new object();

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

        // Unused
        public static string[] GetFilesForThread(string[] allFiles, int threadCount, int threadId)
        {
            int perThread = allFiles.Length / threadCount;
            int startIdx = perThread * threadId;
            int count = perThread;
            if (threadId == threadCount - 1)
            {
                count += allFiles.Length - perThread * threadCount;
                Debug.Assert(startIdx + count == allFiles.Length);
            }

            var section = new string[count];
            Array.Copy(allFiles, startIdx, section, 0, section.Length);
            return section;
        }
    }
}
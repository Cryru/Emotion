// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrotliSharpLib;
using Soul.Encryption;
using Soul.IO;

#endregion

namespace Soul.Engine.AssetPacker
{
    internal class Program
    {
        #region Settings

        /// <summary>
        /// The path where the project will be built.
        /// </summary>
        internal static string OutputPath;

        /// <summary>
        /// The key assets will be encrypted with.
        /// </summary>
        internal static string EncryptionKey;

        /// <summary>
        /// A list of excluded files not to add.
        /// </summary>
        internal static List<string> ExcludedFiles = new List<string> {"Thumbs.db"};

        #endregion

        #region Runtime Properties

        /// <summary>
        /// The path to the unbuilt assets folder.
        /// </summary>
        public static string AssetsPath;

        /// <summary>
        /// The path to the assets cache.
        /// </summary>
        public static string CachePath;

        /// <summary>
        /// The assets encryption service.
        /// </summary>
        private static SymmetricEncryptionService _service;

        /// <summary>
        /// The hash of the meta assets file. This is the string which will be embedded.
        /// </summary>
        private static string _metaHash = "";

        #endregion

        #region Runtime Data

        /// <summary>
        /// Combined data of build and unbuilt files.
        /// </summary>
        private static List<AssetFile> _assetFiles = new List<AssetFile>();

        #endregion

        private static void Main(string[] args)
        {
            Console.WriteLine("================================");
            Console.WriteLine("SoulEngine Asset Packer ");
            Console.WriteLine("Version " + Meta.Version);
            Console.WriteLine("================================");

            Stopwatch timeTracker = new Stopwatch();
            timeTracker.Start();

            // Apply settings.
            ApplySettings(args);

            // Get the assets folder.
            AssetsPath = Path.Combine(OutputPath, "Assets");
            if (!Directory.Exists(AssetsPath))
            {
                Console.WriteLine("No 'Assets' folder found in the build location.");
                return;
            }

            // Get the cache folder.
            CachePath = Path.Combine(OutputPath, ".buildCache");
            if (!Directory.Exists(CachePath)) Directory.CreateDirectory(CachePath);

            // Load unbuilt and built files.
            LoadFiles();

            // Execute actions, and get whether we have to rebuild the meta.
            bool rebuildMeta = ProcessFiles();

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (rebuildMeta)
            {
                BuildAssetPackage();
                Console.WriteLine("Fragments synced, meta and assets package rebuilt.");
            }
            else
            {
                Console.WriteLine("Fragments up to date, nothing to rebuild.");
            }

            timeTracker.Stop();
            Console.WriteLine("Ready in " + timeTracker.ElapsedMilliseconds + " ms");
        }


        /// <summary>
        /// Apply settings. The OutputPath and the encryption key.
        /// </summary>
        /// <param name="args"></param>
        private static void ApplySettings(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Get output path.
            if (args.Length < 1)
            {
                Console.WriteLine("No output file inputted, using execution folder.");
                OutputPath = "";
            }
            else
            {
                OutputPath = args[0];
            }

            if (OutputPath == null)
            {
                Console.WriteLine("The output path cannot be null.");
                return;
            }

            if (OutputPath == "")
                OutputPath = AppDomain.CurrentDomain.BaseDirectory;
            // Output path gotten.

            // Get encryption key.
            if (args.Length < 2)
            {
                Console.WriteLine("No encryption key inputted, generating a new encryption key.");
                EncryptionKey = GenerateRandomString(64);
            }
            else
            {
                EncryptionKey = args[1];
            }

            if (EncryptionKey == null)
            {
                Console.WriteLine("The encryption key cannot be null.");
                return;
            }

            if (EncryptionKey == "")
            {
                Console.WriteLine("Encryption key is empty, generating a new encryption key.");
                OutputPath = GenerateRandomString(64);
            }
            _service = new SymmetricEncryptionService(EncryptionKey);
            // Encryption key gotten.

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Loads built and unbuilt files.
        /// </summary>
        private static void LoadFiles()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            // ReSharper disable once InconsistentNaming
            List<AssetFile> _unbuiltFiles = new List<AssetFile>();
            // ReSharper disable once InconsistentNaming
            List<AssetFile> _builtFiles = new List<AssetFile>();

            // Get the unbuilt files.
            string[] unbuiltFiles = Directory.GetFiles(AssetsPath, "*", SearchOption.AllDirectories);
            Console.WriteLine("Found " + unbuiltFiles.Length + " files to build.");

            foreach (string fileName in unbuiltFiles)
            {
                AssetFile temp = new AssetFile
                {
                    Path = fileName,
                    TimeUpdated = File.GetLastWriteTime(fileName).Ticks
                };

                // Check if that is an excluded file.
                if (ExcludedFiles.IndexOf(temp.FullName) != -1) continue;

                _unbuiltFiles.Add(temp);
            }

            // Get the built files.
            string[] builtFiles = Directory.GetFiles(CachePath, "*", SearchOption.AllDirectories);
            Console.WriteLine("Found " + builtFiles.Length + " build fragments.");

            foreach (string fileName in builtFiles)
            {
                AssetFile temp = new AssetFile
                {
                    Path = fileName,
                    TimeUpdated = File.GetLastWriteTime(fileName).Ticks
                };

                // Check if that is an excluded file.
                if (ExcludedFiles.IndexOf(temp.FullName) != -1) continue;

                _builtFiles.Add(temp);
            }

            // Compare and merge the two file lists.
            foreach (AssetFile unbuiltFile in _unbuiltFiles)
            {
                AssetFile temp = new AssetFile
                {
                    Path = unbuiltFile.Path,
                    TimeUpdated = unbuiltFile.TimeUpdated
                };

                // Check if this file has been built.
                AssetFile foundFile = _builtFiles.FirstOrDefault(x => x.Name == unbuiltFile.Name);

                if (foundFile == null)
                {
                    temp.Status = Status.Added;
                }
                else
                {
                    temp.Status = Status.Unchanged;

                    // Check if updated.
                    if (temp.TimeUpdated > foundFile.TimeUpdated)
                        temp.Status = Status.Updated;

                    // Remove the file from the list.
                    _builtFiles.Remove(foundFile);
                }

                // Add the file to the list.
                _assetFiles.Add(temp);
            }

            // Check for deleted files, those are the only left in the build files list.
            foreach (AssetFile builtFile in _builtFiles)
            {
                AssetFile temp = new AssetFile
                {
                    Path = builtFile.Path,
                    Status = Status.Deleted
                };

                // Add the file to the list.
                _assetFiles.Add(temp);
            }

            // Report actions.
            for (int i = 0; i < _assetFiles.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(i + 1 + ". " + _assetFiles[i].Name);

                if (_assetFiles[i].Status == Status.Added)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (_assetFiles[i].Status == Status.Deleted)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (_assetFiles[i].Status == Status.Updated)
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                // Error case
                else if (_assetFiles[i].Status == Status.Unset)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                Console.Write(new string(' ', Math.Max(50 - _assetFiles[i].Name.Length, 0)) + _assetFiles[i].Status +
                              "\n");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Processes files based on status.
        /// </summary>
        private static bool ProcessFiles()
        {
            bool rebuildMeta = false;

            Console.ForegroundColor = ConsoleColor.Magenta;

            foreach (AssetFile file in _assetFiles)
            {
                // Delete action.
                if (file.Status == Status.Deleted)
                {
                    rebuildMeta = true;
                    File.Delete(file.Path);
                }

                // Update/Add action - both are rebuild.
                if (file.Status == Status.Added || file.Status == Status.Updated)
                {
                    rebuildMeta = true;
                    BuildAsset(file);
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;

            return rebuildMeta;
        }

        /// <summary>
        /// Builds a single asset.
        /// </summary>
        /// <param name="file">The asset to build.</param>
        private static void BuildAsset(AssetFile file)
        {
            Stopwatch currentFileTracker = new Stopwatch();
            currentFileTracker.Start();

            // Find out if folders have to be created.
            string folderPath = file.Path.Replace(AssetsPath + Path.DirectorySeparatorChar, "");

            if (folderPath.IndexOf(Path.DirectorySeparatorChar) != -1)
            {
                folderPath = folderPath.Substring(0, folderPath.LastIndexOf(Path.DirectorySeparatorChar));

                Console.WriteLine("> Creating cache folder - " + folderPath);

                folderPath = Path.Combine(CachePath, folderPath);

                Directory.CreateDirectory(folderPath);
            }

            string fragmentPath = Path.Combine(CachePath, file.Name + ".fragment");

            // Read the file, compress it, and encrypt it.
            byte[] data = File.ReadAllBytes(file.Path);
            byte[] compressedFile = Brotli.CompressBuffer(data, 0, data.Length, 1, 24);
            byte[] encryptedData = _service.Encrypt(compressedFile);

            currentFileTracker.Stop();

            // Write the file.
            Write.FileAsBytes(fragmentPath, encryptedData);

            // Set the last write time to the unbuilt file's time.
            File.SetLastWriteTime(fragmentPath, new DateTime(file.TimeUpdated));

            // Record progress.
            Console.WriteLine("Built " + file.Name + " in " + currentFileTracker.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        /// Builds the assets package.
        /// </summary>
        private static void BuildAssetPackage()
        {
            string packagePath = Path.Combine(OutputPath, "assets.soul");
            string metaPath = Path.Combine(OutputPath, "meta.soulm");
            string[] files = Directory.GetFiles(CachePath, "*", SearchOption.AllDirectories);

            // Clear the old blob if one.
            if (File.Exists(packagePath)) File.Delete(packagePath);
            if (File.Exists(metaPath)) File.Delete(metaPath);

            // Gather meta data.
            ManagedFile metaFile = new ManagedFile("meta", _service);

            // Start writing an assets blob.
            Stream writer = new FileStream(packagePath, FileMode.OpenOrCreate);

            for (int i = 0; i < files.Length; i++)
            {
                byte[] content = Read.FileAsBytes(files[i]);

                // Get data for the meta.
                int start = (int) writer.Position;
                string hash = System.Convert.ToBase64String(Hash.Md5(content));

                writer.Write(content, 0, content.Length);

                AssetFile file = new AssetFile();
                file.Path = files[i];

                // Add the file to the meta.
                metaFile.Edit(file.Name, new AssetMeta
                {
                    Start = start,
                    Length = content.Length,
                    Hash = hash
                });
            }

            writer.Flush();
            writer.Close();

            string metaData = metaFile.Save();
            _metaHash = Hash.Md5(metaData);

            Write.File(Path.Combine(OutputPath, ".buildData"), "MetaHash: " + _metaHash + "\n" + "EncryptionKey: " + EncryptionKey);
        }

        /// <summary>
        /// Generates a random string of a specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string GenerateRandomString(int length)
        {
            List<char> output = new List<char>();

            for (int i = 0; i < length; i++)
            {
                output.Add((char) Utilities.GenerateRandomNumber(32, 127));
            }

            return string.Join("", output);
        }
    }

    public class AssetData
    {
        public int DataStart { get; set; }
        public int DataLength { get; set; }
        public string DataHash { get; set; }
    }
}
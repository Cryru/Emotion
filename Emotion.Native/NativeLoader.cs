using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Emotion.Native
{
    /// <summary>
    /// Loads native libraries by copying them from their platform specific directories to the root directory,
    /// because the CLR will find them easier if they are in the root directory.
    /// </summary>
    public static class NativeLoader
    {
        #region Folder Config

        public const string Win64Folder = "win64";
        public const string Win32Folder = "win32";
        public const string LinuxFolder = "linux";
        public const string MacOSFolder = "macos";

        #endregion

        /// <summary>
        /// These libraries must be loaded manually. They are usually dependencies of other libraries and
        /// must be loaded before the native libraries are called.
        /// </summary>
        #region Libraries Which Must Be Loaded Manually

        public static string[] Windows64ManualLibraries { get; private set; } = new[] { "msvcr100.dll", "vcomp120.dll", "vcruntime140.dll" };
        public static string[] Windows32ManualLibraries { get; private set; } = new[] { "", "" };
        public static string[] LinuxManualLibraries { get; private set; } = new[] { "", "" };
        public static string[] MacOSManualLibraries { get; private set; } = new[] { "", "" };

        #endregion

        /// <summary>
        /// The folder which was loaded.
        /// </summary>
        public static string LoadedFolder { get; private set; } = "";

        /// <summary>
        /// Whether the libraries have been loaded.
        /// </summary>
        public static bool Loaded { get; private set; }

        /// <summary>
        /// Load the native libraries, by copying the files from the native folder to the root folder.
        /// </summary>
        public static void Load(Action<string> manualLoadFunc = null)
        {
            if (Loaded) return;

            if (string.IsNullOrEmpty(LoadedFolder)) DetermineFolder();
            if (string.IsNullOrEmpty(LoadedFolder)) throw new Exception("Could not determine native library folder.");
            if (!Directory.Exists(LoadedFolder)) throw new Exception("Native library folder missing.");

            // Check if already loaded.
            if (File.Exists("native.txt"))
            {
                string cachedFolder = File.ReadAllText("native.txt");
                if (cachedFolder == LoadedFolder)
                {
                    // The correct folder is loaded. Verify integrity.
                    bool folderCorrect = VerifyIntegrity(cachedFolder);
                    if (!folderCorrect) File.Delete("native.txt");
                    else Loaded = true;
                }
            }

            // Check if the cache proved enough.
            if (Loaded) return;

            // Load the folder, and cache the loading.
            LoadFolder(LoadedFolder);
            File.WriteAllText("native.txt", LoadedFolder);
            Loaded = true;
        }

        /// <summary>
        /// Determine which folder to load libraries from.
        /// </summary>
        private static void DetermineFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                LoadedFolder = Environment.Is64BitProcess ? Win64Folder : Win32Folder;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                LoadedFolder = MacOSFolder;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LoadedFolder = LinuxFolder;
            }
        }

        /// <summary>
        /// Verify whether all files in the specified folder exist in the root folder.
        /// </summary>
        /// <param name="folder">The folder to verify against.</param>
        /// <returns>Whether all files exist.</returns>
        private static bool VerifyIntegrity(string folder)
        {
            // Get all files from the specified folder and check whether they exist in the root one as well.
            string[] files = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);
            return files.Select(Path.GetFileName).All(File.Exists);
        }

        /// <summary>
        /// Copy the files from the specified folder to the root folder.
        /// </summary>
        /// <param name="folder"></param>
        private static void LoadFolder(string folder)
        {
            // Get all files from the specified folder and copy them in the root folder.
            string[] files = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName == null) continue;
                File.Copy(file, fileName, true);
            }
        }
    }
}
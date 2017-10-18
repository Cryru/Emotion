// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Raya.Graphics;
using Soul.IO;
using Soul.Encryption;
using Soul.Engine.Internal;

#endregion

namespace Soul.Engine.Modules
{
    public static class AssetLoader
    {
        #region Secure Mode

        /// <summary>
        /// Assets meta.
        /// </summary>
        private static ManagedFile _meta;

        /// <summary>
        /// The encrypting service.
        /// </summary>
        private static SymmetricEncryptionService _encryptor;

        /// <summary>
        /// The assets blob reader.
        /// </summary>
        private static FileStream _assetsBlob;

        #endregion

        #region Loaded Assets Arrays

        /// <summary>
        /// Currently loaded textures.
        /// </summary>
        private static Dictionary<string, Texture> _loadedTextures;

        /// <summary>
        /// Currently loaded fonts.
        /// </summary>
        private static Dictionary<string, Font> _loadedFonts;

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        public static void Setup()
        {
            _loadedTextures = new Dictionary<string, Texture>();
            _loadedFonts = new Dictionary<string, Font>();
        }

        #region Loading Functions

        #region Texture

        /// <summary>
        /// Loads a texture with the provided name. If the texture is already loaded nothing is done.
        /// </summary>
        /// <param name="name">The image file to load.</param>
        public static void LoadTexture(string name)
        {
            // Check if already loaded.
            if (_loadedTextures.ContainsKey(name))
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Tried to load already loaded texture: " + name);
            }

            byte[] readData = LoadFile(name);

            // Check if reading failed.
            if (readData == null) return;

            // Load the data into a texture and add it to the loaded list.
            try
            {
                Texture texture = new Texture(readData)
                    // Apply default settings.
                    {
                        Smooth = false,
                        Repeated = true
                    };



                _loadedTextures.Add(name, texture);
            }
            catch (Exception)
            {
                Error.Raise(245, "Failed to load asset " + name + " as a texture.");
            }
        }

        /// <summary>
        /// Unloads a loaded texture.
        /// </summary>
        /// <param name="name">The name of the texture to unload.</param>
        public static void UnloadTexture(string name)
        {
            // Check if loaded.
            if (_loadedTextures.ContainsKey(name))
            {
                // Dispose of it.
                _loadedTextures[name].Dispose();
                // Remove it from the list.
                _loadedTextures.Remove(name);
            }
        }

        /// <summary>
        /// Returns the loaded texture. If it isn't loaded it will be.
        /// </summary>
        /// <param name="name">The name of the texture to get.</param>
        public static Texture GetTexture(string name)
        {
            // Check if loaded.
            if (!_loadedTextures.ContainsKey(name)) LoadTexture(name);

            // Return the loaded texture.
            return _loadedTextures[name];
        }

        #endregion

        #region Font

        /// <summary>
        /// Loads a font with the provided name. If the font is already loaded nothing is done.
        /// </summary>
        /// <param name="name">The name of the font file to load.</param>
        public static void LoadFont(string name)
        {
            // Check if already loaded.
            if (_loadedFonts.ContainsKey(name))
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Tried to load already loaded font: " + name);
            }

            byte[] readData = LoadFile(name);

            // Check if reading failed.
            if (readData == null) return;

            // Load the data into a font and add it to the loaded list.
            try
            {
                Font font = new Font(readData);
                _loadedFonts.Add(name, font);
            }
            catch (Exception)
            {
                Error.Raise(245, "Failed to load asset " + name + " as a font.");
            }
        }

        /// <summary>
        /// Unloads a loaded font.
        /// </summary>
        /// <param name="name">The name of the font to unload.</param>
        public static void UnloadFont(string name)
        {
            // Check if loaded.
            if (_loadedFonts.ContainsKey(name))
            {
                // Dispose of it.
                _loadedFonts[name].Dispose();
                // Remove it from the list.
                _loadedFonts.Remove(name);
            }
        }

        /// <summary>
        /// Returns the loaded font. If it isn't loaded it will be.
        /// </summary>
        /// <param name="name">The name of the font to get.</param>
        public static Font GetFont(string name)
        {
            // Check if loaded.
            if (!_loadedFonts.ContainsKey(name)) LoadFont(name);

            // Return the loaded font.
            return _loadedFonts[name];
        }

        #endregion

        /// <summary>
        /// Loads a file.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="safe">Whether to load it safely, if true exceptions are thrown on errors. </param>
        /// <returns>The file contents.</returns>
        internal static byte[] LoadFile(string name, bool safe = true)
        {
            // Check which way we are reading data.
            if (_meta == null)
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Loading insecurely: " + name);

                // Insecure reading is done from the assets folder. Check if it exists.
                if (!Directory.Exists("Assets"))
                {
                    Error.Raise(238, "Tried loading an asset insecurely with no asset folder.", safe ? Severity.Critical : Severity.Normal);
                    return null;
                }

                // The file should be in that folder.
                string path = Path.Combine("Assets", name);

                // Check if the file exists.
                if (!File.Exists(path))
                {
                    Error.Raise(239, "Tried loading an asset insecurely which doesn't exist.", safe ? Severity.Critical : Severity.Normal);
                    return null;
                }

                // Read the file.
                return Read.FileAsBytes(path);
            }
            else
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Loading securely: " + name);

                return SecureReadFile(name);
            }
        }

        #endregion

        #region Secure Mode

        /// <summary>
        /// Reads a file from the assets blob.
        /// </summary>
        /// <param name="name">The file's name.</param>
        /// <returns>The file as bytes.</returns>
        private static byte[] SecureReadFile(string name)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            AssetMeta assetMeta;

            try
            {
                // Load the meta for the file.
                assetMeta = _meta.Get<AssetMeta>(name);
            }
            catch (Exception)
            {
                Error.Raise(246, "Asset " + name + "doesn't exist in meta.");
                return null;
            }

            // Read the file from the blob.
            byte[] fileData = new byte[assetMeta.Length];
            _assetsBlob.Position = assetMeta.Start;
            _assetsBlob.Read(fileData, 0, assetMeta.Length);

            // Validate the file hash.
            string loadedFileHash = System.Convert.ToBase64String(Hash.Md5(fileData));
            if (assetMeta.Hash == loadedFileHash)
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.Execution, "AssetMeta - Validated hash of " + name);
                
                // Decrypt the data.
                fileData = _encryptor.Decrypt(fileData);
                // Decompress the data.
                // fileData = Compression.DecompressBrotli(fileData);

                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Loaded file " + name + " in " + timer.ElapsedMilliseconds + " ms");

                // Return the file.
                return fileData;

            }
            else
            {
                Error.Raise(244, "Failed to validate file hash or encryption.");
                return null;
            }
        }

        /// <summary>
        /// Activates secure mode with the provided arguments.
        /// </summary>
        /// <param name="metaHash">The assets meta file's hash.</param>
        /// <param name="encryptionKey">The encryption key used when packing the assets.</param>
        internal static void Lock(string metaHash, string encryptionKey)
        {
            try
            {
                // Prevent locking if already locked.
                if (_meta != null) return;

                // Check if the meta file exists.
                if (!File.Exists("meta.soulm"))
                {
                    // Tried to lock with no meta file?
                    Error.Raise(241, "Missing assets meta file.", Severity.Critical);
                    return;
                }

                // Check if the assets blob is missing.
                if (!File.Exists("assets.soul"))
                {
                    Error.Raise(243, "Missing assets blob.", Severity.Critical);
                    return;
                }

                // Start locking process.
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader,
                    "Locking asset loader, hash is " + metaHash);

                // Read the meta file.
                _encryptor = new SymmetricEncryptionService(encryptionKey);
                _meta = new ManagedFile("meta", _encryptor);

                // Get the contents to compute hash, also switch to memory only to prevent the file from being corrupted.
                string contents = _meta.Save(null);
                string hash = Hash.Md5(contents);

                // Check hash.
                if (metaHash != hash)
                {
                    Error.Raise(242, "Wrong assets meta hash.", Severity.Critical);
                    return;
                }

                // Load the assets blob.
                _assetsBlob = new FileStream("assets.soul", FileMode.Open);
            }
            catch (Exception e)
            {
                // Check if not an Error thrown exception.
                if (!(e is SoulEngineException))
                {
                    Error.Raise(240, e.Message, Severity.Critical);
                }
                // Otherwise throw it.
                else
                {
                    throw;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Meta for an asset file.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class AssetMeta
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Hash { get; set; }
    }
}
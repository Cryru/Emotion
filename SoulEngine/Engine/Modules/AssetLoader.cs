// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Breath.Objects;
using Soul.Encryption;
using Soul.Engine.Enums;
using Soul.IO;
using SharpFont;
using Soul.Engine.Graphics;
using Soul.Engine.Graphics.Text;

#endregion

namespace Soul.Engine.Modules
{
    public static class AssetLoader
    {
        #region Secure Mode Variables

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
        private static Dictionary<string, Texture> _loadedTextures = new Dictionary<string, Texture>();

        /// <summary>
        /// Currently loaded fonts.
        /// </summary>
        private static Dictionary<string, Font> _loadedFonts = new Dictionary<string, Font>();

        #endregion

        #region Others

        /// <summary>
        /// An instance of the Freetype library.
        /// </summary>
        internal static SharpFont.Library FreeTypeLib = new SharpFont.Library();

        #endregion

        #region Loading Functions

        #region Texture

        /// <summary>
        /// Loads a texture from the provided path. If the texture is already loaded nothing is done.
        /// </summary>
        /// <param name="path">The image file to load.</param>
        public static void LoadTexture(string path)
        {
#if DEBUG
            // Check if already loaded.
            if (_loadedTextures.ContainsKey(path))
                Debugging.DebugMessage(DebugMessageType.Warning, "Tried to load already loaded texture: " + path);
#endif

            byte[] readData = LoadFile(path);

            // Check if reading failed.
            if (readData == null) return;

            // Load the data into a texture and add it to the loaded list.
            try
            {
                Texture texture = new Texture(readData);
                _loadedTextures.Add(path, texture);

#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoDark, "Loaded a " + texture.Width + "x" + texture.Height + " texture from [" + path + "]");
#endif

            }
            catch (Exception)
            {
                ErrorHandling.Raise(ErrorOrigin.AssetManager, "Failed to load asset " + path + " as a texture.");
            }
        }

        /// <summary>
        /// Unloads a loaded texture.
        /// </summary>
        /// <param name="path">The name of the texture to unload.</param>
        public static void UnloadTexture(string path)
        {
            // Check if loaded.
            if (_loadedTextures.ContainsKey(path))
            {
                // Dispose of it.
                _loadedTextures[path].Destroy();
                // Remove it from the list.
                _loadedTextures.Remove(path);
            }
        }

        /// <summary>
        /// Returns the loaded texture. If it isn't loaded it will be.
        /// </summary>
        /// <param name="path">The name of the texture to get.</param>
        public static Texture GetTexture(string path)
        {
            // Check if loaded.
            if (!_loadedTextures.ContainsKey(path)) LoadTexture(path);

            // Return the loaded texture.
            return _loadedTextures[path];
        }

        #endregion

        #region Font

        /// <summary>
        /// Loads a font from the provided path. If the texture is already loaded nothing is done.
        /// </summary>
        /// <param name="path">The image file to load.</param>
        public static void LoadFont(string path)
        {
#if DEBUG
            // Check if already loaded.
            if (_loadedTextures.ContainsKey(path))
                Debugging.DebugMessage(DebugMessageType.Warning, "Tried to load already loaded font: " + path);
#endif

            byte[] readData = LoadFile(path);

            // Check if reading failed.
            if (readData == null) return;

            // Load the data into a texture and add it to the loaded list.
            try
            {
                Font font = new Font(readData);
                _loadedFonts.Add(path, font);

#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoDark, "Loaded font " + font.Name + " from [" + path + "]");
#endif

            }
            catch (Exception)
            {
                ErrorHandling.Raise(ErrorOrigin.AssetManager, "Failed to load asset " + path + " as a font.");
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
        /// <param name="path">The path of the file.</param>
        /// <returns>The file contents.</returns>
        internal static byte[] LoadFile(string path)
        {
            // Check which way we are reading data.
            if (_meta == null)
            {

#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoDark, "Loading an asset insecurely - [" + path + "]");
#endif

                // Insecure reading is done from the assets folder. Check if it exists.
                if (!Directory.Exists("Assets"))
                {
                    ErrorHandling.Raise(ErrorOrigin.AssetManager,
                        "Tried loading an asset insecurely, but no assets folder exists.");
                    return null;
                }

                // The file should be in that folder.
                string truePath = Path.Combine("Assets", path);

                // Check if the file exists.
                if (!File.Exists(truePath))
                {
                    ErrorHandling.Raise(ErrorOrigin.AssetManager, "Tried loading an asset insecurely which doesn't exist. - [" + path + "]");
                    return null;
                }

                // Read the file.
                return Read.FileAsBytes(truePath);
            }

#if DEBUG
            Debugging.DebugMessage(DebugMessageType.InfoDark, "Loading an asset securely - [" + path + "]");
#endif

            return SecureReadFile(path);
        }

        #endregion

        #region Secure Mode

        /// <summary>
        /// Reads a file from the assets blob.
        /// </summary>
        /// <param name="path">The file's name.</param>
        /// <returns>The file as bytes.</returns>
        private static byte[] SecureReadFile(string path)
        {
#if DEBUG
            Stopwatch timer = new Stopwatch();
            timer.Start();
#endif

            AssetMeta assetMeta;

            try
            {
                // Load the meta for the file.
                assetMeta = _meta.Get<AssetMeta>(path);
            }
            catch (Exception)
            {
                ErrorHandling.Raise(ErrorOrigin.AssetManager, "Tried to load an asset which doesn't exist in the meta. - [" + path + "]");
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
#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoBlue, "Validated hash of - [" + path + "]");
#endif

                // Decrypt the data.
                fileData = _encryptor.Decrypt(fileData);

#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoBlue, "Decrypted an asset in " + timer.ElapsedMilliseconds + " ms - [" + path + "]");
#endif

                // Return the file.
                return fileData;
            }

            ErrorHandling.Raise(ErrorOrigin.AssetManager, "Failed to validate file hash or encryption.");
            return null;
        }

        /// <summary>
        /// Activates secure mode with the provided arguments.
        /// </summary>
        /// <param name="metaHash">The assets meta file's hash.</param>
        /// <param name="encryptionKey">The encryption key used when packing the assets.</param>
        public static void Lock(string metaHash, string encryptionKey)
        {
            try
            {
                // Prevent locking if already locked.
                if (_meta != null) return;

                // Check if the meta file exists.
                if (!File.Exists("meta.soulm"))
                {
                    // Tried to lock with no meta file?
                    ErrorHandling.Raise(ErrorOrigin.AssetManager, "Missing assets meta.");
                    return;
                }

                // Check if the assets blob is missing.
                if (!File.Exists("assets.soul"))
                {
                    ErrorHandling.Raise(ErrorOrigin.AssetManager, "Missing assets blob.");
                    return;
                }

                // Start locking process.
#if DEBUG
                Debugging.DebugMessage(DebugMessageType.InfoBlue, "Locking assets. Hash is " + metaHash);
#endif

                // Read the meta file.
                _encryptor = new SymmetricEncryptionService(encryptionKey);
                _meta = new ManagedFile("meta", _encryptor);

                // Get the contents to compute hash, also switch to memory only to prevent the file from being corrupted.
                string contents = _meta.Save(null);
                string hash = Hash.Md5(contents);

                // Check hash.
                if (metaHash != hash)
                {
                    ErrorHandling.Raise(ErrorOrigin.AssetManager, "Assets meta hash is invalid.");
                    return;
                }

                // Load the assets blob.
                _assetsBlob = new FileStream("assets.soul", FileMode.Open);
            }
            catch (Exception e)
            {
                // Check if an exception is thrown.
                ErrorHandling.Raise(ErrorOrigin.AssetManager, e.Message);
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
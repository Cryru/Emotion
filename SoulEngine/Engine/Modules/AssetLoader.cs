// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
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
        private static ManagedFile _meta = null;

        /// <summary>
        /// The encrypting service.
        /// </summary>
        private static SymmetricEncryptionService _encryptor = null;

        /// <summary>
        /// The assets blob reader.
        /// </summary>
        private static FileStream _assetsBlob = null;

        #endregion

        #region Properties

        /// <summary>
        /// Currently loaded textures.
        /// </summary>
        private static Dictionary<string, Texture> _loadedTextures;

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        public static void Setup()
        {
            _loadedTextures = new Dictionary<string, Texture>();
        }

        #region Loading Functions

        /// <summary>
        /// Loads a texture with the provided name. If the texture is already loaded nothing is done.
        /// </summary>
        /// <param name="name"></param>
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
            Texture texture = new Texture(readData);
            _loadedTextures.Add(name, texture);
        }

        private static byte[] LoadFile(string name)
        {
            // Check which way we are reading data.
            if (_meta == null)
            {
                Debugger.DebugMessage(Enums.DebugMessageSource.AssetLoader, "Loading insecurely: " + name);

                // Insecure reading is done from the assets folder. Check if it exists.
                if (!Directory.Exists("Assets"))
                {
                    Error.Raise(238, "Tried loading an asset insecurely with no asset folder.", Severity.Critical);
                    return null;
                }

                // The file should be in that folder.
                string path = Path.Combine("Assets", name);

                // Check if the file exists.
                if (!File.Exists(path))
                {
                    Error.Raise(239, "Tried loading an asset insecurely which doesn't exist.", Severity.Critical);
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
        public static byte[] SecureReadFile(string name)
        {
            return new byte[] {0};
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
                Error.Raise(240, e.Message, Severity.Critical);
            }
        }

        #endregion
    }
}
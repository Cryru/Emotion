using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using Soul.IO;
using SoulEngine.Objects.Components.Helpers;
using Microsoft.Xna.Framework.Audio;
using System.Threading;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manages asset integrity.
    /// </summary>
    static class AssetManager
    {
        #region "Global Assets"
        /// <summary>
        /// The texture loaded when an asset can't be found or loaded.
        /// </summary>
        public static Texture2D MissingTexture;
        /// <summary>
        /// A blank texture.
        /// </summary>
        public static Texture2D BlankTexture;
        /// <summary>
        /// The default font.
        /// </summary>
        public static SpriteFont DefaultFont;
        /// <summary>
        /// The path to the assets meta file.
        /// </summary>
        public static string assetsMetaPath = "Content" + Path.DirectorySeparatorChar + "meta.soul";
        #endregion

        /// <summary>
        /// Loads global assets used throughout the engine.
        /// </summary>
        public static void LoadGlobal()
        {
            try
            {
                //Load the missingtexture.
                MissingTexture = Context.Core.Content.Load<Texture2D>("Engine/missing");
                //Load the default font.
                DefaultFont = Context.Core.Content.Load<SpriteFont>("Font/DefaultScaled");

                /*
                 * Generate the blank texture by creating a new 1 by 1 texture and
                 * inserting white color into it.
                 */
                BlankTexture = new Texture2D(Context.Graphics, 1, 1);
                Color[] data = new Color[] { Color.White };
                BlankTexture.SetData(data);
                BlankTexture.Name = "Engine/blank";
            }
            catch (Exception)
            {
                throw new Exception("ERROR HANDLING - COULD NOT LOAD GLOBAL ASSETS");
            }

           //Load text object tags.
           TagFactory.Initialize();
        }

        #region "Asset Loading"
        /// <summary>
        /// Loads a texture asset into the current scene's content manager and returns it.
        /// </summary>
        /// <param name="textureName">Path and name of the asset, root is the Content folder.</param>
        /// <returns>The texture, or the default texture if not found.</returns>
        public static Texture2D Texture(string textureName)
        {
            return Asset(textureName, MissingTexture);
        }

        /// <summary>
        /// Loads a font into the current scene's content manager and returns it.
        /// </summary>
        /// <param name="fontName">Path and name of the asset, root is the Content folder.</param>
        /// <returns>The texture, or the default font if not found.</returns>
        public static SpriteFont Font(string fontName)
        {
            return Asset(fontName, DefaultFont);
        }

        /// <summary>
        /// Loads a sound asset into the current scene's content manager and returns it.
        /// </summary>
        /// <param name="soundName">Path and name of the asset, root is the Content folder.</param>
        /// <returns>The sound, or the nothing if not found.</returns>
        public static SoundEffect Sound(string soundName)
        {
            return Asset<SoundEffect>(soundName, null);
        }

        /// <summary>
        /// Loads a custom asset file and returns its contents.
        /// </summary>
        /// <param name="fileName">Path and name including extension of the asset, root is the Content folder.</param>
        /// <returns>A string containing the contents of the file.</returns>
        public static string CustomFile(string fileName)
        {
            //Check if the file exists.
            if (!AssetExist(fileName, "")) return "";

            //If it does read it and return it.
            return Utils.ReadFile("Content" + Path.DirectorySeparatorChar + fileName);
        }

        /// <summary>
        /// Loads an asset into the current scene's content manager and returns it, or a specified asset if missing.
        /// </summary>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <param name="assetName">Path and name of the asset, root is the Content folder.</param>
        /// <param name="ifMissing">The asset to load if the requested asset is missing.</param>
        /// <returns>The asset requested, or the replacement if missing.</returns>
        public static T Asset<T>(string assetName, T ifMissing)
        {
            if (AssetExist(assetName))
                if (Context.Core.Scene == null || assetName == "Engine/loadingscreen")
                    return Context.Core.Content.Load<T>(assetName);
                else
                    return Context.Core.Scene.Assets.Content.Load<T>(assetName);
            else
                return ifMissing;
        }
        #endregion

        /// <summary>
        /// Checks whether the requested asset exists.
        /// </summary>
        /// <param name="name">Path and name of the asset, root is the Content folder.</param>
        /// <param name="extension">The extension of the file we are looking for.</param>
        public static bool AssetExist(string name, string extension = ".xnb")
        {
            //Assign the path of the file.
            name = name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string contentpath = System.IO.Path.Combine("Content", name + extension);
            //Check if the file exists.
            if (File.Exists(contentpath))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the assets meta file, and applies checks for validity.
        /// If true is returned then files are as they should be, otherwise false is returned.
        /// Format: Asset Meta Generator Version 4
        /// </summary>
        public static void AssertAssets()
        {
            if (Settings.EnforceAssetIntegrity)
            {
                if (!File.Exists(assetsMetaPath)) throw new AssetsException("The assets meta file is missing");
                if (Soul.Encryption.MD5(Soul.IO.Utils.ReadFile(assetsMetaPath)) != Settings.MetaMD5) throw new AssetsException("The assets meta file is corrupted");

                Thread assert = new Thread(new ThreadStart(assertThread));
                assert.Start();

                //Wait for thread to activate.
                while (!assert.IsAlive);
            }
        }

        private static void assertThread()
        {

            /*
             * The meta.soul file is expected to a Soul Managed File with each key being a
             * file path relative to the Content folder and each value a hash of the file, 
             * as hashed by SoulLib. If encrypted the key from the settings file is used.
             */

            MFile file = new MFile(assetsMetaPath, null, Settings.SecurityKey);

            try
            {
                //Iterate through each file.
                for (int i = 0; i < file.Keys.Count; i++)
                {
                    //Make sure the path is platform agnostic.
                    string currentKey = file.Keys[i].Replace('\\', Path.DirectorySeparatorChar);
                    //Get the path of the file.
                    string enc = file.Content<string>(currentKey);
                    //Get the hash of the current file.
                    string currentFile = Soul.Encryption.MD5(Utils.ReadFile("Content" + Path.DirectorySeparatorChar + currentKey));
                    //Check against the meta stored hash, if it doesn't match return false.
                    if (currentFile != enc)
                    {
                        // throw new AssetsException(currentKey + " has been tampered with.");
                        Console.WriteLine(currentKey + " has been tampered with! " + i.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
                throw new AssetsException("The meta.soul is corrupted or incorrect.");
            }
        }
    }

    public class AssetsException : Exception
    {
        public AssetsException(string message) : base(message)
        {
        }
    }
}

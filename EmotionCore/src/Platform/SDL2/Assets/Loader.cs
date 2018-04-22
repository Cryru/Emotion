// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.Platform.Base.Assets;

#endregion

namespace Emotion.Platform.SDL2.Assets
{
    public class Loader
    {
        #region Declarations

        private SDLContext _context;
        private Dictionary<string, Texture> _loadedTextures;
        private Dictionary<string, Font> _loadedFonts;

        /// <summary>
        /// The root directory in which assets are located.
        /// </summary>
        public string RootDirectory = "Assets";

        #endregion

        public Loader(SDLContext context)
        {
            _context = context;
            _loadedTextures = new Dictionary<string, Texture>();
            _loadedFonts = new Dictionary<string, Font>();
        }

        #region Texture

        /// <summary>
        /// Loads a texture or returns a texture object if already loaded.
        /// </summary>
        /// <param name="path">
        /// A path to the asset, considering the loader's root directory. Directory separators are converted to
        /// cross-platform ones.
        /// </param>
        /// <returns>A texture object corresponding to the specified path.</returns>
        public Texture Texture(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if the asset is already loaded, in which case return it.
            if (_loadedTextures.ContainsKey(enginePath))
                if (_loadedTextures[enginePath].Destroyed)
                    _loadedTextures.Remove(enginePath);
                // If alive, return it.
                else
                    return _loadedTextures[enginePath];

            // Load it and add it to the list of loaded textures.
            _loadedTextures.Add(enginePath, new SDLTexture(_context, ReadFile(path)));

            // Return the just loaded texture.
            return _loadedTextures[enginePath];
        }

        /// <summary>
        /// Unloads and destroys a loaded texture safely, freeing memory. If the specified texture isn't loaded nothing happens.
        /// </summary>
        /// <param name="path">
        /// A path to the asset, considering the loader's root directory. Directory separators are converted to
        /// cross-platform ones.
        /// </param>
        public void UnloadTexture(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if not loaded, in which case there is nothing to unload.
            if (!_loadedTextures.ContainsKey(enginePath)) return;

            // Destroy the texture and remove it from the loaded list.
            _loadedTextures[enginePath].Destroy();
            _loadedTextures.Remove(enginePath);
        }

        #endregion

        #region Font

        /// <summary>
        /// Loads a font or returns a font object if already loaded.
        /// </summary>
        /// <param name="path">
        /// A path to the asset, considering the loader's root directory. Directory separators are converted to
        /// cross-platform ones.
        /// </param>
        /// <returns>A font object corresponding to the specified path.</returns>
        public Font Font(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if the asset is already loaded, in which case return it.
            if (_loadedFonts.ContainsKey(enginePath))
                if (_loadedFonts[enginePath].Destroyed)
                    _loadedFonts.Remove(enginePath);
                // If alive, return it.
                else
                    return _loadedFonts[enginePath];

            // Load it and add it to the list of loaded fonts.
            _loadedFonts.Add(enginePath, new SDLFont(ReadFile(path)));

            // Return the just loaded font.
            return _loadedFonts[enginePath];
        }

        /// <summary>
        /// Unloads and destroys a loaded font safely, freeing memory. If the specified font isn't loaded nothing happens.
        /// </summary>
        /// <param name="path">
        /// A path to the asset, considering the loader's root directory. Directory separators are converted to
        /// cross-platform ones.
        /// </param>
        public void UnloadFont(string path)
        {
            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if not loaded, in which case there is nothing to unload.
            if (!_loadedFonts.ContainsKey(enginePath)) return;

            // Destroy the font and remove it from the loaded list.
            _loadedFonts[enginePath].Destroy();
            _loadedFonts.Remove(enginePath);
        }

        #endregion

        #region Other

        /// <summary>
        /// Returns the contents of a custom text file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The contents of a custom text file.</returns>
        public string[] TextFile(string path)
        {
            return File.ReadAllLines(PathToCrossPlatform(path));
        }

        /// <summary>
        /// Returns the contents of a custom file as a byte array.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A byte array representing the contents of the specified file.</returns>
        public byte[] Other(string path)
        {
            // Load the bytes of the file.
            return File.ReadAllBytes(PathToCrossPlatform(path));
        }

        /// <summary>
        /// Returns whether the specified file exists.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        public bool Exists(string path)
        {
            return File.Exists(PathToCrossPlatform(path));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Reads a file and returns its contents as a byte array.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The contents of the file as a byte array.</returns>
        private byte[] ReadFile(string path)
        {
            string parsedPath = PathToCrossPlatform(path);

            if (!File.Exists(parsedPath)) throw new Exception("The file " + parsedPath + " could not be found.");

            // Load the bytes of the file.
            return File.ReadAllBytes(parsedPath);
        }

        /// <summary>
        /// Converts the provided path to an engine universal format,
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        private static string PathToEnginePath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', '/');
        }

        /// <summary>
        /// Converts the provided path to the current platform's path signature.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        private string PathToCrossPlatform(string path)
        {
            return RootDirectory + Path.DirectorySeparatorChar + path.Replace('/', '$').Replace('\\', '$').Replace('$', Path.DirectorySeparatorChar);
        }

        #endregion
    }
}

#endif
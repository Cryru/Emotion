// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2.Assets
{
    public class Loader
    {
        #region Declarations

        private Context _context;
        private Dictionary<string, Texture> _loadedTextures;

        /// <summary>
        /// The root directory in which assets are located.
        /// </summary>
        public string RootDirectory;

        #endregion

        public Loader(Context context)
        {
            _context = context;
            _loadedTextures = new Dictionary<string, Texture>();
        }

        #region Texture

        /// <summary>
        /// Loads a texture.
        /// </summary>
        /// <param name="path">An engine path to the texture to load.</param>
        public Texture LoadTexture(string path)
        {
            // Add it to the list of loaded textures.
            _loadedTextures.Add(PathToEnginePath(path), new Texture(_context.Renderer, ReadFile(path)));

            // Return the just loaded texture.
            return GetTexture(path);
        }

        /// <summary>
        /// Unloads a loaded texture, freeing memory.
        /// </summary>
        /// <param name="path">An engine path to the texture to unload.</param>
        public void UnloadTexture(string path)
        {
            string enginePath = PathToEnginePath(path);

            // Destroy the texture and remove it from the loaded list.
            SDL.SDL_DestroyTexture(_loadedTextures[enginePath].Pointer);
            _loadedTextures.Remove(enginePath);
        }

        /// <summary>
        /// Returns a loaded texture.
        /// </summary>
        /// <param name="path">The path of the loaded texture.</param>
        /// <returns>A loaded texture.</returns>
        public Texture GetTexture(string path)
        {
            return _loadedTextures[PathToEnginePath(path)];
        }

        #endregion

        #region Other

        /// <summary>
        /// Returns the contents of a custom file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        public byte[] GetFile(string path)
        {
            // Load the bytes of the file.
            return File.ReadAllBytes(PathToCrossPlatform(path));
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

            if (!File.Exists(parsedPath))
            {
                throw new Exception("The file " + parsedPath + " could not be found.");
            } 

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
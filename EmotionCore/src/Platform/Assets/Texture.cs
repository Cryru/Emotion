// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.IO;
using Emotion.Platform;
using FreeImageAPI;
using SDL2;

#endregion

namespace Emotion.Platform.Assets { 
    public class Texture
    {
        internal IntPtr Pointer;

        public Texture(Context context, byte[] textureBytes)
        {
            // Put the bytes into a stream.
            using (MemoryStream stream = new MemoryStream(textureBytes))
            {
                // Load a free image bitmap from memory.
                FIBITMAP freeImageBitmap = FreeImage.LoadFromStream(stream);

                using (MemoryStream bmpConverted = new MemoryStream())
                {
                    // Convert it to a BMP format.
                    FreeImage.SaveToStream(freeImageBitmap, bmpConverted, FREE_IMAGE_FORMAT.FIF_BMP);

                    // Get the converted bytes and convert to a pointer.
                    byte[] memoryToBytes = bmpConverted.ToArray();

                    // Convert to an SDL stream.
                    IntPtr sdlStream = SDL.SDL_RWFromMem(memoryToBytes, memoryToBytes.Length);

                    // Get the SDL surface.
                    IntPtr surface = SDL.SDL_LoadBMP_RW(sdlStream, 1);

                    // Convert the surface to a texture.
                    Pointer = SDL.SDL_CreateTextureFromSurface(context.Renderer.Pointer, surface);

                    // Free memory.
                    SDL.SDL_FreeSurface(surface);
                }

                freeImageBitmap.SetNull();
            }
        }

        #region Functions

        /// <summary>
        /// Set the texture opacity.
        /// </summary>
        /// <param name="alpha">The opacity to set the texture to.</param>
        public void SetAlpha(byte alpha)
        {
            SDL.SDL_SetTextureAlphaMod(Pointer, alpha);
        }

        #endregion
    }
}

#endif
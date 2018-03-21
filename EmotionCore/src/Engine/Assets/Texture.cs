// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SDL2;

#endregion

namespace Emotion.Engine.Assets
{
    public class Texture
    {
        internal IntPtr Pointer;

        public Texture(Context context, byte[] textureBytes)
        {
            // Put the bytes into a stream..
            using (MemoryStream stream = new MemoryStream(textureBytes))
            {
                // Create a System.Drawing bitmap from it.
                Bitmap temp = new Bitmap(stream);

                // Convert to bitmap.
                using (MemoryStream bmpConverted = new MemoryStream())
                {
                    temp.Save(bmpConverted, ImageFormat.Bmp);

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
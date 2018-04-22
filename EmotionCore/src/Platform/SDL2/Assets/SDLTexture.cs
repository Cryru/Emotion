// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2.Assets
{
    /// <inheritdoc />
    public class SDLTexture : Texture
    {
        internal IntPtr Pointer;

        public SDLTexture(int width, int height) : base(width, height)
        {
        }

        public SDLTexture(Context context, byte[] data) : base(context, data)
        {
        }

        protected override void ParseData(Context context, byte[] data)
        {
            // Convert to an SDL stream.
            IntPtr sdlStream = SDL.SDL_RWFromConstMem(data, data.Length);

            // Get the SDL surface.
            IntPtr surface = SDL.SDL_LoadBMP_RW(sdlStream, 1);

            // Convert the surface to a texture.
            Pointer = SDL.SDL_CreateTextureFromSurface(((SDLContext) context).Renderer.Pointer, surface);

            // Free memory.
            SDL.SDL_FreeSurface(surface);
        }

        public override void Destroy()
        {
            SDL.SDL_DestroyTexture(Pointer);
            Destroyed = true;
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
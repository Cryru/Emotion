// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using Emotion.Platform.SDL2.Assets;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2.Objects
{
    public class RenderTarget : SDLTexture
    {
        /// <summary>
        /// Create a blank render target of the specified dimensions.
        /// </summary>
        /// <param name="renderer">The renderer which will render the render target.</param>
        /// <param name="width">The width of the render target.</param>
        /// <param name="height">The height of the render target.</param>
        public RenderTarget(SDLRenderer renderer, int width, int height) : base(width, height)
        {
            Pointer = ErrorHandler.CheckError(SDL.SDL_CreateTexture(renderer.Pointer, SDL.SDL_PIXELFORMAT_RGBA8888, (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height));
        }
    }
}

#endif
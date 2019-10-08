#region Using

using System;
using System.Collections.Generic;
using Emotion.Platform.Config;

#endregion

namespace Emotion.Platform.Implementation
{
    public abstract class GraphicsContext : IDisposable
    {
        public int SwapInternal { get; protected set; }

        /// <summary>
        /// Make this context current.
        /// </summary>
        public abstract void MakeCurrent();

        /// <summary>
        /// How many monitor refreshes to wait before flushing the buffer.
        /// </summary>
        /// <param name="interval">The number of refreshes to wait for before flushing. This is vertical sync.</param>
        public void SetSwapInterval(int interval)
        {
            SwapInternal = interval;
            SetSwapIntervalPlatform(interval);
        }

        protected abstract void SetSwapIntervalPlatform(int interval);

        /// <summary>
        /// Swap the buffers on the active window.
        /// </summary>
        public abstract void SwapBuffers();

        /// <summary>
        /// Returns the pointer of an OpenGL function from its name.
        /// </summary>
        /// <param name="func">The name of the function to return.</param>
        /// <returns>The pointer to the function.</returns>
        public abstract IntPtr GetProcAddress(string func);

        public abstract void Dispose();

        public static bool RefreshContextAttributes(Window win, PlatformConfig conf)
        {
            return true;
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <param name="config">The config which contains the requested pixel format.</param>
        /// <param name="usableConfigs">A list of supported formats by the context.</param>
        /// <returns>The index of the pixel format to use.</returns>
        public static FramebufferConfig ChoosePixelFormat(PlatformConfig config, List<FramebufferConfig> usableConfigs)
        {
            const sbyte redBits = 8;
            const sbyte greenBits = 8;
            const sbyte blueBits = 8;
            const sbyte alphaBits = 8;
            const sbyte depthBits = 24;
            const sbyte stencilBits = 8;

            int leastMissing = int.MaxValue;
            int leastColorDiff = int.MaxValue;
            int leastExtraDiff = int.MaxValue;

            FramebufferConfig closest = null;

            foreach (FramebufferConfig current in usableConfigs)
            {
                // Double buffering is a hard constraint
                if (!current.Doublebuffer) continue;

                // Count number of missing buffers
                var missing = 0;

                if (current.AlphaBits == 0)
                    missing++;

                if (current.DepthBits == 0)
                    missing++;

                if (current.StencilBits == 0)
                    missing++;

                if (config.Samples > 0 && current.Samples == 0)
                    // Technically, several multisampling buffers could be
                    // involved, but that's a lower level implementation detail and
                    // not important to us here, so we count them as one
                    missing++;

                // These polynomials make many small channel size differences matter
                // less than one large channel size difference

                // Calculate color channel size difference value
                var colorDiff = 0;
                colorDiff += (redBits - current.RedBits) *
                             (redBits - current.RedBits);
                colorDiff += (greenBits - current.GreenBits) *
                             (greenBits - current.GreenBits);
                colorDiff += (blueBits - current.BlueBits) *
                             (blueBits - current.BlueBits);

                // Calculate non-color channel size difference value
                var extraDiff = 0;
                extraDiff += (alphaBits - current.AlphaBits) *
                             (alphaBits - current.AlphaBits);
                extraDiff += (depthBits - current.DepthBits) *
                             (depthBits - current.DepthBits);
                extraDiff += (stencilBits - current.StencilBits) *
                             (stencilBits - current.StencilBits);
                extraDiff += (config.Samples - current.Samples) *
                             (config.Samples - current.Samples);

                // Figure out if the current one is better than the best one found so far
                // Least number of missing buffers is the most important heuristic,
                // then color buffer size match and lastly size match for other buffers

                if (missing < leastMissing)
                    closest = current;
                else if (missing == leastMissing)
                    if (colorDiff < leastColorDiff ||
                        colorDiff == leastColorDiff && extraDiff < leastExtraDiff)
                        closest = current;

                if (current != closest) continue;

                leastMissing = missing;
                leastColorDiff = colorDiff;
                leastExtraDiff = extraDiff;
            }

            return closest;
        }
    }
}
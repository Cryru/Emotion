#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Emotion.Platform.RenderDoc;

#endregion

namespace Emotion.Platform
{
    /// <summary>
    /// Handles the graphics API context.
    /// </summary>
    public abstract class GraphicsContext : IDisposable
    {
        /// <summary>
        /// Whether this graphics context came from native code.
        /// On by default, determines how the GL functions will be loaded.
        /// Todo: Currently the native function is required to be implemented.
        /// </summary>
        public bool Native { get; protected set; } = true;

        /// <summary>
        /// Whether the context is valid.
        /// </summary>
        public bool Valid { get; protected set; }

        /// <summary>
        /// How many monitor refreshes to wait before flushing the buffer. This is vertical sync.
        /// </summary>
        public int SwapInterval
        {
            get => _swapInterval;
            set
            {
                _swapInterval = value;
                SetSwapIntervalPlatform(_swapInterval);
            }
        }

        protected int _swapInterval;

        /// <summary>
        /// Reference to the RenderDoc debugger, if attached.
        /// </summary>
        public RenderDocAPI RenderDoc;

        /// <summary>
        /// Internal function for when SwapInterval is called.
        /// </summary>
        /// <param name="interval"></param>
        protected abstract void SetSwapIntervalPlatform(int interval);

        /// <summary>
        /// Make this context current.
        /// </summary>
        public abstract void MakeCurrent();

        /// <summary>
        /// Swap the buffers on the active window.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void SwapBuffers();

        /// <summary>
        /// Returns the pointer of an OpenGL function from its name.
        /// </summary>
        /// <param name="func">The name of the function to return.</param>
        /// <returns>The pointer to the function.</returns>
        public abstract IntPtr GetProcAddress(string func);

        /// <summary>
        /// Returns the delegate of an OpenGL function from its name.
        /// Used only if "Native" is set to false.
        /// </summary>
        /// <param name="func">The name of the function to return.</param>
        /// <returns>The delegate to the function.</returns>
        public virtual Delegate GetProcAddressNonNative(string func)
        {
            return null;
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <param name="usableConfigs">A list of supported formats by the context.</param>
        /// <returns>The index of the pixel format to use.</returns>
        public static FramebufferConfig ChoosePixelFormat(IEnumerable<FramebufferConfig> usableConfigs)
        {
            const sbyte redBits = 8;
            const sbyte greenBits = 8;
            const sbyte blueBits = 8;
            const sbyte alphaBits = 8;
            const sbyte depthBits = 24;
            const sbyte stencilBits = 8;

            var leastMissing = int.MaxValue;
            var leastColorDiff = int.MaxValue;
            var leastExtraDiff = int.MaxValue;

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

        /// <summary>
        /// Clean up resources.
        /// This would probably never be called.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
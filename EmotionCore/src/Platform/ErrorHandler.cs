// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Engine;
using Emotion.Engine.Debugging;
using SDL2;

#endregion

namespace Emotion.Platform
{
    public static class ErrorHandler
    {
        /// <summary>
        /// Raises an error if the response is less than 0.
        /// </summary>
        /// <param name="response">The response from a SDL function.</param>
        /// <param name="invert">Will raise an error if the response is 0 instead.</param>
        internal static void CheckError(int response, bool invert = false)
        {
            if (response < 0 && !invert || response > 0 && invert) ProcessError();
        }

        /// <summary>
        /// Raises an error if the response is an invalid pointer.
        /// </summary>
        /// <param name="response">The response from a SDL function.</param>
        /// <param name="invert">Will raise an error if the pointer is not zero instead.</param>
        internal static IntPtr CheckError(IntPtr response, bool invert = false)
        {
            if (response == IntPtr.Zero && !invert || response != IntPtr.Zero && invert) ProcessError();

            return response;
        }

        /// <summary>
        /// Process the received SDL error.
        /// </summary>
        private static void ProcessError()
        {
            string error = SDL.SDL_GetError();

            //todo
#if DEBUG

            Debugger.Log(error);

#endif

            throw new Exception("External Error: " + error);
        }
    }
}

#endif
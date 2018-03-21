// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

#region Using

using System;
using System.Drawing;
using Emotion.Engine;

#endregion

namespace Emotion.Systems
{
    public static class Debugging
    {
        /// <summary>
        /// Add a message to the debug log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void DebugLoop(Context context)
        {
            // Check if there is an attached renderer with a camera.
            if (context.Renderer?.Camera != null)
            {
                CameraBoundDraw(context.Renderer);
            }
        }

        #region Debug Drawing

        private static void CameraBoundDraw(Renderer renderer)
        {
            renderer.DrawRectangle(renderer.Camera.InnerBounds, Color.Yellow);
            renderer.DrawRectangle(renderer.Camera.Bounds, Color.Green);
        }

        #endregion
    }
}
#endif
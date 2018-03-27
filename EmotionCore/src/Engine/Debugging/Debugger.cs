// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

#region Using

using System;
using Emotion.Platform;
using Emotion.Primitives;

#endregion

namespace Emotion.Engine.Debugging
{
    public static class Debugger
    {
        /// <summary>
        /// Add a message to the debug log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Add a message to the debug log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public static void Log(MessageType type, string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Add a message to the debug log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public static void Log(MessageType type, MessageSource source, string message)
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

            MouseBoundDraw(context.Renderer, context.Input);
        }

        #region Debug Drawing

        private static void CameraBoundDraw(Renderer renderer)
        {
            // Draw bounds.
            renderer.DrawRectangle(renderer.Camera.Bounds, Color.Yellow);

            // Draw center.
            Rectangle centerDraw = new Rectangle(0, 0, 10, 10);
            centerDraw.X = (int) renderer.Camera.Bounds.Center.X - centerDraw.Width / 2;
            centerDraw.Y = (int) renderer.Camera.Bounds.Center.Y - centerDraw.Height / 2;

            renderer.DrawRectangle(centerDraw, Color.Yellow);
        }

        private static void MouseBoundDraw(Renderer renderer, Input input)
        {
            Vector2 mouseLocation = input.GetMousePosition();

            Rectangle mouseBounds = new Rectangle(0, 0, 10, 10);
            mouseBounds.X = (int) mouseLocation.X - mouseBounds.Width / 2;
            mouseBounds.Y = (int) mouseLocation.Y - mouseBounds.Height / 2;

            renderer.DrawRectangle(mouseBounds, Color.Pink, false);
        }

        #endregion
    }
}
#endif
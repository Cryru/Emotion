using System.Numerics;

namespace Emotion.Platform.Implementation.GlfwImplementation
{
    public class GlfwMonitor : Monitor
    {
        public GlfwMonitor(Vector2 position, Vector2 size)
        {
            Position = position;
            Width = (int) size.X;
            Height = (int) size.Y;
        }
    }
}
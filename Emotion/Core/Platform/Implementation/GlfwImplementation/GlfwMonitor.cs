#nullable enable

namespace Emotion.Core.Platform.Implementation.GlfwImplementation;

public class GlfwMonitor : MonitorScreen
{
    public GlfwMonitor(Vector2 position, Vector2 size)
    {
        Position = position;
        Width = (int) size.X;
        Height = (int) size.Y;
    }
}
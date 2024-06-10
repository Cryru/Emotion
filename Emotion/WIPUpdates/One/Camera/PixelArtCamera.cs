using Emotion.Platform.Input;
using Emotion.WIPUpdates.One.Camera;

namespace Emotion.Graphics.Camera;

public class PixelArtCamera : Camera2D
{
    public PixelArtCamera(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
    {
    }

    /// <inheritdoc />
    public override void RecreateViewMatrix()
    {
        var iX = (int)X;
        var iY = (int)Y;

        Vector2 targetSize = Engine.Configuration.RenderSize;
        Vector2 currentSize = Engine.Renderer.DrawBuffer.Size;

        // Transform the position from the center position to the offset position.
        Vector3 posOffset = new Vector3(iX, iY, 0) - new Vector3(targetSize, 0) / 2;

        // Get the scale relative to the zoom.
        float scale = Engine.Renderer.IntScale * Zoom;
        CalculatedScale = scale;

        // Find the camera margin and scale from the center.
        // As the current size expands more of the world will come into view until the integer scale changes at which point everything will be resized.
        // This allows for pixel art to scale integerly in FullScale mode.
        Vector2 margin = (currentSize - targetSize) / 2;
        Vector3 pos = posOffset - new Vector3((int)margin.X, (int)margin.Y, 0);
        var unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos + LookAt, new Vector3(0.0f, 1.0f, 0.0f));
        ViewMatrix = Matrix4x4.CreateScale(new Vector3(scale, scale, 1), new Vector3(iX, iY, 0)) * unscaled;
    }
}

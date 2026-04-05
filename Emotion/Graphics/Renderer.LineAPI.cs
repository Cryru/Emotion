#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics;

public sealed partial class Renderer
{
    public float LinePixelsToMeters = 1f / 100f;
    public float MinLineAlpha = 0.15f;

    public enum RenderLineMode
    {
        Center,
        Inward,
        Outward
    }

    private struct LineRenderItem
    {
        public Vector3 From;
        public Vector3 To;
        public Color Color;
        public float Thickness;
        public RenderLineMode RenderMode;
    }

    private enum LineRenderMode
    {
        ThreeDee,
        TwoDee,
        NoCamera
    }

    private struct LineRenderState
    {
        public LineRenderMode RenderMode;
    }

    private LineRenderState _lineRenderState;
    private bool _lineRenderStarted = false;
    private readonly List<LineRenderItem> _lineQueue = new();

    public void StartLineRender()
    {
        _lineRenderStarted = true;

        if (!CurrentState.ViewMatrix)
            _lineRenderState.RenderMode = LineRenderMode.NoCamera;
        else if (_camera is Camera2D)
            _lineRenderState.RenderMode = LineRenderMode.TwoDee;
        else if (_camera is Camera3D)
            _lineRenderState.RenderMode = LineRenderMode.ThreeDee;
    }

    public void AddLineRender(Vector3 from, Vector3 to, Color color, float thickness = 1f, RenderLineMode renderMode = RenderLineMode.Center)
    {
        Assert(_lineRenderStarted);

        // In 3D we need to convert pixels to meters
        if (_lineRenderState.RenderMode == LineRenderMode.ThreeDee)
            thickness *= LinePixelsToMeters;

        _lineQueue.Add(new LineRenderItem
        {
            From = from,
            To = to,
            Color = color,
            Thickness = thickness,
            RenderMode = renderMode
        });
    }

    public void EndLineRender()
    {
        Assert(_lineRenderStarted);
        if (!_lineRenderStarted && _lineQueue.Count == 0) return;

        Span<LineRenderItem> lineQueueSpan = CollectionsMarshal.AsSpan(_lineQueue);
        if (_lineRenderState.RenderMode == LineRenderMode.NoCamera)
        {
            for (int i = 0; i < lineQueueSpan.Length; i++)
            {
                ref LineRenderItem line = ref lineQueueSpan[i];
                InternalDrawLineUI(line);
            }
        }
        else
        {
            bool threeDee = _lineRenderState.RenderMode == LineRenderMode.ThreeDee;
            for (int i = 0; i < lineQueueSpan.Length; i++)
            {
                ref LineRenderItem line = ref lineQueueSpan[i];
                InternalDrawLineWorldSpace(line, threeDee);
            }
        }

        _lineRenderStarted = false;
        _lineQueue.Clear();
    }

    private void InternalDrawLineWorldSpace(in LineRenderItem item, bool threeDee)
    {
        Vector3 pointOne = item.From;
        Vector3 pointTwo = item.To;
        Color color = item.Color;
        float thickness = MathF.Max(item.Thickness, 0.0001f);

        Vector3 direction = pointTwo - pointOne;
        if (direction.LengthSquared() < float.Epsilon) return;
        direction = Vector3.Normalize(direction);

        Vector3 normal;
        float alpha = 1f;
        if (threeDee)
        {
            Vector3 midPoint = (pointOne + pointTwo) * 0.5f;
            Vector3 toCamera = Vector3.Normalize(_camera.Position - midPoint);
            normal = Vector3.Normalize(Vector3.Cross(toCamera, direction));
            if (normal.LengthSquared() < float.Epsilon) // Right at camera?
            {
                normal = Vector3.Normalize(Vector3.Cross(Renderer.Up, direction));
                if (normal.LengthSquared() < float.Epsilon) // Vertically right at camera? lol
                    return;
            }

            Vector2 screenP1 = _camera.WorldToScreen(pointOne);
            Vector2 screenP2 = _camera.WorldToScreen(pointTwo);
            Vector2 screenSideA1 = _camera.WorldToScreen(pointOne + normal * (thickness / 2f));
            Vector2 screenSideA2 = _camera.WorldToScreen(pointTwo + normal * (thickness / 2f));
            float screenHalfWidthAtP1 = (screenSideA1 - screenP1).Length();
            float screenHalfWidthAtP2 = (screenSideA2 - screenP2).Length();
            float screenThickness = MathF.Min(screenHalfWidthAtP1, screenHalfWidthAtP2) * 2f;
            if (screenThickness < 1f)
            {
                alpha = Maths.Clamp(screenThickness, MinLineAlpha, 1f);

                // We need to invert scale the geometry so it doesn't go too thin
                thickness = thickness / MathF.Max(screenThickness, float.Epsilon);
            }
        }
        else
        {
            float scale = _camera.CalculatedScale;
            float screenThickness = thickness * scale;

            if (screenThickness < 1f)
            {
                alpha = Maths.Clamp(screenThickness, MinLineAlpha, 1f);
                thickness = 1f / scale; // snap to 1px minimum
            }

            normal = new Vector3(-direction.Y, direction.X, 0);
        }

        Vector3 halfDelta = normal * (thickness / 2f);
        Vector3 halfDeltaNeg = -halfDelta;

        if (item.RenderMode == RenderLineMode.Inward)
        {
            pointOne += halfDelta;
            pointTwo += halfDelta;
        }
        else if (item.RenderMode == RenderLineMode.Outward)
        {
            pointOne -= halfDelta;
            pointTwo -= halfDelta;
        }

        Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
        VertexData.WriteDefaultQuadUV(vertices);

        vertices[0].Vertex = pointOne + halfDelta;
        vertices[1].Vertex = pointTwo + halfDelta;
        vertices[2].Vertex = pointTwo + halfDeltaNeg;
        vertices[3].Vertex = pointOne + halfDeltaNeg;

        uint c = (color * alpha).ToUint();
        for (int v = 0; v < vertices.Length; v++)
        {
            vertices[v].Color = c;
        }
    }

    private void InternalDrawLineUI(in LineRenderItem item)
    {
        Vector3 p1 = item.From;
        Vector3 p2 = item.To;
        Color color = item.Color;

        float thicknessPx = MathF.Max(item.Thickness, 1f);

        Vector2 s1 = new Vector2(p1.X, p1.Y);
        Vector2 s2 = new Vector2(p2.X, p2.Y);

        // Scientifically chosen offset lol
        // Not but for real this little offset and texel snap prevents some lines
        // from disappearing during rasterization (aligning to the texel center however
        // often leads to them jumping a whole pixel though, which is also undesirable)
        if (item.RenderMode == RenderLineMode.Center)
        {
            s1.X = MathF.Round(s1.X - 0.5f) + 0.01f;
            s1.Y = MathF.Round(s1.Y - 0.5f) + 0.01f;
            s2.X = MathF.Round(s2.X - 0.5f) + 0.01f;
            s2.Y = MathF.Round(s2.Y - 0.5f) + 0.01f;
        }
        Vector3 snappedP1 = new Vector3(s1, p1.Z);
        Vector3 snappedP2 = new Vector3(s2, p2.Z);

        Vector3 dir = snappedP2 - snappedP1;
        if (dir.LengthSquared() < float.Epsilon) return;
        dir = Vector3.Normalize(dir);

        Vector3 normal = new Vector3(-dir.Y, dir.X, 0);
        Vector3 delta = normal * (thicknessPx / 2f);
        Vector3 deltaNeg = -delta;

        if (item.RenderMode == RenderLineMode.Inward)
        {
            snappedP1 += delta;
            snappedP2 += delta;
        }
        else if (item.RenderMode == RenderLineMode.Outward)
        {
            snappedP1 -= delta;
            snappedP2 -= delta;
        }

        Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
        VertexData.WriteDefaultQuadUV(vertices);

        vertices[0].Vertex = snappedP1 + delta;
        vertices[1].Vertex = snappedP2 + delta;
        vertices[2].Vertex = snappedP2 + deltaNeg;
        vertices[3].Vertex = snappedP1 + deltaNeg;

        uint c = color.ToUint();
        for (int v = 0; v < vertices.Length; v++)
        {
            vertices[v].Color = c;
        }
    }

    #region Individual API

    /// <summary>
    /// Render a line made out of quads.
    /// </summary>
    /// <param name="pointOne">The point to start the line.</param>
    /// <param name="pointTwo">The point to end the line at.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="thickness">The thickness of the line in world units. The line will always be at least 1 pixel thick.</param>
    /// <param name="renderMode">How to treat the points given.</param>
    public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f, RenderLineMode renderMode = RenderLineMode.Center)
    {
        StartLineRender();
        AddLineRender(pointOne, pointTwo, color, thickness, renderMode);
        EndLineRender();
    }

    /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, RenderLineMode)" />
    public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
    {
        RenderLine(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
    }

    /// <summary>
    /// Render a line, from a line segment.
    /// </summary>
    /// <param name="segment">The line segment to render.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="thickness">The thickness of the line.</param>
    public void RenderLine(ref LineSegment segment, Color color, float thickness = 1f)
    {
        RenderLine(segment.Start, segment.End, color, thickness);
    }

    public void RenderLine(LineSegment segment, Color color, float thickness = 1f)
    {
        RenderLine(segment.Start, segment.End, color, thickness);
    }

    /// <summary>
    /// Render a line with an arrow at the end.
    /// </summary>
    /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, RenderLineMode)" />
    public void RenderArrow(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
    {
        RenderLine(pointOne, pointTwo, color, thickness);

        Vector3 diff = pointTwo - pointOne;
        const float maxArrowHeadLength = 10;
        float length = Math.Min(diff.Length() / 2, maxArrowHeadLength);
        float width = length / 2;

        Vector3 direction = Vector3.Normalize(diff);
        var normal = new Vector3(-direction.Y, direction.X, 0);
        Vector3 lengthDelta = length * direction;
        Vector3 delta = width * normal;
        Vector3 arrowPointOne = pointTwo - lengthDelta + delta;
        Vector3 arrowPointTwo = pointTwo - lengthDelta - delta;

        RenderLine(pointTwo, arrowPointOne, color, thickness);
        RenderLine(pointTwo, arrowPointTwo, color, thickness);
    }

    /// <inheritdoc cref="RenderArrow(Vector3, Vector3, Color, float)" />
    public void RenderArrow(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
    {
        RenderArrow(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
    }

    /// <summary>
    /// Render a rectangle outline.
    /// </summary>
    /// <param name="position">The position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="color">The color of the lines.</param>
    /// <param name="thickness">How thick the line should be.</param>
    public void RenderRectOutline(Vector3 position, Vector2 size, Color color, float thickness = 1)
    {
        Vector3 nn = position;
        Vector3 pn = new Vector3(position.X + size.X, position.Y, position.Z);
        Vector3 np = new Vector3(position.X, position.Y + size.Y, position.Z);
        Vector3 pp = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);

        StartLineRender();
        AddLineRender(nn, pn, color, thickness, RenderLineMode.Inward);
        AddLineRender(pn, pp, color, thickness, RenderLineMode.Inward);
        AddLineRender(pp, np, color, thickness, RenderLineMode.Inward);
        AddLineRender(np, nn, color, thickness, RenderLineMode.Inward);
        EndLineRender();
    }

    /// <inheritdoc cref="RenderRectOutline(Vector3, Vector2, Color, float)" />
    public void RenderRectOutline(Vector2 position, Vector2 size, Color color, float thickness = 1)
    {
        RenderRectOutline(position.ToVec3(), size, color, thickness);
    }

    /// <inheritdoc cref="RenderRectOutline(Vector3, Vector2, Color, float)" />
    public void RenderRectOutline(Rectangle rect, Color color, float thickness = 1)
    {
        RenderRectOutline(rect.Position.ToVec3(), rect.Size, color, thickness);
    }


    #endregion
}
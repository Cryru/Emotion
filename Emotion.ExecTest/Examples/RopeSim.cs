#region Using

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Input;
using Emotion.Game.RopeSim;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples;

public class RopeSim : Scene
{
    private List<RopeSimPoint2D> _points = new List<RopeSimPoint2D>();
    private List<RopeSimConnection2D> _connections = new List<RopeSimConnection2D>();
    private bool _run;
    private RopeSimPoint2D _dragging;
    private Vector2 _draggingCut;
    private float _circleRadius = 4;

    public override IEnumerator LoadSceneRoutineAsync()
    {
#if true
        var gridDistance = new Vector2(20);
        var gridSize = new Vector2(30, 15);

        var screen = new Rectangle(0, 0, Engine.Configuration.RenderSize);
        screen.Center = new Vector2(0);

        var grid = new Rectangle(0, 0, gridSize * gridDistance);
        grid.Center = screen.Center;
        Vector2 gridStart = grid.Position;

        var arr = new RopeSimPoint2D[(int) gridSize.X, (int) gridSize.Y];
        for (var y = 0; y < gridSize.Y; y++)
        {
            for (var x = 0; x < gridSize.X; x++)
            {
                Vector2 p = gridStart + new Vector2(x, y) * gridDistance;
                var point = new RopeSimPoint2D(p);
                if (y == 0 && (x % 5 == 0 || x == gridSize.X - 1)) point.Locked = true;

                arr[x, y] = point;
                _points.Add(point);

                if (x != 0) _connections.Add(new RopeSimConnection2D(arr[x - 1, y], point));
                if (y != 0) _connections.Add(new RopeSimConnection2D(arr[x, y - 1], point));
            }
        }
#endif

        Engine.Host.OnKey.AddListener((key, status) =>
        {
            Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();

            if (key == Key.Space && status == KeyState.Down)
            {
                _run = !_run;
            }
            else if (key == Key.MouseKeyLeft)
            {
                RopeSimPoint2D mouseOnPoint = null;
                for (var i = 0; i < _points.Count; i++)
                {
                    RopeSimPoint2D p = _points[i];
                    if (new Circle(_points[i].Position + new Vector2(_circleRadius), _circleRadius).Contains(ref worldMouse))
                    {
                        mouseOnPoint = p;
                        break;
                    }
                }

                if (status == KeyState.Down)
                {
                    if (mouseOnPoint != null)
                    {
                        if (Engine.Host.IsKeyHeld(Key.LeftControl))
                            mouseOnPoint.Locked = !mouseOnPoint.Locked;
                        else
                            _dragging = mouseOnPoint;
                    }
                    else
                    {
                        _points.Add(new RopeSimPoint2D(worldMouse));
                    }
                }
                else if (status == KeyState.Up && _dragging != null)
                {
                    if (mouseOnPoint != null && mouseOnPoint != _dragging)
                        _connections.Add(new RopeSimConnection2D(_dragging, mouseOnPoint));

                    _dragging = null;
                }
            }
            else if (key == Key.MouseKeyRight)
            {
                if (status == KeyState.Down)
                {
                    _draggingCut = worldMouse;
                }
                else if (status == KeyState.Up && _draggingCut != Vector2.Zero)
                {
                    var cutter = new LineSegment(_draggingCut, worldMouse);

                    for (int i = _connections.Count - 1; i >= 0; i--)
                    {
                        RopeSimConnection2D connection = _connections[i];
                        var s = new LineSegment(connection.Start.Position + new Vector2(_circleRadius), connection.End.Position + new Vector2(_circleRadius));
                        if (s.Intersects(ref cutter)) _connections.RemoveAt(i);
                    }

                    _draggingCut = Vector2.Zero;
                }
            }

            return true;
        });
        yield break;
    }

    public override void UpdateScene(float dt)
    {
        if (_run) RopeSimSystem.Simulate2D(_points, _connections, new Vector2(0, 0.01f) * Engine.DeltaTime);
    }

    public override void RenderScene(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.SetUseViewMatrix(true);

        foreach (RopeSimConnection2D p in _connections)
        {
            composer.RenderLine((p.Start.Position + new Vector2(_circleRadius)).ToVec3(), (p.End.Position + new Vector2(_circleRadius)).ToVec3(), Color.White);
        }

        if (_dragging != null)
        {
            Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            composer.RenderLine((_dragging.Position + new Vector2(_circleRadius)).ToVec3(), worldMouse.ToVec3(), Color.Red);
        }

        if (_draggingCut != Vector2.Zero)
        {
            Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            composer.RenderLine(_draggingCut.ToVec3(), worldMouse.ToVec3(), Color.Red);
        }

        foreach (var p in _points)
        {
            composer.RenderCircle(p.Position.ToVec3(), _circleRadius, p.Locked ? Color.Magenta : Color.Black);
        }
    }
}
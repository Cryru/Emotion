#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.RopeSim;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples
{
    public class RopeSim : Scene
    {
        private List<RopeSimPoint2D> _points = new List<RopeSimPoint2D>();
        private List<RopeSimConnection2D> _connections = new List<RopeSimConnection2D>();
        private bool _run;
        private RopeSimPoint2D _dragging;
        private Vector2 _draggingCut;

        public override Task LoadAsync()
        {
            Engine.Host.OnKey.AddListener((key, status) =>
            {
                Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);

                if (key == Key.Space && status == KeyStatus.Down)
                {
                    _run = !_run;
                }
                else if (key == Key.MouseKeyLeft)
                {
                    RopeSimPoint2D mouseOnPoint = null;
                    for (int i = 0; i < _points.Count; i++)
                    {
                        RopeSimPoint2D p = _points[i];
                        if (new Circle(_points[i].Position + new Vector2(5f), 5).Contains(ref worldMouse))
                        {
                            mouseOnPoint = p;
                            break;
                        }
                    }

                    if (status == KeyStatus.Down)
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
                    else if (status == KeyStatus.Up && _dragging != null)
                    {
                        if (mouseOnPoint != null && mouseOnPoint != _dragging)
                            _connections.Add(new RopeSimConnection2D(_dragging, mouseOnPoint, Vector2.Distance(_dragging.Position, mouseOnPoint.Position)));

                        _dragging = null;
                    }
                }
                else if (key == Key.MouseKeyRight)
                {
                    if (status == KeyStatus.Down)
                    {
                        _draggingCut = worldMouse;
                    }
                    else if (status == KeyStatus.Up && _draggingCut != Vector2.Zero)
                    {
                        LineSegment cutter = new LineSegment(_draggingCut, worldMouse);

                        for (int i = _connections.Count - 1; i >= 0; i--)
                        {
                            var connection = _connections[i];
                            LineSegment s = new LineSegment(connection.Start.Position + new Vector2(5f), connection.End.Position + new Vector2(5f));
                            if (s.Intersects(ref cutter)) _connections.RemoveAt(i);
                        }

                        _draggingCut = Vector2.Zero;
                    }
                }

                return true;
            });

            return Task.CompletedTask;
        }

        public override void Update()
        {
            if (_run) RopeSimSystem.Simulate2D(_points, _connections, new Vector2(0, 0.01f) * Engine.DeltaTime);
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.SetUseViewMatrix(true);

            foreach (RopeSimConnection2D p in _connections)
            {
                composer.RenderLine((p.Start.Position + new Vector2(5f)).ToVec3(), (p.End.Position + new Vector2(5f)).ToVec3(), Color.White);
            }

            if (_dragging != null)
            {
                Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
                composer.RenderLine((_dragging.Position + new Vector2(5f)).ToVec3(), worldMouse.ToVec3(), Color.Red);
            }

            if (_draggingCut != Vector2.Zero)
            {
                Vector2 worldMouse = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
                composer.RenderLine(_draggingCut.ToVec3(), worldMouse.ToVec3(), Color.Red);
            }

            foreach (var p in _points)
            {
                composer.RenderCircle(p.Position.ToVec3(), 5f, p.Locked ? Color.Magenta : Color.Black);
            }
        }

        public override void Unload()
        {
        }
    }
}
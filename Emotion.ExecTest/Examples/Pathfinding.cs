#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.AStar;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest.Examples
{
    /// <summary>
    /// Basic example for using the built in AStar pathfinding.
    /// Click to toggle a tile between walkable and unwalkable, shift click on a tile to path to it.
    /// </summary>
    public class Pathfinding : IScene
    {
        private static int _width = 5;
        private static int _height = 5;
        private static float _tileSize = 20;

        private static PathingGrid _grid = new(_width, _height, new Vector2(_tileSize));
        private static AStarContext _ctx = new(_grid);
        private List<Vector2> _path = new();
        private Dictionary<int, AStarNode> _gridDebugData = new();
        private int _lastPathHighestNodeScore;
        private Vector2 _pathfindingStartLocation = new Vector2(0);

        public void Load()
        {
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    _grid.SetWalkable(x, y, true);
                }
            }

            Engine.Renderer.Camera.Position = new Vector3(_width / 2, _height / 2, 0) * _tileSize;
        }

        public void Update()
        {
        }

        public void Draw(RenderComposer composer)
        {
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var tile = new Vector2(x, y);
                    Vector2 pos = tile * _tileSize;
                    if (_pathfindingStartLocation == tile)
                    {
                        composer.RenderSprite(new Vector3(pos, 0), new Vector2(_tileSize), Color.Yellow);
                        continue;
                    }

                    bool walkable = _grid.IsWalkable(x, y);
                    var rect = new Rectangle(pos, new Vector2(_tileSize, _tileSize));
                    bool inHere = rect.Contains(composer.Camera.ScreenToWorld(Engine.Host.MousePosition));
                    Color col = inHere ? Color.Blue : walkable ? Color.Green : Color.Red;

                    int cantor = Maths.GetCantorPair(x, y);
                    if (col == Color.Green && _gridDebugData.TryGetValue(cantor, out AStarNode curNode))
                    {
                        int score = curNode.F;
                        var mapped = (byte) Maths.Map(score, 0, _lastPathHighestNodeScore, 0, 255);
                        col = col.SetAlpha((byte) (265 - mapped));
                    }

                    composer.RenderSprite(new Vector3(pos, 0), new Vector2(_tileSize), col);

                    // Detect click on the tile.
                    if (!Engine.Host.IsKeyDown(Key.MouseKeyLeft) || !inHere) continue;
                    if (Engine.Host.IsKeyHeld(Key.LeftShift))
                    {
                        // The pathfinding operates on the grid, the coordinates here are tile coordinates.
                        _path = _ctx.FindPathWorldSpace(_pathfindingStartLocation * _tileSize, new Vector2(x, y) * _tileSize);
                        _gridDebugData = _ctx.DbgGetCalculationMeta();
                        _lastPathHighestNodeScore = 0;
                        foreach (var node in _gridDebugData)
                        {
                            if (node.Value.F > _lastPathHighestNodeScore) _lastPathHighestNodeScore = node.Value.F;
                        }
                    }
                    else
                    {
                        _grid.SetWalkable(x, y, !walkable);
                    }
                }
            }

            for (var i = 0; i < _path.Count - 1; i++)
            {
                composer.RenderLine(_path[i], _path[i + 1], Color.White, 0.5f);
            }
        }

        public void Unload()
        {
            _ctx.Dispose();
        }
    }
}
#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.AStar;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples
{
    /// <summary>
    /// Basic example for using the built in AStar pathfinding.
    /// Click to toggle a tile between walkable and unwalkable, shift click on a tile to path to it.
    /// </summary>
    public class Pathfinding : IScene
    {
        private static int _width = 10;
        private static int _height = 10;
        private static PathingGrid _grid = new PathingGrid(_width, _height);
        private static AStarContext _ctx = new AStarContext(_grid);
        private List<Vector2> _path = new List<Vector2>();
        private float _tileSize = 10;
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
                    bool walkable = _grid.IsWalkable(x, y);
                    Vector2 pos = new Vector2(x, y) * _tileSize;
                    var rect = new Rectangle(pos, new Vector2(_tileSize, _tileSize));
                    bool inHere = rect.Contains(composer.Camera.ScreenToWorld(Engine.InputManager.MousePosition));
                    Color col = inHere ? Color.Blue : walkable ? Color.Green : Color.Red;
                    composer.RenderSprite(new Vector3(pos, 0), new Vector2(_tileSize), col);

                    // Detect click on the tile.
                    if (!Engine.InputManager.IsKeyDown(Key.MouseKeyLeft) || !inHere) continue;
                    if (Engine.InputManager.IsKeyHeld(Key.LeftShift))
                        // The pathfinding operates on the grid, the coordinates here are tile coordinates.
                        _path = _ctx.FindPath(_pathfindingStartLocation, new Vector2(x, y));
                    else
                        _grid.SetWalkable(x, y, !walkable);
                }
            }

            for (var i = 0; i < _path.Count - 1; i++)
            {
                composer.RenderLine(_path[i] * _tileSize + new Vector2(_tileSize / 2), _path[i + 1] * _tileSize + new Vector2(_tileSize / 2), Color.White, 0.5f);
            }
        }

        public void Unload()
        {
            _ctx.Dispose();
        }
    }
}
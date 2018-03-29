// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.Game.Objects;
using Emotion.Primitives;
using TiledSharp;

#endregion

namespace Emotion.Game.AStar
{
    public class Grid
    {
        private Node[,] _nodes;

        /// <summary>
        /// Create a grid from a list of nodes.
        /// </summary>
        /// <param name="nodes">The list of nodes to create a matrix from.</param>
        public Grid(List<Node> nodes)
        {
        }

        /// <summary>
        /// Create a grid from a a Map object.
        /// </summary>
        public Grid(Map map, int layerId)
        {
            _nodes = new Node[map.TiledMap.Width, map.TiledMap.Height];

            TmxLayer mapLayer = map.TiledMap.Layers[layerId];


            // f(n) = g(n) + h(n)
            for (int x = 0; x < map.TiledMap.Width; x++)
            {
                for (int y = 0; y < map.TiledMap.Height; y++)
                {
                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void FindPath(Vector2 start, Vector2 end)
        {

        }
    }
}
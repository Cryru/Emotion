#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Utility;

#endregion

namespace Emotion.Game.AStar
{
    /// <summary>
    /// An AStar path.
    /// </summary>
    public class AStarContext : IDisposable
    {
        private HashSet<AStarNode> _openSet;
        private HashSet<AStarNode> _closedSet;
        private Dictionary<int, AStarNode> _cache;
        private List<AStarNode> _neighbors = new List<AStarNode>();

        protected PathingGrid _pathingGrid;

        /// <summary>
        /// Create a new A* path.
        /// </summary>
        /// <param name="pathingGrid">The aStarGrid the path is on.</param>
        public AStarContext(PathingGrid pathingGrid)
        {
            _pathingGrid = pathingGrid;
            _openSet = new HashSet<AStarNode>();
            _closedSet = new HashSet<AStarNode>();
            _cache = new Dictionary<int, AStarNode>();
        }

        /// <summary>
        /// Find a path within the grid.
        /// Allocate memory internally.
        /// </summary>
        /// <param name="start">The location to start pathing from</param>
        /// <param name="end">The location to path to.</param>
        /// <param name="diagonalMovement">Whether diagonal movement is allowed.</param>
        public List<Vector2> FindPath(Vector2 start, Vector2 end, bool diagonalMovement = false)
        {
            var newList = new List<Vector2>();
            FindPath(newList, start, end, diagonalMovement);
            return newList;
        }

        /// <summary>
        /// Find a path within the grid.
        /// </summary>
        /// <param name="pathMemory">The memory to fill with the path output.</param>
        /// <param name="start">The location to start pathing from</param>
        /// <param name="end">The location to path to.</param>
        /// <param name="diagonalMovement">Whether diagonal movement is allowed.</param>
        public void FindPath(List<Vector2> pathMemory, Vector2 start, Vector2 end, bool diagonalMovement = false)
        {
            pathMemory.Clear();

            AStarNode startNode = CreateNodeFromIfValid(start);
            AStarNode endNode = CreateNodeFromIfValid(end);
            if (startNode == null || endNode == null) return; // Invalid path

            _openSet.Clear();
            _closedSet.Clear();
            _openSet.Add(startNode);

            // Reset cache.
            foreach ((int _, AStarNode cachedNode) in _cache)
            {
                cachedNode.CameFrom = null;
                cachedNode.G = 0;
                cachedNode.H = 0;
            }

            // Loop while there are nodes in the open set, if there are none left and a path hasn't been found then there is no path.
            while (_openSet.Count > 0)
            {
                // Get the node with the lowest score. (F)
                AStarNode current = null;
                var closestF = 0;
                foreach (AStarNode node in _openSet)
                {
                    if (current != null && closestF <= node.F) continue;
                    current = node;
                    closestF = node.F;
                }

                if (current == null) break; // Should never occur.

                // Check if the current node is the end, in which case the path has been found.
                if (current.Equals(endNode))
                {
                    pathMemory.Add(endNode.Location);

                    // Trace the path backwards.
                    AStarNode trace = endNode;
                    while (trace.CameFrom != null)
                    {
                        AStarNode nextNode = trace.CameFrom;
                        pathMemory.Add(nextNode.Location);
                        trace = nextNode;
                    }

                    // Reverse so the goal isn't at the 0 index but is last node.
                    pathMemory.Reverse();

                    return;
                }

                // Update sets.
                _openSet.Remove(current);
                _closedSet.Add(current);

                // Get neighbors of current.
                GetNeighbors(_neighbors, current, diagonalMovement);

                // Apply heuristics to neighbors.
                for (var i = 0; i < _neighbors.Count; i++)
                {
                    AStarNode node = _neighbors[i];
                    node.H = Heuristic(node, endNode, current);
                }

                _neighbors.Sort();

                for (var i = 0; i < _neighbors.Count; i++)
                {
                    AStarNode neighbor = _neighbors[i];
                    if (neighbor.H < 0) continue;

                    // Check if the neighbor is done with, in which case we skip.
                    if (_closedSet.Contains(neighbor)) continue;

                    // Get the tentative distance between the current and the neighbor. Using 1 as distance.
                    int tentativeG = DistanceBetweenNodes(current, neighbor, endNode);

                    // Check if the neighbor is being evaluated.
                    if (_openSet.Contains(neighbor))
                    {
                        // Check if we have found a more efficient way to the neighbor node.
                        if (tentativeG < neighbor.G) neighbor.G = tentativeG;
                        else continue;
                    }
                    else
                    {
                        // Assign the calculated distance and add the node to the open set.
                        neighbor.G = tentativeG;
                        _openSet.Add(neighbor);
                    }

                    neighbor.CameFrom = current;
                }
            }
        }

        /// <summary>
        /// The function to be used for determining the heuristic value of each node. The first argument is the subject, and the
        /// second is the goal node.
        /// By default this is the euclidean distance.
        /// </summary>
        protected virtual int Heuristic(AStarNode current, AStarNode end, AStarNode currentFrom)
        {
            return (int)Vector2.Distance(current.Location, end.Location);
        }

        protected virtual int DistanceBetweenNodes(AStarNode current, AStarNode other, AStarNode end)
        {
            return current.G + 1;
        }

        /// <summary>
        /// Destroy, and free memory.
        /// </summary>
        public void Dispose()
        {
            _closedSet.Clear();
            _openSet.Clear();
            _cache.Clear();
            _closedSet = null;
            _openSet = null;
            _pathingGrid = null;
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected AStarNode CreateNodeFromIfValid(Vector2 loc)
        {
            var x = (int)loc.X;
            var y = (int)loc.Y;
            return CreateNodeFromIfValid(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected AStarNode CreateNodeFromIfValid(int x, int y)
        {
            if (!_pathingGrid.IsWalkable(x, y)) return null;

            int hashCode = Maths.GetCantorPair(x, y);
            if (_cache.TryGetValue(hashCode, out AStarNode node)) return node;
            var newNode = new AStarNode(x, y);
            _cache.Add(hashCode, newNode);
            return newNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void GetNeighbors(List<AStarNode> memory, AStarNode current, bool diagonal)
        {
            memory.Clear();

            int x = current.X;
            int y = current.Y;

            bool hasLeft = x > 0 && x <= _pathingGrid.Width - 1;
            bool hasRight = x >= 0 && x < _pathingGrid.Width - 1;
            bool hasTop = y > 0 && y <= _pathingGrid.Height - 1;
            bool hasBottom = y >= 0 && y < _pathingGrid.Height - 1;

            // Check for left.
            if (hasLeft)
            {
                AStarNode left = CreateNodeFromIfValid(x - 1, y);
                if (left != null) _neighbors.Add(left);

                // Check top left diagonal.
                if (diagonal && hasTop)
                {
                    AStarNode topLeft = CreateNodeFromIfValid(x - 1, y - 1);
                    if (topLeft != null) _neighbors.Add(topLeft);
                }
            }

            // Check for right.
            if (hasRight)
            {
                AStarNode right = CreateNodeFromIfValid(x + 1, y);
                if (right != null) _neighbors.Add(right);

                // Check top right diagonal.
                if (diagonal && hasTop)
                {
                    AStarNode topRight = CreateNodeFromIfValid(x + 1, y - 1);
                    if (topRight != null) _neighbors.Add(topRight);
                }
            }

            // Check for top.
            if (hasTop)
            {
                AStarNode top = CreateNodeFromIfValid(x, y - 1);
                if (top != null) _neighbors.Add(top);
            }

            // Check for bottom.
            // ReSharper disable once InvertIf
            if (hasBottom)
            {
                AStarNode bottom = CreateNodeFromIfValid(x, y + 1);
                if (bottom != null) _neighbors.Add(bottom);

                // Check bottom left diagonal.
                if (diagonal && hasLeft)
                {
                    AStarNode bottomLeft = CreateNodeFromIfValid(x - 1, y + 1);
                    if (bottomLeft != null) _neighbors.Add(bottomLeft);
                }

                // Check bottom right diagonal.
                // ReSharper disable once InvertIf
                if (diagonal && hasRight)
                {
                    AStarNode bottomRight = CreateNodeFromIfValid(x + 1, y + 1);
                    if (bottomRight != null) _neighbors.Add(bottomRight);
                }
            }
        }

        #endregion

        /// <summary>
        /// Find a path between two world space coordinates.
        /// </summary>
        public List<Vector2> FindPathWorldSpace(Vector2 start, Vector2 end, bool diagonalMovement = false)
        {
            Vector2 halfTile = _pathingGrid.PathGridTileSize / 2;
            start = _pathingGrid.WorldToPathTile(start);
            end = _pathingGrid.WorldToPathTile(end);

            List<Vector2> path = FindPath(start, end, diagonalMovement);
            for (var i = 0; i < path.Count; i++)
            {
                path[i] = path[i] * _pathingGrid.PathGridTileSize + halfTile;
            }

            return path;
        }

#if DEBUG

        public Dictionary<int, AStarNode> DbgGetCalculationMeta()
        {
            return _cache;
        }

#endif
    }
}
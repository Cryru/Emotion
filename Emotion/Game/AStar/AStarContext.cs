#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Game.AStar
{
    /// <summary>
    /// An AStar path.
    /// </summary>
    public sealed class AStarContext : IDisposable
    {
        /// <summary>
        /// The function to be used for determining the heuristic value of each node. The first argument is the subject, and the
        /// second is the goal node.
        /// By default this is the euclidean distance.
        /// </summary>
        public Func<AStarNode, AStarNode, int> HeuristicFunction = (subject, end) => (int) Vector2.Distance(new Vector2(subject.X, subject.Y), new Vector2(end.X, end.Y));

        private HashSet<AStarNode> _openSet;
        private HashSet<AStarNode> _closedSet;
        private Dictionary<int, AStarNode> _cache;

        private PathingGrid _pathingGrid;

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
        /// <param name="memory">The memory to fill with the path output.</param>
        /// <param name="start">The location to start pathing from</param>
        /// <param name="end">The location to path to.</param>
        /// <param name="diagonalMovement">Whether diagonal movement is allowed.</param>
        public void FindPath(List<Vector2> memory, Vector2 start, Vector2 end, bool diagonalMovement = false)
        {
            memory.Clear();

            AStarNode startNode = CreateNodeFromIfValid(start);
            AStarNode endNode = CreateNodeFromIfValid(end);
            if (startNode == null || endNode == null) return; // Invalid path

            _openSet.Clear();
            _closedSet.Clear();
            _openSet.Add(startNode);

            // Loop while there are nodes in the open set, if there are none left and a path hasn't been found then there is no path.
            while (_openSet.Count > 0)
            {
                // Get the node with the lowest score. (F)
                AStarNode current = _openSet.OrderBy(x => x.F).First();

                // Check if the current node is the end, in which case the path has been found.
                if (current.Equals(endNode))
                {
                    memory.Add(endNode.Location);

                    // Trace the path backwards.
                    AStarNode trace = endNode;
                    while (trace.CameFrom != null)
                    {
                        AStarNode nextNode = trace.CameFrom;
                        memory.Add(nextNode.Location);
                        trace = nextNode;
                    }

                    // Reverse so the goal isn't at the 0 index but is last node.
                    memory.Reverse();

                    return;
                }

                // Update sets.
                _openSet.Remove(current);
                _closedSet.Add(current);

                // Get neighbors of current.
                IEnumerable<AStarNode> neighbors = GetNeighbors(current.X, current.Y, diagonalMovement);

                foreach (AStarNode neighbor in neighbors)
                {
                    // Check if the neighbor is done with, in which case we skip.
                    if (_closedSet.Contains(neighbor)) continue;

                    // Get the tentative distance between the current and the neighbor. Using 1 as distance.
                    int tentativeG = current.G + 1;

                    // Check if the neighbor is being evaluated.
                    if (_openSet.Contains(neighbor))
                    {
                        // Check if we have found a more efficient way to the neighbor node.
                        if (tentativeG < neighbor.G) neighbor.G = tentativeG;
                    }
                    else
                    {
                        // Assign the calculated distance and add the node to the open set.
                        neighbor.G = tentativeG;
                        _openSet.Add(neighbor);
                    }

                    neighbor.H = HeuristicFunction(neighbor, endNode);
                    neighbor.CameFrom = current;
                }
            }
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
        private AStarNode CreateNodeFromIfValid(Vector2 loc)
        {
            var x = (int) loc.X;
            var y = (int) loc.Y;
            return CreateNodeFromIfValid(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AStarNode CreateNodeFromIfValid(int x, int y)
        {
            if (!_pathingGrid.IsWalkable(x, y)) return null;

            int hashCode = AStarNode.GetHashCode(x, y);
            if (_cache.TryGetValue(hashCode, out AStarNode node)) return node;
            var newNode = new AStarNode(x, y);
            _cache.Add(hashCode, newNode);
            return newNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<AStarNode> GetNeighbors(int x, int y, bool diagonal)
        {
            var neighbors = new List<AStarNode>();

            bool hasLeft = x > 0 && x <= _pathingGrid.Width - 1;
            bool hasRight = x >= 0 && x < _pathingGrid.Width - 1;
            bool hasTop = y > 0 && y <= _pathingGrid.Height - 1;
            bool hasBottom = y >= 0 && y < _pathingGrid.Height - 1;

            // Check for left.
            if (hasLeft)
            {
                AStarNode left = CreateNodeFromIfValid(x - 1, y);
                if (left != null) neighbors.Add(left);

                // Check top left diagonal.
                if (diagonal && hasTop)
                {
                    AStarNode topLeft = CreateNodeFromIfValid(x - 1, y - 1);
                    if (topLeft != null) neighbors.Add(topLeft);
                }
            }

            // Check for right.
            if (hasRight)
            {
                AStarNode right = CreateNodeFromIfValid(x + 1, y);
                if (right != null) neighbors.Add(right);

                // Check top right diagonal.
                if (diagonal && hasTop)
                {
                    AStarNode topRight = CreateNodeFromIfValid(x + 1, y - 1);
                    if (topRight != null) neighbors.Add(topRight);
                }
            }

            // Check for top.
            if (hasTop)
            {
                AStarNode top = CreateNodeFromIfValid(x, y - 1);
                if (top != null) neighbors.Add(top);
            }

            // Check for bottom.
            // ReSharper disable once InvertIf
            if (hasBottom)
            {
                AStarNode bottom = CreateNodeFromIfValid(x, y + 1);
                if (bottom != null) neighbors.Add(bottom);

                // Check bottom left diagonal.
                if (diagonal && hasLeft)
                {
                    AStarNode bottomLeft = CreateNodeFromIfValid(x - 1, y + 1);
                    if (bottomLeft != null) neighbors.Add(bottomLeft);
                }

                // Check bottom right diagonal.
                // ReSharper disable once InvertIf
                if (diagonal && hasRight)
                {
                    AStarNode bottomRight = CreateNodeFromIfValid(x + 1, y + 1);
                    if (bottomRight != null) neighbors.Add(bottomRight);
                }
            }

            return neighbors;
        }

        #endregion
    }
}
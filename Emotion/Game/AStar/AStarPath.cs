#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

#endregion

namespace Emotion.Game.AStar
{
    /// <summary>
    /// An AStar path.
    /// </summary>
    public sealed class AStarPath
    {
        #region Properties

        /// <summary>
        /// The function to be used for determining the heuristic value of each node. The first argument is the subject, and the
        /// second is the goal node.
        /// By default this is the euclidean distance.
        /// </summary>
        public Func<AStarNode, AStarNode, int> HeuristicFunction = (subject, end) => (int) Vector2.Distance(new Vector2(subject.X, subject.Y), new Vector2(end.X, end.Y));

        /// <summary>
        /// Whether the final path has been found.
        /// </summary>
        public bool Finished { get; private set; }

        /// <summary>
        /// Whether diagonal movement is allowed.
        /// </summary>
        public bool Diagonal { get; private set; }

        /// <summary>
        /// The path last found.
        /// Is null if the path was never Updated.
        /// </summary>
        public AStarNode[] LastFoundPath { get; private set; }

        #endregion

        private HashSet<AStarNode> _openSet;
        private HashSet<AStarNode> _closedSet;

        private AStarNode _end;
        private AStarGrid _aStarGrid;

        /// <summary>
        /// Create a new A* path.
        /// </summary>
        /// <param name="aStarGrid">The aStarGrid the path is on.</param>
        /// <param name="start">The starting point on the aStarGrid.</param>
        /// <param name="end">The ending point on the aStarGrid.</param>
        /// <param name="diagonalMovement">Whether diagonal movement is allowed.</param>
        public AStarPath(AStarGrid aStarGrid, Vector2 start, Vector2 end, bool diagonalMovement = true)
        {
            _openSet = new HashSet<AStarNode>();
            _closedSet = new HashSet<AStarNode>();

            _aStarGrid = aStarGrid;
            Diagonal = diagonalMovement;

            Reset(start, end);
        }

        /// <summary>
        /// Restart the path finding.
        /// </summary>
        /// <param name="start">The new starting point on the aStarGrid.</param>
        /// <param name="end">The new ending point on the aStarGrid.</param>
        public void Reset(Vector2 start, Vector2 end)
        {
            Finished = false;
            _openSet.Clear();
            _closedSet.Clear();

            _openSet.Add(_aStarGrid.GetNodeAt(start));
            _end = _aStarGrid.GetNodeAt(end);
        }

        /// <summary>
        /// Update the path until the goal is found.
        /// </summary>
        /// <returns>The path found.</returns>
        public AStarNode[] RunToEnd()
        {
            while (!Finished) Update();
            return LastFoundPath;
        }

        /// <summary>
        /// Update the path. Executes a single step of path finding until the final path has been found.
        /// </summary>
        /// <returns>The current path in progress, or null if there is no path.</returns>
        public AStarNode[] Update()
        {
            // Loop while there are nodes in the open set, if there are none left and a path hasn't been found then there is no path.
            if (_openSet.Count > 0)
            {
                // Get the node with the lowest score. (F)
                AStarNode current = _openSet.OrderBy(x => x.F).First();

                // Check if the current node is the end, in which case the path has been found.
                if (current.Equals(_end))
                {
                    Finished = true;
                }
                else
                {
                    // Update sets.
                    _openSet.Remove(current);
                    _closedSet.Add(current);

                    // Get neighbors of current.
                    IEnumerable<AStarNode> neighbors = GetNeighbors(current.X, current.Y);

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

                        neighbor.H = HeuristicFunction(neighbor, _end);
                        neighbor.CameFrom = current;
                    }
                }

                // Return the path as currently found.
                List<AStarNode> path = new List<AStarNode> {current};

                // Trace current's ancestry.
                while (current.CameFrom != null)
                {
                    path.Add(current.CameFrom);
                    current = current.CameFrom;
                }

                // Reverse so the goal isn't at the 0 index but is last node.
                path.Reverse();
                LastFoundPath = path.ToArray();

                return LastFoundPath;
            }

            Finished = true;
            return LastFoundPath;
        }

        /// <summary>
        /// Destroy, and free memory.
        /// </summary>
        public void Destroy()
        {
            _closedSet.Clear();
            _openSet.Clear();
            _closedSet = null;
            _openSet = null;
            _aStarGrid = null;
        }

        #region Helpers

        private IEnumerable<AStarNode> GetNeighbors(int x, int y)
        {
            List<AStarNode> neighbors = new List<AStarNode>();

            bool hasLeft = x > 0 && x <= _aStarGrid.Width - 1;
            bool hasRight = x >= 0 && x < _aStarGrid.Width - 1;
            bool hasTop = y > 0 && y <= _aStarGrid.Height - 1;
            bool hasBottom = y >= 0 && y < _aStarGrid.Height - 1;

            // Check for left.
            if (hasLeft)
            {
                AStarNode left = _aStarGrid.GetNodeAt(x - 1, y);
                if (left.Walkable) neighbors.Add(left);

                // Check top left diagonal.
                if (Diagonal && hasTop)
                {
                    AStarNode topLeft = _aStarGrid.GetNodeAt(x - 1, y - 1);
                    if (topLeft.Walkable) neighbors.Add(topLeft);
                }
            }

            // Check for right.
            if (hasRight)
            {
                AStarNode right = _aStarGrid.GetNodeAt(x + 1, y);
                if (right.Walkable) neighbors.Add(right);

                // Check top right diagonal.
                if (Diagonal && hasTop)
                {
                    AStarNode topRight = _aStarGrid.GetNodeAt(x + 1, y - 1);

                    if (topRight.Walkable) neighbors.Add(topRight);
                }
            }

            // Check for top.
            if (hasTop)
            {
                AStarNode top = _aStarGrid.GetNodeAt(x, y - 1);
                if (top.Walkable) neighbors.Add(top);
            }

            // Check for bottom.
            // ReSharper disable once InvertIf
            if (hasBottom)
            {
                AStarNode bottom = _aStarGrid.GetNodeAt(x, y + 1);
                if (bottom.Walkable) neighbors.Add(bottom);

                // Check bottom left diagonal.
                if (Diagonal && hasLeft)
                {
                    AStarNode bottomLeft = _aStarGrid.GetNodeAt(x - 1, y + 1);
                    if (bottomLeft.Walkable) neighbors.Add(bottomLeft);
                }

                // Check bottom right diagonal.
                // ReSharper disable once InvertIf
                if (Diagonal && hasRight)
                {
                    AStarNode bottomRight = _aStarGrid.GetNodeAt(x + 1, y + 1);
                    if (bottomRight.Walkable) neighbors.Add(bottomRight);
                }
            }

            return neighbors;
        }

        #endregion
    }
}
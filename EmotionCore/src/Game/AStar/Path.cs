// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.AStar
{
    public sealed class Path
    {
        #region Properties

        /// <summary>
        /// The function to be used for determining the heuristic value of each node. The first argument is the subject, and the
        /// second is the goal node.
        /// By default this is the euclidean distance.
        /// </summary>
        public Func<Node, Node, int> HeuristicFunction = (subject, end) => (int) Vector2.Distance(new Vector2(subject.X, subject.Y), new Vector2(end.X, end.Y));

        /// <summary>
        /// Whether the final path has been found.
        /// </summary>
        public bool Finished { get; private set; }

        #endregion

        private HashSet<Node> _openSet;
        private HashSet<Node> _closedSet;

        private Node _end;
        private Grid _grid;

        /// <summary>
        /// Create a new A* path.
        /// </summary>
        /// <param name="grid">The grid the path is on.</param>
        /// <param name="start">The starting point on the grid.</param>
        /// <param name="end">The ending point on the grid.</param>
        public Path(Grid grid, Vector2 start, Vector2 end)
        {
            _openSet = new HashSet<Node>();
            _closedSet = new HashSet<Node>();

            _grid = grid;

            Reset(start, end);
        }

        /// <summary>
        /// Restart the path finding.
        /// </summary>
        /// <param name="start">The new starting point on the grid.</param>
        /// <param name="end">The new ending point on the grid.</param>
        public void Reset(Vector2 start, Vector2 end)
        {
            Finished = false;
            _openSet.Clear();
            _closedSet.Clear();

            _openSet.Add(_grid.GetNodeAt(start));
            _end = _grid.GetNodeAt(end);
        }

        /// <summary>
        /// Update the path. Executes a single step of path finding until the final path has been found.
        /// </summary>
        /// <returns>The current path in progress, or null if there is no path.</returns>
        public Node[] Update()
        {
            // Loop while there are nodes in the open set, if there are none left and a path hasn't been found then there is no path.
            if (_openSet.Count > 0)
            {
                // Get the node with the lowest score. (F)
                Node current = _openSet.OrderBy(x => x.F).First();

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
                    Node[] neighbors = GetNeighbors(current.X, current.Y);

                    foreach (Node neighbor in neighbors)
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
                List<Node> path = new List<Node> {current};

                // Trace current's ancestry.
                while (current.CameFrom != null)
                {
                    path.Add(current.CameFrom);
                    current = current.CameFrom;
                }

                // Reverse so the goal isn't at the 0 index but is last node.
                path.Reverse();

                return path.ToArray();
            }

            Finished = true;
            return null;
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
            _grid = null;
        }

        #region Helpers

        private Node[] GetNeighbors(int x, int y)
        {
            List<Node> neighbors = new List<Node>();

            // Check for left.
            if (x != 0)
            {
                Node left = _grid.GetNodeAt(x - 1, y);
                if (left.Walkable) neighbors.Add(left);

                // Check top left diagonal.
                if (y != 0)
                {
                    Node topLeft = _grid.GetNodeAt(x - 1, y - 1);
                    if (topLeft.Walkable) neighbors.Add(topLeft);
                }
            }

            // Check for right.
            if (x != _grid.Width - 1)
            {
                Node right = _grid.GetNodeAt(x + 1, y);
                if (right.Walkable) neighbors.Add(right);

                // Check top right diagonal.
                if (y != 0)
                {
                    Node topRight = _grid.GetNodeAt(x + 1, y - 1);

                    if (topRight.Walkable) neighbors.Add(topRight);
                }
            }

            // Check for top.
            if (y != 0)
            {
                Node top = _grid.GetNodeAt(x, y - 1);
                if (top.Walkable) neighbors.Add(top);
            }

            // Check for bottom.
            // ReSharper disable once InvertIf
            if (y != _grid.Height - 1)
            {
                Node bottom = _grid.GetNodeAt(x, y + 1);
                if (bottom.Walkable) neighbors.Add(bottom);

                // Check bottom left diagonal.
                if (x != 0)
                {
                    Node bottomLeft = _grid.GetNodeAt(x - 1, y + 1);
                    if (bottomLeft.Walkable) neighbors.Add(bottomLeft);
                }

                // Check bottom right diagonal.
                // ReSharper disable once InvertIf
                if (x != _grid.Width - 1)
                {
                    Node bottomRight = _grid.GetNodeAt(x + 1, y + 1);
                    if (bottomRight.Walkable) neighbors.Add(bottomRight);
                }
            }

            return neighbors.ToArray();
        }

        #endregion
    }
}
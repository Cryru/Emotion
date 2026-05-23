#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Primitives.Grids;

#endregion

namespace Emotion.Game.Systems.Pathfinding.AStar;

/// <summary>
/// An AStar path.
/// </summary>
public class AStarContext : IDisposable
{
    private HashSet<AStarNode> _openSet;
    private HashSet<AStarNode> _closedSet;
    private Dictionary<int, AStarNode> _cache;
    private List<AStarNode> _neighbors = new List<AStarNode>();

    protected IGrid _pathingGrid;

    /// <summary>
    /// Create a new A* path.
    /// </summary>
    /// <param name="pathingGrid">The aStarGrid the path is on.</param>
    public AStarContext(IGrid pathingGrid)
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

        AStarNode? startNode = CreateNodeFromIfValid(start);
        AStarNode? endNode = CreateNodeFromIfValid(end);
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
        return (int) Vector2.Distance(current.Location, end.Location);
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
    protected AStarNode? CreateNodeFromIfValid(Vector2 loc)
    {
        var x = (int) loc.X;
        var y = (int) loc.Y;
        return CreateNodeFromIfValid(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected AStarNode? CreateNodeFromIfValid(int x, int y)
    {
        var loc = new Vector2(x, y);
        if (!_pathingGrid.IsValidPosition(loc)) return null;

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

        bool hasLeft = TryAddNeighbor(memory, x - 1, y);
        bool hasRight = TryAddNeighbor(memory, x + 1, y);
        bool hasTop = TryAddNeighbor(memory, x, y - 1);
        bool hasBottom = TryAddNeighbor(memory, x, y + 1);

        if (diagonal && hasTop)
        {
            // Check top left diagonal.
            if (hasLeft)
                TryAddNeighbor(memory, x - 1, y - 1);

            // Check top right diagonal.
            if (hasRight)
                TryAddNeighbor(memory, x + 1, y - 1);
        }

        if (diagonal && hasBottom)
        {
            // Check bottom left diagonal.
            if (hasLeft)
                TryAddNeighbor(memory, x - 1, y + 1);

            // Check bottom right diagonal.
            if (hasRight)
                TryAddNeighbor(memory, x + 1, y + 1);
        }
    }

    private bool TryAddNeighbor(List<AStarNode> memory, int x, int y)
    {
        AStarNode? node = CreateNodeFromIfValid(x, y);
        if (node == null) return false;

        memory.Add(node);
        return true;
    }

    #endregion

    /// <summary>
    /// Returns debugging information about the current state of the AStar context.
    /// The dictionary key is the node hash and the node contains information about the last pathing calculation.
    /// </summary>
    public Dictionary<int, AStarNode> DbgGetCalculationMeta()
    {
        return _cache;
    }
}

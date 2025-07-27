namespace Emotion.ThirdParty.TopologicalSort;

public static class TopologicalSort
{
    /// <summary>
    /// Topological Sorting (Kahn's algorithm) 
    /// https://en.wikipedia.org/wiki/Topological_sorting
    /// 
    /// Dependencies are in the format (dependingOn, node)
    /// Requirements:
    /// 1. One node can only depend on one other node.
    /// 2. No circular dependencies are supported
    /// </summary>
    public static List<T> Sort<T>(List<T> nodes, List<(T, T)> dependencies) where T : IEquatable<T>
    {
        // Empty list that will contain the sorted elements
        var output = new List<T>();

        // Set of all nodes which depend on no other node
        var nodesWithoutDepen = new List<T>();
        for (int i = 0; i < nodes.Count; i++)
        {
            T node = nodes[i];

            bool dependencyFound = false;
            for (int e = 0; e < dependencies.Count; e++)
            {
                if (dependencies[e].Item2.Equals(node))
                {
                    dependencyFound = true;
                    break;
                }
            }

            if (!dependencyFound)
                nodesWithoutDepen.Add(node);
        }

        while (nodesWithoutDepen.Count > 0)
        {
            // This node has no dependency - it is safe to add
            T node = nodesWithoutDepen[0];
            nodesWithoutDepen.RemoveAt(0);
            output.Add(node);

            // Find everyone who depends on this node and
            // add them as having no dependency (since it is now fulfilled)
            for (int d = dependencies.Count - 1; d >= 0; d--)
            {
                (T, T) dependency = dependencies[d];
                if (dependency.Item1.Equals(node))
                {
                    dependencies.RemoveAt(d);

                    T dependent = dependency.Item2;

                    // todo: you can check if this node depends on another node -
                    // but it's not necessary as we're sorting for unique only
                    nodesWithoutDepen.Add(dependent);
                }
            }
        }

        Assert(dependencies.Count == 0, "Topological sort with a circular dependency!");
        return output;
    }
}

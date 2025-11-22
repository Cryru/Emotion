#nullable enable

using System.Text;

namespace Emotion.Primitives.DataStructures;

/// <summary>
/// A tree data structure.
/// </summary>
/// <typeparam name="T">The data type representing the branch names.</typeparam>
/// <typeparam name="T2">The data type representing the leaf values.</typeparam>
public class NTree<T, T2> : IEnumerable<T2>
{
    public T? Name { get; init; }

    public NTree<T, T2>? Parent { get; init; } = null;

    public List<T2> Leaves { get; } = new List<T2>();

    public List<NTree<T, T2>> Branches { get; } = new List<NTree<T, T2>>();

    public NTree()
    {
    }

    protected NTree(T name, NTree<T, T2>? parent)
    {
        Name = name;
        Parent = parent;
    }

    /// <summary>
    /// Add a new leaf to the tree, adding all missing branches along the way.
    /// </summary>
    public void Add(Span<T> path, T2 value)
    {
        NTree<T, T2> target = this;
        foreach (T branchName in path)
        {
            target = target.AddGetBranch(branchName);
        }

        target.Leaves.Add(value);
    }

    /// <summary>
    /// Get an existing branch
    /// </summary>
    public NTree<T, T2>? GetBranch(T branch)
    {
        foreach (NTree<T, T2> subBranch in Branches)
        {
            if (Helpers.AreObjectsEqual(subBranch.Name, branch))
                return subBranch;
        }

        return null;
    }

    public NTree<T, T2>? GetBranchFromPath(T[] path)
    {
        NTree<T, T2> current = this;
        for (var i = 0; i < path.Length; i++)
        {
            T pathItem = path[i];

            var found = false;
            for (var j = 0; j < current.Branches.Count; j++)
            {
                NTree<T, T2> branch = current.Branches[j];
                if (Helpers.AreObjectsEqual(branch.Name, pathItem))
                {
                    current = branch;
                    found = true;
                    break;
                }
            }

            if (!found) return default;
        }

        return current;
    }

    /// <summary>
    /// Add a branch or get it.
    /// </summary>
    public NTree<T, T2> AddGetBranch(T branch)
    {
        NTree<T, T2>? existingBranch = GetBranch(branch);
        if (existingBranch != null)
            return existingBranch;

        var newBranch = new NTree<T, T2>(branch, this);
        Branches.Add(newBranch);
        return newBranch;
    }

    public void AddLeaf(T2 leaf)
    {
        Leaves.Add(leaf);
    }

    public IEnumerable<T2> ForEachLeaf()
    {
        Stack<NTree<T, T2>> stack = new();
        stack.Push(this);

        while (stack.TryPop(out NTree<T, T2>? nextBranch))
        {
            foreach (T2 leaf in nextBranch.Leaves)
            {
                yield return leaf;
            }

            foreach (NTree<T, T2> subBranch in nextBranch.Branches)
            {
                stack.Push(subBranch);
            }
        }
    }

    public IEnumerable<(T2, NTree<T, T2>)> ForEachLeafWithBranch()
    {
        Stack<NTree<T, T2>> stack = new();
        stack.Push(this);

        while (stack.TryPop(out NTree<T, T2>? nextBranch))
        {
            foreach (T2 leaf in nextBranch.Leaves)
            {
                yield return (leaf, nextBranch);
            }

            foreach (NTree<T, T2> subBranch in nextBranch.Branches)
            {
                stack.Push(subBranch);
            }
        }
    }

    public IEnumerator<T2> GetEnumerator()
    {
        return ForEachLeaf().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return $"Branch: {Name} [{Leaves.Count} Leaves]";
    }
}

public class NTree<T> : NTree<T, T>
{
}
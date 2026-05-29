#nullable enable

using System.Text;

namespace Emotion.Primitives.DataStructures;

/// <summary>
/// A tree data structure.
/// </summary>
/// <typeparam name="TBranchName">The data type representing the branch names.</typeparam>
/// <typeparam name="TLeafType">The data type representing the leaf values.</typeparam>
public class NTree<TBranchName, TLeafType> : IEnumerable<TLeafType>
{
    public TBranchName? Name { get; init; }

    public NTree<TBranchName, TLeafType>? Parent { get; init; } = null;

    public List<TLeafType> Leaves { get; } = new List<TLeafType>();

    public List<NTree<TBranchName, TLeafType>> Branches { get; } = new List<NTree<TBranchName, TLeafType>>();

    public NTree()
    {
    }

    protected NTree(TBranchName name, NTree<TBranchName, TLeafType>? parent)
    {
        Name = name;
        Parent = parent;
    }

    /// <summary>
    /// Add a new leaf to the tree, adding all missing branches along the way.
    /// </summary>
    public void Add(Span<TBranchName> path, TLeafType value)
    {
        NTree<TBranchName, TLeafType> target = this;
        foreach (TBranchName branchName in path)
        {
            target = target.AddGetBranch(branchName);
        }

        target.Leaves.Add(value);
    }

    /// <summary>
    /// Get an existing branch
    /// </summary>
    public NTree<TBranchName, TLeafType>? GetBranch(TBranchName branch)
    {
        foreach (NTree<TBranchName, TLeafType> subBranch in Branches)
        {
            if (Helpers.AreObjectsEqual(subBranch.Name, branch))
                return subBranch;
        }

        return null;
    }

    public NTree<TBranchName, TLeafType>? GetBranchFromPath(TBranchName[] path)
    {
        NTree<TBranchName, TLeafType> current = this;
        for (var i = 0; i < path.Length; i++)
        {
            TBranchName pathItem = path[i];

            var found = false;
            for (var j = 0; j < current.Branches.Count; j++)
            {
                NTree<TBranchName, TLeafType> branch = current.Branches[j];
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
    public NTree<TBranchName, TLeafType> AddGetBranch(TBranchName branch)
    {
        NTree<TBranchName, TLeafType>? existingBranch = GetBranch(branch);
        if (existingBranch != null)
            return existingBranch;

        var newBranch = new NTree<TBranchName, TLeafType>(branch, this);
        Branches.Add(newBranch);
        return newBranch;
    }

    public void AddLeaf(TLeafType leaf)
    {
        Leaves.Add(leaf);
    }

    public IEnumerable<TLeafType> ForEachLeaf()
    {
        Stack<NTree<TBranchName, TLeafType>> stack = new();
        stack.Push(this);

        while (stack.TryPop(out NTree<TBranchName, TLeafType>? nextBranch))
        {
            foreach (TLeafType leaf in nextBranch.Leaves)
            {
                yield return leaf;
            }

            foreach (NTree<TBranchName, TLeafType> subBranch in nextBranch.Branches)
            {
                stack.Push(subBranch);
            }
        }
    }

    public IEnumerable<(TLeafType, NTree<TBranchName, TLeafType>)> ForEachLeafWithBranch()
    {
        Stack<NTree<TBranchName, TLeafType>> stack = new();
        stack.Push(this);

        while (stack.TryPop(out NTree<TBranchName, TLeafType>? nextBranch))
        {
            foreach (TLeafType leaf in nextBranch.Leaves)
            {
                yield return (leaf, nextBranch);
            }

            foreach (NTree<TBranchName, TLeafType> subBranch in nextBranch.Branches)
            {
                stack.Push(subBranch);
            }
        }
    }

    public IEnumerator<TLeafType> GetEnumerator()
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
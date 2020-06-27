#region Using

using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Emotion.Game
{
    /// <summary>
    /// A tree data structure.
    /// </summary>
    /// <typeparam name="T">The data type representing the branch names.</typeparam>
    /// <typeparam name="T2">The data type representing the leaf values.</typeparam>
    public class Tree<T, T2> : IEnumerable<T2>
    {
        public T Name { get; set; }
        public List<T2> Leaves { get; set; } = new List<T2>();
        public List<Tree<T, T2>> Branches { get; set; } = new List<Tree<T, T2>>();

        protected Tree(T name)
        {
            Name = name;
        }

        public Tree()
        {
        }

        /// <summary>
        /// Add a leaf to the tree.
        /// </summary>
        /// <param name="path">The path to the branch/es to add the leaf to.</param>
        /// <param name="value">The value to add as a leaf.</param>
        public void Add(T[] path, T2 value)
        {
            Tree<T, T2> target = this;
            foreach (T name in path)
            {
                Tree<T, T2> current = target.Branches.FirstOrDefault(x => x.Name.Equals(name));
                if (current == null)
                {
                    current = new Tree<T, T2>(name);
                    target.Branches.Add(current);
                }

                target = current;
            }

            target.Leaves.Add(value);
        }

        private class IterationState
        {
            public Tree<T, T2> Branch;
            public int BranchIdx;
        }

        /// <summary>
        /// Coroutine for iterating the tree. The leaves of each node are returned as we iterate towards the deepest level and then
        /// backward.
        /// </summary>
        /// <param name="rootNode">The node to start from.</param>
        /// <returns>The leaf values along the tree.</returns>
        public IEnumerator<T2> TreeEnumeratorCoroutine(Tree<T, T2> rootNode)
        {
            var list = new List<IterationState>
            {
                new IterationState {Branch = rootNode, BranchIdx = 0}
            };

            while (list.Count > 0)
            {
                IterationState branch = list[^1];
                Tree<T, T2> newBranch = branch.Branch.Branches[branch.BranchIdx];
                foreach (T2 leaf in newBranch.Leaves)
                {
                    yield return leaf;
                }

                if (newBranch.Branches.Count > 0) list.Add(new IterationState {Branch = newBranch, BranchIdx = 0});

                branch.BranchIdx++;
                if (branch.BranchIdx == branch.Branch.Branches.Count) list.Remove(branch);
            }
        }

        public IEnumerator<T2> GetEnumerator()
        {
            return TreeEnumeratorCoroutine(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
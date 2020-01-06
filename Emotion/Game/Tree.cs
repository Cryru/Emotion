#region Using

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
    public class Tree<T, T2>  
    {
        public T Name { get; set; }
        public List<T2> Leaves { get; set; } = new List<T2>();
        public List<Tree<T, T2>> Branches { get; set; } = new List<Tree<T, T2>>();

        public Tree(T name)
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
    }
}
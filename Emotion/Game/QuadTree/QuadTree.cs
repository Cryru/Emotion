#region Using

using System.Collections;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.QuadTree
{
    /// <summary>
    /// A QuadTree Object that provides storage of objects in a space with fast querying.
    /// </summary>
    /// <typeparam name="T">Any object implementing Transform.</typeparam>
    public class QuadTree<T> : QuadTreeNode<T>, ICollection<T> where T : class, IQuadTreeObject
    {
        /// <summary>
        /// All objects in the entire tree.
        /// </summary>
        private readonly Dictionary<T, QuadTreeObject<T>> _wrappedDictionary = new Dictionary<T, QuadTreeObject<T>>();

        /// <summary>
        /// Creates a QuadTree of the specified dimensions.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        /// <param name="nodeCapacity">The number of objects a node can contain before it subdivides.</param>
        public QuadTree(Rectangle rect, int nodeCapacity = 8) : base(rect, nodeCapacity)
        {
        }

        /// <inheritdoc />
        protected override int ObjectCount()
        {
            return _wrappedDictionary.Count;
        }

        /// <inheritdoc />
        public override void GetAllObjects(List<T> results)
        {
            results.AddRange(_wrappedDictionary.Keys);
        }

        #region ICollection<T> Members

        /// <summary>
        /// Add an object to the quad tree.
        /// It will be distributed to the appropriate leaf and updated when it moves.
        /// </summary>
        public void Add(T item)
        {
            if (item == null) return;
            Assert(!Contains(item));

            var wrappedObject = new QuadTreeObject<T>(item);

            _wrappedDictionary.Add(item, wrappedObject);
            Insert(wrappedObject);
            wrappedObject.AttachToQuadTree();
        }

        /// <summary>
        /// Returns whether any leaf in the tree contains this object.
        /// </summary>
        public bool Contains(T item)
        {
            return item != null && _wrappedDictionary.ContainsKey(item);
        }

        /// <summary>
        /// Remove an object from the tree, whichever leaf it is located in.
        /// </summary>
        public bool Remove(T item)
        {
            if (item == null) return false;
            if (!Contains(item)) return false;

            // Remove from the tree.
            QuadTreeObject<T> wrappedItem = _wrappedDictionary[item];
            wrappedItem.DetachFromQuadTree();
            _wrappedDictionary.Remove(item);

            // Remove the item from its owner.
            wrappedItem.Owner.Remove(wrappedItem);
            wrappedItem.Owner.RemoveEmptyLeavesUpwards();

            return true;
        }

        /// <inheritdoc cref="QuadTreeNode{T}" />
        public override void Clear()
        {
            _wrappedDictionary.Clear();
            base.Clear();
        }

        public bool IsReadOnly
        {
            get => false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new Exception("The quad tree doesn't have indices, so CopyTo is not available.");
        }

        #endregion

        #region IEnumerable<T> and IEnumerable Members

        /// <summary>
        /// Returns an enumerator for the whole tree.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (KeyValuePair<T, QuadTreeObject<T>> keyValue in _wrappedDictionary)
            {
                yield return keyValue.Key;
            }
        }

        /// <summary>
        /// Returns an enumerator for the whole tree.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Render an outline of all leaves in the tree.
        /// Used for debugging purposes.
        /// </summary>
        public void RenderDebug(RenderComposer c, Color color)
        {
            var treeTraverse = new Queue<QuadTreeNode<T>>();
            treeTraverse.Enqueue(this);
            while (treeTraverse.TryDequeue(out QuadTreeNode<T> quadTree))
            {
                c.RenderOutline(quadTree.QuadRect, color);

                if (quadTree.TopLeftChild == null) continue;
                treeTraverse.Enqueue(quadTree.TopLeftChild);
                treeTraverse.Enqueue(quadTree.TopRightChild);
                treeTraverse.Enqueue(quadTree.BottomLeftChild);
                treeTraverse.Enqueue(quadTree.BottomRightChild);
            }
        }
    }
}
#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.QuadTree
{
    /// <summary>
    /// A QuadTree Object that provides storage of objects in a space with fast querying.
    /// </summary>
    /// <typeparam name="T">Any object implementing Transform.</typeparam>
    public class QuadTree<T> : QuadTreeNode<T>, ICollection<T> where T : Transform
    {
        private readonly Dictionary<T, QuadTreeObject<T>> _wrappedDictionary = new Dictionary<T, QuadTreeObject<T>>();

        /// <summary>
        /// Creates a QuadTree of the specified dimensions.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        /// <param name="nodeCapacity">The number of objects a node can contain before it subdivides.</param>
        public QuadTree(Rectangle rect, int nodeCapacity = 8) : base(rect, nodeCapacity)
        {
        }

        /// <summary>
        /// Creates a QuadTree of the specified dimensions.
        /// </summary>
        /// <param name="x">The top-left position of the area rectangle.</param>
        /// <param name="y">The top-right position of the area rectangle.</param>
        /// <param name="width">The width of the area rectangle.</param>
        /// <param name="height">The height of the area rectangle.</param>
        /// <param name="nodeCapacity">The number of objects a node can contain before it subdivides.</param>
        public QuadTree(int x, int y, int width, int height, int nodeCapacity = 8) : this(new Rectangle(x, y, width, height), nodeCapacity)
        {
        }

        /// <inheritdoc />
        protected override int ObjectCount()
        {
            // Fast path for the root since it keeps a wrapped dictionary of all objects.
            return _wrappedDictionary.Count;
        }

        /// <inheritdoc />
        public override void GetAllObjects(ref List<T> results)
        {
            // Fast path for the root since it keeps a wrapped dictionary of all objects. 
            results.AddRange(_wrappedDictionary.Keys);
        }

        #region ICollection<T> Members

        /// <summary>
        /// Add an object to the quad tree. It will be distributed to the appropriate leaf and
        /// updated when it moves.
        /// </summary>
        public void Add(T item)
        {
            if (item == null) return;
            Debug.Assert(!_wrappedDictionary.ContainsKey(item));

            var wrappedObject = new QuadTreeObject<T>(item);
            _wrappedDictionary.Add(item, wrappedObject);
            Insert(wrappedObject);

            // Attach to move event.
            item.OnMove += (s, _) => ObjectMovedInternal((T)s);
        }

        private bool ObjectMovedInternal(T item)
        {
            if (!Contains(item)) return false;

            QuadTreeObject<T> unwrappedItem = _wrappedDictionary[item];
            if (unwrappedItem.Owner != null)
                unwrappedItem.Owner.Relocate(unwrappedItem);
            else
                Relocate(unwrappedItem);

            return true;
        }

        /// <inheritdoc cref="QuadTreeNode{T}" />
        public override void Clear()
        {
            _wrappedDictionary.Clear();
            base.Clear();
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

            // Remove the item from its owner and cleanup empty leaves.
            QuadTreeObject<T> wrappedItem = _wrappedDictionary[item];
            wrappedItem.Owner.Remove(wrappedItem);
            wrappedItem.Owner.RemoveEmptyLeavesUpwards();

            _wrappedDictionary.Remove(item);
            return true;
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
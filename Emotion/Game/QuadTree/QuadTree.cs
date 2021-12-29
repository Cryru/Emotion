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
    public class QuadTree<T> : QuadTreeNode<T>, ICollection<T> where T : IQuadTreeObject
    {
        /// <summary>
        /// Whether the quad tree can grow.
        /// </summary>
        public bool Dynamic;

        private int _dynamicRebuildCapacity;
        private bool _dynamicRebuildInProgress;

        /// <summary>
        /// All objects in the entire tree.
        /// </summary>
        private HashSet<T> _allObjects = new HashSet<T>();

        /// <summary>
        /// Creates a QuadTree of the specified dimensions.
        /// </summary>
        /// <param name="rect">The area this QuadTree object will encompass.</param>
        /// <param name="nodeCapacity">The number of objects a node can contain before it subdivides.</param>
        public QuadTree(Rectangle rect, int nodeCapacity = 8) : base(rect, nodeCapacity)
        {
            Dynamic = rect == Rectangle.Empty;
            _dynamicRebuildCapacity = NodeCapacity;
        }

        protected override void InsertInternal(T item, bool outOfBounds = false)
        {
            base.InsertInternal(item, outOfBounds);
            if (!Dynamic) return;

            if (outOfBounds)
            {
                Rectangle itemBounds = item.GetBoundsForQuadTree();
                if (float.IsNormal(itemBounds.X) && float.IsNormal(itemBounds.Y) && float.IsNormal(itemBounds.Width) && float.IsNormal(itemBounds.Height))
                    _quadRect = Rectangle.Union(_quadRect, itemBounds);
            }

            // Rebuild the entire tree.
            // There is probably a better way of doing this, but dynamic quad trees are a bit meh.
            if (_objects.Count >= _dynamicRebuildCapacity && !_dynamicRebuildInProgress)
            {
                _dynamicRebuildInProgress = true;
                Clear();
                foreach (T obj in _allObjects)
                {
                    Insert(obj);
                }
                _dynamicRebuildInProgress = false;

                // Prevent instant rebuild on next insert if there are a lot of objects out of bounds.
                _dynamicRebuildCapacity = Math.Max(_objects.Count * 2, NodeCapacity);
            }
        }

        #region ICollection<T> Members

        /// <summary>
        /// Add an object to the quad tree.
        /// It will be distributed to the appropriate leaf and updated when it moves.
        /// </summary>
        public void Add(T item)
        {
            if (item == null) return;
            Debug.Assert(!Contains(item));

            Insert(item);
            _allObjects.Add(item);
            item.AddedToQuadTree();
        }

        /// <summary>
        /// Returns whether any leaf in the tree contains this object.
        /// </summary>
        public bool Contains(T item)
        {
            return item != null && _allObjects.Contains(item);
        }

        /// <summary>
        /// Remove an object from the tree, whichever leaf it is located in.
        /// </summary>
        public new bool Remove(T item)
        {
            if (item == null) return false;
            if (!Contains(item)) return false;

            // Remove the item from its owner and cleanup empty leaves.
            var owner = (QuadTreeNode<T>) item.Owner;
            owner.Remove(item);
            owner.RemoveEmptyLeavesUpwards();
            item.RemovedFromQuadTree();
            _allObjects.Remove(item);

            return true;
        }

        public bool IsReadOnly
        {
            get => false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _allObjects.CopyTo(array, arrayIndex);
        }

        #endregion

        #region IEnumerable<T> and IEnumerable Members

        /// <summary>
        /// Returns an enumerator for the whole tree.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _allObjects.GetEnumerator();
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
                c.RenderOutlineScreenSpace(quadTree.QuadRect, color);

                if (quadTree.TopLeftChild == null) continue;
                treeTraverse.Enqueue(quadTree.TopLeftChild);
                treeTraverse.Enqueue(quadTree.TopRightChild);
                treeTraverse.Enqueue(quadTree.BottomLeftChild);
                treeTraverse.Enqueue(quadTree.BottomRightChild);
            }
        }
    }
}
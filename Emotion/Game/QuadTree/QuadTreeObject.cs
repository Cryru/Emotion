#region Using

using System;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.QuadTree
{
    public interface IQuadTreeObject
    {
        public event EventHandler OnMove;
        public event EventHandler OnResize;

        /// <summary>
        /// These are the bounds the object will be considered to have in the quad tree.
        /// Preferably should be a AABB.
        /// </summary>
        public Rectangle GetBoundsForQuadTree();
    }

    /// <summary>
    /// Used internally to attach an Owner to each object stored in the QuadTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuadTreeObject<T> where T : IQuadTreeObject
    {
        /// <summary>
        /// The wrapped data value
        /// </summary>
        public T Data;

        /// <summary>
        /// The QuadTreeNode that owns this object
        /// </summary>
        public QuadTreeNode<T> Owner;

        /// <summary>
        /// Wraps the data value
        /// </summary>
        /// <param name="data">The data value to wrap</param>
        public QuadTreeObject(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Called when the object is added to a quad tree.
        /// </summary>
        public void AttachToQuadTree()
        {
            Data.OnMove += NotifyQuadTreeChange;
            Data.OnResize += NotifyQuadTreeChange;
        }

        /// <summary>
        /// Called when the object leaves a quad tree.
        /// </summary>
        public void DetachFromQuadTree()
        {
            Data.OnMove -= NotifyQuadTreeChange;
            Data.OnResize -= NotifyQuadTreeChange;
        }

        private void NotifyQuadTreeChange(object sender, EventArgs e)
        {
            Debug.Assert(Owner != null);
            Owner.Relocate(this);
        }
    }
}
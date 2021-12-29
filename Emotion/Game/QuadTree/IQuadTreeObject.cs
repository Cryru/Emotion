#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.QuadTree
{
    public interface IQuadTreeObject
    {
        /// <summary>
        /// The node which owns this object.
        /// </summary>
        public QuadTreeNode Owner { get; set; }

        /// <summary>
        /// Called when the object is added to a quad tree.
        /// </summary>
        public void AddedToQuadTree();

        /// <summary>
        /// Called when the object leaves a quad tree.
        /// </summary>
        public void RemovedFromQuadTree();

        /// <summary>
        /// These are the bounds the object will be considered to have in the quad tree.
        /// Preferably should be a AABB.
        /// </summary>
        public Rectangle GetBoundsForQuadTree();
    }
}
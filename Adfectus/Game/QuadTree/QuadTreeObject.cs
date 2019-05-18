#region Using

using Adfectus.Primitives;

#endregion

namespace Adfectus.Game.QuadTree
{
    /// <summary>
    /// Used internally to attach an Owner to each object stored in the QuadTree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuadTreeObject<T> where T : Transform
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
    }
}
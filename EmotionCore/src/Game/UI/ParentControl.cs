// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    /// <summary>
    /// A UI control which parents other UI controls.
    /// </summary>
    public abstract class ParentControl : Control
    {
        /// <summary>
        /// The number of children attached to this control.
        /// </summary>
        public int ChildCount
        {
            get => _children.Count;
        }

        /// <summary>
        /// The children of this control.
        /// </summary>
        protected List<Transform> _children { get; set; } = new List<Transform>();

        /// <summary>
        /// Create a new parent control.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        protected ParentControl(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        /// <summary>
        /// Add a child to this control.
        /// </summary>
        /// <param name="transform">The child to add. Can be a transform or a control.</param>
        public virtual void AddChild(Transform transform)
        {
            if (transform.Z <= Z) transform.Z++;

            Context.Log.Trace($"[{this}] added child transform {transform}.", MessageSource.UIController);
            _children.Add(transform);

            if (!(transform is Control control)) return;
            control.Parent = this;
            Controller.Add(control);
        }

        /// <summary>
        /// Returns the child at the requested index.
        /// </summary>
        /// <param name="index">The index of the control to return.</param>
        public Transform GetChild(int index)
        {
            lock (_children)
            {
                return _children[index];
            }
        }

        /// <summary>
        /// Returns child of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of children to return.</typeparam>
        /// <returns>The found children of the specified type.</returns>
        public Transform[] GetChildByType<T>()
        {
            lock (_children)
            {
                return _children.Where(x => x is T).ToArray();
            }
        }

        /// <summary>
        /// Remove a child of this control.
        /// </summary>
        /// <param name="transform">A reference to the child to remove.</param>
        public virtual void RemoveChild(Transform transform)
        {
            Context.Log.Trace($"[{this}] removed child transform {transform}.", MessageSource.UIController);
            _children.Remove(transform);

            if (transform is Control control) Controller.Remove(control);
        }

        /// <summary>
        /// Renders all children.
        /// </summary>
        public override void Render()
        {
            foreach (Transform child in _children)
            {
                // Check if the child is a control.
                if (child is Control childControl)
                {
                    if (!childControl.Active) continue;

                    Context.Renderer.Render(childControl);
                    continue;
                }

                // Check if the child is a transform renderable.
                if (child is TransformRenderable childRenderable) Context.Renderer.Render(childRenderable);
            }
        }

        /// <summary>
        /// Ensures children are destroyed.
        /// </summary>
        protected override void InternalDestroy()
        {
            foreach (Transform child in _children)
            {
                // Check if the child is a control.
                if (!(child is Control childControl)) continue;

                Controller.Remove(childControl);
            }

            _children.Clear();
        }
    }
}
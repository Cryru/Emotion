// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Debug;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
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

        protected ParentControl(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        /// <summary>
        /// Add a child to this control.
        /// </summary>
        /// <param name="transform">The child to add. Can be a transform or a control.</param>
        public virtual void AddChild(Transform transform)
        {
            if(transform.Z <= Z) transform.Z++;

            Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{this}] adding child transform.");
            _children.Add(transform);

            if (transform is Control control)
            {
                control.Parent = this;
                Controller.Add(control);
            }
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
            Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{this}] removing child transform.");
            _children.Remove(transform);

            if (transform is Control control) Controller.Remove(control);
        }

        /// <summary>
        /// Renders all children.
        /// </summary>
        /// <param name="renderer"></param>
        public override void Render(Renderer renderer)
        {
            foreach (Transform child in _children)
            {
                // Check if the child is a control.
                if (child is Control childControl)
                {
                    if (!childControl.Active) continue;

                    renderer.Render(childControl);
                    continue;
                }

                // Check if the child is a transform renderable.
                if (child is TransformRenderable childRenderable)
                {
                    renderer.Render(childRenderable);
                }
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
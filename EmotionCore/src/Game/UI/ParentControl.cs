// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.Debugging;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public abstract class ParentControl : Control
    {
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
            Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{this}] adding child transform.");
            _children.Add(transform);

            if (transform is Control control)
            {
                control.Parent = this;
                Controller.Add(control);
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

            if (transform is Control control)
            {
                Controller.Remove(control);
            }
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
                if (!(child is Control childControl)) continue;

                if (!childControl.Active) continue;

                renderer.Render(childControl);
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
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.Debug;
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
        protected List<Control> _children { get; set; } = new List<Control>();

        protected ParentControl(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        /// <summary>
        /// Add a child to this control.
        /// </summary>
        /// <param name="control">The child to add.</param>
        public void AddChild(Control control)
        {
            Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{this}] adding child of type [{control.GetType()}]");
            _children.Add(control);
            control.Parent = this;
            Controller.Add(control);
        }

        /// <summary>
        /// Remove a child of this control.
        /// </summary>
        /// <param name="control">A reference to the control to remove.</param>
        public void RemoveChild(Control control)
        {
            Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{this}] removing child of type [{control.GetType()}]");
            _children.Remove(control);
            Controller.Remove(control);
        }

        /// <summary>
        /// Renders all children.
        /// </summary>
        /// <param name="renderer"></param>
        public override void Render(Renderer renderer)
        {
            foreach (Control child in _children)
            {
                if (!child.Active) continue;

                renderer.Render(child);
            }
        }

        /// <summary>
        /// Ensures children are destroyed.
        /// </summary>
        public override void Destroy()
        {
            foreach (Control child in _children)
            {
                Controller.Remove(child);
            }

            base.Destroy();
        }
    }
}
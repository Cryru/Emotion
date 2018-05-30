// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.GLES;
using Emotion.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public abstract class Control : Transform
    {
        #region Properties

        /// <summary>
        /// The object's priority. The higher the number is the higher the object will be.
        /// </summary>
        public int Priority { get; protected set; }

        /// <summary>
        /// Whether the control is visible and responsive.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// The controller which owns this control.
        /// </summary>
        protected Controller _controller;

        /// <summary>
        /// Whether the control is destroyed.
        /// </summary>
        public bool Destroyed { get; protected set; }

        #endregion

        #region State

        /// <summary>
        /// Whether the mouse is inside the control.
        /// </summary>
        public bool MouseInside { get; internal set; }

        /// <summary>
        /// Whether each button is held on the control.
        /// </summary>
        public bool[] Held { get; internal set; } = new bool[Enum.GetNames(typeof(MouseKeys)).Length];

        #endregion

        protected Control(Controller controller, Rectangle bounds, int priority) : base(bounds)
        {
            Priority = priority;
            _controller = controller;

            _controller.Add(this);
        }

        public virtual void Destroy()
        {
            Destroyed = true;
        }

        public abstract void Draw(Renderer renderer);

        #region UI Events

        public virtual void MouseEnter(Vector2 mousePosition)
        {
        }

        public virtual void MouseLeave(Vector2 mousePosition)
        {
        }

        public virtual void MouseDown(MouseKeys key)
        {
        }

        public virtual void MouseUp(MouseKeys key)
        {
        }

        public virtual void MouseMoved(Vector2 oldPosition, Vector2 newPosition)
        {

        }

        #endregion

        
        #region Debugging

        public override string ToString()
        {
            string result = "[" + base.ToString() + "]";
            result += $"(priority: {Priority}, active: {Active})";
            return result;
        }

        #endregion
    }
}
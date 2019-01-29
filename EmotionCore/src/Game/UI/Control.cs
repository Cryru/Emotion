// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    /// <summary>
    /// A UI control.
    /// </summary>
    public abstract class Control : TransformRenderable
    {
        #region Properties

        /// <summary>
        /// Whether the control is visible and responsive.
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// The controller which owns this control.
        /// </summary>
        public Controller Controller
        {
            get
            {
                if (_controller == null) throw new Exception("A UI control cannot use the UIController property before it is added to one.");

                return _controller;
            }
            protected set => _controller = value;
        }

        private Controller _controller;

        /// <summary>
        /// Whether the control is destroyed.
        /// </summary>
        public bool Destroyed { get; protected set; }

        /// <summary>
        /// The parent of this control, if any.
        /// </summary>
        public Control Parent { get; internal set; }

        #endregion

        #region State

        /// <summary>
        /// Whether the mouse is inside the control. Can be interpreted as being focused.
        /// </summary>
        public bool MouseInside { get; internal set; }

        /// <summary>
        /// Whether each button is held on the control.
        /// </summary>
        public bool[] Held { get; internal set; } = new bool[Enum.GetNames(typeof(MouseKeys)).Length];

        /// <summary>
        /// Whether the control was active. Used for active and deactivate events.
        /// </summary>
        public bool WasActive { get; internal set; } = true;

        #endregion

        /// <summary>
        /// Create a new UI control.
        /// </summary>
        /// <param name="position">The position of the control on the screen.</param>
        /// <param name="size">The size of the control.</param>
        protected Control(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        /// <summary>
        /// Perform control initialization here.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Perform cleanup here.
        /// </summary>
        protected virtual void InternalDestroy()
        {
        }

        /// <summary>
        /// Is called by the UI controller when destroying the control. Calls internal destroy.
        /// </summary>
        internal void Destroy()
        {
            Destroyed = true;
            InternalDestroy();
        }

        #region UI Events

        /// <summary>
        /// The mouse has entered the control.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse.</param>
        public virtual void MouseEnter(Vector2 mousePosition)
        {
        }

        /// <summary>
        /// The mouse has left the control.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse.</param>
        public virtual void MouseLeave(Vector2 mousePosition)
        {
        }

        /// <summary>
        /// The mouse has clicked on the control.
        /// </summary>
        /// <param name="key">The key which was clicked.</param>
        public virtual void MouseDown(MouseKeys key)
        {
        }

        /// <summary>
        /// A mouse key was let go on the control.
        /// </summary>
        /// <param name="key">The key which was let go.</param>
        public virtual void MouseUp(MouseKeys key)
        {
        }

        /// <summary>
        /// The mouse moved within the control.
        /// </summary>
        /// <param name="oldPosition">The old position of the mouse.</param>
        /// <param name="newPosition">The new position of the mouse.</param>
        public virtual void MouseMoved(Vector2 oldPosition, Vector2 newPosition)
        {
        }

        /// <summary>
        /// The control was activated. This happens when Active is set to true, and it was false previously.
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        /// The control was deactivated. This happens when Active is set to false, and it was true previously.
        /// </summary>
        public virtual void OnDeactivate()
        {
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Convert the control to a string.
        /// </summary>
        /// <returns>The control as a string.</returns>
        public override string ToString()
        {
            return $"[Transform:{base.ToString()} Active:{Active} MouseInside:{MouseInside} Destroyed:{Destroyed}]";
        }

        #endregion

        #region System

        /// <summary>
        /// Is called by the UI controller to attach itself. Calls the Init user code.
        /// </summary>
        internal void Build(Controller controller)
        {
            _controller = controller;
            Init();
        }

        /// <summary>
        /// Returns the true position of the control. This function is a temporary(tm) workaround.
        /// </summary>
        /// <returns></returns>
        public Rectangle GetTruePosition()
        {
            // Calculate actual relative position.
            // todo: This should be fixed through propagation, see #31
            Rectangle controlBounds = ToRectangle();
            Control parent = Parent;
            while (parent != null)
            {
                controlBounds.X += parent.X;
                controlBounds.Y += parent.Y;
                parent = parent.Parent;
            }

            return controlBounds;
        }

        #endregion
    }
}
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics;
using Emotion.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public abstract class Control : Transform
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
                if (_controller == null) throw new Exception("A UI control cannot use the UIController before the Init function is called.");

                return _controller;
            }
            internal set => _controller = value;
        }

        private Controller _controller;

        /// <summary>
        /// Whether the control is destroyed.
        /// </summary>
        public bool Destroyed { get; protected set; }

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

        protected Control(Vector3 position, Vector2 size) : base(position, size)
        {

        }

        /// <summary>
        /// Is called by the UI controller when initializing the control. Perform initialization connected with the controller
        /// here.
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Is called by the UI controller when destroying the control. Perform cleanup here.
        /// </summary>
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

        public virtual void OnActivate()
        {
        }

        public virtual void OnDeactivate()
        {
        }

        #endregion

        #region Debugging

        public override string ToString()
        {
            return $"[Transform:{base.ToString()} Active:{Active} MouseInside:{MouseInside} Destroyed:{Destroyed}]";
        }

        #endregion
    }
}
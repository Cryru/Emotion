using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A helper UI component used to detect mouse input on the object like clicks, mouse overing etc.
    /// </summary>
    class MouseInput : Component
    {
        #region "Declarations"
        /// <summary>
        /// The current interaction between the mouse and the component.
        /// </summary>
        public Enums.MouseInputStatus Status
        {
            get
            {
                return status;
            }
        }
        #region "Private"
        private Enums.MouseInputStatus status = Enums.MouseInputStatus.None;
        #endregion
        #region "Events"
        /// <summary>
        /// Triggered when the mouse leaves the object's bounds.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> OnMouseLeave;
        /// <summary>
        /// Triggered when the mouse enters the object's bounds.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> OnMouseEnter;
        /// <summary>
        /// Triggered when the object is clicked.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> OnClicked;
        /// <summary>
        /// Triggered when the object is no longer clicked.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> OnLetGo;
        /// <summary>
        /// Triggered when the anything other than the object is clicked.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> OnClickOutside;
        /// <summary>
        /// Triggered when mouse moves inside the object's bounds.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> OnMouseMove;
        /// <summary>
        /// Triggered when mouse's scrolls inside the object.
        /// </summary>
        public event EventHandler<MouseScrollEventArgs> OnMouseScroll;
        #endregion
        #endregion

        public override void Initialize()
        {
            //Check if object we are attaching to is on the UI layer.
            if (attachedObject.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");
            Input.OnMouseMove += Input_OnMouseMove;
            Input.OnMouseButtonDown += Input_OnMouseButtonDown;
            Input.OnMouseButtonUp += Input_OnMouseButtonUp;
            Input.OnMouseScroll += Input_OnMouseScroll;
        }

        #region "Event Handlers"
        /// <summary>
        /// Check if the mouse wheel was scrolled on the object.
        /// </summary>
        private void Input_OnMouseScroll(object sender, MouseScrollEventArgs e)
        {
            bool isIn = inObject(Input.getMousePos());

            if (isIn) OnMouseScroll?.Invoke(attachedObject, e);
        }

        /// <summary>
        /// Check if any of the mouse's buttons were let go on top of the object..
        /// </summary>
        private void Input_OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool isIn = inObject(Input.getMousePos());

            if (isIn)
            {
                status = Enums.MouseInputStatus.MouseOvered;
                OnLetGo?.Invoke(attachedObject, e);
            }
        }

        /// <summary>
        /// Check if the object was clicked by any of the mouse's buttons.
        /// </summary>
        private void Input_OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool isIn = inObject(Input.getMousePos());

            if (isIn)
            {
                status = Enums.MouseInputStatus.Clicked;
                OnClicked?.Invoke(attachedObject, e);
            }
            else
            {
                OnClickOutside?.Invoke(attachedObject, e);
            }
        }

        /// <summary>
        /// Check if the mouse moved inside the object, entered or left it.
        /// </summary>
        private void Input_OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            bool wasIn = inObject(e.From);
            bool isIn = inObject(e.To);

            //Check if it was in but now isn't, in which case it left.
            if (wasIn && !isIn)
            {
                OnMouseLeave?.Invoke(attachedObject, e);
                status = Enums.MouseInputStatus.None;
            }
            //Check if it wasn't in but now is, in which case it entered.
            else if (!wasIn && isIn)
            {
                OnMouseEnter?.Invoke(attachedObject, e);
                status = Enums.MouseInputStatus.MouseOvered;
            }
            //Check if it was in and still is, in which case it moved inside the object.
            else if (wasIn && isIn) OnMouseMove?.Invoke(attachedObject, e);
        }
        #endregion

        #region "Functions"
        /// <summary>
        /// Check if the provided position is within the object.
        /// </summary>
        /// <param name="Position">The position to check.</param>
        /// <returns>True if inside, false if not.</returns>
        private bool inObject(Vector2 Position)
        {
            bool inObject = attachedObject.Bounds.Intersects(Position);

            //Check if the mouse is within the bounds of the object.
            if (!inObject) return false;

            //Get the bounds of all other UI objects.  
            List<GameObject> objects = Context.Core.Scene.AttachedObjects.Select(x => x.Value)
                .Where(x => x.Layer == Enums.ObjectLayer.UI)
                .OrderByDescending(x => x.Priority).ToList();

            //Check if any objects are blocking this one.
            for (int i = 0; i < objects.Count; i++)
            {
                //Check if this is us, we don't care about what's below us so break.
                if (objects[i] == attachedObject) break;

                //Check if the mouse intersects with the bounds of the object.
                if (objects[i].Bounds.Intersects(Position)) return false;
            }

            return true;
        }
        #endregion

        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Detach from events.
                    Input.OnMouseMove -= Input_OnMouseMove;
                    Input.OnMouseButtonDown -= Input_OnMouseButtonDown;
                    Input.OnMouseButtonUp -= Input_OnMouseButtonUp;
                    Input.OnMouseScroll -= Input_OnMouseScroll;
                }

                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
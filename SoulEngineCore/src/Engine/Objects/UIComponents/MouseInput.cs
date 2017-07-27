using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;
using SoulEngine.Modules;

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

        public override void Initialize()
        {
            //Check if object we are attaching to is on the UI layer.
            if (attachedObject.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");
            InputModule.OnMouseMove += Input_OnMouseMove;
            InputModule.OnMouseButtonDown += Input_OnMouseButtonDown;
            InputModule.OnMouseButtonUp += Input_OnMouseButtonUp;
            InputModule.OnMouseScroll += Input_OnMouseScroll;
        }

        #region "Event Handlers"
        /// <summary>
        /// Check if the mouse wheel was scrolled on the object.
        /// </summary>
        private void Input_OnMouseScroll(object sender, MouseScrollEventArgs e)
        {
            if (!attachedObject.Drawing || !Functions.CheckOpacity(attachedObject)) return;

            bool isIn = Functions.inObject(InputModule.getMousePos(), attachedObject.Bounds, attachedObject.Priority);

            if (isIn) OnMouseScroll?.Invoke(attachedObject, e);
        }

        /// <summary>
        /// Check if any of the mouse's buttons were let go on top of the object..
        /// </summary>
        private void Input_OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!attachedObject.Drawing || !Functions.CheckOpacity(attachedObject)) return;

            bool isIn = Functions.inObject(InputModule.getMousePos(), attachedObject.Bounds, attachedObject.Priority);

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
            if (!attachedObject.Drawing || !Functions.CheckOpacity(attachedObject)) return;

            bool isIn = Functions.inObject(InputModule.getMousePos(), attachedObject.Bounds, attachedObject.Priority);

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
            if (!attachedObject.Drawing || !Functions.CheckOpacity(attachedObject)) return;

            bool wasIn = Functions.inObject(e.From, attachedObject.Bounds, attachedObject.Priority);
            bool isIn = Functions.inObject(e.To, attachedObject.Bounds, attachedObject.Priority);

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
                    InputModule.OnMouseMove -= Input_OnMouseMove;
                    InputModule.OnMouseButtonDown -= Input_OnMouseButtonDown;
                    InputModule.OnMouseButtonUp -= Input_OnMouseButtonUp;
                    InputModule.OnMouseScroll -= Input_OnMouseScroll;
                }

                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
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
        private Enums.MouseInputStatus lastTickStatus = Enums.MouseInputStatus.None;
        private Vector2 position;
        private Vector2 lastTickPosition;
        private int scrollPosition;
        private int lastscrollPosition;
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
        /// Triggered when mouse moves inside the object's bounds.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> OnMouseMove;
        /// <summary>
        /// Triggered when mouse's scrolls inside the object.
        /// </summary>
        public event EventHandler<MouseScrollEventArgs> OnMouseScroll;
        #endregion
        #endregion

        public override Component Initialize()
        {
            //Check if object we are attaching to is on the UI layer.
            if (attachedObject.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");
            Input.OnMouseMove += Input_OnMouseMove;
            Input.OnMouseButtonDown += Input_OnMouseButtonDown;

            return this;
        }

        /// <summary>
        /// Check if the object was clicked by any of the mouse's buttons.
        /// </summary>
        private void Input_OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
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
                OnMouseLeave.Invoke(attachedObject, e);
                status = Enums.MouseInputStatus.None;
            }
            //Check if it wasn't in but now is, in which case it entered.
            else if (!wasIn && isIn) 
            {
                OnMouseEnter.Invoke(attachedObject, e);
                status = Enums.MouseInputStatus.MouseOvered;
            }
            //Check if it was in and still is, in which case it moved inside the object.
            else if (wasIn && isIn) OnMouseMove.Invoke(attachedObject, e);
        }

        

        #region "Functions"
        public override void Update()
        {
            lastTickStatus = status;
            status = UpdateStatus();

            if (lastTickStatus == Enums.MouseInputStatus.MouseOvered &&
                status == Enums.MouseInputStatus.Clicked)
                OnClicked?.Invoke(attachedObject, EventArgs.Empty);

            if (lastTickStatus == Enums.MouseInputStatus.Clicked &&
                status == Enums.MouseInputStatus.MouseOvered)
                OnLetGo?.Invoke(attachedObject, EventArgs.Empty);

            //Check for moving.
            position = Input.getMousePos();

            if(Status != Enums.MouseInputStatus.None)
            {
                if(lastTickPosition != null && position != lastTickPosition)
                {
                    OnMouseMove?.Invoke(attachedObject, new MouseMoveEventArgs(lastTickPosition, position));
                }
            }

            lastTickPosition = position;

            //Check for scrolling.
            scrollPosition = Input.currentFrameMouseState.ScrollWheelValue;

            if (Status != Enums.MouseInputStatus.None)
            {
                int scrollDif = lastscrollPosition - scrollPosition;

                OnMouseScroll?.Invoke(attachedObject, new MouseScrollEventArgs(scrollDif));
            }

            lastscrollPosition = scrollPosition;

        }
        private Enums.MouseInputStatus UpdateStatus()
        {
            //Get the bounds of my object.
            if (attachedObject.Layer != Enums.ObjectLayer.UI) return Enums.MouseInputStatus.None;

            Rectangle objectBounds = attachedObject.Bounds;

            //Get the location of the mouse.
            Vector2 mouseLoc = Input.getMousePos();

            //Check if within object bounds.
            bool inObject = objectBounds.Intersects(mouseLoc);

            if (!inObject) return Enums.MouseInputStatus.None;

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
                if (objects[i].Bounds.Intersects(mouseLoc)) return Enums.MouseInputStatus.None;
            }

            //Check if mouse is clicked now that we have determined the focus is on us.
            if (Input.isLeftClickDown()) return Enums.MouseInputStatus.Clicked; else return Enums.MouseInputStatus.MouseOvered;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
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
                //if (objects[i].Bounds.Intersects(mouseLoc)) return Enums.MouseInputStatus.None;
            }

            //PH
            return false;
        }
        #endregion
    }
}
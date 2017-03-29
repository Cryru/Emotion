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
    /// used to detect mouse input on the object like clicks, mouse overing etc.
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
        #endregion
        #endregion

        #region "Functions"
        public override void Update()
        {
            lastTickStatus = status;
            status = UpdateStatus();

            //Process events.
            if (lastTickStatus == Enums.MouseInputStatus.MouseOvered &&
                status == Enums.MouseInputStatus.None)
                ESystem.Add(new Event(EType.MOUSEINPUT_LEFT, attachedObject));

            if (lastTickStatus == Enums.MouseInputStatus.None &&
                 status == Enums.MouseInputStatus.MouseOvered)
                ESystem.Add(new Event(EType.MOUSEINPUT_ENTERED, attachedObject));

            if (lastTickStatus == Enums.MouseInputStatus.MouseOvered &&
                status == Enums.MouseInputStatus.Clicked)
                ESystem.Add(new Event(EType.MOUSEINPUT_CLICKDOWN, attachedObject));

            if (lastTickStatus == Enums.MouseInputStatus.Clicked &&
                status == Enums.MouseInputStatus.MouseOvered)
                ESystem.Add(new Event(EType.MOUSEINPUT_CLICKUP, attachedObject));

        }
        private Enums.MouseInputStatus UpdateStatus()
        {
            //Get the bounds of my object.
            if (!attachedObject.HasComponent<Transform>() || attachedObject.Layer != Enums.ObjectLayer.UI) return Enums.MouseInputStatus.None;

            Rectangle objectBounds = attachedObject.GetProperty("Bounds", new Rectangle());

            //Get the location of the mouse.
            Vector2 mouseLoc = Input.getMousePos();

            //Check if within object bounds.
            bool inObject = objectBounds.Intersects(mouseLoc);

            if (!inObject) return Enums.MouseInputStatus.None;

            //Get the bounds of all other UI objects.
            List<GameObject> objects = Context.Core.Scene.AttachedObjects
                .Where(x => x.Layer == Enums.ObjectLayer.UI)
                .Where(x => x.HasComponent<Transform>() == true)
                .OrderBy(x => x.Priority).ToList();

            //Check if any objects are blocking this one.
            for (int i = 0; i < objects.Count; i++)
            {
                //Check if this is us, we don't care about what's below us so break.
                if (objects[i] == attachedObject) break;

                //Check if the mouse intersects with the bounds of the object.
                if (objects[i].Component<Transform>().Bounds.Intersects(mouseLoc)) return Enums.MouseInputStatus.None;
            }

            //Check if mouse is clicked now that we have determined the focus is on us.
            if (Input.isLeftClickDown()) return Enums.MouseInputStatus.MouseOvered; else return Enums.MouseInputStatus.Clicked;
        }
        #endregion

        #region "Component Interface"
        public override void Draw()
        {

        }
        public override void Compose()
        {

        }
        #endregion
    }
}
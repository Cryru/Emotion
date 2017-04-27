using Microsoft.Xna.Framework;
using SoulEngine.Objects.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;

namespace SoulEngine.Objects
{
    public class UIObject : GameObject
    {
        #region "Declarations"
        /// <summary>
        /// Children objects of this object.
        /// </summary>
        public Dictionary<string, GameObject> Children = new Dictionary<string, GameObject>();

        /// <summary>
        /// Override the layer property so the object is always on the UI layer.
        /// </summary>
        public override ObjectLayer Layer
        {
            get
            {
                return ObjectLayer.UI;
            }
        }
        #endregion

        public override void Draw()
        {
            Children.OrderBy(x => x.Value.Priority);

            //Offset the drawing of all children objects by the location of the parent.
            foreach (GameObject child in Children.Select(x => x.Value))
            {
                //Store the position of the child.
                int PositionX = child.X;
                int PositionY = child.Y;

                //Set the position to the one offset by the parent.
                child.X += X;
                child.Y += Y;

                //Draw the object.
                child.Draw();

                //Restore old position.
                child.X = PositionX;
                child.Y = PositionY;
            }
        }

        #region "Helper Functions"
        public Vector2 ActualPositionToChild(Vector2 ActualPosition)
        {
            if (ActualPosition.X >= X) ActualPosition.X -= X; else ActualPosition.X += X;
            if (ActualPosition.Y >= Y) ActualPosition.Y -= Y; else ActualPosition.Y += Y;

            return ActualPosition;
        }
        #endregion

        #region "Templates"
        /// <summary>
        /// A UI horizontal scroll bar. The value of it is under the Scrollbar component.
        /// </summary>
        public static UIObject Scrollbar()
        {
            UIObject Object = new UIObject();
            Object.AddComponent(new Scrollbar());

            //Some UI components have to initialized separately.
            Object.Component<Scrollbar>().Initialize();

            return Object;
        }
        #endregion
    }
}

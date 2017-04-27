using Microsoft.Xna.Framework;
using SoulEngine.Objects.Components;
using SoulEngine.Objects.Components.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    public class UIObject : GameObject
    {
        /// <summary>
        /// Children objects of this object.
        /// </summary>
        public Dictionary<string, GameObject> Children = new Dictionary<string, GameObject>();

        public override void Draw()
        {

            //Offset the drawing of all children objects by the location of the parent.
            foreach (GameObject child in Children.Select(x => x.Value))
            {
                //Store the position of the child.
                Vector2 Position = child.Position;

                //Set the position to the one offset by the parent.
                child.X += X;
                child.Y += Y;

                //Draw the object.
                child.Draw();

                //Restore old position.
                child.Position = Position;
            }
        }

        #region "Templates"
        public static UIObject Scrollbar()
        {
            UIObject Object = new UIObject();

            Object.Layer = Enums.ObjectLayer.UI;
            Object.AddComponent(new MouseInput());
            Object.AddComponent(new Scrollbar());

            return Object;
        }
#endregion
    }
}

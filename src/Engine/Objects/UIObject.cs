using Microsoft.Xna.Framework;
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
        public Dictionary<string, GameObject> Children;

        public UIObject(GameObject Origin)
        {
            base = Origin;
        }

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
    }
}

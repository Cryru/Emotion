using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // The base for the screen object.                                          //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class ScreenObjectBase
    {
        //Variables
        public bool Active = false; //Whether this is the currently active screen.

        //Events
        public Action onCreate; //Is run when the screen is created.

        //Is run when the screen is selected.
        public void Activate()
        {
            Core.ScreenUpdate = Update; //Assign the update loop to the screen's.
            Core.ScreenDraw = Draw; //Assign the drawing loop to the screen's.
            LoadObjects(); //Load the screen's objects.
            onCreate?.Invoke(); //Run the on creation event.
            Active = true; //Flip the active variable.
        }

        #region "Overriden Methods"
        public virtual void LoadObjects()
        {

        }
        public virtual void Update(GameTime gameTime)
        {

        }
        public virtual void Draw(GameTime gameTime)
        {

        }
        #endregion

    }
}

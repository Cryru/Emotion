using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // The master screen is drawn on top of the curent screen.                  //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class MasterScreen : ScreenObjectBase
    {
        public override void LoadObjects()
        {

        }
        public override void Update(GameTime gameTime)
        {

        }
        public override void Draw(GameTime gameTime)
        {
            Core.DrawScreen(); //Screen Cycle Start.
            Core.ink.End(); //Screen Cycle End.
            Core.DrawWorld(); //World Cycle Start.
            Core.ink.End(); //World Cycle End.
        }
    }
}

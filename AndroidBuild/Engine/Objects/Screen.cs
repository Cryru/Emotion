using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Collections.Generic;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // A screen object template for copying.                                    //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class Screen : ScreenObjectBase
    {

        public override void LoadObjects()
        {


        }
        public override void Update(GameTime gameTime)
        {

   
        }
        public override void Draw(GameTime gameTime)
        {
            Core.DrawWorld(); //World Cycle Start.
            Core.ink.End(); //World Cycle End.
            Core.DrawScreen(); //Screen Cycle Start.
            Core.ink.End(); //Screen Cycle End.
        }
    }
}

using Microsoft.Xna.Framework;
using SoulEngine.Objects;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // A screen object template for copying.                                    //
    // Screens are what other engines and frameworks refer to as scenes.        //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class Screen : ScreenObjectBase
    {
        internal int Priority;

        //This is run when the screen is loaded.
        public override void LoadObjects()
        {


        }
        //This is run every frame on the CPU.
        public override void Update(GameTime gameTime)
        {

   
        }
        //This is run every frame on the GPU.
        public override void Draw(GameTime gameTime)
        {
            Core.DrawWorld(); //World Cycle Start.
            Core.ink.End(); //World Cycle End.
            Core.DrawScreen(); //Screen Cycle Start.
            Core.ink.End(); //Screen Cycle End.
        }
    }
}

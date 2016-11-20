using Microsoft.Xna.Framework;
using SoulEngine.Physics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Physics
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    public static class PhysicsEngine
    {
        public static World world;


        public static Vector2 Gravity
        {
            set
            {
                //Set the world's gravity, if a world is initialized.
                if(world != null) world.Gravity = value;
                //Set the variable.
                _gravity = value;
            }
            get
            {
                return _gravity;
            }
        }
        
        private static Vector2 _gravity = new Vector2(0.0f, -10.0f);

        public static float transX;// = 320.0f;
        public static float transY;// = 240.0f;
        public static float scaleFactor;// = 10.0f;
        public static float yFlip;// = -1.0f; //flip y coordinate

        public static Body groundBody;


        public static void CreateWorld()
        {
            world = new World(Gravity);

        }



        #region "Helper Functions"
        public static void WorldToPixel(float x, float y)
        {

        }
        public static void WorldToPixel(Vector2 location)
        {

        }

        public static void PixelToWorld(float x, float y)
        {

        }
        public static void PixelToWorld(Vector2 location)
        {

        }
        #endregion

    }
}

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
    public static class Engine
    {
        public static World world;
        /// <summary>
        /// The ratio of display units to simulation units.
        /// </summary>
        public static float Scale = 10f;
        /// <summary>
        /// The ratio of simulation units to display units.
        /// </summary>
        public static float ScaleReverse
        {
            get
            {
                return 1 / Scale;
            }
        }

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
        
        private static Vector2 _gravity = new Vector2(0.0f, 20.0f);

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
        public static float PhysicsToPixel(float num)
        {
            return num * Scale;
        }
        public static Vector2 PhysicsToPixel(Vector2 vec)
        {
            return vec * Scale;
        }
        public static float PixelToPhysics(float num)
        {
            return num * ScaleReverse;
        }
        public static Vector2 PixelToPhysics(Vector2 vec)
        {
            return vec * ScaleReverse;
        }
        #endregion

    }
}

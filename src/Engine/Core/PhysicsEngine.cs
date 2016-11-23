using Microsoft.Xna.Framework;
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
    /// <summary>
    /// Handles physics.
    /// </summary>
    public static class Engine
    {
        #region "Declarations"
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
        #endregion


        /// <summary>
        /// Calculate physics for all screens.
        /// </summary>
        public static void Update()
        {
            if (Core.gameTime == null) return;

            //Get the list of screens active.
            for (int i = 0; i < Core.Screens.Count; i++)
            {
                //Advance simulation by the amount of time that has passed since the last update.
                Core.Screens[i].PhysicsWorld.Step((float)Core.gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        #region "Helper Functions"
        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static float PhysicsToPixel(float num)
        {
            return num * Scale;
        }
        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static Vector2 PhysicsToPixel(Vector2 vec)
        {
            return vec * Scale;
        }
        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static float PixelToPhysics(float num)
        {
            return num * ScaleReverse;
        }
        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static Vector2 PixelToPhysics(Vector2 vec)
        {
            return vec * ScaleReverse;
        }
        #endregion

    }
}

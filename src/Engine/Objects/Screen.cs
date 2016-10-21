using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A screen object.
    /// Screens are used to control what objects are loaded and drawn on the scren at the time.
    /// The order in which the screens will be drawn is determined by the "Priority" variable.
    /// Screens are the SoulEngine concept of what most engines and frameworks refer to as "scenes".
    /// All screens created by the user should be children of this object.
    /// </summary>
    public abstract class Screen
    {
        #region "Declarations"
        /// <summary>
        /// The screens priority. The higher it is the earlier it will be executed.
        /// </summary>
        internal int Priority = 0;
        #endregion

        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public abstract void LoadObjects();
        /// <summary>
        /// Is run every frame on the CPU.
        /// Game logic and other stuff go here.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);
        /// <summary>
        /// Is run every frame on the GPU.
        /// Your draw calls go here.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Draw(GameTime gameTime);

    }
}

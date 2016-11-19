using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;

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
    /// A basis for objects affected by physics.
    /// IN TESTING
    /// </summary>
    public class PhysicsObject : ObjectBase
    {
        #region "Declarations"
      
        #endregion

        /// <summary>
        /// Initializes an object.
        /// </summary>
        /// <param name="Image">The texture object that represents the object.</param>
        public PhysicsObject(Texture Image = null) : base(Image)
        {
           
        }
    }
}

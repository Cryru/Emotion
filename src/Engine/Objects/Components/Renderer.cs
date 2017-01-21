using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Hosts and handles textures and drawing.
    /// </summary>
    public class Renderer : Component
    {
        #region "Variables"
        //Main variables.
        #region "Primary"
        /// <summary>
        /// 
        /// </summary>
        ActiveTexture Texture { get; set; }
        #endregion
        //Private variables.
        #region "Private"

        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public Renderer(ActiveTexture Texture)
        {
            this.Texture = Texture;
        }
        #endregion

        //Main functions.
        #region "Functions"

        #endregion
        //Private functions.
        #region "Internal Functions"

        #endregion
    }
}

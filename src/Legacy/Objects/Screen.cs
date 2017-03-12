using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;

namespace SoulEngine.Legacy.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public abstract class Screen : Scene
    {
        #region "Declarations"
        internal int Priority = 0;
        internal bool ObjectsLoaded = false;
        #endregion

        public void LoadObjects()
        {
            SetupScene();
            ObjectsLoaded = true;
        }
    }
}

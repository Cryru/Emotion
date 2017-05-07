using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Events;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public class ScenePrim : Scene
    {
        #region "Declarations"
        #endregion

        public override void Start()
        {
            Context.Core.LoadScene(new StressTest());
           

        }

        public override void Update()
        {

        }
    }
}

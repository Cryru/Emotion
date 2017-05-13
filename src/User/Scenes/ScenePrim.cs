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

            GameObject animtest = new GameObject();
            animtest.AddComponent(new ActiveTexture(AssetManager.MissingTexture, new Vector2(1, 1)));
            animtest.AddComponent(new Animation(0, -1, Enums.LoopType.Normal, 1));

            AddObject("anim", animtest);

        }

        public override void Update()
        {

        }
    }
}

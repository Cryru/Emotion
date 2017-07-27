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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulEngine.Modules;
using SoulEngine.Enums;

namespace SoulEngine.Scenes
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public class Loading : Scene
    {
        #region "Declarations"
        Texture2D LoadingTexture;
        #endregion

        public override void Start()
        {
            LoadingTexture = Context.Core.Content.Load<Texture2D>("Engine/loadingscreen");
        }

        public override void Update()
        {

        }

        public override void DrawHook()
        {
            Context.ink.Start(DrawMatrix.Screen);
            Context.ink.Draw(LoadingTexture, new Rectangle(0, 0, Settings.Width, Settings.Height), Color.White);
            Context.ink.End();
        }
    }
}

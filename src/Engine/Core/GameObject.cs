using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The base for engine objects.
    /// </summary>
    public class GameObject
    {
        #region "Variables"
        /// <summary>
        /// 
        /// </summary>
        private List<Components.Component> Components = new List<Components.Component>();





        ////Display properties of the object.
        //#region "Display"
        ///// <summary>
        ///// 
        ///// </summary>
        //Internal.ActiveTexture Texture { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //float Opacity = 1f;

        ///// <summary>
        ///// 
        ///// </summary>
        //Color Tint = Color.White;

        ///// <summary>
        ///// 
        ///// </summary>
        //public SpriteEffects MirrorEffects = SpriteEffects.None;
        //#endregion

        ////Events of the object.
        //#region "Triggers"
        ///// <summary>
        ///// 
        ///// </summary>
        //Internal.Trigger onUpdate = new Internal.Trigger();
        ///// <summary>
        ///// 
        ///// </summary>
        //Internal.Trigger onDraw = new Internal.Trigger();
        //#endregion

        ////Ways for the object to link with parental objects.
        //#region "Hooks"
        ///// <summary>
        ///// 
        ///// </summary>
        //Main.Scene Scene { get; set; }
        //#endregion

        ////Other
        //#region "Others"
        ///// <summary>
        ///// Tags used to store information within the object.
        ///// </summary>
        //Dictionary<string, string> Tags = new Dictionary<string, string>();
        //#endregion
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Draw()
        {

        }

        #region "Component System"
        public void AddComponent(Components.Component Component)
        {
            //Check if the component's dependencies are attached.
            if (Component.CheckDependencies(this)) throw new Exception("ERROR C01: Failed adding component " + Component.GetType() + " due to missing dependencies.");
            Components.Add(Component);
        }
        #endregion
    }
}

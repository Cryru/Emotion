using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;
using SoulEngine.Events;
using SoulEngine.Objects.Components.Helpers;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A managed texture object.
    /// </summary>
    public class ActiveTexture : Component
    {

        #region "Declarations"
        /// <summary>
        /// Returns the active texture's texture.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture()
        {
            if (TextureMode == TextureMode.Animate && attachedObject.HasComponent<Animation>()) return attachedObject.Component<Animation>().Frames[attachedObject.Component<Animation>().FrameTotal];
            if (TextureMode == TextureMode.Stretch) return Texture;
            if (_texture == null) return AssetManager.MissingTexture;
            return _texture as Texture2D;
        }

        /// <summary>
        /// Sets the active texture's texture.
        /// </summary>
        /// <param name="value"></param>
        public void SetTexture(Texture2D value)
        {
            Texture = value;
        }

        /// <summary>
        /// The way to texture should be rendered to fill its bounds.
        /// </summary>
        public TextureMode TextureMode = 0;
        #region "Private"
        private RenderTarget2D _texture;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextureMode"></param>
        public ActiveTexture(TextureMode TextureMode = 0)
        {
            this.TextureMode = TextureMode;
            this.SetTexture(AssetManager.MissingTexture);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        /// <param name="TextureMode"></param>
        /// <param name="DrawArea"></param>
        public ActiveTexture(Texture2D Texture, TextureMode TextureMode = 0)
        {
            this.TextureMode = TextureMode;
            this.SetTexture(Texture);
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        public override void Compose()
        {
            if (TextureMode != TextureMode.Tile) return;

            //Get the size of the object, or if a Transform component is attached get the bounds from it.
            Rectangle Bounds = attachedObject.Bounds;

            //Start drawing on internal target.
            Context.ink.StartRenderTarget(ref _texture, Bounds.Width, Bounds.Height);

            //Draw the texture depending on how we are stretching.
            switch (TextureMode)
            {
                case TextureMode.Tile:
                    //Calculate the limit of the texture tile, we go higher as the rendertarget will not allow out of bounds drawing anyway.
                    int xLimit = (int)Math.Ceiling((double)Bounds.Width / GetTexture().Width);
                    int yLimit = (int)Math.Ceiling((double)Bounds.Height / GetTexture().Height);

                    for (int x = 0; x < xLimit; x++)
                    {
                        for (int y = 0; y < yLimit; y++)
                        {
                            Context.ink.Draw(GetTexture(), new Rectangle(GetTexture().Width * x, GetTexture().Height * y,
                                GetTexture().Width, GetTexture().Height), null, Color.White);
                        }
                    }
                    break;
            }

            //Stop drawing.
            Context.ink.EndRenderTarget();
        }
        #endregion

        #region "Component Interface"
        public override void Update() { }
        #endregion

        //Other
        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //Free resources.
                _texture = null;
                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}

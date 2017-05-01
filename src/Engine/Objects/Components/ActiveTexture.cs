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
    public class ActiveTexture : DrawComponent
    {

        #region "Declarations"
        public override Texture2D Texture
        {
            get
            {
                if (TextureMode == TextureMode.Animate && attachedObject.HasComponent<Animation>()) return attachedObject.Component<Animation>().Frames[attachedObject.Component<Animation>().FrameTotal];
                if (TextureMode == TextureMode.Stretch) return _xnatexture;
                if (_texture == null && _xnatexture == null) return AssetManager.MissingTexture;
                if (TextureMode == TextureMode.Tile) return _texture as Texture2D;
                return _xnatexture;
            }
            set
            {
                _xnatexture = value;
            }
        }

        /// <summary>
        /// The way to texture should be rendered to fill its bounds.
        /// </summary>
        public TextureMode TextureMode = 0;
        #region "Private"
        private RenderTarget2D _texture;
        private Texture2D _xnatexture;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public override Component Initialize()
        {
            TextureMode = TextureMode.Stretch;
            Texture = AssetManager.MissingTexture;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextureMode"></param>
        public Component Initialize(TextureMode TextureMode)
        {
            this.TextureMode = TextureMode;
            Texture = AssetManager.MissingTexture;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        /// <param name="TextureMode"></param>
        public Component Initialize(TextureMode TextureMode, Texture2D Texture)
        {
            this.TextureMode = TextureMode;
            this.Texture = Texture;

            return this;
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
                    int xLimit = (int)Math.Ceiling((double)Bounds.Width / Texture.Width);
                    int yLimit = (int)Math.Ceiling((double)Bounds.Height / Texture.Height);

                    for (int x = 0; x < xLimit; x++)
                    {
                        for (int y = 0; y < yLimit; y++)
                        {
                            Context.ink.Draw(Texture, new Rectangle(Texture.Width * x, Texture.Height * y,
                                Texture.Width, Texture.Height), null, Color.White);
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

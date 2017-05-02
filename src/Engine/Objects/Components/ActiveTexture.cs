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
    /// A component for holding textures and managing how they are drawn.
    /// </summary>
    public class ActiveTexture : DrawComponent
    {

        #region "Declarations"
        /// <summary>
        /// The ActiveTexture's processed texture.
        /// </summary>
        public override Texture2D Texture
        {
            get
            {
                //Texture mode animate requires an animation component.
                if (TextureMode == TextureMode.Animate && attachedObject.HasComponent<Animation>())
                    return attachedObject.Component<Animation>().FrameTexture;
                if (TextureMode == TextureMode.Tile && _texture != null) return _texture as Texture2D;
                if (ActualTexture == null) return AssetManager.MissingTexture; else return ActualTexture;
            }
            set
            {
                //Set the internal texture to the one provided.
                ActualTexture = value;
                //Clear the compose buffer to force a recomposing.
                if(_texture != null) _texture.Dispose();
                _texture = null;

                //Trigger texture changing event.
                OnTextureChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The way to texture should be rendered.
        /// </summary>
        public TextureMode TextureMode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                //Trigger texture mode change event.
                OnTextureModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// The internal texture.
        /// </summary>
        public Texture2D ActualTexture;

        #region "Private"
        /// <summary>
        /// The buffer we compose textures to.
        /// </summary>
        private RenderTarget2D _texture;
        /// <summary>
        /// The way the texture should be rendered.
        /// </summary>
        private TextureMode _mode = 0;
        #endregion
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the texture is set from outside.
        /// </summary>
        public event EventHandler<EventArgs> OnTextureChanged;
        /// <summary>
        /// Triggered when the texture mode is changed.
        /// </summary>
        public event EventHandler<EventArgs> OnTextureModeChanged;
        #endregion

        #region "Initialization"
        /// <summary>
        /// Create a new ActiveTexture object.
        /// </summary>
        /// <param name="TextureMode">The texture drawing mode.</param>
        public ActiveTexture(TextureMode TextureMode)
        {
            this.TextureMode = TextureMode;
            Texture = AssetManager.MissingTexture;
        }
        /// <summary>
        /// Create a new ActiveTexture object with a specified Texture.
        /// </summary>
        /// <param name="Texture">The texture to draw.</param>
        /// <param name="TextureMode">The texture drawing mode.</param>
        public ActiveTexture(TextureMode TextureMode, Texture2D Texture)
        {
            this.TextureMode = TextureMode;
            this.Texture = Texture;
        }
        #endregion

        #region "Functions"
        /// <summary>
        /// Compose the ActiveTexture's texture based on its TextureMode.
        /// </summary>
        public override void Compose()
        {
            //Check if the texture mode is set to tile, and a texture hasn't been composed.
            if (TextureMode == TextureMode.Tile && _texture == null) ComposeTile();
        }

        /// <summary>
        /// Composes a tiled image of the provided texture.
        /// </summary>
        public void ComposeTile()
        {
            //Get the size of the object, or if a Transform component is attached get the bounds from it.
            Rectangle Bounds = attachedObject.Bounds;

            //Start drawing on internal target.
            Context.ink.StartRenderTarget(ref _texture, Bounds.Width, Bounds.Height);

            //Draw the texture depending on how we are stretching.
            switch (TextureMode)
            {
                case TextureMode.Tile:
                    //Calculate the limit of the texture tile, we go higher as the rendertarget will not allow out of bounds drawing anyway.
                    int xLimit = (int)Math.Ceiling((double)Bounds.Width / ActualTexture.Width);
                    int yLimit = (int)Math.Ceiling((double)Bounds.Height / ActualTexture.Height);

                    for (int x = 0; x < xLimit; x++)
                    {
                        for (int y = 0; y < yLimit; y++)
                        {
                            Context.ink.Draw(ActualTexture, new Rectangle(ActualTexture.Width * x, ActualTexture.Height * y,
                                ActualTexture.Width, ActualTexture.Height), null, Color.White);
                        }
                    }
                    break;
            }

            //Stop drawing.
            Context.ink.EndRenderTarget();
        }
        #endregion

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
                    //Free resources.
                    if (_texture != null) _texture.Dispose();
                    _texture = null;
                }

                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}

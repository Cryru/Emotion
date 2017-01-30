using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;

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
        #region "Variables"
        /// <summary>
        /// Internal XNA Texture holder.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return _texture as Texture2D;
            }
            set
            {
                _xnaTexture = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                if (value == new Rectangle()) _bounds = _xnaTexture.Bounds;
                else _bounds = value;
            }
        }
        /// <summary>
        /// The way to texture should be rendered to fill its bounds.
        /// </summary>
        public TextureMode TextureMode = 0;
        #region "Variables for the Renderer Component"
        /// <summary>
        /// Used to mirror textures horizontally or vertically. Used by the renderer component.
        /// </summary>
        public SpriteEffects MirrorEffects = SpriteEffects.None;
        /// <summary>
        /// Used to decide the texture opacity. Used by the renderer component.
        /// </summary>
        public float Opacity = 1f;
        /// <summary>
        /// Used to color the texture. Used by the renderer component.
        /// </summary>
        public Color Tint = Color.White;
        #endregion
        #region "Private"
        private RenderTarget2D _texture;
        private Viewport _tempPortHolder;
        private Texture2D _xnaTexture;
        private Rectangle _bounds;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public ActiveTexture(TextureMode TextureMode = 0)
        {
            this.TextureMode = TextureMode;
            Texture = AssetManager.MissingTexture;
            Bounds = _xnaTexture.Bounds;
            DefineTexture();
            GenerateTexture();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public ActiveTexture(Texture2D Texture, TextureMode TextureMode = 0, Rectangle Bounds = new Rectangle())
        {
            this.TextureMode = TextureMode;
            this.Texture = Texture;
            this.Bounds = Bounds;
            DefineTexture();
            GenerateTexture();
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        private void GenerateTexture()
        {
            //Check if regenerating with a differently sized texture, in which case we want to generate a new render target.
            if (_texture == null ||
                Bounds.X != _texture.Bounds.Size.X ||
                Bounds.Y != _texture.Bounds.Size.Y) DefineTexture(Bounds.Size.X, Bounds.Size.Y);

            //Start drawing on internal target.
            BeginTextureDraw();

            Context.ink.Start();

            //Draw the texture depending on how we are stretching.
            switch(TextureMode)
            {
                case TextureMode.Stretch:
                    Context.ink.Draw(_xnaTexture, new Rectangle(0, 0, Bounds.Width, Bounds.Height), Bounds, Color.White);
                    break;

                case TextureMode.Tile:
                    for (int x = 0; x < Bounds.Width / _xnaTexture.Width; x++)
                    {
                        for (int y = 0; y < Bounds.Height / _xnaTexture.Height; y++)
                        {
                            Context.ink.Draw(_xnaTexture, new Rectangle(_xnaTexture.Width * x, _xnaTexture.Height * y, _xnaTexture.Width, _xnaTexture.Height), Bounds, Color.White);
                        }
                    }

                    break;
            }

            Context.ink.End();

            //Stop drawing.
            EndTextureDraw();
        }
        /// <summary>
        /// 
        /// </summary>
        public void BeginTextureDraw()
        {
            //Record the viewport of the current graphics device, as the rendertarget switching resets it.
            _tempPortHolder = Context.Graphics.Viewport;

            //Set the current rendertarget to the drawer.
            Context.Graphics.SetRenderTarget(_texture);

            //Clear the rendertarget.
            Context.Graphics.Clear(Color.Transparent);
        }
        /// <summary>
        /// 
        /// </summary>
        public void EndTextureDraw()
        {
            //Return to the default render target.
            Context.Graphics.SetRenderTarget(null);

            //Return the viewport holder.
            Context.Graphics.Viewport = _tempPortHolder;
        }
        #endregion
        //Private functions.
        #region "Internal Functions"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        private void DefineTexture(int Width = 0, int Height = 0)
        {
            //Destroy previous render target safely, if any.
            if (_texture != null) _texture.Dispose();

            //Generate a new rendertarget with the specified size.
            _texture = new RenderTarget2D(Context.Graphics, Width, Height);
        }
        #endregion

        //Other
        #region "Component Interface"
        public override void Update(){}
        public override void Draw(){}
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
                    base.Dispose();
                }

                //Free resources.
                Texture = null;
                _texture = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

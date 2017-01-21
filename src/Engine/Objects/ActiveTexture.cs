using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A managed texture object.
    /// </summary>
    public class ActiveTexture
    {
        #region "Variables"
        /// <summary>
        /// 
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                return _texture as Texture2D;
            }
        }
        #region "Private"
        private RenderTarget2D _texture;
        private Viewport _tempPortHolder;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public ActiveTexture(Texture2D Texture)
        {
            Generate();
            Regenerate(Texture);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextureName"></param>
        public ActiveTexture(string TextureName)
        {
            Generate();
            Regenerate(TextureName);
        }
        #endregion

        //Main functions.
        #region "Functions"

        #region "Regeneration"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public void Regenerate(Texture2D Texture, Rectangle Bounds = new Rectangle())
        {
            //Check if regenerating with a differently sized texture, in which case we want to generate a new render target.
            if (Bounds.Size.X != _texture.Bounds.Size.X ||
                Bounds.Size.X != _texture.Bounds.Size.Y) Generate(Bounds.Size.X, Bounds.Size.Y);

            //Start drawing on internal target.
            BeginTextureDraw();

            Context.ink.Begin();
            Context.ink.Draw(Texture, new Rectangle(0, 0, Bounds.Width, Bounds.Height), Bounds, Color.White);
            Context.ink.End();

            EndTextureDraw();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextureName"></param>
        public void Regenerate(string TextureName, Rectangle Bounds = new Rectangle())
        {
            //TODO: ASSET LOADING CODE
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Size"></param>
        public void Regenerate(Vector2 Size)
        {
            Generate((int) Size.X, (int) Size.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public void Regenerate(int Width, int Height)
        {
            Regenerate(new Vector2(Width, Height));
        }
        #endregion
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
        private void Generate(int Width = 0, int Height = 0)
        {
            //Destroy previous render target safely, if any.
            if (_texture != null) _texture.Dispose();

            //Generate a new rendertarget with the specified size.
            _texture = new RenderTarget2D(Context.Graphics, Width, Height);
        }
        #endregion

    }
}

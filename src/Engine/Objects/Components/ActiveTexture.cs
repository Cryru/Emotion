using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Redefine(value);
            }
        }
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
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public ActiveTexture()
        {
            Generate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public ActiveTexture(Texture2D Texture, Rectangle Bounds = new Rectangle())
        {
            Generate();
            Redefine(Texture, Bounds);
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        public void Redefine(Texture2D Texture, Rectangle Bounds = new Rectangle())
        {
            //Check if no bounds specified, in which case texture bounds are taken.
            if(Bounds == new Rectangle()) Bounds = Texture.Bounds;

            //Check if regenerating with a differently sized texture, in which case we want to generate a new render target.
            if (Bounds.Size.X != _texture.Bounds.Size.X ||
                Bounds.Size.X != _texture.Bounds.Size.Y) Generate(Bounds.Size.X, Bounds.Size.Y);

            //Start drawing on internal target.
            BeginTextureDraw();

            Context.ink.Begin();
            Context.ink.Draw(Texture, new Rectangle(0, 0, Bounds.Width, Bounds.Height), Bounds, Color.White);
            Context.ink.End();

            //Stop drawing.
            EndTextureDraw();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Size"></param>
        public void Resize(Vector2 Size)
        {
            Generate((int) Size.X, (int) Size.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public void Resize(int Width, int Height)
        {
            Resize(new Vector2(Width, Height));
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
        private void Generate(int Width = 0, int Height = 0)
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
    }
}

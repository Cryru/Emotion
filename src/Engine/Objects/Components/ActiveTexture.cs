using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;
using SoulEngine.Events;

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
                shouldUpdate = true;
                if (_texture == null) return AssetManager.MissingTexture;
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
        public Rectangle DrawArea;
        /// <summary>
        /// The way to texture should be rendered to fill its bounds.
        /// </summary>
        public TextureMode TextureMode = 0;
        /// <summary>
        /// Whether to regenerate the texture at the beginning of the next frame.
        /// </summary>
        public bool shouldUpdate = true;
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
        /// <param name="TextureMode"></param>
        public ActiveTexture(TextureMode TextureMode = 0)
        {
            this.TextureMode = TextureMode;
            Texture = AssetManager.MissingTexture;
            DrawArea = _xnaTexture.Bounds; //Render whole texture if area not specified.

            //Hook to the event frame start event where we will generate the texture.
            ESystem.Add(new Listen(EType.GAME_FRAMESTART, GenerateTexture));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        /// <param name="TextureMode"></param>
        /// <param name="DrawArea"></param>
        public ActiveTexture(Texture2D Texture, TextureMode TextureMode = 0, Rectangle DrawArea = new Rectangle())
        {
            this.TextureMode = TextureMode;
            this.Texture = Texture;
            this.DrawArea = DrawArea;

            //Hook to the event frame start event where we will generate the texture.
            ESystem.Add(new Listen(EType.GAME_FRAMESTART, GenerateTexture));
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        private void GenerateTexture(Event t)
        {
            //Check if we should update the texture.
            if (shouldUpdate == false) return;

            //Check empty draw area.
            if (DrawArea == new Rectangle()) DrawArea = _xnaTexture.Bounds;

            //Get the size of the object, if a Transform component is attached.
            Rectangle Bounds = DrawArea;
            if (attachedObject.HasComponent<Transform>())
            {
                Bounds = attachedObject.Component<Transform>().Bounds;
            }

            //Check if the render target is the same size as the draw area, because if it's not we need to redefine it.
            if (_texture == null ||
                Bounds.Width != _texture.Bounds.Size.X ||
                Bounds.Height != _texture.Bounds.Size.Y) DefineTarget(Bounds.Width, Bounds.Height);

            //Start drawing on internal target.
            BeginTargetDraw();

            Context.ink.Start();
            //Draw the texture depending on how we are stretching.
            switch (TextureMode)
            {
                case TextureMode.Stretch:
                    Context.ink.Draw(_xnaTexture, new Rectangle(0, 0, Bounds.Width, Bounds.Height), DrawArea, Color.White);
                    break;

                case TextureMode.Tile:
                    for (int x = 0; x < Bounds.Width / _xnaTexture.Width; x++)
                    {
                        for (int y = 0; y < Bounds.Height / _xnaTexture.Height; y++)
                        {
                            Context.ink.Draw(_xnaTexture, new Rectangle(_xnaTexture.Width * x, _xnaTexture.Height * y,
                                _xnaTexture.Width, _xnaTexture.Height), DrawArea, Color.White);
                        }
                    }
                    break;
            }
            Context.ink.End();

            //Stop drawing.
            EndTargetDraw();

            //Reset update flag.
            shouldUpdate = false;
        }
        /// <summary>
        /// 
        /// </summary>
        public void BeginTargetDraw()
        {
            //Record the viewport of the current graphics device, as the rendertarget switching resets it.
            _tempPortHolder = Context.Graphics.Viewport;

            //Set the current rendertarget to the drawer.
            Context.Graphics.SetRenderTarget(_texture);

            //Clear the rendertarget.
            Context.Graphics.Clear(Color.Blue);
        }
        /// <summary>
        /// 
        /// </summary>
        public void EndTargetDraw()
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
        private void DefineTarget(int Width = 0, int Height = 0)
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
                    //Unhook event.
                    ESystem.Remove(GenerateTexture);
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

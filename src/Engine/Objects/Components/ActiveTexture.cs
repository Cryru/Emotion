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
                if (TextureMode == TextureMode.Animate && attachedObject.HasComponent<Animation>())
                {
                    Frame = attachedObject.Component<Animation>().FrameIndex;
                }
                if ((TextureMode == TextureMode.Tile || TextureMode == TextureMode.Area || TextureMode == TextureMode.Animate || TextureMode == TextureMode.Frame) && _texture != null) return _texture as Texture2D;
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

        #region "Mode - Area"
        /// <summary>
        /// The area of the texture to draw when texture mode is set to area.
        /// </summary>
        public Rectangle DrawArea
        {
            get
            {
                return drawarea;
            }
            set
            {
                if (value == drawarea) return;
                drawarea = value;
                recompose = true;
            }
        }
        private Rectangle drawarea;
        #endregion

        #region "Mode - Frame"
        /// <summary>
        /// The frame of from the current texture to render when texture mode is frame, and frame size is defined.
        /// </summary>
        public int Frame
        {
            get
            {
                return frame;
            }
            set
            {
                if (value == frame) return;
                frame = value;

                //Calculate draw area from frame data.
                CalculateAreaForFrame();
            }
        }
        private int frame = -1;

        /// <summary>
        /// The size of individual frames of the texture to draw when texture mode is set to frame.
        /// </summary>
        public Vector2 FrameSize
        {
            get
            {
                return framesize;
            }
            set
            {
                if (value == framesize) return;
                framesize = value;

                //Calculate draw area from frame data.
                CalculateAreaForFrame();
            }
        }
        private Vector2 framesize;

        /// <summary>
        /// The spacing between individual frames of the texture to draw when texture mode is set to frame.
        /// </summary>
        public Vector2 Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (value == spacing) return;
                spacing = value;

                //Calculate draw area from frame data.
                CalculateAreaForFrame();
            }
        }
        private Vector2 spacing;

        /// <summary>
        /// The number of frames the spritesheet has with the current frame size.
        /// </summary>
        public int FramesCount
        {
            get
            {
                int fColumns = ActualTexture.Width / (int)FrameSize.X;
                int fRows = ActualTexture.Height / (int)FrameSize.Y;

                return fColumns * fRows;
            }
        }

        /// <summary>
        /// Calculates the draw area from frame data.
        /// </summary>
        private void CalculateAreaForFrame()
        {
            //Check if mode is frame and the size of frames is defined.
            if (framesize == null) return;

            //If no spacing is set, reset to default.
            if (spacing == null) spacing = new Vector2(0, 0);

            //Calculate columns and rows.
            int fColumns = ActualTexture.Width / (int)FrameSize.X;
            int fRows = ActualTexture.Height / (int)FrameSize.Y;

            //Check if frame is out of bounds.
            if (frame > FramesCount) frame = 0;
            if (frame < 0) frame = FramesCount;

            //Calculate the location of the current sprite within the image.
            int Row = (int)(Frame / (float)fColumns);
            int Column = Frame % fColumns;

            //Determine the rectangle area of the frame within the spritesheet.
            Rectangle FrameRect = new Rectangle((int)FrameSize.X * Column + (int)(Spacing.X * (Column + 1)),
                (int)FrameSize.Y * Row + (int)(Spacing.Y * (Row + 1)), (int)FrameSize.X, (int)FrameSize.Y);

            //Overwrite the frame.
            DrawArea = FrameRect;
        }
        #endregion

        #region "Private"
        /// <summary>
        /// The buffer we compose textures to.
        /// </summary>
        private RenderTarget2D _texture;
        /// <summary>
        /// The way the texture should be rendered.
        /// </summary>
        private TextureMode _mode = 0;
        /// <summary>
        /// Whether to recompose the texture.
        /// </summary>
        private bool recompose = false;
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
        public ActiveTexture()
        {
            TextureMode = TextureMode.Stretch;
            Texture = AssetManager.MissingTexture;
        }
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
        /// <summary>
        /// Create a new ActiveTexture object for animation.
        /// </summary>
        /// <param name="Texture">The texture to draw.</param>
        /// <param name="FrameSize">The size of individual frames.</param>
        /// <param name="Spacing">The spacing between frames.</param>
        public ActiveTexture(Texture2D Texture, Vector2 FrameSize, Vector2 Spacing = new Vector2())
        {
            TextureMode = TextureMode.Animate;
            this.Texture = Texture;
            framesize = FrameSize;
            this.Spacing = Spacing;
        }
        #endregion

        #region "Functions"
        /// <summary>
        /// Compose the ActiveTexture's texture based on its TextureMode.
        /// </summary>
        public override void Compose()
        {
            //Check if the texture mode is set to tile, and a texture hasn't been composed.
            if (TextureMode == TextureMode.Tile && (_texture == null || recompose)) ComposeTile();

            //Check if the texture mode is set to area, and a texture hasn't been composed.
            if ((TextureMode == TextureMode.Area || TextureMode == TextureMode.Animate || TextureMode == TextureMode.Frame) && (_texture == null || recompose)) ComposeArea();
        }

        /// <summary>
        /// Composes a tiled image of the provided texture.
        /// </summary>
        public void ComposeArea()
        {
            //Start drawing on internal target.
            Context.ink.StartRenderTarget(ref _texture, DrawArea.Width, DrawArea.Height);

            Context.ink.Draw(ActualTexture, new Rectangle(0, 0,
                        DrawArea.Width, DrawArea.Height), DrawArea, Color.White);

            //Stop drawing.
            Context.ink.EndRenderTarget();
        }

        /// <summary>
        /// Composes a tiled image of the provided texture.
        /// </summary>
        public void ComposeTile()
        {
            //Get the size of the object.
            Rectangle Bounds = attachedObject.Bounds;

            //Start drawing on internal target.
            Context.ink.StartRenderTarget(ref _texture, Bounds.Width, Bounds.Height);

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

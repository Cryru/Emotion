#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.Animation2D;
using Emotion.Graphics;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Utility;

#endregion

namespace Emotion.UI
{
    public class UITexture : UISolidColor
    {
        /// <summary>
        /// Path to the texture file to load.
        /// </summary>
        public string TextureFile
        {
            get => _textureFile;
            set
            {
                _textureFile = value;
                InvalidateLoaded();
            }
        }

        private string _textureFile;

        /// <summary>
        /// The size of the window. If not specified either the UV or parent size will be taken.
        /// </summary>
        public Vector2? RenderSize;

        /// <summary>
        /// An override scale for the image. By default it is drawn either in full size, uv size, or render size.
        /// </summary>
        public Vector2? ImageScale;

        /// <summary>
        /// Custom UVs for the texture. By default the whole texture is drawn.
        /// </summary>
        public Rectangle? UV
        {
            get => _uvBacking;
            set
            {
                _uvBacking = value;
                CalculateUV();
            }
        }

        private Rectangle? _uvBacking;

        /// <summary>
        /// Whether to enable smoothing on the texture. This is only done when loaded and will overwrite the
        /// texture setting for other users using the texture.
        /// </summary>
        public bool Smooth;

        /// <summary>
        /// The column of the texture to display.
        /// </summary>
        public int Column
        {
            get => _columnBacking;
            set
            {
                _columnBacking = value;
                CalculateUV();
            }
        }

        /// <summary>
        /// The row of the texture to display.
        /// </summary>
        public int Row
        {
            get => _rowBacking;
            set
            {
                _rowBacking = value;
                CalculateUV();
            }
        }

        private int _columnBacking = 1;
        private int _rowBacking = 1;

        /// <summary>
        /// The total number of columns in the texture.
        /// </summary>
        public int Columns = 1;

        /// <summary>
        /// The total number of rows in the texture.
        /// </summary>
        public int Rows = 1;

        /// <summary>
        /// Space between rows and columns in texture.
        /// </summary>
        public Vector2 RowAndColumnSpacing = Vector2.Zero;

        public bool FlipX;

        protected Rectangle _uv;

        [DontSerialize]
        public TextureAsset TextureAsset { get; protected set; }

        #region Shader

        public string ShaderFilePath
        {
            get => _shaderFilePath;
            set
            {
                _shaderFilePath = value;
                InvalidateLoaded();
            }
        }

        private string _shaderFilePath;

        [DontSerialize]
        public ShaderAsset ShaderAsset { get; set; }

        [DontSerialize]
        public Action<ShaderProgram> OnShaderSet;

        #endregion

        public UITexture(TextureAsset texture) : this()
        {
            TextureAsset = texture;
        }

        public UITexture()
        {
            GrowX = false;
            GrowY = false;
        }

        protected override async Task LoadContent()
        {
            var loadedNew = false;
            if (TextureFile == null) return;

            string fileEngineName = AssetLoader.NameToEngineName(TextureFile);
            if (TextureAsset == null || TextureAsset.Name != fileEngineName || TextureAsset.Disposed)
            {
                TextureAsset = await Engine.AssetLoader.GetAsync<TextureAsset>(fileEngineName);
                loadedNew = true;
            }

            if (ShaderFilePath == null)
            {
                ShaderAsset = null;
            }
            else
            {
                string shaderPathEngineName = AssetLoader.NameToEngineName(ShaderFilePath);
                if (ShaderAsset == null || ShaderAsset.Name != shaderPathEngineName || ShaderAsset.Disposed)
                {
                    ShaderAsset = await Engine.AssetLoader.GetAsync<ShaderAsset>(ShaderFilePath);
                }
            }

            if (TextureAsset == null) return;

            if (loadedNew)
            {
                if (Smooth != TextureAsset.Texture.Smooth) TextureAsset.Texture.Smooth = Smooth;
                InvalidateLayout();
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            CalculateUV();

            float scale = GetScale();
            Vector2 size;
            if (RenderSize != null)
                size = GetRenderSizeProcessed(space);
            else
                size = _uv.Size * scale;

            if (ImageScale != null) size *= ImageScale.Value;
            return size;
        }

#if !NEW_UI
        protected override Vector2 NEW_InternalMeasure(Vector2 space)
        {
            return InternalMeasure(space);
        }
#endif

        protected override bool RenderInternal(RenderComposer c)
        {
            ShaderProgram prevShader = c.CurrentState.Shader;
            if (ShaderAsset != null)
            {
                c.SetShader(ShaderAsset.Shader);
                OnShaderSet?.Invoke(ShaderAsset.Shader);
            }

            if (TextureAsset == null)
                base.RenderInternal(c);
            else
                c.RenderSprite(Position, Size, _calculatedColor, TextureAsset.Texture, _uv, FlipX);

            if (ShaderAsset != null)
                c.SetShader(prevShader);

            return true;
        }

        protected void CalculateUV()
        {
            if (UV != null)
            {
                _uv = UV.Value;
                return;
            }

            if (TextureAsset == null)
            {
                _uv = Rectangle.Empty;
                return;
            }

            Vector2 textureSize = TextureAsset.Texture.Size;
            var colRowVec = new Vector2(Columns, Rows);
            Vector2 frameSize = (textureSize - RowAndColumnSpacing * colRowVec) / colRowVec;

            int rowIdx = Maths.Clamp(_rowBacking, 1, Rows) - 1;
            int columnIdx = Maths.Clamp(_columnBacking, 1, Columns) - 1;
            _uv = Animation2DHelpers.GetGridFrameBounds(textureSize, frameSize, RowAndColumnSpacing, rowIdx, columnIdx);
        }

        protected Vector2 GetRenderSizeProcessed(Vector2 space)
        {
            float scale = GetScale();
            float xVal = RenderSize!.Value.X;
            float yVal = RenderSize!.Value.Y;

            if (MathF.Abs(xVal) < 0 || MathF.Abs(yVal) < 0)
                Engine.Log.Warning($"RenderSize of texture {Id} is less than 0. You sure?", MessageSource.UI, true);

            // Percentage of space.
            if (xVal < 0)
            {
                // Percentage of height.
                if (xVal < -100)
                    xVal = space.Y * ((-xVal - 100) / 100.0f);
                else
                    xVal = space.X * (-xVal / 100.0f);
            }
            else
            {
                xVal *= scale;
            }

            if (yVal < 0)
            {
                if (yVal < -100)
                    yVal = space.X * ((-yVal - 100) / 100.0f);
                else
                    yVal = space.Y * (-yVal / 100.0f);
            }
            else
            {
                yVal *= scale;
            }

            return new Vector2(xVal, yVal);
        }
    }
}
// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics.GLES;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Renderable2D : Transform
    {
        #region Properties

        /// <summary>
        /// The objects texture.
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// The area of the texture to render.
        /// </summary>
        public Rectangle? TextureArea = null;

        /// <summary>
        /// The color to render in.
        /// </summary>
        public Color Color = Color.White;

        #endregion

        #region Matrix

        /// <summary>
        /// The translation and rotation matrix of this renderable.
        /// </summary>
        public Matrix4 ModelMatrix
        {
            get
            {
                // Check if we need to update the model matrix.
                if (!_updated) return _modelMatrix;

                Matrix4 matrix = Matrix4.Identity;

                float xCenter = Width / 2;
                float yCenter = Height / 2;

                // Add rotation.
                matrix *= Matrix4.CreateTranslation(xCenter, yCenter, 1).Inverted() * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(xCenter, yCenter, 1);

                // Add position.
                matrix *= Matrix4.CreateTranslation(X, Y, Z);

                _modelMatrix = matrix;
                _updated = false;

                return _modelMatrix;
            }
        }

        private Matrix4 _modelMatrix;

        #endregion

        #region Constructors

        public Renderable2D(Rectangle bounds, float rotation = 0f, Color? color = null) : base(bounds.Location, bounds.Size, rotation)
        {
            if (color != null) Color = (Color) color;
        }

        public Renderable2D(Vector3 position, Vector2 size, float rotation = 0f, Color? color = null) : base(position.X, position.Y, position.Z, size.X, size.Y, rotation)
        {
            if (color != null) Color = (Color) color;
        }

        public Renderable2D(Vector2 position, Vector2 size, float rotation = 0f, Color? color = null) : base(position.X, position.Y, 0, size.X, size.Y, rotation)
        {
            if (color != null) Color = (Color) color;
        }

        public Renderable2D(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f, float rotation = 0f, Color? color = null) :
            base (x, y, z, width, height, rotation)
        {
            if (color != null) Color = (Color) color;
        }

        #endregion
    }
}
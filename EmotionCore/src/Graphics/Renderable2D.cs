// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics.GLES;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Renderable2D
    {
        #region Properties

        public Vector3 Position { get; set; }
        public Vector2 Size { get; set; }
        public Color Color { get; set; }

        #endregion

        #region Buffers

        public ShaderProgram Shader { get; protected set; }

        #endregion

        public Renderable2D(Vector3 position, Vector2 size, Color color, ShaderProgram shader)
        {
            Position = position;
            Size = size;
            Color = color;
            Shader = shader;

        }

        /// <summary>
        /// Destroy the renderable freeing memory.
        /// </summary>
        protected void Destroy()
        {

        }
    }
}
#region Using

using System.Numerics;
using Emotion.Graphics.Shading;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Effects
{
    public class Palette
    {
        public string Name { get; set; }
        public Color[] Colors { get; set; }

        public Palette()
        {
        }

        public Palette(string name, Color[] colors)
        {
            Name = name;
            Colors = colors;
        }

        /// <summary>
        /// Upload the palette to the shader.
        /// </summary>
        /// <param name="shader">The shader to upload to.</param>
        /// <param name="uniformName">The name of the uniform</param>
        public void Upload(ShaderProgram shader, string uniformName = "Palette")
        {
            var array = new Vector4[Colors.Length];
            for (var i = 0; i < Colors.Length; i++)
            {
                array[i] = Colors[i].ToVec4();
            }

            shader.SetUniformVector4Array(uniformName, array);
        }
    }
}
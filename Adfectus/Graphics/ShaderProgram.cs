#region Using

using System.Numerics;
using Adfectus.Primitives;

#endregion

namespace Adfectus.Graphics
{
    /// <summary>
    /// A shader interface.
    /// </summary>
    public abstract class ShaderProgram
    {
        /// <summary>
        /// The id of the shader program.
        /// </summary>
        public uint Id { get; protected set; }

        /// <summary>
        /// Whether the shader is broken.
        /// </summary>
        public bool Broken { get; protected set; }

        /// <summary>
        /// Delete this program freeing memory.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Returns the id of a uniform with a specified name within the shader program.
        /// </summary>
        /// <param name="name">The name of the uniform whose id to return.</param>
        /// <returns>The id of the uniform the name belongs to.</returns>
        public abstract uint GetUniformLocation(string name);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public abstract void SetUniformMatrix4(string name, Matrix4x4 matrix4);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public abstract void SetUniformInt(string name, int data);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public abstract void SetUniformFloat(string name, float data);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public abstract void SetUniformColor(string name, Color data);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int array value to set it to.</param>
        public abstract void SetUniformIntArray(string name, int[] data);
        
        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector 2 to set it to.</param>
        public abstract void SetUniformVector2(string name, Vector2 data);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector 3 to set it to.</param>
        public abstract void SetUniformVector3(string name, Vector3 data);

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector4 to set it to.</param>
        public abstract void SetUniformVector4(string name, Vector4 data);

        /// <summary>
        /// Checks whether the current shader and the one provided are the same.
        /// </summary>
        /// <param name="obj">The shader to compare with.</param>
        /// <returns>Whether the two shader programs are the same.</returns>
        public bool Equals(ShaderProgram obj)
        {
            return Id == obj.Id;
        }
    }
}
#region Using

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    public abstract class VertexArrayObject : IDisposable
    {
        /// <summary>
        /// The OpenGL pointer to this VertexArrayObject.
        /// </summary>
        public uint Pointer { get; set; }

        /// <summary>
        /// The bound vertex array buffer.
        /// </summary>
        public static uint Bound;

        /// <summary>
        /// The vertex buffer attached to this vertex array object.
        /// When the vao is bound, this vbo is bound automatically.
        /// </summary>
        public VertexBuffer VBO { get; protected set; }

        /// <summary>
        /// The ibo attached to this vertex array object.
        /// When the vao is bound, this ibo is bound automatically.
        /// </summary>
        public IndexBuffer IBO { get; protected set; }

        /// <summary>
        /// The byte size of one instance of the structure.
        /// </summary>
        public int ByteSize { get; set; }

        /// <summary>
        /// The offset of the UV field within the structure. The UV field is expected to be at least a Vec2.
        /// </summary>
        public int UVByteOffset { get; set; }

        /// <summary>
        /// Ensures the provided pointer is the currently bound vertex array buffer.
        /// </summary>
        /// <param name="vao">The vertex array object to ensure is bound.</param>
        public static void EnsureBound(VertexArrayObject vao)
        {
            // todo: VertexArrayObject binding cache has been temporarily commented out as it seems to have problems in some cases.
            // Check if it is already bound.
            //if (Bound == vao.Pointer)
            //{
            //    // If in debug mode, verify this with OpenGL.
            //    if (!Engine.Configuration.DebugMode) return;

            //    // Ensure the bound elements are correct as well.
            //    VertexBuffer.EnsureBound(vao.VBO.Pointer);
            //    if (vao.IBO != null) IndexBuffer.EnsureBound(vao.IBO.Pointer);

            //    Gl.GetInteger(GetPName.VertexArrayBinding, out uint actualBound);
            //    if (actualBound != vao.Pointer) Engine.Log.Error($"Assumed bound vertex array buffer was {vao.Pointer} but it was {actualBound}.", MessageSource.GL);
            //    return;
            //}

            uint ptr = vao?.Pointer ?? 0;
            Gl.BindVertexArray(ptr);
            Bound = ptr;

            // Reset the cached binding of VertexBuffers and IndexBuffers as the VAO just bound something else.
            // This does disable the cache until the binding, but otherwise we will be binding twice.
            VertexBuffer.Bound = 0;
            IndexBuffer.Bound = 0;

            // Some Intel drivers don't bind the IBO when the VAO is bound.
            // https://stackoverflow.com/questions/8973690/vao-and-element-array-buffer-state
            if (vao?.IBO != null) IndexBuffer.EnsureBound(vao.IBO.Pointer);
        }

        /// <summary>
        /// Convert a managed C# type to a GL vertex attribute type.
        /// </summary>
        /// <param name="type">The managed type to convert.</param>
        /// <returns>The vertex attribute type corresponding to the provided managed type.</returns>
        public static VertexAttribType GetAttribTypeFromManagedType(MemberInfo type)
        {
            switch (type.Name)
            {
                default:
                    return VertexAttribType.Float;
                case "Int32":
                    return VertexAttribType.Int;
                case "UInt32":
                    return VertexAttribType.UnsignedInt;
                case "SByte":
                    return VertexAttribType.Byte;
                case "Byte":
                    return VertexAttribType.UnsignedByte;
            }
        }

        /// <summary>
        /// Setup vertex attributes.
        /// </summary>
        public abstract void SetupAttributes();

        #region Cleanup

        public void Dispose()
        {
            uint ptr = Pointer;
            Pointer = 0;

            if (Engine.Host == null) return;
            if (ptr == Bound) Bound = 0;

            GLThread.ExecuteGLThreadAsync(() => { Gl.DeleteVertexArrays(ptr); });
        }

        #endregion
    }

    /// <summary>
    /// Create a vertex array object from the described structure.
    /// </summary>
    /// <typeparam name="T">The structure to convert to a vertex array object.</typeparam>
    public sealed class VertexArrayObject<T> : VertexArrayObject where T : new()
    {
        public VertexArrayObject(VertexBuffer vbo, IndexBuffer ibo = null)
        {
            Pointer = Gl.GenVertexArray();
            VBO = vbo;
            IBO = ibo;
            SetupAttributes();
            EnsureBound(null);
        }

        public override void SetupAttributes()
        {
            EnsureBound(this);

            Type structFormat = typeof(T);
            FieldInfo[] fields = structFormat.GetFields(BindingFlags.Public | BindingFlags.Instance);
            ByteSize = Marshal.SizeOf(new T());

            for (uint i = 0; i < fields.Length; i++)
            {
                Attribute[] fieldAttributes = fields[i].GetCustomAttributes().ToArray();
                VertexAttributeAttribute vertexAttributeData = null;

                // Go through all attributes and find the VertexAttributeAttribute.
                foreach (Attribute attribute in fieldAttributes)
                {
                    if (!(attribute is VertexAttributeAttribute data)) continue;
                    vertexAttributeData = data;
                    break;
                }

                // If the vertex attribute data not found, stop searching.
                if (vertexAttributeData == null) continue;

                string fieldName = fields[i].Name;
                IntPtr offset = Marshal.OffsetOf(structFormat, fieldName);
                Type fieldType = vertexAttributeData.TypeOverride ?? fields[i].FieldType;
                if (fieldName == "UV") UVByteOffset = (int) offset;

                uint position = vertexAttributeData.PositionOverride != -1 ? (uint) vertexAttributeData.PositionOverride : i;
                Gl.EnableVertexAttribArray(position);
                Gl.VertexAttribPointer(position, vertexAttributeData.ComponentCount, GetAttribTypeFromManagedType(fieldType), vertexAttributeData.Normalized, ByteSize, offset);
            }
        }
    }
}
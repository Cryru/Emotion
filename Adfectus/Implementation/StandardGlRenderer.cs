#region Using

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Adfectus.Common;
using Adfectus.Graphics;
using OpenGL;

#endregion

namespace Adfectus.Implementation
{
    /// <summary>
    /// Work in progress class.
    /// </summary>
    [Obsolete("This class is a work in progress.")]
    public class StandardGLRenderer
    {
        public void ProcessDrawCommands()
        {
        }

        /// <summary>
        /// Create a vao and bind it to a vbo.
        /// </summary>
        /// <param name="vbo">The VBO to bind to the vao.</param>
        /// <param name="structFormat">The structure which describes the vao format.</param>
        /// <returns>The id of the created vao.</returns>
        private uint CreateVao(uint vbo, Type structFormat)
        {
            uint vaoId = Gl.GenVertexArray();
            Gl.BindVertexArray(vaoId);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            FieldInfo[] fields = structFormat.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int size = Marshal.SizeOf(Activator.CreateInstance(structFormat));

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

                IntPtr offset = Marshal.OffsetOf(structFormat, fields[i].Name);
                Type fieldType = vertexAttributeData.TypeOverride ?? fields[i].FieldType;

                Gl.EnableVertexAttribArray(i);
                Gl.VertexAttribPointer(i, vertexAttributeData.ComponentCount, GetAttribTypeFromManagedType(fieldType), vertexAttributeData.Normalized, size, offset);
            }

            return vaoId;
        }

        #region Helpers

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
        /// Get debugging information about the gl state.
        /// </summary>
        /// <returns>A string of all currently bound gl objects</returns>
        public string GetBoundDebugInfo()
        {
            Gl.Get(GetPName.ElementArrayBufferBinding, out int boundIbo);
            Gl.Get(GetPName.ArrayBufferBinding, out int boundVbo);
            Gl.Get(GetPName.VertexArrayBinding, out int boundVao);
            Gl.Get(GetPName.CurrentProgram, out int boundShader);
            Gl.GetBufferParameter(BufferTarget.ArrayBuffer, Gl.BUFFER_SIZE, out int boundVboSize);

            return $"IBO: {boundIbo}\r\nVBO: {boundVbo} [size:{boundVboSize}]\r\nVAO: {boundVao}\r\nShader: {boundShader}";
        }

        /// <summary>
        /// Check for a gl error.
        /// </summary>
        /// <param name="location">The location where the check is - for debugging purposes.</param>
        public void CheckError(string location = "")
        {
            ErrorCode errorCheck = Gl.GetError();
            if (errorCheck != ErrorCode.NoError) ErrorHandler.SubmitError(new Exception($"OpenGL error at {location}: {errorCheck}"));
        }

        #endregion
    }
}
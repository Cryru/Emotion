#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Graphics.Shading
{
    /// <summary>
    /// An object representing a GLSL shader uniform.
    /// </summary>
    public class ShaderUniform
    {
        public int Location = -1;
        public string Name;
        public string Type;
        public object Value;

        /// <summary>
        /// Create a new shader uniform from its data representation.
        /// The glsl type name will be inferred.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ShaderUniform(string name, object value)
        {
            Name = name;
            Type = ""; // todo: Infer from value.
            Value = value;
        }

        /// <summary>
        /// Create a new shader uniform from its shader source representation.
        /// The value will be parsed.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public ShaderUniform(string name, string type, string value)
        {
            Name = name;
            Type = type;
            Value = ParseValue(type, value);
        }

        /// <summary>
        /// Parse the value of a uniform from its string representation.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ParseValue(string type, string value)
        {
            float[] floatVals = null;
            if (type == "vec2" || type == "vec3" || type == "vec4")
            {
                int startIdx = value.IndexOf("(", StringComparison.Ordinal);
                int endIdx = value.LastIndexOf(")", StringComparison.Ordinal);
                if (startIdx != -1 && endIdx != -1) value = value.Substring(startIdx + 1, endIdx - startIdx - 1);
                string[] values = value.Split(",");
                floatVals = new float[values.Length];
                for (var i = 0; i < values.Length; i++)
                {
                    float.TryParse(values[i].Trim(), out float floatVal);
                    floatVals[i] = floatVal;
                }
            }

            switch (type)
            {
                case "float":
                {
                    float.TryParse(value, out float floatVal);
                    return floatVal;
                }
                case "int":
                    int.TryParse(value, out int intVal);
                    return intVal;
                case "vec2":
                {
                    if (floatVals == null || floatVals.Length == 0) floatVals = new float[2];

                    // vec2(0.0) or vec2(0.0, 0.0)
                    return floatVals.Length == 1 ? new Vector2(floatVals[0]) : new Vector2(floatVals[0], floatVals[1]);
                }
                case "vec3":
                {
                    if (floatVals == null) floatVals = new float[3];
                    if (floatVals.Length == 2) Array.Resize(ref floatVals, 3);
                    return floatVals.Length == 1 ? new Vector3(floatVals[0]) : new Vector3(floatVals[0], floatVals[1], floatVals[2]);
                }
                case "vec4":
                {
                    if (floatVals == null) floatVals = new float[3];
                    if (floatVals.Length == 2 || floatVals.Length == 3) Array.Resize(ref floatVals, 4);
                    return floatVals.Length == 1 ? new Vector4(floatVals[0]) : new Vector4(floatVals[0], floatVals[1], floatVals[2], floatVals[3]);
                }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Apply the value of this uniform to the specified shader.
        /// </summary>
        /// <param name="program">The shader to apply to.</param>
        public void ApplySelf(ShaderProgram program)
        {
            var applied = false;
            switch (Type)
            {
                case "float":
                    applied = program.SetUniformFloat(Name, (float) Value);
                    break;
                case "int":
                    applied = program.SetUniformInt(Name, (int) Value);
                    break;
                case "vec2":
                    applied = program.SetUniformVector2(Name, (Vector2) Value);
                    break;
                case "vec3":
                    applied = program.SetUniformVector3(Name, (Vector3) Value);
                    break;
                case "vec4":
                    applied = program.SetUniformVector4(Name, (Vector4) Value);
                    break;
                default:
                    Engine.Log.Warning($"Unknown shader uniform type {Type}. Default value will probably be missing.", MessageSource.Renderer);
                    return;
            }

            if (!applied && Engine.Configuration.GlDebugMode) Engine.Log.Info($"Couldn't apply shader uniform {this}", MessageSource.Renderer);
        }

        public override string ToString()
        {
            return $"{Type} {Name} {Value}";
        }
    }
}
#region Using

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
                case "vec4":
                {
                    string[] values = value.Split(",");
                    var floatVals = new float[4];
                    for (var i = 0; i < values.Length; i++)
                    {
                        float.TryParse(values[i].Trim(), out float floatVal);
                        floatVals[i] = floatVal;
                    }

                    // vec4(0.0) or vec4(0.0, 0.0, 0.0, 0.0)
                    return values.Length == 1 ? new Vector4(floatVals[0]) : new Vector4(floatVals[0], floatVals[1], floatVals[2], floatVals[3]);
                }
                case "vec2":
                {
                    string[] values = value.Split(",");
                    var floatVals = new float[2];
                    for (var i = 0; i < values.Length; i++)
                    {
                        float.TryParse(values[i].Trim(), out float floatVal);
                        floatVals[i] = floatVal;
                    }

                    // vec2(0.0) or vec2(0.0, 0.0)
                    return values.Length == 1 ? new Vector2(floatVals[0]) : new Vector2(floatVals[0], floatVals[1]);
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
            switch (Type)
            {
                case "float":
                    program.SetUniformFloat(Name, (float) Value);
                    break;
                case "vec4":
                    program.SetUniformVector4(Name, (Vector4) Value);
                    break;
                case "int":
                    program.SetUniformInt(Name, (int) Value);
                    break;
                case "vec2":
                    program.SetUniformVector2(Name, (Vector2) Value);
                    break;
                default:
                    Engine.Log.Warning($"Unknown shader uniform type {Type}. Default value will probably be missing.", MessageSource.Renderer);
                    break;
            }
        }
    }
}
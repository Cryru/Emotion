#region Using

using System;
using System.Collections.Generic;
using System.Reflection;
using Emotion.Common;
using Emotion.Standard.Logging;
using Khronos;

#endregion

// ReSharper disable once CheckNamespace
namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// Class collecting the OpenGL implementation limits.
        /// </summary>
        public sealed partial class Limits
        {
            #region Query

            /// <summary>
            /// Query the OpenGL implementation limits.
            /// </summary>
            /// <param name="version">
            /// The <see cref="KhronosVersion" /> that specifies the GL version.
            /// </param>
            /// <param name="glExtensions">
            /// A <see cref="Gl.Extensions" /> that specify the supported OpenGL extension by the current
            /// implementation.
            /// </param>
            /// <returns>
            /// It returns a <see cref="Gl.Limits" /> that specify the current OpenGL implementation limits.
            /// </returns>
            /// <remarks>
            /// It is assumed to have a valid OpenGL context current on the calling thread.
            /// </remarks>
            public static Limits Query(KhronosVersion version, Extensions glExtensions)
            {
                if (glExtensions == null)
                    throw new ArgumentNullException("glExtensions");

                var graphicsLimits = new Limits();
                IEnumerable<FieldInfo> graphicsLimitsFields = typeof(Limits).GetTypeInfo().DeclaredFields;

                // Supress errors. Some limits might be missing from certain drivers and versions.
                SuppressingErrors = true;

                foreach (FieldInfo field in graphicsLimitsFields)
                {
                    var graphicsLimitAttribute = (LimitAttribute) field.GetCustomAttribute(typeof(LimitAttribute));
                    if (graphicsLimitAttribute == null) continue;

                    // Check extension support
                    Attribute[] graphicsExtensionAttributes = new List<Attribute>(field.GetCustomAttributes(typeof(RequiredByFeatureAttribute))).ToArray();
                    if (graphicsExtensionAttributes != null && graphicsExtensionAttributes.Length > 0)
                    {
                        bool supported = Array.Exists(graphicsExtensionAttributes, delegate(Attribute item)
                        {
                            var featureAttribute = (RequiredByFeatureAttribute) item;
                            return featureAttribute.IsSupported(version, glExtensions);
                        });

                        if (!supported) continue;
                    }

                    // Determine which method is used to get the OpenGL limit
                    MethodInfo getMethod;
                    if (field.FieldType != typeof(string))
                        getMethod = typeof(Gl).GetMethod("Get", field.FieldType.IsArray ? new[] {typeof(int), field.FieldType} : new[] {typeof(int), field.FieldType.MakeByRefType()});
                    else
                        getMethod = typeof(Gl).GetMethod("GetString", new[] {typeof(int)});

                    if (getMethod == null)
                    {
                        Engine.Log.Error("GraphicsLimits field " + field.Name + " doesn't have a OpenGL compatible type.", MessageSource.GL);
                        continue;
                    }

                    if (field.FieldType != typeof(string))
                    {
                        object obj = field.FieldType.IsArray == false
                            ? Activator.CreateInstance(field.FieldType)
                            : Array.CreateInstance(field.FieldType.GetElementType(), (int) graphicsLimitAttribute.ArrayLength);

                        try
                        {
                            object[] @params = {graphicsLimitAttribute.EnumValue, obj};
                            getMethod.Invoke(null, @params);
                            field.SetValue(graphicsLimits, @params[1]);
                        }
                        catch (TargetInvocationException exception)
                        {
                            Engine.Log.Error($"Getting {field.Name} (0x{graphicsLimitAttribute.EnumValue:X4}): {exception.InnerException?.Message}", MessageSource.GL);
                        }
                    }
                    else
                    {
                        try
                        {
                            var s = (string) getMethod.Invoke(null, new object[] {graphicsLimitAttribute.EnumValue});
                            field.SetValue(graphicsLimits, s);
                        }
                        catch (TargetInvocationException exception)
                        {
                            Engine.Log.Error($"Getting {field.Name} (0x{graphicsLimitAttribute.EnumValue}): {exception.InnerException?.Message}", MessageSource.GL);
                        }
                    }
                }

                SuppressingErrors = false;
                return graphicsLimits;
            }

            #endregion
        }
    }
}
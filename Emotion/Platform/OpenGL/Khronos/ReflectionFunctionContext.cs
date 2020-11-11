#region Using

using System;
using System.Diagnostics;
using System.Reflection;
using DelegateList = System.Collections.Generic.List<System.Reflection.FieldInfo>;

#endregion

namespace Emotion.Platform.OpenGL.Khronos
{
    /// <summary>
    /// Helps with loading delegate functions from a static class.
    /// </summary>
    public class ReflectionFunctionContext
    {
        /// <summary>
        /// Type of class containing all delegates.
        /// </summary>
        private readonly Type _delegateType;

        /// <summary>
        /// The delegate fields list for the underlying type.
        /// </summary>
        public readonly DelegateList Delegates;

        /// <summary>
        /// Construct a list of all static functions of a type.
        /// </summary>
        public ReflectionFunctionContext(Type type)
        {
            Type delegatesClass = type.GetNestedType("Delegates", BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(delegatesClass != null);
            _delegateType = delegatesClass;
            Delegates = GetDelegateList(type);
        }

        /// <summary>
        /// Get the field representing the delegate for an API function.
        /// </summary>
        public FieldInfo GetFunction(string functionName)
        {
            if (functionName == null)
                throw new ArgumentNullException(nameof(functionName));

            FieldInfo functionField = _delegateType.GetField("p" + functionName, BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(functionField != null);

            return functionField;
        }

        /// <summary>
        /// Get the delegates methods for the specified type.
        /// </summary>
        public static DelegateList GetDelegateList(Type type)
        {
            Type delegatesClass = type.GetNestedType("Delegates", BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(delegatesClass != null);

            return new DelegateList(delegatesClass.GetFields(BindingFlags.Static | BindingFlags.Public));
        }
    }
}
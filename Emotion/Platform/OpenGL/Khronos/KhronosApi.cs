#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Emotion.Platform;
using Emotion.Platform.OpenGL.Khronos;
using OpenGL;

#endregion

// ReSharper disable once CheckNamespace
namespace Khronos
{
    /// <summary>
    /// Base class for loading external routines.
    /// </summary>
    public class KhronosApi
    {
        #region Function Linkage

        /// <summary>
        /// The function context for delegates in the Gl class.
        /// </summary>
        public static ReflectionFunctionContext _functionContext = new ReflectionFunctionContext(typeof(Gl));

        public static void BindAPIFunction(string functionName, KhronosVersion version, ExtensionsCollection extensions, GraphicsContext context)
        {
            BindAPIFunction(_functionContext.GetFunction(functionName), version, extensions, context);
        }

        public static void BindAPIFunction(FieldInfo function, KhronosVersion version, ExtensionsCollection extensions, GraphicsContext context)
        {
            Debug.Assert(function != null);

            RequiredByFeatureAttribute requiredByFeature = null;
            var requiredByExtensions = new List<RequiredByFeatureAttribute>();
            string defaultName = function.Name.Substring(1); // Delegate name always prefixes with 'p'

            if (version != null || extensions != null)
            {
                var removed = false;

                #region Check Requirement

                IEnumerable<Attribute> attrRequired = new List<Attribute>(function.GetCustomAttributes(typeof(RequiredByFeatureAttribute)));
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (RequiredByFeatureAttribute attr in attrRequired)
                {
                    // Check for API support
                    if (attr.IsSupported(version, extensions) == false)
                        continue;
                    // Keep track of the features requiring this command
                    if (attr.FeatureVersion != null)
                    {
                        // Version feature: keep track only of the maximum version
                        if (requiredByFeature == null || requiredByFeature.FeatureVersion < attr.FeatureVersion)
                            requiredByFeature = attr;
                    }
                    else
                    {
                        // Extension feature: collect every supporting extension
                        requiredByExtensions.Add(attr);
                    }
                }

                #endregion

                #region Check Deprecation/Removal

                if (requiredByFeature != null)
                {
                    // Note: indeed the feature could be supported; check whether it is removed; this is checked only if
                    // a non-extension feature is detected: extensions cannot remove commands
                    Attribute[] attrRemoved = Attribute.GetCustomAttributes(function, typeof(RemovedByFeatureAttribute));
                    KhronosVersion maxRemovedVersion = null;

                    // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                    foreach (RemovedByFeatureAttribute attr in attrRemoved)
                    {
                        // Check for API support
                        if (attr.IsRemoved(version, extensions) == false)
                            continue;
                        // Removed!
                        removed = true;
                        // Keep track of the maximum API version removing this command
                        if (maxRemovedVersion == null || maxRemovedVersion < attr.FeatureVersion)
                            maxRemovedVersion = attr.FeatureVersion;
                    }

                    // Check for resurrection
                    if (removed)
                    {
                        Debug.Assert(requiredByFeature != null);
                        Debug.Assert(maxRemovedVersion != null);

                        if (requiredByFeature.FeatureVersion > maxRemovedVersion)
                            removed = false;
                    }
                }

                #endregion

                // Do not check feature requirements in case of removal. Note: extensions are checked all the same
                if (removed)
                    requiredByFeature = null;
            }

            // Load function pointer
            IntPtr importAddress;
            Delegate importDelegate;

            if (requiredByFeature != null || version == null)
            {
                // Load command address (version feature)
                string functionName = defaultName;

                if (requiredByFeature?.EntryPoint != null)
                    functionName = requiredByFeature.EntryPoint;

                if (context.Native)
                {
                    if ((importAddress = context.GetProcAddress(functionName)) != IntPtr.Zero)
                    {
                        BindNativeAPIFunction(function, importAddress);
                        return;
                    }
                }
                else
                {
                    if ((importDelegate = context.GetProcAddressNonNative(functionName)) != null)
                    {
                        function.SetValue(null, importDelegate);
                        return;
                    }
                }
            }

            // Load command address (extension features)
            foreach (RequiredByFeatureAttribute extensionFeature in requiredByExtensions)
            {
                string functionName = extensionFeature.EntryPoint ?? defaultName;

                if (context.Native)
                {
                    if ((importAddress = context.GetProcAddress(functionName)) != IntPtr.Zero)
                    {
                        BindNativeAPIFunction(function, importAddress);
                        return;
                    }
                }
                else
                {
                    if ((importDelegate = context.GetProcAddressNonNative(functionName)) != null)
                    {
                        function.SetValue(null, importDelegate);
                        return;
                    }
                }
            }

            // Function not implemented: reset
            function.SetValue(null, null);
        }

        private static void BindNativeAPIFunction(FieldInfo function, IntPtr importAddress)
        {
            Debug.Assert(function != null);
            Debug.Assert(importAddress != IntPtr.Zero);

            Delegate delegatePtr = Marshal.GetDelegateForFunctionPointer(importAddress, function.FieldType);

            Debug.Assert(delegatePtr != null);
            function.SetValue(null, delegatePtr);
        }

        /// <summary>
        /// Determine whether an API command is compatible with the specific API version and extensions registry.
        /// </summary>
        /// <param name="function">
        /// A <see cref="FieldInfo" /> that specifies the command delegate to set. This argument make avail attributes useful
        /// to determine the actual support for this command.
        /// </param>
        /// <param name="version">
        /// The <see cref="KhronosVersion" /> that specifies the API version.
        /// </param>
        /// <param name="extensions">
        /// The <see cref="ExtensionsCollection" /> that specifies the API extensions registry.
        /// </param>
        /// <returns>
        /// It returns a <see cref="bool" /> that specifies whether <paramref name="function" /> is supported by the
        /// API having the version <paramref name="version" /> and the extensions registry <paramref name="extensions" />.
        /// </returns>
        internal static bool IsCompatibleField(FieldInfo function, KhronosVersion version, ExtensionsCollection extensions)
        {
            Debug.Assert(function != null);
            Debug.Assert(version != null);

            IEnumerable<Attribute> attrRequired = Attribute.GetCustomAttributes(function, typeof(RequiredByFeatureAttribute));

            KhronosVersion maxRequiredVersion = null;
            bool required = false, removed = false;

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (RequiredByFeatureAttribute attr in attrRequired)
            {
                // Check for API support
                if (attr.IsSupported(version, extensions) == false)
                    continue;
                // Supported!
                required = true;
                // Keep track of the maximum API version supporting this command
                // Note: useful for resurrected commands after deprecation
                if (maxRequiredVersion == null || maxRequiredVersion < attr.FeatureVersion)
                    maxRequiredVersion = attr.FeatureVersion;
            }

            if (required)
            {
                // Note: indeed the feature could be supported; check whether it is removed
                IEnumerable<Attribute> attrRemoved = Attribute.GetCustomAttributes(function, typeof(RemovedByFeatureAttribute));
                KhronosVersion maxRemovedVersion = null;

                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (RemovedByFeatureAttribute attr in attrRemoved)
                {
                    if (attr.IsRemoved(version, extensions) == false)
                        continue;

                    // Removed!
                    removed = true;
                    // Keep track of the maximum API version removing this command
                    if (maxRemovedVersion == null || maxRemovedVersion < attr.FeatureVersion)
                        maxRemovedVersion = attr.FeatureVersion;
                }

                // Check for resurrection
                if (removed)
                {
                    Debug.Assert(maxRequiredVersion != null);
                    Debug.Assert(maxRemovedVersion != null);

                    if (maxRequiredVersion > maxRemovedVersion)
                        removed = false;
                }

                return removed == false;
            }

            return false;
        }

        #endregion

        #region Extension Support

        /// <summary>
        /// Attribute asserting the extension requiring the underlying member.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public sealed class ExtensionAttribute : Attribute
        {
            /// <summary>
            /// Construct a ExtensionAttribute, specifying the extension name.
            /// </summary>
            /// <param name="extensionName">
            /// A <see cref="string" /> that specifies the name of the extension that requires the element.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Exception thrown if <paramref name="extensionName" /> is null or empty.
            /// </exception>
            public ExtensionAttribute(string extensionName)
            {
                ExtensionName = extensionName;
            }

            /// <summary>
            /// The name of the extension.
            /// </summary>
            public readonly string ExtensionName;

            /// <summary>
            /// </summary>
            public string Api;
        }

        /// <summary>
        /// Attribute asserting the extension requiring the underlying member.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public sealed class CoreExtensionAttribute : Attribute
        {
            #region Constructors

            /// <summary>
            /// Construct a CoreExtensionAttribute specifying the version numbers.
            /// </summary>
            /// <param name="major">
            /// A <see cref="Int32" /> that specifies that major version number.
            /// </param>
            /// <param name="minor">
            /// A <see cref="Int32" /> that specifies that minor version number.
            /// </param>
            /// <param name="api">
            /// A <see cref="string" /> that specifies the API name.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> is less than 0.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Exception thrown if <paramref name="api" /> is null.
            /// </exception>
            public CoreExtensionAttribute(int major, int minor, string api)
            {
                Version = new KhronosVersion(major, minor, 0, api);
            }

            /// <summary>
            /// Construct a CoreExtensionAttribute specifying the version numbers.
            /// </summary>
            /// <param name="major">
            /// A <see cref="Int32" /> that specifies that major version number.
            /// </param>
            /// <param name="minor">
            /// A <see cref="Int32" /> that specifies that minor version number.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Exception thrown if <paramref name="major" /> is less or equals to 0, or if <paramref name="minor" /> is less than 0.
            /// </exception>
            public CoreExtensionAttribute(int major, int minor) :
                this(major, minor, KhronosVersion.API_GL)
            {
            }

            #endregion

            #region Required API Version

            /// <summary>
            /// The required major OpenGL version for supporting the extension.
            /// </summary>
            public readonly KhronosVersion Version;

            #endregion
        }

        /// <summary>
        /// Base class for managing OpenGL extensions.
        /// </summary>
        public abstract class ExtensionsCollection
        {
            /// <summary>
            /// Check whether the specified extension is supported by current platform.
            /// </summary>
            /// <param name="extensionName">
            /// A <see cref="string" /> that specifies the extension name.
            /// </param>
            /// <returns>
            /// It returns a boolean value indicating whether the extension identified with <paramref name="extensionName" />
            /// is supported or not by the current platform.
            /// </returns>
            public bool HasExtensions(string extensionName)
            {
                if (extensionName == null)
                    throw new ArgumentNullException(nameof(extensionName));

                return _extensionsRegistry.ContainsKey(extensionName);
            }

            /// <summary>
            /// Force extension support.
            /// </summary>
            /// <param name="extensionName">
            /// A <see cref="string" /> that specifies the extension name.
            /// </param>
            internal void EnableExtension(string extensionName)
            {
                if (extensionName == null)
                    throw new ArgumentNullException(nameof(extensionName));

                _extensionsRegistry[extensionName] = true;
            }

            /// <summary>
            /// Query the supported extensions.
            /// </summary>
            /// <param name="version">
            /// The <see cref="KhronosVersion" /> that specifies the version of the API context.
            /// </param>
            /// <param name="extensionsString">
            /// A string that specifies the supported extensions, those names are separated by spaces.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Exception thrown if <paramref name="extensionsString" /> is null.
            /// </exception>
            protected void Query(KhronosVersion version, string extensionsString)
            {
                if (extensionsString == null)
                    throw new ArgumentNullException(nameof(extensionsString));

                Query(version, extensionsString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
            }

            /// <summary>
            /// Query the supported extensions.
            /// </summary>
            /// <param name="version">
            /// The <see cref="KhronosVersion" /> that specifies the version of the API context.
            /// </param>
            /// <param name="extensions">
            /// An array of strings that specifies the supported extensions.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Exception thrown if <paramref name="extensions" /> is null.
            /// </exception>
            protected void Query(KhronosVersion version, string[] extensions)
            {
                if (extensions == null)
                    throw new ArgumentNullException(nameof(extensions));

                // Cache extension names in registry
                _extensionsRegistry.Clear();
                foreach (string extension in extensions)
                {
                    if (!_extensionsRegistry.ContainsKey(extension))
                        _extensionsRegistry.Add(extension, true);
                }

                // Sync fields
                SyncMembers(version);
            }

            /// <summary>
            /// Set all fields of this ExtensionsCollection, depending on current extensions.
            /// </summary>
            /// <param name="version">
            /// The <see cref="KhronosVersion" /> that specifies the context version/API. It can be null.
            /// </param>
            protected internal void SyncMembers(KhronosVersion version)
            {
                Type thisType = GetType();
                IEnumerable<FieldInfo> thisTypeFields = thisType.GetTypeInfo().DeclaredFields;

                foreach (FieldInfo fieldInfo in thisTypeFields)
                {
                    // Check boolean field (defensive)
                    // Debug.Assert(fieldInfo.FieldType == typeof(bool));
                    if (fieldInfo.FieldType != typeof(bool))
                        continue;

                    var support = false;

                    // Support by extension
                    IEnumerable<Attribute> extensionAttributes = fieldInfo.GetCustomAttributes(typeof(ExtensionAttribute));
                    // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                    foreach (ExtensionAttribute extensionAttribute in extensionAttributes)
                    {
                        if (!_extensionsRegistry.ContainsKey(extensionAttribute.ExtensionName))
                            continue;
                        if (version != null && version.Api != null && extensionAttribute.Api != null && !Regex.IsMatch(version.Api, "^" + extensionAttribute.Api + "$"))
                            continue;

                        support = true;
                        break;
                    }

                    // Support by version
                    if (version != null && support == false)
                    {
                        IEnumerable<Attribute> coreAttributes = fieldInfo.GetCustomAttributes(typeof(CoreExtensionAttribute));
                        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                        foreach (CoreExtensionAttribute coreAttribute in coreAttributes)
                        {
                            if (version.Api != coreAttribute.Version.Api || version < coreAttribute.Version)
                                continue;

                            support = true;
                            break;
                        }
                    }

                    fieldInfo.SetValue(this, support);
                }
            }

            /// <summary>
            /// Registry of supported extensions.
            /// </summary>
            private readonly Dictionary<string, bool> _extensionsRegistry = new Dictionary<string, bool>();
        }

        #endregion

        #region Command Checking

        /// <summary>
        /// Check whether commands implemented by the current driver have a corresponding extension declaring the
        /// support of them.
        /// </summary>
        /// <param name="version">
        /// The <see cref="KhronosVersion" /> currently implemented by the current context on this thread.
        /// </param>
        /// <param name="extensions">
        /// The <see cref="ExtensionsCollection" /> that specifies the extensions supported by the driver.
        /// </param>
        /// <param name="enableExtensions"></param>
        protected static void CheckExtensionCommands(KhronosVersion version, ExtensionsCollection extensions, bool enableExtensions)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));

            var hiddenVersions = new Dictionary<string, List<Type>>();
            var hiddenExtensions = new Dictionary<string, bool>();

            foreach (FieldInfo fi in _functionContext.Delegates)
            {
                var fiDelegateType = (Delegate) fi.GetValue(null);
                bool commandDefined = fiDelegateType != null;
                var supportedByFeature = false;

                // Get the delegate type
                Type delegateType = fi.DeclaringType?.GetNestedType(fi.Name.Substring(1), BindingFlags.Public | BindingFlags.NonPublic);
                if (delegateType == null)
                    continue; // Support fields names not in sync with delegate types
                // TODO Why not use 'fi' directly for getting attributes? They should be in sync
                IEnumerable<object> requiredByFeatureAttributes = delegateType.GetCustomAttributes(typeof(RequiredByFeatureAttribute), false);

                foreach (RequiredByFeatureAttribute requiredByFeatureAttribute in requiredByFeatureAttributes)
                {
                    supportedByFeature |= requiredByFeatureAttribute.IsSupported(version, extensions);
                }

                // Find the underlying extension
                RequiredByFeatureAttribute hiddenVersionAttrib = null;
                RequiredByFeatureAttribute hiddenExtensionAttrib = null;

                foreach (RequiredByFeatureAttribute requiredByFeatureAttribute in requiredByFeatureAttributes)
                {
                    if (requiredByFeatureAttribute.IsSupportedApi(version.Api) == false)
                    {
                        // Version attribute
                        if (hiddenVersionAttrib == null)
                            hiddenVersionAttrib = requiredByFeatureAttribute;
                    }
                    else
                    {
                        // Extension attribute
                        if (hiddenExtensionAttrib == null)
                            hiddenExtensionAttrib = requiredByFeatureAttribute;
                    }
                }

                if (commandDefined != supportedByFeature)
                    if (commandDefined)
                    {
                        if (hiddenVersionAttrib != null && hiddenExtensionAttrib == null)
                        {
                            if (hiddenVersions.TryGetValue(hiddenVersionAttrib.FeatureName, out List<Type> versionDelegates) == false)
                                hiddenVersions.Add(hiddenVersionAttrib.FeatureName, versionDelegates = new List<Type>());
                            versionDelegates.Add(delegateType);
                        }

                        if (hiddenExtensionAttrib != null) // Eventually leave to false for incomplete extensions
                            if (hiddenExtensions.ContainsKey(hiddenExtensionAttrib.FeatureName) == false)
                                hiddenExtensions.Add(hiddenExtensionAttrib.FeatureName, true);
                    }

                // Partial extensions are not supported
                if (hiddenExtensionAttrib != null && commandDefined == false && hiddenExtensions.ContainsKey(hiddenExtensionAttrib.FeatureName))
                    hiddenExtensions[hiddenExtensionAttrib.FeatureName] = false;
            }

            if (!enableExtensions) return;

            var sync = false;

            foreach (KeyValuePair<string, bool> hiddenExtension in hiddenExtensions.Where(hiddenExtension => hiddenExtension.Value))
            {
                extensions.EnableExtension(hiddenExtension.Key);
                sync = true;
            }

            if (sync)
                extensions.SyncMembers(version);
        }

        #endregion
    }
}
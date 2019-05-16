#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A function used to initialize (not create) a new module object.
    /// </summary>
    /// <param name="module">The module to initialize.</param>
    /// <returns>FreeType error code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Error ModuleConstructor(NativeReference<Module> module);

    /// <summary>
    /// A function used to finalize (not destroy) a given module object.
    /// </summary>
    /// <param name="module">The module to finalize.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ModuleDestructor(NativeReference<Module> module);

    /// <summary>
    /// A function used to query a given module for a specific interface.
    /// </summary>
    /// <param name="module">The module that contains the interface.</param>
    /// <param name="name">The name of the interface in the module.</param>
    /// <returns>The interface.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr ModuleRequester(NativeReference<Module> module, [MarshalAs(UnmanagedType.LPStr)] string name);

    /// <summary>
    /// The module class descriptor.
    /// </summary>
    public class ModuleClass : NativeObject
    {
        #region Fields

        private ModuleClassRec rec;

        #endregion

        #region Constructors

        internal ModuleClass(IntPtr reference) : base(reference)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets bit flags describing the module.
        /// </summary>

        public uint Flags
        {
            get => rec.module_flags;
        }

        /// <summary>
        /// Gets the size of one module object/instance in bytes.
        /// </summary>
        public int Size
        {
            get => (int) rec.module_size;
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name
        {
            get => rec.module_name;
        }

        /// <summary>
        /// Gets the version, as a 16.16 fixed number (major.minor).
        /// </summary>
        public Fixed16Dot16 Version
        {
            get => Fixed16Dot16.FromRawValue((int) rec.module_version);
        }

        /// <summary>
        /// Gets the version of FreeType this module requires, as a 16.16 fixed number (major.minor). Starts at version
        /// 2.0, i.e., 0x20000.
        /// </summary>
        public Fixed16Dot16 Requires
        {
            get => Fixed16Dot16.FromRawValue((int) rec.module_requires);
        }

        /// <summary>
        /// Get the module interface.
        /// </summary>
        public IntPtr Interface
        {
            get => rec.module_interface;
        }

        /// <summary>
        /// Gets the initializing function.
        /// </summary>
        public ModuleConstructor Init
        {
            get => rec.module_init;
        }

        /// <summary>
        /// Gets the finalizing function.
        /// </summary>
        public ModuleDestructor Done
        {
            get => rec.module_done;
        }

        /// <summary>
        /// Gets the interface requesting function.
        /// </summary>
        public ModuleRequester GetInterface
        {
            get => rec.get_interface;
        }

        internal override IntPtr Reference
        {
            get => base.Reference;

            set
            {
                base.Reference = value;
                rec = PInvokeHelper.PtrToStructure<ModuleClassRec>(value);
            }
        }

        #endregion
    }
}
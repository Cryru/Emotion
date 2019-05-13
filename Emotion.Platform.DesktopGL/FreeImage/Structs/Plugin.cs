#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace FreeImageAPI.Plugins
{
    /// <summary>
    /// The structure contains functionpointers that make up a FreeImage plugin.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Plugin
    {
        /// <summary>
        /// Delegate to a function that returns a string which describes
        /// the plugins format.
        /// </summary>
        public FormatProc formatProc;

        /// <summary>
        /// Delegate to a function that returns a string which contains
        /// a more detailed description.
        /// </summary>
        public DescriptionProc descriptionProc;

        /// <summary>
        /// Delegate to a function that returns a comma seperated list
        /// of file extensions the plugin can read or write.
        /// </summary>
        public ExtensionListProc extensionListProc;

        /// <summary>
        /// Delegate to a function that returns a regular expression that
        /// can be used to idientify whether a file can be handled by the plugin.
        /// </summary>
        public RegExprProc regExprProc;

        /// <summary>
        /// Delegate to a function that opens a file.
        /// </summary>
        public OpenProc openProc;

        /// <summary>
        /// Delegate to a function that closes a previosly opened file.
        /// </summary>
        public CloseProc closeProc;

        /// <summary>
        /// Delegate to a function that returns the number of pages of a multipage
        /// bitmap if the plugin is capable of handling multipage bitmaps.
        /// </summary>
        public PageCountProc pageCountProc;

        /// <summary>
        /// UNKNOWN
        /// </summary>
        public PageCapabilityProc pageCapabilityProc;

        /// <summary>
        /// Delegate to a function that loads and decodes a bitmap into memory.
        /// </summary>
        public LoadProc loadProc;

        /// <summary>
        /// Delegate to a function that saves a bitmap.
        /// </summary>
        public SaveProc saveProc;

        /// <summary>
        /// Delegate to a function that determines whether the source is a valid image.
        /// </summary>
        public ValidateProc validateProc;

        /// <summary>
        /// Delegate to a function that returns a string which contains
        /// the plugin's mime type.
        /// </summary>
        public MimeProc mimeProc;

        /// <summary>
        /// Delegate to a function that returns whether the plugin can handle the
        /// specified color depth.
        /// </summary>
        public SupportsExportBPPProc supportsExportBPPProc;

        /// <summary>
        /// Delegate to a function that returns whether the plugin can handle the
        /// specified image type.
        /// </summary>
        public SupportsExportTypeProc supportsExportTypeProc;

        /// <summary>
        /// Delegate to a function that returns whether the plugin can handle
        /// ICC-Profiles.
        /// </summary>
        public SupportsICCProfilesProc supportsICCProfilesProc;
    }
}
#region Using

using System;
using System.Diagnostics;

#endregion

namespace FreeImageAPI.Plugins
{
    /// <summary>
    /// Class representing a FreeImage format.
    /// </summary>
    public sealed class FreeImagePlugin
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="fif">The FreeImage format to wrap.</param>
        internal FreeImagePlugin(FREE_IMAGE_FORMAT fif)
        {
            FIFormat = fif;
        }

        /// <summary>
        /// Gets the format of this instance.
        /// </summary>
        [field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public FREE_IMAGE_FORMAT FIFormat { get; }

        /// <summary>
        /// Gets or sets whether this plugin is enabled.
        /// </summary>
        public bool Enabled
        {
            get => FreeImage.IsPluginEnabled(FIFormat) == 1;
            set => FreeImage.SetPluginEnabled(FIFormat, value);
        }

        /// <summary>
        /// Gets a string describing the format.
        /// </summary>
        public string Format
        {
            get => FreeImage.GetFormatFromFIF(FIFormat);
        }

        /// <summary>
        /// Gets a comma-delimited file extension list describing the bitmap formats
        /// this plugin can read and/or write.
        /// </summary>
        public string ExtentsionList
        {
            get => FreeImage.GetFIFExtensionList(FIFormat);
        }

        /// <summary>
        /// Gets a descriptive string that describes the bitmap formats
        /// this plugin can read and/or write.
        /// </summary>
        public string Description
        {
            get => FreeImage.GetFIFDescription(FIFormat);
        }

        /// <summary>
        /// Returns a regular expression string that can be used by
        /// a regular expression engine to identify the bitmap.
        /// FreeImageQt makes use of this function.
        /// </summary>
        public string RegExpr
        {
            get => FreeImage.GetFIFRegExpr(FIFormat);
        }

        /// <summary>
        /// Gets whether this plugin can load bitmaps.
        /// </summary>
        public bool SupportsReading
        {
            get => FreeImage.FIFSupportsReading(FIFormat);
        }

        /// <summary>
        /// Gets whether this plugin can save bitmaps.
        /// </summary>
        public bool SupportsWriting
        {
            get => FreeImage.FIFSupportsWriting(FIFormat);
        }

        /// <summary>
        /// Checks whether this plugin can save a bitmap in the desired data type.
        /// </summary>
        /// <param name="type">The desired image type.</param>
        /// <returns>True if this plugin can save bitmaps as the desired type, else false.</returns>
        public bool SupportsExportType(FREE_IMAGE_TYPE type)
        {
            return FreeImage.FIFSupportsExportType(FIFormat, type);
        }

        /// <summary>
        /// Checks whether this plugin can save bitmaps in the desired bit depth.
        /// </summary>
        /// <param name="bpp">The desired bit depth.</param>
        /// <returns>True if this plugin can save bitmaps in the desired bit depth, else false.</returns>
        public bool SupportsExportBPP(int bpp)
        {
            return FreeImage.FIFSupportsExportBPP(FIFormat, bpp);
        }

        /// <summary>
        /// Gets whether this plugin can load or save an ICC profile.
        /// </summary>
        public bool SupportsICCProfiles
        {
            get => FreeImage.FIFSupportsICCProfiles(FIFormat);
        }

        /// <summary>
        /// Checks whether an extension is valid for this format.
        /// </summary>
        /// <param name="extension">The desired extension.</param>
        /// <returns>True if the extension is valid for this format, false otherwise.</returns>
        public bool ValidExtension(string extension)
        {
            return FreeImage.IsExtensionValidForFIF(FIFormat, extension);
        }

        /// <summary>
        /// Checks whether an extension is valid for this format.
        /// </summary>
        /// <param name="extension">The desired extension.</param>
        /// <param name="comparisonType">The string comparison type.</param>
        /// <returns>True if the extension is valid for this format, false otherwise.</returns>
        public bool ValidExtension(string extension, StringComparison comparisonType)
        {
            return FreeImage.IsExtensionValidForFIF(FIFormat, extension, comparisonType);
        }

        /// <summary>
        /// Checks whether a filename is valid for this format.
        /// </summary>
        /// <param name="filename">The desired filename.</param>
        /// <returns>True if the filename is valid for this format, false otherwise.</returns>
        public bool ValidFilename(string filename)
        {
            return FreeImage.IsFilenameValidForFIF(FIFormat, filename);
        }

        /// <summary>
        /// Checks whether a filename is valid for this format.
        /// </summary>
        /// <param name="filename">The desired filename.</param>
        /// <param name="comparisonType">The string comparison type.</param>
        /// <returns>True if the filename is valid for this format, false otherwise.</returns>
        public bool ValidFilename(string filename, StringComparison comparisonType)
        {
            return FreeImage.IsFilenameValidForFIF(FIFormat, filename, comparisonType);
        }

        /// <summary>
        /// Gets a descriptive string that describes the bitmap formats
        /// this plugin can read and/or write.
        /// </summary>
        /// <returns>A descriptive string that describes the bitmap formats.</returns>
        public override string ToString()
        {
            return Description;
        }
    }
}
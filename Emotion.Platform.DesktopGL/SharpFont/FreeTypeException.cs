#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// An exception that gets thrown when FreeType returns an error code.
    /// </summary>
    public class FreeTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FreeTypeException" /> class.
        /// </summary>
        /// <param name="error">The error returned by FreeType.</param>
        public FreeTypeException(Error error)
            : base("FreeType error: " + GetErrorMessage(error))
        {
            Error = error;
        }

        /// <summary>
        /// Gets the FreeType error code that caused the exception.
        /// </summary>
        public Error Error { get; }

        private static string GetErrorMessage(Error err)
        {
            switch (err)
            {
                case Error.Ok: return "No error.";
                case Error.CannotOpenResource: return "Cannot open resource.";
                case Error.UnknownFileFormat: return "Unknown file format.";
                case Error.InvalidFileFormat: return "Broken file.";
                case Error.InvalidVersion: return "Invalid FreeType version.";
                case Error.LowerModuleVersion: return "Module version is too low.";
                case Error.InvalidArgument: return "Invalid argument.";
                case Error.UnimplementedFeature: return "Unimplemented feature.";
                case Error.InvalidTable: return "Broken table.";
                case Error.InvalidOffset: return "Broken offset within table.";
                case Error.ArrayTooLarge: return "Array allocation size too large.";
                case Error.InvalidGlyphIndex: return "Invalid glyph index.";
                case Error.InvalidCharacterCode: return "Invalid character code.";
                case Error.InvalidGlyphFormat: return "Unsupported glyph image format.";
                case Error.CannotRenderGlyph: return "Cannot render this glyph format.";
                case Error.InvalidOutline: return "Invalid outline.";
                case Error.InvalidComposite: return "Invalid composite glyph.";
                case Error.TooManyHints: return "Too many hints.";
                case Error.InvalidPixelSize: return "Invalid pixel size.";
                case Error.InvalidHandle: return "Invalid object handle.";
                case Error.InvalidLibraryHandle: return "Invalid library handle.";
                case Error.InvalidDriverHandle: return "Invalid module handle.";
                case Error.InvalidFaceHandle: return "Invalid face handle.";
                case Error.InvalidSizeHandle: return "Invalid size handle.";
                case Error.InvalidSlotHandle: return "Invalid glyph slot handle.";
                case Error.InvalidCharMapHandle: return "Invalid charmap handle.";
                case Error.InvalidCacheHandle: return "Invalid cache manager handle.";
                case Error.InvalidStreamHandle: return "Invalid stream handle.";
                case Error.TooManyDrivers: return "Too many modules.";
                case Error.TooManyExtensions: return "Too many extensions.";
                case Error.OutOfMemory: return "Out of memory.";
                case Error.UnlistedObject: return "Unlisted object.";
                case Error.CannotOpenStream: return "Cannot open stream.";
                case Error.InvalidStreamSeek: return "Invalid stream seek.";
                case Error.InvalidStreamSkip: return "Invalid stream skip.";
                case Error.InvalidStreamRead: return "Invalid stream read.";
                case Error.InvalidStreamOperation: return "Invalid stream operation.";
                case Error.InvalidFrameOperation: return "Invalid frame operation.";
                case Error.NestedFrameAccess: return "Nested frame access.";
                case Error.InvalidFrameRead: return "Invalid frame read.";
                case Error.RasterUninitialized: return "Raster uninitialized.";
                case Error.RasterCorrupted: return "Raster corrupted.";
                case Error.RasterOverflow: return "Raster overflow.";
                case Error.RasterNegativeHeight: return "Negative height while rastering.";
                case Error.TooManyCaches: return "Too many registered caches.";
                case Error.InvalidOpCode: return "Invalid opcode.";
                case Error.TooFewArguments: return "Too few arguments.";
                case Error.StackOverflow: return "Stack overflow.";
                case Error.CodeOverflow: return "Code overflow.";
                case Error.BadArgument: return "Bad argument.";
                case Error.DivideByZero: return "Division by zero.";
                case Error.InvalidReference: return "Invalid reference.";
                case Error.DebugOpCode: return "Found debug opcode.";
                case Error.EndfInExecStream: return "Found ENDF opcode in execution stream.";
                case Error.NestedDefs: return "Nested DEFS.";
                case Error.InvalidCodeRange: return "Invalid code range.";
                case Error.ExecutionTooLong: return "Execution context too long.";
                case Error.TooManyFunctionDefs: return "Too many function definitions.";
                case Error.TooManyInstructionDefs: return "Too many instruction definitions.";
                case Error.TableMissing: return "SFNT font table missing.";
                case Error.HorizHeaderMissing: return "Horizontal header (hhea) table missing.";
                case Error.LocationsMissing: return "Locations (loca) table missing.";
                case Error.NameTableMissing: return "Name table missing.";
                case Error.CMapTableMissing: return "Character map (cmap) table missing.";
                case Error.HmtxTableMissing: return "Horizontal metrics (hmtx) table missing.";
                case Error.PostTableMissing: return "PostScript (post) table missing.";
                case Error.InvalidHorizMetrics: return "Invalid horizontal metrics.";
                case Error.InvalidCharMapFormat: return "Invalid character map (cmap) format.";
                case Error.InvalidPPem: return "Invalid ppem value.";
                case Error.InvalidVertMetrics: return "Invalid vertical metrics.";
                case Error.CouldNotFindContext: return "Could not find context.";
                case Error.InvalidPostTableFormat: return "Invalid PostScript (post) table format.";
                case Error.InvalidPostTable: return "Invalid PostScript (post) table.";
                case Error.SyntaxError: return "Opcode syntax error.";
                case Error.StackUnderflow: return "Argument stack underflow.";
                case Error.Ignore: return "Ignore this error.";
                case Error.NoUnicodeGlyphName: return "No Unicode glyph name found.";
                case Error.MissingStartfontField: return "`STARTFONT' field missing.";
                case Error.MissingFontField: return "`FONT' field missing.";
                case Error.MissingSizeField: return "`SIZE' field missing.";
                case Error.MissingFontboudingboxField: return "`FONTBOUNDINGBOX' field missing.";
                case Error.MissingCharsField: return "`CHARS' field missing.";
                case Error.MissingStartcharField: return "`STARTCHAR' field missing.";
                case Error.MissingEncodingField: return "`ENCODING' field missing.";
                case Error.MissingBbxField: return "`BBX' field missing.";
                case Error.BbxTooBig: return "`BBX' too big.";
                case Error.CorruptedFontHeader: return "Font header corrupted or missing fields.";
                case Error.CorruptedFontGlyphs: return "Font glyphs corrupted or missing fields.";
                default: return "Encountered an unknown error. Most likely this is a new error that hasn't been included in SharpFont yet. Error:" + (int) err;
            }
        }
    }
}
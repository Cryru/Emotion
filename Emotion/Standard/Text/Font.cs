#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.Text.FontTables;
using Emotion.Standard.Utility;

#if StbTrueType
using Emotion.Primitives;
using StbTrueTypeSharp;
#endif

#if FreeType
using System.IO;
using System.Reflection;
#endif

#endregion

namespace Emotion.Standard.Text
{
    /// <summary>
    /// Represents an OpenType font.
    /// </summary>
    public class Font
    {
        /// <summary>
        /// Whether the font was successfully parsed.
        /// </summary>
        public bool Valid;

        #region Meta

        /// <summary>
        /// The font's family name.
        /// </summary>
        public string FontFamily;

        /// <summary>
        /// The font's sub family name.
        /// </summary>
        public string FontSubFamily;

        /// <summary>
        /// The full name of the font.
        /// </summary>
        public string FullName;

        /// <summary>
        /// The font's version.
        /// </summary>
        public string Version;

        /// <summary>
        /// The font's copyright.
        /// </summary>
        public string Copyright;

        /// <summary>
        /// The font's uniqueId.
        /// </summary>
        public string UniqueId;

        #endregion

        /// <summary>
        /// The font's format.
        /// This will be either "truetype" or "cff".
        /// </summary>
        public string Format { get; protected set; }

        /// <summary>
        /// Glyphs found in the font.
        /// </summary>
        public Glyph[] Glyphs;

        /// <summary>
        /// The character index of the first glyph.
        /// </summary>
        public uint FirstCharIndex { get; protected set; }

        /// <summary>
        /// The largest character index found in the font.
        /// </summary>
        public uint LastCharIndex { get; protected set; }

        /// <summary>
        /// The font's ascender minus its descender. Is used as the distance between lines and is regarded as the safe space.
        /// </summary>
        public int Height
        {
            get => Ascender - Descender;
        }

        /// <summary>
        /// The highest a glyph can reach.
        /// </summary>
        public short Ascender { get; protected set; }

        /// <summary>
        /// The lowest a glyph can reach.
        /// </summary>
        public short Descender { get; protected set; }

        /// <summary>
        /// The font's defined resolution. Is used for scaling glyphs.
        /// </summary>
        public ushort UnitsPerEm { get; protected set; }

        #region Parsing

        /// <summary>
        /// The font's magic tag from which the format was inferred.
        /// Generally this is useless to users.
        /// </summary>
        public string Tag { get; protected set; }

        /// <summary>
        /// Font tables found within the font.
        /// After the initial parsing these aren't used.
        /// </summary>
        public List<FontTable> Tables = new List<FontTable>();

        /// <summary>
        /// The number of horizontal metrics found in the font.
        /// </summary>
        public ushort NumberOfHMetrics { get; protected set; }

        #endregion

#if FreeType
        private static IntPtr _freeTypeLib;
        private static dynamic _freeTypeLibrary;
        private static Type _freeTypeFaceType;
        private object _freeTypeFace;

        private static MethodInfo _setCharSizeMethod;
        private static MethodInfo _renderGlyphMethod;
        private static PropertyInfo _facePropertySize;

        public Font(byte[] fontData, bool freeTypeRasterizer = true)
        {
            if (freeTypeRasterizer)
            {
                try
                {
                    if (_freeTypeLib == IntPtr.Zero)
                    {
                        Assembly freeTypeSupportAssembly = Assembly.LoadFrom(Path.Combine("Standard", "Text", "Freetype", "Emotion.Standard.FreeType.dll"));
                        MethodInfo initFunc = freeTypeSupportAssembly.GetType("SharpFont.FT").GetMethod("Init");
                        _freeTypeLib = Engine.Host.LoadLibrary(Path.Combine("Standard", "Text", "Freetype", "freetype6"));
                        if (_freeTypeLib != IntPtr.Zero)
                        {
                            initFunc?.Invoke(null, new object[] { _freeTypeLib });
                            Type freeTypeWrapperType = freeTypeSupportAssembly.GetType("Emotion.Standard.FreeType.Wrapper");
                            Type freeTypeLibraryType = freeTypeSupportAssembly.GetType("SharpFont.Library");
                            _freeTypeLibrary = Activator.CreateInstance(freeTypeLibraryType);

                            _freeTypeFaceType = freeTypeSupportAssembly.GetType("SharpFont.Face");
                            _facePropertySize = _freeTypeFaceType.GetProperty("Size");

                            _setCharSizeMethod = freeTypeWrapperType?.GetMethod("SetCharSize");
                            _renderGlyphMethod = freeTypeWrapperType?.GetMethod("RenderGlyphDefaultOptions");
                        }
                        else
                        {
                            return;
                        }
                    }

                    _freeTypeFace = Activator.CreateInstance(_freeTypeFaceType, _freeTypeLibrary, fontData, 0);
                }
                catch (Exception)
                {
                    // Suppress errors.
                    Engine.Log.Error("Couldn't load FreeType.", "Emotion.Standard.FreeType");
                }
            }
#else
        public Font(byte[] fontData)
        {
#endif

            // Note: OpenType fonts use big endian byte ordering.
            using var r = new ByteReader(fontData);
            Tag = new string(r.ReadChars(4));

            switch (Tag)
            {
                case "\0\u0001\0\0":
                case "true":
                case "typ1":
                {
                    Format = "truetype";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    Tables = GetTables(r, numTables);
                    break;
                }

                case "OTTO":
                {
                    Format = "cff";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    Tables = GetTables(r, numTables);
                    break;
                }
                case "wOFF":
                {
                    Format = new string(r.ReadChars(4)) == "OTTO" ? "cff" : "truetype";
                    r.ReadBytes(4);
                    r.ReadUShortBE();
                    // todo: Read wOFF tables.
                    break;
                }
            }

            // Header
            FontTable table = GetTable("head");
            short indexToLocFormat;
            if (table != null)
            {
                var head = new Head(r.Branch(table.Offset, true, table.Length));
                UnitsPerEm = head.UnitsPerEm;
                indexToLocFormat = head.IndexToLocFormat;
            }
            else
            {
                Engine.Log.Warning("Font head table not found.", MessageSource.FontParser);
                return;
            }

            // Horizontal header - information about horizontal layout of glyphs.
            table = GetTable("hhea");
            if (table != null)
            {
                var hhea = new Hhea(r.Branch(table.Offset, true, table.Length));

                Ascender = hhea.Ascender;
                Descender = hhea.Descender;
                NumberOfHMetrics = hhea.NumberOfHMetrics;
            }
            else
            {
                Engine.Log.Warning("Font hhea table not found.", MessageSource.FontParser);
                return;
            }

            // Name table - contains meta information about the font.
            table = GetTable("name");
            if (table != null)
            {
                // todo: ltag parsing
                Dictionary<string, Dictionary<string, string>> names = Name.ParseName(r.Branch(table.Offset, true, table.Length), null);

                FontFamily = Name.GetDefaultValue(names, "fontFamily");
                FontSubFamily = Name.GetDefaultValue(names, "fontSubfamily");
                FullName = Name.GetDefaultValue(names, "fullName");
                Version = Name.GetDefaultValue(names, "version");
                Copyright = Name.GetDefaultValue(names, "copyright");
                UniqueId = Name.GetDefaultValue(names, "uniqueID");
            }
            else
            {
                Engine.Log.Warning("Font name table not found.", MessageSource.FontParser);
                return;
            }

            // MaxP - contains information about the font's memory footprint.
            table = GetTable("maxp");
            ushort numGlyphs;
            if (table != null)
            {
                var maxp = new Maxp(r.Branch(table.Offset, true, table.Length));
                numGlyphs = maxp.NumGlyphs;
            }
            else
            {
                Engine.Log.Warning("Font maxp table not found.", MessageSource.FontParser);
                return;
            }

            // Glyf - glyph data.
            // Also reads loca for locations, cmap, and post for glyph names.
            table = GetTable("glyf");
            if (table != null)
            {
                bool shortVersion = indexToLocFormat == 0;
                FontTable locaTable = GetTable("loca");
                int[] locaOffsets = Loca.ParseLoca(r.Branch(locaTable.Offset, true, locaTable.Length), numGlyphs, shortVersion);

                Glyphs = Glyf.ParseGlyf(r.Branch(table.Offset, true, table.Length), locaOffsets);

                // Add glyph names.
                FontTable cmapTable = GetTable("cmap");
                FontTable postTable = GetTable("post");
                if (cmapTable != null && postTable != null)
                {
                    var cMap = new CMap(r.Branch(cmapTable.Offset, true, cmapTable.Length));
                    var post = new Post(r.Branch(postTable.Offset, true, postTable.Length));
                    if (cMap.GlyphIndexMap != null)
                        AddGlyphNames(ref Glyphs, cMap.GlyphIndexMap, post.Names);
                }
            }
            else
            {
                table = GetTable("CFF ");
                if (table == null)
                {
                    Engine.Log.Warning("Font - neither glyf nor cff table found.", MessageSource.FontParser);
                    return;
                }

                var cff = new Cff(r.Branch(table.Offset, true, table.Length));
                var cffGlyphs = new List<Glyph>();

                FontTable cmapTable = GetTable("cmap");
                if (cmapTable != null)
                {
                    // Using Cmap encoding.
                    var cMap = new CMap(r.Branch(cmapTable.Offset, true, cmapTable.Length));
                    foreach (KeyValuePair<uint, uint> glyph in cMap.GlyphIndexMap)
                    {
                        Glyph g = cff.CffGlyphLoad((int) glyph.Value);
                        g.Name = cff.Charset[(int) glyph.Value];
                        g.MapIndex = glyph.Value;
                        g.CharIndex = glyph.Key;
                        cffGlyphs.Add(g);
                    }

                    Glyphs = cffGlyphs.ToArray();
                }
                else
                {
                    // Using CFF encoding.
                    // todo
                    Debug.Assert(false);
                    for (var i = 0; i < cff.NumberOfGlyphs; i++)
                    {
                        byte[] s = cff.CharStringIndex[i];
                    }
                }
            }

            // Add metrics.
            table = GetTable("hmtx");
            if (table != null) Hmtx.ParseHmtx(r.Branch(table.Offset, true, table.Length), NumberOfHMetrics, Glyphs);

            // os/2 parsed, but unused
            // cvt parsed, but unused
            // todo: kern
            // todo: gsub
            // todo: gpos
            // todo: fvar
            // todo: meta

            // Calculate last char index.
            LastCharIndex = Glyphs.Max(x => x.CharIndex);
            Array.Sort(Glyphs, _comparer); // todo: Check if all parse paths have them sorted. Some do.
            FirstCharIndex = Glyphs[0].CharIndex;

            // Check if a space glyph exists, and if not add one.
            var hasSpace = false;
            var nonBreakingSpaceIndex = 0;
            var spaceIndex = 0;
            for (var i = 0; i < Glyphs.Length; i++)
            {
                Glyph g = Glyphs[i];

                // If a space character is found, all is well.
                if (g.CharIndex == ' ')
                {
                    hasSpace = true;
                    break;
                }

                // If no space is found, we can synthesize one from the non-breaking space char - 160.
                if (g.CharIndex == 160)
                {
                    nonBreakingSpaceIndex = i;
                    break;
                }

                // Find where the space should go.
                if (g.CharIndex < ' ') spaceIndex++;
            }

            if (!hasSpace)
            {
                Engine.Log.Warning("Font didn't have a space glyph.", MessageSource.FontParser);
                var fakeSpace = new Glyph
                {
                    Name = "fake-space",
                    CharIndex = ' ',
                    MapIndex = Glyphs[nonBreakingSpaceIndex].MapIndex,
                    AdvanceWidth = Glyphs[nonBreakingSpaceIndex].AdvanceWidth,
                    Vertices = new GlyphVertex[0]
                };
                List<Glyph> asList = Glyphs.ToList();
                asList.Insert(spaceIndex, fakeSpace);
                Glyphs = asList.ToArray();
            }

            Valid = true;
        }

        private static GlyphCompare _comparer = new GlyphCompare();

        private class GlyphCompare : Comparer<Glyph>
        {
            public override int Compare(Glyph x, Glyph y)
            {
                if (x == null || y == null) return 0;
                return (int) x.CharIndex - (int) y.CharIndex;
            }
        }

        #region Atlas Rasterization

        public enum GlyphRasterizer
        {
            /// <summary>
            /// The default renderer.
            /// Based on the Stb renderer, but has some differences.
            /// If everything is well it should produce the same results as the Stb renderer.
            /// Fastest
            /// </summary>
            Emotion,
#if StbTrueType
            /// <summary>
            /// A more mature rasterizer to be used if the Emotion rasterizer produces bugs/unwanted results.
            /// Is sort of a fallback.
            /// Fast
            /// </summary>
            StbTrueType,
#endif
#if FreeType
            /// <summary>
            /// The most mature and advanced renderer, but it isn't portable as it is a native library.
            /// Emotion.Standard includes the MacOS64, Windows64 and Linux64 freetype libraries, but this
            /// should be used for reference only. Please don't use this.
            ///
            /// Additionally this rasterizer will reload the font as the information parsed from Emotion is not
            /// transferable.
            ///
            /// The rasterizer isn't slow itself, but due to how everything is implemented this is the slowest option.
            /// </summary>
            FreeType
#endif
        }

        public FontAtlas GetAtlas(float fontSize, uint firstChar = 0, int numChars = -1, GlyphRasterizer rasterizer = GlyphRasterizer.Emotion)
        {
            if (Glyphs == null || Glyphs.Length == 0) return null;
            if (firstChar < FirstCharIndex) firstChar = FirstCharIndex;
            if (numChars == -1) numChars = (int) (LastCharIndex - firstChar);

            // The scale to render at.
            float scale = fontSize / Height;
            var glyphRenders = new List<Task<GlyphRenderer.GlyphCanvas>>();

            const int samples = 1;

            // Go through all glyphs who are assumed to be ordered by char index.
            // Start from the requested first index and go until the processed char index is either above the
            // start plus the size, or we run out of glyphs.
            uint glyphIndex = 0;
            uint lastCharIndex = firstChar;
            while (lastCharIndex <= firstChar + numChars)
            {
                // Verify that the index is valid.
                if (glyphIndex >= Glyphs.Length) break;
                Glyph g = Glyphs[glyphIndex];
                lastCharIndex = g.CharIndex;

                // Start rendering this glyph.
                switch (rasterizer)
                {
                    case GlyphRasterizer.Emotion:
                        glyphRenders.Add(Task.Run(() => RenderGlyph(this, g, scale, samples)));
                        break;
#if StbTrueType
                    case GlyphRasterizer.StbTrueType:
                        glyphRenders.Add(Task.Run(() => RenderGlyphStb(this, g, scale)));
                        break;
#endif

#if FreeType
                    case GlyphRasterizer.FreeType:
                        glyphRenders.Add(Task.Run(() => RenderGlyphFreeType(g, fontSize)));
                        break;
#endif
                }

                glyphIndex++;
            }

            // Get rendered canvases.
            GlyphRenderer.GlyphCanvas[] canvases = Task.WhenAll(glyphRenders).Result;
            const int glyphSpacing = 2;

            // The location of the brush within the bitmap.
            var pen = new Vector2(glyphSpacing, glyphSpacing);

            // Determine size of the atlas texture based on the largest atlases.
            int glyphCountSqrt = canvases.Length > 0 ? (int) Math.Ceiling(Math.Sqrt(canvases.Length)) : 0;
            int atlasWidth = canvases.Length > 0 ? glyphCountSqrt * (canvases.Max(x => x.Width) + glyphSpacing * 2) : 0;
            int atlasHeight = canvases.Length > 0 ? glyphCountSqrt * (canvases.Max(x => x.Height) + glyphSpacing * 2) : 0;
            int atlasSize = Math.Max(atlasWidth, atlasHeight); // Square
            var atlas = new byte[atlasSize * atlasSize];

            var atlasObj = new FontAtlas(new Vector2(atlasSize, atlasSize), atlas, rasterizer.ToString(), scale, this);
            var atlasGlyphs = new AtlasGlyph[canvases.Length];

            float atlasRowSpacing = MathF.Ceiling(Height * (scale * samples));

            for (var i = 0; i < canvases.Length; i++)
            {
                atlasGlyphs[i] = canvases[i].Glyph;
                if (canvases[i].Data.Length == 0) continue;

                // Check if going over to a new line.
                if (pen.X + canvases[i].Width >= atlasSize - glyphSpacing)
                {
                    pen.X = glyphSpacing;
                    pen.Y += atlasRowSpacing + glyphSpacing;
                }

                // Copy pixels.
                for (var row = 0; row < canvases[i].Height; row++)
                {
                    for (var col = 0; col < canvases[i].Width; col++)
                    {
                        var x = (int) (pen.X + col);
                        var y = (int) (pen.Y + row);
                        atlas[y * atlasSize + x] = canvases[i].Data[row * canvases[i].Stride + col];
                    }
                }

                atlasGlyphs[i].Location = pen;
                atlasGlyphs[i].UV = new Vector2(canvases[i].Width, canvases[i].Height);

                // Increment pen. Leave space between glyphs.
                pen.X += canvases[i].Width + glyphSpacing;
            }

            foreach (AtlasGlyph glyph in atlasGlyphs)
            {
                atlasObj.Glyphs[glyph.CharIndex] = glyph;
            }

            return atlasObj;
        }

        private static GlyphRenderer.GlyphCanvas RenderGlyph(Font f, Glyph g, float scale, int samples)
        {
            var atlasGlyph = new AtlasGlyph(g, scale, f.Ascender);
            var canvas = new GlyphRenderer.GlyphCanvas(atlasGlyph, (int) (atlasGlyph.Size.X + 1) * samples, (int) (atlasGlyph.Size.Y + 1) * samples);

            // Check if glyph can be rendered, and render it.
            if (g.Vertices != null && g.Vertices.Length != 0)
                GlyphRenderer.RenderGlyph(canvas, g, scale * samples);

            // Remove padding.
            canvas.Width -= 1;
            canvas.Height -= 1;

            return canvas;
        }

#if StbTrueType
        private static unsafe GlyphRenderer.GlyphCanvas RenderGlyphStb(Font f, Glyph g, float scale)
        {
            var atlasGlyph = new AtlasGlyph(g, scale, f.Ascender);
            var canvas = new GlyphRenderer.GlyphCanvas(atlasGlyph, (int) atlasGlyph.Size.X + 1, (int) atlasGlyph.Size.Y + 1);

            // Check if glyph can be rendered.
            if (g.Vertices == null || g.Vertices.Length == 0)
                return canvas;

            // Render the current glyph.
            if ((int) atlasGlyph.Size.X == 0 || (int) atlasGlyph.Size.Y == 0) return canvas;
            GlyphVertex[] vertices = g.Vertices;

            fixed (byte* pixels = &canvas.Data[0])
            {
                var bitmap = new StbTrueType.stbtt__bitmap
                {
                    w = canvas.Width,
                    h = canvas.Height,
                    stride = canvas.Width,
                    pixels = pixels
                };

                var verts = new StbTrueType.stbtt_vertex[vertices.Length];
                for (var i = 0; i < vertices.Length; i++)
                {
                    verts[i].x = vertices[i].X;
                    verts[i].y = vertices[i].Y;
                    verts[i].cx = vertices[i].Cx;
                    verts[i].cy = vertices[i].Cy;
                    verts[i].cx1 = vertices[i].Cx1;
                    verts[i].cy1 = vertices[i].Cy1;
                    verts[i].type = (byte) vertices[i].TypeFlag;
                }

                Rectangle bbox = g.GetBBox(scale);

                fixed (StbTrueType.stbtt_vertex* vert = &verts[0])
                {
                    StbTrueType.stbtt_Rasterize(&bitmap, 0.35f, vert, vertices.Length, scale, scale, 0, 0, (int) bbox.X, (int) bbox.Y, 1);
                }
            }

            // Remove padding.
            canvas.Width -= 1;
            canvas.Height -= 1;

            return canvas;
        }
#endif

#if FreeType
        private GlyphRenderer.GlyphCanvas RenderGlyphFreeType(Glyph g, float scale)
        {
            if(_freeTypeFace == null)
            {
                return null;
            }

            GlyphRenderer.GlyphCanvas glyphCanvas;
            lock (_freeTypeFace)
            {
                _setCharSizeMethod.Invoke(null, new[] { _freeTypeFace, scale, 0 });
                dynamic ftGlyph = _renderGlyphMethod.Invoke(null, new[] { _freeTypeFace, g.CharIndex });
                dynamic bitmap = ftGlyph.Bitmap;
                var bitmapWidth = (int)bitmap.Width;
                var bitmapHeight = (int)bitmap.Rows;
                var bitmapPitch = (int)bitmap.Pitch;

                //YBearing = (float) (face.Size.Metrics.Ascender.ToDouble() - face.Glyph.Metrics.HorizontalBearingY.ToDouble()),
                // Get metrics as the Emotion parsed ones don't fit the FreeType ones.
                var minX = (float)ftGlyph.Metrics.HorizontalBearingX.ToDouble();
                var advance = (float)ftGlyph.Metrics.HorizontalAdvance.ToDouble();

                dynamic faceSize = _facePropertySize.GetValue(_freeTypeFace);
                var ascender = (float)faceSize.Metrics.Ascender.ToDouble();
                float yBearing = ascender - (float)ftGlyph.Metrics.HorizontalBearingY.ToDouble();
                var glyph = new AtlasGlyph((char) g.CharIndex, advance, minX, yBearing)
                {
                    Size = new Vector2(bitmapWidth, bitmapHeight)
                };

                glyphCanvas = new GlyphRenderer.GlyphCanvas(glyph, bitmapWidth, bitmapHeight);
                if (bitmapWidth == 0 || bitmapHeight == 0) return glyphCanvas;

                var bitmapData = (byte[])bitmap.BufferData;

                for (var row = 0; row < bitmapHeight; row++)
                {
                    for (var col = 0; col < bitmapWidth; col++)
                    {
                        glyphCanvas.Data[row * bitmapWidth + col] = bitmapData[row * bitmapPitch + col];
                    }
                }
            }

            return glyphCanvas;
        }
#endif

        #endregion

        #region Parse Helpers

        /// <summary>
        /// Add names and unicode indices to all the glyphs.
        /// </summary>
        /// <param name="glyphs">The glyphs themselves.</param>
        /// <param name="glyphIndexMap">Indices to glyph unicode symbols.</param>
        /// <param name="glyphNames">The glyph names read from the postscript table.</param>
        private static void AddGlyphNames(ref Glyph[] glyphs, Dictionary<uint, uint> glyphIndexMap, IReadOnlyList<string> glyphNames)
        {
            var glyphsReorder = new List<Glyph>();
            foreach ((uint key, uint value) in glyphIndexMap)
            {
                Glyph glyph = glyphs[value];
                if (glyphNames != null)
                    glyph.Name = glyphNames[(int) value];
                glyph.MapIndex = value;
                glyph.CharIndex = key;
                glyphsReorder.Add(glyph);
            }

            glyphs = glyphsReorder.ToArray();
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/cvt
        /// Instructions for control over certain glyphs.
        /// </summary>
        private static short[] ParseCvt(ByteReader reader, uint length)
        {
            var cvt = new short[length / 2];

            for (uint i = 0; i < length / 2; i++)
            {
                cvt[i] = reader.ReadShortBE();
            }

            return cvt;
        }

        /// <summary>
        /// Finds all tables in the font and their data.
        /// </summary>
        private static List<FontTable> GetTables(ByteReader reader, ushort count)
        {
            var tables = new List<FontTable>();

            for (var i = 0; i < count; i++)
            {
                var t = new FontTable
                {
                    Tag = new string(reader.ReadChars(4)),
                    Checksum = (int) reader.ReadULongBE(),
                    Offset = (int) reader.ReadULongBE(),
                    Length = (int) reader.ReadULongBE()
                };
                tables.Add(t);
            }

            return tables;
        }

        /// <summary>
        /// Get the font table under the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the table to get.</param>
        /// <returns>The table with the specified tag, or null if not found.</returns>
        public FontTable GetTable(string tag)
        {
            return Tables.FirstOrDefault(x => x.Tag == tag);
        }

        #endregion
    }
}
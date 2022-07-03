#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType.FontTables;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Standard.OpenType
{
    public class FontAnton
    {
        #region Meta

        /// <summary>
        /// The font's format.
        /// This will be either "truetype" or "cff" or "unsupported".
        /// </summary>
        public string Format { get; protected set; } = "unsupported";

        /// <summary>
        /// The font's family name.
        /// </summary>
        public string FontFamily { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's sub family name.
        /// </summary>
        public string FontSubFamily { get; protected set; } = "Unknown";

        /// <summary>
        /// The full name of the font.
        /// </summary>
        public string FullName { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's version.
        /// </summary>
        public string Version { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's copyright.
        /// </summary>
        public string Copyright { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's uniqueId.
        /// </summary>
        public string UniqueId { get; protected set; } = "Unknown";

        #endregion

        /// <summary>
        /// The ratio of glyph units per em.
        /// Used to scale uniformly across fonts.
        /// </summary>
        public int UnitsPerEm { get; protected set; }

        /// <summary>
        /// The font's ascender minus its descender.
        /// </summary>
        public float Height
        {
            get => Ascender - Descender;
        }

        /// <summary>
        /// The highest a glyph can reach.
        /// </summary>
        public float Ascender { get; protected set; }

        /// <summary>
        /// The lowest a glyph can reach.
        /// </summary>
        public float Descender { get; protected set; }

        /// <summary>
        /// Distance between lines.
        /// </summary>
        public float LineGap { get; protected set; }

        // This should probably be private
        private List<FontTable>? _tables;

        /// <summary>
        /// All of the glyphs and their data from this font.
        /// </summary>
        public FontGlyph[]? Glyphs;

        public FontAnton(ReadOnlyMemory<byte> fileData)
        {
            // Note: OpenType fonts use big endian byte ordering.
            using var r = new ByteReader(fileData);
            var tag = new string(r.ReadChars(4));

            switch (tag)
            {
                case "\0\u0001\0\0":
                case "true":
                case "typ1":
                {
                    Format = "truetype";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    _tables = GetTables(r, numTables);
                    break;
                }

                case "OTTO":
                {
                    Format = "cff";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    _tables = GetTables(r, numTables);
                    break;
                }
                default:
                {
                    Engine.Log.Error($"Unsupported font format - {tag}", MessageSource.FontParser);
                    return;
                }
            }

            // Parse required tables
            FontTable? headTable = GetTable("head");
            if (headTable == null)
            {
                Engine.Log.Error("Font is missing 'head' table.", MessageSource.FontParser);
                return;
            }

            FontTable? maxpTable = GetTable("maxp");
            if (maxpTable == null)
            {
                Engine.Log.Error("Font is missing 'maxp' table.", MessageSource.FontParser);
                return;
            }

            FontTable? hheaTable = GetTable("hhea");
            if (hheaTable == null)
            {
                Engine.Log.Error("Font is missing 'hhea' table.", MessageSource.FontParser);
                return;
            }

            var headTableParsed = new HeadTable(r.Branch(headTable.Offset, true, headTable.Length));
            var maxpTableParsed = new MaxpTable(r.Branch(maxpTable.Offset, true, maxpTable.Length));
            var hheaTableParsed = new HheaTable(r.Branch(hheaTable.Offset, true, hheaTable.Length));

            int numberOfHMetrics = hheaTableParsed.NumberOfHMetrics;
            int glyphCount = maxpTableParsed.NumGlyphs;

            short ascenderEm = hheaTableParsed.Ascender;
            short descenderEm = hheaTableParsed.Descender;
            short lineGapEm = hheaTableParsed.LineGap;

            // todo: do in anton rasterizer and apply scale to glyph shit as well, ig
            float scale = 1f / ascenderEm;
            Ascender = 1f;
            Descender = descenderEm * scale;
            LineGap = lineGapEm * scale;

            // glyf fonts
            FontTable? glyfTable = GetTable("glyf");
            if (glyfTable != null)
            {
                FontTable? locaTable = GetTable("loca");
                FontTable? hmtxTable = GetTable("hmtx");

                if (locaTable == null || hmtxTable == null) return;

                short indexToLocFormat = headTableParsed.IndexToLocFormat;
                AntonLocaTable? locaTableParsed = AntonLocaTable.ParseLoca(r.Branch(locaTable.Offset, true, locaTable.Length), glyphCount, indexToLocFormat == 0);
                if (locaTableParsed == null) return;

                var glyphs = new FontGlyph[glyphCount];
                for (var i = 0; i < glyphs.Length; i++)
                {
                    var glyph = new FontGlyph();
                    glyph.MapIndex = i;
                    glyphs[i] = glyph;
                }

                AntonHmtxTable hmtxTableParsed = AntonHmtxTable.ParseHmtx(r.Branch(hmtxTable.Offset, true, hmtxTable.Length));
                hmtxTableParsed.ApplyToGlyphs(numberOfHMetrics, glyphs);
                for (var i = 0; i < glyphs.Length; i++)
                {
                    FontGlyph glyph = glyphs[i];
                    glyph.Advance *= scale;
                    glyph.LeftSideBearing *= scale;
                }

                ByteReader glyfTableReader = r.Branch(glyfTable.Offset, true, glyfTable.Length)!;

                // First pass: read commands of all glyphs
                for (var i = 0; i < glyphs.Length; i++)
                {
                    FontGlyph glyph = glyphs[i];
                    ReadGlyph(glyph, locaTableParsed, glyfTableReader, scale);
                }

                // Second pass: combine composite glyphs.
                for (var i = 0; i < glyphs.Length; i++)
                {
                    FontGlyph glyph = glyphs[i];
                    if (!glyph.Composite) continue;
                    CombineCompositeGlyph(glyph, glyphs);
                }

                Glyphs = glyphs;
            }

            // todo: cff

            // Parse optional tables.
            FontTable? nameTable = GetTable("name");
            if (nameTable != null)
            {
                // todo: ltag parsing
                Dictionary<string, Dictionary<string, string>> names = NameTable.ParseName(r.Branch(nameTable.Offset, true, nameTable.Length), null);

                FontFamily = NameTable.GetDefaultValue(names, "fontFamily");
                FontSubFamily = NameTable.GetDefaultValue(names, "fontSubfamily");
                FullName = NameTable.GetDefaultValue(names, "fullName");
                Version = NameTable.GetDefaultValue(names, "version");
                Copyright = NameTable.GetDefaultValue(names, "copyright");
                UniqueId = NameTable.GetDefaultValue(names, "uniqueID");
            }

            var cmapTable = GetTable("cmap");
        }

        /// <summary>
        /// Finds all tables in the font and their data.
        /// </summary>
        private static List<FontTable> GetTables(ByteReaderBase reader, ushort count)
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
        private FontTable? GetTable(string tag)
        {
            for (var i = 0; i < _tables?.Count; i++)
            {
                FontTable table = _tables[i];
                if (table.Tag == tag) // should probably be name and not tag
                    return table;
            }

            return null;
        }

        #region Table Handlers

        private static void ReadGlyphCoordinate(ByteReader reader, byte flags, ref int prevVal, short shortVectorBitMask, short sameBitMask)
        {
            // The coordinate is short, and so is 1 byte long
            if ((flags & shortVectorBitMask) != 0)
            {
                short val = reader.ReadByte();

                // The `same` bit is re-used for short values to signify the sign of the value.
                prevVal += (flags & sameBitMask) != 0 ? val : -val;
            }
            else
            {
                // The coordinate is long, and so is 2 bytes long
                // If the `same` bit is set, the coordinate is the same as the previous coordinate (hasn't changed).
                if ((flags & sameBitMask) == 0) prevVal += reader.ReadShortBE();
            }
        }

        public enum GlyphDrawCommandType : byte
        {
            Move,
            Line,
            Curve,
            Close
        }

        public struct GlyphDrawCommand
        {
            public GlyphDrawCommandType Type;
            public Vector2 P0;
            public Vector2 P1;
        }

        private void CombineCompositeGlyph(FontGlyph g, FontGlyph[] glyphs)
        {
            var combinedCommands = new List<GlyphDrawCommand>();

            GlyphDrawCommand[]? commands = g.Commands;
            if (commands != null && commands.Length != 0) combinedCommands.AddRange(commands);

            FontGlyphComponent[]? components = g.Components;
            Debug.Assert(components != null);
            for (var i = 0; i < components.Length; i++)
            {
                FontGlyphComponent component = components[i];
                int gIdx = component.GlyphIdx;

                Debug.Assert(gIdx < glyphs.Length, "Composite glyph is trying to fetch a non-existent component glyph.");
                FontGlyph comp = glyphs[gIdx];
                if (comp.Commands == null || comp.Commands.Length <= 0) continue;

                float[] mtx = component.Matrix;
                var m = (float) Math.Sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                var n = (float) Math.Sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

                // Copy vertices from the composite part.
                for (var c = 0; c < comp.Commands.Length; c++)
                {
                    GlyphDrawCommand v = comp.Commands[c]; // This should copy.
                    float x = v.P0.X;
                    float y = v.P0.Y;
                    float tX = (m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                    float tY = (n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                    v.P0 = new Vector2(tX, tY);

                    x = v.P1.X;
                    y = v.P1.Y;
                    tX = m * (mtx[0] * x + mtx[2] * y + mtx[4]);
                    tY = n * (mtx[1] * x + mtx[3] * y + mtx[5]);
                    v.P1 = new Vector2(tX, tY);

                    combinedCommands.Add(v);
                }
            }

            g.Commands = combinedCommands.ToArray();
        }

        private void ReadGlyph(FontGlyph g, AntonLocaTable locaTable, ByteReader glyfTableReader, float scale)
        {
            int glyphIndex = g.MapIndex;
            int[] glyphOffsets = locaTable.GlyphOffsets;

            if (glyphIndex > glyphOffsets.Length) return;
            int glyphOffset = glyphOffsets[glyphIndex];
            int nextOffset = glyphOffsets[glyphIndex + 1];

            // No data for glyph.
            if (glyphOffset == nextOffset || glyphOffset >= glyfTableReader.Data.Length) return;

            glyfTableReader.Position = glyphOffset;
            int numberOfContours = glyfTableReader.ReadShortBE();
            float minX = glyfTableReader.ReadShortBE();
            float minY = glyfTableReader.ReadShortBE();
            float maxX = glyfTableReader.ReadShortBE();
            float maxY = glyfTableReader.ReadShortBE();

            g.Min = new Vector2(minX, minY) * scale;
            g.Max = new Vector2(maxX, maxY) * scale;

            // Simple glyph
            if (numberOfContours > 0)
            {
                // Indices for the last point of each contour
                var endPointIndices = new ushort[numberOfContours];
                for (var i = 0; i < endPointIndices.Length; i++)
                {
                    endPointIndices[i] = glyfTableReader.ReadUShortBE();
                }

                ushort instructionLength = glyfTableReader.ReadUShortBE();
                glyfTableReader.ReadBytes(instructionLength);

                int numberOfCoordinates = endPointIndices[^1] + 1;
                var flagArray = new byte[numberOfCoordinates];

                // Read the flags needed to properly read the coordinates afterward.
                // Flag bits:
                // 0x01 - on-curve, ~0x01 - off-curve
                // Two consecutive off-curve points assume on-curve point between them    
                //
                // 0x02 - x-coord is 8-bit unsigned integer
                //       0x10 - positive, ~0x10 - negative
                // ~0x02 - x-coord is 16-bit signed integer
                // ~0x02 & 0x10 - x-coord equals x-coord of the previous point
                // 
                // 0x04 - y-coord is 8-bit unsigned integer
                //       0x20 - positive, ~0x20 - negative
                // ~0x04 - y-coord is 16-bit signed integer
                // ~0x04 & 0x20 - y-coord equals y-coord of the previous point
                //
                // 0x08 - repeat flag N times, read next byte for N
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = glyfTableReader.ReadByte();
                    flagArray[i] = flags;

                    // Check for repeat
                    if ((flags & 0x08) <= 0) continue;

                    byte repeatCount = glyfTableReader.ReadByte();
                    for (var j = 0; j < repeatCount; j++)
                    {
                        i++;
                        flagArray[i] = flags;
                    }
                }

                // Read vertex X coordinates. They are lined up first.
                var xArray = new short[numberOfCoordinates];
                var prevX = 0;
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = flagArray[i];
                    ReadGlyphCoordinate(glyfTableReader, flags, ref prevX, 2, 16);
                    xArray[i] = (short) prevX;
                }

                // Read vertex Y coordinates.
                var yArray = new short[numberOfCoordinates];
                var prevY = 0;
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = flagArray[i];
                    ReadGlyphCoordinate(glyfTableReader, flags, ref prevY, 4, 32);
                    yArray[i] = (short) prevY;
                }

                Vector2 curPos = Vector2.Zero;
                var curOnCurve = true;
                var newContour = true;
                var newContourOffCurve = false;

                var nextEndpoint = 0;
                var endPointIndex = 0;
                var currentContourStartCommand = 0;

                var drawCommands = new List<GlyphDrawCommand>();

                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flag = flagArray[i];

                    bool prevOnCurve = curOnCurve;
                    curOnCurve = (flag & 0x01) != 0;

                    Vector2 prevPos = curPos;
                    short xCoord = xArray[i];
                    short yCoord = yArray[i];

                    curPos = new Vector2(xCoord, yCoord);

                    // Starting a new contour
                    if (newContour)
                    {
                        newContourOffCurve = !curOnCurve;

                        drawCommands.Add(new GlyphDrawCommand
                        {
                            Type = GlyphDrawCommandType.Move,
                            P0 = curPos * scale
                        });
                        currentContourStartCommand = drawCommands.Count - 1;

                        nextEndpoint = endPointIndices[endPointIndex];
                        endPointIndex++;
                        newContour = false;
                    }
                    else if (curOnCurve)
                    {
                        if (prevOnCurve)
                            // Normal (non smooth) control point
                            drawCommands.Add(new GlyphDrawCommand
                            {
                                Type = GlyphDrawCommandType.Line,
                                P0 = curPos * scale
                            });
                        else
                            // Normal control point
                            drawCommands.Add(new GlyphDrawCommand
                            {
                                Type = GlyphDrawCommandType.Curve,
                                P0 = prevPos * scale,
                                P1 = curPos * scale
                            });
                    }
                    else if (!prevOnCurve)
                    {
                        // Smooth curve, inserting control point in the middle
                        Vector2 middle = 0.5f * (prevPos + curPos);
                        drawCommands.Add(new GlyphDrawCommand
                        {
                            Type = GlyphDrawCommandType.Curve,
                            P0 = prevPos * scale,
                            P1 = scale * middle
                        });
                    }

                    if (i != 0 && i == nextEndpoint)
                    {
                        GlyphDrawCommand startingCommand = drawCommands[currentContourStartCommand];

                        if (newContourOffCurve)
                        {
                            // Contour starts off-curve, contour start to current point
                            if (curOnCurve)
                            {
                                startingCommand.P0 = curPos * scale;
                            }
                            // Contour starts and ends off-curve,
                            // calculating contour starting point, setting first Move P0,
                            // and closing contour with a curve
                            else
                            {
                                Vector2 scaledPos = curPos * scale;
                                GlyphDrawCommand nextCommand = drawCommands[currentContourStartCommand + 1]; // First CurveTo off-curve CP
                                Vector2 curveControlPoint = 0.5f * (scaledPos + nextCommand.P0); // Contour start point;
                                startingCommand.P0 = curveControlPoint;

                                drawCommands.Add(new GlyphDrawCommand
                                {
                                    Type = GlyphDrawCommandType.Curve,
                                    P0 = scaledPos,
                                    P1 = curveControlPoint
                                });
                            }
                        }
                        // Contour ends off-curve, closing contour with BezTo to contour starting point
                        else if (!curOnCurve)
                        {
                            drawCommands.Add(new GlyphDrawCommand
                            {
                                Type = GlyphDrawCommandType.Curve,
                                P0 = curPos * scale,
                                P1 = startingCommand.P0
                            });
                        }

                        drawCommands[currentContourStartCommand] = startingCommand;

                        drawCommands.Add(new GlyphDrawCommand
                        {
                            Type = GlyphDrawCommandType.Close
                        });
                        newContour = true;
                    }
                }

                g.Commands = drawCommands.ToArray();
            }
            // Composite glyph (-1)
            else if (numberOfContours < 0)
            {
                g.Composite = true;
                var components = new List<FontGlyphComponent>();
                bool more = true;
                while (more)
                {
                    var flags = (ushort) glyfTableReader.ReadShortBE();

                    // The glyph index of the composite part.
                    var gIdx = (ushort) glyfTableReader.ReadShortBE();

                    // Matrix3x2 for composite transformation
                    var mtx = new float[6];
                    mtx[0] = 1;
                    mtx[1] = 0;
                    mtx[2] = 0;
                    mtx[3] = 1;
                    mtx[4] = 0;
                    mtx[5] = 0;

                    // Position
                    if ((flags & 2) != 0)
                    {
                        if ((flags & 1) != 0)
                        {
                            mtx[4] = glyfTableReader.ReadShortBE() * scale;
                            mtx[5] = glyfTableReader.ReadShortBE() * scale;
                        }
                        else
                        {
                            mtx[4] = glyfTableReader.ReadSByte() * scale;
                            mtx[5] = glyfTableReader.ReadSByte() * scale;
                        }
                    }

                    // Uniform scale
                    if ((flags & (1 << 3)) != 0)
                    {
                        mtx[0] = mtx[3] = glyfTableReader.ReadShortBE() / 16384.0f;
                        mtx[1] = mtx[2] = 0;
                    }
                    // XY-scale
                    else if ((flags & (1 << 6)) != 0)
                    {
                        mtx[0] = glyfTableReader.ReadShortBE() / 16384.0f;
                        mtx[1] = mtx[2] = 0;
                        mtx[3] = glyfTableReader.ReadShortBE() / 16384.0f;
                    }
                    // Rotation
                    else if ((flags & (1 << 7)) != 0)
                    {
                        mtx[0] = glyfTableReader.ReadShortBE() / 16384.0f;
                        mtx[1] = glyfTableReader.ReadShortBE() / 16384.0f;
                        mtx[2] = glyfTableReader.ReadShortBE() / 16384.0f;
                        mtx[3] = glyfTableReader.ReadShortBE() / 16384.0f;
                    }

                    components.Add(new FontGlyphComponent
                    {
                        GlyphIdx = gIdx,
                        Matrix = mtx
                    });

                    more = (flags & (1 << 5)) != 0;
                }

                g.Components = components.ToArray();
            }
        }

        #endregion

        public class FontGlyphComponent
        {
            public int GlyphIdx;
            public float[] Matrix;
        }

        public class FontGlyph
        {
            public int MapIndex;

            public bool Composite;
            public FontGlyphComponent[]? Components;

            public float Advance;
            public float LeftSideBearing;

            public Vector2 Min;
            public Vector2 Max;

            public GlyphDrawCommand[]? Commands;
        }

        public class AntonLocaTable
        {
            public int[] GlyphOffsets;

            private AntonLocaTable(int[] glyphOffsets)
            {
                GlyphOffsets = glyphOffsets;
            }

            public static AntonLocaTable? ParseLoca(ByteReader reader, int glyphCount, bool shortValues)
            {
                int[]? offsets = LocaTable.ParseLoca(reader, (ushort) glyphCount, shortValues);
                if (offsets == null) return null;
                return new AntonLocaTable(offsets);
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/hmtx
        /// </summary>
        public class AntonHmtxTable
        {
            private ByteReader _reader;

            private AntonHmtxTable(ByteReader reader)
            {
                _reader = reader;
            }

            public static AntonHmtxTable ParseHmtx(ByteReader reader)
            {
                return new AntonHmtxTable(reader);
            }

            public void ApplyToGlyphs(int numberOfHMetrics, FontGlyph[] glyphs)
            {
                _reader.Position = 0;

                for (var i = 0; i < numberOfHMetrics; i++)
                {
                    FontGlyph glyph = glyphs[i];
                    glyph.Advance = _reader.ReadUShortBE();
                    glyph.LeftSideBearing = _reader.ReadShortBE();
                }

                // There can be more left side bearings after the ending.
                // These contain only the LSB.
                if (_reader.Position + 2 <= _reader.Data.Length)
                {
                    _reader.ReadBytes(2);
                    int extraMetricCount = (_reader.Data.Length - _reader.Position) / 2;
                    if (extraMetricCount < 0) extraMetricCount = 0;

                    for (int i = numberOfHMetrics; i < extraMetricCount; i++)
                    {
                        FontGlyph glyph = glyphs[i];
                        glyph.LeftSideBearing = _reader.ReadShortBE();
                    }
                }
            }
        }
    }
}
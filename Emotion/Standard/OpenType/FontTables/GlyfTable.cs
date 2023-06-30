#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

#nullable enable

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/glyf
    /// </summary>
    public static class GlyfTable
    {
        public static void ParseGlyf(ByteReader reader, LocaTable locaTableParsed, FontGlyph[] glyphs, float scale)
        {
            // First pass: read commands of all glyphs
            for (var i = 0; i < glyphs.Length; i++)
            {
                FontGlyph glyph = glyphs[i];
                ReadGlyph(glyph, locaTableParsed, reader, scale);
            }

            // Second pass: combine composite glyphs.
            for (var i = 0; i < glyphs.Length; i++)
            {
                FontGlyph glyph = glyphs[i];
                if (!glyph.Composite) continue;
                CombineCompositeGlyph(glyph, glyphs);
            }
        }

        private static void ReadGlyph(FontGlyph g, LocaTable locaTable, ByteReader glyfTableReader, float scale)
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

                    // The next position for the brush.
                    // In line commands this is the next line vertex
                    // In curves this is the control point.
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
                                P1 = prevPos * scale,
                                P0 = curPos * scale
                            });
                    }
                    else if (!prevOnCurve)
                    {
                        // Smooth curve, inserting control point in the middle
                        Vector2 middle = 0.5f * (prevPos + curPos);
                        drawCommands.Add(new GlyphDrawCommand
                        {
                            Type = GlyphDrawCommandType.Curve,
                            P1 = prevPos * scale,
                            P0 = middle * scale
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
                                Vector2 contourStart = 0.5f * (scaledPos + nextCommand.P0); // Contour start point;
                                startingCommand.P0 = contourStart;

                                drawCommands.Add(new GlyphDrawCommand
                                {
                                    Type = GlyphDrawCommandType.Curve,
                                    P1 = scaledPos,
                                    P0 = contourStart
                                });
                            }
                        }
                        // Contour ends off-curve, closing contour with BezTo to contour starting point
                        else if (!curOnCurve)
                        {
                            drawCommands.Add(new GlyphDrawCommand
                            {
                                Type = GlyphDrawCommandType.Curve,
                                P1 = curPos * scale,
                                P0 = startingCommand.P0
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
                var more = true;
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

                    components.Add(new FontGlyphComponent(gIdx, mtx));

                    more = (flags & (1 << 5)) != 0;
                }

                g.Components = components.ToArray();
            }
        }

        private static void CombineCompositeGlyph(FontGlyph g, FontGlyph[] glyphs)
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
                    float tX = m * (mtx[0] * x + mtx[2] * y + mtx[4]);
                    float tY = n * (mtx[1] * x + mtx[3] * y + mtx[5]);
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
    }
}
#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Standard.Utility;

#endregion

namespace Emotion.Standard.Text.FontTables
{
    public static class Glyf
    {
        public static Glyph[] ParseGlyf(ByteReader reader, int[] locaOffsets)
        {
            var glyphs = new Glyph[locaOffsets.Length - 1];

            Glyph ResolveGlyph(int idx)
            {
                if (glyphs[idx] != null) return glyphs[idx];
                if (idx > locaOffsets.Length) return new Glyph();

                int glyphOffset = locaOffsets[idx];
                int nextOffset = locaOffsets[idx + 1];

                if (glyphOffset != nextOffset && glyphOffset < reader.Data.Length)
                    return TtfGlyphLoader(reader.Branch(glyphOffset, true), ResolveGlyph);

                return new Glyph();
            }

            for (var i = 0; i < glyphs.Length; i++)
            {
                glyphs[i] = ResolveGlyph(i);
            }

            return glyphs;
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

        public static Glyph TtfGlyphLoader(ByteReader reader, Func<int, Glyph> compositeGlyphRequest)
        {
            short numberOfContours = reader.ReadShortBE();

            var glyph = new Glyph
            {
                XMin = reader.ReadShortBE(),
                YMin = reader.ReadShortBE(),
                XMax = reader.ReadShortBE(),
                YMax = reader.ReadShortBE()
            };

            if (numberOfContours > 0)
            {
                // Read the end point indices.
                // These mark which points are last in their polygons.
                var endPointIndices = new ushort[numberOfContours];
                for (var i = 0; i < endPointIndices.Length; i++)
                {
                    endPointIndices[i] = reader.ReadUShortBE();
                }

                ushort instructionLength = reader.ReadUShortBE();
                reader.ReadBytes(instructionLength);

                // ReSharper disable once UseIndexFromEndExpression
                int numberOfCoordinates = endPointIndices[endPointIndices.Length - 1] + 1;
                var vertices = new GlyphVertex[numberOfCoordinates];

                // Read the flags needed to properly read the coordinates afterward.
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = reader.ReadByte();
                    vertices[i].Flags = flags;

                    // If bit 3 is set, we repeat this flag n times, where n is the next byte.
                    if ((flags & 8) <= 0) continue;
                    byte repeatCount = reader.ReadByte();
                    for (var j = 0; j < repeatCount; j++)
                    {
                        i++;
                        vertices[i].Flags = flags;
                    }
                }

                // Read vertex X coordinates. They are lined up first.
                var prevX = 0;
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = vertices[i].Flags;
                    ReadGlyphCoordinate(reader, flags, ref prevX, 2, 16);
                    vertices[i].X = (short) prevX;
                }

                // Read vertex Y coordinates.
                var prevY = 0;
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    byte flags = vertices[i].Flags;
                    ReadGlyphCoordinate(reader, flags, ref prevY, 4, 32);
                    vertices[i].Y = (short) prevY;
                }

                // Process vertices.
                var offCurve = false;
                var wasOffCurve = false;
                short sx = 0;
                short sy = 0;
                short scx = 0;
                short scy = 0;
                short cx = 0;
                short cy = 0;

                var nextEndpoint = 0;
                var endPointIndex = 0;

                var verticesProcessed = new List<GlyphVertex>();
                for (var i = 0; i < numberOfCoordinates; i++)
                {
                    GlyphVertex curVert = vertices[i];
                    short x = curVert.X;
                    short y = curVert.Y;
                    bool curVertexOffCurve = (curVert.Flags & 1) == 0;

                    if (nextEndpoint == i)
                    {
                        // Close the polygon. The first vertex passes through here in order to move the brush the first time, but
                        // there is no polygon for it to close, therefore this check must be here.
                        if (i != 0) CloseShape(verticesProcessed, wasOffCurve, offCurve, sx, sy, scx, scy, cx, cy);

                        offCurve = curVertexOffCurve;
                        if (offCurve && i + 1 < vertices.Length - 1) // Check if in range as well.
                        {
                            scx = x;
                            scy = y;

                            bool nextNotOnCurve = (vertices[i + 1].Flags & 1) == 0;
                            if (nextNotOnCurve)
                            {
                                sx = (short) ((x + vertices[i + 1].X) >> 1);
                                sy = (short) ((y + vertices[i + 1].Y) >> 1);
                            }
                            else
                            {
                                sx = vertices[i + 1].X;
                                sy = vertices[i + 1].Y;
                                i++;
                            }
                        }
                        else
                        {
                            sx = x;
                            sy = y;
                        }

                        verticesProcessed.Add(new GlyphVertex
                        {
                            TypeFlag = VertexTypeFlag.Move,
                            X = sx,
                            Y = sy,
                            Cx = 0,
                            Cy = 0
                        });

                        wasOffCurve = false;

                        nextEndpoint = endPointIndices[endPointIndex] + 1;
                        endPointIndex++;
                    }
                    else
                    {
                        if (curVertexOffCurve)
                        {
                            if (wasOffCurve)
                                verticesProcessed.Add(new GlyphVertex
                                {
                                    TypeFlag = VertexTypeFlag.Curve,
                                    X = x,
                                    Y = y,
                                    Cx = cx,
                                    Cy = cy
                                });

                            cx = x;
                            cy = y;
                            wasOffCurve = true;
                        }
                        else
                        {
                            if (wasOffCurve)
                                verticesProcessed.Add(new GlyphVertex
                                {
                                    TypeFlag = VertexTypeFlag.Curve,
                                    X = (short) ((cx + x) >> 1),
                                    Y = (short) ((cy + y) >> 1),
                                    Cx = cx,
                                    Cy = cy
                                });
                            else
                                verticesProcessed.Add(new GlyphVertex
                                {
                                    TypeFlag = VertexTypeFlag.Line,
                                    X = x,
                                    Y = y,
                                    Cx = 0,
                                    Cy = 0
                                });

                            wasOffCurve = false;
                        }
                    }
                }

                CloseShape(verticesProcessed, wasOffCurve, offCurve, sx, sy, scx, scy, cx, cy);
                glyph.Vertices = verticesProcessed.ToArray();
            }
            else if (numberOfContours == -1)
            {
                var more = true;
                var numVertices = 0;
                GlyphVertex[] vertices = null;
                while (more)
                {
                    var mtx = new float[6];
                    mtx[0] = 1;
                    mtx[1] = 0;
                    mtx[2] = 0;
                    mtx[3] = 1;
                    mtx[4] = 0;
                    mtx[5] = 0;
                    var flags = (ushort) reader.ReadShortBE();
                    var gidx = (ushort) reader.ReadShortBE();
                    if ((flags & 2) != 0)
                    {
                        if ((flags & 1) != 0)
                        {
                            mtx[4] = reader.ReadShortBE();
                            mtx[5] = reader.ReadShortBE();
                        }
                        else
                        {
                            mtx[4] = reader.ReadByte();
                            mtx[5] = reader.ReadByte();
                        }
                    }

                    if ((flags & (1 << 3)) != 0)
                    {
                        mtx[0] = mtx[3] = reader.ReadShortBE() / 16384.0f;
                        mtx[1] = mtx[2] = 0;
                    }
                    else if ((flags & (1 << 6)) != 0)
                    {
                        mtx[0] = reader.ReadShortBE() / 16384.0f;
                        mtx[1] = mtx[2] = 0;
                        mtx[3] = reader.ReadShortBE() / 16384.0f;
                    }
                    else if ((flags & (1 << 7)) != 0)
                    {
                        mtx[0] = reader.ReadShortBE() / 16384.0f;
                        mtx[1] = reader.ReadShortBE() / 16384.0f;
                        mtx[2] = reader.ReadShortBE() / 16384.0f;
                        mtx[3] = reader.ReadShortBE() / 16384.0f;
                    }

                    var m = (float) Math.Sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                    var n = (float) Math.Sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);
                    Glyph comp = compositeGlyphRequest(gidx);
                    if (comp?.Vertices != null && comp.Vertices.Length > 0)
                    {
                        for (var i = 0; i < comp.Vertices.Length; ++i)
                        {
                            GlyphVertex v = comp.Vertices[i];
                            short x = v.X;
                            short y = v.Y;
                            v.X = (short) (m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                            v.Y = (short) (n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                            x = v.Cx;
                            y = v.Cy;
                            v.Cx = (short) (m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                            v.Cy = (short) (n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                        }

                        var tmp = new GlyphVertex[numVertices + comp.Vertices.Length];

                        if (vertices != null && numVertices != 0)
                            for (var j = 0; j < numVertices; j++)
                            {
                                tmp[j] = vertices[j];
                            }

                        for (var j = 0; j < comp.Vertices.Length; j++)
                        {
                            tmp[j + numVertices] = comp.Vertices[j];
                        }

                        vertices = tmp;
                        numVertices += comp.Vertices.Length;
                    }

                    more = (flags & (1 << 5)) != 0;
                }

                glyph.Vertices = vertices;
            }
            else if (numberOfContours == 0)
            {
                // error handling?
                // todo
                Debug.Assert(true);
            }

            return glyph;
        }

        private static void CloseShape(List<GlyphVertex> vertices, bool wasOnCurve, bool onCurve,
            short sx, short sy, short scx, short scy, short cx, short cy)
        {
            if (onCurve)
            {
                if (wasOnCurve)
                    vertices.Add(new GlyphVertex
                    {
                        TypeFlag = VertexTypeFlag.Curve,
                        X = (short) ((cx + scx) >> 1),
                        Y = (short) ((cy + scy) >> 1),
                        Cx = cx,
                        Cy = cy
                    });

                vertices.Add(new GlyphVertex
                {
                    TypeFlag = VertexTypeFlag.Curve,
                    X = sx,
                    Y = sy,
                    Cx = scx,
                    Cy = scy
                });
            }
            else
            {
                if (wasOnCurve)
                    vertices.Add(new GlyphVertex
                    {
                        TypeFlag = VertexTypeFlag.Curve,
                        X = sx,
                        Y = sy,
                        Cx = cx,
                        Cy = cy
                    });
                else
                    vertices.Add(new GlyphVertex
                    {
                        TypeFlag = VertexTypeFlag.Line,
                        X = sx,
                        Y = sy,
                        Cx = 0,
                        Cy = 0
                    });
            }
        }
    }
}
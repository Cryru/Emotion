#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Graphics.Text
{
    public static class CommonGlyphRenderer
    {
        public static GlyphRendererState PrepareGlyphRenderer(DrawableFontAtlas atlas, GlyphRendererState state, List<AtlasGlyph> glyphsToAdd, out Rectangle[] intermediateAtlasUVs,
            out Vector2 intermediateAtlasSize, float glyphSpacing = 1)
        {
            var spacing = new Vector2(glyphSpacing);
            Vector2 spacing2 = spacing * 2;

            intermediateAtlasUVs = null;
            if (state == null)
            {
                // Initialize atlas by binning first.
                var binningRects = new Rectangle[glyphsToAdd.Count];
                intermediateAtlasUVs = new Rectangle[glyphsToAdd.Count];
                for (var i = 0; i < glyphsToAdd.Count; i++)
                {
                    AtlasGlyph atlasGlyph = glyphsToAdd[i];
                    if (atlasGlyph == null) continue;

                    Vector2 glyphRenderSize = atlasGlyph.Size + spacing2;
                    binningRects[i] = new Rectangle(0, 0, glyphRenderSize);
                    intermediateAtlasUVs[i] = new Rectangle(0, 0, glyphRenderSize);
                }

                // Apply to atlas glyphs.
                var resumableState = new Binning.BinningResumableState(Vector2.Zero);
                Vector2 atlasSize = Binning.FitRectangles(binningRects, false, resumableState);
                for (var i = 0; i < binningRects.Length; i++)
                {
                    AtlasGlyph atlasGlyph = glyphsToAdd[i];
                    if (atlasGlyph == null) continue;

                    Rectangle binPosition = binningRects[i];
                    atlasGlyph.UVLocation = binPosition.Position + spacing;
                    atlasGlyph.UVSize = binPosition.Size - spacing2;
                }

                state = new GlyphRendererState();
                state.BinningState = resumableState;
                state.AtlasBuffer = new FrameBuffer(atlasSize).WithColor();
            }

            // Log requests after the first.
            Engine.Log.Trace($"Requested rendering of {glyphsToAdd.Count} font glyphs using {atlas.RenderedWith}.", MessageSource.Renderer);

            // Bin new glyphs in the big atlas.
            if (intermediateAtlasUVs == null)
            {
                intermediateAtlasUVs = new Rectangle[glyphsToAdd.Count];
                var resizedOnceAlready = false;
                for (var i = 0; i < glyphsToAdd.Count; i++)
                {
                    AtlasGlyph atlasGlyph = glyphsToAdd[i];
                    if (atlasGlyph == null) continue;

                    Vector2 glyphRenderSize = atlasGlyph.Size + spacing2;
                    Vector2? position = Binning.FitRectanglesResumable(glyphRenderSize, state.BinningState);

                    // No space for new glyph.
                    if (position == null)
                    {
                        if (resizedOnceAlready) Engine.Log.Warning("Resizing glyph atlas multiple times in one request.", MessageSource.Renderer);

                        // Go through all existing glyphs and request them to be rerendered as well.
                        foreach (KeyValuePair<char, AtlasGlyph> renderedGlyph in atlas.Glyphs)
                        {
                            // Already exists in request.
                            if (glyphsToAdd.IndexOf(renderedGlyph.Value) != -1) continue;
                            glyphsToAdd.Add(renderedGlyph.Value);
                        }

                        state.BinningState = new Binning.BinningResumableState(state.BinningState.Size * 2f);
                        state.AtlasBuffer.Resize(state.BinningState.Size, true);
                        Array.Resize(ref intermediateAtlasUVs, glyphsToAdd.Count);
                        i = -1; // Reset the loop.
                        resizedOnceAlready = true;
                        continue;
                    }

                    atlasGlyph.UVLocation = position.Value + spacing;
                    atlasGlyph.UVSize = glyphRenderSize - spacing2;
                    intermediateAtlasUVs[i] = new Rectangle(0, 0, glyphRenderSize);
                }
            }

            intermediateAtlasSize = Binning.FitRectangles(intermediateAtlasUVs);
            for (var i = 0; i < intermediateAtlasUVs.Length; i++)
            {
                intermediateAtlasUVs[i].Size -= spacing2;
            }

            return state;
        }
    }
}
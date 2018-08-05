/* ============================================================================
 * Freetype GL - A C OpenGL Freetype engine
 * Platform:    Any
 * WWW:         http://code.google.com/p/freetype-gl/
 * ----------------------------------------------------------------------------
 * Copyright 2011,2012 Nicolas P. Rougier. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *  1. Redistributions of source code must retain the above copyright notice,
 *     this list of conditions and the following disclaimer.
 *
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY NICOLAS P. ROUGIER ''AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL NICOLAS P. ROUGIER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * The views and conclusions contained in the software and documentation are
 * those of the authors and should not be interpreted as representing official
 * policies, either expressed or implied, of Nicolas P. Rougier.
 * ============================================================================
 */


using System.Collections.Generic;


/**
 * A structure that hold a kerning value relatively to a charcode.
 *
 * This structure cannot be used alone since the (necessary) right charcode is
 * implicitely held by the owner of this structure.
 */
public class kerning_t
{
    /**
	 * Left character code in the kern pair.
	 */
    public char charcode;

    /**
	 * Kerning value (in fractional pixels).
	 */
    public float kerning;

}


/*
 * Glyph metrics:
 * --------------
 *
 *                       xmin                     xmax
 *                        |                         |
 *                        |<-------- width -------->|
 *                        |                         |
 *              |         +-------------------------+----------------- ymax
 *              |         |    ggggggggg   ggggg    |     ^        ^
 *              |         |   g:::::::::ggg::::g    |     |        |
 *              |         |  g:::::::::::::::::g    |     |        |
 *              |         | g::::::ggggg::::::gg    |     |        |
 *              |         | g:::::g     g:::::g     |     |        |
 *    offset_x -|-------->| g:::::g     g:::::g     |  offset_y    |
 *              |         | g:::::g     g:::::g     |     |        |
 *              |         | g::::::g    g:::::g     |     |        |
 *              |         | g:::::::ggggg:::::g     |     |        |
 *              |         |  g::::::::::::::::g     |     |      height
 *              |         |   gg::::::::::::::g     |     |        |
 *  baseline ---*---------|---- gggggggg::::::g-----*--------      |
 *            / |         |             g:::::g     |              |
 *     origin   |         | gggggg      g:::::g     |              |
 *              |         | g:::::gg   gg:::::g     |              |
 *              |         |  g::::::ggg:::::::g     |              |
 *              |         |   gg:::::::::::::g      |              |
 *              |         |     ggg::::::ggg        |              |
 *              |         |         gggggg          |              v
 *              |         +-------------------------+----------------- ymin
 *              |                                   |
 *              |------------- advance_x ---------->|
 */

/**
 * A structure that describe a glyph.
 */
public class texture_glyph_t
{
    /**
	 * Wide character this glyph represents
	 */
    public int charcode;

    /**
	 * Glyph id (used for display lists)
	 */
    public uint id;

    /**
	 * Glyph's width in pixels.
	 */
    public uint width = new uint();

    /**
	 * Glyph's height in pixels.
	 */
    public uint height = new uint();

    /**
	 * Glyph's left bearing expressed in integer pixels.
	 */
    public int offset_x;

    /**
	 * Glyphs's top bearing expressed in integer pixels.
	 *
	 * Remember that this is the distance from the baseline to the top-most
	 * glyph scanline, upwards y coordinates being positive.
	 */
    public int offset_y;

    /**
	 * For horizontal text layouts, this is the horizontal distance (in
	 * fractional pixels) used to increment the pen position when the glyph is
	 * drawn as part of a string of text.
	 */
    public float advance_x;

    /**
	 * For vertical text layouts, this is the vertical distance (in fractional
	 * pixels) used to increment the pen position when the glyph is drawn as
	 * part of a string of text.
	 */
    public float advance_y;

    /**
	 * First normalized texture coordinate (x) of top-left corner
	 */
    public float s0;

    /**
	 * Second normalized texture coordinate (y) of top-left corner
	 */
    public float t0;

    /**
	 * First normalized texture coordinate (x) of bottom-right corner
	 */
    public float s1;

    /**
	 * Second normalized texture coordinate (y) of bottom-right corner
	 */
    public float t1;

    /**
	 * A vector of kerning pairs relative to this glyph.
	 */
    public List<kerning_t> kerning;

    /**
	 * Glyph outline type (0 = None, 1 = line, 2 = inner, 3 = outer)
	 */
    public int outline_type;

    /**
	 * Glyph outline thickness
	 */
    public float outline_thickness;

}

public enum DataLocation
{
    TEXTURE_FONT_FILE = 0,
    TEXTURE_FONT_MEMORY,
}

/**
 *  Texture font structure.
 */
public class texture_font_t
{
    /**
	 * Vector of glyphs contained in this font.
	 */
    public List<texture_glyph_t> glyphs;

    /**
	 * Atlas structure to store glyphs data.
	 */
    public texture_atlas_t atlas;

    /**
	 * font location
	 */
    public DataLocation location;

    /**
     * * Font filename, for when location == TEXTURE_FONT_FILE
     */
    public string filename;

    /**
     * * Font memory address, for when location == TEXTURE_FONT_MEMORY
     * */
    public byte[] memory;

    /**
	 * Font size
	 */
    public float size;

    /**
	 * Whether to use autohint when rendering font
	 */
    public bool hinting;

    /**
	 * Outline type (0 = None, 1 = line, 2 = inner, 3 = outer)
	 */
    public int outline_type;

    /**
	 * Outline thickness
	 */
    public float outline_thickness;

    /**
	 * Whether to use our own lcd filter.
	 */
    public bool filtering;

    /**
	 * Whether to use kerning if available
	 */
    public bool kerning;

    /**
	 * LCD filter weights
	 */
    public byte[] lcd_weights = new byte[5];

    /**
	 * This field is simply used to compute a default line spacing (i.e., the
	 * baseline-to-baseline distance) when writing text with this font. Note
	 * that it usually is larger than the sum of the ascender and descender
	 * taken as absolute values. There is also no guarantee that no glyphs
	 * extend above or below subsequent baselines when using this distance.
	 */
    public float height;

    /**
	 * This field is the distance that must be placed between two lines of
	 * text. The baseline-to-baseline distance should be computed as:
	 * ascender - descender + linegap
	 */
    public float linegap;

    /**
	 * The ascender is the vertical distance from the horizontal baseline to
	 * the highest 'character' coordinate in a font face. Unfortunately, font
	 * formats define the ascender differently. For some, it represents the
	 * ascent of all capital latin characters (without accents), for others it
	 * is the ascent of the highest accented character, and finally, other
	 * formats define it as being equal to bbox.yMax.
	 */
    public float ascender;

    /**
	 * The descender is the vertical distance from the horizontal baseline to
	 * the lowest 'character' coordinate in a font face. Unfortunately, font
	 * formats define the descender differently. For some, it represents the
	 * descent of all capital latin characters (without accents), for others it
	 * is the ascent of the lowest accented character, and finally, other
	 * formats define it as being equal to bbox.yMin. This field is negative
	 * for values below the baseline.
	 */
    public float descender;

    /**
	 * The position of the underline line for this face. It is the center of
	 * the underlining stem. Only relevant for scalable formats.
	 */
    public float underline_position;

    /**
	 * The thickness of the underline for this face. Only relevant for scalable
	 * formats.
	 */
    public float underline_thickness;
}

// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;
using SharpFont;

#endregion

namespace ftgl
{
    public static class FreeTypeGL
    {
        // ------------------------------------------------------ texture_atlas_new ---
        public static texture_atlas_t texture_atlas_new(uint width, uint height, uint depth)
        {
            texture_atlas_t self = new texture_atlas_t();

            // We want a one pixel border around the whole atlas to avoid any artefact when
            // sampling texture
            Vector3 node = new Vector3(1, 1, width - 2);

            self.nodes = new List<Vector3>();
            self.used = 0;
            self.width = width;
            self.height = height;
            self.depth = depth;
            self.id = -1;
            self.dirty = 0;

            self.nodes.Add(node);
            self.data = new byte[width * height * depth];

            return self;
        }


        // --------------------------------------------------- texture_atlas_delete ---
        public static void texture_atlas_delete(texture_atlas_t self)
        {
            self.nodes.Clear();
            self.data = null;
            if (self.id > 0) GL.DeleteTexture(self.id);
        }


        // ----------------------------------------------- texture_atlas_set_region ---
        public static void texture_atlas_set_region(texture_atlas_t self, uint x, uint y, uint width, uint height, byte[] data, uint stride)
        {
            uint i = new uint();
            uint j = new uint();
            uint depth = new uint();
            uint charsize = new uint();
            byte[] row;
            byte[] src;

            depth = self.depth;
            charsize = sizeof(char);
            for (i = 0; i < height; ++i)
            {
                if (depth == 2)
                {
                    // todo:
                    //row = self.data + ((y + i) * self.width + x) * charsize * depth;
                    //src = data + i * stride * charsize;
                    //for (j = 0; j < width; j++)
                    //{
                    //    row[j * 2 + 0] = 0xff;
                    //    row[j * 2 + 1] = src[j];
                    //}
                }
                else
                {
                    Array.Copy(data, self.data, width * charsize * depth);
                }
            }
        }


        // ------------------------------------------------------ texture_atlas_fit ---
        public static int texture_atlas_fit(texture_atlas_t self, int index, uint width, uint height)
        {
            Vector3 node;
            int x;
            int y;
            int width_left;
            int i = new int();


            node = self.nodes[index];
            x = (int) node.X;
            y = (int) node.Y;
            width_left = (int) width;
            i = index;

            if (x + width > self.width - 1) return -1;
            y = (int) node.Y;
            while (width_left > 0)
            {
                node = self.nodes[i];
                if (node.Y > y) y = (int) node.Y;
                if (y + height > self.height - 1) return -1;
                width_left -= (int) node.Z;
                ++i;
            }

            return y;
        }


        // ---------------------------------------------------- texture_atlas_merge ---
        public static void texture_atlas_merge(texture_atlas_t self)
        {
            Vector3 node;
            Vector3 next;
            for (int i = 0; i < self.nodes.Count - 1; ++i)
            {
                node = self.nodes[i];
                next = self.nodes[i + 1];
                if (node.Y == next.Y)
                {
                    node.Z += next.Z;
                    self.nodes.RemoveAt(i + 1);
                    --i;
                }
            }
        }


        // ----------------------------------------------- texture_atlas_get_region ---
        public static Vector4 texture_atlas_get_region(texture_atlas_t self, uint width, uint height)
        {
            int y;
            int best_height;
            int best_width;
            int best_index;
            Vector3 node;
            Vector3 prev;
            Vector4 region = new Vector4(0, 0, width, height);

            best_height = int.MaxValue;
            best_index = -1;
            best_width = int.MaxValue;
            for (int i = 0; i < self.nodes.Count; ++i)
            {
                y = texture_atlas_fit(self, i, width, height);
                if (y >= 0)
                {
                    node = self.nodes[i];
                    if (y + height < best_height || y + height == best_height && node.Z < best_width)
                    {
                        best_height = (int) (y + height);
                        best_index = i;
                        best_width = (int) node.Z;
                        region.X = node.X;
                        region.Y = y;
                    }
                }
            }

            if (best_index == -1)
            {
                region.X = -1;
                region.Y = -1;
                region.Z = 0;
                region.W = 0;
                return region;
            }

            node = new Vector3();
            node.X = region.X;
            node.Y = region.Y + height;
            node.Z = width;
            self.nodes.Insert(best_index, node);

            for (int i = best_index + 1; i < self.nodes.Count; ++i)
            {
                node = self.nodes[i];
                prev = self.nodes[i - 1];

                if (node.X < prev.X + prev.Z)
                {
                    int shrink = (int) (prev.X + prev.Z - node.X);
                    node.X += shrink;
                    node.Z -= shrink;
                    if (node.Z <= 0)
                    {
                        self.nodes.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            texture_atlas_merge(self);
            self.used += width * height;
            return region;
        }


        // ---------------------------------------------------- texture_atlas_clear ---
        public static void texture_atlas_clear(texture_atlas_t self)
        {
            Vector3 node = new Vector3(1, 1, 1);

            self.nodes.Clear();
            self.used = 0;
            // We want a one pixel border around the whole atlas to avoid any artefact when
            // sampling texture
            node.Z = self.width - 2;

            self.nodes.Add(node);
            self.data = new byte[self.width * self.height * self.depth];
        }


        // --------------------------------------------------- texture_atlas_upload ---
        public static void texture_atlas_upload(texture_atlas_t self)
        {
            self.dirty = 1;

            if (self.id == -1)
            {
                self.id = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, self.id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float) All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float) All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Linear);

            if (self.depth == 4)
            {
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba, (int) self.width, (int) self.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, self.data);
            }
            else if (self.depth == 3)
            {
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgb, (int) self.width, (int) self.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, self.data);
            }
            else if (self.depth == 2)
            {
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.LuminanceAlpha, (int) self.width, (int) self.height, 0, PixelFormat.LuminanceAlpha, PixelType.UnsignedByte, self.data);
            }
            else
            {
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.R8, (int) self.width, (int) self.height, 0, PixelFormat.Red, PixelType.UnsignedByte, self.data);
            }
        }

        private static int texture_font_load_face(texture_font_t self, float size, Library library, Face face)
        {
            // ------------------------------------------------- texture_font_load_face ---
            FTMatrix matrix = new FTMatrix((int) (1.0 / Constants.HRES * 0x10000), (int) (0.0 * 0x10000), (int) (0.0 * 0x10000), (int) (1.0 * 0x10000));

            /* Load face */
            face = new Face(library, self.memory, 0);

            /* Select charmap */
            face.SelectCharmap(Encoding.Unicode);

            /* Set char size */
            face.SetCharSize((int) (size * Constants.HRES), 0, Constants.DPI * Constants.HRES, Constants.DPI);

            /* Set transform matrix */
            face.SetTransform(matrix);

            return 1;
        }

        internal static int texture_font_get_face_with_size(texture_font_t self, float size, Library library, Face face)
        {
            return texture_font_load_face(self, size, library, face);
        }

        internal static int texture_font_get_face(texture_font_t self, Library library, Face face)
        {
            return texture_font_get_face_with_size(self, self.size, library, face);
        }

        internal static int texture_font_get_hires_face(texture_font_t self, Library library, Face face)
        {
            return texture_font_get_face_with_size(self, self.size * 100.0f, library, face);
        }

        // ------------------------------------------------------ texture_glyph_new ---
        public static texture_glyph_t texture_glyph_new()
        {
            texture_glyph_t self = new texture_glyph_t();

            self.id = 0;
            self.width = 0;
            self.height = 0;
            self.outline_type = 0;
            self.outline_thickness = 0.0f;
            self.offset_x = 0;
            self.offset_y = 0;
            self.advance_x = 0.0f;
            self.advance_y = 0.0f;
            self.s0 = 0.0f;
            self.t0 = 0.0f;
            self.s1 = 0.0f;
            self.t1 = 0.0f;
            self.kerning = new List<kerning_t>();
            return self;
        }


        // --------------------------------------------------- texture_glyph_delete ---
        public static void texture_glyph_delete(texture_glyph_t self)
        {
            self.kerning.Clear();
        }

        // ---------------------------------------------- texture_glyph_get_kerning ---
        public static float texture_glyph_get_kerning(texture_glyph_t self, char charcode)
        {
            for (int i = 0; i < self.kerning.Count; ++i)
            {
                kerning_t kerning = self.kerning[i];
                if (kerning.charcode == charcode) return kerning.kerning;
            }

            return 0F;
        }


        // ------------------------------------------ texture_font_generate_kerning ---
        public static void texture_font_generate_kerning(texture_font_t self)
        {
            Library library = new Library();
            Face face = new Face(library, self.memory, 0);
            uint glyph_index = new uint();
            uint prev_index = new uint();
            texture_glyph_t glyph;
            texture_glyph_t prev_glyph;
            FTVector26Dot6 kerning = new FTVector26Dot6();


            /* Load font */
            if (texture_font_get_face(self, library, face) == 0) return;

            /* For each glyph couple combination, check if kerning is necessary */
            /* Starts at index 1 since 0 is for the special backgroudn glyph */
            for (int i = 1; i < self.glyphs.Count; ++i)
            {
                glyph = self.glyphs[i];
                glyph_index = face.GetCharIndex((char) glyph.charcode);
                glyph.kerning.Clear();

                for (int j = 1; j < self.glyphs.Count; ++j)
                {
                    prev_glyph = self.glyphs[j];
                    prev_index = face.GetCharIndex((char) prev_glyph.charcode);
                    kerning = face.GetKerning(prev_index, glyph_index, KerningMode.Unfitted);
                    // printf("%c(%d)-%c(%d): %ld\n",
                    //       prev_glyph->charcode, prev_glyph->charcode,
                    //       glyph_index, glyph_index, kerning.X);
                    if (kerning.X != 0)
                    {
                        kerning_t k = new kerning_t();
                        k.charcode = (char) prev_glyph.charcode;
                        k.kerning = (uint) (kerning.X.ToInt32() / (Constants.HRESf * Constants.HRESf));
                        glyph.kerning.Add(k);
                    }
                }
            }

            face.Dispose();
            library.Dispose();
        }

        // ------------------------------------------------------ texture_font_init ---
        internal static int texture_font_init(texture_font_t self)
        {
            Library library = new Library();
            Face face = new Face(library, self.memory, 0);

            self.glyphs = new List<texture_glyph_t>();
            self.height = 0;
            self.ascender = 0;
            self.descender = 0;
            self.outline_type = 0;
            self.outline_thickness = 0.0f;
            self.hinting = true;
            self.kerning = true;
            self.filtering = true;

            // FT_LCD_FILTER_LIGHT   is (0x00, 0x55, 0x56, 0x55, 0x00)
            // FT_LCD_FILTER_DEFAULT is (0x10, 0x40, 0x70, 0x40, 0x10)
            self.lcd_weights[0] = 0x10;
            self.lcd_weights[1] = 0x40;
            self.lcd_weights[2] = 0x70;
            self.lcd_weights[3] = 0x40;
            self.lcd_weights[4] = 0x10;

            /* Get font metrics at high resolution */
            if (texture_font_get_hires_face(self, library, face) == 0) return -1;

            self.underline_position = face.UnderlinePosition / (Constants.HRESf * Constants.HRESf) * self.size;
            self.underline_position = (float) Math.Round(self.underline_position);
            if (self.underline_position > -2) self.underline_position = -2.0f;

            self.underline_thickness = face.UnderlineThickness / (Constants.HRESf * Constants.HRESf) * self.size;
            self.underline_thickness = (float) Math.Round(self.underline_thickness);
            if (self.underline_thickness < 1) self.underline_thickness = 1.0f;

            SizeMetrics metrics = face.Size.Metrics;
            self.ascender = (metrics.Ascender.ToInt32() >> 6) / 100.0f;
            self.descender = (metrics.Descender.ToInt32() >> 6) / 100.0f;
            self.height = (metrics.Height.ToInt32() >> 6) / 100.0f;
            self.linegap = self.height - self.ascender + self.descender;
            face.Dispose();
            library.Dispose();

            /* -1 is a special glyph */
            texture_font_get_glyph(self, -1);

            return 0;
        }

        // ------------------------------------------- texture_font_new_from_memory ---
        public static texture_font_t texture_font_new_from_memory(texture_atlas_t atlas, float pt_size, byte[] memory_base)
        {
            texture_font_t self = new texture_font_t();

            self.atlas = atlas;
            self.size = pt_size;

            self.location = DataLocation.TEXTURE_FONT_MEMORY;
            self.memory = memory_base;

            if (texture_font_init(self) != 0)
            {
                texture_font_delete(self);
                return null;
            }

            return self;
        }

        // ---------------------------------------------------- texture_font_delete ---
        public static void texture_font_delete(texture_font_t self)
        {
            texture_glyph_t glyph;

            for (int i = 0; i < self.glyphs.Count; ++i)
            {
                glyph = self.glyphs[i];
                texture_glyph_delete(glyph);
            }

            self.glyphs.Clear();
        }


        // ----------------------------------------------- texture_font_load_glyphs ---
        public static int texture_font_load_glyphs(texture_font_t self, int[] charcodes)
        {
            uint x = new uint();
            uint y = new uint();
            uint width = new uint();
            uint height = new uint();
            uint depth = new uint();
            uint w = new uint();
            uint h = new uint();
            Library library = new Library();
            Face face = new Face(library, self.memory, 0);
            Glyph ft_glyph = null;
            GlyphSlot slot = null;
            FTBitmap ft_bitmap;

            uint glyph_index;
            texture_glyph_t glyph;
            LoadFlags flags = 0;
            int ft_glyph_top = 0;
            int ft_glyph_left = 0;

            Vector4 region = new Vector4();
            uint missed = 0;
            bool pass;

            width = self.atlas.width;
            height = self.atlas.height;
            depth = self.atlas.depth;

            if (depth == 2) depth = 1;

            if (texture_font_get_face(self, library, face) == 0) return charcodes.Length;

            /* Load each glyph */
            for (int i = 0; i < charcodes.Length; ++i)
            {
                pass = false;
                /* Check if charcode has been already loaded */
                for (int j = 0; j < self.glyphs.Count; ++j)
                {
                    glyph = self.glyphs[j];
                    // If charcode is -1, we don't care about outline type or thickness
                    // if( (glyph->charcode == charcodes[i])) {
                    if (glyph.charcode == charcodes[i] && (charcodes[i] == -1 || glyph.outline_type == self.outline_type && glyph.outline_thickness == self.outline_thickness))
                    {
                        pass = true;
                        break;
                    }
                }

                if (pass) continue; // don't add the item

                flags = 0;
                ft_glyph_top = 0;
                ft_glyph_left = 0;
                glyph_index = face.GetCharIndex((uint) charcodes[i]);
                // WARNING: We use texture-atlas depth to guess if user wants
                //          LCD subpixel rendering

                if (self.outline_type > 0)
                    flags |= LoadFlags.NoBitmap;
                else
                    flags |= LoadFlags.Render;

                if (!self.hinting)
                    flags |= LoadFlags.NoHinting | LoadFlags.NoAutohint;
                else
                    flags |= LoadFlags.ForceAutohint;

                LoadTarget loadTarget = LoadTarget.Normal;
                if (depth == 3)
                {
                    library.SetLcdFilter(LcdFilter.Light);
                    loadTarget = LoadTarget.Lcd;

                    if (self.filtering) library.SetLcdFilterWeights(self.lcd_weights);
                }

                face.LoadGlyph(glyph_index, flags, loadTarget);


                if (self.outline_type == 0)
                {
                    slot = face.Glyph;
                    ft_bitmap = slot.Bitmap;
                    ft_glyph_top = slot.BitmapTop;
                    ft_glyph_left = slot.BitmapLeft;
                }
                else
                {
                    Stroker stroker = new Stroker(library);
                    BitmapGlyph ft_bitmap_glyph;

                    stroker.Set((int) (self.outline_thickness * Constants.HRES), StrokerLineCap.Round, StrokerLineJoin.Round, 0);
                    ft_glyph = face.Glyph.GetGlyph();

                    if (self.outline_type == 1)
                        ft_glyph.Stroke(stroker, true);
                    else if (self.outline_type == 2)
                        ft_glyph.StrokeBorder(stroker, false, true);
                    else if (self.outline_type == 3)
                        ft_glyph.StrokeBorder(stroker, true, true);

                    if (depth == 1)
                        ft_glyph.ToBitmap(RenderMode.Normal, new FTVector26Dot6(0, 0), true);
                    else
                        ft_glyph.ToBitmap(RenderMode.Lcd, new FTVector26Dot6(0, 0), true);

                    ft_bitmap_glyph = (BitmapGlyph) ft_glyph;
                    ft_bitmap = ft_bitmap_glyph.Bitmap;
                    ft_glyph_top = ft_bitmap_glyph.Top;
                    ft_glyph_left = ft_bitmap_glyph.Left;
                    stroker.Dispose();
                }


                // We want each glyph to be separated by at least one black pixel
                // (for example for shader used in demo-subpixel.c)
                w = (uint) ft_bitmap.Width / depth + 1;
                h = (uint) ft_bitmap.Rows + 1;
                region = texture_atlas_get_region(self.atlas, w, h);
                if (region.X < 0)
                {
                    missed++;
                    continue;
                }

                w = w - 1;
                h = h - 1;
                x = (uint) region.X;
                y = (uint) region.Y;
                texture_atlas_set_region(self.atlas, x, y, w, h, ft_bitmap.BufferData, (uint) ft_bitmap.Pitch);

                glyph = texture_glyph_new();
                glyph.charcode = charcodes[i];
                glyph.width = w;
                glyph.height = h;
                glyph.outline_type = self.outline_type;
                glyph.outline_thickness = self.outline_thickness;
                glyph.offset_x = ft_glyph_left;
                glyph.offset_y = ft_glyph_top;
                glyph.s0 = x / (float) width;
                glyph.t0 = y / (float) height;
                glyph.s1 = (x + glyph.width) / (float) width;
                glyph.t1 = (y + glyph.height) / (float) height;

                // Discard hinting to get advance
                face.LoadGlyph(glyph_index, LoadFlags.Render | LoadFlags.NoHinting, LoadTarget.Normal);
                slot = face.Glyph;
                glyph.advance_x = slot.Advance.X.ToInt32() / Constants.HRESf;
                glyph.advance_y = slot.Advance.Y.ToInt32() / Constants.HRESf;

                self.glyphs.Add(glyph);

                if (self.outline_type > 0) ft_glyph.Dispose();
            }

            face.Dispose();
            library.Dispose();
            // SP: texture_atlas_upload( self->atlas );
            texture_atlas_upload(self.atlas);
            texture_font_generate_kerning(self);
            return (int) missed;
        }

        private static byte[] texture_font_get_glyph_data =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0
        };


        // ------------------------------------------------- texture_font_get_glyph ---
        public static texture_glyph_t texture_font_get_glyph(texture_font_t self, int charcode)
        {
            int[] buffer = {0, 0};
            texture_glyph_t glyph;

            /* Check if charcode has been already loaded */
            for (int i = 0; i < self.glyphs.Count; ++i)
            {
                glyph = self.glyphs[i];
                // If charcode is -1, we don't care about outline type or thickness
                if (glyph.charcode == charcode && (charcode == -1 || glyph.outline_type == self.outline_type && glyph.outline_thickness == self.outline_thickness)) return glyph;
            }

            /* charcode -1 is special : it is used for line drawing (overline,
             * underline, strikethrough) and background.
             */
            if (charcode == -1)
            {
                uint width = self.atlas.width;
                uint height = self.atlas.height;
                Vector4 region = texture_atlas_get_region(self.atlas, 5, 5);
                glyph = texture_glyph_new();
                if (region.X < 0) throw new Exception("Texture atlas is full.");

                texture_atlas_set_region(self.atlas, (uint) region.X, (uint) region.Y, 4, 4, texture_font_get_glyph_data, 0);
                glyph.charcode = -1;
                glyph.s0 = (region.X + 2) / width;
                glyph.t0 = (region.Y + 2) / height;
                glyph.s1 = (region.X + 3) / width;
                glyph.t1 = (region.Y + 3) / height;
                self.glyphs.Add(glyph);
                return glyph; //*(texture_glyph_t **) vector_back( self->glyphs );
            }

            /* Glyph has not been already loaded */
            buffer[0] = charcode;
            if (texture_font_load_glyphs(self, buffer) == 0) return self.glyphs[self.glyphs.Count - 1];
            return null;
        }
    }
}